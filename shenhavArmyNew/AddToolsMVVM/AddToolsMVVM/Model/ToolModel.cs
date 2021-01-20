using AddToolsMVVM.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace AddToolsMVVM.Model
{
    public class ToolModel : ViewModelBase
    {
        private string toolName, toolDesc, toolFolder, toolResultNeeded;

        public string ToolName
        {
            get
            {
                return toolName;
            }
            set
            {
                if (toolName != value)
                {
                    toolName = value;
                    NotifyPropertyChanged("ToolName");
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
                    NotifyPropertyChanged("ToolDescription");
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
                    NotifyPropertyChanged("ToolFolder");
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
                    NotifyPropertyChanged("ToolResultNeeded");
                }
            }
        }
    }
}
