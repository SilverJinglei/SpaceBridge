using System;
using System.Windows;

namespace Military.Wpf.Utility
{
    public static class VisualHelper
    {
        public static void RaiseInLoaded(this FrameworkElement frameworkElement, Action loadedAction)
        {
            if (frameworkElement.IsLoaded)
            {
                loadedAction();
                return;
            }

            RoutedEventHandler loadedOnceHandle = null;
            loadedOnceHandle = (ls, le) =>
            {
                frameworkElement.Loaded -= loadedOnceHandle;
                loadedAction();
            };

            frameworkElement.Loaded += loadedOnceHandle;
        }
    }
}