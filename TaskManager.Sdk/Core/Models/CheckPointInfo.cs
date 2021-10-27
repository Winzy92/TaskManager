using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;

namespace TaskManager.Sdk.Core.Models
{
    public class CheckPointInfo : BindableBase
    {
        public Int32 Id { get; set; }
        public Int32 TaskUnitId { get; set; }
        public Int32 CheckPointYear { get; set; }
        public Int32 CheckPointMonth { get; set; }
        
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
    }
}