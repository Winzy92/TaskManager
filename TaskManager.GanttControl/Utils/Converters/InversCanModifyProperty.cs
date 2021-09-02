using System;
using System.Globalization;
using System.Windows.Data;

namespace TaskManager.GanttControl.Utils.Converters
{
    public class InversCanModifyProperty : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is Boolean valueBool)
            {
                if (valueBool)
                    return false;
                else
                {
                    return true;
                }
            }

            return value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return value;
        }
    }
}