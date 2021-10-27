using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using DevExpress.Xpf.Gantt;

namespace TaskManager.Sdk.Core.Models
{
    public class StripLineInfo : BindableBase
    {
        public Int32 Id { get; set; }
        public DateTime DateTime { get; set; }
        public TimeSpan StripLineDuration { get; set; }
        
        private ObservableCollection<TaskUnitInfo> _taskUnits;
        public ObservableCollection<TaskUnitInfo> TaskUnits
        {
            get => _taskUnits;
            set
            {
                _taskUnits = value;
                RaisePropertiesChanged(nameof(TaskUnits));
            }
        }
        
        private ObservableCollection<String> _usersWithCheckPoint;
        public ObservableCollection<String> UsersWithCheckPoint
        {
            get => _usersWithCheckPoint;
            set
            {
                _usersWithCheckPoint = value;
                RaisePropertiesChanged(nameof(UsersWithCheckPoint));
            }
        }

        public StripLineInfo()
        {
            TaskUnits = new ObservableCollection<TaskUnitInfo>();
            UsersWithCheckPoint = new ObservableCollection<String>();
        }
    }
}