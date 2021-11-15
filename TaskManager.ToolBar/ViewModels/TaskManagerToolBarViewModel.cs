using System;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.ToolBar.ViewModels
{
    public class TaskManagerToolBarViewModel
    {
        private readonly IDialogService _dialogService;
        public DelegateCommand OpenLibrary { get; }
        public DelegateCommand OpenUsersLibrary { get; }
        public DelegateCommand OpenChangeActiveUserDialog { get; }
        
        public DelegateCommand OpenPrintCurrentGanttItemsDialog { get; }
        
        public TaskManagerToolBarViewModel()
        {
            _dialogService = TaskManagerServices.Instance.GetInstance<IDialogService>();

            OpenLibrary = new DelegateCommand(() =>
                _dialogService.ShowDialog("TMProjectsLibrary", new DialogParameters(), result => { }));

            OpenUsersLibrary = new DelegateCommand(() =>
                _dialogService.ShowDialog("UsersLibraryControl", new DialogParameters(), result => { }));

            var keyvalue = new DialogParameters();
            keyvalue.Add("key", 1);
            
            OpenChangeActiveUserDialog = new DelegateCommand(() => 
                _dialogService.ShowDialog("UserAuthorizationDialog", keyvalue,  result => { }));
            
            OpenPrintCurrentGanttItemsDialog = new DelegateCommand(() => 
                TaskManagerServices.Instance.EventAggregator.GetEvent<PrintCurrentGanttItemsEvent>().Publish());
            
            

        }
    }
}