using System;
using System.Threading.Tasks;

namespace SpaceStation.Core
{
    /// <summary>
    /// Interface of common socket communication: 1 to 1, 1 to *, * to *
    /// </summary>
    public interface ITerminal
    {
        string ServerIp { get; set; }
        int Port { get; set; }

        bool IsConnected { get; }

        event EventHandler Connected;
        event EventHandler Losted;

#if NETFX3_5
        event EventHandler<EventArgs<Tuple<IntPtr, string>>> Recieved;
#else
        event EventHandler<Tuple<IntPtr, string>> Recieved;
#endif

        Task LaunchAsync();
        Task Boradcast(string content);
        Task SendTo(string content, IntPtr socketHandle);
        //Task Recieve();

        void Close();
    }
}