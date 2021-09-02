using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using DevExpress.Mvvm.Gantt;
using Prism.Commands;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.GanttControl.ViewModels
{
    public class TaskManagerGanttControlViewModel : BindBase
    {
        private readonly ISettingsService _settingsService;
        
        private readonly IDatabaseConnectionService _connectionService;
        
        public ObservableCollection<GanttItemInfo> Tasks { get; set; }
        
        public Boolean CanModify { get; set; }

        public UsersInfo CurrentUser { get; set; }

        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; }
        
        public ObservableCollection<TaskResourceInfo> TaskResources { get; set; }

        private GanttItemInfo _selectedItem;

        public GanttItemInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SelectedItem != null && SelectedItem is GanttItemInfo ganttItemInfo)
                {
                    ganttItemInfo.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                    ganttItemInfo.ResourceIds.CollectionChanged -= ResourceIdsCollectionChanged;
                }
                
                base.SetProperty(ref _selectedItem, value);
                
                if (SelectedItem != null && value is GanttItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.PropertyChanged += GanttItemInfoOnPropertyChanged;
                    ganttItemInfoItem.ResourceIds.CollectionChanged += ResourceIdsCollectionChanged;
                }
            }
        }

        private void ResourceIdsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            if (SelectedItem is GanttItemInfo ganttItemInfo)
            {
                switch (e.Action)
                {
                    case NotifyCollectionChangedAction.Add:
                        _connectionService.AddResourceLink(e.NewItems, ganttItemInfo);
                        _connectionService.UpdateResourceLinks(ganttItemInfo);
                        break;
                    
                    case NotifyCollectionChangedAction.Remove:
                        _connectionService.RemoveResourceLink(e.OldItems, ganttItemInfo);
                        _connectionService.UpdateResourceLinks(ganttItemInfo);
                        break;
                }
            }
        }

        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttItemInfo ganttItemInfo)
            {
                _connectionService.UpdateGanttObject(ganttItemInfo, e.PropertyName);
            }
        }

        public TaskManagerGanttControlViewModel()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();

            Tasks = _settingsService.Settings.ActiveTasks;

            GanttResourceItems = _settingsService.Settings.GanttResourceItems;

            TaskResources = _settingsService.Settings.TaskResources;
            
            CurrentUser = _settingsService.Settings.CurrentUser;

            CanModify = _settingsService.Settings.PositionsInfoItems.
                FirstOrDefault(t => t.Id == CurrentUser.PositionId).CanModify;
        }
    }
}