using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaskManager.Sdk.Core.Containers;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Interfaces.ProjectsLibrary
{
    public interface IProjectsLibraryService
    {
        ProjectsLibraryInfo ProjectsLibrary { get; set; }
        
        void LoadGanttObjects();
        
        void LoadUsersAdditionalGanttItems();

        void LoadTasksResourceItems();

        void LoadTasksUnits();
        
        void UpdateResourceLinks(GanttTreeViewItemInfo ganttItemInfo);
        
        void RemoveAllUnits(GanttItemInfo ganttItemInfo);
        
        void AddGanttObject(String name);
                
        void AddGanttChildObject(GanttTreeViewItemInfo selectedGanttItem, String name);
        
        void UpdateGanttObject(GanttItemInfo ganttItemInfo, String prop);
        
        void RemoveGanttObject(ObservableCollection<GanttTreeViewItemInfo> selectedGanttItems);
        
        void CopyGanttObject(ObservableCollection<GanttItemInfo> selectedGanttItems);
        
        void AddResourceLink(IList ResourceLinks, GanttTreeViewItemInfo ganttItemInfo);
        
        void RemoveResourceLink(IList ResourceLinks, GanttTreeViewItemInfo ganttItemInfo);
        
        void UpdateTaskUnits(GanttItemInfo selectedGanttItem, String prop);

        void UpdateTaskResources(TaskResourceInfo taskResourceInfo);

    }
}