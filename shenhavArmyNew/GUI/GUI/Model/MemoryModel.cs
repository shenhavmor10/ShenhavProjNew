using GUI.ViewModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace GUI.Model
{
    public class MemoryModel : ViewModelBase 
    {
        private string malloc1, malloc2, malloc3, malloc4;
        private string free1, free2;
        public string Malloc1
        {
            get
            {
                return malloc1;
            }
            set
            {
                if (malloc1 != value)
                {
                    malloc1 = value;
                    NotifyPropertyChanged("Malloc1");
                }
            }
        }

        public string Malloc2
        {
            get
            {
                return malloc2;
            }
            set
            {
                if (malloc2 != value)
                {
                    malloc2 = value;
                    NotifyPropertyChanged("Malloc2");
                }
            }
        }
        public string Malloc3
        {
            get
            {
                return malloc3;
            }
            set
            {
                if (malloc3 != value)
                {
                    malloc3 = value;
                    NotifyPropertyChanged("Malloc3");
                }
            }
        }
        public string Malloc4
        {
            get
            {
                return malloc4;
            }
            set
            {
                if (malloc4 != value)
                {
                    malloc4 = value;
                    NotifyPropertyChanged("Malloc4");
                }
            }
        }
        public string Free1
        {
            get
            {
                return free1;
            }
            set
            {
                if (free1 != value)
                {
                    free1 = value;
                    NotifyPropertyChanged("Free1");
                }
            }
        }
        public string Free2
        {
            get
            {
                return free2;
            }
            set
            {
                if (free2 != value)
                {
                    free2 = value;
                    NotifyPropertyChanged("Free2");
                }
            }
        }
    }
}
