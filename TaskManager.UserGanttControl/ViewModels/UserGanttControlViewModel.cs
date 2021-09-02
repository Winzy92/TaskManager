using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.UserGanttControll.ViewModels
{
    public class UserGanttControlViewModel : BindBase 
    {
        private readonly ISettingsService _settingsService;
        
        private readonly IDatabaseConnectionService _connectionService;
        
        public ObservableCollection<GanttItemInfo> UserTasks { get; set; }
        
        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; }
        
        private GanttItemInfo _selectedItem;

        public GanttItemInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SelectedItem != null && SelectedItem is GanttItemInfo ganttItemInfo)
                {
                    ganttItemInfo.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedItem, value);
                
                if (SelectedItem != null && value is GanttItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttItemInfo ganttItemInfo)
            {
                _connectionService.UpdateTaskUnits(ganttItemInfo, e.PropertyName);
            }
        }

        public UserGanttControlViewModel()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();
            
            GanttResourceItems = _settingsService.Settings.GanttResourceItems;

            UserTasks = _settingsService.Settings.CurrentUserGanttItems;
        }
    }
}