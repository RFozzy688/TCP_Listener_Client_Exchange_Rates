using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace TCP_Listener_Client_Exchange_Rates
{
    public class AppVM
    {
        MainWindow _viewMainWnd;
        User _user;
        TcpClient _tcpClient;
        NetworkStream _stream;
        Commands _getLogin;
        Commands _getSend;
        Commands _getCurrency;

        public AppVM(MainWindow view) 
        {
            _viewMainWnd = view;

            _user = new User();

            _getLogin = new Commands(CreateWndUserEntrance);
            _getSend = new Commands(Send);

            ConnectToServer();
        }
        public Commands GetLogin { get { return _getLogin; } }
        public Commands GetSend { get { return _getSend; } }
        public Commands GetCurrency { get { return _getCurrency; } }
        private void CreateWndUserEntrance(object param)
        {
            UserEntrance userEntrance = new UserEntrance(this, _user);
            userEntrance.Show();
        }
        private async void Send(object param)
        {
            byte[] data = Encoding.UTF8.GetBytes(_viewMainWnd.Currency.SelectionBoxItem.ToString());
            await _stream.WriteAsync(data);

            byte[] response = new byte[256];
            await _stream.ReadAsync(response);

            _viewMainWnd.Result.Text = Encoding.UTF8.GetString(response);
        }
        public void CheckingUser()
        {
            //MessageBox.Show(_user.Nickname + " " + _user.Password);
        }
        private async void ConnectToServer()
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
    }
}
