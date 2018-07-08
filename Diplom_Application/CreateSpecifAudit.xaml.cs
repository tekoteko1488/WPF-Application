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
using Microsoft.SqlServer.Management.Common;
using Microsoft.SqlServer.Management.Smo;
using System.Data;
using System.Data.SqlClient;

namespace Diplom_Application
{
    /// <summary>
    /// Логика взаимодействия для CreateSpecifAudit.xaml
    /// </summary>
    public partial class CreateSpecifAudit : Window
    {
        Server server;
        List<string> listAudits = new List<string>();

        public CreateSpecifAudit(Server _server, List<string> _listAudits)
        {
            InitializeComponent();

            server = _server;

            listAudits = _listAudits;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            foreach(string x in listAudits)
            {
                comboBox_Audit.Items.Add(x);
            }

            listBox_Specific.Items.Add("APPLICATION_ROLE_CHANGE_PASSWORD_GROUP");
            listBox_Specific.Items.Add("AUDIT_CHANGE_GROUP");
            listBox_Specific.Items.Add("BACKUP_RESTORE_GROUP");
            listBox_Specific.Items.Add("BROKER_LOGIN_GROUP");
            listBox_Specific.Items.Add("DATABASE_CHANGE_GROUP");
            listBox_Specific.Items.Add("DATABASE_LOGOUT_GROUP");
            listBox_Specific.Items.Add("DATABASE_OBJECT_ACCESS_GROUP");
            listBox_Specific.Items.Add("DATABASE_OBJECT_PERMISSION_CHANGE_GROUP");
            listBox_Specific.Items.Add("FAILED_LOGIN_GROUP");
            listBox_Specific.Items.Add("FAILED_DATABASE_AUTHENTICATION_GROUP");
            listBox_Specific.Items.Add("LOGOUT_GROUP");
            listBox_Specific.Items.Add("USER_CHANGE_PASSWORD_GROUP");
            listBox_Specific.Items.Add("SUCCESSFUL_LOGIN_GROUP");
            listBox_Specific.Items.Add("TRANSACTION_GROUP");
            listBox_Specific.Items.Add("USER_DEFINED_AUDIT_GROUP");
            listBox_Specific.Items.Add("SUCCESSFUL_DATABASE_AUTHENTICATION_GROUP");
            listBox_Specific.Items.Add("TRACE_CHANGE_GROUP");
            listBox_Specific.Items.Add("SERVER_OPERATION_GROUP");
            listBox_Specific.Items.Add("SERVER_PERMISSION_CHANGE_GROUP");
            listBox_Specific.Items.Add("SERVER_PRINCIPAL_CHANGE_GROUP");
            listBox_Specific.Items.Add("FULLTEXT_GROUP");
            listBox_Specific.Items.Add("DATABASE_ROLE_MEMBER_CHANGE_GROUP");
            listBox_Specific.Items.Add("DATABASE_OWNERSHIP_CHANGE_GROUP");
            listBox_Specific.Items.Add("DATABASE_MIRRORING_LOGIN_GROUP");
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SqlCommand cmd;

            if(textBox_NameOfSpecifAudit.Text.Length == 0)
            {
                MessageBox.Show("Введите имя спецификации аудита");

            } else if(listBox_Specific.SelectedItems.Count == 0)
            {
                MessageBox.Show("Выберите спецификацию");

            } else if(comboBox_Audit.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите аудит");

            } else
            {
                string sqlCommand = $"create server audit specification [{textBox_NameOfSpecifAudit.Text}] " +
                    $"for server audit [{comboBox_Audit.SelectedItem.ToString()}] ";

                string sqlCommandStateOn = $"alter server audit specification [{textBox_NameOfSpecifAudit.Text}] " +
                    $"with(state=on)";

                for (int i = 0; i < listBox_Specific.SelectedItems.Count; i++)
                {
                    sqlCommand += $"add ({listBox_Specific.SelectedItems[i]})";
                    if (i < listBox_Specific.SelectedItems.Count - 1)
                    {
                        sqlCommand += ",";
                    }
                }

                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    try
                    {
                        cnn.Open();

                        cmd = new SqlCommand(sqlCommand, cnn);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand(sqlCommandStateOn, cnn);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show($"Спецификация {textBox_NameOfSpecifAudit.Text} успешно создана\n " +
                                $"и подключена к {comboBox_Audit.SelectedItem.ToString()}","Успешно",
                                MessageBoxButton.OK,MessageBoxImage.Information);

                    }catch(SqlException ex)
                    {
                        foreach(SqlError ob in ex.Errors)
                        {
                            if(ob.Number == 15530 && ob.State == 1)
                            {
                                MessageBox.Show("Спецификация аудита с таким именем уже существует",
                                    "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                            }

                            if (ob.Number == 33230 && ob.State == 1)
                            {
                                MessageBox.Show("Спецификация аудита для выбранного аудита уже существует",
                                    "Ошибка",MessageBoxButton.OK,MessageBoxImage.Error);
                            }
                        }
                    }
                }
            }
        }
    }
}
