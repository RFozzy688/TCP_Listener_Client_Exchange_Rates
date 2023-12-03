using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;

namespace TCP_Listener_Client_Exchange_Rates
{
    public class WorkWithServer
    {
        TcpClient? _tcpClient;
        NetworkStream? _stream;

        private async Task ConnectToServer()
        {
            try
            {
                _tcpClient = new TcpClient();
                await _tcpClient.ConnectAsync("127.0.0.1", 8080);
                _stream = _tcpClient.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
        public async Task<bool> AuthorizationUser(User user)
        {
            await ConnectToServer();

            if (await IsMaxCountUsers())
            {
                byte[] data = Encoding.UTF8.GetBytes(JsonSerializer.Serialize(user));
                await _stream.WriteAsync(data);

                byte[] response = new byte[256];

                int countByte = await _stream.ReadAsync(response);

                string str = Encoding.UTF8.GetString(response, 0, countByte);

                if (str.Contains("Authorization success"))
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                MessageBox.Show("Maximum number of users connected");
                return false;
            }
        }
        public async Task<string> Send(string currency)
        {
            byte[] data = Encoding.UTF8.GetBytes(currency);
            await _stream.WriteAsync(data);

            byte[] response = new byte[256];
            await _stream.ReadAsync(response);

            return Encoding.UTF8.GetString(response);
        }
        public async void UserLogout()
        {
            byte[] data = Encoding.UTF8.GetBytes("END");
            await _stream.WriteAsync(data);
        }
        private async Task<bool> IsMaxCountUsers()
        {
            byte[] response = new byte[256];
            int countByte = await _stream.ReadAsync(response);

            string str = Encoding.UTF8.GetString(response, 0, countByte);

            if (!str.Contains("Max count users"))
            {
                return true;
            }
            else 
            {
                return false;
            }
        }
    }
}

