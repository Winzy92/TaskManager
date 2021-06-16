using System;
using System.Windows;
using Prism.Ioc;
using Prism.Regions;
using TaskManager.GanttControl.Views;
using TaskManager.ToolBar.Views;

namespace TaskManager.Shell
{
    /// <summary>
    /// Interaction logic for App.xaml
    /// </summary>
    public partial class App
    {
        protected override void RegisterTypes(IContainerRegistry containerRegistry)
        {
            
        }
        
        /// <summary>Contains actions that should occur last.</summary>
        protected override void OnInitialized()
        {
            base.OnInitialized();

            base.Container.Resolve<IRegionManager>().RegisterViewWithRegion("TaskManagerToolBar", typeof(TaskManagerToolBar));
            
            base.Container.Resolve<IRegionManager>().RegisterViewWithRegion("TaskManagerGanttControl", typeof(TaskManagerGanttControl));
        }
        
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.TaskManager>();
        }
    }
}