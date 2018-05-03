using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Threading.Tasks;
using UnityEngine;

namespace SpaceStation
{
    public class Class1 : MonoBehaviour
    {
        public String host = "127.0.0.1";
        public Int32 port = 6666;

        internal Boolean socket_ready = false;
        internal String input_buffer = "";
        TcpClient tcp_socket;
        NetworkStream net_stream;

        StreamWriter socket_writer;
        StreamReader socket_reader;

        void Update()
        {
            string received_data = readSocket();
            string key_stroke = Input.inputString;
            
            // Collects keystrokes into a buffer
            if (key_stroke != "")
            {
                input_buffer += key_stroke;

                if (key_stroke == "\n")
                {
                    // Send the buffer, clean it
                    Debug.Log("Sending: " + input_buffer);
                    writeSocket(input_buffer);
                    input_buffer = "";
                }
                
            }

            if (received_data != "")
            {
                // Do something with the received data,
                // print it in the log for now
                Debug.Log(received_data);
            }
        }

        private void FixedUpdate()
        {

        }

        private void LateUpdate()
        {

        }

        private void Reset()
        {

        }

        private void Start() => Task.Factory.StartNew(setupSocket);

        void Awake() { }

        void OnApplicationQuit() => closeSocket();

        public void setupSocket()
        {
            
            try
            {
                tcp_socket = new TcpClient("127.0.0.1", 6666);

                net_stream = tcp_socket.GetStream();
                socket_writer = new StreamWriter(net_stream);
                socket_reader = new StreamReader(net_stream);

                socket_ready = true;
            }
            catch (Exception e)
            {
                // Something went wrong
                Debug.Log("Socket error: " + e);
            }
        }

        public void writeSocket(string line)
        {
            if (!socket_ready)
                return;

            line = line + "\r\n";
            socket_writer.Write(line);
            socket_writer.Flush();
        }

        public String readSocket()
        {
            if (!socket_ready)
                return "";

            if (net_stream.DataAvailable)
                return socket_reader.ReadLine();

            return "";
        }

        public void closeSocket()
        {
            if (!socket_ready)
                return;

            socket_writer.Close();
            socket_reader.Close();
            tcp_socket.Close();
            socket_ready = false;
        }
    }
}
