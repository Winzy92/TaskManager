using System;
using System.Windows;
using Prism.Ioc;
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
            containerRegistry.Register<Object, TaskManagerToolBar>("TaskManagerToolBar");
            //containerRegistry.Register<Object, GanttControl.Views.GanttControl>("GanttControl");
        }
        
        protected override Window CreateShell()
        {
            return Container.Resolve<Views.TaskManager>();
        }
    }
}