using System;
using System.Collections.ObjectModel;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.GanttControl.Utils.Converters
{
    public class CollectionToString : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is ObservableCollection<String> collection)
            {
                String newvalue = String.Join("\n", collection);
                return newvalue;
            }

            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}