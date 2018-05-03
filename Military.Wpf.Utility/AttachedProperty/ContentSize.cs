using System.Windows;

namespace Military.Wpf.Utility.AttachedProperty
{
    public class ContentSize
    {
        #region Width

        public static readonly DependencyProperty WidthProperty = DependencyProperty.RegisterAttached(
            "Width", typeof(double), typeof(ContentSize), new PropertyMetadata(default(double)));

        public static void SetWidth(DependencyObject element, double value)
        {
            element.SetValue(WidthProperty, value);
        }

        public static double GetWidth(DependencyObject element)
        {
            return (double) element.GetValue(WidthProperty);
        }

        #endregion
            
        #region Height
        public static readonly DependencyProperty HeightProperty = DependencyProperty.RegisterAttached(
            "Height", typeof(double), typeof(ContentSize), new PropertyMetadata(default(double)));

        public static void SetHeight(DependencyObject element, double value)
        {
            element.SetValue(HeightProperty, value);
        }

        public static double GetHeight(DependencyObject element)
        {
            return (double)element.GetValue(HeightProperty);
        } 
        #endregion
    }
}