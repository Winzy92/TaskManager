using System;
using DevExpress.Mvvm;
using DevExpress.Mvvm.Gantt;
using TaskManager.Sdk.Events;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Core.Models
{
    public class TaskResourceInfo : GanttResourceLink
    {
        public Int32 Id { get; set; }

        public Int32 TaskId { get; set; }

    }
}