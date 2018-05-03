using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using System.Windows.Interactivity;

namespace Military.Wpf.Utility.Behavior
{
    public class TextBoxTraceListenerBehavior : Behavior<TextBox>
    {
        protected override void OnAttached()
        {
            base.OnAttached();

            TextBoxTraceListener.Register(AssociatedObject);
        }

        protected override void OnDetaching()
        {
            base.OnDetaching();

            TextBoxTraceListener.Unregister(AssociatedObject);
        }
    }
}
