using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.SpecialGanttControl.ViewModels
{
    public class SpecialUsersGanttControlViewModel : BindBase 
    {
        private readonly IUsersLibraryService _usersLibraryService;
        
        private readonly IProjectsLibraryService _projectsLibraryService;
        
        private readonly IDialogService _dialogService;
        
        public DelegateCommand OpenAddSpecialGanttItemDialog { get; }
        
        public ObservableCollection<GanttTreeViewItemInfo> SpecialUserTasks { get; set; }

        private GanttTreeViewItemInfo _selectedItem;

        public GanttTreeViewItemInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SelectedItem != null && SelectedItem is GanttTreeViewItemInfo ganttItemInfo)
                {
                    ganttItemInfo.Id.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                }
                
                if (SelectedItem != null && value is GanttTreeViewItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.Id.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedItem, value);
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttTreeViewItemInfo ganttItemInfo)
            {
                _projectsLibraryService.UpdateTaskUnits(ganttItemInfo.Id, e.PropertyName);
            }
        }
        
        private DelegateCommand<string> _removeSpecialGanttItem;
        
        public DelegateCommand<string> RemoveSpecialGanttItem =>
            _removeSpecialGanttItem ?? (_removeSpecialGanttItem = new DelegateCommand<string>(RemoveSpecialItem));

        private void RemoveSpecialItem(string obj)
        {
            throw new NotImplementedException();
        }

        public SpecialUsersGanttControlViewModel()
        {
            _dialogService = TaskManagerServices.Instance.GetInstance<IDialogService>();
            
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();

            SpecialUserTasks = _projectsLibraryService.ProjectsLibrary.CurrentUserAdditionalGanttItems;
            
            OpenAddSpecialGanttItemDialog = new DelegateCommand(()=> 
                _dialogService.Show("SpecialGanttItemDialog", new DialogParameters(), result =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        var SelectedItems =
                            result.Parameters.GetValue<ObservableCollection<GanttItemInfo>>("SelectedItems");
                        //CreateSpecialUserTasksCollection(SelectedItems);
                    }
                }));
        }
    }
}