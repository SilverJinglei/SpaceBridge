using System;
using System.Linq;
using System.Collections.Generic;
using System.Diagnostics;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Threading;
using GalaSoft.MvvmLight.Threading;

namespace Military.Wpf.Utility
{
    public class TextBoxTraceListener : TraceListener
    {
        public TextBox Target { get; }

        private TextBoxTraceListener(TextBox target)
        {
            Target = target;

            Target.IsReadOnly = true;
            Target.TextWrapping = TextWrapping.Wrap;
            Target.HorizontalScrollBarVisibility = ScrollBarVisibility.Auto;
            Target.VerticalScrollBarVisibility = ScrollBarVisibility.Auto;

            Target.TextChanged += _target_TextChanged;
        }

        private void _target_TextChanged(object sender, TextChangedEventArgs e)
        {
            Target.ScrollToEnd();
        }

        public static void Register(TextBox target)
        {
            var theListener = new TextBoxTraceListener(target);
            Debug.Listeners.Add(theListener);
        }

        public static void Unregister(TextBox target)
        {
            foreach (TextBoxTraceListener listener in Debug.Listeners)
            {
                if(listener == null)
                    continue;

                if (Equals(listener.Target, target))
                {
                    Debug.Listeners.Remove(listener);
                    return;
                }
            }
        }

        public override void Write(string message) =>
            DispatcherHelper.CheckBeginInvokeOnUI(() =>
                Target.Text += $@"[{DateTime.Now}] {message}");

        public override void WriteLine(string message)
        {
            //DispatcherHelper.CheckBeginInvokeOnUI(() =>
            //    _target.Text += $@"[{DateTime.Now}] {message} {Environment.NewLine}");

            Target.Dispatcher.BeginInvoke(DispatcherPriority.SystemIdle, new Action(() =>
                Target.Text += $@"[{DateTime.Now}] {message} {Environment.NewLine}"));
        }
    }
}