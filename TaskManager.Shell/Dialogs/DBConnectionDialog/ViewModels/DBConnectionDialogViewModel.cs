using System;
using System.Linq;
using System.Windows;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Shell.Dialogs.DBConnectionDialog.ViewModels
{
    public class DBConnectionDialogViewModel : BindBase, IDialogAware
    {
        public String Title => "Подключение к базе данных";
        
        public DelegateCommand CommandOk { get; }
        
        public DelegateCommand CommandCancel { get; }
        
        private readonly IDialogService _dialogService;
        
        private readonly IUsersLibraryService _usersLibraryService;
        
        private readonly IDatabaseConnectionService _connectionService;
        
        private readonly ISettingsService _settingsService;
        
        private Boolean _showMessage;
        
        public Boolean ShowMessage
        {
            get => _showMessage;

            set
            {
                base.SetProperty(ref _showMessage, value);
            }
        }
        
        #region Параметры окна диалога подключения
        
        private DbConnectionInfo _connectionInfo;
        
        public DbConnectionInfo ConnectionInfo
        {
            get => _connectionInfo;
            set => base.SetProperty(ref _connectionInfo, value);
        }
        
        #endregion

        private void ConnectToDb()
        {
            TaskManagerServices.Instance.GetInstance<ISettingsService>().SaveSettings();
            TaskManagerServices.Instance.GetInstance<ISettingsService>().LoadSettings();
            try
            {
                TaskManagerServices.Instance.GetInstance<IUsersLibraryService>().LoadUsers();
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
            catch (Exception e)
            {
                ShowMessage = true;
            }
        }

        public event Action<IDialogResult> RequestClose;
        
        protected virtual void CloseDialog()
        {
            
        }
        
        public virtual void RaiseRequestClose(IDialogResult dialogResult)
        {
            RequestClose?.Invoke(dialogResult);
        }
        
        public virtual bool CanCloseDialog()
        {
            return true;
        }

        public virtual void OnDialogClosed()
        {
            if (_usersLibraryService.UsersLibrary.Users.Any())
            {
                var keyvalue = new DialogParameters();
                keyvalue.Add("key", 0);
                _dialogService.ShowDialog("UserAuthorizationDialog", keyvalue,  result => { });
            }
            else
            {
                var text = new DialogParameters();
                text.Add("message", "Ошибка подключения к базе данных."+"\n"+"Завершить работу с программой?");
                _dialogService.ShowDialog("MessageDialog", text,  result => { });
            }
        }

        public virtual void OnDialogOpened(IDialogParameters parameters)
        {
            ConnectionInfo = TaskManagerServices.Instance.GetInstance<ISettingsService>().Settings.DbConnectionInfo;
        }

        public DBConnectionDialogViewModel(IDialogService dialogService)
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();
            
            ShowMessage = false;
            
            _dialogService = dialogService;

            CommandOk = new DelegateCommand(ConnectToDb);
            
            CommandCancel = new DelegateCommand(CloseDialogCommand);
            
            _connectionService = TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>();
            
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
        }

        private void CloseDialogCommand()
        {
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
        }
    }
}