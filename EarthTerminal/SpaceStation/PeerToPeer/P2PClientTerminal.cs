using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SpaceStation.PeerToPeer
{
    internal class P2PClientTerminal : P2PTerminalBase
    {
        public P2PClientTerminal()
        {
            Client = new TcpClient();
        }

        public override async Task LaunchAsync()
        {
            if (string.IsNullOrEmpty(ServerIp))
                ServerIp = DEFAULT_SERVER;

            if (Port == 0)
                Port = DEFAULT_PORT;

            try
            {
                await Client.ConnectAsync(ServerIp, Port);
            }
            catch (SocketException se)
            {
                Debug.WriteLine(se.Message);

                if (se.ErrorCode == 10061)
                {
                    // not find server
                    return;
                }
                throw;
            }

            ConnectedStream = Client.GetStream();

            OnConnected();

            await Recieve();
            // Recieve().Forget(); // Forget() => run asynchronous. Recieve() is infinite loop, it cannot return [await]. so Forget() could let return await
        }
    }
}