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
using System.Data;
using System.Data.SqlClient;
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;

namespace Diplom_Application
{
    /// <summary>
    /// Логика взаимодействия для Login.xaml
    /// </summary>
    public partial class Login : Window
    {
        private string serverName = "DESKTOP-M3HMGRB";
        Server server;
        User user;

        public Login()
        {
            InitializeComponent();
        }

        private void button_Enter_Click(object sender, RoutedEventArgs e)
        {
            if(comboBox_Servers.SelectedIndex != -1)
            {
                server = Repository.ConnectionToServer(comboBox_Servers.SelectedItem.ToString());

                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    cnn.Open();

                    SqlCommand cmd = new SqlCommand("use ForAudit; select Surname, Name, Id_Rights from dbo.Persons " +
                        $"where Login = '{textBox_Login.Text}' and Password = '{passwordBox_Password.Password}'",cnn);

                    SqlDataReader reader = cmd.ExecuteReader();

                    if (reader.HasRows)
                    {
                        reader.Read();

                        user = new User()
                        {
                            Surname = reader.GetString(0),
                            Name = reader.GetString(1),
                            Id_Rights = reader.GetInt32(2)
                        };

                        reader.Close();

                        MainWindow mainWindowForm = new MainWindow(server,user.Id_Rights);

                        mainWindowForm.Show();

                        this.Close();

                    } else
                    {
                        MessageBox.Show("Логин или пароль введены неправильно","Неудачная аутентификация",
                            MessageBoxButton.OK, MessageBoxImage.Stop);
                    }
                }
            } else
            {
                MessageBox.Show("Вы не выбрали сервер","Выберите сервер",MessageBoxButton.OK,MessageBoxImage.Stop);
            }

        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox_Servers.Items.Add(serverName);
        }
    }
}
