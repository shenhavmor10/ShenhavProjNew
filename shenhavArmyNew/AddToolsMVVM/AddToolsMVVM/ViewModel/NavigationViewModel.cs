using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AddToolsMVVM.ViewModel;

namespace AddToolsMVVM.ViewModel
{
    class NavigationViewModel : INotifyPropertyChanged

    {

        public ICommand _AddToolCommand { get; set; }

        public ICommand _UpdateToolCommand { get; set; }
        public ICommand _RemoveToolCommand { get; set; }

        private ViewModelBase selectedViewModel;

        public ViewModelBase SelectedViewModel

        {

            get { return selectedViewModel; }

            set { selectedViewModel = value; OnPropertyChanged("SelectedViewModel"); }

        }



        public NavigationViewModel()
        {

            _AddToolCommand = new BaseCommand(OpenAdd);

            _UpdateToolCommand = new BaseCommand(OpenUpdate);
            _RemoveToolCommand = new BaseCommand(OpenRemove);

        }
        
        private void OpenAdd(object obj)

        {
            SelectedViewModel = new AddToolViewModel();

        }

        private void OpenUpdate(object obj)

        {
            SelectedViewModel = new UpdateViewModel();

        }

        private void OpenRemove(object obj)

        {
            SelectedViewModel = new RemoveViewModel();

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
