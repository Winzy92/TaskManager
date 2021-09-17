using System.Collections.ObjectModel;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Core.Containers
{
    public class ProjectsLibraryInfo
    {
        public ObservableCollection<GanttTreeViewItemInfo> CurrentUserGanttItems { get; set; } = new ObservableCollection<GanttTreeViewItemInfo>();
        
        public ObservableCollection<GanttTreeViewItemInfo> CurrentUserAdditionalGanttItems { get; set; } = new ObservableCollection<GanttTreeViewItemInfo>();
        
        public ObservableCollection<GanttTreeViewItemInfo> GanttItems { get; set; } = new ObservableCollection<GanttTreeViewItemInfo>();
        
        public ObservableCollection<GanttTreeViewItemInfo> RootItemsProjectsLibrary { get; set; } = new ObservableCollection<GanttTreeViewItemInfo>();

        public ObservableCollection<TaskResourceInfo> TaskResources { get; set; } = new ObservableCollection<TaskResourceInfo>();
        
        public ObservableCollection<TaskUnitInfo> GanttTasksUnits { get; set; } = new ObservableCollection<TaskUnitInfo>();
    }
}