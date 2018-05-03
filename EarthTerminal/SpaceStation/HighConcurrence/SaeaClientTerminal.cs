using System;
using System.Diagnostics;
using System.Net;
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceStation.HighConcurrence
{
    /// <summary>
    /// class OSClient : OSCore
    /// This is a naive client class that I added into this project just to test the server.
    /// It does very little error checking and is not suitable for anything but testing.
    /// </summary>
    internal class SaeaClientTerminal : SaeaTerminalBase
    {
        static readonly ManualResetEventSlim ClientDone = new ManualResetEventSlim(false);

        public SocketAsyncEventArgs AcceptAndReceiveSaea { get; set; }

        public SocketAsyncEventArgs SendSaea { get; set; } = new SocketAsyncEventArgs();

        public SaeaClientTerminal()
        {
        }

        private void OnSendSaea_Completed(object sender, SocketAsyncEventArgs e)
        {

        }

        private void OnAcceptReceiveSaea_Completed(object sender, SocketAsyncEventArgs e)
        {
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Connect:
                    ProcessConnect(e);
                    break;

                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
            }
        }

        // Called when a ConnectAsync operation completes
        private void ProcessConnect(SocketAsyncEventArgs connectedSaea)
        {
            if (connectedSaea.SocketError == SocketError.Success)
            {
                OnConnected();

                connectedSaea.SetBuffer(new byte[DEFAULT_BUFFER_SIZE], 0, DEFAULT_BUFFER_SIZE);

                if (!ConnectionSocket.ReceiveAsync(connectedSaea))
                    ProcessReceive(connectedSaea);
            }
            else
            {
                TryHandleSocketError(connectedSaea.SocketError);
            }
        }

        /// <summary>
        /// Called when a ReceiveAsync operation completes
        /// </summary>
        private void ProcessReceive(SocketAsyncEventArgs readSaea)
        {
            if (TryHandleSocketError(readSaea.SocketError))
                return;

            if (readSaea.BytesTransferred == 0)
            {
                OnLosted();
                return;
            }

            RemainingPiece += Encoding.GetString(readSaea.Buffer, 0, readSaea.BytesTransferred);
            
            if (ConnectionSocket.Available == 0)
                ResolveReceivedData(ref RemainingPiece, ConnectionSocket.Handle);

            if (!ConnectionSocket.ReceiveAsync(readSaea))
                ProcessReceive(readSaea);
        }

        bool TryHandleSocketError(SocketError error)
        {
            switch (error)
            {
                case SocketError.Success:
                    return false;
                case SocketError.OperationAborted:
                case SocketError.ConnectionRefused:
                    return true;
                case SocketError.ConnectionReset:
                    OnLosted();
                    return true;
                default:
                    throw new SocketException((int)error);
            }
        }

        /// <summary>
        /// This method connects us to the server.
        /// Winsock is very optimistic about connecting to the server.
        /// It will not tell you, for instance, if the server actually accepted the connection.  It assumes that it did.
        /// </summary>
        public override Task LaunchAsync() =>
#if NETFX3_5
            TaskEx.Run(() =>
#else
            Task.Run(() =>
#endif
            {
                CreateSocket();

                AcceptAndReceiveSaea = new SocketAsyncEventArgs
                {
                    RemoteEndPoint = ConnectionEndpoint,
                    UserToken = ConnectionSocket
                };

                AcceptAndReceiveSaea.Completed += OnAcceptReceiveSaea_Completed;
                SendSaea.Completed += OnSendSaea_Completed;

                ConnectionSocket.ConnectAsync(AcceptAndReceiveSaea);
                ClientDone.Wait();

                TryHandleSocketError(AcceptAndReceiveSaea.SocketError);
            });

        /// <summary>
        /// This method is used to send a message to the server
        /// </summary>
        public override Task Boradcast(string content)
        {
            if (string.IsNullOrEmpty(content))
                return GetExceptionTask("No message provided for Boradcast.");

            // We need a connection to the server to send a message
            if (!ConnectionSocket.Connected)
                return GetExceptionTask("Not connect");

            OnSent(content);

#if NETFX3_5
            return TaskEx.Run(() =>
#else
            return Task.Run(() =>
#endif
            {
                var contentBytes = Encoding.GetBytes(content);

                SendSaea.SetBuffer(contentBytes, 0, contentBytes.Length);
                ConnectionSocket.SendAsync(SendSaea);
            });
        }

        public override Task SendTo(string content, IntPtr socketHandle)
        {
            throw new NotImplementedException();
        }

        public override void Close() => DisConnect();

        // This method disconnects us from the server
        private void DisConnect()
        {
            try
            {
                ConnectionSocket.Shutdown(SocketShutdown.Both);
                ConnectionSocket.Close();
                ClientDone.Set();
            }
            catch
            {
                //nothing to do since connection is already closed
            }
        }
    }
}