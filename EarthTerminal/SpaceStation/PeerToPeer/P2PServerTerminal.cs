using System.Diagnostics;
using System.Net.Sockets;
using System.Threading.Tasks;

namespace SpaceStation.PeerToPeer
{
    internal class P2PServerTerminal : P2PTerminalBase
    {
        private TcpListener _listener;

        public override async Task LaunchAsync()
        {
            if (Port == 0)
                Port = DEFAULT_PORT;

            _listener = TcpListener.Create(Port);
            _listener.ExclusiveAddressUse = true;
            _listener.Start();

            await AcceptClientAsync();
        }

#if TERMINAL_SERVER_TEST
        public bool IsListening { get; set; } = true;

        private async Task AcceptClientAsync()
        {
            while (IsListening)
            {
                var client  = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
                ProcessAsync(client).Forget();
            }
#else
        private async Task AcceptClientAsync()
        {
            Client = await _listener.AcceptTcpClientAsync().ConfigureAwait(false);
            await ProcessAsync(Client);
        }
#endif

        async Task ProcessAsync(TcpClient client)
        {
            ConnectedStream = client.GetStream();
            
            OnConnected();
            await Recieve();
        }

        protected override async void OnLosted()
        {
            base.OnLosted();

            Close();
            await AcceptClientAsync();
        }
    }
}