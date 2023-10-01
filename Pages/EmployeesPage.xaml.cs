using JustKey.Classes;
using JustKey.Windows;
using System;
using System.Collections.Generic;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace JustKey.Pages
{
    public partial class EmployeesPage : Page
    {
        private const string SEARCH_PLACEHOLDER = "Поиск...";
        private int _startIndex = 0;
        private int _endIndex = 21;
        private WorkWithConnection _workWithConnection;
        private List<Employee> _employees = new List<Employee>();

        public EmployeesPage()
        {
            InitializeComponent();
            Load(loadDataFromDB: true);
        }

        internal Employee GetEmployee(int IDEmployee)
        {
            return _employees[IDEmployee];
        }

        private void Load(bool loadDataFromDB)
        {
            if(loadDataFromDB)
                LoadDataFromDB();
            LoadViews();
            DisplaySettings();
        }

        private void LoadDataFromDB()
        {
            List<Position> positionsName = new List<Position>();
            List<Credentials> credentials = new List<Credentials>();
            List<EmployeePhoto> photos = new List<EmployeePhoto> (); 
            _employees.Clear();
            _workWithConnection = new WorkWithConnection();

            if (_workWithConnection.Connect())
            {
                string commnad = "SELECT * FROM Employees";
                _workWithConnection.NewCommand(commnad, returnValue: false);
                SqlDataReader dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    _employees.Add(new Employee
                    {
                        ID = dataReader.GetInt32(0),
                        Position = dataReader.GetInt32(1).ToString(),
                        Name = dataReader.GetString(2),
                        LastName = dataReader.GetString(3),
                        MiddleName = dataReader.GetString(4),
                        DateOfBirth = dataReader.GetString(5),
                        Ages = dataReader.GetInt32(6),
                        Experience = dataReader.GetInt32(7),
                        Salary = dataReader.GetInt32(8),
                        AccountNumber = dataReader.GetString(9),
                        DateOfEmployment = dataReader.GetString(10),
                        PhoneNumber = dataReader.GetString(11),
                        Email = dataReader.GetString(12),
                    });
                }
                dataReader.Close();

                commnad = "SELECT * FROM Positions";
                _workWithConnection.NewCommand(commnad, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    positionsName.Add(new Position
                    {
                        ID = dataReader.GetInt32(0),
                        Name = dataReader.GetString(1)
                    });
                }
                dataReader.Close();

                commnad = "SELECT * FROM Credentials";
                _workWithConnection.NewCommand(commnad, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    credentials.Add(new Credentials
                    {
                        IDEmployee = dataReader.GetInt32(1),
                        Login = dataReader.GetString(2),
                        Password = dataReader.GetString(3)
                    });
                }
                dataReader.Close();

                commnad = "SELECT Image, Name FROM Employee_images";
                _workWithConnection.NewCommand(commnad, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    photos.Add(new EmployeePhoto()
                    { 
                        Photo = (byte[])dataReader.GetValue(0), 
                        Name = dataReader.GetString(1) 
                    });
                }
                dataReader.Close();
                _workWithConnection.Disconnect();

                for (int i = 0; i < _employees.Count; i++)
                {
                    _employees[i].Position = positionsName.Find(x => x.ID.ToString() == _employees[i].Position).Name;
                    _employees[i].Login = credentials.Find(x => x.IDEmployee == _employees[i].ID)?.Login;
                    _employees[i].Password = credentials.Find(x => x.IDEmployee == _employees[i].ID)?.Password;
                }
                for(int i = 0; i < photos.Count; i++)
                {
                    _employees[i].Photo = photos[i];
                }

                if(FilterRequest.Items.Count == 0)
                    for(int i = 0; i < positionsName.Count; i++)
                        FilterRequest.Items.Add(positionsName[i].Name);
            }
            else
                MessageBox.Show("Ошибка подключения", "Ошибка");
        }

        private void LoadViews()
        {
            employeesViews.Children.Clear();
            if (_employees.Count > 0)
            {
                for(int i = 0; i < _employees.Count; i++)
                {
                    if (i < _startIndex || i >= _endIndex)
                        continue;
                    else if (i > _endIndex)
                        break;

                    string path = string.Empty;
                    
                    StackPanel employeeView = new StackPanel
                    {
                        Background = Brushes.AliceBlue,
                        Margin = new Thickness(25,15,0,0),
                        Cursor = Cursors.Hand
                    };

                    if (_employees[i].Photo.Photo.Length == 0)
                        path = $"{AppOptions.ErrorImage}";
                    else
                    {
                        using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_employees[i].Photo.Name}", FileMode.OpenOrCreate))
                        {
                            fs.Write(_employees[i].Photo.Photo, 0, _employees[i].Photo.Photo.Length);
                            path = $"{fs.Name}";
                        }
                    }

                    Label ID = new Label
                    {
                        Content = _employees[i].ID,
                        Visibility = Visibility.Collapsed
                    };

                    var image = new BitmapImage();
                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;
                    if(path == AppOptions.ErrorImage)
                        image.UriSource = new Uri($"{path}", UriKind.Relative);
                    else
                        image.UriSource = new Uri($"{path}");
                    image.EndInit();

                    Image employeePhoto = new Image
                    {
                        Source = image,
                        Margin = new Thickness(0, 5, 0, 0),
                        Stretch = Stretch.Uniform,
                        Height = 110
                    };

                    TextBlock employeeNames = new TextBlock
                    {
                        Text = $"{_employees[i].LastName} {_employees[i].Name}",
                        HorizontalAlignment = HorizontalAlignment.Center,
                        LineHeight = 19,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0,5,0,0)
                    };

                    if(employeeNames.Text.Length >= 17)
                    {
                        employeeNames.Text = $"{_employees[i].LastName}\n{_employees[i].Name}";
                        employeeNames.LineHeight = double.NaN;
                    }

                    TextBlock employeePosition = new TextBlock
                    {
                        Text = $"{_employees[i].Position}",
                        HorizontalAlignment = HorizontalAlignment.Center
                    };

                    employeeView.Children.Add(ID);
                    employeeView.Children.Add(employeePhoto);
                    employeeView.Children.Add(employeeNames);
                    employeeView.Children.Add(employeePosition);
                    employeeView.MouseLeftButtonDown += EmployeeView_MouseLeftButtonDown;

                    employeesViews.Children.Add(employeeView);
                }
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void EmployeeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InfoAboutEmployeeWindow infoAboutEmployee = new InfoAboutEmployeeWindow
            {
                SetEmployee = _employees.Find(x => x.ID == int.Parse(((sender as StackPanel).Children[0] as Label).Content.ToString()))
            };

            if ((bool)infoAboutEmployee.ShowDialog() == true)
                Load(loadDataFromDB: true);
        }

        private void ColorizeButton(object sender, MouseEventArgs e)
        {
            (sender as Button).Style = (Style)FindResource("RoundedGradientButton");
        }

        private void UnColorizeButton(object sender, MouseEventArgs e)
        {
            (sender as Button).Style = (Style)FindResource("RoundedDefaultButton");
        }

        private void SearchRequest_GotFocus(object sender, RoutedEventArgs e)
        {
            if (SearchRequest.Text == SEARCH_PLACEHOLDER)
                SearchRequest.Text = null;

            SearchRequest.Foreground = Brushes.Black;
        }

        private void SearchRequest_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(SearchRequest.Text) || SearchRequest.Text == SEARCH_PLACEHOLDER)
            {
                SearchRequest.Text = SEARCH_PLACEHOLDER;
                SearchRequest.Foreground = Brushes.Gray;
            }
        }
        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            AutorisateWindow main = new AutorisateWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Window.GetWindow(this).Close();
        }

        private void OpenKeysPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new KeysPage());
        }

        private void OpenGamesPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamesPage());
        }

        private void OpenNewRaportPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RaportsPage());
        }

        private void ReloadPage_Click(object sender, RoutedEventArgs e)
        {
            FilterRequest.SelectedIndex = -1;
            FilterRequest.Text = "Фильтровать по должности";
            Load(loadDataFromDB: true);
        }

        private void DisplaySettings()
        {
            if(_employees.Count > 21)
            {
                MoveToRight.Style = (Style)FindResource("RoundedDefaultButton");
                MoveToEndList.Style = (Style)FindResource("RoundedDefaultButton");
            }
        }

        private void AddEmployee(Employee newEmployee)
        {
            _workWithConnection = new WorkWithConnection();

            if (_workWithConnection.Connect())
            {
                string command = $"INSERT Employees VALUES ((SELECT ID FROM Positions WHERE Position = '{newEmployee.Position}'), " +
                    $"'{newEmployee.Name}', '{newEmployee.LastName}', '{newEmployee.MiddleName}', '{newEmployee.DateOfBirth}'," +
                    $"{newEmployee.Ages}, {newEmployee.Experience}, {newEmployee.Salary}, '{newEmployee.AccountNumber}', '{newEmployee.DateOfEmployment}', '{newEmployee.PhoneNumber}', " +
                    $"'{newEmployee.Email}')";
                _workWithConnection.NewCommand(command, returnValue: false);

                command = $"INSERT Credentials Values ((SELECT ID FROM Employees WHERE Phone_number = '{newEmployee.PhoneNumber}'), '{newEmployee.Login}', '{newEmployee.Password}')";
                _workWithConnection.NewCommand(command, returnValue: false);

                command = $@"INSERT Employee_images Values ((SELECT ID FROM Employees WHERE Phone_number = '{newEmployee.PhoneNumber}'), @Image, @Name)";
                
                _workWithConnection.GetCommand.Parameters.Add("@Image", SqlDbType.Image, 1000000);
                _workWithConnection.GetCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 1000000);
                _workWithConnection.GetCommand.Parameters["@Image"].Value = newEmployee.Photo.Photo;
                _workWithConnection.GetCommand.Parameters["@Name"].Value = newEmployee.Photo.Name;

                _workWithConnection.NewCommand(command, returnValue: false);
                _workWithConnection.GetCommand.Parameters.Clear();
                _workWithConnection.Disconnect();
                Load(loadDataFromDB: true);
            }
        }

        private void AddNewEmployee_Click(object sender, RoutedEventArgs e)
        {
            AddEmployeeWindow addNewEmployee = new AddEmployeeWindow();
            if(addNewEmployee.ShowDialog() == true)
                AddEmployee(addNewEmployee.GetEmployee);
        }

        private void MoveToRight_Click(object sender, RoutedEventArgs e)
        {
            MoveToLeft.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToStartList.Style = (Style)FindResource("RoundedDefaultButton");

            _startIndex += 21;
            _endIndex += 21;

            LoadViews();

            if (employeesViews.Children.Count <= 21 &&
            _endIndex >= _employees.Count)
            {
                MoveToRight.Style = (Style)FindResource("BlockButton");
                MoveToEndList.Style = (Style)FindResource("BlockButton");
            }
        }

        private void MoveToLeft_Click(object sender, RoutedEventArgs e)
        {
            MoveToRight.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToEndList.Style = (Style)FindResource("RoundedDefaultButton");

            _startIndex -= 21;
            _endIndex -= 21;
            LoadViews();

            if (_startIndex <= 0)
            {
                MoveToLeft.Style = (Style)FindResource("BlockButton");
                MoveToStartList.Style = (Style)FindResource("BlockButton");
            }
        }

        private void MoveToStartList_Click(object sender, RoutedEventArgs e)
        {
            MoveToRight.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToEndList.Style = (Style)FindResource("RoundedDefaultButton");

            _startIndex = 0;
            _endIndex = 21;
            LoadViews();

            MoveToLeft.Style = (Style)FindResource("BlockButton");
            MoveToStartList.Style = (Style)FindResource("BlockButton");
        }

        private void MoveToEndList_Click(object sender, RoutedEventArgs e)
        {
            MoveToLeft.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToStartList.Style = (Style)FindResource("RoundedDefaultButton");

            if (_employees.Count % 21 != 0)
                _startIndex = _employees.Count - (_employees.Count % 21);
            else
                _startIndex = _employees.Count - 21;

            _endIndex = _startIndex + 21;

            LoadViews();

            MoveToRight.Style = (Style)FindResource("BlockButton");
            MoveToEndList.Style = (Style)FindResource("BlockButton");
        }

        private List<Employee> Filter(List<Employee> employees, int selectedIndex)
        {
            employees.RemoveAll(x => x.Position != FilterRequest.SelectedItem.ToString());
            return employees;
        }

        private List<Employee> Search(List<Employee> employees)
        {
            List<Employee> editedListWithEmployees = new List<Employee>();

            foreach (var employee in employees)
                if (Regex.IsMatch($"{employee.Name} {employee.LastName}", $"{SearchRequest.Text}.*"))
                    editedListWithEmployees.Add(employee);

            return editedListWithEmployees;
        }

        private void FilterRequest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(FilterRequest.SelectedIndex != -1)
            {
                _employees = Filter(_employees, FilterRequest.SelectedIndex);
                Load(loadDataFromDB: false);
            }
        }

        private void SearchStart_Click(object sender, RoutedEventArgs e)
        {
            _employees = Search(_employees);
            Load(loadDataFromDB: false);
        }

        private void SearchRequest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _employees = Search(_employees);
            Load(loadDataFromDB: false);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            SearchRequest.PreviewTextInput += OptionsOfText.OnlyRussianKeybord;
        }
    }
}
