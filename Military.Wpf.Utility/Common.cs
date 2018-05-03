using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Windows;

namespace Military.Wpf.Utility
{
    public static class Common
    {
        public static object GetPropertyValue(this object src, string propName)
        {
            return src.GetType().GetProperty(propName).GetValue(src, null);
        }

        public static void SetPropertyValue( this object src, string propertyName, object value)
        {
            var srcType = src.GetType();
            var propertyInfo = srcType.GetProperty(propertyName);
            //Convert.ChangeType(value, propertyInfo.PropertyType)
            //http://codinghelmet.com/?path=hints/value-type-property-setting
            if (srcType.IsValueType)
            {
                var boxed = RuntimeHelpers.GetObjectValue(src);
                srcType.GetProperty(propertyName).SetValue(boxed, value, null);
                src = boxed;
                return;
            }

            propertyInfo.SetValue(src, value, null);
        }

        public static void SetPropertyOfStruct<T>(ref T src, string propertyName, object value)
        {
            var srcType = src.GetType();
            var propertyInfo = srcType.GetProperty(propertyName);
            //Convert.ChangeType(value, propertyInfo.PropertyType)
            //http://codinghelmet.com/?path=hints/value-type-property-setting
            Debug.Assert(srcType.IsValueType);

            var boxed = RuntimeHelpers.GetObjectValue(src);
            srcType.GetProperty(propertyName).SetValue(boxed, value, null);
            src = (T) boxed;
        }

        public static void OnUpdate<T>(this IEnumerable<T> instances, int id, string property, object value)
        {
            var theOne = instances.SingleOrDefault(i => (int)i.GetPropertyValue("Id") == id);

            Debug.Assert(theOne != null, "theOne != null");
            theOne.SetPropertyValue(property, value);
        }


        public static void Refresh(this UIElement control, DependencyProperty property)
        {
            var value = control.GetValue(property);

            control.ClearValue(property);
            control.SetValue(property, value);
        }
    }
}