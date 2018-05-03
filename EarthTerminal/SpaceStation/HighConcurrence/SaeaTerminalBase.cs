using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Threading.Tasks;
using SpaceStation.Core;

namespace SpaceStation.HighConcurrence
{
    /// <summary>
    /// class OSCore
    /// This is a base class that is used by both clients and servers.
    /// It contains the plumbing to set up a socket connection.
    /// </summary>
    abstract class SaeaTerminalBase : TerminalBase
    {
        // these are the defaults if the user does not provide any parameters
        protected const string DEFAULT_SERVER = "localhost";
        protected const int DEFAULT_PORT = 804;

        //  We default to a 256 Byte buffer size
        protected const int DEFAULT_BUFFER_SIZE = 256;

        // This is the connection socket and endpoint information
        protected Socket ConnectionSocket;
        protected IPEndPoint ConnectionEndpoint;

        // This is some error handling stuff that is not well implemented
        public string LastError { get; protected set; }

        protected bool Exceptionthrown;

        // This is the current buffer size for receive and send
        protected int Buffersize;

        /// <summary>
        /// An IPEndPoint contains all of the information about a server or client
        /// machine that a socket needs.  Here we create one from information
        /// that we send in as parameters
        /// </summary>
        public IPEndPoint CreateIPEndPoint()
        {
            if (string.IsNullOrEmpty(ServerIp))
                ServerIp = DEFAULT_SERVER;

            if (Port == 0)
                Port = DEFAULT_PORT;

            // We get the IP address and stuff from DNS (Domain Name Services)
            // I think you can also pass in an IP address, but I would not because
            // that would not be extensible to IPV6 later
            IPHostEntry hostInfo = Dns.GetHostEntry(ServerIp);
            IPAddress serverAddr = hostInfo.AddressList[1]; // [0]IPv6  [1]IPv4
            return new IPEndPoint(serverAddr, Port);
        }

        /// <summary>
        /// This method peels apart the command string to create either a client or server socket,
        /// which is not great because it means the method has to know the semantics of the command
        /// that is passed to it.  So this needs to be fixed.
        /// </summary>
        protected void CreateSocket()
        {
            // If we get here, we try to create the socket using the IPEndpoint information.
            // We are defaulting here to TCP Stream sockets, but you could change this with more parameters.
            ConnectionEndpoint = CreateIPEndPoint();

            ConnectionSocket = new Socket(ConnectionEndpoint.AddressFamily, SocketType.Stream, ProtocolType.Tcp);
        }

        protected Task GetExceptionTask(string message)
        {
            LastError = message;

#if NETFX3_5
            return TaskEx.Run(() => { throw new Exception(LastError); });
#else
            return Task.FromException(new Exception(LastError));
#endif
        }
    }
}