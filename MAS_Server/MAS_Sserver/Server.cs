using System;
using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Threading;
using System.Collections.Generic;


namespace MinerServer
{
    public class MinerData          // User data
    {
        public MinerData(string wn, string wi, string pf)
        {
            worker_name = wn;
            wallet_id = wi;
            preference = pf;
        }
        public string worker_name { get; set; }
        public string wallet_id { get; set; }
        public string preference { get; set; }

    }
    
    public class StateObject        // Object for async data transfer
    {
        public StringBuilder sb = new StringBuilder();
        public Socket workSocket = null;
        public const int BufferSize = 1024;
        public byte[] buffer = new byte[BufferSize];
    }

    public class AsynchronousSocketListener
    {
        const string ETH = "ETH";   // Ethereum 
        const string ETC = "ETC";   // Ethereum Classic
        const string NS = "NS";     // Not Selected

        public   List<MinerData> m_list = new List<MinerData>();
        public   ManualResetEvent allDone = new ManualResetEvent(false);
        public AsynchronousSocketListener(){}

        private   void SendCallback(IAsyncResult ar)
        {
            try
            {
                Socket handler = (Socket)ar.AsyncState; 
                int bytesSent = handler.EndSend(ar);
                Console.WriteLine("Sent {0} bytes to client.", bytesSent);

                handler.Shutdown(SocketShutdown.Both);
                handler.Close();

            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }

        private   void Send(Socket handler, String data)
        { 
            byte[] byteData = Encoding.ASCII.GetBytes(data);
            handler.BeginSend(byteData, 0, byteData.Length, 0, new AsyncCallback(SendCallback), handler);
        }

        public void ReadCallback(IAsyncResult ar)
        {
            String content = String.Empty;
            StateObject state = (StateObject)ar.AsyncState;
            Socket handler = state.workSocket;

            int bytesRead = handler.EndReceive(ar);

            if (bytesRead > 0)
            {
                
                state.sb.Append(Encoding.ASCII.GetString(state.buffer, 0, bytesRead));
                content = state.sb.ToString();
                System.Console.WriteLine(content.ToString() + '\n');

                string tmp_worker_name = content.Substring(content.IndexOf(":") + 1, content.IndexOf("-") - content.IndexOf(":") - 1);
                string tmp_wallet = content.Substring(content.IndexOf("-") + 1, content.Length - content.IndexOf("-") - 1);

                if (content.IndexOf("A:") > -1)
                {
                    int tmp_check = 0;
                    for (int iterator = 0; iterator < m_list.Count; iterator++)
                    {
                        if (m_list[iterator].wallet_id == tmp_wallet && tmp_check == 0)
                        {
                            if (m_list[iterator].worker_name == tmp_worker_name)
                                tmp_check = 2;
                            else
                            {
                                tmp_check = 1;
                                m_list[iterator].worker_name = tmp_worker_name;
                            }
                        }
                        else
                            tmp_check = 0;
                    }

                    switch (tmp_check)
                    {
                        case 0:
                            Send(handler, "User not found, please register-0");
                            break;
                        case 1:
                            Send(handler, "User found, Worker not found. Name was replaced-1");
                            break;
                        case 2:
                            Send(handler, "User found and Worker found! Mining alowed-1");
                            break;
                    }
                }
                else if (content.IndexOf("R:") > -1)
                {
                    int tmp_check = 0;
                    for (int iterator = 0; iterator < m_list.Count; iterator++)
                    {
                        if (m_list[iterator].wallet_id == tmp_wallet && tmp_check == 0)
                        {
                            if (m_list[iterator].worker_name == tmp_worker_name)
                                tmp_check = 2;
                            else
                            {
                                tmp_check = 1;
                                m_list[iterator].worker_name = tmp_worker_name;
                            }
                        }
                        else
                            tmp_check = 0;
                    }
                    switch (tmp_check)
                    {
                        case 0:
                            m_list.Add(new MinerData(tmp_worker_name, tmp_wallet, NS));
                            Send(handler, "User succefully registered-1");
                            break;
                        case 1:
                            Send(handler, "User found, Worker not found. Name was replaced-1");
                            break;
                        //case 2:
                        //    Send(handler, "User found and Worker found! Mining alowed-1");
                        //    break;
                    }
                }
                else if (content.IndexOf("С:") > -1)
                {
                    string tmp_choise = content.Substring(content.IndexOf("=") + 1, content.Length - content.IndexOf("=") - 1);
                    switch (tmp_choise)
                    {
                        case "ETH":
                            Send(handler, "set to ETH");
                            break;
                        case "ETC":
                            Send(handler, "set to ETC");
                            break;
                    }
                }
                else if (content.IndexOf("D:") > -1)
                {
                    int tmp_check = 0;
                    for (int iterator = 0; iterator < m_list.Count; iterator++)
                    {
                        if (m_list[iterator].wallet_id == tmp_wallet && tmp_check != 1)
                        {
                            m_list.RemoveAt(iterator);
                            tmp_check = 1;
                        }
                        else
                            tmp_check = 0;
                    }

                    switch (tmp_check)
                    {
                        case 0:
                            Send(handler, "User not found, please try again");
                            break;
                        case 1:
                            Send(handler, "User deleted-0");
                            break;
                    }
                }
            }
        }

        public   void AcceptCallback(IAsyncResult ar)
        {
            allDone.Set();
            Socket listener = (Socket)ar.AsyncState;
            Socket handler = listener.EndAccept(ar);

            StateObject state = new StateObject();
            state.workSocket = handler;
            handler.BeginReceive(state.buffer, 0, StateObject.BufferSize, 0, new AsyncCallback(ReadCallback), state);
        }

        public   void StartListening()
        {
            IPHostEntry ipHostInfo = Dns.GetHostEntry("127.0.0.1");
            IPAddress ipAddress = ipHostInfo.AddressList[0];
            IPEndPoint localEndPoint = new IPEndPoint(ipAddress, 11000);

            Console.WriteLine(Dns.GetHostName());
            Socket listener = new Socket(ipAddress.AddressFamily, SocketType.Stream, ProtocolType.Tcp);

            try
            {
                listener.Bind(localEndPoint);
                listener.Listen(100);

                while (true)
                {
                    allDone.Reset();
                    Console.WriteLine("Awaiting for a connection");

                    listener.BeginAccept(new AsyncCallback(AcceptCallback), listener);
                    allDone.WaitOne();

                    
                    for (int iterator = 0; iterator < m_list.Count; iterator++)
                    {
                        Console.WriteLine("elem: {0}, Worker: {1}, Wallet: {2}", iterator, m_list[iterator].worker_name, m_list[iterator].wallet_id);
                    }
                }
            }
            catch (Exception e)
            {
                Console.WriteLine(e.ToString());
            }
        }      

        public static int Main(String[] args)
        {
            AsynchronousSocketListener my_listener = new AsynchronousSocketListener();
            my_listener.StartListening();
            return 0;
        }
    }
}