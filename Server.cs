using System.Net.Sockets;
using System.Net;
using System.Text;

namespace STUN
{
    public class Server
    {
        public Dictionary<int, SocketHelper>? Clients;

        Thread? TCPThread;
        Thread? UDPThread;
        TcpListener? TCPListener = null;
        UdpClient? UDPClient = null;
        Random rnd = null;

        object lockname = new();
		CancellationTokenSource myCancelSource = new CancellationTokenSource();

		/// <summary>
		/// Starts the server.
		/// </summary>
		public void StartServer(int tcpPort)
        {
            //Creates a client list.
            Clients = new Dictionary<int, SocketHelper>();
            rnd = new Random();

            //Starts a listen thread to listen for connections.
            TCPThread = new Thread(new ThreadStart(delegate
            {
                Listen(tcpPort,myCancelSource.Token);
            }));
            TCPThread.Start();
            Console.WriteLine("TCP Listen thread started.");

            UDPThread = new Thread(new ThreadStart(delegate
            {
                ListenUDP(tcpPort, myCancelSource.Token);    
            }));
            UDPThread.Start();
            Console.WriteLine("UDP Listen thread started.");
        }

        /// <summary>
        /// Stops the server from running.
        /// </summary>
        public void StopServer()
        {
            myCancelSource.Cancel();

            if (Clients != null) {
                foreach (KeyValuePair<int, SocketHelper> entry in Clients) { 
                    SocketHelper currentClient = entry.Value;
                    currentClient.MscClient?.GetStream()?.Close();  
                    currentClient.MscClient?.Close();
                    currentClient.DisconnectClient();
                }
            }

            Clients?.Clear();
        }

        /// <summary>
        /// Listens for clients and starts threads to handle them.
        /// </summary>
        private void Listen(int port, CancellationToken myToken)
        {
            TCPListener = new TcpListener(IPAddress.Any, port);
            TCPListener.Start();

            while (!myToken.IsCancellationRequested)
            {
                Thread.Sleep(10);
                TcpClient tcpClient = TCPListener.AcceptTcpClient();
                Console.WriteLine("\nNew client detected. Connecting client...");
                SocketHelper helper = new SocketHelper();

                int PropId = rnd.Next(0, 4096);
                while (Clients.ContainsKey(PropId)) {
					PropId = rnd.Next(0, 4096);
				}

                helper.StartClient(tcpClient, this, PropId);
                Clients?.Add(helper.ClientId,helper);
            }
            Console.WriteLine("Listen Thread has been cancelled on main server!");
        }

		/// <summary>
		/// Listens for UDP Datagrams and sets proper UDP port to client.
		/// </summary>
		private void ListenUDP(int port, CancellationToken myToken)
		{
			UDPClient = new UdpClient(port);
            IPEndPoint groupEp = new IPEndPoint(IPAddress.Any, port);
			
			while (!myToken.IsCancellationRequested)
			{
                Thread.Sleep(10);
				Console.WriteLine("Waiting for UDP Identification ...");

                IPEndPoint anyIP = new IPEndPoint(IPAddress.Any, port);
                UDPClient.Receive(ref anyIP);

                if (Clients != null){              
					foreach (KeyValuePair<int, SocketHelper> entry in Clients)
					{
						SocketHelper client = entry.Value;

						if (client.ClientIPAddress == anyIP.Address.ToString())
						{
							client.ClientUDPPort = anyIP.Port.ToString();

							Console.WriteLine($"Recieved UDP Data from client: {client.ClientId}. " +
								$"\nThe TCP Port of Client is: {client.ClientPort}. " +
								$"\nThe UDP Port of Client is: {client.ClientUDPPort} ");

							BufferStream buffer = new(NetworkConfig.BufferSize, NetworkConfig.BufferAlignment);
							buffer.Seek(0);
							buffer.Write((UInt16)0);
							buffer.Write((UInt16)InterfaceTCPMessageType.StationUDPDataReport);
							buffer.Write(client.ClientUDPPort);
							client.SendMessage(buffer);							
							break;
						}
					}
				}               
			}
			Console.WriteLine("UDP Listen Thread has been cancelled on main server!");
		}
    }
}