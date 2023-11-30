using System.Net;
using System.Net.Sockets;
using System.Text;

namespace Server_TcpListener
{
    internal class Program
    {
        Dictionary<string, double> _exchangeRates;

        public Program()
        {
            _exchangeRates = new Dictionary<string, double>();

            _exchangeRates.Add("USD/EUR", 0.91);
            _exchangeRates.Add("EUR/USD", 1.08);
            _exchangeRates.Add("USD/UAH", 37.25);
            _exchangeRates.Add("UAH/USD", 0.03);
            _exchangeRates.Add("UAH/EUR", 0.02);
            _exchangeRates.Add("EUR/UAH", 40.55);
            _exchangeRates.Add("USD/PLN", 3.97);
            _exchangeRates.Add("PLN/USD", 0.24);
        }
        static async Task Main(string[] args)
        {
            Program program = new Program();

            await program.StartServer();
        }
        async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);

            try
            {
                listener.Start();
                Console.WriteLine("Server run. Wait for connection...");

                //while (true)
                //{
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    Console.WriteLine(tcpClient.Client.RemoteEndPoint?.ToString());

                    await Task.Run(async () => await ProcessClientAsync(tcpClient));
                //}
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        async Task ProcessClientAsync(TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();

            byte[] received = new byte[256];

            while (true) 
            {
                int countByte = await stream.ReadAsync(received);

                if (countByte == 0) continue;

                string str = Encoding.UTF8.GetString(received, 0, countByte);

                if (str.Contains("END"))
                {
                    stream.Close();
                    break;
                }

                string message = str + " " + _exchangeRates[str].ToString();
                byte[] dataBytes = Encoding.UTF8.GetBytes(message);
                await stream.WriteAsync(dataBytes);
            }
        }
    }
}