using System;
using DevExpress.Mvvm;

namespace TaskManager.Sdk.Core.Models
{
    public class TaskResourceInfo : BindableBase
    {
        public Int32 Id { get; set; }

        public Int32 TaskId { get; set; }

        public Int32 GanttSourceId { get; set; }

        private Double _percent;
        public Double Percent
        {
            get => _percent;
            set
            {
                _percent = value;
                RaisePropertiesChanged(nameof(Percent));
            }
        }
    }
}