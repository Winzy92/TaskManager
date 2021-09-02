using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Interfaces
{
    public interface IDatabaseConnectionService
    {
        UsersInfo CurrentUser { get; set; }
        void LoadGanttObjects();

        void LoadUsersAdditionalGanttItems();

        void LoadUsersGanttItems();

        void LoadGanttResourceItems();
        
        void LoadTasksResourceItems();
        
        void LoadUsers();

        void AddUser(UsersInfo usersInfo);
        
        void AddGanttSourceItem(GanttResourceItemInfo ganttResourceItemInfo);

        void RemoveSelectedItemUserLibrary(Object selectedItem);

        void UpdatePropertiesSelectedItemUserLibrary(Object selectedItem, String property);
        
        void LoadUsersPositions();

        void LoadTasksUnits();

        void UpdateResourceLinks(GanttItemInfo ganttItemInfo);

        void RemoveAllUnits(GanttItemInfo ganttItemInfo);

        void AddGanttObject(String name);
        
        void AddGanttChildObject(GanttItemInfo selectedGanttItem, String name);

        void UpdateGanttObject(GanttItemInfo ganttItemInfo, String prop);

        void RemoveGanttObject(ObservableCollection<GanttItemInfo> selectedGanttItems);

        void CopyGanttObject(ObservableCollection<GanttItemInfo> selectedGanttItems);

        void AddResourceLink(IList ResourceLinks, GanttItemInfo ganttItemInfo);

        void RemoveResourceLink(IList ResourceLinks, GanttItemInfo ganttItemInfo);

        void UpdateTaskUnits(GanttItemInfo selectedGanttItem, String prop);
        
    }
}