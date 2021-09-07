using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Linq;
using System.Windows.Data;
using TaskManager.Sdk.Core.Models;
using TaskManager.Sdk.Interfaces;
using TaskManager.Sdk.Interfaces.UsersLibrary;
using TaskManager.Sdk.Services.TaskManagerService;

namespace TaskManager.UsersLibrary.Utils.Converters
{
    public class IdToObjConverter : IValueConverter
    {
        private readonly IUsersLibraryService _usersLibraryService;
        
        public ObservableCollection<PositionsInfo> PositionsInfos { get; set; }
        
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Int32 intValue)
            {
                var item = PositionsInfos.FirstOrDefault(t => t.Id == intValue);

                if (item != null)
                {
                    return item;
                }
                else
                {
                    return "";
                }
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is PositionsInfo positions)
            {
                var item = PositionsInfos.FirstOrDefault(t => t.Id == positions.Id);

                return item.Id;
            }

            return 0;
        }

        public IdToObjConverter()
        {
            _usersLibraryService = TaskManagerServices.Instance.GetInstance<IUsersLibraryService>();
            PositionsInfos = _usersLibraryService.UsersLibrary.PositionsInfoItems;
        }
    }
}