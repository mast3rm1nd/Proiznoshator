using System;
using System.Net.Sockets;
using System.Net;


using System.Diagnostics;

namespace Client
{
    class Client
    {
        private Socket _clientSocket;
        private const int _PORT = 100;
        public bool IsConnected { get; set; } = false;

        private const int _BUFFER_SIZE = 4096;

        private static readonly byte[] _buffer = new byte[_BUFFER_SIZE];


        //protected override void OnClosing(CancelEventArgs e)
        //{
        //    base.OnClosing(e);

        //    if (_clientSocket != null && _clientSocket.Connected)
        //    {
        //        _clientSocket.Close();
        //    }
        //}

        public void Connect(string serverIp)
        {
            try
            {
                _clientSocket = new Socket(AddressFamily.InterNetwork, SocketType.Stream, ProtocolType.Tcp);
                // Connect to the local host
                _clientSocket.BeginConnect(new IPEndPoint(IPAddress.Parse(serverIp), _PORT), new AsyncCallback(ConnectCallback), null);
                //return true;
            }
            catch (Exception ex)
            {
                //return false;
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        private void ConnectCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndConnect(AR);
                _clientSocket.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, _clientSocket);
                IsConnected = true;
                Debug.WriteLine("Client: connected");
                //UpdateControlStates(true);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
            }
        }

        /// <summary>
        /// A thread safe way to enable the send button
        /// </summary>
        //private void UpdateControlStates(bool toggle)
        //{
        //    MethodInvoker invoker = new MethodInvoker(delegate
        //    {
        //        btnSend.Enabled = toggle;
        //        btnConnect.Enabled = !toggle;
        //        lblIP.Visible = txtAddress.Visible = !toggle;
        //    });

        //    this.Invoke(invoker);
        //}

        public void Send(byte[] byteArr)
        {
            Debug.WriteLine("Client: trying to send");
            try
            {
                // Serialize the textBoxes text before sending

                //PersonPackage person = new PersonPackage(chkMale.Checked, (ushort)nudAge.Value, txtEmployee.Text);

                //byte[] buffer = person.ToByteArray();
                _clientSocket.BeginSend(byteArr, 0, byteArr.Length, SocketFlags.None, new AsyncCallback(SendCallback), null);
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                //UpdateControlStates(false);
                Debug.WriteLine("Client: send failed at beginning");
            }
        }

        private void SendCallback(IAsyncResult AR)
        {
            try
            {
                _clientSocket.EndSend(AR);
                Debug.WriteLine("Client: sending done");
            }
            catch (Exception ex)
            {
                //MessageBox.Show(ex.Message, Application.ProductName, MessageBoxButtons.OK, MessageBoxIcon.Error);
                Debug.WriteLine("Client: sending failed at ending");
            }
        }


        public void CloseConnection()
        {
            Debug.WriteLine("Client: connection closed");
            if (_clientSocket != null && _clientSocket.Connected)
            {
                _clientSocket.Close();
            }
        }



        private static void ReceiveCallback(IAsyncResult AR)
        {
            Socket current = (Socket)AR.AsyncState;
            int received;

            try
            {
                Debug.WriteLine("Client: receive callback");
                received = current.EndReceive(AR);
            }
            catch (SocketException)
            {
                Debug.WriteLine("Client: receive callback failed");
                current.Close(); // Dont shutdown because the socket may be disposed and its disconnected anyway
                //_clientSockets.Remove(current);
                return;
            }

            byte[] recBuf = new byte[received];
            Array.Copy(_buffer, recBuf, received);

            //string text = Encoding.UTF8.GetString(recBuf);

            var receivedVoice = new VoicePackage(recBuf); // TESTING

            //MessageBox.Show("client : " + receivedVoice.Text);
            Debug.WriteLine("Client: received text: " + receivedVoice.Text);

            Globals.messagesQue.Add(receivedVoice);

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
                Debug.WriteLine("Client: try begin receive");
                current.BeginReceive(_buffer, 0, _BUFFER_SIZE, SocketFlags.None, ReceiveCallback, current);
            }
            catch
            {
                Debug.WriteLine("Client: try begin receive failed");
            }
            
        }
    }
}
