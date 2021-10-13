using System.Collections.ObjectModel;
using DevExpress.Mvvm.Gantt;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Core.Containers
{
    public class UsersLibraryInfo
    {
        public UsersInfo CurrentUser { get; set; }
        
        public ObservableCollection<PositionsInfo> PositionsInfoItems { get; set; } = new ObservableCollection<PositionsInfo>();
        
        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; } = new ObservableCollection<GanttResourceItemInfo>();
        
        public ObservableCollection<UsersInfo> Users { get; set; } = new ObservableCollection<UsersInfo>();
        
        
    }
}