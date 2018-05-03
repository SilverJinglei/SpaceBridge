using System;
using System.Diagnostics;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;
using SpaceStation.Core;

namespace SpaceStation.PeerToPeer
{
    internal abstract class P2PTerminalBase : TerminalBase
    {
        protected const string DEFAULT_SERVER = "localhost";
        protected const int DEFAULT_PORT = 45389;
        
        // one connectedStream <-> one bytes buffer
        protected readonly byte[] ReceivedBuffer = new byte[4096];

        protected TcpClient Client;
        protected NetworkStream ConnectedStream;

        public override Task SendTo(string content, IntPtr socketHandle) => Boradcast(content);

        public override Task Boradcast(string content)
        {
            if (!IsConnected)
#if NETFX3_5
                return TaskEx.FromResult(0);
#else
                return Task.CompletedTask;
#endif

            OnSent(content);

            var contentBytes = Encoding.GetBytes(content);
            return ConnectedStream.WriteAsync(contentBytes, 0, contentBytes.Length);
        }

        public virtual async Task Recieve()
        {
            try
            {
                while (IsConnected)
                {
#if UNITY
                    AsyncTools.WhereAmI("begin receive");
#endif
                    var byteCount = await ConnectedStream.ReadAsync(ReceivedBuffer, 0, ReceivedBuffer.Length);

#if UNITY
                    AsyncTools.WhereAmI("end receive");
#endif

                    if (byteCount == 0)
                    {
                        OnLosted();
                        return;
                    }

                    RemainingPiece += Encoding.GetString(ReceivedBuffer, 0, byteCount);

                    if (ConnectedStream.DataAvailable)
                        continue;

                    ResolveReceivedData(ref RemainingPiece, Client.Client.Handle);
                }
            }
            catch (IOException ex)
            {
                Func<bool> isLost = () =>
                {
                    var socketException = ex.InnerException as SocketException;
                    return socketException?.ErrorCode == 10054;
                };

                if (!isLost()) throw;

                OnLosted();
            }
            catch (Exception ex)
            {
                Debug.WriteLine(ex.Message);
                throw;
            }
        }

        public override void Close() => ConnectedStream?.Close();
    }
}