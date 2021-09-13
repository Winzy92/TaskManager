using System;
using System.CodeDom;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Windows.Media;
using DevExpress.Mvvm.Gantt;

namespace TaskManager.Sdk.Core.Models
{
    public class GanttItemInfo : GanttTask
    {
      
    
        private String _numOfContract;
        public String NumOfContract
        {
            get => _numOfContract;
            set
            {
                _numOfContract = value;
                RaisePropertiesChanged(nameof(NumOfContract));
            }
        }
        
        private ObservableCollection<UsersInfo> _usersInfos;
        public ObservableCollection<UsersInfo> UsersInfos
        {
            get => _usersInfos;
            set
            {
                _usersInfos = value;
                RaisePropertiesChanged(nameof(UsersInfos));
            }
        }

        private List<Object> _listUsers;
        public List<Object> ListUsers
        {
            get => _listUsers;
            set
            {
                _listUsers = value;
                RaisePropertiesChanged(nameof(ListUsers));
            }
        }

        private ObservableCollection<UsersInfo> _resourceUsers;
        public ObservableCollection<UsersInfo> ResourceUsers
        {
            get => _resourceUsers;
            set
            {
                _resourceUsers = value;
                RaisePropertiesChanged(nameof(ResourceUsers));
            }
        }

        private Boolean _globalTask;
        public Boolean GlobalTask
        {
            get => _globalTask;
            set
            {
                _globalTask = value;
                RaisePropertiesChanged(nameof(GlobalTask));
            }
        }
        
        private Boolean _isActive;
        public Boolean IsActive
        {
            get => _isActive;
            set
            {
                _isActive = value;
                RaisePropertiesChanged(nameof(IsActive));
            }
        }
        
        private Boolean _isArchive;
        public Boolean IsArchive
        {
            get => _isArchive;
            set
            {
                _isArchive = value;
                RaisePropertiesChanged(nameof(IsArchive));
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
        
        public GanttItemInfo()
        {
            Tag = null;
            ResourceUsers = new ObservableCollection<UsersInfo>();
            UsersInfos = new ObservableCollection<UsersInfo>();
            ListUsers = new List<Object>();
        }
    }
}