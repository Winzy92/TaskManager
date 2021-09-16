using System;
using System.Collections.ObjectModel;
using System.Windows.Media;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Gantt;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Core.Models
{
    public class GanttTreeViewItemInfo : BindableBase
    {
        public GanttItemInfo Id { get; set; }

        public GanttItemInfo ParentId { get; set; }
        
        private ObservableCollection<TaskResourceInfo> _resourceIds;
        public ObservableCollection<TaskResourceInfo> ResourceIds
        {
            get => _resourceIds;
            set
            {
                _resourceIds = value;
                RaisePropertiesChanged(nameof(ResourceIds));
            }
        }
        
        public ImageSource Image { get; set; }

        public GanttTreeViewItemInfo(GanttItemInfo ganttItemInfo)
        {
            Id = ganttItemInfo;
            ResourceIds = new ObservableCollection<TaskResourceInfo>();
        }
    }
}