using System;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Interfaces
{
    public interface ISettingsService
    {
        SettingsInfo Settings { get; set; }
        
        void LoadSettings();
        
        void SaveSettings();
    }
}