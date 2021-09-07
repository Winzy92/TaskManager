using System;
using System.Net.Mime;
using System.Windows;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Shell.Dialogs.MessageDialog.ViewModels
{
    public class MessageDialogViewModel : BindBase, IDialogAware
    {
        public String Title => "Системное сообщение:";

        private readonly IDialogService _dialogService;
        public DelegateCommand CommandOk { get; }
        
        public DelegateCommand CommandCancel { get; }
        
        private String _message;
        
        public String Message
        {
            get => _message;

            set
            {
                base.SetProperty(ref _message, value);
            }
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
            Message = parameters.GetValue<String>("message");
        }
        
        public event Action<IDialogResult> RequestClose;

        public MessageDialogViewModel(IDialogService dialogService)
        {
            CommandOk = new DelegateCommand(ShutDownApp);
            
            _dialogService = dialogService;
            
            CommandCancel = new DelegateCommand(ReturnToConnectionDbDialog);
        }

        private void ReturnToConnectionDbDialog()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.Cancel));
            _dialogService.ShowDialog("DBConnectDialog", new DialogParameters (),  result => { });
        }

        private void ShutDownApp()
        {
            RequestClose?.Invoke(new DialogResult(ButtonResult.OK));
            Application.Current.Shutdown();
        }
    }
}