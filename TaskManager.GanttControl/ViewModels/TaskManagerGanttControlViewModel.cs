﻿using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows;
using DevExpress.Mvvm.POCO;
using DevExpress.Xpf.Gantt;
using DevExpress.Xpf.Grid.TreeList;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.GanttControl.ViewModels
{
    public class TaskManagerGanttControlViewModel : BindBase
    {
        private readonly IUsersLibraryService _usersLibraryService;
        
        private readonly IProjectsLibraryService _projectsLibraryService;
        public ObservableCollection<GanttTreeViewItemInfo> Tasks { get; set; }
        
        public Boolean CanModify { get; set; }

        public UsersInfo CurrentUser { get; set; }

        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; }
        
        public ObservableCollection<TaskResourceInfo> TaskResources { get; set; }

        private GanttTreeViewItemInfo _selectedItem;

        public GanttTreeViewItemInfo SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SelectedItem != null && SelectedItem is GanttTreeViewItemInfo ganttItemInfo)
                {
                    ganttItemInfo.Id.PropertyChanged -= GanttItemInfoOnPropertyChanged;
                    ganttItemInfo.ResourceIds.CollectionChanged -= ResourceIdsCollectionChanged;
                    //TaskManagerServices.Instance.EventAggregator.GetEvent<UpdateMainGanttEvent>().Publish();
                }
                
                base.SetProperty(ref _selectedItem, value);
                
                if (SelectedItem != null && value is GanttTreeViewItemInfo ganttItemInfoItem)
                {
                    ganttItemInfoItem.Id.PropertyChanged += GanttItemInfoOnPropertyChanged;
                    ganttItemInfoItem.ResourceIds.CollectionChanged += ResourceIdsCollectionChanged;
                    TaskManagerServices.Instance.EventAggregator.GetEvent<TaskResourceInfoUpdateEvent>().Subscribe(UpdateTaskResource);
                }
            }
        }

        private void UpdateTaskResource(TaskResourceInfo taskResourceInfo)
        {
            _projectsLibraryService.UpdateTaskResources(taskResourceInfo);
        }

        private void ResourceIdsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            foreach (var element in e.NewItems)
            {
                if (SelectedItem is GanttTreeViewItemInfo ganttItemInfo && element is TaskResourceInfo taskResourceInfo)
                {
                    if (!(taskResourceInfo.Id == 0 && taskResourceInfo.TaskId == 0 && taskResourceInfo.GanttSourceId == 0))
                    {
                        switch (e.Action)
                        {
                            case NotifyCollectionChangedAction.Add:
                                _projectsLibraryService.AddResourceLink(e.NewItems, ganttItemInfo);
                                _projectsLibraryService.UpdateResourceLinks(ganttItemInfo);
                                break;
                    
                            case NotifyCollectionChangedAction.Remove:
                                _projectsLibraryService.RemoveResourceLink(e.OldItems, ganttItemInfo);
                                _projectsLibraryService.UpdateResourceLinks(ganttItemInfo);
                                break;
                        }
                    }
                }
            }
        }
        
        private void GanttItemInfoOnPropertyChanged(Object sender, PropertyChangedEventArgs e)
        {
            if (SelectedItem is GanttTreeViewItemInfo ganttItemInfo)
            {
                _projectsLibraryService.UpdateGanttObject(ganttItemInfo.Id, e.PropertyName);
            }
        }

        public TaskManagerGanttControlViewModel()
        {
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();

            Tasks = _projectsLibraryService.ProjectsLibrary.GanttItems;

            GanttResourceItems = _usersLibraryService.UsersLibrary.GanttResourceItems;

            TaskResources = _projectsLibraryService.ProjectsLibrary.TaskResources;
            
            CurrentUser = _usersLibraryService.UsersLibrary.CurrentUser;

            if (CurrentUser != null)
            {
                CanModify = _usersLibraryService.UsersLibrary.PositionsInfoItems.
                    FirstOrDefault(t => t.Id == CurrentUser.PositionId).CanModify;
            }
        }
    }
}