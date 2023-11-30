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
        NetworkStream _stream;
        Commands _getLogin;
        Commands _getSend;

        public AppVM(MainWindow view) 
        {
            _viewMainWnd = view;

            _user = new User();

            _getLogin = new Commands(CreateWndUserEntrance);
            _getSend = new Commands(Send);

            //ConnectToServer();
        }
        public Commands GetLogin { get { return _getLogin; } }
        public Commands GetSend { get { return _getSend; } }
        private void CreateWndUserEntrance(object param)
        {
            UserEntrance userEntrance = new UserEntrance(this, _user);
            userEntrance.Show();
        }
        private void Send(object param)
        {
            //MessageBox.Show(_user.Nickname + " " + _user.Password);
        }
        public void CheckingUser()
        {
            //MessageBox.Show(_user.Nickname + " " + _user.Password);
        }
        private async void ConnectToServer()
        {
            try
            {
                using TcpClient tcpClient = new TcpClient();
                await tcpClient.ConnectAsync("127.0.0.1", 8080);
                _stream = tcpClient.GetStream();
            }
            catch (SocketException ex)
            {
                MessageBox.Show(ex.ToString());
            }
        }
    }
}
