using System.Diagnostics;
using System.Linq;
using System.Reflection;
using System.Threading;

namespace Military.Wpf.Utility
{
    public static class MutexHelper
    {
        private static Mutex _mutex;

        ///<summary>
        ///Create Mutex
        ///</summary>
        ///<returns>result，true -> success，false -> failed。</returns>
        public static bool TryCreateMutex()
            => TryCreateMutex(Assembly.GetEntryAssembly().FullName);

        ///<summary>
        ///Create Mutex
        ///</summary>
        ///<param name="name">Mutex name</param>
        ///<returns>result，true -> success，false -> failed。</returns>
        public static bool TryCreateMutex(string name)
        {
            bool result;
            _mutex = new Mutex(true, name, out result);
            return result;
        }

        ///<summary>
        ///release Mutex
        ///</summary>
        public static void ReleaseMutex()
        {
            if (_mutex == null) return;
            _mutex.Close();
            _mutex.Dispose();
        }

        public static bool IsAppAlreadyRunning()
        {
            Process currentProcess = Process.GetCurrentProcess();
            return Process.GetProcessesByName(currentProcess.ProcessName).Any(p => p.Id != currentProcess.Id);
        }
    }
}