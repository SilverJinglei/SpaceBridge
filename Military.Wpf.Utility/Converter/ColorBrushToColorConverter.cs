using System;
using System.Globalization;
using System.Windows.Data;
using System.Windows.Media;

namespace Military.Wpf.Utility.Converter
{
    public class ColorBrushToColorConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ToColor(value);
        }

        private static Color ToColor(object value)
        {
            SolidColorBrush brush = value as SolidColorBrush;
            if (brush != null)
                return brush.Color;

            GradientBrush gradientBrush = value as GradientBrush;
            if (gradientBrush != null)
                return gradientBrush.GradientStops[0].Color;

            return Colors.Yellow;
            //throw new Exception("It's not ColorBrush");
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}