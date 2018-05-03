using System.Diagnostics;
using System.Drawing;
using System.Windows;
using System.Windows.Forms;

namespace Military.Wpf.Utility
{
    public static class WindowsUtility
    {
        public static void Show(this System.Windows.Controls.UserControl viewControl)
            => new Window {Content = viewControl, Topmost = false, SizeToContent = SizeToContent.WidthAndHeight}.Show();

        public static void ShowMonitor(this System.Windows.Controls.UserControl viewControl, int screenIndex)
            => new Window {Content = viewControl}.ShowMonitor(screenIndex);

        // NB : Best to call this function from the windows Loaded event or after showing the window
        // (otherwise window is just positioned to fill the secondary monitor rather than being maximised).
        public static void ShowMonitor(this Window window, int screenIndex)
        {
            var screenCount = Screen.AllScreens.Length;

            int id = screenIndex;
            if (id >= screenCount)
                id = screenCount - 1;

            var secondaryScreen = Screen.AllScreens[id];

            if (secondaryScreen != null)
            {
                if (!window.IsLoaded)
                    window.WindowStartupLocation = WindowStartupLocation.Manual;

                Rectangle workingArea = secondaryScreen.WorkingArea;
                window.Left = workingArea.Left;
                window.Top = workingArea.Top;
                window.Width = workingArea.Width;
                window.Height = workingArea.Height;
                window.WindowStyle = WindowStyle.None;

                window.Show();

                // If window isn't loaded then maxmizing will result in the window displaying on the primary monitor
                if (window.IsLoaded)
                {
                    // code do this, never do it in window self.
                    window.WindowState = WindowState.Maximized;
                    window.WindowStyle = WindowStyle.None;
                }
            }
        }


        //This is a replacement for Cursor.Position in WinForms
        [System.Runtime.InteropServices.DllImport("user32.dll")]
        static extern bool SetCursorPos(int x, int y);

        [System.Runtime.InteropServices.DllImport("user32.dll")]
        public static extern void mouse_event(int dwFlags, int dx, int dy, int cButtons, int dwExtraInfo);

        public const int MOUSEEVENTF_LEFTDOWN = 0x02;
        public const int MOUSEEVENTF_LEFTUP = 0x04;

        //This simulates a left mouse click
        public static void LeftMouseClick(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            System.Threading.Thread.Sleep(100);
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
        }

        public static void LeftMouseDown(int xpos, int ypos)
        {
            mouse_event(MOUSEEVENTF_LEFTDOWN, xpos, ypos, 0, 0);
            Debug.WriteLine("Mouse Down");
        }

        public static void LeftMouseMove(int xpos, int ypos)
        {
            SetCursorPos(xpos, ypos);
        }

        public static void LeftMouseUp(int xpos, int ypos)
        {
            mouse_event(MOUSEEVENTF_LEFTUP, xpos, ypos, 0, 0);
            Debug.WriteLine("Mouse Up");
        }
    }
}