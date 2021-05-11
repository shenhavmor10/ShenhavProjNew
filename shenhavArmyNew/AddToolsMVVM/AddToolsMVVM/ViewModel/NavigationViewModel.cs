using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AddToolsMVVM.ViewModel;

namespace AddToolsMVVM.ViewModel
{
    class NavigationViewModel : INotifyPropertyChanged

    {
        static string DestProjectPath;
        public static string connectionString;
        private string resultBlock;
        public ICommand _AddToolCommand { get; set; }

        public ICommand _UpdateToolCommand { get; set; }
        public ICommand _RemoveToolCommand { get; set; }

        private ViewModelBase selectedViewModel;

        public ViewModelBase SelectedViewModel

        {

            get { return selectedViewModel; }

            set { selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }

        }


        // constructor for the view model.
        public NavigationViewModel()
        {

            initializeConfig();
            _AddToolCommand = new BaseCommand(OpenAdd);

            _UpdateToolCommand = new BaseCommand(OpenUpdate);
            _RemoveToolCommand = new BaseCommand(OpenRemove);

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
                connectionString = System.IO.File.ReadAllText(connectionStringFile);
            }
            catch (Exception e)
            {
                ResultBlock = "Couldnt find AddToolConfigFile or missed one of the Files";
            }
            configDict.Clear();
        }
        //ResultBlock get set
        public string ResultBlock
        {
            get
            {
                return resultBlock;
            }
            set
            {
                resultBlock = value;
                OnPropertyChanged("ResultBlock");
            }
        }
        /// Function - OpenAdd
        /// <summary>
        /// function to change to the add view model
        /// </summary>
        /// <param name="obj"></param>
        private void OpenAdd(object obj)

        {
            SelectedViewModel = new AddToolViewModel();

        }
        /// Function - OpenUpdate
        /// <summary>
        /// function to change to the update view model
        /// </summary>
        /// <param name="obj"></param>
        private void OpenUpdate(object obj)

        {
            SelectedViewModel = new UpdateViewModel();

        }
        /// Function - OpenRemove
        /// <summary>
        /// function to change to the remove view model
        /// </summary>
        /// <param name="obj"></param>
        private void OpenRemove(object obj)

        {
            SelectedViewModel = new RemoveViewModel();

        }

        public event PropertyChangedEventHandler PropertyChanged;
        /// Function - OnPropertyChanged
        /// <summary>
        /// just make sure that whenever something is being changed in the data structures it make the changes.
        /// </summary>
        /// <param name="propName"></param>
        private void OnPropertyChanged(string propName)

        {

            if (PropertyChanged != null)

            {

                PropertyChanged(this, new PropertyChangedEventArgs(propName));

            }

        }

    }
}
