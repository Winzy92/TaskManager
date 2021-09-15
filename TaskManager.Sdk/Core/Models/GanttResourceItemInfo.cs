using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm;
using TaskManager.Sdk.Interfaces;

namespace TaskManager.Sdk.Core.Models
{
    public class GanttResourceItemInfo : BindableBase, IBaseUsersInfo
    {
        private String _name;
        public String Name
        {
            get => _name;
            set
            {
                _name = value;
                RaisePropertiesChanged(nameof(Name));
            }
        }
        public Int32 Id { get; set; }
    }
}