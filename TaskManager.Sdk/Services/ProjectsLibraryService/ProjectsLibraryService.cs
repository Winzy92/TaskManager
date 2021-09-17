using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using DevExpress.Mvvm.Native;
using Npgsql;
using TaskManager.Sdk.Core.Containers;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Services.ProjectsLibraryService
{
    public class ProjectsLibraryService : BindBase, IProjectsLibraryService
    {
        private readonly ISettingsService _settingsService;

        private readonly IDatabaseConnectionService _connectionService;

        private readonly IUsersLibraryService _usersLibraryService;

        public ProjectsLibraryInfo ProjectsLibrary { get; set; } = new ProjectsLibraryInfo();

        public void LoadGanttObjects()
        {
            _connectionService.CheckDbConnection();

            var queryText = @"SELECT tasks.id as tasktable_items_id, tasks.parentid, tasks.name as tasktable_items_name, tasks.tag, tasks.baselinestartdate, tasks.baselinefinishdate,
                            projects.id as project_id, projects.name as project_name,
                            tasksunits.id as tasksunits_id, tasksunits.ganttitemid, tasksunits.unitid, tasksunits.sourceid, tasksunits.startdate, 
                            tasksunits.finishdate, tasksunits.progress, tasksunits.isadditional,
                            globaltask, isactive, isarchive, numberofcontract FROM projects
                            LEFT JOIN tasktable_items tasks
                            ON tasks.parentid = projects.id
                            LEFT JOIN tasks_units tasksunits
                            ON tasks.id = tasksunits.ganttitemid
                            where projects.isarchive = false";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                //Родитель
                var rootganttItemInfo = new GanttItemInfo()
                {
                    Id = Convert.ToInt32(data["project_id"]),
                    ParentId = null,
                    Name = Convert.ToString(data["project_name"]),
                    GlobalTask = Convert.ToBoolean(data["globaltask"]),
                    IsActive = Convert.ToBoolean(data["isactive"]),
                    IsArchive = Convert.ToBoolean(data["isarchive"]),
                    NumOfContract = (data["numberofcontract"] == System.DBNull.Value) ? null : Convert.ToString(data["numberofcontract"])
                };
                
                //Дочерний элемент
                var childganttItemInfo = new GanttItemInfo()
                {
                    Id = (data["tasktable_items_id"] == System.DBNull.Value) ? (Int32?)null : Convert.ToInt32(data["tasktable_items_id"]),
                    ParentId = rootganttItemInfo.Id,
                    Name =  Convert.ToString(data["tasktable_items_name"]),
                    Tag = (data["tag"] == System.DBNull.Value) ? null : Convert.ToString(data["tag"]),
                    BaselineFinishDate = (data["baselinefinishdate"] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data["baselinefinishdate"]),
                    BaselineStartDate = (data["baselinestartdate"] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data["baselinestartdate"]),
                    GlobalTask = rootganttItemInfo.GlobalTask,
                    IsActive = rootganttItemInfo.IsActive,
                    IsArchive = rootganttItemInfo.IsArchive
                };

                var elem = ProjectsLibrary.GanttItems.FirstOrDefault(t => t.Id.Equals(childganttItemInfo.Id));
                Int32? check = (data["unitid"] == System.DBNull.Value) ? (Int32?)null : Convert.ToInt32(data["unitid"]);
                    
                if(elem != null && check !=null)
                {
                    elem.Id.UsersInfos.Add(_usersLibraryService.UsersLibrary.Users.FirstOrDefault(t=>t.Id == check));
                    elem.Id.ListUsers.Add(_usersLibraryService.UsersLibrary.Users.FirstOrDefault(t=>t.Id == check));
                }
                else
                {
                    if (check != null)
                    {
                        childganttItemInfo.UsersInfos.Add(_usersLibraryService.UsersLibrary.Users.FirstOrDefault(t=>t.Id == check));
                        childganttItemInfo.ListUsers.Add(_usersLibraryService.UsersLibrary.Users.FirstOrDefault(t=>t.Id == check));
                        childganttItemInfo.StartDate = (data["startdate"] == System.DBNull.Value)
                            ? (DateTime?) null : Convert.ToDateTime(data["startdate"]);
                        childganttItemInfo.FinishDate = (data["finishdate"] == System.DBNull.Value)
                            ? (DateTime?) null : Convert.ToDateTime(data["finishdate"]);
                        childganttItemInfo.Progress = Convert.ToDouble(data["progress"]);
                        childganttItemInfo.IsAdditional = Convert.ToBoolean(data["isadditional"]);
                    }
                };
                
                var treeItem1 = new GanttTreeViewItemInfo(rootganttItemInfo);
                treeItem1.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/Folder.png",
                    UriKind.Relative));
                
                var treeItem2 = new GanttTreeViewItemInfo(childganttItemInfo);
                treeItem2.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/List.png",
                    UriKind.Relative));

                if (rootganttItemInfo.IsActive)
                {
                    var element = ProjectsLibrary.GanttItems.FirstOrDefault(t =>
                        (Int32) t.Id.Id == (Int32) rootganttItemInfo.Id);

                    if (ProjectsLibrary.GanttItems.FirstOrDefault(t =>
                        (Int32) t.Id.Id == (Int32) childganttItemInfo.Id) == null)
                    {
                        if (element == null)
                        {
                            treeItem2.ParentId = rootganttItemInfo;

                            if (treeItem1.Id.BaselineStartDate == null ||
                                treeItem1.Id.BaselineStartDate >= treeItem2.Id.BaselineStartDate)
                            {
                                treeItem1.Id.BaselineStartDate = treeItem2.Id.BaselineStartDate;
                            }

                            if (treeItem1.Id.BaselineFinishDate == null ||
                                treeItem1.Id.BaselineFinishDate <= treeItem2.Id.BaselineFinishDate)
                            {
                                treeItem1.Id.BaselineFinishDate = treeItem2.Id.BaselineFinishDate;
                            }

                            ProjectsLibrary.GanttItems.Add(treeItem1);
                        }
                        else
                        {
                            treeItem2.ParentId = element.Id;

                            if (element.Id.BaselineStartDate == null ||
                                element.Id.BaselineStartDate >= treeItem2.Id.BaselineStartDate)
                            {
                                element.Id.BaselineStartDate = treeItem2.Id.BaselineStartDate;
                            }

                            if (element.Id.BaselineFinishDate == null ||
                                element.Id.BaselineFinishDate <= treeItem2.Id.BaselineFinishDate)
                            {
                                element.Id.BaselineFinishDate = treeItem2.Id.BaselineFinishDate;
                            }
                        }
                        ProjectsLibrary.GanttItems.Add(treeItem2);
                    }
                }
                
                if(ProjectsLibrary.RootItemsProjectsLibrary.FirstOrDefault(t=>
                    (Int32)t.Id.Id == (Int32)treeItem1.Id.Id) == null)
                    ProjectsLibrary.RootItemsProjectsLibrary.Add(treeItem1);

            }
            _connectionService.CloseConnection();
        }

        public void LoadUsersAdditionalGanttItems()
        {
            _connectionService.CheckDbConnection();

            var queryText = $@"SELECT tasks.id as tasktable_items_id, tasks.parentid, tasks.name as tasktable_items_name, tasks.tag, tasks.baselinestartdate, tasks.baselinefinishdate,
                            projects.id as project_id, projects.name as project_name,
                            tasksunits.id as tasksunits_id, tasksunits.ganttitemid, tasksunits.unitid, tasksunits.sourceid, tasksunits.startdate, 
                            tasksunits.finishdate, tasksunits.progress, tasksunits.isadditional,
                            globaltask, isactive, isarchive, numberofcontract FROM projects
                            LEFT JOIN tasktable_items tasks
                            ON tasks.parentid = projects.id
                            LEFT JOIN tasks_units tasksunits
                            ON tasks.id = tasksunits.ganttitemid 
                            where projects.isactive = true 
                            and projects.isarchive = false
                            and tasksunits.unitid = '{_usersLibraryService.UsersLibrary.CurrentUser.Id}';";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                //Родитель
                var rootganttItemInfo = new GanttItemInfo()
                {
                    Id = Convert.ToInt32(data["project_id"]),
                    ParentId = null,
                    Name = Convert.ToString(data["project_name"]),
                    GlobalTask = Convert.ToBoolean(data["globaltask"]),
                    IsActive = Convert.ToBoolean(data["isactive"]),
                    IsArchive = Convert.ToBoolean(data["isarchive"]),
                    NumOfContract = (data["numberofcontract"] == System.DBNull.Value) ? null : Convert.ToString(data["numberofcontract"])
                };
                
                //Дочерний элемент
                var childganttItemInfo = new GanttItemInfo()
                {
                    Id = Convert.ToInt32(data["tasktable_items_id"]),
                    ParentId = rootganttItemInfo,
                    Name = Convert.ToString(data["tasktable_items_name"]),
                    Tag = (data["tag"] == System.DBNull.Value) ? null : Convert.ToString(data["tag"]),
                    BaselineFinishDate = (data["baselinefinishdate"] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data["baselinefinishdate"]),
                    BaselineStartDate = (data["baselinestartdate"] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data["baselinestartdate"]),
                    StartDate = (data["startdate"] == System.DBNull.Value) ? (DateTime?) null : Convert.ToDateTime(data["startdate"]),
                    FinishDate = (data["finishdate"] == System.DBNull.Value) ? (DateTime?) null : Convert.ToDateTime(data["finishdate"]),
                    Progress = Convert.ToDouble(data["progress"]),
                    GlobalTask = rootganttItemInfo.GlobalTask,
                    IsActive = rootganttItemInfo.IsActive,
                    IsArchive = rootganttItemInfo.IsArchive,
                    IsAdditional = Convert.ToBoolean(data["isadditional"])
                };

                var treeItem1 = new GanttTreeViewItemInfo(rootganttItemInfo);
                treeItem1.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/Folder.png",
                    UriKind.Relative));
                
                var treeItem2 = new GanttTreeViewItemInfo(childganttItemInfo);
                treeItem2.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/List.png",
                    UriKind.Relative));

                if (childganttItemInfo.IsAdditional)
                {
                    var root = ProjectsLibrary.CurrentUserAdditionalGanttItems.FirstOrDefault(t =>
                        (Int32) t.Id.Id == (Int32) rootganttItemInfo.Id);
                    
                    if (root == null)
                    {
                        ProjectsLibrary.CurrentUserAdditionalGanttItems.Add(treeItem1);
                    }

                    if (ProjectsLibrary.CurrentUserAdditionalGanttItems.FirstOrDefault(t =>
                        (Int32) t.Id.Id == (Int32) childganttItemInfo.Id) == null)
                    {
                        if (root == null)
                        {
                            treeItem2.ParentId = rootganttItemInfo;
                        }
                        else
                        {
                            treeItem2.ParentId = root.Id;
                        }
                    }
                    
                    ProjectsLibrary.CurrentUserAdditionalGanttItems.Add(treeItem2);
                }
                else
                {
                    var root = ProjectsLibrary.CurrentUserGanttItems.FirstOrDefault(t =>
                        (Int32) t.Id.Id == (Int32) rootganttItemInfo.Id);
                    
                    if (root == null)
                    {
                        ProjectsLibrary.CurrentUserGanttItems.Add(treeItem1);
                    }

                    if (ProjectsLibrary.CurrentUserGanttItems.FirstOrDefault(t =>
                        (Int32) t.Id.Id == (Int32) childganttItemInfo.Id) == null)
                    {
                        if (root == null)
                        {
                            treeItem2.ParentId = rootganttItemInfo;
                        }
                        else
                        {
                            treeItem2.ParentId = root.Id;
                        }
                    }
                    
                    ProjectsLibrary.CurrentUserGanttItems.Add(treeItem2);
                }
            }
            _connectionService.CloseConnection();
        }

        public void LoadTasksResourceItems()
        {
            _connectionService.CheckDbConnection();

            var queryText = "SELECT * FROM taskitems_resource;";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var taskResourceInfo = new TaskResourceInfo()
                {
                    Id = data.GetInt32(0),
                    TaskId = data.GetInt32(1),
                    GanttSourceId = data.GetInt32(2),
                    Percent = data.GetDouble(3)
                };
                
                ProjectsLibrary.TaskResources.Add(taskResourceInfo);
            }

            CreateTaskResourcesList();

            _connectionService.CloseConnection();
        }
        
        public void CreateTaskResourcesList()
        {
            foreach (var element in ProjectsLibrary.GanttItems)
            {
                foreach (var item in ProjectsLibrary.TaskResources)
                {
                    if (element.Id.Id is Int32 parentId && parentId == item.TaskId)
                    {
                        element.ResourceIds.Add(item);
                        var collection = _usersLibraryService.UsersLibrary.Users.Where(t => t.GanttSourceItemId == item.GanttSourceId);
                        if (collection != null)
                        {
                            foreach (var elem in collection)
                            {
                                if (!element.Id.ResourceUsers.Contains(elem))
                                {
                                    element.Id.ResourceUsers.Add(elem);
                                }
                            }
                        }
                    }
                }
            }
        }

        public void LoadTasksUnits()
        {
            _connectionService.CheckDbConnection();

            var queryText = "SELECT * FROM tasks_units;";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var taskUnitInfo = new TaskUnitInfo()
                {
                    Id = data.GetInt32(0),
                    GanttItemId = data.GetInt32(1),
                    UnitId = data.GetInt32(2),
                    SourceId = data.GetInt32(3),
                    StartDate = (data[4] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(4)),
                    FinishDate = (data[5] == System.DBNull.Value) ? (DateTime?)null : Convert.ToDateTime(data.GetDateTime(5)),
                    Progress = data.GetDouble(6),
                    IsAdditional = data.GetBoolean(7)
                };
                
                ProjectsLibrary.GanttTasksUnits.Add(taskUnitInfo);
            }

            _connectionService.CloseConnection();
        }

        public void UpdateResourceLinks(GanttTreeViewItemInfo ganttItemInfo)
        {
            if (ganttItemInfo.ResourceIds.Count != 0)
            {
                foreach (var element in ganttItemInfo.ResourceIds)
                {
                    var collection = _usersLibraryService.UsersLibrary.Users.Where(t => t.GanttSourceItemId == element.GanttSourceId);

                    foreach (var elem in collection)
                    {
                        if (ganttItemInfo.Id.ResourceUsers.FirstOrDefault(t=>t.Id == elem.Id) == null)
                        {
                            ganttItemInfo.Id.ResourceUsers.Add(elem);
                        }
                    }
                }
            }
            else
            {
                ganttItemInfo.Id.ResourceUsers.Clear();
            }
        }

        public void RemoveAllUnits(GanttItemInfo ganttItemInfo)
        {
            _connectionService.CheckDbConnection();

            if (ganttItemInfo != null)
            {
                var queryText =
                    $"DELETE FROM tasks_units WHERE ganttitemid='{ganttItemInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();
            }
        }

        public void AddGanttObject(string name)
        {
            _connectionService.CheckDbConnection();

            var obj = new GanttItemInfo();
            obj.Name = name;

            var queryText =
                $"INSERT INTO projects(name) VALUES('{obj.Name}') RETURNING id";
            
            NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteScalar(); 

            var rootitem = new GanttTreeViewItemInfo(obj);
            rootitem.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/Folder.png",
                UriKind.Relative));
            
            ProjectsLibrary.RootItemsProjectsLibrary.Add(rootitem);
            
            _connectionService.CloseConnection();
        }

        public void AddGanttChildObject(GanttTreeViewItemInfo selectedGanttItem, string name)
        {
            _connectionService.CheckDbConnection();
            
            var obj = new GanttItemInfo();
            obj.Name = name;
            obj.ParentId = selectedGanttItem.Id;

            var queryText =
                $@"INSERT INTO tasktable_items(name, parentid) VALUES('{obj.Name}', '{obj.ParentId}') RETURNING id";
            NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
            command.ExecuteScalar();
            
            var childitem = new GanttTreeViewItemInfo(obj);
            childitem.Image = new BitmapImage(new Uri(@"/TaskManager.Sdk;component/Multimedia/List.png",
                UriKind.Relative));

            ProjectsLibrary.GanttItems.Add(childitem);
            
            _connectionService.CloseConnection();
        }

        public void UpdateGanttObject(GanttItemInfo ganttItemInfo, string prop)
        {
            _connectionService.CheckDbConnection();

            if (ganttItemInfo.ParentId == null)
            {
                var queryText =  $@"UPDATE projects SET ";

                switch (prop)
                {
                    case "Name":
                        queryText += $"name='{ganttItemInfo.Name}'";
                        break;
                    
                    case "GlobalTask":
                        queryText += $"globaltask='{ganttItemInfo.GlobalTask}'";
                        UpdateChildGanttObject(ganttItemInfo, "GlobalTask");
                        break;
                    
                    case "IsActive":
                        queryText += $"isactive='{ganttItemInfo.IsActive}'";
                        UpdateChildGanttObject(ganttItemInfo, "IsActive");
                        break;
                    
                    case "IsArchive":
                        queryText += $"isarchive='{ganttItemInfo.IsArchive}'";
                        UpdateChildGanttObject(ganttItemInfo, "IsArchive");
                        break;
                    
                    case "NumOfContract":
                        queryText += $"numberofcontract='{ganttItemInfo.NumOfContract}'";
                        break;
                }
                
                queryText += $" WHERE id='{ganttItemInfo.Id}';";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();
            }
            else
            {
                UpdateChildGanttObject(ganttItemInfo, prop);
            }
            
            _connectionService.CloseConnection();
        }

        public void UpdateChildGanttObject(GanttItemInfo ganttItemInfo, string prop)
        {
            _connectionService.CheckDbConnection();
            
            var queryText =  $@"UPDATE tasktable_items SET ";

            switch (prop)
            {
                case "StartDate":
                    UpdateTaskUnits(ganttItemInfo, "StartDate");
                    break;
                
                case "FinishDate":
                    UpdateTaskUnits(ganttItemInfo, "FinishDate");
                    break;
                
                case "Name":
                    queryText += $"name='{ganttItemInfo.Name}'";
                    break;
                
                case "Progress":
                    UpdateTaskUnits(ganttItemInfo, "Progress");
                    break;
                
                case "Tag":
                    queryText += $"tag='{ganttItemInfo.Tag}'";
                    break;
                
                case "BaselineFinishDate":
                    queryText += $"baselinefinishdate='{ganttItemInfo.BaselineFinishDate}'";
                    var element = ProjectsLibrary.GanttItems.FirstOrDefault(t => t.Id.Id == ganttItemInfo.Id);
                    if (element != null)
                    {
                        var rootitem = ProjectsLibrary.GanttItems.FirstOrDefault(t => t.Id.Id == element.ParentId.Id);

                        if (rootitem != null)
                        {
                            if (rootitem.Id.BaselineFinishDate <= ganttItemInfo.BaselineFinishDate)
                                rootitem.Id.BaselineFinishDate = ganttItemInfo.BaselineFinishDate;
                        }
                    }

                    break;
                
                case "BaselineStartDate":
                    queryText += $"baselinestartdate='{ganttItemInfo.BaselineStartDate}'";
                    var item = ProjectsLibrary.GanttItems.FirstOrDefault(t => t.Id.Id == ganttItemInfo.Id);
                    if (item != null)
                    {
                        var rootitem = ProjectsLibrary.GanttItems.FirstOrDefault(t => t.Id.Id == item.ParentId.Id);

                        if (rootitem != null)
                        {
                            if (rootitem.Id.BaselineStartDate >= ganttItemInfo.BaselineStartDate)
                                rootitem.Id.BaselineStartDate = ganttItemInfo.BaselineStartDate;
                        }
                    }
                    break;

                case "GlobalTask":
                    /*var collection = ProjectsLibrary.GanttItems.Where(t =>
                            (Int32) t.ParentId == (Int32) ganttItemInfo.Id);
                    if (collection != null)
                    {
                        foreach (var item in collection)
                        {
                            ProjectsLibrary.GanttItems.Remove(item);
                        }
                    }*/
                    LoadGanttObjects();
                    break;
                
                case "IsActive":
                    LoadGanttObjects();
                    break;
                
                case "IsArchive":
                    LoadGanttObjects();
                    break;
                
                 case "ListUsers":
                     UpdateTaskUnits(ganttItemInfo, null);
                     break;
            }

            if (prop != "ListUsers")
            {
                queryText += $" WHERE id='{ganttItemInfo.Id}';";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();
            }
            
            _connectionService.CloseConnection();
        }
        
        public void RemoveGanttObject(ObservableCollection<GanttTreeViewItemInfo> selectedGanttItems)
        {
            _connectionService.CheckDbConnection();

            if (selectedGanttItems != null)
            {
                foreach (var num in selectedGanttItems)
                {
                    if (num.ParentId == null)
                    {
                        try
                        {
                            var queryText = $"DELETE FROM projects WHERE id='{num.Id.Id}'";
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                            command.ExecuteNonQuery();
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
                            var queryText = $"DELETE FROM tasktable_items WHERE id='{num.Id.Id}'";
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                            command.ExecuteNonQuery();
                        }
                        catch (Exception e)
                        {
                            // ignore
                        }
                    }
                }
                
                LoadTasksUnits();
                LoadTasksResourceItems();
            }
            
            _connectionService.CloseConnection();
        }

        public void CopyGanttObject(ObservableCollection<GanttItemInfo> selectedGanttItems)
        {
            throw new System.NotImplementedException();
        }

        public void AddResourceLink(IList ResourceLinks, GanttTreeViewItemInfo ganttItemInfo)
        {
            _connectionService.CheckDbConnection();
            
            foreach (var element in ResourceLinks)
            {
                if (element is TaskResourceInfo taskResourceInfo)
                {
                    if (taskResourceInfo.TaskId == null || taskResourceInfo.GanttSourceId == null)
                    {
                        Console.WriteLine("Не удалось добавить связь между задачей и ресурсом исполнителей");
                    }
                    else
                    {
                        var queryText =
                            $@"INSERT INTO taskitems_resource(taskid, ganttsourceid, percent) VALUES('{taskResourceInfo.TaskId}', '{taskResourceInfo.GanttSourceId}', '{taskResourceInfo.Percent.ToString().Replace(',','.')}') RETURNING id";
                        NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                        command.ExecuteScalar();

                        ProjectsLibrary.TaskResources.Add(taskResourceInfo);
                    }
                }
            }
            _connectionService.CloseConnection();
        }

        public void RemoveResourceLink(IList ResourceLinks, GanttTreeViewItemInfo ganttItemInfo)
        {
            _connectionService.CheckDbConnection();

            if (ResourceLinks != null)
            {
                foreach (var element in ResourceLinks)
                {
                    if (element is TaskResourceInfo taskResourceInfo)
                    {
                        var queryText = $"DELETE FROM taskitems_resource WHERE id='{taskResourceInfo.Id}'";
                            
                        NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                        command.ExecuteNonQuery();
                    
                        ProjectsLibrary.TaskResources.Remove(taskResourceInfo);
                        ganttItemInfo.ResourceIds.Remove(taskResourceInfo);
                    }
                }
            }
            
            _connectionService.CloseConnection();
        }

        public void UpdateTaskUnits(GanttItemInfo selectedGanttItem, string prop)
        {
            _connectionService.CheckDbConnection();
            
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

                            if (selectedGanttItem.Id is Int32 id)
                            {
                                taskUnitInfo.GanttItemId = id;
                            }
                                 
                            var queryText =
                                $@"INSERT INTO tasks_units(ganttitemid, unitid, sourceid) VALUES('{taskUnitInfo.GanttItemId}', '{taskUnitInfo.UnitId}', '{taskUnitInfo.SourceId}') RETURNING id";
                            NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                            command.ExecuteNonQuery();

                            ProjectsLibrary.GanttTasksUnits.Add(taskUnitInfo);
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
                
                queryText += $" WHERE ganttitemid='{selectedGanttItem.Id}' AND unitid='{_usersLibraryService.UsersLibrary.CurrentUser.Id}';";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();
            }
            
            _connectionService.CloseConnection();
        }

        public void UpdateTaskResources(TaskResourceInfo taskResourceInfo)
        {
            _connectionService.CheckDbConnection();
            
            var queryText =  $@"UPDATE taskitems_resource SET percent='{taskResourceInfo.Percent.ToString().Replace(',','.')}' WHERE id='{taskResourceInfo.Id}'";
            
            NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
            command.ExecuteNonQuery();
            
            _connectionService.CloseConnection();
        }

        public ProjectsLibraryService()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();

            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
        }
    }
}