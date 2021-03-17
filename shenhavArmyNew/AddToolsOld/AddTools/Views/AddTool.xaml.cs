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
using AddTools.ViewModels;

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
            this.DataContext = new AddToolsViewModel();
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            SqlConnection cnn = new SqlConnection(connectionString);
            cnn.Open();
            SqlCommand command = new SqlCommand("Select tool_name from tools_table;", cnn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    CheckBox temp = new CheckBox();
                    temp.Content = reader["tool_name"].ToString();
                    StackPanelCheckBox.Children.Add(temp);
                }
            }
            initializeConfig();
        }
        static string DestProjectPath;
        static void initializeConfig()
        {
            Dictionary<string, string> configDict = new Dictionary<string, string>();
            try
            {
                using (var sr = new StreamReader("AddToolConfigFile.txt"))
                {
                    string line = null;

                    // while it reads a key
                    while ((line = sr.ReadLine()) != null)
                    {
                        // add the key and whatever it 
                        // can read next as the value
                        configDict.Add(line.Split('=')[0], line.Split('=')[1]);

                    }
                }
                DestProjectPath = configDict["DestProjectPath"];
            }
            catch (Exception e)
            {
                Console.WriteLine("needs to add error log..");
            }
            configDict.Clear();
        }
        static string SetResultNeeded(StackPanel StackPanelCheckBox)
        {
            string result_string="";
            foreach (CheckBox tool in StackPanelCheckBox.Children)
            {
                if(tool.IsChecked==true)
                {
                    result_string += tool.Content.ToString() + ',';
                }
            }
            if(result_string.Length>0)
            {
                result_string = result_string.Substring(0, result_string.Length - 1);
            }
            return result_string;
        }
        private void ApplyBtn_Click(object sender, RoutedEventArgs e)
        {
            SqlConnection cnn;
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            cnn = new SqlConnection(connectionString);
            cnn.Open();
            try
            {
                SqlCommand command = new SqlCommand(string.Format(@"INSERT INTO tools_table (tool_name, tool_desc, tool_result_needed,tool_exe_name, tool_is_working) VALUES('{0}','{1}',{2},'{3}',1);", ToolName.Text, ToolDescription.Text, SetResultNeeded(StackPanelCheckBox), string.Format(ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)) + "\\fileScript.txt"), cnn);
                
                GeneralFunctions.DirectoryCopy(ToolFolderPath.Text, string.Format(DestProjectPath + "\\" + ToolFolderPath.Text.Substring(ToolFolderPath.Text.LastIndexOf("\\") + 1)), true);

                command.ExecuteNonQuery();
                ResultTextBlock.Text = "Success";
            }
            catch(Exception error)
            {
                ResultTextBlock.Text = "Error =" + error.Message;
            }
            
        }
        
        

        private void ToolName_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
