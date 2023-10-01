using JustKey.Classes;
using Microsoft.Win32;
using System;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JustKey.Windows
{
    public partial class AddEmployeeWindow : Window
    {
        WorkWithConnection _connection;
        private Employee _employee = new Employee();
        private readonly string _waterMark = string.Empty;

        //Редактирование существующего сотрудника
        public AddEmployeeWindow(object employee)
        {
            InitializeComponent();
            LoadPositions();
            EmployeeBirthDay.DisplayDateEnd = DateTime.Now;
            _employee = (Employee)employee;
            LoadInfoAboutEmployee();
        }

        //Добавление сотрудника
        public AddEmployeeWindow()
        {
            InitializeComponent();
            LoadPositions();
            _waterMark = EmployeePhoto.Source.ToString();
        }

        private void LoadInfoAboutEmployee()
        {
            AddNewEmployee.Content = "Готово";
            EmployeeName.Text = _employee.Name;
            EmployeeLastName.Text = _employee.LastName;
            EmployeeMiddleName.Text = _employee.MiddleName;
            EmployeePositions.SelectedIndex = EmployeePositions.Items.IndexOf(_employee.Position);
            EmployeeBirthDay.SelectedDate = DateTime.Parse(_employee.DateOfBirth);
            EmployeeAges.Text = _employee.Ages.ToString();
            EmployeePhoneNumber.Text = _employee.PhoneNumber;
            EmployeeEmail.Text = _employee.Email;
            EmployeeExperience.Text = _employee.Experience.ToString();
            EmployeeSalary.Text = _employee.Salary.ToString();
            EmployeeDateOfEmployment.SelectedDate = DateTime.Parse(_employee.DateOfEmployment);
            EmployeeAccountNumber.Text = _employee.AccountNumber;
            EmployeeLogin.Text = _employee.Login;
            EmployeePassword.Text = _employee.Password;
            LoadPhoto(_employee.Photo);
        }

        private void SaveEmployeeData()
        {
            _employee = new Employee
            {
                ID = _employee.ID,
                Name = EmployeeName.Text,
                LastName = EmployeeLastName.Text,
                MiddleName = EmployeeMiddleName.Text,
                DateOfBirth = ((DateTime)EmployeeBirthDay.SelectedDate).ToString("dd.MM.yyyy"),
                Ages = int.Parse(EmployeeAges.Text),
                Experience = int.Parse(EmployeeExperience.Text),
                Position = EmployeePositions.SelectedItem.ToString(),
                Salary = int.Parse(EmployeeSalary.Text),
                DateOfEmployment = ((DateTime)EmployeeDateOfEmployment.SelectedDate).ToString("dd.MM.yyyy"),
                AccountNumber = EmployeeAccountNumber.Text,
                Email = EmployeeEmail.Text,
                PhoneNumber = EmployeePhoneNumber.Text,
                Login = EmployeeLogin.Text,
                Password = EmployeePassword.Text
            };

            using (FileStream fs = new FileStream(new Uri(EmployeePhoto.Source.ToString()).LocalPath, FileMode.OpenOrCreate))
            {
                if (EmployeePhoto.Source.ToString() != _waterMark)
                {
                    _employee.Photo = new EmployeePhoto { Name = fs.Name.Substring(fs.Name.LastIndexOf("\\") + 1), Photo = new byte[fs.Length]};
                    fs.Read(_employee.Photo.Photo,0, _employee.Photo.Photo.Length);
                }
            }
        }

        public void LoadPositions()
        {
             _connection = new WorkWithConnection();
            if (_connection.Connect())
            {
                string command = "SELECT * FROM Positions";
                _connection.NewCommand(command, returnValue: false);

                SqlDataReader dataReader = _connection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    EmployeePositions.Items.Add(dataReader.GetString(1));
                }
                dataReader.Close();
                _connection.Disconnect();
            }
        }

        private void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && !Char.IsNumber(e.Text[0]))
                e.Handled = true;
        }

        private void AddNewEmployee_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(EmployeeName.Text) && !string.IsNullOrEmpty(EmployeeLastName.Text) &&
                !string.IsNullOrEmpty(EmployeeMiddleName.Text) && !string.IsNullOrEmpty(EmployeeBirthDay.SelectedDate.ToString()) &&
                EmployeeAges.Text != "0" && EmployeePositions.SelectedIndex >= 0 && EmployeeSalary.Text != "0" &&
                !string.IsNullOrEmpty(EmployeeDateOfEmployment.SelectedDate.ToString()) && EmployeeAccountNumber.Text.Length > 0 &&
                CheckEmail(EmployeeEmail.Text) && !string.IsNullOrEmpty(EmployeePhoneNumber.Text) && !string.IsNullOrEmpty(EmployeeLogin.Text) &&
                !string.IsNullOrEmpty(EmployeePassword.Text))
            {
                try
                {
                    SaveEmployeeData();
                    this.DialogResult = true;
                }
                catch (Exception)
                {
                    MessageBox.Show("Что-то пошло не так. Проверьте введённые данные и повторите попытку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                }

            }
            else
            {
                MessageBox.Show("Не все поля заполнены. Пожалуйста, заполните все поля и повторите попытку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        private void IncrementAge_Click(object sender, RoutedEventArgs e)
        {
            EmployeeAges.Text = Increment(EmployeeAges.Text);
        }

        private void DecrementAge_Click(object sender, RoutedEventArgs e)
        {
            EmployeeAges.Text = Decrement(EmployeeAges.Text);
        }

        private void DecrementExperience_Click(object sender, RoutedEventArgs e)
        {
            EmployeeExperience.Text = Decrement(EmployeeExperience.Text);
        }

        private void IncrementExperience_Click(object sender, RoutedEventArgs e)
        {
            EmployeeExperience.Text = Increment(EmployeeExperience.Text);
        }

        private string Increment(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "0";
            return (int.Parse(text) + 1).ToString();
        }

        private string Decrement(string text)
        {
            if (string.IsNullOrEmpty(text))
                text = "0";
            else if (text.Equals("0"))
                return "0";

            return (int.Parse(text) - 1).ToString();
        }

        public Employee GetEmployee
        {
            get { return _employee; }
        }

        private void LoadPhoto(EmployeePhoto imageData)
        {
            string pathToPhoto;
            var image = new BitmapImage();

            ((TextBlock)PhotoCanvas.Children[0]).Visibility = Visibility.Collapsed;
            
            Binding borderHeight = new Binding
            {
                ElementName = "MainBorder",
                Path = new PropertyPath("Height")
            };
            Binding borderWidth = new Binding
            {
                ElementName = "MainBorder",
                Path = new PropertyPath("Width")
            };

            using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{imageData.Name}", FileMode.OpenOrCreate))
            {
                fs.Write(imageData.Photo, 0, imageData.Photo.Length);
                pathToPhoto = fs.Name;
            }

            image.BeginInit();
            image.CacheOption = BitmapCacheOption.OnLoad;
            image.UriSource = new Uri(pathToPhoto);
            image.EndInit();

            EmployeePhoto.Source = image;
            Canvas.SetBottom(EmployeePhoto, 0);
            Canvas.SetLeft(EmployeePhoto, 0);
            EmployeePhoto.SetBinding(HeightProperty, borderHeight);
            EmployeePhoto.SetBinding(WidthProperty, borderWidth);
            EmployeePhoto.Height -= 4;
            EmployeePhoto.Width -= 4;
        }

        private void AddEmployeePhoto_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Files|*.jpg;*.jpeg;*.png;*.JPG;",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                using (FileStream fs = new FileStream(openFileDialog.FileName, FileMode.Open))
                {
                    byte[] data = new byte[fs.Length];
                    fs.Read(data, 0, data.Length);
                    LoadPhoto(new EmployeePhoto { Photo = data, Name = fs.Name.Substring(fs.Name.LastIndexOf('\\') + 1)});
                }
            }
        }

        private void EmployeePhoneNumber_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!string.IsNullOrEmpty(e.Text) && !Char.IsNumber(e.Text[0]) && e.Text[0] != '-' && e.Text[0] != '+')
                e.Handled = true;
        }

        private bool CheckEmail(string email)
        {
            if (Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase) &&
                !string.IsNullOrEmpty(email))
                return true;
            else
                return false;
        }

        private void EmployeeEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox email = sender as TextBox;

            if (CheckEmail(email.Text))
                email.Foreground = Brushes.Black;
            else
                email.Foreground = Brushes.Red;
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            EmployeeEmail.PreviewTextInput += OptionsOfText.DisableRussianKeybord;
            EmployeeLogin.PreviewTextInput += OptionsOfText.DisableRussianKeybord;
            EmployeePassword.PreviewTextInput += OptionsOfText.DisableRussianKeybord;

            EmployeeLastName.PreviewTextInput += OptionsOfText.OnlyRussianKeybord;
            EmployeeName.PreviewTextInput += OptionsOfText.OnlyRussianKeybord;
            EmployeeMiddleName.PreviewTextInput += OptionsOfText.OnlyRussianKeybord;

            EmployeeAccountNumber.PreviewTextInput += OptionsOfText.OnlyNumber;
            EmployeeSalary.PreviewTextInput += OptionsOfText.OnlyNumber;
            EmployeeExperience.PreviewTextInput += OptionsOfText.OnlyNumber;
            EmployeeAges.PreviewTextInput += OptionsOfText.OnlyNumber;

            this.PreviewKeyDown += OptionsOfText.LockSpace;
        }

        private void Window_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OptionsOfText.DisableCommands(sender, e);
        }
    }
}