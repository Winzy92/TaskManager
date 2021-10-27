using System;
using DevExpress.Mvvm;

namespace TaskManager.Sdk.Core.Models
{
    public class TaskUnitInfo : BindableBase
    {
        public Int32 Id { get; set; }
        
        public Int32 GanttItemId { get; set; }

        public Int32 UnitId { get; set; }

        public Int32 SourceId { get; set; }

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