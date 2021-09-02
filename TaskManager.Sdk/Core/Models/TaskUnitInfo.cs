using System;
using DevExpress.Mvvm;

namespace TaskManager.Sdk.Core.Models
{
    public class TaskUnitInfo : BindableBase
    {
        public Int32 Id { get; set; }
        
        public Int32 GanttItemId { get; set; }

        public Int32 UnitId { get; set; }

        public String UnitName { get; set; }

        public Int32 SourceId { get; set; }
        
        private DateTime? _startDate;
        public DateTime? StartDate
        {
            get => _startDate;
            set
            {
                _startDate = value;
                RaisePropertiesChanged(nameof(StartDate));
            }
        }
        
        private DateTime? _finishDate;
        public DateTime? FinishDate
        {
            get => _finishDate;
            set
            {
                _finishDate = value;
                RaisePropertiesChanged(nameof(FinishDate));
            }
        }
        
        private double _progress;
        public double Progress
        {
            get => _progress;
            set
            {
                _progress = value;
                RaisePropertiesChanged(nameof(Progress));
            }
        }
        
        private Boolean _isAdditional;
        public Boolean IsAdditional
        {
            get => _isAdditional;
            set
            {
                _isAdditional = value;
                RaisePropertiesChanged(nameof(IsAdditional));
            }
        }
    }
}