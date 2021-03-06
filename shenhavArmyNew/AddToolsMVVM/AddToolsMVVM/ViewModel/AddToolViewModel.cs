﻿using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.Dynamic;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AddToolsMVVM.Model;

namespace AddToolsMVVM.ViewModel
{
    public class AddToolViewModel : ViewModelBase
    {
        static string DestProjectPath;
        private ObservableCollection<ToolModel> toolsList;
        private ToolModel newTool;
        private string resultBlock;
        private ICommand _ApplyCommand;
        private ICommand _BrowseCommand;
        // constructor for the viewmodel.
        public AddToolViewModel()
        {
            initializeConfig();
            GetAllToolsFromDB();
            Tool = new ToolModel();
            Tools.CollectionChanged += new System.Collections.Specialized.NotifyCollectionChangedEventHandler(Tools_CollectionChanged);
        }
        //collefctionChangedType
        void Tools_CollectionChanged(object sender, System.Collections.Specialized.NotifyCollectionChangedEventArgs e)
        {
            NotifyPropertyChanged("Tools");
        }
        /// Function - GetAllToolsFromDB
        /// <summary>
        /// Get all tools from the database and add them to the toolsList parameter.
        /// </summary>
        public void GetAllToolsFromDB()
        {
            SqlConnection cnn = new SqlConnection(NavigationViewModel.connectionString);
            cnn.Open();
            SqlCommand command = new SqlCommand("Select tool_name,tool_desc,tool_result_needed from tools_table;", cnn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                toolsList = new ObservableCollection<ToolModel>();
                while (reader.Read())
                {
                    toolsList.Add(new ToolModel { ToolName = (string)reader["tool_name"], ToolDescription = (string)reader["tool_desc"], ToolResultNeeded = (reader["tool_result_needed"]== DBNull.Value) ? string.Empty : (string)reader["tool_result_needed"] });
                }
            }
        }
        //ResultBlock get set
        public string ResultBlockAdd
        {
            get
            {
                return resultBlock;
            }
            set
            {
                resultBlock = value;
                NotifyPropertyChanged("ResultBlockAdd");
            }
        }
        //Tool get set
        public ToolModel Tool
        {
            get
            {
                return newTool;
            }
            set
            {
                newTool = value;
                NotifyPropertyChanged("Tool");
            }
        }
        //Tools get set
        public ObservableCollection<ToolModel> Tools
        {
            get
            {
                return toolsList;
            }
            set
            {
                toolsList = value;
                NotifyPropertyChanged("Tools");
            }
        }
        // Function to the apply button.
        public ICommand ApplyCommand
        {
            get
            {
                if (_ApplyCommand == null)
                {
                    _ApplyCommand = new RelayCommand(param => this.Apply(), null);
                }
                return _ApplyCommand;
            }
        }
        // Function to browse the folder path.
        public ICommand BrowseCommand
        {
            get
            {
                if (_BrowseCommand == null)
                {
                    _BrowseCommand = new RelayCommand(param => this.Browse(), null);
                }
                return _BrowseCommand;
            }
        }
        /// Function - Apply
        /// <summary>
        /// happens when you press on the button Apply - 
        /// this function adds to the database a new tool. after the client filled all information.
        /// </summary>
        public void Apply()
        {
            SqlConnection cnn;
            //connection string
            cnn = new SqlConnection(NavigationViewModel.connectionString);
            cnn.Open();
            SqlCommand command;
            try
            {
                //commands
                if (Tool.ToolResultNeeded == null)
                {
                    command = new SqlCommand(string.Format(@"INSERT INTO tools_table (tool_name, tool_desc,tool_exe_name, tool_is_working) VALUES('{0}','{1}','{2}',1);", Tool.ToolName, Tool.ToolDescription, string.Format(Tool.ToolFolder.Substring(Tool.ToolFolder.LastIndexOf("\\") + 1)) + "\\fileScript.txt"), cnn);
                }
                else
                {
                    command = new SqlCommand(string.Format(@"INSERT INTO tools_table (tool_name, tool_desc, tool_result_needed,tool_exe_name, tool_is_working) VALUES('{0}','{1}',{2},'{3}',1);", Tool.ToolName, Tool.ToolDescription, Tool.ToolResultNeeded, string.Format(Tool.ToolFolder.Substring(Tool.ToolFolder.LastIndexOf("\\") + 1)) + "\\fileScript.txt"), cnn);
                }
                GeneralFunctions.DirectoryCopy(Tool.ToolFolder, string.Format(DestProjectPath + "\\" + Tool.ToolFolder.Substring(Tool.ToolFolder.LastIndexOf("\\") + 1)), true);
                command.ExecuteNonQuery();
                Tools.Add(Tool);
                ResultBlockAdd = "Success";
            }
            catch(Exception e)
            {
                ResultBlockAdd = "Error in database insertion , error = "+e.Message;
            }
            
        }
        /// Function - Browse
        /// <summary>
        /// using Ookii Dialogs Wpf and when someone presses browse it pops a browse window for folders.
        /// </summary>
        public void Browse()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                Tool.ToolFolder = folderDialog.SelectedPath;
            }
        }
        /// Function - initializeConfig
        /// <summary>
        /// Gets all config for the project (like getting the DestProjectPath).
        /// </summary>
        public void initializeConfig()
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
                string connectionStringFile = configDict["SqlConnectionString"];
                Console.WriteLine(connectionStringFile);
                /*if (!System.IO.File.Exists(connectionStringFile))
                {
                    connectionString = System.IO.File.ReadAllText(connectionStringFile);
                }*/
                NavigationViewModel.connectionString = System.IO.File.ReadAllText(connectionStringFile);
            }
            catch (Exception e)
            {
                ResultBlockAdd = "Couldnt find AddToolConfigFile or missed one of the Files";
            }
            configDict.Clear();
        }

    }
    
}
