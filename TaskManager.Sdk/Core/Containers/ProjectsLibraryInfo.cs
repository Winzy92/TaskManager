using System.Collections.ObjectModel;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Core.Containers
{
    public class ProjectsLibraryInfo
    {
        public ObservableCollection<GanttItemInfo> CurrentUserGanttItems { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<GanttItemInfo> CurrentUserAdditionalGanttItems { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<GanttTreeViewItemInfo> GanttItems { get; set; } = new ObservableCollection<GanttTreeViewItemInfo>();
        
        public ObservableCollection<GanttItemInfo> RootItemsProjectsLibrary { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<GanttItemInfo> ChildItemsProjectsLibrary { get; set; } = new ObservableCollection<GanttItemInfo>();
        
        public ObservableCollection<TaskResourceInfo> TaskResources { get; set; } = new ObservableCollection<TaskResourceInfo>();
        
        public ObservableCollection<TaskUnitInfo> GanttTasksUnits { get; set; } = new ObservableCollection<TaskUnitInfo>();
    }
}