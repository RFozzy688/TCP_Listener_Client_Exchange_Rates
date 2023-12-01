using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace TCP_Listener_Client_Exchange_Rates
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        AppVM _appVM;
        public MainWindow()
        {
            InitializeComponent();

            _appVM = new AppVM(this);
            DataContext = _appVM;
        }

        private void Window_Closed(object sender, EventArgs e)
        {
            _appVM.GetWorkWithServer.UserLogout();
        }
    }
}
