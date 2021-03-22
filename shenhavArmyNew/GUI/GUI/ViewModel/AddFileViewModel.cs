﻿using GUI.Model;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices.WindowsRuntime;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GUI.ViewModel
{
    class AddFileViewModel : ViewModelBase
    {
        private ObservableCollection<ToolModel> toolsList;
        private FileModel newFile;
        private MemoryModel customMemoryNames;
        private string resultBlock;
        private ICommand _ConnectCommand;
        private ICommand _BrowseCommandFilePath;
        private ICommand _BrowseCommandEVarsPath;
        private ICommand _BrowseCommandDestPath;
        private ICommand _BrowseCommandProjectPath;
        private ICommand _BrowseCommandGccPath;
        private ICommand _BrowseCommandOtherIncludes;
        private ICommand _GoToFileCommand;
        //constructor for the viewmodel
        public AddFileViewModel()
        {
            GetAllToolsFromDB();
            Memory = new MemoryModel();
            TestAllUntimedTools();
            File = new FileModel();
            File.ButtonName = "File"+ (NavigationViewModel.fileList.Count+1);
            NavigationViewModel.fileList.Add(File);

        }
        //ResultBlock Get Set
        public string ResultBlock
        {
            get
            {
                return resultBlock;
            }
            set
            {
                resultBlock = value;
                NotifyPropertyChanged("ResultBlock");
            }
        }
        //Memory Get Set
        public MemoryModel Memory
        {
            get
            {
                return customMemoryNames;
            }
            set
            {
                customMemoryNames = value;
                NotifyPropertyChanged("Memory");
            }
        }
        //File Get Set
        public FileModel File
        {
            get
            {
                return newFile;
            }
            set
            {
                newFile = value;
                NotifyPropertyChanged("File");
            }
        }
        //Tools Get Set
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
        public ICommand ConnectCommand
        {
            get
            {
                if(_ConnectCommand == null)
                {
                    _ConnectCommand = new RelayCommand(param => this.Connect(), null);
                }
                return _ConnectCommand;
            }
        }
        /// Function - createProtocol
        /// <summary>
        /// creates the protocol for the platform. for the connection.
        /// </summary>
        /// <param name="f">file type FileModel.</param>
        /// <param name="toolsArrayExeOnly">tools tool_exe_name array for all tools chosen.</param>
        /// <returns></returns>
        public string createProtocol(FileModel f,ArrayList toolsArrayExeOnly)
        {
            //add first paths.
            string path = f.FilePath + "," + f.ProjectPath + "," + f.GccPath + "," + f.OtherInclude + "," + f.DestinationPath + ",";
            string tools = "";
            foreach (string tool_exe in toolsArrayExeOnly)
            {
                tools += tool_exe + ",";
            }
            try
            {
                if (tools.Length > 0)
                {
                    tools = tools.Substring(0, tools.Length - 1);
                }
                if (tools != "")
                {
                    //add tools.
                    path += "tools={" + tools + "}";
                }
                string memoryPatterns = "";
                ArrayList tempMemoryArray = new ArrayList();
                tempMemoryArray.Add(Memory.Malloc1);
                tempMemoryArray.Add(Memory.Malloc2);
                tempMemoryArray.Add(Memory.Malloc3);
                tempMemoryArray.Add(Memory.Malloc4);
                foreach (string memory in tempMemoryArray)
                {
                    memoryPatterns += memory + ",";
                }
                if (memoryPatterns.Length > 0)
                {
                    memoryPatterns = memoryPatterns.Substring(0, memoryPatterns.Length - 1);
                }
                if (memoryPatterns != "")
                {
                    //add mallocs
                    path += ",memory={" + memoryPatterns + "}";
                }
                string freePatterns = "";
                tempMemoryArray.Clear();
                tempMemoryArray.Add(Memory.Free1);
                tempMemoryArray.Add(Memory.Free2);
                foreach (string memory in tempMemoryArray)
                {
                    freePatterns += memory + ",";
                }
                if (freePatterns.Length > 0)
                {
                    freePatterns = freePatterns.Substring(0, freePatterns.Length - 1);
                }
                if (freePatterns != "")
                {
                    //add frees
                    path += ",free={" + freePatterns + "}";
                }
                if (f.EVarsPath != "")
                {
                    //add environment variables
                    path += ",environmentVariablePath={" + f.EVarsPath + "}";
                }
                
            }
            catch(Exception e)
            {
                Console.WriteLine("entered an error "+e.ToString());
            }
            return path;
        }
        /// Function - Connect
        /// <summary>
        /// This function happens when you press Connect - 
        /// it starts the connection with the platform and finishes aswell.
        /// </summary>
        public void Connect()
        {
            try
            {
                ResultBlock = "";
                ArrayList toolsArray = new ArrayList();
                ObservableCollection<ToolModel> newResultOrder = FixResultOrder(toolsList);
                if(newResultOrder==null)
                {
                    ResultBlock = "Picked tools without delivering them the tools they need. please pick again. or picked 0 tools";
                    throw new Exception("Picked tools without delivering them the tools they need.");
                }
                foreach (ToolModel tool in newResultOrder)
                {
                    toolsArray.Add(tool.ToolExeName);
                }
                if (toolsArray.Count == 0)
                {
                    ResultBlock = "Please Enter at least one tool.";
                    throw new Exception("no tools were being added to the check. add atleast one.");
                }
                string path = createProtocol(File, toolsArray);
                Thread clientThread;
                clientThread = new Thread(() => ClientConnection.ExecuteClient(path,this));
                clientThread.Start();
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
            }
            
            
        }
        /// Function - GetAllToolsFromDB
        /// <summary>
        /// Get all tools from the database and adds them to the toolsList parameter.
        /// </summary>
        public void GetAllToolsFromDB()
        {
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            SqlConnection cnn = new SqlConnection(connectionString);
            cnn.Open();
            SqlCommand command = new SqlCommand("Select tool_name,tool_desc,tool_exe_name,tool_result_needed,tool_avg_line,tool_is_working from tools_table;", cnn);
            using (SqlDataReader reader = command.ExecuteReader())
            {
                toolsList = new ObservableCollection<ToolModel>();
                while (reader.Read())
                {
                    if((bool)reader["tool_is_working"]==true)
                    {
                        toolsList.Add(new ToolModel { ToolName = (string)reader["tool_name"], ToolDescription = (string)reader["tool_desc"],ToolExeName=(string)reader["tool_exe_name"], ToolResultNeeded = (reader["tool_result_needed"] == DBNull.Value) ? string.Empty : (string)reader["tool_result_needed"], ToolLineAvgTime = (reader["tool_avg_line"]==DBNull.Value)? 0.0: reader.GetDouble(4), ToolIsCheck = false});
                    }
                    
                }

            }
        }
        /// Function - FixResultOrder
        /// <summary>
        /// This function makes sure that the tools are being sort in the way the need. tool that need a result of another tool
        /// wont be before this tool in the sort of the protocol. so first up tools that does not need results and after tools 
        /// that need results sorted.
        /// </summary>
        /// <param name="toolsArray">All tools array type ToolModel.</param>
        /// <returns></returns>
        public ObservableCollection<ToolModel> FixResultOrder(ObservableCollection<ToolModel> toolsArray)
        {
            ObservableCollection<ToolModel> allClickedTools = new ObservableCollection<ToolModel>();
            ObservableCollection<ToolModel> newToolOrder = new ObservableCollection<ToolModel>();
            try
            {
                foreach (ToolModel tool in toolsArray)
                {
                    if (tool.ToolIsCheck == true)
                    {
                        allClickedTools.Add(tool);
                    }
                }
                foreach (ToolModel tool in allClickedTools)
                {
                    if (tool.ToolResultNeeded == "")
                    {
                        newToolOrder.Add(tool);
                    }
                }
                if(newToolOrder.Count==0)
                {
                    ResultBlock = "Picked tools without delivering them the tools they need. please pick again.";
                    throw new Exception("Picked tools without delivering them the tools they need.");
                }
                foreach(ToolModel alreadyAdded in newToolOrder)
                {
                    if(allClickedTools.Contains(alreadyAdded))
                    {
                        allClickedTools.Remove(alreadyAdded);
                    }
                }
                bool contains = true;
                while (allClickedTools.Count>0)
                {
                    foreach(ToolModel tool in allClickedTools)
                    {
                        ArrayList tempResultArray = new ArrayList();
                        tempResultArray.AddRange(tool.ToolResultNeeded.Split(','));
                        for(int i=0;i<tempResultArray.Count;i++)
                        {
                            if(!newToolOrder.Contains(tempResultArray[i]))
                            {
                                contains = false;
                            }
                        }
                        if(contains)
                        {
                            newToolOrder.Add(tool);
                        }
                    }
                    foreach(ToolModel alreadyAdded in newToolOrder)
                    {
                        if(allClickedTools.Contains(alreadyAdded))
                        {
                            allClickedTools.Remove(alreadyAdded);
                        }
                    }
                }
            }
            catch(Exception e)
            {
                Console.WriteLine(e.Message);
                newToolOrder = null;
            }
            return newToolOrder;
        }
        //commands - mostly for browsing file or folders.
        public ICommand BrowseCommandFolderPath
        {
            get
            {
                if (_BrowseCommandProjectPath == null)
                {
                    _BrowseCommandProjectPath = new RelayCommand(param => this.BrowseFolderPath(), null);
                }
                return _BrowseCommandProjectPath;
            }
        }
        public void BrowseFolderPath()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                File.ProjectPath = folderDialog.SelectedPath;
            }
        }
        public ICommand BrowseCommandFilePath
        {
            get
            {
                if (_BrowseCommandFilePath == null)
                {
                    _BrowseCommandFilePath = new RelayCommand(param => this.BrowseFilePath(), null);
                }
                return _BrowseCommandFilePath;
            }
        }
        public void BrowseFilePath()
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result==true)
            {
                File.FilePath = openFileDlg.FileName;
            }
        }
        public ICommand BrowseCommandEVarsPath
        {
            get
            {
                if (_BrowseCommandEVarsPath == null)
                {
                    _BrowseCommandEVarsPath = new RelayCommand(param => this.BrowseEVarsPath(), null);
                }
                return _BrowseCommandEVarsPath;
            }
        }
        public void BrowseEVarsPath()
        {
            Microsoft.Win32.OpenFileDialog openFileDlg = new Microsoft.Win32.OpenFileDialog();
            // Launch OpenFileDialog by calling ShowDialog method
            Nullable<bool> result = openFileDlg.ShowDialog();
            if (result == true)
            {
                File.EVarsPath = openFileDlg.FileName;
            }
        }
        public ICommand BrowseCommandDestPath
        {
            get
            {
                if (_BrowseCommandDestPath == null)
                {
                    _BrowseCommandDestPath = new RelayCommand(param => this.BrowseDestPath(), null);
                }
                return _BrowseCommandDestPath;
            }
        }
        public void BrowseDestPath()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                File.DestinationPath = folderDialog.SelectedPath;
            }
        }
        public ICommand BrowseCommandGCCPath
        {
            get
            {
                if (_BrowseCommandGccPath == null)
                {
                    _BrowseCommandGccPath = new RelayCommand(param => this.BrowseGCCPath(), null);
                }
                return _BrowseCommandGccPath;
            }
        }
        public void BrowseGCCPath()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                File.GccPath = folderDialog.SelectedPath;
            }
        }
        public ICommand BrowseCommandGCCPath2
        {
            get
            {
                if (_BrowseCommandOtherIncludes == null)
                {
                    _BrowseCommandOtherIncludes = new RelayCommand(param => this.BrowseGCCPath2(), null);
                }
                return _BrowseCommandOtherIncludes;
            }
        }
        public void BrowseGCCPath2()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                File.OtherInclude = folderDialog.SelectedPath;
            }
        }
        /// Function - FindAllUntimedTools
        /// <summary>
        /// find all untimed tool (tools that werent being checked for average time per code line).
        /// </summary>
        /// <returns>ArrayList of tools.</returns>
        public ArrayList FindAllUntimedTools()
        {
            ArrayList toolsArray = new ArrayList();
            foreach(ToolModel tool in toolsList)
            {
                if(tool.ToolLineAvgTime==0.0)
                {
                    toolsArray.Add(tool.ToolExeName);
                }
            }
            return toolsArray;
        }
        /// Function - TestAllUntimedTools
        /// <summary>
        /// if some of the tools werent being checked it tests them and the platform adds for them a new time.
        /// </summary>
        public void TestAllUntimedTools()
        {
            FileModel testFile = initializeTestFile();
            ArrayList toolsArray = new ArrayList();
            toolsArray = FindAllUntimedTools();
            if(toolsArray.Count>0)
            {
                string path = createProtocol(testFile, toolsArray);
                path += ",test!!";
                Thread clientThread;
                clientThread = new Thread(() => ClientConnection.ExecuteClient(path,this));
                clientThread.Start();
            }
            
        }
        /// Function - initializeTestFile
        /// <summary>
        /// Have all configs for the gui (static paths). all of them are for the test of average time per code line .
        /// </summary>
        /// <returns> File type file model.</returns>
        public FileModel initializeTestFile()
        {
            Dictionary<string, string> configDict = new Dictionary<string, string>();
            FileModel f = new FileModel();
            try
            {
                using (var sr = new StreamReader("GuiConfigFile.txt"))
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
                f.FilePath = configDict["FilePath"];
                f.ProjectPath= configDict["ProjectPath"];
                f.EVarsPath = configDict["Evars"];
                f.GccPath = configDict["GCCFirst"];
                f.OtherInclude = configDict["GCCSecond"];
                f.DestinationPath = configDict["DestPath"];

            }
            catch (Exception e)
            {
                ResultBlock = "Couldnt find AddToolConfigFile or missed one of the Files";
            }
            configDict.Clear();
            return f;
        }

    }
}