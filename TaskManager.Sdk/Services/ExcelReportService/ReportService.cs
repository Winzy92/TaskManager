using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.IO;
using System.Linq;
using DevExpress.Mvvm.Native;
using NPOI.SS.UserModel;
using NPOI.XSSF.UserModel;
using TaskManager.Sdk.Core.Containers;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.ProjectsLibrary;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.Sdk.Services.ExcelReportService
{
    public class ReportService : IReportService
    {
        public Dictionary<Int32, String> StringValues;
        
        
        public ReportInfo Report { get; set; } = new ReportInfo();

        private Int32 _rowNums;

        private Int32 _rootItemIndex;

        private Int32 _childItemIndex;

        private Boolean _timeToWriteChildrens;

        private Boolean _timeToWriteAdditionalChildrens;

        private XSSFWorkbook wb { get; set; }

        private readonly IUsersLibraryService _usersLibraryService;

        private Int32 _currentRow;
        
        private Int32 _maxCells;

        private readonly IProjectsLibraryService _projectsLibraryService;

        private List<GanttTreeViewItemInfo> UserRootCollection { get; set; }

        private List<GanttTreeViewItemInfo> UserAdditionalRootCollection { get; set; }

        public String SheetName { get; set; }

        public ISheet Sheet { get; set; }

        public string ReportPath { get; set; }
        
        public string SavePath { get; set; }
        
        public void CreateMonthReport(UsersInfo CurrentUser)
        {
            StringValues.Clear();

            using (var streem = new FileStream(ReportPath, FileMode.Open, FileAccess.Read))
            {
                wb = new XSSFWorkbook(streem);
                SheetName = wb.GetSheetAt(0).SheetName;
                Sheet = (XSSFSheet) wb.GetSheet(SheetName);
            }

            if (Sheet != null)
            {
                _rowNums = Sheet.LastRowNum;
                _currentRow = Sheet.FirstRowNum;
                

                while (_currentRow <= _rowNums)
                {
                    var row = Sheet.GetRow(_currentRow);
                    
                    if (row == null)
                    {
                        _currentRow++;

                        continue;
                    }

                    _maxCells = Math.Max(_maxCells, row.LastCellNum);

                    for (var i = 0; i <= _maxCells; i++)
                    {
                        var cell = row.GetCell(i, MissingCellPolicy.CREATE_NULL_AS_BLANK);

                        if (cell.CellType == CellType.String)
                        {
                            switch (cell.StringCellValue)
                            {
                                case "User.Source":
                                    cell.SetCellValue(_usersLibraryService.UsersLibrary.GanttResourceItems
                                        .FirstOrDefault(t =>
                                            (Int32) t.Id == CurrentUser.GanttSourceItemId).Name);
                                    break;

                                case "Report.DateTime":
                                    cell.SetCellValue(DateTime.Now.ToString("MM-yyy"));
                                    break;

                                case "User.Name":
                                    cell.SetCellValue(CurrentUser.Name);
                                    break;

                                case "Task.Root":
                                    StringValues.Add(cell.ColumnIndex, "Name");
                                    var element = UserRootCollection.FirstOrDefault();
                                    if (element != null)
                                    {
                                        _rootItemIndex = UserRootCollection.IndexOf(element);
                                        cell.SetCellValue(element.Id.Name);
                                    }
                                    break;
                                
                                case "AdditionalTask.Root":
                                    if (_projectsLibraryService.ProjectsLibrary.CurrentUserAdditionalGanttItems
                                        .Count() != 0)
                                    {
                                        var additionalelement = UserAdditionalRootCollection.FirstOrDefault();
                                        
                                        if (additionalelement != null)
                                        {
                                            _rootItemIndex = UserAdditionalRootCollection.IndexOf(additionalelement);
                                            cell.SetCellValue(additionalelement.Id.Name);
                                        }
                                    }
                                    else
                                    {
                                        Sheet.RemoveRow(Sheet.GetRow(_currentRow - 1));
                                        Sheet.RemoveRow(row);
                                        _currentRow++;
                                    }
                                    
                                    break;

                                case "Task.NumOfContract":
                                    StringValues.Add(cell.ColumnIndex, "NumOfContract");
                                    cell.SetCellValue(UserRootCollection[_rootItemIndex].Id.NumOfContract);
                                    break;
                                
                                case "AdditionalTask.NumOfContract":
                                    cell.SetCellValue(UserAdditionalRootCollection[_rootItemIndex].Id.NumOfContract);
                                    break;

                                case "Task.DateTime":
                                    StringValues.Add(cell.ColumnIndex, "TaskDateTime");
                                    cell.SetCellValue("");
                                    break;

                                case "AdditionalTask.DateTime":
                                    cell.SetCellValue("");
                                    break;
                                
                                case "Task.TaskDuration":
                                    StringValues.Add(cell.ColumnIndex, "TaskDuration");
                                    cell.SetCellValue("");
                                    break;
                                
                                case "AdditionalTask.TaskDuration":
                                    cell.SetCellValue("");
                                    break;

                                case "Task.Progress":
                                    StringValues.Add(cell.ColumnIndex, "Progress");
                                    _timeToWriteChildrens = true;
                                    cell.SetCellValue("");
                                    break;
                                
                                case "AdditionalTask.Progress":
                                    _timeToWriteAdditionalChildrens = true;
                                    cell.SetCellValue("");
                                    break;
                            }

                            if (_timeToWriteChildrens || _timeToWriteAdditionalChildrens)
                            {
                                WriteChildItemsInFile();
                            }
                            
                        }
                    }
                    
                    _currentRow++;
                }
            }
            
            using (var fileStream = new FileStream(SavePath, FileMode.Create, FileAccess.Write))
            {
                wb.Write(fileStream);
            }
        }

        public void WriteChildItemsInFile()
        {
            List<GanttTreeViewItemInfo> countChildItems = new List<GanttTreeViewItemInfo>();

            if (_timeToWriteChildrens)
            {
                countChildItems = _projectsLibraryService.ProjectsLibrary.CurrentUserGanttItems
                    .Where(t => t.ParentId != null &&
                                (Int32) t.ParentId.Id == (Int32) UserRootCollection[_rootItemIndex].Id.Id)
                    .ToList();
            }
            else
            {
                countChildItems = _projectsLibraryService.ProjectsLibrary.CurrentUserAdditionalGanttItems
                    .Where(t => t.ParentId != null &&
                                (Int32) t.ParentId.Id == (Int32) UserAdditionalRootCollection[_rootItemIndex].Id.Id)
                    .ToList();
            }
            
            if (countChildItems.Count() != 0)
            {
                while (_childItemIndex < countChildItems.Count())
                {
                    Sheet.CopyRow(_currentRow, _currentRow + 1);
                    _currentRow++;
                    var row = Sheet.GetRow(_currentRow);
                    _rowNums++;

                    foreach (var item in StringValues.Keys)
                    {
                       var cell = row.GetCell(item, MissingCellPolicy.CREATE_NULL_AS_BLANK);

                        if (cell.CellType == CellType.String)
                        {
                            if (StringValues[item] != "NumOfContract")
                            {
                                var prop = countChildItems[_childItemIndex].Id.GetType()
                                    .GetProperty(StringValues[item])
                                    ?.GetValue(countChildItems[_childItemIndex].Id).ToString();
                                
                                if(StringValues[item] == "Progress") 
                                    cell.SetCellValue(Convert.ToString(Convert.ToInt32(prop)*100)+"%");
                                else
                                {
                                    cell.SetCellValue(prop);
                                }
                                
                            }
                            else
                            {
                                cell.SetCellValue("");
                            }
                        }
                    }

                    _childItemIndex++;
                }

                _timeToWriteChildrens = false;
                _timeToWriteAdditionalChildrens = false;
                _childItemIndex = 0;
            }
        }

        public ReportService()
        {
            ReportPath = @"G:\CodeSolutions\TestGanttControl\TaskManager\TaskManager\ReportTemplates\MonthUserReportTemplate.xlsx";
            SavePath = @"G:\CodeSolutions\TestGanttControl\TaskManager\TaskManager\ReportTemplates\Result.xlsx";
            _rowNums = 0;
            _currentRow = 0;
            _maxCells = 0;
            _rootItemIndex = 0;
            _timeToWriteChildrens = false;
            _timeToWriteAdditionalChildrens = false;
            _childItemIndex = 0;
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            _projectsLibraryService = TaskManagerServices.Instance.GetInstance<IProjectsLibraryService>();
            UserRootCollection =
                _projectsLibraryService.ProjectsLibrary.CurrentUserGanttItems.Where(t => t.ParentId == null).ToList();
            UserAdditionalRootCollection = _projectsLibraryService.ProjectsLibrary.CurrentUserAdditionalGanttItems.Where(t => t.ParentId == null).ToList();
            StringValues = new Dictionary<int, string>();
        }
    }
}