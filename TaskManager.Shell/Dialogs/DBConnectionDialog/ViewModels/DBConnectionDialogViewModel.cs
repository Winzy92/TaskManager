using System;
using System.Windows;
using CommonServiceLocator;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Shell.Dialogs.DBConnectionDialog.ViewModels
{
    public class DBConnectionDialogViewModel : BindBase, IDialogAware
    {
        public String Title => "Подключение к базе данных";
        
        public DelegateCommand CommandOk { get; }
        
        public DelegateCommand CommandCancel { get; }
        
        private readonly IDialogService _dialogService;
        
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
            TaskManagerServices.Instance.GetInstance<IDatabaseConnectionService>().LoadUsers();

            if (_settingsService.Settings.IsConnected)
            {
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            }
            else
            {
                ShowMessage = true;
            }
        }

        public event Action<IDialogResult> RequestClose;
        
        //сделать в отдельной команде а не в стандартной
        protected virtual void CloseDialog()
        {
            Application.Current.Shutdown();
            RaiseRequestClose(new DialogResult(ButtonResult.Cancel));
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
            if (_settingsService.Settings.IsConnected)
            {
                var keyvalue = new DialogParameters();
                keyvalue.Add("key", 0);
                _dialogService.ShowDialog("UserAuthorizationDialog", keyvalue,  result => { });
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
            
            CommandCancel = new DelegateCommand(CloseDialog);
        }
    }
}