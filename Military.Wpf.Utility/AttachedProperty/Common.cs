using System.Windows;
using System.Windows.Media;

namespace Military.Wpf.Utility.AttachedProperty
{
    public class Common
    {
        #region MouseOverGlow
        public static readonly DependencyProperty MouseOverGlowProperty = DependencyProperty.RegisterAttached(
             "MouseOverGlow", typeof(Color), typeof(Common), new PropertyMetadata(default(Color)));

        public static void SetMouseOverGlow(DependencyObject element, Color value)
        {
            element.SetValue(MouseOverGlowProperty, value);
        }

        public static Color GetMouseOverGlow(DependencyObject element)
        {
            return (Color)element.GetValue(MouseOverGlowProperty);
        } 
        #endregion

        #region IconTemplate
        public static readonly DependencyProperty IconTemplateProperty = DependencyProperty.RegisterAttached(
           "IconTemplate", typeof(DataTemplate), typeof(Common), new PropertyMetadata(default(DataTemplate)));

        public static void SetIconTemplate(DependencyObject element, DataTemplate value)
        {
            element.SetValue(IconTemplateProperty, value);
        }

        public static DataTemplate GetIconTemplate(DependencyObject element)
        {
            return (DataTemplate)element.GetValue(IconTemplateProperty);
        } 
        #endregion
    }
}
