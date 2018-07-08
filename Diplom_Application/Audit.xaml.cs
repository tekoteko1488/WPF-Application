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
    /// Логика взаимодействия для Audit.xaml
    /// </summary>
    public partial class Audit : Window
    {
        Server server;
        List<string> listAudits = new List<string>();

        public Audit(Server _server)
        {
            InitializeComponent();

            server = _server;
        }

        DefaultAudit ob = new DefaultAudit();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            using(SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand("use master; select name from sys.server_audits;",cnn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    string name = reader.GetString(0);
                    listAudits.Add(name);
                    comboBox_NameOfAudit.Items.Add(name);
                }

                reader.Close();
            }

            comboBox_AuditAvailability.Items.Add("Выключен");
            comboBox_AuditAvailability.Items.Add("Включен");

            comboBox_AuditError.Items.Add("Продолжить");
            comboBox_AuditError.Items.Add("Завершение работы сервера");
            comboBox_AuditError.Items.Add("Сбой операции");
        }

        private void comboBox_NameOfAudit_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            int? on_failure = null;
            bool is_state_enabled = false;
            //string type = null;
            int queueDelay = -1;

            if(comboBox_NameOfAudit.SelectedIndex != -1)
            {
                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    cnn.Open();

                    SqlCommand cmd = new SqlCommand(
                        "use master; " +
                        "select audit_id,audit_guid,create_date,modify_date,queue_delay,on_failure,is_state_enabled " +
                        "from sys.server_audits " +
                        $"where name = '{comboBox_NameOfAudit.SelectedItem.ToString()}'"
                        , cnn);

                    SqlDataReader reader = cmd.ExecuteReader();

                    while (reader.Read())
                    {
                        textBlock_AuditID.Text = reader.GetInt32(0).ToString();
                        textBlock_AuditGUID.Text = reader.GetGuid(1).ToString();
                        textBlock_AuditCreateDate.Text = reader.GetDateTime(2).ToString();
                        textBlock_AuditModifyDate.Text = reader.GetDateTime(3).ToString();
                        queueDelay = reader.GetInt32(4);
                        on_failure = reader.GetByte(5);
                        is_state_enabled = reader.GetBoolean(6);
                    }

                    textBox_QueueDelay.Text = queueDelay.ToString();
                    ob.queueDelay = queueDelay;

                    switch (is_state_enabled)
                    {
                        case false:
                            ob.is_state_enabled = 0;
                            comboBox_AuditAvailability.SelectedIndex = 0;
                            break;
                        case true:
                            ob.is_state_enabled = 1;
                            comboBox_AuditAvailability.SelectedIndex = 1;
                            break;
                    }

                    switch (on_failure)
                    {
                        case 0:
                            ob.on_failure = 0;
                            comboBox_AuditError.SelectedIndex = 0;
                            break;
                        case 1:
                            ob.on_failure = 1;
                            comboBox_AuditError.SelectedIndex = 1;
                            break;
                        case 2:
                            ob.on_failure = 2;
                            comboBox_AuditError.SelectedIndex = 2;
                            break;
                        default:
                            comboBox_AuditError.Items.Clear();
                            comboBox_AuditError.Items.Add("Данная настройка не указана");
                            comboBox_AuditError.SelectedIndex = 0;
                            break;
                    }

                    reader.Close();
                }                   
            }
        }

        //private void Button_Click(object sender, RoutedEventArgs e)
        //{

        //}

        private void Button_Click_1(object sender, RoutedEventArgs e) // Изменить Аудит 
        {
            if(comboBox_NameOfAudit.SelectedIndex != -1)
            {
                string auditErrorChange;

                switch (comboBox_AuditError.SelectedItem.ToString()) // Свич 1
                {
                    case "Продолжить":
                        auditErrorChange = "CONTINUE";
                        break;
                    case "Завершение работы сервера":
                        auditErrorChange = "SHUTDOWN";
                        break;
                    case "Сбой операции":
                        auditErrorChange = "FAIL_OPERATION";
                        break;
                    default:
                        MessageBox.Show("Ошибка в свиче 1");
                        auditErrorChange = "ERROR";
                        break;
                }

                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    SqlCommand cmd;
                    bool per_Change = false;

                    string sqlCommand = "use [master]; " +
                        $"alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}]";

                    cnn.Open();

                    if (ob.on_failure != comboBox_AuditError.SelectedIndex || ob.queueDelay != Int32.Parse(textBox_QueueDelay.Text))
                    {
                        sqlCommand += $" with(queue_delay = {textBox_QueueDelay.Text},on_failure = {auditErrorChange})";
                        per_Change = true;
                    }

                    if (per_Change == false && comboBox_AuditAvailability.SelectedIndex == 0 && ob.is_state_enabled == 1)
                    {
                        cmd = new SqlCommand("use master;" +
                               $" alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}] " +
                               $"with (STATE = off)", cnn);

                        cmd.ExecuteNonQuery();

                        ob.is_state_enabled = 0;

                    }
                    else if (per_Change == false && comboBox_AuditAvailability.SelectedIndex == 1 && ob.is_state_enabled == 0)
                    {
                        cmd = new SqlCommand("use master;" +
                               $" alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}] " +
                               $"with (STATE = on)", cnn);

                        cmd.ExecuteNonQuery();

                        ob.is_state_enabled = 1;

                    }
                    else if (per_Change == true && comboBox_AuditAvailability.SelectedIndex == 1 && ob.is_state_enabled == 0)
                    {
                        cmd = new SqlCommand(sqlCommand, cnn);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("use master;" +
                               $" alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}] " +
                               $"with (STATE = on)", cnn);

                        cmd.ExecuteNonQuery();

                        ob.is_state_enabled = 1;

                    }
                    else if (per_Change == true && comboBox_AuditAvailability.SelectedIndex == 0 && ob.is_state_enabled == 1)
                    {
                        cmd = new SqlCommand("use master;" +
                               $" alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}] " +
                               $"with (STATE = off)", cnn);

                        cmd.ExecuteNonQuery();

                        ob.is_state_enabled = 0;

                        cmd = new SqlCommand(sqlCommand, cnn);

                        cmd.ExecuteNonQuery();

                    }
                    else if (per_Change == true && comboBox_AuditAvailability.SelectedIndex == 0 && ob.is_state_enabled == 0)
                    {
                        cmd = new SqlCommand(sqlCommand, cnn);

                        cmd.ExecuteNonQuery();

                    }
                    else if (per_Change == true && comboBox_AuditAvailability.SelectedIndex == 1 && ob.is_state_enabled == 1)
                    {
                        cmd = new SqlCommand("use master;" +
                               $" alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}] " +
                               $"with (STATE = off)", cnn);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand(sqlCommand, cnn);

                        cmd.ExecuteNonQuery();

                        cmd = new SqlCommand("use master;" +
                               $" alter server audit [{comboBox_NameOfAudit.SelectedItem.ToString()}] " +
                               $"with (STATE = on)", cnn);

                        cmd.ExecuteNonQuery();
                    }
                }

                ob.on_failure = comboBox_AuditError.SelectedIndex;
                ob.queueDelay = Int32.Parse(textBox_QueueDelay.Text);
                ob.is_state_enabled = comboBox_AuditAvailability.SelectedIndex;

                label_AuditAvailability.Foreground = Brushes.Black;
                label_AuditError.Foreground = Brushes.Black;
                label_QueueDelay.Foreground = Brushes.Black;

            } else
            {
                MessageBox.Show("Выберите аудит, который Вы хотите изменить", "Выберите аудит",
                    MessageBoxButton.OK, MessageBoxImage.Stop);
            }

        }

            private void Button_Click_2(object sender, RoutedEventArgs e) // Откат изменений
        {
            if(comboBox_NameOfAudit.SelectedIndex != -1)
            {
                label_AuditAvailability.Foreground = Brushes.Black;
                label_AuditError.Foreground = Brushes.Black;
                label_QueueDelay.Foreground = Brushes.Black;

                comboBox_AuditAvailability.SelectedIndex = ob.is_state_enabled;
                comboBox_AuditError.SelectedIndex = ob.on_failure;

                textBox_QueueDelay.Text = Convert.ToString(ob.queueDelay);
            }
        }

        private void comboBox_AuditAvailability_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            label_AuditAvailability.Foreground = Brushes.Red;

            if(ob.is_state_enabled == comboBox_AuditAvailability.SelectedIndex)
            {
                label_AuditAvailability.Foreground = Brushes.Black;
            }

        }

        private void comboBox_AuditError_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            label_AuditError.Foreground = Brushes.Red;

            if (ob.on_failure == comboBox_AuditError.SelectedIndex)
            {
                label_AuditError.Foreground = Brushes.Black;
            }
        }

        private void comboBox_AuditType_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {

        }

        private void textBox_QueueDelay_KeyUp(object sender, KeyEventArgs e)
        {
            label_QueueDelay.Foreground = Brushes.Red;

            if (ob.queueDelay == Int32.Parse(textBox_QueueDelay.Text))
            {
                label_QueueDelay.Foreground = Brushes.Black;
            }
        }

        private void button_DataFromAudit_Click(object sender, RoutedEventArgs e)
        {
            if(comboBox_NameOfAudit.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите аудит, данные которого, Вы хотите узнать","Выберите аудит",
                    MessageBoxButton.OK,MessageBoxImage.Stop);

            } else
            {
                AuditData auditForm = new AuditData(server, comboBox_NameOfAudit.SelectedItem.ToString());

                auditForm.Show();
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            CreateAudit createForm = new CreateAudit(server);

            createForm.Show();
        }

        private void Button_Click_3(object sender, RoutedEventArgs e)
        {
            comboBox_NameOfAudit.Items.Clear();

            listAudits.Clear();

            using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand("use master; select name from sys.server_audits;", cnn);

                SqlDataReader reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    listAudits.Add(reader.GetString(0));
                }

                reader.Close();

                comboBox_NameOfAudit.ItemsSource = listAudits;
            }
        }

        private void Button_Click_4(object sender, RoutedEventArgs e)
        {
            CreateSpecifAudit specifAuditForm = new CreateSpecifAudit(server, listAudits);

            specifAuditForm.Show();
        }
    }
}
