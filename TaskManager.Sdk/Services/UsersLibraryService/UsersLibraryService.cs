using System;
using System.Linq;
using Npgsql;
using Prism.Mvvm;
using TaskManager.Sdk.Core.Containers;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Services.UsersLibraryService
{
    public class UsersLibraryService : BindableBase, IUsersLibraryService
    {
        private readonly ISettingsService _settingsService;

        private readonly IDatabaseConnectionService _connectionService;
        
        private UsersInfo _currentUser;

        public UsersLibraryInfo UsersLibrary { get; set; } = new UsersLibraryInfo();

        public UsersInfo CurrentUser
        {
            get => _currentUser;
            set
            {
                base.SetProperty(ref _currentUser, value);
            }
        }
        
        public void LoadUsersPositions()
        {
            _connectionService.CheckDbConnection();

            var queryText = "SELECT * FROM positions;";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var positionItemInfo = new PositionsInfo()
                {
                    Id = data.GetInt32(0),
                    Name = data.GetString(1),
                    CanModify = data.GetBoolean(2)
                };
                
                UsersLibrary.PositionsInfoItems.Add(positionItemInfo);
            }

            _connectionService.CloseConnection();
        }

        public void LoadGanttResourceItems()
        {
            _connectionService.CheckDbConnection();

            var queryText = "SELECT * FROM ganttresource_items;";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var ganttResource = new GanttResourceItemInfo()
                {
                    Id = Convert.ToInt32(data["id"]),
                    Name = Convert.ToString(data["name"]),
                };
                
                UsersLibrary.GanttResourceItems.Add(ganttResource);
            }

            _connectionService.CloseConnection();
        }

        public void LoadUsers()
        {
            _connectionService.CheckDbConnection();

            var queryText = "SELECT * FROM users;";

            var command = new NpgsqlCommand(queryText, _connectionService.Connection);
            var data = command.ExecuteReader();

            while (data.Read())
            {
                var userInfo = new UsersInfo()
                {
                    Id = Convert.ToInt32(data["id"]),
                    Name = Convert.ToString(data["name"]),
                    GanttSourceItemId = Convert.ToInt32(data["ganttsourceitemid"]),
                    Password = Convert.ToString(data["password"]),
                    PositionId = Convert.ToInt32(data["positionid"])
                };
                
                UsersLibrary.Users.Add(userInfo);
            }

            _connectionService.CloseConnection();
        }

        public void AddUser(UsersInfo usersInfo)
        {
            _connectionService.CheckDbConnection();
            
            if (usersInfo != null)
            {
                var queryText =
                    $@"INSERT INTO users(name, ganttsourceitemid, positionid, password) VALUES('{usersInfo.Name}', '{usersInfo.GanttSourceItemId}', '{usersInfo.PositionId}', '{usersInfo.Password}') RETURNING id";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                var executeScalar = command.ExecuteScalar();

                if (executeScalar is Int32 id)
                {
                    usersInfo.Id = id;
                }

                UsersLibrary.Users.Add(usersInfo);
            }
            
            _connectionService.CloseConnection();
        }

        public void AddGanttSourceItem(GanttResourceItemInfo ganttResourceItemInfo)
        {
            _connectionService.CheckDbConnection();
            
            if (ganttResourceItemInfo != null)
            {
                var queryText =
                    $@"INSERT INTO ganttresource_items(name) VALUES('{ganttResourceItemInfo.Name}') RETURNING id";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                var executeScalar = command.ExecuteScalar();

                if (executeScalar is Int32 id)
                {
                    ganttResourceItemInfo.Id = id;
                }

                UsersLibrary.GanttResourceItems.Add(ganttResourceItemInfo);
            }
            
            _connectionService.CloseConnection();
        }

        public void RemoveSelectedItemUserLibrary(Object selectedItem)
        {
            _connectionService.CheckDbConnection();
            
            if (selectedItem is GanttResourceItemInfo ganttResourceItemInfo)
            {
                var queryText = $"DELETE FROM ganttresource_items WHERE id='{ganttResourceItemInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();

                var element =
                    UsersLibrary.GanttResourceItems.FirstOrDefault(t =>
                        t.Id == ganttResourceItemInfo.Id);
                if (element != null)
                {
                    UsersLibrary.GanttResourceItems.Remove(element);
                }
            }

            if (selectedItem is UsersInfo usersInfo)
            {
                var queryText = $"DELETE FROM users WHERE id='{usersInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();
                
                var element =
                    UsersLibrary.Users.FirstOrDefault(t =>
                        t.Id == usersInfo.Id);
                if (element != null)
                {
                    UsersLibrary.Users.Remove(element);
                }
            }
            
            _connectionService.CloseConnection();
        }

        public void UpdatePropertiesSelectedItemUserLibrary(object selectedItem, string property)
        {
            _connectionService.CheckDbConnection();

            if (selectedItem is UsersInfo usersInfo)
            {
                var elem = UsersLibrary.Users.FirstOrDefault(t=>t.Id == usersInfo.Id);
                
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
                    NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                    command.ExecuteNonQuery();
                }
            }

            if (selectedItem is GanttResourceItemInfo ganttResourceItemInfo)
            {
                var queryText =  $@"UPDATE ganttresource_items SET name='{ganttResourceItemInfo.Name}' WHERE id='{ganttResourceItemInfo.Id}'";
                NpgsqlCommand command = new NpgsqlCommand(queryText, _connectionService.Connection);
                command.ExecuteNonQuery();
            }
            
            _connectionService.CloseConnection();
        }

        public UsersLibraryService()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();
            
        }
    }
}