using JustKey.Classes;
using JustKey.Windows;
using System.Windows;
using System.Windows.Controls;

namespace JustKey.Pages
{
    public partial class TreatmentsPage : Page
    {
        WorkWithConnection _workWithConnection;
        //List<User> _users = new List<User>();

        public TreatmentsPage()
        {
            InitializeComponent();
        }

        //private void Load(bool loadDataFromDB)
        //{
        //    if (loadDataFromDB)
        //        LoadDataFromDB();
        //    //LoadViews();
        //    //DisplaySettings();
        //}

        //private void LoadDataFromDB()
        //{
        //    List<Position> positionsName = new List<Position>();
        //    List<Credentials> credentials = new List<Credentials>();
        //    _employees.Clear();
        //    _workWithConnection = new WorkWithConnection();

        //    if (_workWithConnection.Connect())
        //    {
        //        string commnad = "SELECT * FROM Employees";
        //        _workWithConnection.NewCommand(commnad, returnValue: false);
        //        SqlDataReader dataReader = _workWithConnection.GetCommand.ExecuteReader();

        //        while (dataReader.Read())
        //        {
        //            _employees.Add(new Employee
        //            {
        //                ID = dataReader.GetInt32(0),
        //                Position = dataReader.GetInt32(1).ToString(),
        //                PhotoPath = dataReader.GetValue(2).ToString(),
        //                Name = dataReader.GetString(3),
        //                LastName = dataReader.GetString(4),
        //                MiddleName = dataReader.GetString(5),
        //                DateOfBirth = dataReader.GetString(6),
        //                Ages = dataReader.GetInt32(7),
        //                Experience = dataReader.GetInt32(8),
        //                Salary = dataReader.GetInt32(9),
        //                AccountNumber = dataReader.GetString(10),
        //                DateOfEmployment = dataReader.GetString(11),
        //                PhoneNumber = dataReader.GetString(12),
        //                Email = dataReader.GetString(13),
        //            });
        //        }
        //        dataReader.Close();

        //        commnad = "SELECT * FROM Positions";
        //        _workWithConnection.NewCommand(commnad, returnValue: false);
        //        dataReader = _workWithConnection.GetCommand.ExecuteReader();

        //        while (dataReader.Read())
        //        {
        //            positionsName.Add(new Position
        //            {
        //                ID = dataReader.GetInt32(0),
        //                Name = dataReader.GetString(1)
        //            });
        //        }
        //        dataReader.Close();

        //        commnad = "SELECT * FROM Credentials";
        //        _workWithConnection.NewCommand(commnad, returnValue: false);
        //        dataReader = _workWithConnection.GetCommand.ExecuteReader();

        //        while (dataReader.Read())
        //        {
        //            credentials.Add(new Credentials
        //            {
        //                IDEmployee = dataReader.GetInt32(1),
        //                Login = dataReader.GetString(2),
        //                Password = dataReader.GetString(3)
        //            });
        //        }
        //        dataReader.Close();
        //        _workWithConnection.Disconnect();

        //        for (int i = 0; i < _employees.Count; i++)
        //        {
        //            _employees[i].Position = positionsName.Find(x => x.ID.ToString() == _employees[i].Position).Name;
        //            _employees[i].Login = credentials.Find(x => x.IDEmployee == _employees[i].ID).Login;
        //            _employees[i].Password = credentials.Find(x => x.IDEmployee == _employees[i].ID).Password;
        //        }
        //    }
        //    else
        //        MessageBox.Show("Ошибка подключения", "Ошибка");
        //}

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            AutorisateWindow main = new AutorisateWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Window.GetWindow(this).Close();
        }
    }
}
