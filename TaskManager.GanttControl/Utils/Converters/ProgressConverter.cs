using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.GanttControl.Utils.Converters
{
    public class ProgressConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is double doubleValue)
            {
                doubleValue = doubleValue * 100;
                String newvalue = doubleValue.ToString();
                return newvalue;
            }
            return "";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is String currentvalue)
            {
                currentvalue = currentvalue.Substring(0, currentvalue.Length - 1);
                double doubleValue = double.Parse(currentvalue);
                doubleValue = doubleValue/100;
                return doubleValue;
            }

            return 0;
        }
    }
}