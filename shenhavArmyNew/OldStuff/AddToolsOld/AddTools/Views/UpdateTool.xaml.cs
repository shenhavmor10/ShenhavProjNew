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
using AddTools;
using System.IO;

namespace ViewChanger.ViewModels
{
    /// <summary>
    /// Interaction logic for UpdateTool.xaml
    /// </summary>
    public partial class UpdateTool : UserControl
    {
        const string DestProjectPath = @"..\..\..\..\ToolsExe";
        public UpdateTool()
        {
            InitializeComponent();
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            DeleteTool.IsChecked = true;
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

        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection cnn;
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            cnn = new SqlConnection(connectionString);
            
            cnn.Open();
            SqlCommand command;
            try
            {
                if (DeleteTool.IsChecked == true)
                {
                    command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_is_working={0} WHERE tool_name='{1}'", 0, ToolName.Text), cnn);
                }
                else
                {
                    string OGPath="";
                    SqlCommand select = new SqlCommand(string.Format("Select tool_exe_name from tools_table WHERE tool_name='{0}';",ToolName.Text), cnn);
                    using (SqlDataReader reader = select.ExecuteReader())
                    {
                        while (reader.Read())
                        {
                            OGPath= DestProjectPath+"\\"+reader["tool_exe_name"].ToString().Substring(0, reader["tool_exe_name"].ToString().IndexOf("\\"));
                        }
                    }
                    command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_exe_name='{0}' WHERE tool_name='{1}'", string.Format(ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)+"/fileScript.txt"), ToolName.Text), cnn);
                    GeneralFunctions.DirectoryCopy(ToolFolderPath.Text, string.Format(DestProjectPath + "\\" + ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)), true);
                    if (Directory.Exists(OGPath))
                    {
                        GeneralFunctions.DeleteDirectory(OGPath);
                    }
                }
                command.ExecuteNonQuery();
                ResultTextBlock.Text = "Success";
            }
            catch(Exception error)
            {
                ResultTextBlock.Text = "error = "+error.Message;
            }
            
        }

        private void ToolName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
