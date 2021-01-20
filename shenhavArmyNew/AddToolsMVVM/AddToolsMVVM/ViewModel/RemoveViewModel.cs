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
        public RemoveViewModel()
        {
            Tool = new ToolModel();
        }
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
        public string ResultBlockRemove
        {
            get
            {
                return resultBlockRemove;
            }
            set
            {
                resultBlockRemove = value;
                NotifyPropertyChanged("ResultBlock");
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
        public void Remove()
        {
            SqlConnection cnn;
            string connectionString = "Data Source=DESKTOP-L628613\\SQLEXPRESS;Initial Catalog=ToolsDB;User ID=shenhav;Password=1234";
            try
            {
                cnn = new SqlConnection(connectionString);
                cnn.Open();

                //tool_is_working
                SqlCommand command = new SqlCommand(string.Format(@"UPDATE tools_table SET tool_is_working='{0}' WHERE tool_name='{1}'", 0, Tool.ToolName), cnn);
                command.ExecuteNonQuery();
                ResultBlockRemove = "Success";
            }
            catch(Exception e)
            {
                ResultBlockRemove = "ERROR";
            }
        }
    }

}
