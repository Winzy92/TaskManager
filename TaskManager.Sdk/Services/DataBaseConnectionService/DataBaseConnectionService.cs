using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using CommonServiceLocator;
using DevExpress.Mvvm;
using DevExpress.Mvvm.DataAnnotations;
using Npgsql;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Services.DataBaseConnectionService
{
    public class DataBaseConnectionService : BindBase, IDatabaseConnectionService
    {
        private UsersInfo _currentUser;

        public UsersInfo CurrentUser
        {
            get => _currentUser;
            set
            {
                base.SetProperty(ref _currentUser, value);
            }
        }

        private NpgsqlConnection _connection;

        private Boolean _isConnected;
        
        private readonly ISettingsService _settingsService;

        public ObservableCollection<GanttItemInfo> NewGanttProjects { get; set; }

        public DataBaseConnectionService()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            NewGanttProjects = new ObservableCollection<GanttItemInfo>();
        }
        
        private String BuildConnectionString(DbConnectionInfo connectionInfo)
        {
            var npgsqlConnectionStringBuilder = new NpgsqlConnectionStringBuilder
            {
                Host = connectionInfo.Host,
                Username = connectionInfo.Username,
                Password = connectionInfo.Password,
                Port = connectionInfo.Port
            };

            if (!String.IsNullOrWhiteSpace(connectionInfo.DbName))
            {
                npgsqlConnectionStringBuilder.Database = connectionInfo.DbName;
            }

            return npgsqlConnectionStringBuilder.ToString();
        }
        
        private void OpenConnection()
        {
            _connection = new NpgsqlConnection(BuildConnectionString(_settingsService.Settings.DbConnectionInfo));

            try
            {
                _connection.Open();
                _isConnected = true;
                _settingsService.Settings.IsConnected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                _isConnected = false;
                _settingsService.Settings.IsConnected = false;
            }
        }
        
        private void CheckDbConnection()
        {
            if (!_isConnected)
            {
                OpenConnection();
            }
        }
        
        private void CloseConnection()
        {
            _connection.Close();
            _isConnected = false;
        }
        
        public void LoadGanttObjects()
        {
            CheckDbConnection();

            var queryText = @"SELECT * FROM tasktable_items
            LEFT JOIN tasks_units
                ON tasktable_items.id = tasks_units.ganttitemid
                ORDER BY tasktable_items.parentid";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var ganttItemInfo = new GanttItemInfo()
                {
                    Id = data.GetInt32(0),
                    ParentId = data.GetInt32(1),
                    Name = data.GetString(2),
                    Tag = (data[3] == System.DBNull.Value) ? null : Convert.ToString(data.GetString(3)),
                    BaselineFinishDate = (data[4] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(4)),
                    BaselineStartDate = (data[5] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(5)),
                    NumOfContract = (data[6] == System.DBNull.Value) ? null : Convert.ToString(data.GetString(6)),
                    GlobalTask = data.GetBoolean(7),
                    IsActive = data.GetBoolean(8),
                    IsArchive = data.GetBoolean(9)
                };

                if ((Int32)ganttItemInfo.ParentId == 0)
                {
                    ganttItemInfo.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/Folder.png",
                        UriKind.Relative));
                }
                else
                {
                    ganttItemInfo.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/List.png",
                        UriKind.Relative));
                }

                var elem = _settingsService.Settings.GanttItems.FirstOrDefault(t => t.Id.Equals(ganttItemInfo.Id));
                Int32? check = (data[12] == System.DBNull.Value) ? (Int32?)null : data.GetInt32(12);
                    
                if(elem != null && check !=null)
                {
                    elem.UsersInfos.Add(_settingsService.Settings.Users.FirstOrDefault(t=>t.Id == check));
                    elem.ListUsers.Add(_settingsService.Settings.Users.FirstOrDefault(t=>t.Id == check));
                    
                }
                else
                {
                    if (check != null)
                    {
                        ganttItemInfo.UsersInfos.Add(_settingsService.Settings.Users.FirstOrDefault(t=>t.Id == check));
                        ganttItemInfo.ListUsers.Add(_settingsService.Settings.Users.FirstOrDefault(t=>t.Id == check));
                        ganttItemInfo.StartDate = (data[15] == System.DBNull.Value)
                            ? (DateTime?) null : Convert.ToDateTime(data.GetDateTime(15));
                        ganttItemInfo.FinishDate = (data[16] == System.DBNull.Value)
                            ? (DateTime?) null : Convert.ToDateTime(data.GetDateTime(16));
                        ganttItemInfo.Progress = data.GetDouble(17);
                        ganttItemInfo.IsAdditional = data.GetBoolean(18);
                    }
                    
                    if(ganttItemInfo.IsArchive == false)
                        _settingsService.Settings.GanttItems.Add(ganttItemInfo);
                }
            }
            
            CreateActualTaskCollection();

            CloseConnection();
        }
        
        public void LoadUsersAdditionalGanttItems()
        {
            _settingsService.Settings.CurrentUserAdditionalGanttItems.Clear();
            
            var collection =
                _settingsService.Settings.GanttItems.Where(t =>
                    t.UsersInfos.Any(z=>z.Id == _settingsService.Settings.CurrentUser.Id));

            if (collection != null)
            {
                var items = collection.Where(t => t.IsAdditional);

                if (items != null)
                {
                    foreach (var item in items)
                    {
                        var element = _settingsService.Settings.GanttItems.FirstOrDefault(t =>
                            (Int32) t.Id == (Int32) item.ParentId);
                        
                        if (element != null)
                        {
                            element.IsAdditional = true;
                            if (_settingsService.Settings.CurrentUserAdditionalGanttItems.FirstOrDefault(t =>
                                (Int32) t.Id == (Int32) element.Id) == null)
                                _settingsService.Settings.CurrentUserAdditionalGanttItems.Add(element);
                        }
                        
                        if (_settingsService.Settings.CurrentUserAdditionalGanttItems.FirstOrDefault(t =>
                            (Int32) t.Id == (Int32) item.Id) == null)
                            _settingsService.Settings.CurrentUserAdditionalGanttItems.Add(item);
                    }
                }
            }
        }

        public void CreateActualTaskCollection()
        {
            _settingsService.Settings.ActiveTasks.Clear();
            
            var collection = _settingsService.Settings.GanttItems.Where(t => t.IsActive && (Int32)t.ParentId == 0 && t.GlobalTask == false);
            
            foreach (var item in collection)
            {
                var elements = _settingsService.Settings.GanttItems.Where(t => (Int32)t.ParentId == (Int32)item.Id);

                foreach (var elem in elements)
                {
                    elem.IsActive = true;
                    _settingsService.Settings.ActiveTasks.Add(elem);
                }
                
                _settingsService.Settings.ActiveTasks.Add(item);
            }
        }

        public void LoadUsersPositions()
        {
            CheckDbConnection();

            var queryText = "SELECT * FROM positions;";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var positionItemInfo = new PositionsInfo()
                {
                    Id = data.GetInt32(0),
                    Name = data.GetString(1),
                    CanModify = data.GetBoolean(2)
                };
                
                _settingsService.Settings.PositionsInfoItems.Add(positionItemInfo);
            }

            CloseConnection();
        }

        public void AddGanttObject(String name)
        {
            CheckDbConnection();

            var obj = new GanttItemInfo();
            obj.Name = name;
            obj.ParentId = 0;
            
            var queryText =
                $"INSERT INTO tasktable_items(name, parentid) VALUES('{obj.Name}', '{obj.ParentId}') RETURNING id";
            
            NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteScalar();

            if (data is Int32 inData)
            {
                obj.Id = inData;
            }
            
            _settingsService.Settings.GanttItems.Add(obj);

        }
        
        public void CopyGanttObject(ObservableCollection<GanttItemInfo> selectedGanttItems)
        {
            CheckDbConnection();

            if (selectedGanttItems != null)
            {
                foreach (var per in selectedGanttItems)
                {
                    if ((per.ParentId is Int32 pId) && (pId == 0))
                    {
                        var obj = new GanttItemInfo();
                        obj.Name = $"{per.Name}"+"_копия";
                        obj.ParentId = 0;
                        obj.NumOfContract = per.NumOfContract;
            
                        var queryText =
                            $"INSERT INTO tasktable_items(name, parentid, numofcontract) VALUES('{obj.Name}', '{obj.ParentId}', '{obj.NumOfContract}') RETURNING id";
            
                        NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                        var data = command.ExecuteScalar();

                        if (data is Int32 inData)
                        {
                            obj.Id = inData;
                        }
            
                        _settingsService.Settings.GanttItems.Add(obj);
                        NewGanttProjects.Add(obj);
                    }
                }

                if (selectedGanttItems.FirstOrDefault().ParentId is Int32 perId && perId != 0)
                {
                    foreach (var p in selectedGanttItems)
                    {
                        foreach (var member in _settingsService.Settings.GanttItems.ToList())
                        {
                            if ((p.ParentId is Int32 parentId) && (member.Id is Int32 mId) && (parentId == mId))
                            {
                                var obj = new GanttItemInfo();
                                obj.Name = p.Name;
                                obj.ParentId = p.ParentId;
            
                                var queryText =
                                    $"INSERT INTO tasktable_items(name, parentid) VALUES('{obj.Name}', '{obj.ParentId}') RETURNING id";
            
                                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                                var data = command.ExecuteScalar();

                                if (data is Int32 inData)
                                {
                                    obj.Id = inData;
                                }
            
                                _settingsService.Settings.GanttItems.Add(obj);
                            }
                        }
                    }
                }
                else
                {
                    foreach (var p in selectedGanttItems)
                    {
                        var element = NewGanttProjects.FirstOrDefault();
                        
                        foreach (var member in _settingsService.Settings.GanttItems.ToList())
                        {
                            if ((p.Id is Int32 pId) && (member.ParentId is Int32 mPId) && (pId == mPId))
                            {
                                var obj = new GanttItemInfo();
                                obj.Name = member.Name;
                                obj.ParentId = element.Id;
            
                                var queryText =
                                    $"INSERT INTO tasktable_items(name, parentid) VALUES('{obj.Name}', '{obj.ParentId}') RETURNING id";
            
                                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                                var data = command.ExecuteScalar();

                                if (data is Int32 inData)
                                {
                                    obj.Id = inData;
                                }
            
                                _settingsService.Settings.GanttItems.Add(obj);
                            }
                        }
                        NewGanttProjects.Remove(element);
                    }
                }
            }
            NewGanttProjects.Clear();
        }

        public void AddGanttChildObject(GanttItemInfo selectedGanttItem, string name)
        {
            CheckDbConnection();
            
            var obj = new GanttItemInfo();
            obj.Name = name;
            obj.ParentId = selectedGanttItem.Id;

            var queryText =
                $@"INSERT INTO tasktable_items(name, parentid) VALUES('{obj.Name}', '{obj.ParentId}') RETURNING id";
            NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
            command.ExecuteNonQuery();
            
            _settingsService.Settings.GanttItems.Add(obj);
        }
        
        public void UpdateGanttObject(GanttItemInfo selectedGanttItem, String prop)
        {
            CheckDbConnection();
            
            var queryText =  $@"UPDATE tasktable_items SET ";

            switch (prop)
            {
                case "StartDate":
                    UpdateTaskUnits(selectedGanttItem, "StartDate");
                    break;
                
                case "FinishDate":
                    UpdateTaskUnits(selectedGanttItem, "FinishDate");
                    break;
                
                case "Name":
                    queryText += $"name='{selectedGanttItem.Name}'";
                    break;
                
                case "Progress":
                    UpdateTaskUnits(selectedGanttItem, "Progress");
                    break;
                
                case "Tag":
                    queryText += $"tag='{selectedGanttItem.Tag}'";
                    break;
                
                case "BaselineFinishDate":
                    queryText += $"baselinefinishdate='{selectedGanttItem.BaselineFinishDate}'";
                    break;
                
                case "BaselineStartDate":
                    queryText += $"baselinestartdate='{selectedGanttItem.BaselineStartDate}'";
                    break;
                
                case "NumOfContract":
                    queryText += $"numofcontract='{selectedGanttItem.NumOfContract}'";
                    break;
                
                case "GlobalTask":
                    queryText += $"globaltask='{selectedGanttItem.GlobalTask}'";
                    UpdateActualTasksCollection(selectedGanttItem, "GlobalTask");
                    break;
                
                case "IsActive":
                    queryText += $"isactive='{selectedGanttItem.IsActive}'";
                    UpdateActualTasksCollection(selectedGanttItem, "IsActive");
                    break;
                
                case "IsArchive":
                    queryText += $"isarchive='{selectedGanttItem.IsArchive}'";
                    UpdateActualTasksCollection(selectedGanttItem, "IsArchive");
                    break;
                
                 case "ListUsers":
                     UpdateTaskUnits(selectedGanttItem, null);
                     break;
            }

            if (prop != "ListUsers")
            {
                queryText += $" WHERE id='{selectedGanttItem.Id}';";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                command.ExecuteNonQuery();
            }
        }

        public void UpdateActualTasksCollection(GanttItemInfo selectedGanttItem, String prop)
        {
            switch (prop)
            {
                case "GlobalTask":
                    if (selectedGanttItem.GlobalTask)
                    {
                        FindAndRemoveItem(selectedGanttItem);
                    }
                    else
                    {
                        FindAndAddItems(selectedGanttItem);
                    }
                    break;
                
                case "IsActive":
                    if (selectedGanttItem.IsActive && selectedGanttItem.GlobalTask == false)
                    {
                        FindAndAddItems(selectedGanttItem);
                    }
                    else
                    {
                        FindAndRemoveItem(selectedGanttItem);
                    }
                    break;
                
                case "IsArchive":
                    if (selectedGanttItem.IsArchive)
                    {
                        FindAndRemoveItem(selectedGanttItem);
                    }
                    else
                    {
                        FindAndAddItems(selectedGanttItem);
                    }
                    break;
            }
            
            TaskManagerServices.Instance.EventAggregator.GetEvent<UpdateMainGanttEvent>().Publish();
        }

        public void FindAndRemoveItem(GanttItemInfo selectedGanttItem)
        {
            var obj = _settingsService.Settings.ActiveTasks.FirstOrDefault(t => (Int32)t.Id == (Int32)selectedGanttItem.Id);

            if (obj != null)
            {
                var collection = _settingsService.Settings.ActiveTasks.ToList().Where(t => (Int32) t.ParentId == (Int32) obj.Id);

                foreach (var element in collection)
                {
                    _settingsService.Settings.ActiveTasks.Remove(element);
                }
                
                _settingsService.Settings.ActiveTasks.Remove(obj);
            }
        }

        public void FindAndAddItems(GanttItemInfo selectedGanttItem)
        {
            var obj = _settingsService.Settings.GanttItems.FirstOrDefault(t => (Int32)t.Id == (Int32)selectedGanttItem.Id);

            if (obj != null)
            {
                var collection = _settingsService.Settings.GanttItems.Where(t => (Int32) t.ParentId == (Int32) obj.Id);
                
                foreach (var element in collection)
                {
                    _settingsService.Settings.ActiveTasks.Add(element);
                    UpdateResourceLinks(element);
                }
                
                _settingsService.Settings.ActiveTasks.Add(obj);
            }
        }

        public void UpdateTaskUnits(GanttItemInfo selectedGanttItem, String prop)
        {
            CheckDbConnection();
            
            if (prop == null)
            {
                if (selectedGanttItem.ListUsers == null || selectedGanttItem.ListUsers.Count == 0)
                {
                    RemoveAllUnits(selectedGanttItem);
                }
                else
                {
                    RemoveAllUnits(selectedGanttItem);
                         
                    foreach (var element in selectedGanttItem.ListUsers)
                    {
                        if (element is UsersInfo usersInfo)
                        {
                            TaskUnitInfo taskUnitInfo = new TaskUnitInfo();

                            taskUnitInfo.SourceId = usersInfo.GanttSourceItemId;
                            taskUnitInfo.UnitId = usersInfo.Id;
                            taskUnitInfo.UnitName = usersInfo.Name;

                            if (selectedGanttItem.Id is Int32 id)
                            {
                                taskUnitInfo.GanttItemId = id;
                            }
                                 
                            var queryText =
                                $@"INSERT INTO tasks_units(ganttitemid, unitid, unit_name, sourceid) VALUES('{taskUnitInfo.GanttItemId}', '{taskUnitInfo.UnitId}', '{taskUnitInfo.UnitName}', '{taskUnitInfo.SourceId}') RETURNING id";
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                            command.ExecuteNonQuery();

                            _settingsService.Settings.GanttTasksUnits.Add(taskUnitInfo);
                        }
                        else
                        {
                            Console.WriteLine("Не удалось определить типа элемента добавляемого в коллецию");
                        }
                    }
                }
            }
            else
            {
                var queryText =  $@"UPDATE tasks_units SET ";

                switch (prop)
                {
                    case "StartDate":
                        queryText += $"startdate='{selectedGanttItem.StartDate}'";
                        break;

                    case "FinishDate":
                        queryText += $"finishdate='{selectedGanttItem.FinishDate}'";
                        break;
                    
                    case "Progress":
                        queryText += $"progress='{selectedGanttItem.Progress.ToString("n2").Replace(',', '.')}'";
                        break;
                    
                    case "IsAdditional":
                        queryText += $"isadditional='{selectedGanttItem.IsAdditional}'";
                        break;
                }
                
                queryText += $" WHERE ganttitemid='{selectedGanttItem.Id}' AND unitid='{_settingsService.Settings.CurrentUser.Id}';";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                command.ExecuteNonQuery();
            }
        }

        public void RemoveGanttObject(ObservableCollection<GanttItemInfo> selectedGanttItems)
        {
            CheckDbConnection();

            if (selectedGanttItems != null)
            {
                foreach (var num in selectedGanttItems)
                {
                    if ((num.ParentId is Int32 pId) && (pId == 0))
                    {
                        foreach (var cell in _settingsService.Settings.GanttItems.ToList())
                        {
                            if (cell.ParentId == num.Id)
                            {
                                try
                                {
                                    var queryText = $"DELETE FROM tasktable_items WHERE id='{cell.Id}'";
                    
                                    NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                    
                                    command.ExecuteNonQuery();
                    
                                    _settingsService.Settings.GanttItems.Remove(cell);
                                }
                                catch (Exception e)
                                {
                                    // ignore
                                }
                            }
                        }
                        try
                        {
                            var queryText = $"DELETE FROM tasktable_items WHERE id='{num.Id}'";
                    
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                    
                            command.ExecuteNonQuery();
                    
                            _settingsService.Settings.GanttItems.Remove(num);
                        }
                        catch (Exception e)
                        {
                            // ignore
                        }
                        
                    }
                    else
                    {
                        try
                        {
                            var queryText = $"DELETE FROM tasktable_items WHERE id='{num.Id}'";
                    
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                    
                            command.ExecuteNonQuery();
                    
                            _settingsService.Settings.GanttItems.Remove(num);

                            foreach (var elem in  _settingsService.Settings.TaskResources.ToList())
                            {
                                if (num.Id is Int32 numId && elem.TaskId is Int32 elemTaskId)
                                {
                                    TaskResourceInfo item = _settingsService.Settings.TaskResources.FirstOrDefault(t => t.TaskId == numId);

                                    if (item != null)
                                    {
                                        _settingsService.Settings.TaskResources.Remove(item);
                                    }
                                }
                            }
                            
                        }
                        catch (Exception e)
                        {
                            // ignore
                        }
                    }
                }
            }
        }
        
        public void LoadGanttResourceItems()
        {
            CheckDbConnection();

            var queryText = "SELECT * FROM ganttresource_items;";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var ganttResource = new GanttResourceItemInfo()
                {
                    Id = data.GetInt32(0),
                    Name = data.GetString(1)
                };
                
                _settingsService.Settings.GanttResourceItems.Add(ganttResource);
            }

            CloseConnection();
        }
        
        public void LoadTasksResourceItems()
        {
            CheckDbConnection();

            var queryText = "SELECT * FROM taskitems_resource;";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var taskResourceInfo = new TaskResourceInfo()
                {
                    Id = data.GetInt32(0),
                    TaskId = data.GetInt32(1),
                    GanttSourceId = data.GetInt32(2)
                };
                
                _settingsService.Settings.TaskResources.Add(taskResourceInfo);
            }

            CreateTaskResourcesList();

            CloseConnection();
        }

        public void LoadUsers()
        {
            CheckDbConnection();

            var queryText = "SELECT * FROM users;";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var userInfo = new UsersInfo()
                {
                    Id = data.GetInt32(0),
                    Name = data.GetString(1),
                    GanttSourceItemId = data.GetInt32(2),
                    Password = data.GetString(3),
                    PositionId = data.GetInt32(4)
                };
                
                _settingsService.Settings.Users.Add(userInfo);
            }

            CloseConnection();
        }
        

        public void CreateTaskResourcesList()
        {
            foreach (var element in _settingsService.Settings.GanttItems)
            {
                foreach (var item in _settingsService.Settings.TaskResources)
                {
                    if (element.Id is Int32 parentId && parentId == item.TaskId)
                    {
                        element.ResourceIds.Add(item.GanttSourceId);
                        var collection = _settingsService.Settings.Users.Where(t => t.GanttSourceItemId == item.GanttSourceId);
                        if (collection != null)
                        {
                            foreach (var elem in collection)
                            {
                                if (!element.ResourceUsers.Contains(elem))
                                {
                                    element.ResourceUsers.Add(elem);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadTasksUnits()
        {
            CheckDbConnection();

            var queryText = "SELECT * FROM tasks_units;";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var taskUnitInfo = new TaskUnitInfo()
                {
                    Id = data.GetInt32(0),
                    GanttItemId = data.GetInt32(1),
                    UnitId = data.GetInt32(2),
                    UnitName = data.GetString(3),
                    SourceId = data.GetInt32(4),
                    StartDate = (data[5] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(5)),
                    FinishDate = (data[6] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(6)),
                    Progress = data.GetDouble(7),
                    IsAdditional = data.GetBoolean(8)
                };
                
                _settingsService.Settings.GanttTasksUnits.Add(taskUnitInfo);
            }

            CloseConnection();
        }

        public void LoadUsersGanttItems()
        {
            CheckDbConnection();

            if (_settingsService.Settings.CurrentUserGanttItems != null)
            {
                _settingsService.Settings.CurrentUserGanttItems.Clear();
            }

            var queryText = $@"SELECT t.id, t.parentid, t.baselinestartdate, t.baselinefinishdate,
                                    t.name, t.numofcontract, t.tag, t.isactive, t.isarchive, t.globaltask, tf.startdate, tf.finishdate, tf.progress, tf.isadditional, tg.*
                                    FROM tasktable_items t
                                    LEFT JOIN tasks_units tf on t.id = tf.ganttitemid 
                                    LEFT JOIN tasktable_items tg on t.parentid = tg.id    
                                    WHERE tf.unitid = '{_settingsService.Settings.CurrentUser.Id}';";

            var command = new NpgsqlCommand(queryText, _connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var ganttItem = new GanttItemInfo()
                {
                    Id = data.GetInt32(0),
                    ParentId = data.GetInt32(1),
                    BaselineStartDate = (data[2] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(2)),
                    BaselineFinishDate = (data[3] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(3)),
                    Name = data.GetString(4),
                    NumOfContract = (data[5] == System.DBNull.Value) ? null : Convert.ToString(data.GetString(5)),
                    Tag = (data[6] == System.DBNull.Value) ? null : Convert.ToString(data.GetString(6)),
                    IsActive = data.GetBoolean(7),
                    IsArchive = data.GetBoolean(8),
                    GlobalTask = data.GetBoolean(9),
                    StartDate = (data[10] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(10)),
                    FinishDate = (data[11] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(11)),
                    Progress = data.GetDouble(12),
                    IsAdditional = data.GetBoolean(13),
                    Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/List.png",
                        UriKind.Relative))
                };

                var parentGanttItem = new GanttItemInfo()
                {
                    Id = data.GetInt32(14),
                    ParentId = data.GetInt32(15),
                    Name = data.GetString(16),
                    Tag = (data[17] == System.DBNull.Value) ? null : Convert.ToString(data.GetString(17)),
                    BaselineFinishDate = (data[18] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(18)),
                    BaselineStartDate = (data[19] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(19)),
                    NumOfContract = (data[20] == System.DBNull.Value) ? null : Convert.ToString(data.GetString(20)),
                    GlobalTask = data.GetBoolean(21),
                    IsActive = data.GetBoolean(22),
                    IsArchive = data.GetBoolean(23),
                    Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/Folder.png",
                        UriKind.Relative))
                };
                
                if (ganttItem.IsAdditional == false)
                {
                    _settingsService.Settings.CurrentUserGanttItems.Add(ganttItem);
                }
                else
                {
                    parentGanttItem.IsAdditional = true;
                }
                
                if (!_settingsService.Settings.CurrentUserGanttItems.Any(t=>(Int32)t.Id == (Int32)parentGanttItem.Id) && parentGanttItem.IsAdditional == false)
                {
                    _settingsService.Settings.CurrentUserGanttItems.Add(parentGanttItem);
                }
            }

            CloseConnection();
        }

        public void RemoveAllUnits(GanttItemInfo ganttItemInfo)
          {
              CheckDbConnection();
              
              if (ganttItemInfo != null)
              {
                  var queryText =
                      $"DELETE FROM tasks_units WHERE ganttitemid='{ganttItemInfo.Id}'";
                  NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                  command.ExecuteNonQuery();

              }
          }

          public void AddResourceLink(IList ResourceLinks, GanttItemInfo ganttItemInfo)
        {
            CheckDbConnection();

            foreach (var element in ResourceLinks)
            {
                TaskResourceInfo taskResourceInfo = new TaskResourceInfo();

                if (taskResourceInfo.TaskId is Int32 tInt && ganttItemInfo.Id is Int32 gInt)
                {
                    taskResourceInfo.TaskId = gInt;
                }
                
                if (taskResourceInfo.GanttSourceId is Int32 GSId && element is Int32 elemId)
                {
                    taskResourceInfo.GanttSourceId = elemId;
                }

                if (taskResourceInfo.TaskId == null || taskResourceInfo.GanttSourceId == null)
                {
                    Console.WriteLine("Не удалось добавить связь между задачей и ресурсом исполнителей");
                }
                else
                {
                    var queryText =
                        $@"INSERT INTO taskitems_resource(taskid, ganttsourceid) VALUES('{taskResourceInfo.TaskId}', '{taskResourceInfo.GanttSourceId}') RETURNING id";
                    NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                    command.ExecuteNonQuery();

                    _settingsService.Settings.TaskResources.Add(taskResourceInfo);
                }
            }
        }
        
        public void RemoveResourceLink(IList ResourceLinks, GanttItemInfo ganttItemInfo)
        {
            CheckDbConnection();

            if (ResourceLinks != null)
            {
                foreach (var element in ResourceLinks)
                {
                    TaskResourceInfo taskResourceInfo = new TaskResourceInfo();

                    if (taskResourceInfo.TaskId is Int32 TId && ganttItemInfo.Id is Int32 gInt && element is Int32 elemId && taskResourceInfo.GanttSourceId is Int32 gsId)
                    {
                        TaskResourceInfo source = _settingsService.Settings.TaskResources.FirstOrDefault(t =>
                            t.TaskId == gInt && t.GanttSourceId == elemId);

                        if (source != null)
                        {
                            var queryText = $"DELETE FROM taskitems_resource WHERE id='{source.Id}'";
                            
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                    
                            command.ExecuteNonQuery();
                    
                            _settingsService.Settings.TaskResources.Remove(source);
                            /*ganttItemInfo.ResourceIds.Remove(source.Id);*/
                        }
                    }
                }
            }
        }

        public void UpdateResourceLinks(GanttItemInfo ganttItemInfo)
        {
            if (ganttItemInfo.ResourceIds.Count != 0)
            {
                foreach (var element in ganttItemInfo.ResourceIds)
                {
                    var collection = _settingsService.Settings.Users.Where(t => t.GanttSourceItemId == element);

                    foreach (var elem in collection)
                    {
                        if (!ganttItemInfo.ResourceIds.Contains(elem.Id))
                        {
                            ganttItemInfo.ResourceUsers.Add(elem);
                        }
                    }
                }
            }
            else
            {
                ganttItemInfo.ResourceUsers.Clear();
            }
        }

        public void AddUser(UsersInfo usersInfo)
        {
            CheckDbConnection();
            
            if (usersInfo != null)
            {
                var queryText =
                    $@"INSERT INTO users(name, ganttsourceitemid, positionid, password) VALUES('{usersInfo.Name}', '{usersInfo.GanttSourceItemId}', '{usersInfo.PositionId}', '{usersInfo.Password}') RETURNING id";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                var executeScalar = command.ExecuteScalar();

                if (executeScalar is Int32 id)
                {
                    usersInfo.Id = id;
                }

                _settingsService.Settings.Users.Add(usersInfo);
            }
        }
        
        public void AddGanttSourceItem(GanttResourceItemInfo ganttResourceItemInfo)
        {
            CheckDbConnection();
            
            if (ganttResourceItemInfo != null)
            {
                var queryText =
                    $@"INSERT INTO ganttresource_items(name) VALUES('{ganttResourceItemInfo.Name}') RETURNING id";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                var executeScalar = command.ExecuteScalar();

                if (executeScalar is Int32 id)
                {
                    ganttResourceItemInfo.Id = id;
                }

                _settingsService.Settings.GanttResourceItems.Add(ganttResourceItemInfo);
            }
        }
        
        public void RemoveSelectedItemUserLibrary(Object selectedItem)
        {
            CheckDbConnection();
            
            if (selectedItem is GanttResourceItemInfo ganttResourceItemInfo)
            {
                var queryText = $"DELETE FROM ganttresource_items WHERE id='{ganttResourceItemInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                command.ExecuteNonQuery();

                var element =
                    _settingsService.Settings.GanttResourceItems.FirstOrDefault(t =>
                        t.Name == ganttResourceItemInfo.Name);
                if (element != null)
                {
                    _settingsService.Settings.GanttResourceItems.Remove(element);
                }
            }

            if (selectedItem is UsersInfo usersInfo)
            {
                var queryText = $"DELETE FROM users WHERE id='{usersInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                command.ExecuteNonQuery();
                
                var element =
                    _settingsService.Settings.Users.FirstOrDefault(t =>
                        t.Id == usersInfo.Id);
                if (element != null)
                {
                    _settingsService.Settings.Users.Remove(element);
                }
            }
        }
        
        public void UpdatePropertiesSelectedItemUserLibrary(Object selectedItem, String property)
        {
            CheckDbConnection();

            if (selectedItem is UsersInfo usersInfo)
            {
                var elem = _settingsService.Settings.Users.FirstOrDefault(t=>t.Id == usersInfo.Id);
                
                var queryText =  $@"UPDATE users SET ";

                if (elem != null)
                {
                    switch (property)
                    {
                        case "Name":
                            queryText += $"name='{usersInfo.Name}'";
                            break;
                
                        case "PositionId":
                            queryText += $"positionid='{usersInfo.PositionId}'";
                            break;
                
                        case "Password":
                            queryText += $"password='{usersInfo.Password}'";
                            break;
                    }
                
                    queryText += $" WHERE id='{usersInfo.Id}';";
                    NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                    command.ExecuteNonQuery();
                }
            }

            if (selectedItem is GanttResourceItemInfo ganttResourceItemInfo)
            {
                var queryText =  $@"UPDATE ganttresource_items SET name='{ganttResourceItemInfo.Name}' WHERE id='{ganttResourceItemInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connection);
                command.ExecuteNonQuery();
            }
        }
    }
}