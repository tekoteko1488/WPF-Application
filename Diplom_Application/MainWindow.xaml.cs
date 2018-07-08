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
using System.Windows.Navigation;
using System.Windows.Shapes;
using System.Data;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Diplom_Application
{
    /// <summary>
    /// Логика взаимодействия для MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        public MainWindow(Server _server, int _rights)
        {
            InitializeComponent();

            server = _server;

            rights = _rights;
        }

        private string serverName = "DESKTOP-M3HMGRB";
        Server server;
        Database db;
        int rights;

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            Audit form = new Audit(server);

            form.Show();
        }

        private void Button_Click_2(object sender, RoutedEventArgs e)
        {
            if (rights == 2 || rights == 3) //Проверка на уровень доступа
            {
                TDE tDEForm = new TDE(server);

                tDEForm.Show();

            }
            else
            {
                MessageBox.Show("У Вас нет прав для получения доступа к данному разделу",
                    "Ограничение доступа",
                    MessageBoxButton.OK, MessageBoxImage.Stop);

            }
        }
    }
}
