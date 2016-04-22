using System;
using System.Collections.Generic;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Windows;

using System.Diagnostics;

namespace Proiznoshator
{
    class Server
    {
        private static Socket _serverSocket;
        private static readonly List<Socket> _clientSockets = new List<Socket>();
        private const int _BUFFER_SIZE = 4096;
        private const int _PORT = 100;
        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];

        public void SetupServer()
        {
            _serverSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
            _serverSocket.Bind(new IPEndPoint(IPAddress.Any, _PORT));
            _serverSocket.Listen(10);
            _serverSocket.BeginAccept(AcceptCallback, null);

            Debug.WriteLine("Server started");
        }

        /// <summary>
        /// Close all connected client (we do not need to shutdown the server socket as its connections
        /// are already closed with the clients)
        /// </summary>
        public void CloseAllSockets()
        {
            foreach (Socket socket in _clientSockets)
            {
                socket.Shutdown(SocketShutdown.Both);
                socket.Close();
            }

            if(_serverSocket != null)
                _serverSocket.Close();

            Debug.WriteLine("Server sockets closed");
        }

        private  void AcceptCallback(IAsyncResult AR)
        {
            Socket socket;

            try
            {
                socket = _serverSocket.EndAccept(AR);
            }
            catch (ObjectDisposedException) // I cannot seem to avoid this (on exit when properly closing sockets)
            {
                return;
            }

            _clientSockets.Add(socket);
            Debug.WriteLine("Server: someone connected");
            socket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, socket);
            _serverSocket.BeginAccept(AcceptCallback, null);
        }

        private  void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                Debug.WriteLine("Server: trying receiving callback");
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Debug.WriteLine("Server: trying receiving callback failed");
                current.Close(); // Dont shutdown because the socket may be disposed and its disconnected anyway
                _clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            var receivedVoice = new VoicePackage(recBuf);

            Debug.WriteLine("server : " + receivedVoice.Text);

            Globals.messagesQue.Add(receivedVoice);

            if(Globals.IsServer)
            {
                Debug.WriteLine("Server: resending...");
                ResendToEveryClient(recBuf);
                Debug.WriteLine("Server: resending done...");
            }
                
            //Console.WriteLine("Received Text: " + text);

            //if (text.ToLower() == "get time") // Client requested time
            //{
            //    Console.WriteLine("Text is a get time request");
            //    byte[] data = Encoding.ASCII.GetBytes(DateTime.Now.ToLongTimeString());
            //    current.Send(data);
            //    Console.WriteLine("Time sent to client");
            //}
            //else if (text.ToLower() == "exit") // Client wants to exit gracefully
            //{
            //    // Always Shutdown before closing
            //    current.Shutdown(SocketShutdown.Both);
            //    current.Close();
            //    _clientSockets.Remove(current);
            //    Console.WriteLine("Client disconnected");
            //    return;
            //}
            //else
            //{
            //    Console.WriteLine("Text is an invalid request");
            //    byte[] data = Encoding.ASCII.GetBytes("Invalid request");
            //    current.Send(data);
            //    Console.WriteLine("Warning Sent");
            //}

            try
            {
                Debug.WriteLine("Server: trying begin receive");
                current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
            catch
            {
                Debug.WriteLine("Server: trying begin receive failed");
            }
            
        }

        public void ResendToEveryClient(byte[] data)
        {
            Debug.WriteLine("Server: resending started");
            foreach (Socket socket in _clientSockets)
            {
                socket.Send(data);
            }
            Debug.WriteLine("Server: resending ended");
        }
    }
}
