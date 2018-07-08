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
using System.Text.RegularExpressions;

namespace Diplom_Application
{
    /// <summary>
    /// Логика взаимодействия для TDE.xaml
    /// </summary>
    public partial class TDE : Window
    {
        Server server;

        public TDE(Server _server)
        {
            InitializeComponent();

            server = _server;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            textBlock_NameOfTheServer.Text = server.Name;

            using (SqlConnection cnn = new SqlConnection
                (server.ConnectionContext.ConnectionString))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand("SELECT name KeyName,symmetric_key_id KeyID FROM sys.symmetric_keys;", cnn);
                SqlDataReader reader = cmd.ExecuteReader();

                reader.Read();
                textBlock_DMK.Text = reader.GetString(0);
                textBlock_DMKID.Text = reader.GetInt32(1).ToString();
                reader.Read();
                textBlock_SMK.Text = reader.GetString(0);
                textBlock_SMKID.Text = reader.GetInt32(1).ToString();

                reader.Close();

                cmd = new SqlCommand("select name from sys.certificates",cnn);

                reader = cmd.ExecuteReader();

                List<string> listWithNameCerf = new List<string>();

                while (reader.Read())
                {
                    string nameCerf = reader.GetString(0);

                    switch (nameCerf)
                    {
                        case "##MS_AgentSigningCertificate##":
                            break;
                        case "##MS_PolicySigningCertificate##":
                            break;
                        case "##MS_SchemaSigningCertificate3AD2C1F412E64C5724F4EB805ED2334862F3FBD5##":
                            break;
                        case "##MS_SmoExtendedSigningCertificate##":
                            break;
                        case "##MS_SQLAuthenticatorCertificate##":
                            break;
                        case "##MS_SQLReplicationSigningCertificate##":
                            break;
                        case "##MS_SQLResourceSigningCertificate##":
                            break;
                        default:
                            listWithNameCerf.Add(nameCerf);
                            break;
                    }
                }

                reader.Close();

                comboBox_NameCerf.ItemsSource = listWithNameCerf;

            }

            foreach (Database x in server.Databases)
            {
                if (x.Name == "master" || x.Name == "model" || x.Name == "msdb" || x.Name == "tempdb")
                {

                }
                else comboBox_Database.Items.Add(x.Name);
            }
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            if(textBox_NameCerf.Text == "")
            {
                MessageBox.Show("Введите название сертификата");

            } else if(textBox_SubjectCerf.Text == "")
            {
                MessageBox.Show("Введите подпись сертификата");

            } else 
            {
                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    try
                    {
                        cnn.Open();

                        SqlCommand cmd = new SqlCommand(
                            $"create certificate {textBox_NameCerf.Text} with subject" +
                            $" = '{textBox_SubjectCerf.Text}';", cnn);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show($"Сертификат {textBox_NameCerf.Text} " +
                            $"с подписью {textBox_SubjectCerf.Text} успешно создан!");

                    } catch(SqlException ex)
                    {
                        foreach(SqlError ob in ex.Errors)
                        {
                            if(ob.Number == 15232 && ob.State == 1)
                            {
                                MessageBox.Show($"Сертификат с именем {textBox_NameCerf.Text} " +
                                    $"уже существует", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Error);
                            }
                        }
                    }
 
                }
            }
        }

        private void checkBox_AES128_Checked(object sender, RoutedEventArgs e)
        {
            
        }

        private void checkBox_AES128_Checked_1(object sender, RoutedEventArgs e)
        {
            if (checkBox_AES128.IsChecked == true)
            {
                checkBox_AES192.IsChecked = false;
                checkBox_AES256.IsChecked = false;
            }
        }

        private void checkBox_AES192_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox_AES192.IsChecked == true)
            {
                checkBox_AES128.IsChecked = false;
                checkBox_AES256.IsChecked = false;
            }
        }

        private void checkBox_AES256_Checked(object sender, RoutedEventArgs e)
        {
            if (checkBox_AES256.IsChecked == true)
            {
                checkBox_AES192.IsChecked = false;
                checkBox_AES128.IsChecked = false;
            }
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            if(comboBox_Database.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите базу данных, для которой нужно создать ключ");

            } else if(comboBox_NameCerf.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите сертификат");

            } else
            {
                string encryptorParametr;

                if (checkBox_AES128.IsChecked == true)
                {
                    encryptorParametr = checkBox_AES128.Content.ToString();
                }
                else if (checkBox_AES192.IsChecked == true)
                {
                    encryptorParametr = checkBox_AES192.Content.ToString();
                }
                else
                {
                    encryptorParametr = checkBox_AES256.Content.ToString();
                }

                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    try
                    {
                        cnn.Open();

                        SqlCommand cmd = new SqlCommand(
                            $"use {comboBox_Database.SelectedItem.ToString()}; " +
                            $"create database encryption key with algorithm = {encryptorParametr}" +
                            $" encryption by server certificate {comboBox_NameCerf.SelectedItem.ToString()};"
                            , cnn);

                        cmd.ExecuteNonQuery();

                        MessageBox.Show($"Ключ для шифрования данных для сервера " +
                            $"{comboBox_Database.SelectedItem.ToString()}" +
                            $" успешно создан.\n" +
                            $"Алгоритмом шифрования ключа является {encryptorParametr}.\n" +
                            $"Для шифрования ключа использовался сертификат " +
                            $"{comboBox_NameCerf.SelectedItem.ToString()}.");

                    }
                    catch (SqlException ex)
                    {
                        foreach (SqlError ob in ex.Errors)
                        {
                            if (ob.Message == "Ключ шифрования для этой базы данных уже существует." && ob.Number == 33103 && ob.State == 1)
                            {
                                MessageBox.Show($"Ключ шифрования для этой базы данных уже существует");
                            }
                        }
                    }
                }
            }
        }

        private void button_OnTDE_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_Database.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите базу данных");

            } else
            {
                try
                {
                    using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                    {
                        cnn.Open();

                        SqlCommand cmd = new SqlCommand(
                            $"alter database {comboBox_Database.SelectedItem.ToString()} set encryption on;", cnn);

                        cmd.ExecuteNonQuery();
                    }

                } catch(SqlException ex)
                {
                    foreach (SqlError ob in ex.Errors)
                    {
                        if (ob.Number == 33107 && ob.State == 1)
                        {
                            MessageBox.Show($"Не удается отключить шифрование базы данных {comboBox_Database.SelectedItem.ToString()}," +
                                $" поскольку оно уже включено");
                        }
                    }
                }
            }
        }

        private void button_OffTDE_Click(object sender, RoutedEventArgs e)
        {
            if (comboBox_Database.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите базу данных");

            } else
            {
                try
                {
                    using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                    {
                        cnn.Open();

                        SqlCommand cmd = new SqlCommand(
                            $"alter database {comboBox_Database.SelectedItem.ToString()} set encryption off;", cnn);

                        cmd.ExecuteNonQuery();
                    }

                } catch(SqlException ex)
                {
                    foreach (SqlError ob in ex.Errors)
                    {
                        if (ob.Number == 33108  && ob.State == 1)
                        {
                            MessageBox.Show($"Не удается отключить шифрование базы данных {comboBox_Database.SelectedItem.ToString()}," +
                                $" поскольку оно уже отключено");
                        }
                    }
                }
            }
        }


    }
}
