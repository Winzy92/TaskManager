using System;
using System.Windows.Media;
using DevExpress.Mvvm;
using TaskManager.Sdk.Interfaces;

namespace TaskManager.Sdk.Core.Models
{
    public class UsersInfo : BindableBase, IBaseUsersInfo
    {

        public Int32 Id { get; set; }
        
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

        public Int32 GanttSourceItemId { get; set; }

        private String _password;
        public String Password
        {
            get => _password;
            set
            {
                _password = value;
                RaisePropertiesChanged(nameof(Password));
            }
        }

        private Int32 _positionId;
        public Int32 PositionId
        {
            get => _positionId;
            set
            {
                _positionId = value;
                RaisePropertiesChanged(nameof(PositionId));
            }
        }
        
    }
}