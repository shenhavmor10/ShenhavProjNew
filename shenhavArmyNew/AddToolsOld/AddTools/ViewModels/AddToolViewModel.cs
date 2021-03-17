using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using AddTools.Models;
using AddTools;

namespace AddTools.ViewModels
{
    public class AddToolsViewModel
    {
        private AddToolModel obj=new AddToolModel();
       /* public RelayCommand BrowseCommand
        {
            get
            {
                this.BrowseBtn_Click();
            }
        }*/
        public void BrowseBtn_Click()
        {
            var folderDialog = new Ookii.Dialogs.Wpf.VistaFolderBrowserDialog();
            var folderResult = folderDialog.ShowDialog();
            if (folderResult.HasValue && folderResult.Value)
            {
                obj.ToolName = folderDialog.SelectedPath;
            }
        }
    }
}
