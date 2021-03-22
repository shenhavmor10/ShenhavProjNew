using System;
using System.Windows;
using System.Windows.Controls;
using System.ComponentModel;
using System.Threading;
using System.Data.SqlClient;
using ClassesSolution;
using System.Collections;
using System.Collections.Generic;

namespace GUI
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        //the real gui will not be like that and will have trees (this is just a test for me to see that 2 threads can run together)!
        const string EXIT_LINE = "exit";
        const int FirstThread = 0;
        const int SecondThread = 1;
        static int threadNumber = 0;
        static Dictionary<string,string> exeNames = new Dictionary<string, string>();
        static Dictionary<string, string[]> NameAndResultNeeded = new Dictionary<string, string[]>();
        static ArrayList onlyForTest = new ArrayList();
        internal static MainWindow main;
        public MainWindow()
        {
            InitializeComponent();
            main = this;
            SqlConnection cnn;
            //Desktop sql DB name = DESKTOP-L628613
            //Laptop sql DB name = E560-02
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            cnn = new SqlConnection(connectionString);
            cnn.Open();
            SqlCommand command = new SqlCommand("Select tool_name,tool_desc,tool_exe_name,tool_result_needed from tools_table;", cnn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                while (reader.Read())
                {
                    CheckBox temp = new CheckBox();
                    temp.Content= reader["tool_name"].ToString();
                    NameAndResultNeeded.Add(reader["tool_name"].ToString(), reader["tool_result_needed"].ToString().Split(','));
                    exeNames.Add(reader["tool_name"].ToString(), reader["tool_exe_name"].ToString());
                    onlyForTest.Add(reader["tool_exe_name"].ToString());
                    StackPanelCheckBox.Children.Add(temp);
                }
            }
            Console.WriteLine(cnn);
        }
        private void When_Close_Event(object sender, CancelEventArgs e)
        {
            Thread clientThread;
            clientThread = new Thread(() => ClientConnection.ExecuteClient(EXIT_LINE, threadNumber));
            clientThread.Start();
        }
        public void setTextBlock(string data,int number)
        {
            switch (number)
            {
                case FirstThread:
                    Dispatcher.Invoke(new Action(() => { this.TextBlock1.Text = data; })) ;
                    break;
                case SecondThread:
                    Dispatcher.Invoke(new Action(() => { this.TextBlock2.Text = data; }));
                    break;
                default:
                    break;
            }
        }
        
        private void ConnectionButton_Click(object sender, RoutedEventArgs e)
        {
            string content = (sender as Button).Name.ToString();
            string path;
            if(content=="Connect")
            {
                path = FileNameTextBox1.Text + ',' + FileNameTextBox2.Text + ',' + FileNameTextBox3.Text + ',' + FileNameTextBox4.Text+','+FileNameTextBoxDest2.Text;
                //Connect.IsEnabled = false;
                Console.WriteLine("connect1 pressed.");
            }
            else
            {
                path = FileNameTextBox5.Text + ',' + FileNameTextBox6.Text + ',' + FileNameTextBox7.Text + ',' + FileNameTextBox8.Text + ',' + FileNameTextBoxDest.Text;
                //Connect3.IsEnabled = false;
                Console.WriteLine("connect3 pressed.");

            }
            string tools = GeneralConsts.EMPTY_STRING;
            int i = 0;
            foreach(CheckBox tool in StackPanelCheckBox.Children)
            {
                if(tool.IsChecked==true)
                {
                    tools += onlyForTest[i]+",";
                }
                i++;
            }
            //test
            /*
            bool atLeastOneTool = false;
            try
            {
                foreach (CheckBox tool in StackPanelCheckBox.Children)
                {
                    if (!tool.IsChecked == true)
                    {
                        NameAndResultNeeded.Remove(tool.Content.ToString());
                    }
                    else
                    {
                        atLeastOneTool = true;
                    }
                }
                if (!atLeastOneTool)
                {
                    throw new Exception();
                }
                //now everyone in NameAndResultNeeded is only the only who the client checked.
                ArrayList tempArray = new ArrayList();
                foreach (string key in NameAndResultNeeded.Keys)
                {
                    if (NameAndResultNeeded[key].Length == 1 && NameAndResultNeeded[key][0] == "")
                    {
                        tempArray.Add(key);
                    }
                }
                if (tempArray.Count == 0)
                {
                    throw new Exception("Picked tools without delivering them the tools they need.");
                }
                foreach (string alreadyAdded in tempArray)
                {
                    if (NameAndResultNeeded.ContainsKey(alreadyAdded))
                    {
                        NameAndResultNeeded.Remove(alreadyAdded);
                    }
                }
                bool contains = true;
                while (NameAndResultNeeded.Count > 0)
                {
                    foreach (string key in NameAndResultNeeded.Keys)
                    {
                        contains = true;
                        for (i = 0; i < NameAndResultNeeded[key].Length; i++)
                        {
                            if (!tempArray.Contains(NameAndResultNeeded[key][i]))
                            {
                                contains = false;
                            }
                        }
                        if (contains)
                        {
                            tempArray.Add(key);
                        }
                    }
                    foreach (string alreadyAdded in tempArray)
                    {
                        if (NameAndResultNeeded.ContainsKey(alreadyAdded))
                        {
                            NameAndResultNeeded.Remove(alreadyAdded);
                        }

                    }
                }
                for (i = 0; i < tempArray.Count; i++)
                {
                    tools += exeNames[(string)tempArray[i]] + ",";
                }
                */
            //test
            try 
            {
                if (tools.Length > 0)
                {
                    tools = tools.Substring(0, tools.Length - 1);
                }
                if (tools != GeneralConsts.EMPTY_STRING)
                {
                    path += ",tools={" + tools + "}";
                }
                string memoryPatterns = GeneralConsts.EMPTY_STRING;
                foreach (TextBox t in StackPanelMemory.Children)
                {
                    if (t.Text != GeneralConsts.EMPTY_STRING)
                    {
                        memoryPatterns += t.Text + ",";
                    }
                }
                if (memoryPatterns.Length > 0)
                {
                    memoryPatterns = memoryPatterns.Substring(0, memoryPatterns.Length - 1);
                }
                if (memoryPatterns != GeneralConsts.EMPTY_STRING)
                {
                    path += ",memory={" + memoryPatterns + "}";
                }
                string freePatterns = GeneralConsts.EMPTY_STRING;
                foreach (TextBox t in StackPanelFree.Children)
                {
                    if (t.Text != GeneralConsts.EMPTY_STRING)
                    {
                        freePatterns += t.Text + ",";
                    }
                }
                if (freePatterns.Length > 0)
                {
                    freePatterns = freePatterns.Substring(0, freePatterns.Length - 1);
                }
                if (memoryPatterns != GeneralConsts.EMPTY_STRING)
                {
                    path += ",free={" + freePatterns + "}";
                }
                if (Environment_variable_path.Text != "")
                {
                    path += ",environmentVariablePath={" + Environment_variable_path.Text + "}";
                }
                Thread clientThread;
                clientThread = new Thread(() => ClientConnection.ExecuteClient(path, threadNumber));
                clientThread.Start();
                threadNumber++;
            }
            
            
            catch(Exception error)
            {
                TextBlock1.Text = error.ToString();
            }
        }

        private void BrowseButtonFile_Click(object sender, RoutedEventArgs e)
        {
            // Create OpenFileDialog
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            string content = (sender as Button).Name.ToString();
            // Get the selected file name and display in a TextBox.
            // Load content of file in a TextBlock
            if (result == true)
            {
                if(content=="Browse1")
                {
                    FileNameTextBox1.Text = openFileDlg.FileName;
                }
                else if(content == "environmentVariablePath")
                {
                    Environment_variable_path.Text = openFileDlg.FileName;
                }
                else
                {
                    FileNameTextBox5.Text = openFileDlg.FileName;
                }
                
            }
        }
        private void BrowseButtonFolder_Click(object sender, RoutedEventArgs e)
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                string content = (sender as Button).Name.ToString();
                switch (content)
                {
                    case "Browse2":
                        FileNameTextBox2.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse3":
                        FileNameTextBox3.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse4":
                        FileNameTextBox4.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse6":
                        FileNameTextBox6.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse7":
                        FileNameTextBox7.Text = folderDialog.SelectedPath;
                        break;
                    case "Browse8":
                        FileNameTextBox8.Text = folderDialog.SelectedPath;
                        break;
                    case "BrowseDest":
                        FileNameTextBoxDest.Text = folderDialog.SelectedPath;
                        break;
                    case "BrowseDest2":
                        FileNameTextBoxDest2.Text = folderDialog.SelectedPath;
                        break;
                    default:
                        break;

                }
                
            }
        }

        private void FileNameTextBoxDest_Copy_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void FileNameTextBox5_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void Environment_variable_path_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CustomFree_TextChanged(object sender, TextChangedEventArgs e)
        {

        }

        private void CustomMalloc_TextChanged(object sender, TextChangedEventArgs e)
        {

        }
    }
}
