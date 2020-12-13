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
            string connectionString = "Data Source=E560-02\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            cnn = new SqlConnection(connectionString);
            
            cnn.Open();
            if (DeleteTool.IsChecked==true)
            {
                SqlCommand command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_is_working={0} WHERE tool_name='{1}'", DeleteTool.IsChecked, ToolName.Text), cnn);
            }
            else
            {
                SqlCommand command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_exe_name={0} WHERE tool_name='{1}'", string.Format(DestProjectPath + "\\" + ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)),ToolName.Text),cnn);
                GeneralFunctions.DirectoryCopy(ToolFolderPath.Text, string.Format(DestProjectPath + "\\" + ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)), true);
            }
        }

        private void ToolName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
