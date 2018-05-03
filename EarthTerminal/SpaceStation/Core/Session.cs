using System;
using System.Diagnostics;
using System.Net.Sockets;

namespace SpaceStation.Core
{
    public class Session : ICloneable
    {
        public Session(Socket clientSocket)
        {
            Debug.Assert(clientSocket != null, "clientSocket != null");
            ClientSocket = clientSocket;
        }

        public int Id => ClientSocket.Handle.ToInt32();

        public string Datagram { get; set; }

        public Socket ClientSocket { get; }

        /// <summary>
        /// False => exception exit
        /// </summary>
        public bool IsNormalExit { get; set; }

        public override int GetHashCode() => ClientSocket.Handle.ToInt32();
        public object Clone() => new Session(ClientSocket) {Datagram = Datagram, IsNormalExit = IsNormalExit};

        public override bool Equals(object obj)
            => ClientSocket.Handle.ToInt32() == ((Session) obj).ClientSocket.Handle.ToInt32();

        public override string ToString() => $@"{nameof(Session)}:{Id}, IP:{ClientSocket.RemoteEndPoint}";

        public void Close()
        {
            ClientSocket.Shutdown(SocketShutdown.Both);
            ClientSocket.Close();
        }
    }
}