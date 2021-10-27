using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Shell.Dialogs.UserAuthorizationDialog.ViewModels
{
    public class UserAuthorizationDialogViewModel : BindBase, IDialogAware
    {
        public String Title => "Авторизация пользователя";
        
        public DelegateCommand CommandOk { get; }
        
        public DelegateCommand CommandCancel { get; }
        
        private readonly IUsersLibraryService _usersLibraryService;

        private readonly IDialogService _dialogService;
        
        private String _errorMessage;

        private Int32 KeyValue { get; set; }

        public String ErrorMessage
        {
            get => _errorMessage;

            set
            {
                base.SetProperty(ref _errorMessage, value);
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

        private UsersInfo _selectedUser;
        
        public UsersInfo SelectedUser
        {
            get => _selectedUser;
            set
            {
                base.SetProperty(ref _selectedUser, value);
            }
        }

        private String _passwordField;
        
        public String PasswordField
        {
            get => _passwordField;
            set
            {
                base.SetProperty(ref _passwordField, value);
            }
        }

        private void UserAuthorization()
        {
            if (SelectedUser.Password == PasswordField.GetHashCode().ToString())
            {
                if (KeyValue == 0)
                {
                    _usersLibraryService.UsersLibrary.CurrentUser = SelectedUser;
                    TaskManagerServices.Instance.GetInstance<IUsersLibraryService>().LoadUsersPositions();
                    TaskManagerServices.Instance.GetInstance<IUsersLibraryService>().LoadGanttResourceItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadGanttObjects();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadTasksResourceItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadTasksUnits();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadUsersAdditionalGanttItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadCheckPointItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().CreateStriplinesCollection();
                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                }
                
                if (KeyValue == 1 && _usersLibraryService.UsersLibrary.CurrentUser != null)
                {
                    _usersLibraryService.UsersLibrary.CurrentUser = SelectedUser;
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadUsersAdditionalGanttItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadCheckPointItems();
                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                }

                if (KeyValue == 1 && _usersLibraryService.UsersLibrary.CurrentUser == null)
                {
                    _usersLibraryService.UsersLibrary.CurrentUser = SelectedUser;
                    TaskManagerServices.Instance.GetInstance<IUsersLibraryService>().LoadUsersPositions();
                    TaskManagerServices.Instance.GetInstance<IUsersLibraryService>().LoadGanttResourceItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadGanttObjects();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadTasksResourceItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadTasksUnits();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadUsersAdditionalGanttItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().LoadCheckPointItems();
                    TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>().CreateStriplinesCollection();
                    RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
                }

            }
            else
            {
                ShowMessage = true;
            }
        }

        public ObservableCollection<UsersInfo> Users { get; set; }
        
        public bool CanCloseDialog()
        {
            return true;
        }

        public void OnDialogClosed()
        {
           
        }

        public void OnDialogOpened(IDialogParameters parameters)
        {
            KeyValue = parameters.GetValue<Int32>("key");
        }
        
        protected virtual void CloseDialog()
        {
            
        }
        
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
        
        public event Action<IDialogResult> RequestClose;

        public UserAuthorizationDialogViewModel()
        {
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            
            Users = _usersLibraryService.UsersLibrary.Users;

            ShowMessage = false;

            ErrorMessage = "Ошибка в пароле";

            SelectedUser = Users.FirstOrDefault();
            
            CommandOk = new DelegateCommand(UserAuthorization);
            
            CommandCancel = new DelegateCommand(CloseDialogCommand);
        }

        private void CloseDialogCommand()
        {
            if (_usersLibraryService.UsersLibrary.CurrentUser == null)
            {
                RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
            }
        }
    }
}