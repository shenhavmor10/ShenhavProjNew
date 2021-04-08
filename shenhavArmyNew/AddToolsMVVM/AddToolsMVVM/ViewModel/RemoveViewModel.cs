using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Input;
using AddToolsMVVM.Model;

namespace AddToolsMVVM.ViewModel
{
    public class RemoveViewModel : ViewModelBase
    {
        static string DestProjectPath = @"..\..\..\..\ToolsExe";
        private string resultBlockRemove;
        private ToolModel newTool;
        private ICommand _RemoveCommand;
        //constructor for the viewmodel
        public RemoveViewModel()
        {
            Tool = new ToolModel();
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
        //ResultBlockRemove Get Set
        public string ResultBlockRemove
        {
            get
            {
                return resultBlockRemove;
            }
            set
            {
                resultBlockRemove = value;
                NotifyPropertyChanged("ResultBlockRemove");
            }
        }
        public ICommand RemoveCommand
        {
            get
            {
                if (_RemoveCommand == null)
                {
                    _RemoveCommand = new RelayCommand(param => this.Remove(), null);
                }
                return _RemoveCommand;
            }
        }
        /// Function - Remove
        /// <summary>
        /// When pressing the button Remove - 
        /// it changes in the database the tool section is_working to false. 
        /// </summary>
        public void Remove()
        {
            SqlConnection cnn;
            try
            {
                cnn = new SqlConnection(NavigationViewModel.connectionString);
                cnn.Open();

                //tool_is_working
                SqlCommand command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_is_working='{0}' WHERE tool_name='{1}'", 0, Tool.ToolName), cnn);
                command.ExecuteNonQuery();
                ResultBlockRemove = "Success";
            }
            catch(Exception e)
            {
                ResultBlockRemove = "Error in updating database, error = "+e.Message;
            }
        }
    }

}
