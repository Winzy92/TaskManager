using System;
using System.Collections.ObjectModel;
using System.Linq;
using Prism.Commands;
using Prism.Services.Dialogs;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.SpecialGanttControl.Dialogs.AddSpecialGanttItemDialog.ViewModels
{
    public class SpecialGanttItemDialogViewModel : BindBase, IDialogAware
    {
        private readonly IProjectsLibraryService _projectsLibraryService;
        
        public DelegateCommand CommandOk { get; }

        public ObservableCollection<GanttTreeViewItemInfo> LibraryTasks { get; set; }

        private ObservableCollection<GanttTreeViewItemInfo> _selectedItems;
        
        public ObservableCollection<GanttTreeViewItemInfo> SelectedItems 
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
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();

            LibraryTasks = new ObservableCollection<GanttTreeViewItemInfo>(_projectsLibraryService.ProjectsLibrary.AllGanttItems);
            
            SelectedItems = new ObservableCollection<GanttTreeViewItemInfo>();

            CommandOk = new DelegateCommand(SendSelectedItemsCollection);
        }
    }
}