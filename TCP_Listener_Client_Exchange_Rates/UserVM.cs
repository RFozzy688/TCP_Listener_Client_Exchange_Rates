using System;
using System.Collections.Generic;
using System.Configuration;
using System.IO;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Text.Json;
using System.Threading.Tasks;

namespace TCP_Listener_Client_Exchange_Rates
{
    public class UserVM
    {
        UserEntrance _viewUserEntrance;
        User _user;
        AppVM _appVM;
        Commands _getEnter;

        public UserVM(UserEntrance view, AppVM appVM, User user)
        {
            _viewUserEntrance = view;
            _user = user;
            _appVM = appVM;

            _getEnter = new Commands(GetUserData);
        }
        public Commands GetEnter { get { return _getEnter; } }
        private void GetUserData(object param)
        {
            _user.Nickname = _viewUserEntrance.Nickname.Text;
            _user.Password = _viewUserEntrance.Password.Text;

            _viewUserEntrance.Close();
            _appVM.AuthorizationUser();
        }
    }
}
