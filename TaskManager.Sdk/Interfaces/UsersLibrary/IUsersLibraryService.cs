using System;
using DevExpress.Mvvm.Gantt;
using TaskManager.Sdk.Core.Containers;
using TaskManager.Sdk.Core.Models;

namespace TaskManager.Sdk.Interfaces.UsersLibrary
{
    public interface IUsersLibraryService
    {
        UsersLibraryInfo UsersLibrary { get; set; }
        
        //авторизованный юзер
        UsersInfo CurrentUser { get; set; }
          
        //должности
        void LoadUsersPositions();
        
        //отделы
        void LoadGanttResourceItems();
           
        //коллекция пользователей 
        void LoadUsers();
          
        // Добавить юзера
        void AddUser(UsersInfo usersInfo);
          
        // Добавить отдел
        void AddGanttSourceItem(GanttResourceItemInfo ganttResourceItemInfo);
          
        // Удалить выбранный элемент из библиотеки пользователей
        void RemoveSelectedItemUserLibrary(Object selectedItem);
          
        // Обновить свойство выбранного объекта библиоткеи пользователей
        void UpdatePropertiesSelectedItemUserLibrary(Object selectedItem, String property);
    }
}