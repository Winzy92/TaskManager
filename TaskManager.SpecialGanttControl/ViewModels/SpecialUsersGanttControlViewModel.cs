using System;
using System.Collections;
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
        
        public DelegateCommand RemoveSpecialGanttItem { get; }
        
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
        
        private ObservableCollection<GanttTreeViewItemInfo> _selectedSpecialItems;

        public ObservableCollection<GanttTreeViewItemInfo> SelectedSpecialItems
        {
            get => _selectedSpecialItems;
            set => base.SetProperty(ref _selectedSpecialItems, value);
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttTreeViewItemInfo ganttItemInfo)
            {
                _projectsLibraryService.UpdateTaskUnits(ganttItemInfo.Id, e.PropertyName);
            }
        }

        private void RemoveSpecialItem()
        {
            if (SelectedSpecialItems != null)
            {
                foreach (var item in SelectedSpecialItems.ToList())
                {
                    //Проверяем на корневой тип
                    if (item.ParentId == null)
                    {
                        var collection = SpecialUserTasks.Where(t => (Int32) t.ParentId.Id == (Int32) item.Id.Id);

                        if (collection != null)
                        {
                            foreach (var element in collection)
                            {
                                element.Id.ResourceUsers.Remove(_usersLibraryService.UsersLibrary.CurrentUser);
                                element.Id.ListUsers.Add(_usersLibraryService.UsersLibrary.CurrentUser);
                                _usersLibraryService.UsersLibrary.CurrentUser.Tasks.Remove(element.Id);
                                _projectsLibraryService.UpdateGanttObject(element.Id, "ListUsers");
                                SpecialUserTasks.Remove(element);
                            }
                        }
                        SpecialUserTasks.Remove(item);
                        
                    }
                    else
                    {
                        var elements = SpecialUserTasks.Where(t => t.ParentId != null && (Int32)t.ParentId.Id == (Int32)item.ParentId.Id);
                        item.Id.ResourceUsers.Remove(_usersLibraryService.UsersLibrary.CurrentUser);
                        item.Id.ListUsers.Remove(_usersLibraryService.UsersLibrary.CurrentUser);
                        _usersLibraryService.UsersLibrary.CurrentUser.Tasks.Remove(item.Id);
                        _projectsLibraryService.UpdateGanttObject(item.Id, "ListUsers");
                        SpecialUserTasks.Remove(item);
                        if (elements.Count() <= 1)
                        {
                            foreach (var element in elements)
                            {
                                SpecialUserTasks.Remove(element);
                            }
                        }
                    }
                }
            }
        }

        public SpecialUsersGanttControlViewModel()
        {
            _dialogService = TaskManagerServices.Instance.GetInstance<IDialogService>();
            
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();

            SpecialUserTasks = _projectsLibraryService.ProjectsLibrary.CurrentUserAdditionalGanttItems;
            
            SelectedSpecialItems = new ObservableCollection<GanttTreeViewItemInfo>();
            
            RemoveSpecialGanttItem = new DelegateCommand(RemoveSpecialItem);
            
            OpenAddSpecialGanttItemDialog = new DelegateCommand(()=> 
                _dialogService.Show("SpecialGanttItemDialog", new DialogParameters(), result =>
                {
                    if (result.Result == ButtonResult.OK)
                    {
                        var SelectedItems =
                            result.Parameters.GetValue<ObservableCollection<GanttTreeViewItemInfo>>("SelectedItems");
                        AddItemSpecialUserTasksCollection(SelectedItems);
                    }
                }));
        }
        
        private void AddItemSpecialUserTasksCollection(ObservableCollection<GanttTreeViewItemInfo> selectedItems)
        {
            foreach (var item in selectedItems)
            {
                /*Добавляем корневой элемент*/
                if (item.ParentId != null && item.ParentId.Id is Int32 parentId)
                {
                    var rootElement =
                        _projectsLibraryService.ProjectsLibrary.AllGanttItems.FirstOrDefault(t => (Int32)t.Id.Id == parentId && t.ParentId == null);
                    if (rootElement != null)
                    {
                        rootElement.Id.IsAdditional = true;

                        if (!SpecialUserTasks.Any(t=>(Int32)t.Id.Id == (Int32)rootElement.Id.Id && t.ParentId == null))
                            SpecialUserTasks.Add(rootElement);
                    }
                }

                /*Добавлеяем дочерний элемент*/
                item.Id.IsAdditional = true;
                
                if (item.Id.IsActive || item.Id.GlobalTask)
                {
                    if (!SpecialUserTasks.Any(t=>(Int32)t.Id.Id == (Int32)item.Id.Id))
                    {
                        item.Id.ResourceUsers.Add(_usersLibraryService.UsersLibrary.CurrentUser);
                        
                        if (!item.ResourceIds.Any(t=>(Int32)t.ResourceId == _usersLibraryService.UsersLibrary.CurrentUser.GanttSourceItemId))
                        {
                            var newlink = new TaskResourceInfo()
                            {
                                ResourceId = _usersLibraryService.UsersLibrary.CurrentUser.GanttSourceItemId,
                                TaskId = (Int32) item.Id.Id,
                                AllocationPercentage = 1
                            };
                            _projectsLibraryService.AddResourceLink(
                                new Collection<TaskResourceInfo>()
                                {
                                    newlink
                                });
                            
                            item.ResourceIds.Add(newlink);
                        }
                        
                        item.Id.UsersInfos.Add(_usersLibraryService.UsersLibrary.CurrentUser);
                        item.Id.ListUsers.Add(_usersLibraryService.UsersLibrary.CurrentUser);
                        _usersLibraryService.UsersLibrary.CurrentUser.Tasks.Add(item.Id);
                        _projectsLibraryService.UpdateGanttObject(item.Id, "ListUsers");
                        SpecialUserTasks.Add(item);
                    }
                }
            }
        }
    }
}