using System;
using System.ComponentModel;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Mvvm;
using Prism.Regions;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Shell.ViewModels
{
    public class TaskManagerViewModel : BindableBase
    {
        public String UserTasks { get; set; }

        public String SpecialUserTasks { get; set; }

        private GanttItemInfo _selectedItem;

        public GanttItemInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SelectedItem != null && SelectedItem.Id is GanttItemInfo ganttItemInfo)
                {
                    ganttItemInfo.PropertyChanged -= WorkspaceItemInfoOnPropertyChanged;
                }

                if (value != null && value.Id is GanttItemInfo ganttItem)
                {
                    ganttItem.PropertyChanged += WorkspaceItemInfoOnPropertyChanged;
                }

                _selectedItem = value;

                RaisePropertyChanged(nameof(SelectedItem));
            }
        }
        
        private void WorkspaceItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem.Id is GanttItemInfo ganttItemInfo)
            {
                TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>().UpdateGanttObject(ganttItemInfo, e.PropertyName);
            }
        }
        
        private readonly IDialogService _dialogService;
        
        public DelegateCommand CommandStartupDialog { get; }

        public TaskManagerViewModel()
        {
            _dialogService = TaskManagerServices.Instance.GetInstance<IDialogService>();
            CommandStartupDialog = new DelegateCommand(CommandStartUp);
            UserTasks = "Задачи пользователя";
            SpecialUserTasks = "Внеплановые задачи пользователя";
        }
        
        
        private void CommandStartUp()
        {
            _dialogService.ShowDialog("DBConnectDialog", new DialogParameters (),  result => { });
        }
    }
}