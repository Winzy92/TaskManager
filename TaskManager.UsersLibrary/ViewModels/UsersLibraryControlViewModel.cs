using System;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.ComponentModel;
using System.Linq;
using System.Windows.Media.Imaging;
using DevExpress.Xpf.Core.Native;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;
using Prism.Commands;
using TaskManager.UsersLibrary.Models;

namespace TaskManager.UsersLibrary.ViewModels
{
    public class UsersLibraryControlViewModel  : BindBase, IDialogAware
    {
        private readonly ISettingsService _settingsService;
        
        private readonly IDatabaseConnectionService _connectionService;
        
        public ObservableCollection<UsersInfo> UsersInfos { get; set; }
        
        public ObservableCollection<GanttResourceItemInfo> GanttResourceItems { get; set; }

        public ObservableCollection<PositionsInfo> PositionsInfos { get; set; }
        
        public DelegateCommand CommandSetNewPassword { get; }
            
        public DelegateCommand AddNewGanttSource { get; }
        
        public DelegateCommand AddNewUser { get; }
        
        public DelegateCommand RemoveObject { get; }
        
        
        private ObservableCollection<TreeListItemInfoUsersLibrary> _treeListItems;

        public ObservableCollection<TreeListItemInfoUsersLibrary> TreeListItems
        {
            get => _treeListItems;
            set
            {
                base.SetProperty(ref _treeListItems, value);
            }
        }

        private TreeListItemInfoUsersLibrary _selectedItem;
        
        public TreeListItemInfoUsersLibrary SelectedItem
        {
            get => _selectedItem;
            set
            {
                if (SelectedItem != null && SelectedItem is TreeListItemInfoUsersLibrary treeListItemInfo)
                {
                    if (treeListItemInfo.Entity != null && treeListItemInfo.Entity is UsersInfo usersInfo)
                    {
                        usersInfo.PropertyChanged -= TreeListItemInfoUsersLibraryOnPropertyChanged;
                    }
                    
                    if (treeListItemInfo.Entity != null && treeListItemInfo.Entity is GanttResourceItemInfo ganttResource)
                    {
                        ganttResource.PropertyChanged -= TreeListItemInfoUsersLibraryOnPropertyChanged;
                    }
                }
                
                base.SetProperty(ref _selectedItem, value);
                
                if (SelectedItem != null && value is TreeListItemInfoUsersLibrary treeListItem)
                {
                    if (treeListItem.Entity != null && treeListItem.Entity is UsersInfo user)
                    {
                        user.PropertyChanged += TreeListItemInfoUsersLibraryOnPropertyChanged;
                    }
                    
                    if (treeListItem.Entity != null && treeListItem.Entity is GanttResourceItemInfo ganttResourceItem)
                    {
                        ganttResourceItem.PropertyChanged += TreeListItemInfoUsersLibraryOnPropertyChanged;
                    }
                }
                
                PasswordFieldsClear();
                ShowMessage = false;
                UserMessage = "";

                IsShowItem = SelectedItem.Entity is UsersInfo;
            }
        }

        private void TreeListItemInfoUsersLibraryOnPropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            if (sender is UsersInfo usersInfo)
            {
                var item = TreeListItems.FirstOrDefault(t => t.Entity == usersInfo);
                if (item != null)
                {
                    _connectionService.UpdatePropertiesSelectedItemUserLibrary(item.Entity, e.PropertyName);
                }
            }

            if (sender is GanttResourceItemInfo ganttResource)
            {
                var item = TreeListItems.FirstOrDefault(t => t.Entity == ganttResource);
                if (item != null)
                {
                    _connectionService.UpdatePropertiesSelectedItemUserLibrary(item.Entity, e.PropertyName);
                }
            }
        }

        private String _firstPasswordField;
        
        public String FirstPasswordField
        {
            get => _firstPasswordField;

            set
            {
                base.SetProperty(ref _firstPasswordField, value);
            }
        }

        private String _secondPasswordField;
        
        public String SecondPasswordField
        {
            get => _secondPasswordField;

            set
            {
                base.SetProperty(ref _secondPasswordField, value);
            }
        }
        
        private String _userMessage;
        
        public String UserMessage
        {
            get => _userMessage;

            set
            {
                base.SetProperty(ref _userMessage, value);
            }
        }
        
        private Boolean _showMessage;
        
        public Boolean ShowMessage
        {
            get => _showMessage;

            set
            {
                base.SetProperty(ref _showMessage, value);
            }
        }
        
        private Boolean _isShowItem;
        
        public Boolean IsShowItem
        {
            get => _isShowItem;

            set
            {
                base.SetProperty(ref _isShowItem, value);
            }
        }
        
        private void PasswordFieldsClear()
        {
            FirstPasswordField = String.Empty;

            SecondPasswordField = String.Empty;
        }
        
        private void SetNewPassword()
        {
            if (FirstPasswordField != SecondPasswordField)
            {
                Console.WriteLine("Внимание! Пароли отличаются!");
                UserMessage = "Внимание! Пароли отличаются!";
                ShowMessage = true;
            }
            else
            {
                if (SelectedItem.Entity is UsersInfo usersInfo)
                {
                    usersInfo.Password = FirstPasswordField.GetHashCode().ToString();
                    UserMessage = "Пароль успешно изменён.";
                    ShowMessage = true;
                }
            }
        }

        private void AddNewGanttSourceItem()
        {
            GanttResourceItemInfo item = new GanttResourceItemInfo
            {
                Name = "Новый отдел"
            };
            _connectionService.AddGanttSourceItem(item);
            
            TreeListItemInfoUsersLibrary elem = new TreeListItemInfoUsersLibrary
            {
                ParentId = 0,
                Entity = item,
                Image = new BitmapImage(new Uri(@"/TaskManager.UsersLibrary;component/Multimedia/settings16x16.png", UriKind.Relative))
            };
            TreeListItems.Add(elem);
        }
        
        private void AddNewUserItem()
        {
            if (SelectedItem.Entity is GanttResourceItemInfo ganttResourceItemInfo)
            {
                var ganttsource = _settingsService.Settings.
                    GanttResourceItems.FirstOrDefault(t => t.Id == ganttResourceItemInfo.Id);
                
                if (ganttsource != null)
                {
                    UsersInfo user = new UsersInfo
                    {
                        Name = "Новый сотрудник",
                        GanttSourceItemId = ganttsource.Id,
                        PositionId = 1,
                        Password = ""
                    };
                    _connectionService.AddUser(user);
                    
                    TreeListItemInfoUsersLibrary elem = new TreeListItemInfoUsersLibrary
                    {
                        ParentId = SelectedItem,
                        Entity = user,
                        Image = new BitmapImage(new Uri(@"/TaskManager.UsersLibrary;component/Multimedia/user_icon.png", UriKind.Relative)) 
                    };
                    TreeListItems.Add(elem);
                }
            }

            if (SelectedItem.Entity is UsersInfo usersInfo)
            {
                var ganttsource = _settingsService.Settings.
                    Users.FirstOrDefault(t => t.Id == usersInfo.Id);
                
                if (ganttsource != null)
                {
                    UsersInfo user = new UsersInfo
                    {
                        Name = "Новый сотрудник",
                        GanttSourceItemId = ganttsource.GanttSourceItemId,
                        PositionId = 1,
                        Password = ""
                    };
                    _connectionService.AddUser(user);
                    
                    TreeListItemInfoUsersLibrary elem = new TreeListItemInfoUsersLibrary
                    {
                        ParentId = SelectedItem.ParentId,
                        Entity = user,
                        Image = new BitmapImage(new Uri(@"/TaskManager.UsersLibrary;component/Multimedia/user_icon.png", UriKind.Relative))
                    };
                    TreeListItems.Add(elem);
                }
            }
        }
        
        private void RemoveSelectedItem()
        {
            if (SelectedItem.Entity is GanttResourceItemInfo ganttResource)
            {
                var collection = TreeListItems.Where(t=>t.ParentId == SelectedItem);

                if (collection != null)
                {
                    foreach (var element in collection.ToList())
                    {
                        TreeListItems.Remove(element);
                    }
                }

                TreeListItems.Remove(SelectedItem);
                GanttResourceItemInfo item = new GanttResourceItemInfo
                {
                    Id = ganttResource.Id,
                    Name = ganttResource.Name
                };
                _connectionService.RemoveSelectedItemUserLibrary(item);
            }
            if (SelectedItem.Entity is UsersInfo usersInfo)
            {
                TreeListItems.Remove(SelectedItem);
                UsersInfo user = new UsersInfo
                {
                    Id = usersInfo.Id,
                    Name = usersInfo.Name,
                    PositionId = usersInfo.PositionId,
                    GanttSourceItemId = usersInfo.GanttSourceItemId
                };
                _connectionService.RemoveSelectedItemUserLibrary(user);
            }
        }

        public void CreateTreeListCollection()
        {
            foreach (var item in GanttResourceItems)
            {
                TreeListItemInfoUsersLibrary elem = new TreeListItemInfoUsersLibrary
                {
                    ParentId = 0,
                    Entity = item,
                    Image = new BitmapImage(new Uri(@"/TaskManager.UsersLibrary;component/Multimedia/settings16x16.png", UriKind.Relative))
                };

                TreeListItems.Add(elem);
                
                var collection = UsersInfos.Where(t => t.GanttSourceItemId == item.Id);

                if (collection != null && collection.Count() != 0)
                {
                    
                    foreach (var element in collection)
                    {
                        var position = PositionsInfos.FirstOrDefault(t => t.Id == element.PositionId);
                        TreeListItems.Add(new TreeListItemInfoUsersLibrary()
                        {
                            ParentId = elem,
                            Entity = element,
                            Image = new BitmapImage(new Uri(@"/TaskManager.UsersLibrary;component/Multimedia/user_icon.png", UriKind.Relative))
                        });
                    }
                }
            }
        }

        private void TreeListItemsCollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            
        }

        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
            
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            
        }

        public String Title => "Библиотека пользователей";
        
        public event Action<IDialogResult> RequestClose;

        public UsersLibraryControlViewModel()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();
            
            GanttResourceItems = _settingsService.Settings.GanttResourceItems;
            
            TreeListItems = new ObservableCollection<TreeListItemInfoUsersLibrary>();
            
            TreeListItems.CollectionChanged += TreeListItemsCollectionChanged;

            UsersInfos = _settingsService.Settings.Users;

            PositionsInfos = _settingsService.Settings.PositionsInfoItems;

            CreateTreeListCollection();

            CommandSetNewPassword = new DelegateCommand(SetNewPassword);
            
            AddNewGanttSource = new DelegateCommand(AddNewGanttSourceItem);
            
            AddNewUser = new DelegateCommand(AddNewUserItem);
            
            RemoveObject = new DelegateCommand(RemoveSelectedItem);

            IsShowItem = false;
        }
    }
}