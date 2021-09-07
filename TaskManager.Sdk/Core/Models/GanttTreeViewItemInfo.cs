using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace TaskManager.Sdk.Core.Models
{
    public class GanttTreeViewItemInfo : BindableBase
    {
        public GanttItemInfo Id { get; set; }

        public GanttItemInfo ParentId { get; set; }
        
        private ObservableCollection<Int32> _resourceIds;
        public ObservableCollection<Int32> ResourceIds
        {
            get => _resourceIds;
            set
            {
                _resourceIds = value;
                RaisePropertiesChanged(nameof(ResourceIds));
            }
        }

        public GanttTreeViewItemInfo(GanttItemInfo ganttItemInfo)
        {
            Id = ganttItemInfo;
            ResourceIds = new ObservableCollection<int>();
        }
    }
}