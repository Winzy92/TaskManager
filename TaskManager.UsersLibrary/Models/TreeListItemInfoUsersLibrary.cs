using System;
using System.Windows.Media;
using DevExpress.Mvvm;
using DevExpress.Xpf.Editors.Internal;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;

namespace TaskManager.UsersLibrary.Models
{
    public class TreeListItemInfoUsersLibrary : BindableBase
    {
        public ImageSource Image { get; set; }
        
        public Object Id => this;

        public Object ParentId { get; set; }

        public String Name => GetName();

        private string GetName()
        {
            var str = "";
            
            if (Entity is GanttResourceItemInfo ganttResourceItemInfo)
            {
                str = ganttResourceItemInfo.Name;
            }

            if (Entity is UsersInfo usersInfo)
            {
                str = usersInfo.Name;
            }

            return str;
        }

        private Boolean _isExpanded;

        public Boolean IsExpanded
        {
            get => _isExpanded;
            set
            {
                _isExpanded = value;
                RaisePropertiesChanged(nameof(IsExpanded));
            }
        }

        private IBaseUsersInfo _entity;

        public IBaseUsersInfo Entity
        {
            get => _entity;
            set
            {
                _entity = value;
                RaisePropertiesChanged(nameof(Entity));
            }
        }

        public TreeListItemInfoUsersLibrary()
        {
            IsExpanded = true;
        }
    }
}