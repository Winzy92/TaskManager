﻿using Prism.Ioc;
using Prism.Modularity;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.ExcelReportService;

namespace TaskManager.Sdk.Services
{
    public class ServiceModule : IModule
    {
        public void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterSingleton<ISettingsService, SettingsService.SettingsService>();
            containerRegistry.RegisterSingleton<IReportService, ReportService>();
            containerRegistry.RegisterSingleton<IDatabaseConnectionService, DataBaseConnectionService.DataBaseConnectionService>();
            containerRegistry.RegisterSingleton<IProjectsLibraryService, ProjectsLibraryService.ProjectsLibraryService>();
            containerRegistry.RegisterSingleton<IUsersLibraryService, UsersLibraryService.UsersLibraryService>();
        }

        public void OnInitialized(IContainerProvider containerProvider)
        {
            
        }
    }
}