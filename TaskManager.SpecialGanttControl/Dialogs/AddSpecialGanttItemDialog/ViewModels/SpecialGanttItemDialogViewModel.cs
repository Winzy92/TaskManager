using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.SpecialGanttControl.Dialogs.AddSpecialGanttItemDialog.ViewModels
{
    public class SpecialGanttItemDialogViewModel : BindBase, IDialogAware
    {
        private readonly ISettingsService _settingsService;
        
        public DelegateCommand CommandOk { get; }

        public ObservableCollection<GanttItemInfo> LibraryTasks { get; set; }

        private ObservableCollection<GanttItemInfo> _selectedItems;
        
        public ObservableCollection<GanttItemInfo> SelectedItems 
        {
            get => _selectedItems;
            set
            {
                base.SetProperty(ref _selectedItems, value);
            }
        }

        private void SendSelectedItemsCollection()
        {
            if (SelectedItems != null)
            {
                var dParams = new DialogParameters();
                dParams.Add("SelectedItems", SelectedItems);
                RequestClose?.Invoke(new DialogResult(ButtonResult.OK, dParams));
            }
            else
            {
                Console.WriteLine(("Отсутствуют выделенные элементы"));
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
            
        }

        public String Title => "Текущие проекты и задачи";
        
        public event Action<IDialogResult> RequestClose;


        public SpecialGanttItemDialogViewModel()
        {
            _settingsService = TaskManagerServices.Instance.GetInstance<ISettingsService>();

            LibraryTasks = new ObservableCollection<GanttItemInfo>(_settingsService.Settings.GanttItems);
            
            SelectedItems = new ObservableCollection<GanttItemInfo>();

            CommandOk = new DelegateCommand(SendSelectedItemsCollection);
        }
    }
}