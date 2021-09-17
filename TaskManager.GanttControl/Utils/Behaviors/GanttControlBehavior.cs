using System;
using System.Collections;
using System.Linq;
using System.Windows;
using System.Windows.Interactivity;
using DevExpress.Xpf.Core.Native;
using DevExpress.Xpf.Gantt;
using DevExpress.Xpf.Grid.TreeList;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.GanttControl.Utils.Behaviors
{
    public class MainGanttControlBehavior : Behavior<DevExpress.Xpf.Gantt.GanttControl>
    {
        
        private readonly IProjectsLibraryService _projectsLibraryService;
        
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs e)
        {
            TaskManagerServices.Instance.EventAggregator.GetEvent<UpdateMainGanttEvent>().Subscribe(UpdateViewGanttControl);
            AssociatedObject.View.AddingNewResourceLink += GanttView_AddingNewResourceLink;
        }

        void GanttView_AddingNewResourceLink(object sender, AddingNewResourceLinkEventArgs e) 
        {
            var item = e.Task;
            TaskResourceInfo taskResourceInfo = new TaskResourceInfo();

            if (item is GanttTreeViewItemInfo ganttTreeViewItemInfo)
            {
                var element = _projectsLibraryService.ProjectsLibrary.GanttItems.FirstOrDefault(t => (Int32)t.Id.Id == (Int32)ganttTreeViewItemInfo.Id.Id);

                if (element != null)
                {
                    if (taskResourceInfo.TaskId is Int32 tInt && element.Id.Id is Int32 gInt)
                    {
                        taskResourceInfo.TaskId = gInt;
                    }
                
                    if (taskResourceInfo.GanttSourceId is Int32 GSId && ((GanttResourceItemInfo)e.Resource).Id is Int32 elemId)
                    {
                        taskResourceInfo.GanttSourceId = elemId;
                        taskResourceInfo.Percent = e.AllocationPercentage;
                    }
                    
                    element.ResourceIds.Add(taskResourceInfo);
                }
            }
        }

        private void UpdateViewGanttControl()
        {
            AssociatedObject.View.ResourceLinksPath = "";
            AssociatedObject.View.ResourceLinksPath = "Id.ResourceIds";
            AssociatedObject.View.AutoExpandAllNodes = false;
            AssociatedObject.View.AutoExpandAllNodes = true;
        }

        public MainGanttControlBehavior()
        {
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();
        }
    }
}