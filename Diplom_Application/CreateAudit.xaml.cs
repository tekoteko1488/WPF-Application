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
using Microsoft.WindowsAPICodePack.Dialogs;
namespace Diplom_Application
{
    /// <summary>
    /// Логика взаимодействия для CreateAudit.xaml
    /// </summary>
    public partial class CreateAudit : Window
    {
        Server server;
        Database db;
        SqlCommand cmd;
        string pathToFile;

        public CreateAudit(Server _server)
        {
            InitializeComponent();

            server = _server;
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            string on_failure;

            if(textBox_NameOfAudit.Text.Length == 0)
            {
                MessageBox.Show("Выберите имя аудита");

            } else if(textBox_QueueDelay.Text.Length == 0)
            {
                MessageBox.Show("Введите количество мс задержки");

            } else if(comboBox_AuditError.SelectedIndex == -1)
            {
                MessageBox.Show("Выберите опцию при сбое журнала");

            } else if(textBox_PathToFile.Text.Length == 0)
            {
                MessageBox.Show("Выберите путь к папке");

            } else
            {
                switch (comboBox_AuditError.SelectedItem.ToString())
                {
                    case "Продолжить":
                        on_failure = "CONTINUE";
                        break;
                    case "Завершение работы сервера":
                        on_failure = "SHUTDOWN";
                        break;
                    case "Сбой операции":
                        on_failure = "FAIL_OPERATION";
                        break;
                    default:
                        MessageBox.Show("Ошибка в свиче, отвечающем за комбобокс об ошибке");
                        on_failure = "CONTINUE";
                        break;
                }

                string sqlCommand = $"use [master]; create server audit [{textBox_NameOfAudit.Text}] " +
                    $"to file ( filepath = N'{pathToFile}',maxsize = 0 mb, MAX_ROLLOVER_FILES = 2147483647, RESERVE_DISK_SPACE = OFF)" +
                    $"with(QUEUE_DELAY = {textBox_QueueDelay.Text}, ON_FAILURE = {on_failure})";

                using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
                {
                    try
                    {
                        cnn.Open(); 

                        cmd = new SqlCommand(sqlCommand, cnn);

                        cmd.ExecuteNonQuery();

                        db = server.Databases["ForAudit"];

                        db.ExecuteNonQuery($@"exec dbo.AuditPath @_AuditName = N'{textBox_NameOfAudit.Text}',
                                       @_PathFile = N'{pathToFile}'");

                        MessageBox.Show($"Аудит {textBox_NameOfAudit.Text} успешно создан");

                    }
                    catch (SqlException ex)
                    {
                        foreach (SqlError ob in ex.Errors)
                        {
                            if (ob.Number == 15530 && ob.State == 1)
                            {
                                MessageBox.Show("Аудит с таким именем уже существует.\n" +
                                    "Пожалуйста, введите другое имя для аудита", "Ошибка",
                                    MessageBoxButton.OK, MessageBoxImage.Stop);
                            }
                        }
                    }
                }
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            comboBox_AuditError.Items.Add("Продолжить");
            comboBox_AuditError.Items.Add("Завершение работы сервера");
            comboBox_AuditError.Items.Add("Сбой операции");
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            CommonOpenFileDialog dlg = new CommonOpenFileDialog();
            dlg.Title = "Выберите папку";
            dlg.IsFolderPicker = true;

            dlg.AddToMostRecentlyUsedList = false;
            dlg.AllowNonFileSystemItems = false;
            dlg.EnsureFileExists = true;
            dlg.EnsurePathExists = true;
            dlg.EnsureReadOnly = false;
            dlg.EnsureValidNames = true;
            dlg.Multiselect = false;
            dlg.ShowPlacesList = true;

            if (dlg.ShowDialog() == CommonFileDialogResult.Ok)
            {
                pathToFile = dlg.FileName;
                textBox_PathToFile.Text = dlg.FileName;
            }
        }
    }
}
