using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.SpecialGanttControl.ViewModels
{
    public class SpecialUsersGanttControlViewModel : BindBase 
    {
        private readonly ISettingsService _settingsService;
        
        private readonly IDatabaseConnectionService _connectionService;
        
        private readonly IDialogService _dialogService;
        
        public DelegateCommand OpenAddSpecialGanttItemDialog { get; }
        
        public ObservableCollection<GanttItemInfo> SpecialUserTasks { get; set; }

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
                
                if (SelectedItem != null && value is GanttItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedItem, value);
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttItemInfo ganttItemInfo)
            {
                _connectionService.UpdateTaskUnits(ganttItemInfo, e.PropertyName);
            }
        }

        private void CreateSpecialUserTasksCollection(ObservableCollection<GanttItemInfo> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                /*Добавляем корневой элемент*/
                if (item.ParentId is Int32 parentId && parentId != 0)
                {
                    var rootElement =
                        _settingsService.Settings.GanttItems.FirstOrDefault(t => (Int32)t.Id == parentId);
                    if (rootElement != null)
                    {
                        var rootGanttItem = new GanttItemInfo()
                        {
                            Id = rootElement.Id,
                            ParentId = rootElement.ParentId,
                            Progress = rootElement.Progress,
                            BaselineStartDate = rootElement.BaselineStartDate,
                            BaselineFinishDate = rootElement.BaselineFinishDate,
                            Name = rootElement.Name,
                            NumOfContract = rootElement.NumOfContract,
                            Tag = rootElement.Tag,
                            StartDate = rootElement.StartDate,
                            FinishDate = rootElement.FinishDate,
                            IsAdditional = true
                        };
                        
                        if (!SpecialUserTasks.Any(t=>(Int32)t.Id == (Int32)rootGanttItem.Id))
                            SpecialUserTasks.Add(rootGanttItem);
                    }
                }

                /*Добавлеяем дочерний элемент*/
                var ganttItem = new GanttItemInfo()
                {
                    Id = item.Id,
                    ParentId = item.ParentId,
                    Progress = item.Progress,
                    BaselineStartDate = item.BaselineStartDate,
                    BaselineFinishDate = item.BaselineFinishDate,
                    Name = item.Name,
                    NumOfContract = item.NumOfContract,
                    Tag = item.Tag,
                    StartDate = item.StartDate,
                    FinishDate = item.FinishDate,
                    IsAdditional = true
                };

                if (!SpecialUserTasks.Any(t=>(Int32)t.Id == (Int32)ganttItem.Id))
                {
                    ganttItem.ResourceUsers.Add(_settingsService.Settings.CurrentUser);
                    ganttItem.ResourceIds.Add(_settingsService.Settings.CurrentUser.Id);
                    ganttItem.UsersInfos.Add(_settingsService.Settings.CurrentUser);
                    ganttItem.ListUsers.Add(_settingsService.Settings.CurrentUser);
                    _connectionService.UpdateGanttObject(ganttItem, "ListUsers");
                
                    SpecialUserTasks.Add(ganttItem);
                }
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
            
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();

            SpecialUserTasks = _settingsService.Settings.CurrentUserAdditionalGanttItems;
            
            OpenAddSpecialGanttItemDialog = new DelegateCommand(()=> 
                _dialogService.Show("SpecialGanttItemDialog", new DialogParameters(), result =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        var SelectedItems =
                            result.Parameters.GetValue<ObservableCollection<GanttItemInfo>>("SelectedItems");
                        CreateSpecialUserTasksCollection(SelectedItems);
                    }
                }));
        }
    }
}