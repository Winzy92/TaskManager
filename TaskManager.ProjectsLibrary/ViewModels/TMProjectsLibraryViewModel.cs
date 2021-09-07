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

        private GanttItemInfo _selectedRootItem;

        public GanttItemInfo SelectedRootItem
        {
            get => _selectedRootItem;
            set
            {
                if (SelectedRootItem != null && SelectedRootItem is GanttItemInfo ganttItemInfo)
                {
                    ganttItemInfo.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedRootItem, value);
                
                if (SelectedRootItem != null && value is GanttItemInfo ganttItemInfoItem)
                {
                    RefreshChildList(value);
                    ganttItemInfoItem.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }

                if (SelectedRootItem.GlobalTask)
                {
                    ContextMenuItemName = "Изменить тип на локальный";
                }
                else
                {
                    ContextMenuItemName = "Изменить тип на глобальный";
                }
            }
        }
        
        private ObservableCollection<GanttItemInfo> _selectedRootItems;

        public ObservableCollection<GanttItemInfo> SelectedRootItems
        {
            get => _selectedRootItems;
            set => base.SetProperty(ref _selectedRootItems, value);
        }
        
        private GanttItemInfo _selectedChildItem;

        public GanttItemInfo SelectedChildItem
        {
            get => _selectedChildItem;
            set
            {
                if (SelectedChildItem != null && SelectedChildItem is GanttItemInfo ganttItemInfo)
                {
                    ganttItemInfo.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                }
                
                base.SetProperty(ref _selectedChildItem, value);
                
                if (SelectedChildItem != null && value is GanttItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.PropertyChanged += GanttItemInfoOnPropertyChanged;
                }
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (sender is GanttItemInfo gantt && (Int32)gantt.ParentId == 0)
            {
                if (SelectedRootItem is GanttItemInfo ganttItemInfo)
                {
                    _projectsLibraryService.UpdateGanttObject(ganttItemInfo, e.PropertyName);
                }
            }
            else
            {
                if (SelectedChildItem is GanttItemInfo ganttItemInfo)
                {
                    _projectsLibraryService.UpdateGanttObject(ganttItemInfo, e.PropertyName);
                }
            }
        }
        
        private ObservableCollection<GanttItemInfo> _selectedChildrenItems;

        public ObservableCollection<GanttItemInfo> SelectedChildrenItems
        {
            get => _selectedChildrenItems;
            set => base.SetProperty(ref _selectedChildrenItems, value);
        }
        
        public ObservableCollection<GanttItemInfo> Tasks { get; set; }
        

        private ObservableCollection<GanttItemInfo> _rootGanttItems;
        public ObservableCollection<GanttItemInfo> RootGanttItems
        {
            get => _rootGanttItems;
            set => base.SetProperty(ref _rootGanttItems, value);
        }

        public ObservableCollection<GanttItemInfo> _childGanttItems;

        public ObservableCollection<GanttItemInfo> ChildGanttItems
        {
            get => _childGanttItems;
            set => base.SetProperty(ref _childGanttItems, value);
        }

        private void AddNewProject()
        {
            _projectsLibraryService.AddGanttObject("Новый проект");

            /*CreateRootCollection(new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList()));*/
        }

        private void RemoveSelectedProjects()
        {
            _projectsLibraryService.RemoveGanttObject(SelectedRootItems);
            
            /*CreateRootCollection(new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList()));*/
        }

        private void CopySelectedProjects()
        {
            _projectsLibraryService.CopyGanttObject(SelectedRootItems);
            
            /*CreateRootCollection(new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList()));*/
        }

        private void AddChildrenItem()
        {
            _projectsLibraryService.AddGanttChildObject(SelectedRootItem,"Новый этап");
            
            /*CreateRootCollection(new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList()));*/

            RefreshChildList(SelectedRootItem);
        }

        private void RemoveChildItem()
        {
            _projectsLibraryService.RemoveGanttObject(SelectedChildrenItems);
            
            /*CreateRootCollection(new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList()));*/
            
            RefreshChildList(SelectedRootItem);
        }

        private void CopyChildrenItems()
        {
            _projectsLibraryService.CopyGanttObject(SelectedChildrenItems);
            
            /*CreateRootCollection(new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList()));*/
            
            RefreshChildList(SelectedRootItem);
        }
        
        private DelegateCommand<string> _commandChangePropertyGlobalTask;
        
        public DelegateCommand<string> CommandChangePropertyGlobalTask =>
            _commandChangePropertyGlobalTask ?? (_commandChangePropertyGlobalTask = new DelegateCommand<string>(ChangePropertyGlobalTask));

        private void ChangePropertyGlobalTask(string obj)
        {
            if (SelectedRootItem.GlobalTask)
            {
                SelectedRootItem.GlobalTask = false;
            }
            else
            {
                SelectedRootItem.GlobalTask = true;
            }
            
            _projectsLibraryService.UpdateGanttObject(SelectedRootItem, "GlobalTask");
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

            /*Tasks = new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems.ToList());*/
            
            SelectedRootItems = new ObservableCollection<GanttItemInfo>();

            RootGanttItems = _projectsLibraryService.ProjectsLibrary.RootItemsProjectsLibrary;
            
            ChildGanttItems = new ObservableCollection<GanttItemInfo>();

            SelectedChildrenItems = new ObservableCollection<GanttItemInfo>();

            AddTMProject = new DelegateCommand(AddNewProject);

            RemoveProject = new DelegateCommand(RemoveSelectedProjects);

            CopyProject = new DelegateCommand(CopySelectedProjects);

            AddChild = new DelegateCommand(AddChildrenItem);

            RemoveChild = new DelegateCommand(RemoveChildItem);

            CopyChild = new DelegateCommand(CopyChildrenItems);

           
            CurrentUser = _usersLibraryService.UsersLibrary.CurrentUser;

            if (CurrentUser != null)
            {
                /*CreateRootCollection(Tasks);*/
            
                SelectedRootItem = RootGanttItems.FirstOrDefault();

                RefreshChildList(SelectedRootItem);

                CanModify = _usersLibraryService.UsersLibrary.PositionsInfoItems.
                    FirstOrDefault(t => t.Id == CurrentUser.PositionId).CanModify;
            }
        }

        private void RefreshChildList(GanttItemInfo selectedRootItem)
        {
            var collection = new ObservableCollection<GanttItemInfo>(_projectsLibraryService.ProjectsLibrary.ChildItemsProjectsLibrary.Where(t =>
                t.ParentId is Int32 intParentId && selectedRootItem.Id is Int32 intSelectedItemId &&
                intParentId == intSelectedItemId));
            foreach (var item in collection)
            {
                item.Image = new BitmapImage(new Uri(@"/TaskManager.ProjectsLibrary;component/Multimedia/List.png",
                    UriKind.Relative));
            }

            ChildGanttItems = collection;
        }


        /*public void CreateRootCollection(ObservableCollection<GanttItemInfo> collection)
        {
            RootGanttItems = new ObservableCollection<GanttItemInfo>();

            foreach (var element in collection)
            {
                if (element.ParentId is Int32 pId && pId == 0)
                {
                    element.Image =
                        new BitmapImage(new Uri(@"/TaskManager.ProjectsLibrary;component/Multimedia/Folder.png",
                            UriKind.Relative));
                    RootGanttItems.Add(element);
                }
            }
        }*/
    }
}