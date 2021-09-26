using System;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.ProjectsLibrary.ViewModels
{
    public class TMProjectsLibraryViewModel : BindBase, IDialogAware
    {
        private readonly IUsersLibraryService _usersLibraryService;
        
        private readonly IProjectsLibraryService _projectsLibraryService;
        public UsersInfo CurrentUser { get; set; }
        public Boolean CanModify { get; set; }
        public String ContextMenuItemName { get; set; }
        public DelegateCommand AddTMProject { get; }
        public DelegateCommand RemoveProject { get; }
        public DelegateCommand CopyProject { get; }
        public DelegateCommand AddChild { get; }
        public DelegateCommand RemoveChild { get; }
        public DelegateCommand CopyChild { get; }

        private GanttTreeViewItemInfo _selectedRootItem;

        public GanttTreeViewItemInfo SelectedRootItem
        {
            get => _selectedRootItem;
            set
            {
                if (SelectedRootItem != null && SelectedRootItem is GanttTreeViewItemInfo ganttItemInfo)
                {
                    ganttItemInfo.Id.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedRootItem, value);
                
                if (SelectedRootItem != null && value is GanttTreeViewItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.Id.PropertyChanged += GanttItemInfoOnPropertyChanged;
                    RefreshChildCollection(value);
                }

                if (SelectedRootItem.Id.GlobalTask)
                {
                    ContextMenuItemName = "Изменить тип на локальный";
                }
                else
                {
                    ContextMenuItemName = "Изменить тип на глобальный";
                }
            }
        }
        
        private ObservableCollection<GanttTreeViewItemInfo> _selectedRootItems;

        public ObservableCollection<GanttTreeViewItemInfo> SelectedRootItems
        {
            get => _selectedRootItems;
            set => base.SetProperty(ref _selectedRootItems, value);
        }
        
        private GanttTreeViewItemInfo _selectedChildItem;

        public GanttTreeViewItemInfo SelectedChildItem
        {
            get => _selectedChildItem;
            set
            {
                if (SelectedChildItem != null && SelectedChildItem is GanttTreeViewItemInfo ganttItemInfo)
                {
                    ganttItemInfo.Id.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedChildItem, value);
                
                if (SelectedChildItem != null && value is GanttTreeViewItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.Id.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (sender is GanttItemInfo gantt && gantt.ParentId == null)
            {
                if (SelectedRootItem is GanttTreeViewItemInfo ganttItemInfo)
                {
                    _projectsLibraryService.UpdateGanttObject(ganttItemInfo.Id, e.PropertyName);
                }
            }
            else
            {
                if (SelectedChildItem is GanttTreeViewItemInfo ganttItemInfo)
                {
                    _projectsLibraryService.UpdateGanttObject(ganttItemInfo.Id, e.PropertyName);
                }
            }
        }
        
        private ObservableCollection<GanttTreeViewItemInfo> _selectedChildrenItems;

        public ObservableCollection<GanttTreeViewItemInfo> SelectedChildrenItems
        {
            get => _selectedChildrenItems;
            set => base.SetProperty(ref _selectedChildrenItems, value);
        }

        private ObservableCollection<GanttTreeViewItemInfo> _rootGanttItems;
        public ObservableCollection<GanttTreeViewItemInfo> RootGanttItems
        {
            get => _rootGanttItems;
            set => base.SetProperty(ref _rootGanttItems, value);
        }

        public ObservableCollection<GanttTreeViewItemInfo> _childGanttItems;

        public ObservableCollection<GanttTreeViewItemInfo> ChildGanttItems
        {
            get => _childGanttItems;
            set => base.SetProperty(ref _childGanttItems, value);
        }

        private void AddNewProject()
        {
            _projectsLibraryService.AddGanttObject("Новый проект");
        }

        private void RemoveSelectedProjects()
        {
            _projectsLibraryService.RemoveGanttObject(SelectedRootItems);
        }

        private void CopySelectedProjects()
        {
            _projectsLibraryService.CopyGanttObject(SelectedRootItems);
            
        }

        private void AddChildrenItem()
        {
            _projectsLibraryService.AddGanttChildObject(SelectedRootItem,"Новый этап");

            RefreshChildCollection(SelectedRootItem);
        }

        private void RemoveChildItem()
        {
            _projectsLibraryService.RemoveGanttObject(SelectedChildrenItems);

            RefreshChildCollection(SelectedRootItem);
        }

        private void CopyChildrenItems()
        {
            _projectsLibraryService.CopyGanttObject(SelectedChildrenItems);

            RefreshChildCollection(SelectedRootItem);
        }
        
        private DelegateCommand<string> _commandChangePropertyGlobalTask;
        
        public DelegateCommand<string> CommandChangePropertyGlobalTask =>
            _commandChangePropertyGlobalTask ?? (_commandChangePropertyGlobalTask = new DelegateCommand<string>(ChangePropertyGlobalTask));

        private void ChangePropertyGlobalTask(string obj)
        {
            if (SelectedRootItem.Id.GlobalTask)
            {
                SelectedRootItem.Id.GlobalTask = false;
            }
            else
            {
                SelectedRootItem.Id.GlobalTask = true;
            }
            
           // _projectsLibraryService.UpdateGanttObject(SelectedRootItem, "GlobalTask");
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }

        public String Title => "Библиотека проектов";
        
        public event Action<IDialogResult> RequestClose;

        public TMProjectsLibraryViewModel()
        {
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();

            SelectedRootItems = new ObservableCollection<GanttTreeViewItemInfo>();

            RootGanttItems = _projectsLibraryService.ProjectsLibrary.RootItemsProjectsLibrary;
            
            ChildGanttItems = new ObservableCollection<GanttTreeViewItemInfo>();

            SelectedChildrenItems = new ObservableCollection<GanttTreeViewItemInfo>();

            AddTMProject = new DelegateCommand(AddNewProject);

            RemoveProject = new DelegateCommand(RemoveSelectedProjects);

            CopyProject = new DelegateCommand(CopySelectedProjects);

            AddChild = new DelegateCommand(AddChildrenItem);

            RemoveChild = new DelegateCommand(RemoveChildItem);

            CopyChild = new DelegateCommand(CopyChildrenItems);

            CurrentUser = _usersLibraryService.UsersLibrary.CurrentUser;

            if (CurrentUser != null)
            {
                SelectedRootItem = RootGanttItems.FirstOrDefault();

                CanModify = _usersLibraryService.UsersLibrary.PositionsInfoItems.
                    FirstOrDefault(t => t.Id == CurrentUser.PositionId).CanModify;
            }
        }

        private void RefreshChildCollection(GanttTreeViewItemInfo selectedRootItem)
        {
            ChildGanttItems = new ObservableCollection<GanttTreeViewItemInfo>(_projectsLibraryService.ProjectsLibrary.AllGanttItems.Where(t =>
                    t.ParentId != null && t.ParentId.Id is Int32 intParentId && selectedRootItem.Id.Id is Int32 intSelectedItemId && intParentId == intSelectedItemId));
        }
    }
}