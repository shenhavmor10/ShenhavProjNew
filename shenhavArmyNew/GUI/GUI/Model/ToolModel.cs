using GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Model
{
    public class ToolModel : ViewModelBase
    {
        private string toolName, toolDesc, toolFolder, toolResultNeeded,toolExeName;
        private double toolAvgTimeLine;
        private bool toolIsCheck;

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
        public string ToolExeName
        {
            get
            {
                return toolExeName;
            }
            set
            {
                if (toolExeName != value)
                {
                    toolExeName = value;
                    NotifyPropertyChanged("ToolExeName");
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
        public double ToolLineAvgTime
        {
            get
            {
                return toolAvgTimeLine;
            }
            set
            {
                if (toolAvgTimeLine != value)
                {
                    toolAvgTimeLine = value;
                    NotifyPropertyChanged("ToolLineAvgTime");
                }
            }
        }
        public bool ToolIsCheck
        {
            get
            {
                return toolIsCheck;
            }
            set
            {
                if (toolIsCheck != value)
                {
                    toolIsCheck = value;
                    NotifyPropertyChanged("ToolIsCheck");
                }
            }
        }

    }
}
