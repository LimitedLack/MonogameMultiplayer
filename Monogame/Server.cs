using System;
using System.Collections.Generic;
using System.Text;
using System.Threading.Tasks;
using System.Net.Sockets;
using System.Net;

namespace Monogame
{
    class Server
    {

        static Dictionary<int, TcpClient> clients = new Dictionary<int, TcpClient>();
        
        public static void StartServer()
        {
            clients.Add(1, null);
            clients.Add(2, null);
            clients.Add(3, null);

            Console.SetWindowSize(70, 17);

            TcpListener listener = new TcpListener(IPAddress.Parse("192.168.56.1"), 8778);
            listener.Start();

            Console.WriteLine("Opened Listener at " + listener.LocalEndpoint + ".");

            while (true)
            {
                var client = listener.AcceptTcpClient();
                NetworkStream clientStream = client.GetStream();

                byte[] buffer = new byte[64];
                int count = clientStream.Read(buffer);

                int loginSelection = 0;

                for (int i = 0; i < count; i++)
                {


                    if (Convert.ToChar(buffer[i]) == '1')
                    {
                        clients[1] = client;
                        loginSelection = 1;
                    }
                    else if (Convert.ToChar(buffer[i]) == '2')
                    {
                        clients[2] = client;
                        loginSelection = 2;
                    }
                    else if (Convert.ToChar(buffer[i]) == '3')
                    {
                        clients[3] = client;
                        loginSelection = 3;
                    }
                }

                Task.Run(() => HandleClient(client.GetStream(), loginSelection));
            }
        }

        private static void HandleClient(NetworkStream stream, int clientNum)
        {
            Console.WriteLine("Accepted client #" + clientNum);
            ASCIIEncoding asen = new ASCIIEncoding();

            stream.Write(asen.GetBytes(clientNum.ToString()));


            while (true)
            {
                byte[] readBuffer = new byte[100];

                int readBufferCount = stream.Read(readBuffer);

                string bufferString = "";

                // Print buffer to server console
                for (int i = 0; i < readBufferCount; i++)
                {
                    bufferString += Convert.ToChar(readBuffer[i]);
                }

                // Relay buffer to all clients
                foreach (var item in clients)
                {
                    try
                    {
                        item.Value.GetStream().Write(asen.GetBytes(clientNum.ToString() + bufferString));
                    }
                    catch
                    {
                    }
                }
            }
        }
    }
}
