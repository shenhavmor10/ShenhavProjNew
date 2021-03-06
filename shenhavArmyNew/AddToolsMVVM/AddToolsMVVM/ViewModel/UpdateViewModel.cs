﻿using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AddToolsMVVM.Model;

namespace AddToolsMVVM.ViewModel
{
    public class UpdateViewModel : ViewModelBase
    {
        static string DestProjectPath = @"..\..\..\..\ToolsExe";
        private ToolModel newTool;
        private string resultBlockUpdate;
        private ICommand _BrowseCommandUpdate;
        private ICommand _ApplyCommandUpdate;
        //constructor for the viewmodel.
        public UpdateViewModel()
        {
            Tool = new ToolModel();
        }
        public ICommand BrowseCommandUpdate
        {
            get
            {
                if (_BrowseCommandUpdate == null)
                {
                    _BrowseCommandUpdate = new RelayCommand(param => this.Browse(), null);
                }
                return _BrowseCommandUpdate;
            }
        }
        //ResultBlockUpdate Get Set
        public string ResultBlockUpdate
        {
            get
            {
                return resultBlockUpdate;
            }
            set
            {
                resultBlockUpdate = value;
                NotifyPropertyChanged("ResultBlockUpdate");
            }
        }
        public ICommand ApplyCommandUpdate
        {
            get
            {
                if (_ApplyCommandUpdate == null)
                {
                    _ApplyCommandUpdate = new RelayCommand(param => this.Apply(), null);
                }
                return _ApplyCommandUpdate;
            }
        }
        /// Function - Apply
        /// <summary>
        /// Updates the tool requested.
        /// </summary>
        public void Apply()
        {
            SqlConnection cnn;
            try
            {
                cnn = new SqlConnection(NavigationViewModel.connectionString);

                cnn.Open();
                SqlCommand command;
                string OGPath = "";
                SqlCommand select = new SqlCommand(string.Format("Select tool_exe_name from tools_table WHERE tool_name='{0}';", Tool.ToolName), cnn);
                using (SqlDataReader reader = select.ExecuteReader())
                {
                    while (reader.Read())
                    {
                        OGPath = DestProjectPath + "\\" + reader["tool_exe_name"].ToString().Substring(0, reader["tool_exe_name"].ToString().IndexOf("\\"));
                    }
                }
                command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_exe_name='{0}' WHERE tool_name='{1}'", string.Format(Tool.ToolFolder.Substring(Tool.ToolFolder.LastIndexOf("\\") + 1) + "/fileScript.txt"), Tool.ToolName), cnn);
                GeneralFunctions.DirectoryCopy(Tool.ToolFolder, string.Format(DestProjectPath + "\\" + Tool.ToolFolder.Substring(Tool.ToolFolder.LastIndexOf("\\") + 1)), true);
                if (Directory.Exists(OGPath))
                {
                    GeneralFunctions.DeleteDirectory(OGPath);
                }
                ResultBlockUpdate = "Success";
            }
            catch(Exception e)
            {
                ResultBlockUpdate = "Error in updating database, error = " + e.Message;
            }
            
        }
        //Tool Get Set
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
    }
}
