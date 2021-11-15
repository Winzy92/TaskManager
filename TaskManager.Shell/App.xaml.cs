using System;
using System.Windows;
using Prism.Ioc;
using Prism.Modularity;
using Prism.Regions;
using TaskManager.GanttControl.Views;
using TaskManager.ProjectsLibrary.ViewModels;
using TaskManager.ProjectsLibrary.Views;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services;
using TaskManager.Shell.Dialogs.DBConnectionDialog.ViewModels;
using TaskManager.Shell.Dialogs.DBConnectionDialog.Views;
using TaskManager.Shell.Dialogs.MessageDialog.ViewModels;
using TaskManager.Shell.Dialogs.MessageDialog.Views;
using TaskManager.Shell.Dialogs.UserAuthorizationDialog.ViewModels;
using TaskManager.Shell.Dialogs.UserAuthorizationDialog.Views;
using TaskManager.SpecialGanttControl.Dialogs.AddSpecialGanttItemDialog.ViewModels;
using TaskManager.SpecialGanttControl.Dialogs.AddSpecialGanttItemDialog.Views;
using TaskManager.SpecialGanttControl.Views;
using TaskManager.ToolBar.Views;
using TaskManager.UserGanttControll.Views;
using TaskManager.UsersLibrary.ViewModels;
using TaskManager.UsersLibrary.Views;

namespace TaskManager.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            containerRegistry.RegisterDialog<DBConnectionDialog, DBConnectionDialogViewModel>("DBConnectDialog");
            containerRegistry.RegisterDialog<UserAuthorizationDialog, UserAuthorizationDialogViewModel>("UserAuthorizationDialog");
            containerRegistry.RegisterDialog<TMProjectsLibrary, TMProjectsLibraryViewModel>("TMProjectsLibrary");
            containerRegistry.RegisterDialog<UsersLibraryControl, UsersLibraryControlViewModel>("UsersLibraryControl");
            containerRegistry.RegisterDialog<SpecialGanttItemDialog, SpecialGanttItemDialogViewModel>("SpecialGanttItemDialog");
            containerRegistry.RegisterDialog<MessageDialog, MessageDialogViewModel>("MessageDialog");
        }
        
        /// <summary>Contains actions that should occur last.</summary>
        protected override void OnInitialized()
        {
            Container.Resolve<ISettingsService>().LoadSettings();

            try
            {
                var databaseConnectionService = Container.Resolve<IDatabaseConnectionService>();
            }
            catch (Exception e)
            {
                Console.WriteLine(e);
                throw;
            }
            
            base.OnInitialized();

            base.Container.Resolve<IRegionManager>().RegisterViewWithRegion("TaskManagerToolBar", typeof(TaskManagerToolBar));
            base.Container.Resolve<IRegionManager>().RegisterViewWithRegion("TaskManagerGanttControl", typeof(TaskManagerGanttControl));
            base.Container.Resolve<IRegionManager>().RegisterViewWithRegion("UserGanttControl", typeof(UserGanttControl));
            base.Container.Resolve<IRegionManager>().RegisterViewWithRegion("SpecialUsersGanttControl", typeof(SpecialUsersGanttControl));
        }
        
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.TaskManager>();
        }
        
        protected override void ConfigureModuleCatalog(IModuleCatalog moduleCatalog)
        {
            base.ConfigureModuleCatalog(moduleCatalog);

            moduleCatalog.AddModule(new ModuleInfo(typeof(ServiceModule)));
        }
    }
}