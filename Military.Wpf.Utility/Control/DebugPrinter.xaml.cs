using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using Microsoft.Expression.Interactivity.Core;

namespace Military.Wpf.Utility.Control
{
    /// <summary>
    /// Interaction logic for DebugPrinter.xaml
    /// </summary>
    public partial class DebugPrinter : UserControl
    {
        public DebugPrinter()
        {
            InitializeComponent();
        }

        private void HideDebugButton_OnClick(object sender, RoutedEventArgs e)
        {
            (this.Parent as Panel).Children.Remove(this);
        }
    }
}
