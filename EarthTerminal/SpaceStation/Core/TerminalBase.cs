using System;
using System.Diagnostics;
using System.Text;
using System.Threading.Tasks;

namespace SpaceStation.Core
{
    internal abstract class TerminalBase : ITerminal
    {
        public string ServerIp { get; set; }
        public int Port { get; set; }

        public bool IsConnected { get; protected set; }

        protected Encoding Encoding { get; } = Encoding.ASCII;// Encoding.GetEncoding("gb2312");

        protected string RemainingPiece = string.Empty;

#region Resovler

        protected DatagramResolverBase Resolver { get; } = new JsonResolver();

        protected string ResolveReceivedData(string request, IntPtr socketHandle)
        {
            ResolveReceivedData(ref request, socketHandle);
            return request; // it is remainingPiece
        }

        protected void ResolveReceivedData(ref string remainingPiece, IntPtr socketHandle)
        {
            var resolvedDatagrams = Resolver.Resolve(ref remainingPiece);

            foreach (var gram in resolvedDatagrams)
            {
                Debug.Assert(gram.EndsWith("}") && gram.StartsWith("{"));

                Debug.WriteLine($"[Remote] wrote {gram}");
                OnRecieved(gram, socketHandle);
            }
        }

#endregion

#region Event
        public event EventHandler Losted;

        protected virtual void OnLosted()
        {
            Debug.WriteLine("[Client] Lost");

            IsConnected = false;
            Losted?.Invoke(this, EventArgs.Empty);
        }

        public event EventHandler Connected;

        protected virtual void OnConnected()
        {
            Debug.WriteLine("Client has connected");

            IsConnected = true;
            Connected?.Invoke(this, EventArgs.Empty);
        }

#if NETFX3_5
        public event EventHandler<EventArgs<Tuple<IntPtr, string>>> Recieved;
#else
        public event EventHandler<Tuple<IntPtr, string>> Recieved;
#endif

        protected virtual void OnRecieved(string msg, IntPtr socketHandle)
        {
            Debug.WriteLine(msg);

#if NETFX3_5
            Recieved?.Invoke(
                this,
                new EventArgs<Tuple<IntPtr, string>>
                {
                    Item = new Tuple<IntPtr, string>(socketHandle, msg)
                });
#else
            Recieved?.Invoke(this, new Tuple<IntPtr, string>(socketHandle, msg));
#endif
        }

        protected virtual void OnSent(string e)
        {
            Debug.WriteLine($"[Send] {Environment.NewLine} {e}");
        }

#endregion

        public abstract void Close();
        public abstract Task LaunchAsync();
        public abstract Task Boradcast(string content);
        public abstract Task SendTo(string content, IntPtr socketHandle);
    }
}