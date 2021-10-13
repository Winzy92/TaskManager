using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using DevExpress.Mvvm.Gantt;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.UserGanttControll.ViewModels
{
    public class UserGanttControlViewModel : BindBase 
    {
        private readonly IUsersLibraryService _usersLibraryService;
        
        private readonly IProjectsLibraryService _projectsLibraryService;
        
        public ObservableCollection<GanttTreeViewItemInfo> UserTasks { get; set; }
        
        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; }
        
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
                
                base.SetProperty(ref _selectedItem, value);
                
                if (SelectedItem != null && value is GanttTreeViewItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.Id.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttTreeViewItemInfo ganttItemInfo)
            {
                _projectsLibraryService.UpdateTaskUnits(ganttItemInfo.Id, e.PropertyName);
            }
        }

        public UserGanttControlViewModel()
        {
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();
            
            GanttResourceItems = _usersLibraryService.UsersLibrary.GanttResourceItems;

            UserTasks = _projectsLibraryService.ProjectsLibrary.CurrentUserGanttItems;
        }
    }
}