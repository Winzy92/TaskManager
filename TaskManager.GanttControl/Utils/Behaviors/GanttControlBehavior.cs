using System.Collections;
using System.Windows;
using System.Windows.Interactivity;
using DevExpress.Xpf.Gantt;
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
        }

        private void UpdateViewGanttControl()
        {
            AssociatedObject.View.ResourceLinksPath = "";
            AssociatedObject.View.ResourceLinksPath = "Id.ResourceIds";
            AssociatedObject.View.AutoExpandAllNodes = false;
            AssociatedObject.View.AutoExpandAllNodes = true;
            
        }
    }
}