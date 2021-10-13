using System;
using System.Windows;
using System.Windows.Interactivity;
using DevExpress.Mvvm.Gantt;
using DevExpress.Xpf.Gantt;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.GanttControl.Utils.Behaviors
{
    public class MainGanttControlBehavior : Behavior<DevExpress.Xpf.Gantt.GanttControl>
    {
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
            e.NewResourceLink = new TaskResourceInfo(){ResourceId = ((GanttResource)e.Resource).Id, AllocationPercentage = e.AllocationPercentage, TaskId = (Int32)((GanttTreeViewItemInfo)e.Task).Id.Id};
        }

        private void UpdateViewGanttControl()
        {
            AssociatedObject.View.AutoExpandAllNodes = false;
            AssociatedObject.View.AutoExpandAllNodes = true;
        }
    }
}