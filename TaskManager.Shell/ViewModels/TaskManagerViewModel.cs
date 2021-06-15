using System;
using CommonServiceLocator;
using Prism.Events;
using Prism.Mvvm;
using Prism.Regions;
using TaskManager.Sdk.Events;

namespace TaskManager.Shell.ViewModels
{
    public class TaskManagerViewModel : BindableBase
    {
        public IRegionManager RegionManager { get; set; }

        public TaskManagerViewModel()
        {
            RegionManager = ServiceLocator.Current.GetInstance<IRegionManager>().CreateRegionManager();
            //RegionManager.Regions["TaskManagerToolBar"].RequestNavigate("TaskManagerToolBar");
            DbInitializedEventHandler(true);
            // ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<DbInitializedEvent>().Publish(true);
            // ServiceLocator.Current.GetInstance<IEventAggregator>().GetEvent<DbInitializedEvent>()
            //     .Subscribe(DbInitializedEventHandler);
        }
        
        private void DbInitializedEventHandler(Boolean obj)
        {
            RegionManager.Regions["TaskManagerToolBar"].RequestNavigate("TaskManagerToolBar");
            if (obj)
            {
                //RegionManager.Regions["GanttControl"].RequestNavigate("GanttControl");
                //RegionManager.Regions["TaskManagerToolBar"].RequestNavigate("TaskManagerToolBar");
            }
        }
    }
}