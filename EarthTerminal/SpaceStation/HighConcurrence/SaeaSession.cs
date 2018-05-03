using System;
using System.Net.Sockets;
using System.Text;

namespace SpaceStation.HighConcurrence
{
    /// <summary>
    /// class SaeaSession : IDisposable
    /// This class represents the instantiated read socket on the server side.
    /// It is instantiated when a server listener socket accepts a connection.
    /// </summary>
    sealed class SaeaSession : IDisposable
    {
        // This is a ref copy of the socket that owns this token

        // this stringbuilder is used to accumulate data off of the readsocket
        private readonly StringBuilder _stringbuilder;

        public string RemainingPiece { get; set; }

        // This stores the total bytes accumulated so far in the stringbuilder
        private int _totalbytecount;

        // We are holding an exception string in here, but not doing anything with it right now.
        public string LastError;

        private static readonly ObjectPool<SocketAsyncEventArgs> SendSaeaPool = new ObjectPool<SocketAsyncEventArgs>(() => new SocketAsyncEventArgs());

        // The read socket that creates this object sends a copy of its "parent" accept socket in as a reference
        // We also take in a max buffer size for the data to be read off of the read socket
        public SaeaSession(Socket readSocket, int bufferSize)
        {
            OwnerSocket = readSocket;
            _stringbuilder = new StringBuilder(bufferSize);
        }

        // This allows us to refer to the socket that created this token's read socket
        public Socket OwnerSocket { get; }

#if NETFX3_5
        public event EventHandler<EventArgs<string>> Recieved;
#else
        public event EventHandler<string> Recieved;
#endif

        /// <summary>
        /// Do something with the received data, then reset the token for use by another connection.
        /// This is called when all of the data have been received for a read socket.
        /// </summary>
        public void ProcessData(SocketAsyncEventArgs args)
        {
            // Get the last message received from the client, which has been stored in the stringbuilder.
            var received = _stringbuilder.ToString();

#if NETFX3_5
            Recieved?.Invoke(this, new EventArgs<string>
            {
                Item = received
            });
#else
            Recieved?.Invoke(this, received);
#endif

            // Clear StringBuffer, so it can receive more data from the client.
            _stringbuilder.Length = 0;
            _totalbytecount = 0;
        }

        /// <summary>
        /// This method gets the data out of the read socket and adds it to the accumulator string builder
        /// </summary>
        public bool ReadSocketData(SocketAsyncEventArgs readSaea)
        {
            var bytecount = readSaea.BytesTransferred;

            if ((_totalbytecount + bytecount) > _stringbuilder.Capacity)
            {
                LastError = "Receive ReceivedBuffer cannot hold the entire message for this connection.";
                return false;
            }

            _stringbuilder.Append(Encoding.ASCII.GetString(readSaea.Buffer, readSaea.Offset, bytecount));
            _totalbytecount += bytecount;
            return true;
        }

        public void Send(byte[] data)
        {
            var sendSaea = SendSaeaPool.GetObject();
            sendSaea.Completed += SendSaea_Completed;
            sendSaea.SetBuffer(data, 0, data.Length);

            OwnerSocket.SendAsync(sendSaea);
        }

        private void SendSaea_Completed(object sender, SocketAsyncEventArgs e)
        {
            e.Completed -= SendSaea_Completed;
            SendSaeaPool.PutObject(e);
        }

        // This is a standard IDisposable method
        // In this case, disposing of this token closes the accept socket
        public void Dispose()
        {
            try
            {
                OwnerSocket.Shutdown(SocketShutdown.Both);
            }
            catch
            {
                //Nothing to do here, connection is closed already
            }
            finally
            {
                OwnerSocket.Close();
            }
        }
    }
}