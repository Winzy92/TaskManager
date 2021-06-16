using System;
using CommonServiceLocator;
using Prism.Regions;

namespace TaskManager.Shell.ViewModels
{
    public class TaskManagerViewModel
    {

        public TaskManagerViewModel()
        {
            
        }
        
        private void DbInitializedEventHandler(Boolean obj)
        {
            //RegionManager.Regions["TaskManagerToolBar"].RequestNavigate("TaskManagerToolBar");
            if (obj)
            {
                //RegionManager.Regions["GanttControl"].RequestNavigate("GanttControl");
                //RegionManager.Regions["TaskManagerToolBar"].RequestNavigate("TaskManagerToolBar");
            }
        }
    }
}