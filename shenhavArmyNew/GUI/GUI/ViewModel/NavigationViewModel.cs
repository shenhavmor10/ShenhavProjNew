using GUI.Model;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;

namespace GUI.ViewModel
{
    class NavigationViewModel : INotifyPropertyChanged

    {
        public static ObservableCollection<FileModel> fileList = new ObservableCollection<FileModel>();
        private ObservableCollection<AddFileViewModel> addFileViewModels = new ObservableCollection<AddFileViewModel>();
        public ICommand _AddFileCommand { get; set; }
        public ICommand _DynamicButtonsCommand { get; set; }
        private ICommand _GoToFileCommand;


        private ViewModelBase selectedViewModel;

        public ViewModelBase SelectedViewModel

        {

            get { return selectedViewModel; }

            set { selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }

        }


        public ObservableCollection<FileModel> Files
        {
            get
            {
                return fileList;
            }
            set
            {
                fileList = value;
                OnPropertyChanged("Files");
            }
        }
        public NavigationViewModel()
        {

            _AddFileCommand = new BaseCommand(OpenAdd);

        }

        private void OpenAdd(object obj)

        {
            AddFileViewModel newFileViewModel = new AddFileViewModel();
            SelectedViewModel = newFileViewModel;
            addFileViewModels.Add(newFileViewModel);
        }
        public ICommand GoToFileCommand
        {
            get
            {
                if (_GoToFileCommand == null)
                {
                    _GoToFileCommand = new RelayCommand(param => this.GoToFile(fileList[0].ButtonName), null);
                }
                return _GoToFileCommand;
            }
        }
        public void GoToFile(string buttonName)
        {

            for(int i =0;i<fileList.Count;i++)
            {
                if(fileList[i].ButtonName==buttonName)
                {
                    selectedViewModel = addFileViewModels[i];
                }
            }
        }
        
        public event PropertyChangedEventHandler PropertyChanged;

        private void OnPropertyChanged(string propName)

        {

            if (PropertyChanged != null)

            {

                PropertyChanged(this, new PropertyChangedEventArgs(propName));

            }

        }

    }
}
