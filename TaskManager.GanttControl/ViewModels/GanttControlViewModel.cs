using System;
using System.Collections.ObjectModel;
using DevExpress.Mvvm.Gantt;

namespace TaskManager.GanttControl.ViewModels
{
    public class GanttControlViewModel
    {
        public ObservableCollection<GanttTask> Tasks { get; set; }
        
        public GanttControlViewModel()
        {
            Tasks = new ObservableCollection<GanttTask> {
                new GanttTask() {
                    Id = 0,
                    Name = "ТЭЦ-22, КНОУ, КН ЭБ1",
                    StartDate = DateTime.Now.AddDays(60),
                    FinishDate = DateTime.Now.AddDays(100),
                    //NumberOfContract = "№15/21/ТИ-1 от 02.02.2021",
                },
                new GanttTask() {
                    Id =1, 
                    ParentId = 0,
                    Name = "Сбор, анализ и выдача замечаний к ИД",
                    StartDate = DateTime.Now.AddDays(-1),
                    FinishDate = DateTime.Now.AddDays(2),
                    //Department = "ОРВ АСУ ТП",
                    //Unit = "Куксов С.В."
                },
                new GanttTask() {
                    Id = 2,
                    ParentId = 0,
                    Name = "Разработка ТЗ на создание АСУ ТП",
                    StartDate = DateTime.Now.AddDays(2),
                    FinishDate = DateTime.Now.AddDays(5),
                    //Department = "ОРВ АСУ ТП",
                    //Unit = "Шарабакин А.С."
                },
                new GanttTask() {
                    Id = 3,
                    ParentId = 0,
                    Name = "Разработка ОТР, ПЗ, презентации",
                    StartDate = DateTime.Now.AddDays(2),
                    FinishDate = DateTime.Now.AddDays(5),
                    //Department = "ОРВ АСУ ТП",
                    //Unit = "Тычков В.В."
                        
                },
                new GanttTask() {
                    Id = 4,
                    ParentId = 0,
                    Name = "ПИР ПТК",
                    StartDate = DateTime.Now.AddDays(5),
                    FinishDate = DateTime.Now.AddDays(6),
                    //Department = "ОРВ АСУ ТП",
                    //Unit = "Куксов С.В."
                }
            };
        }
        
        
    }
}