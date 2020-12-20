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
using System.Data.SqlClient;
using System.ComponentModel;
using System.IO;
using AddTools;


namespace ViewChanger.ViewModels
{
    /// <summary>
    /// Interaction logic for UserControl1.xaml
    /// </summary>
    public partial class AddTools : UserControl
    {
        public AddTools()
        {
            InitializeComponent();
        }
        const string DestProjectPath = @"..\..\..\..\ToolsExe";
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection cnn;
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            cnn = new SqlConnection(connectionString);
            try
            {
                SqlCommand command = new SqlCommand(string.Format(@"INSERT INTO tools_table (tool_name, tool_desc, tool_priority,tool_exe_name, tool_is_working) VALUES('{0}','{1}',{2},'{3}',1);", ToolName.Text, ToolDescription.Text, ToolPriority.Text, string.Format(ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)) + "\\fileScript.txt"), cnn);
                cnn.Open();
                GeneralFunctions.DirectoryCopy(ToolFolderPath.Text, string.Format(DestProjectPath + "\\" + ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)), true);

                command.ExecuteNonQuery();
                ResultTextBlock.Text = "Success";
            }
            catch(Exception error)
            {
                ResultTextBlock.Text = "Error =" + error.Message;
            }
            
        }
        
        private void BrowseBtn_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                ToolFolderPath.Text = folderDialog.SelectedPath;
            }
        }

        private void ToolName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
