using System;
using System.Windows;
using System.Windows.Interactivity;
using DevExpress.Mvvm.Gantt;
using DevExpress.Xpf.Gantt;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.GanttControl.Utils.Behaviors
{
    public class MainGanttControlBehavior : Behavior<DevExpress.Xpf.Gantt.GanttControl>
    {
        private readonly IReportService _reportService;
        
        private readonly IUsersLibraryService _usersLibraryService;
        
        protected override void OnAttached()
        {
            AssociatedObject.Loaded += AssociatedObjectOnLoaded;
        }

        private void AssociatedObjectOnLoaded(object sender, RoutedEventArgs e)
        {
            TaskManagerServices.Instance.EventAggregator.GetEvent<UpdateMainGanttEvent>().Subscribe(UpdateViewGanttControl);
            TaskManagerServices.Instance.EventAggregator.GetEvent<PrintCurrentGanttItemsEvent>().Subscribe(PrintCurrentGanttItems);
            TaskManagerServices.Instance.EventAggregator.GetEvent<PrintMonthGanttItemsEvent>().Subscribe(PrintMonthGanttItems);
            AssociatedObject.View.AddingNewResourceLink += GanttView_AddingNewResourceLink;
        }

        private void PrintMonthGanttItems()
        {
            throw new NotImplementedException();
        }

        private void PrintCurrentGanttItems()
        {
            TaskManagerServices.Instance.GetInstance<IReportService>().CreateMonthReport(_usersLibraryService.UsersLibrary.CurrentUser);
            //AssociatedObject.View.ShowRibbonPrintPreviewDialog(Application.Current.MainWindow);
            //AssociatedObject.View.ExportToXlsx(@"G:\CodeSolutions\grid_export.xlsx");
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

        public MainGanttControlBehavior()
        {
            _reportService = TaskManagerServices.Instance.GetInstance<IReportService>();
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
        }
    }
}