using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Shapes;

namespace TCP_Listener_Client_Exchange_Rates
{
    /// <summary>
    /// Логика взаимодействия для UserEntrance.xaml
    /// </summary>
    public partial class UserEntrance : Window
    {
        public UserEntrance(AppVM appVM, User user)
        {
            InitializeComponent();

            DataContext = new UserVM(this, appVM, user);
        }
    }
}
