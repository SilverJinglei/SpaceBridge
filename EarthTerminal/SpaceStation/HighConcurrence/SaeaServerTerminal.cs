using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

namespace SpaceStation.HighConcurrence
{
    /// <summary>
    /// class OSServer : OSCore
    /// This is the server class that is derived from OSCore.
    /// It creates a server that listens for client connections, then receives
    /// text data from those clients and writes it to the console screen
    /// </summary>
    internal class SaeaServerTerminal : SaeaTerminalBase
    {
        // We limit this server client connections for test purposes
        protected const int DEFAULT_MAX_CONNECTIONS = 15;

        // We use a Mutex to block the listener thread so that limited client connections are active
        // on the server.  If you stop the server, the mutex is released. 
        // First we set up our mutex and semaphore
        private static readonly Mutex Mutex = new Mutex();

        // Here is where we track the number of client connections
        protected int NumConnections;

        // Here is where we track the totalbytes read by the server
        protected int Totalbytesread;

        // Here is our stack of available accept sockets
        protected ObjectPool<SocketAsyncEventArgs> ReceiveSaeaPool = new ObjectPool<SocketAsyncEventArgs>(()=> new SocketAsyncEventArgs());

        public  ConcurrentBag<SaeaSession> Sessions { get; } = new ConcurrentBag<SaeaSession>();

        

        // Default constructor
        public SaeaServerTerminal()
        {
            NumConnections = 0;
            
            // Now we create enough read sockets to service the maximum number of clients
            // that we will allow on the server
            // We also assign the event handler for IO Completed to each socket as we create it
            // and set up its buffer to the right size.
            // Then we push it onto our stack to wait for a client connection
            for (var i = 0; i < DEFAULT_MAX_CONNECTIONS; i++)
            {
                var readSaea = new SocketAsyncEventArgs();
                readSaea.Completed += OnReceivedSaeaCompleted;
                readSaea.SetBuffer(new byte[DEFAULT_BUFFER_SIZE], 0, DEFAULT_BUFFER_SIZE);
                ReceiveSaeaPool.PutObject(readSaea);
            }
        }

        /// <summary>
        /// This method is called when there is no more data to read from a connected client
        /// </summary>
        private void OnReceivedSaeaCompleted(object sender, SocketAsyncEventArgs e)
        {
            // Determine which type of operation just completed and call the associated handler.
            // We are only processing receives right now on this server.
            switch (e.LastOperation)
            {
                case SocketAsyncOperation.Receive:
                    ProcessReceive(e);
                    break;
                default:
                    throw new ArgumentException("The last operation completed on the socket was not a receive");
            }
        }

        /// <summary>
        // We call this method once to start the server if it is not started
        /// </summary>
        public override Task LaunchAsync() => Task.Run(() =>
        {
            Mutex.WaitOne();
           
            // First create a generic socket
            CreateSocket();
            // Now make it a listener socket at the IP address and port that we specified
            ConnectionSocket.Bind(ConnectionEndpoint);

            // Now start listening on the listener socket and wait for asynchronous client connections
            ConnectionSocket.Listen(DEFAULT_MAX_CONNECTIONS);

            StartAcceptAsync(null);
        });

        /// <summary>
        /// This method is called once to stop the server if it is started.
        /// We could check for the open socket here
        /// to stop some exception noise.
        /// </summary>
        public override void Close()
        {
            ConnectionSocket.Close();
            Mutex.ReleaseMutex();
        }

        #region Accept

        /// <summary>
        /// This method implements the asynchronous loop of events
        /// that accepts incoming client connections
        /// </summary>
        public void StartAcceptAsync(SocketAsyncEventArgs acceptSaea)
        {
            // If there is not an accept socket, create it
            // If there is, reuse it
            if (acceptSaea == null)
            {
                acceptSaea = new SocketAsyncEventArgs();
                acceptSaea.Completed += OnAcceptCompleted;
            }
            else
            {
                acceptSaea.AcceptSocket = null;
            }

            // this will return true if there is a connection
            // waiting to be processed (IO Pending) 
            bool acceptpending = ConnectionSocket.AcceptAsync(acceptSaea);

            // If not, we can go ahead and process the accept.
            // Otherwise, the Completed event we tacked onto the accept socket will do it when it completes
            if (!acceptpending)
            {
                // Process the accept event
                ProcessAccept(acceptSaea);
            }
        }

        /// <summary>
        /// This method is triggered when the accept socket completes an operation async
        /// In the case of our accept socket, we are looking for a client connection to complete
        /// </summary>
        private void OnAcceptCompleted(object sender, SocketAsyncEventArgs asyncEventArgs) => ProcessAccept(asyncEventArgs);

        // This method is used to process the accept socket connection
        private void ProcessAccept(SocketAsyncEventArgs acceptSaea)
        {
            // First we get the accept socket from the passed in arguments
            var acceptSocket = acceptSaea.AcceptSocket;

            // If the accept socket is connected to a client we will process it
            // otherwise nothing happens
            if (!acceptSocket.Connected)
                return;

            if (NumConnections >= DEFAULT_MAX_CONNECTIONS)
            {
                acceptSocket.Close();
                var ex = new Exception("No more connections can be accepted on the server.");
                throw ex;
            }

            Interlocked.Increment(ref NumConnections);
            OnConnected();

            // Go get a read socket out of the read socket stack
            var readSaea = ReceiveSaeaPool.GetObject();

            // Create our user object and put the accept socket into it to use later
            var newSession = new SaeaSession(acceptSocket, DEFAULT_BUFFER_SIZE);
            readSaea.UserToken = newSession;

            newSession.Recieved += OnSession_Recieved;
            Sessions.Add(newSession);
            
            if (!acceptSocket.ReceiveAsync(readSaea))
                ProcessReceive(readSaea);

            // Start the process again to wait for the next connection
            StartAcceptAsync(acceptSaea);
        }

        private void OnSession_Recieved(object sender, string e) => OnRecieved(e, ((SaeaSession) sender).OwnerSocket.Handle);

        #endregion

        // This method processes the read socket once it has a transaction
        private void ProcessReceive(SocketAsyncEventArgs readSaea)
        {
            // if BytesTransferred is 0, then the remote end closed the connection
            if (readSaea.BytesTransferred <= 0)
            {
                OnLosted();
                CloseReadSocket(readSaea);
                return;
            }

//SocketError.Success indicates that the last operation on the underlying socket succeeded
            if (readSaea.SocketError != SocketError.Success)
            {
                ProcessError(readSaea);
                return;
            }

            var session = (SaeaSession) readSaea.UserToken;

            session.RemainingPiece += Encoding.GetString(readSaea.Buffer, 0, readSaea.BytesTransferred);
            
            var readsocket = session.OwnerSocket;

            session.RemainingPiece = ResolveReceivedData(session.RemainingPiece, readsocket.Handle);

            if (!readsocket.ReceiveAsync(readSaea))
                ProcessReceive(readSaea);

            //if (session.ReadSocketData(readSaea))
            //{
            //    var readsocket = session.OwnerSocket;

            //    // If the read socket is empty, we can do something with the data that we accumulated
            //    // from all of the previous read requests on this socket
            //    if (readsocket.Available == 0)
            //        session.ProcessData(readSaea);

            //    // Start another receive request and immediately check to see if the receive is already complete
            //    // Otherwise OnIOCompleted will get called when the receive is complete
            //    // We are basically calling this same method recursively until there is no more data
            //    // on the read socket
            //    var ioPending = readsocket.ReceiveAsync(readSaea);
            //    if (!ioPending)
            //        ProcessReceive(readSaea);

            //    return;
            //}
        }


        private void ProcessError(SocketAsyncEventArgs readSocket)
        {
            Debug.WriteLine(readSocket.SocketError.ToString());
            CloseReadSocket(readSocket);
        }


        // This overload of the close method doesn't require a token
        private void CloseReadSocket(SocketAsyncEventArgs readSocket)
        {
            var token = readSocket.UserToken as SaeaSession;
            CloseReadSocket(token, readSocket);
        }


        // This method closes the read socket and gets rid of our user token associated with it
        private void CloseReadSocket(SaeaSession token, SocketAsyncEventArgs readSaea)
        {
            // Put the read socket back in the stack to be used again
            ReceiveSaeaPool.PutObject(readSaea);

            Sessions.TryTake(out token);

            token.Dispose();

            // Decrement the counter keeping track of the total number of clients connected to the server.
            Interlocked.Decrement(ref NumConnections);
        }

        public void Send(SaeaSession session, string content)
        {
            session.Send(Encoding.GetBytes(content));
        }

        public override Task SendTo(string content, IntPtr socketHandle) => Task.Run(() =>
        {
            var contentBytes = Encoding.GetBytes(content);
            Sessions.SingleOrDefault(s => s.OwnerSocket.Handle == socketHandle)?.Send(contentBytes);
        });

        public override Task Boradcast(string content) => Task.Run(() =>
        {
            OnSent(content);
            var contentBytes = Encoding.GetBytes(content);

            foreach (var session in Sessions)
            {
                session.Send(contentBytes);
            }
        });
    }
}