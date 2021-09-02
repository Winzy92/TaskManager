using System;
using System.Collections.ObjectModel;

namespace TaskManager.Sdk.Core.Models
{
    public class SettingsInfo
    {
        public ObservableCollection<GanttItemInfo> GanttItems { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<GanttItemInfo> ActiveTasks { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<GanttItemInfo> CurrentUserGanttItems { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<GanttItemInfo> CurrentUserAdditionalGanttItems { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public DbConnectionInfo DbConnectionInfo { get; set; }
        
        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; } = new ObservableCollection<GanttResourceItemInfo>();
        
        public ObservableCollection<TaskResourceInfo> TaskResources { get; set; } = new ObservableCollection<TaskResourceInfo>();
        
        public ObservableCollection<PositionsInfo> PositionsInfoItems { get; set; } = new ObservableCollection<PositionsInfo>();
        
        public ObservableCollection<UsersInfo> Users { get; set; } = new ObservableCollection<UsersInfo>();
        
        public ObservableCollection<TaskUnitInfo> GanttTasksUnits { get; set; } = new ObservableCollection<TaskUnitInfo>();

        public UsersInfo CurrentUser { get; set; }
        
        public Boolean IsConnected { get; set; }
        
    }
}