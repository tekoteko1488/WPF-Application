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
using System.IO;
using System.ComponentModel;

namespace Diplom_Application
{
    /// <summary>
    /// Логика взаимодействия для AuditData.xaml
    /// </summary>
    public partial class AuditData : Window
    {
        Server server;
        Database db;
        string NameAudit;

        public AuditData(Server _server, string _NameAudit)
        {
            InitializeComponent();

            server = _server;

            NameAudit = _NameAudit;
        }

        IList<AuditInfo> listInfo = new List<AuditInfo>();

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            AuditInfo ob;

            string pathToFile;

            using (SqlConnection cnn = new SqlConnection(server.ConnectionContext.ConnectionString))
            {
                cnn.Open();

                SqlCommand cmd = new SqlCommand($"use [ForAudit]; select PathFile from dbo.PathFileForAudit where AuditName = '{NameAudit}'",cnn);

                SqlDataReader reader = cmd.ExecuteReader();

                reader.Read();

                pathToFile = reader.GetString(0);

                reader.Close();

                string sqlCommand = "SELECT application_name,server_principal_name,event_time,client_ip,affected_rows," +
                    "succeeded,server_instance_name,additional_information,file_name,action_id FROM sys.fn_get_audit_file " +
                     $@"(N'{pathToFile}\*',default,default)";

                cmd = new SqlCommand(sqlCommand, cnn);

                reader = cmd.ExecuteReader();

                while (reader.Read())
                {
                    ob = new AuditInfo();
                    ob.application_name = reader.GetString(0);
                    ob.server_principal_name = reader.GetString(1);
                    ob.event_time = reader.GetDateTime(2);
                    ob.client_ip = reader.GetString(3);
                    ob.affected_rows = reader.GetInt64(4);
                    ob.succeeded = reader.GetBoolean(5);
                    ob.server_instance_name = reader.GetString(6);
                    ob.additional_information = reader.GetString(7);
                    ob.file_name = reader.GetString(8);
                    ob.action_id = reader.GetString(9);

                    listInfo.Add(ob);
                    //MyDataGrid.Items.Add(ob);
                }

                reader.Close();

                MyDataGrid.ItemsSource = listInfo;
            }
            
        }

        public class AuditInfo
        {
            public string application_name { get; set; }
            public string server_principal_name { get; set; }
            public DateTime event_time { get; set; }
            public string client_ip { get; set; }
            public long affected_rows { get; set; }
            public bool succeeded { get; set; }
            public string server_instance_name { get; set; }
            public string additional_information { get; set; }
            public string file_name { get; set; }
            public string action_id { get; set; }
        }

        Microsoft.Office.Interop.Excel.Application excel;
        Microsoft.Office.Interop.Excel.Workbook workBook;
        Microsoft.Office.Interop.Excel.Worksheet workSheet;
        Microsoft.Office.Interop.Excel.Range cellRange;

        private void button_ExportToExcel_Click(object sender, RoutedEventArgs e)
        {
            GenerateExcel(listInfo.ToDataTable());
        }

        private void GenerateExcel(DataTable DtIN)
        {
            try
            {
                excel = new Microsoft.Office.Interop.Excel.Application();
                excel.DefaultFilePath = @"C:\Картиночки";
                excel.DisplayAlerts = true;
                excel.Visible = true;
                workBook = excel.Workbooks.Add(Type.Missing);
                workSheet = (Microsoft.Office.Interop.Excel.Worksheet)workBook.ActiveSheet;
                workSheet.Name = "Журнал аудита";
                System.Data.DataTable tempDt = DtIN;
                MyDataGrid.ItemsSource = tempDt.DefaultView;
                workSheet.Cells.Font.Size = 11;
                int rowcount = 1;
                for (int i = 1; i <= tempDt.Columns.Count; i++) 
                {
                    workSheet.Cells[1, i] = tempDt.Columns[i - 1].ColumnName;
                }
                foreach (System.Data.DataRow row in tempDt.Rows) 
                {
                    rowcount += 1;
                    for (int i = 0; i < tempDt.Columns.Count; i++)  
                    {
                        workSheet.Cells[rowcount, i + 1] = row[i].ToString();
                    }
                }
                cellRange = workSheet.Range[workSheet.Cells[1, 1], workSheet.Cells[rowcount, tempDt.Columns.Count]];
                cellRange.EntireColumn.AutoFit();
            }
            catch (Exception)
            {
                
            }
        }
    }
}
