using System.Net;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Xml.Serialization;

namespace Server_TcpListener
{
    public class User : IComparable
    {
        public string Nickname { get; set; }
        public string Password { get; set; }
        public int CompareTo(object obj)
        {
            if (Nickname.CompareTo((obj as User).Nickname) == 0 && Password.CompareTo((obj as User).Password) == 0)
            {
                return 0;
            }
            else
            {
                return -1;
            }
        }
    }
    public class LogUserInfo
    {
        public string Nickname { get; set; }
        public DateTime DateConnect { get; set;}
        public DateTime DateDisconnect { get; set;}
        public List<string> ExchangeRates { get; set; }
        public LogUserInfo()
        {
            Nickname = string.Empty;
            DateConnect = DateTime.MinValue;
            DateDisconnect = DateTime.MinValue;
            ExchangeRates = new List<string>();
        }
    }
    internal class Program
    {
        Dictionary<string, double> _exchangeRates;
        List<User>? _users;
        Dictionary<TcpClient, LogUserInfo> _logs;
        object _lock = new object();
        const int _requeryMaxCount = 5;
        const int _connectionsMaxCount = 2;
        int _currentConnectionsCount = 0;

        public Program()
        {
            _exchangeRates = new Dictionary<string, double>();
            _users = new List<User>();
            _logs = new Dictionary<TcpClient, LogUserInfo>();

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

            program.LoadingDataUsers();
            await program.StartServer();
        }
        void LoadingDataUsers()
        {
            using (FileStream fs = new FileStream(@"..\..\..\users.json", FileMode.Open))
            {
                _users = JsonSerializer.Deserialize<List<User>>(fs);
            }
        }
        async Task StartServer()
        {
            TcpListener listener = new TcpListener(IPAddress.Parse("127.0.0.1"), 8080);

            try
            {
                listener.Start();
                Console.WriteLine("Server run. Wait for connection...");

                while (true)
                {
                    TcpClient tcpClient = await listener.AcceptTcpClientAsync();
                    NetworkStream stream = tcpClient.GetStream();

                    string msgError = null;
                    byte[] dataByte = new byte[256];

                    if (_currentConnectionsCount + 1 <= _connectionsMaxCount)
                    {
                        _currentConnectionsCount++;

                        msgError = "Good";
                        dataByte = Encoding.UTF8.GetBytes(msgError);
                        await stream.WriteAsync(dataByte);

                        if (await UserAuthorizationRequest(tcpClient))
                        {
                            Task.Run(async () => await ProcessClientAsync(tcpClient));
                        }
                    }
                    else
                    {
                        msgError = "Max count users";
                        dataByte = Encoding.UTF8.GetBytes(msgError);
                        await stream.WriteAsync(dataByte);

                        tcpClient.Close();
                        stream.Close();
                    }
                }
            }
            catch (SocketException ex)
            {
                Console.WriteLine(ex.ToString());
            }
        }
        async Task ProcessClientAsync(TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();
            int currentRequeryCount = 0;

            byte[] received = new byte[256];

            while (true) 
            {
                try
                {
                    int countByte = await stream.ReadAsync(received);

                    if (countByte == 0) continue;

                    string str = Encoding.UTF8.GetString(received, 0, countByte);

                    if (str.Contains("END"))
                    {
                        lock (_lock)
                        {
                            Console.WriteLine(_logs[tcpClient].Nickname + " disconnected: " + tcpClient.Client.RemoteEndPoint?.ToString());

                            _logs[tcpClient].DateDisconnect = DateTime.Now;

                            SaveUserLog(_logs[tcpClient]);

                            _logs.Remove(tcpClient);

                            _currentConnectionsCount--;
                        }

                        stream.Close();
                        tcpClient.Close();

                        break;
                    }

                    if (currentRequeryCount + 1 <= _requeryMaxCount)
                    {
                        currentRequeryCount++;

                        string message = str + " " + _exchangeRates[str].ToString();
                        byte[] dataBytes = Encoding.UTF8.GetBytes(message);
                        await stream.WriteAsync(dataBytes);

                        lock (_lock)
                        {
                            _logs[tcpClient].ExchangeRates.Add(message);
                        }
                    }
                    else
                    {
                        string message = "Max requery count";
                        byte[] dataBytes = Encoding.UTF8.GetBytes(message);
                        await stream.WriteAsync(dataBytes);
                    }
                }
                catch (IOException ex)
                {
                    Console.WriteLine(ex.ToString());
                    break;
                }
            }
        }
        bool CheckingUser(User? user)
        {
            if (user == null) return false;

            foreach (User item in _users)
            {
                if (item.CompareTo(user) == 0)
                {
                    return true;
                }
            }

            return false;
        }
        async Task<bool> UserAuthorizationRequest(TcpClient tcpClient)
        {
            NetworkStream stream = tcpClient.GetStream();

            byte[] received = new byte[256];
            int countByte = await stream.ReadAsync(received);
            string str = Encoding.UTF8.GetString(received, 0, countByte);

            User? user = JsonSerializer.Deserialize<User>(str);

            if (CheckingUser(user))
            {
                string msgAuthorization = "Authorization success";
                byte[] dataByte = Encoding.UTF8.GetBytes(msgAuthorization);
                await stream.WriteAsync(dataByte);

                Console.WriteLine(user.Nickname + " connected: " + tcpClient.Client.RemoteEndPoint?.ToString());

                LogUserInfo logUserInfo = new LogUserInfo();
                logUserInfo.Nickname = user.Nickname;
                logUserInfo.DateConnect = DateTime.Now;

                lock(_lock)
                {
                    _logs.Add(tcpClient, logUserInfo);
                }

                return true;
            }
            else
            {
                string msgError = "Error authorization!!!";
                byte[] dataByte = Encoding.UTF8.GetBytes(msgError);
                await stream.WriteAsync(dataByte);

                tcpClient.Client.Shutdown(SocketShutdown.Both);
                stream.Close();
                tcpClient.Close();

                return false;
            }
        }
        void SaveUserLog(LogUserInfo logUserInfo)
        {
            string path = @"..\..\..\logUsers.json";

            using (FileStream fs = new FileStream(path, FileMode.Append))
            {
                JsonSerializer.Serialize(fs, logUserInfo);
            }
        }
    }
}