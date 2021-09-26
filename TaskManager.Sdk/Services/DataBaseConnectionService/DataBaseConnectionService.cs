using System;
using System.Collections;
using System.Collections.ObjectModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Npgsql;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Services.DataBaseConnectionService
{
    public class DataBaseConnectionService : BindBase, IDatabaseConnectionService
    {
        
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
            Connection = new NpgsqlConnection(BuildConnectionString(_settingsService.Settings.DbConnectionInfo));

            try
            {
                Connection.Open();
                IsConnected = true;
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                IsConnected = false;
            }
        }
        
        public void CheckDbConnection()
        {
            if (!IsConnected)
            {
                OpenConnection();
            }
        }
        
        public void CloseConnection()
        {
            Connection.Close();
            IsConnected = false;
        }

        public bool IsConnected { get; set; }
        public NpgsqlConnection Connection { get; set; }
        
    }
}