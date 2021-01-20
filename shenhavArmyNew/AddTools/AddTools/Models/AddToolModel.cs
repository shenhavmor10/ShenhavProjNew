using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddTools.Models
{
    public class AddToolModel : INotifyPropertyChanged
    {
        private string toolName,toolDesc,toolFolder,toolResultNeeded;

        public event PropertyChangedEventHandler PropertyChanged;
        public string ToolName
        {
            get
            {
                return toolName;
            }
            set
            {
                if(toolName!=value)
                {
                    toolName = value;
                    OnPropertyChanged("ToolName");
                }
            }
        }
        public string ToolDescription
        {
            get
            {
                return toolDesc;
            }
            set
            {
                if (toolDesc != value)
                {
                    toolDesc = value;
                    OnPropertyChanged("ToolDescription");
                }
            }
        }
        public string ToolFolder
        {
            get
            {
                return toolFolder;
            }
            set
            {
                if (toolFolder != value)
                {
                    toolFolder = value;
                    OnPropertyChanged("ToolFolder");
                }
            }
        }
        public string ToolResultNeeded
        {
            get
            {
                return toolResultNeeded;
            }
            set
            {
                if (toolResultNeeded != value)
                {
                    toolResultNeeded = value;
                    OnPropertyChanged("ToolResultNeeded");
                }
            }
        }
        private void OnPropertyChanged(string propertyName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propertyName));
            }

                
        }
    }
}
