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
    [Serializable]
    public struct LogUserInfo
    {
        public string _nickname;
        public DateTime _dateConnect;
        public DateTime _dateDisconnect;
        public List<string> _exchangeRates;
        public LogUserInfo()
        {
            _nickname = string.Empty;
            _dateConnect = DateTime.MinValue;
            _dateDisconnect = DateTime.MinValue;
            _exchangeRates = new List<string>();
        }
    }
    internal class Program
    {
        Dictionary<string, double> _exchangeRates;
        List<User>? _users;
        Dictionary<TcpClient, LogUserInfo> _logs;

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

                    if (await UserAuthorizationRequest(tcpClient))
                    {
                        Task.Run(async () => await ProcessClientAsync(tcpClient));
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
                        LogUserInfo logUserInfo = _logs[tcpClient];
                        logUserInfo._dateDisconnect = DateTime.Now;
                        _logs[tcpClient] = logUserInfo;

                        SaveUserLog(ref logUserInfo);

                        _logs.Remove(tcpClient);

                        stream.Close();
                        tcpClient.Close();

                        break;
                    }

                    string message = str + " " + _exchangeRates[str].ToString();
                    byte[] dataBytes = Encoding.UTF8.GetBytes(message);
                    await stream.WriteAsync(dataBytes);

                    _logs[tcpClient]._exchangeRates.Add(message);
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

                Console.WriteLine(user.Nickname + ": " + tcpClient.Client.RemoteEndPoint?.ToString());

                LogUserInfo logUserInfo = new LogUserInfo();
                logUserInfo._nickname = user.Nickname;
                logUserInfo._dateConnect = DateTime.Now;

                _logs.Add(tcpClient, logUserInfo);

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
        void SaveUserLog(ref LogUserInfo logUserInfo)
        {
            string path = @"..\..\..\logUsers.txt";
            XmlSerializer xmlSerializer = new XmlSerializer(typeof(LogUserInfo));

            using (FileStream fs = new FileStream(path, FileMode.Append))
            {
                using (StreamWriter sw = new StreamWriter(fs))
                {
                    sw.WriteLine("Nickname: {0}", logUserInfo._nickname);
                    sw.WriteLine("Connect: {0}", logUserInfo._dateConnect);
                    sw.WriteLine("Disconnect: {0}", logUserInfo._dateDisconnect);
                    sw.WriteLine("Currency:");

                    foreach (var item in logUserInfo._exchangeRates)
                    {
                        sw.WriteLine(item);
                    }

                    sw.WriteLine();
                }
            }
        }
    }
}