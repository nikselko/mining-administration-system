using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Text;



namespace MAS_Client
{
    public class StateObject
    {
        public Socket workSocket = null;
        public const int BufferSize = 256;
        public byte[] buffer = new byte[BufferSize];
        public StringBuilder sb = new StringBuilder();
    }

    public class AsynchronousClient
    {
        public String Worker_Name = String.Empty;
        public String Walled_ID = String.Empty;

        public int trust_level = 0;

        public string AUTH = "A:";
        public string REG = "R:";
        public string CHS = "C:";
        public string DEL = "D:";

        private string response = String.Empty;
        private const int port = 11000;

        private ManualResetEvent connectDone =  new ManualResetEvent(false);
        private ManualResetEvent sendDone = new ManualResetEvent(false);
        private ManualResetEvent receiveDone = new ManualResetEvent(false);

        private void ConnectCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                client.EndConnect(ar);
                connectDone.Set();
            }
            catch (Exception e)
            {
                response = e.ToString();
            }
        }

        private void Receive(Socket client)
        {
            try
            {
                StateObject state = new StateObject();
                state.workSocket = client;

                client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
            }
            catch (Exception e)
            {
                response = e.ToString();
            }
        }

        private void ReceiveCallback(IAsyncResult ar)
        {
            try
            {
                StateObject state = (StateObject)ar.AsyncState;
                Socket client = state.workSocket;

                int bytesRead = client.EndReceive(ar);
                if (bytesRead > 0)
                {
                    state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                    client.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReceiveCallback), state);
                }
                else
                {
                    if (state.sb.Length > 1)
                        response = state.sb.ToString();
                    receiveDone.Set();
                }
            }
            catch (Exception e)
            {
                response = e.ToString();
            }
        }

        private void Send(Socket client, String data)
        {
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            client.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), client);
        }

        private void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket client = (Socket)ar.AsyncState;
                int bytesSent = client.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to server.", bytesSent);
                sendDone.Set();
            }
            catch (Exception e)
            {
                response = e.ToString();
            }
        }

        public void SendCommand(string m_mode, string m_worker_name, string m_wallet_id, string currency)
        {
            try
            {
                IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
                IPAddress ipAddress = ipHostInfo.AddressList[0];
                IPEndPoint remoteEP = new IPEndPoint(ipAddress, port);
                Socket client = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

                client.BeginConnect(remoteEP, new AsyncCallback(ConnectCallback), client);
                connectDone.WaitOne();

                if (m_mode == "C:")
                    Send(client, m_mode + m_worker_name + '-' + m_wallet_id + '=' + currency);
                else
                    Send(client, m_mode + m_worker_name + '-' + m_wallet_id);
                sendDone.WaitOne();

                Receive(client);
                receiveDone.WaitOne();

                //client.Shutdown(SocketShutdown.Both);
                //client.Close();
            }
            catch (Exception e)
            {
                response = e.ToString();
            }
        }

        public string respond()
        {
            return response;
        }
    }
}