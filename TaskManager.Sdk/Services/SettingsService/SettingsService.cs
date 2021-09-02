using System;
using System.IO;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using Newtonsoft.Json;

namespace TaskManager.Sdk.Services.SettingsService
{
    public class SettingsService : ISettingsService
    {
        public const String ConnectionSettings = "connection_settings.json";
        
        public SettingsInfo Settings { get; set; } = new SettingsInfo();
        
        public Boolean IsNeedToEditDbSettings { get; set; }
        
        public void LoadSettings()
        {
            if (!File.Exists(ConnectionSettings))
            {
                CreateEmptyConfiguration();

                IsNeedToEditDbSettings = true;
            }

            Settings.DbConnectionInfo = JsonConvert.DeserializeObject<DbConnectionInfo>(File.ReadAllText(ConnectionSettings));
        }

        private void CreateEmptyConfiguration()
        {
            var settingsInfo = new SettingsInfo()
            {
                DbConnectionInfo = new DbConnectionInfo()
            };

            var serializedSettings = JsonConvert.SerializeObject(settingsInfo);

            File.WriteAllText(ConnectionSettings, serializedSettings);
        }

        public void SaveSettings()
        {
            var serializedSettings = JsonConvert.SerializeObject(Settings.DbConnectionInfo);

            File.WriteAllText(ConnectionSettings, serializedSettings);
        }
        
    }
}