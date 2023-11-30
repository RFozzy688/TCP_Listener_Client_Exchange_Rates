using System.Net;
using System.Net.Sockets;

namespace Server_TcpListener
{
    internal class Program
    {
        static async Task Main(string[] args)
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);

            try
            {
                listener.Start();
                Console.WriteLine("Server run. Wait for connection...");

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Console.WriteLine(tcpClient.Client.RemoteEndPoint?.ToString());
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
    }
}