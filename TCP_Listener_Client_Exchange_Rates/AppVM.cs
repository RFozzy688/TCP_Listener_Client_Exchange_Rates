using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Reflection;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Threading;

namespace TCP_Listener_Client_Exchange_Rates
{
    public class AppVM
    {
        MainWindow _viewMainWnd;
        User _user;
        WorkWithServer _workWithServer;
        Commands _getLogin;
        Commands _getSend;
        Commands _getLogout;

        public AppVM(MainWindow view) 
        {
            _viewMainWnd = view;

            _user = new User();
            _workWithServer = new WorkWithServer();

            _getLogin = new Commands(CreateWndUserEntrance);
            _getSend = new Commands(Send);
            _getLogout = new Commands(UserLogout);
        }
        public Commands GetLogin { get { return _getLogin; } }
        public Commands GetSend { get { return _getSend; } }
        public Commands GetLogout { get { return _getLogout; } }
        public WorkWithServer GetWorkWithServer {  get { return _workWithServer; } }
        private void CreateWndUserEntrance(object param)
        {
            UserEntrance userEntrance = new UserEntrance(this, _user);
            userEntrance.Show();
        }
        private void UserLogout(object param)
        {
            _workWithServer.UserLogout();

            _viewMainWnd.LogIn.IsEnabled = true;
            _viewMainWnd.LogOut.IsEnabled = false;
            _viewMainWnd.Send.IsEnabled = false;

            _viewMainWnd.UserName.Text = string.Empty;
            _viewMainWnd.Result.Text = string.Empty;
        }
        private async void Send(object param)
        {
            string str = await _workWithServer.Send(_viewMainWnd.Currency.SelectionBoxItem.ToString());

            if (!str.Contains("Max requery count"))
            {
                _viewMainWnd.Result.Text = str;
            }
            else
            {
                MessageBox.Show(str);
                UserLogout(param);
            }
        }
        public void AuthorizationUser()
        {
            if (_user.Nickname == "" && _user.Password == "")
            {
                MessageBox.Show("Error authorization!!!");
                return;
            }

            _viewMainWnd.Dispatcher.Invoke(async () =>
            {
                if (await _workWithServer.AuthorizationUser(_user))
                {
                    _viewMainWnd.UserName.Text = _user.Nickname;

                    _viewMainWnd.LogIn.IsEnabled = false;
                    _viewMainWnd.LogOut.IsEnabled = true;
                    _viewMainWnd.Send.IsEnabled = true;
                }
                else
                {
                    MessageBox.Show("Error authorization!!!");
                }
            });
        }
    }
}
