using System;
using System.Windows.Data;

namespace Military.Wpf.Utility.Converter
{
    public class ObjectToTypeConverter : IValueConverter
    {
        public object Convert(
            object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture) => value.GetType();

        public object ConvertBack(
            object value, Type targetType,
            object parameter, System.Globalization.CultureInfo culture)
        {
            // I don't think you'll need this
            throw new Exception("Can't convert back");
        }
    }
}