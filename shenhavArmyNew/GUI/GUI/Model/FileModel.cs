using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using GUI.ViewModel;

namespace GUI.Model
{
    public class FileModel : ViewModelBase
    {
        private string filePath, projectPath, gccPath, otherInclude, destinationPath, eVarsPath,buttonName;
        public string FilePath
        {
            get
            {
                return filePath;
            }
            set
            {
                if (filePath != value)
                {
                    filePath = value;
                    NotifyPropertyChanged("FilePath");
                }
            }
        }
        public string ButtonName
        {
            get
            {
                return buttonName;
            }
            set
            {
                if (buttonName != value)
                {
                    buttonName = value;
                    NotifyPropertyChanged("ButtonName");
                }
            }
        }
        public string ProjectPath
        {
            get
            {
                return projectPath;
            }
            set
            {
                if (projectPath != value)
                {
                    projectPath = value;
                    NotifyPropertyChanged("ProjectPath");
                }
            }
        }
        public string GccPath
        {
            get
            {
                return gccPath;
            }
            set
            {
                if (gccPath != value)
                {
                    gccPath = value;
                    NotifyPropertyChanged("GccPath");
                }
            }
        }
        public string OtherInclude
        {
            get
            {
                return otherInclude;
            }
            set
            {
                if (otherInclude != value)
                {
                    otherInclude = value;
                    NotifyPropertyChanged("OtherInclude");
                }
            }
        }
        public string DestinationPath
        {
            get
            {
                return destinationPath;
            }
            set
            {
                if (destinationPath != value)
                {
                    destinationPath = value;
                    NotifyPropertyChanged("DestinationPath");
                }
            }
        }
        public string EVarsPath
        {
            get
            {
                return eVarsPath;
            }
            set
            {
                if (eVarsPath != value)
                {
                    eVarsPath = value;
                    NotifyPropertyChanged("EVarsPath");
                }
            }
        }

    }
}
