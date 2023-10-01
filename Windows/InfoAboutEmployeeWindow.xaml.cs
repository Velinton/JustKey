using JustKey.Classes;
using System;
using System.Data;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media.Imaging;

namespace JustKey.Windows
{
    public partial class InfoAboutEmployeeWindow : Window
    {
        Employee _employee;
        
        private WorkWithConnection _workWithConnection;

        public InfoAboutEmployeeWindow()
        {
            InitializeComponent();
        }

        public Employee SetEmployee
        {
            set { _employee = value; }
        }

        private void LoadInfo()
        {
            EmployeeFullName.Text = $"{_employee.LastName} {_employee.Name} {_employee.MiddleName}";
            EmployeePosition.Text = $"{_employee.Position}";
            EmployeeBirthday.Text = $" {_employee.DateOfBirth}";
            EmployeeAge.Text = $" {_employee.Ages}";
            EmployeePhoneNumber.Text = $" {_employee.PhoneNumber}";
            EmployeeEmail.Text = $" {_employee.Email}";
            EmployeeSalary.Text = $" {_employee.Salary}";
            EmployeeDateOfEmployment.Text = $" {_employee.DateOfEmployment}";
            EmployeeAccountNumber.Text = $" {_employee.AccountNumber}";
            EmployeeLogin.Text = $" {_employee.Login}";
            EmployeePassword.Text = $" {_employee.Password}";

            using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_employee.Photo.Name}", FileMode.OpenOrCreate))
            {
                fs.Write(_employee.Photo.Photo, 0, _employee.Photo.Photo.Length);
                EmployeePhoto.Source = new BitmapImage((Uri)new UriTypeConverter().ConvertFromString(fs.Name));
            }

            if (_employee.Experience >= 12)
            {
                if((_employee.Experience / 12) > 19)
                {
                    if ((_employee.Experience / 12).ToString().Last() != '2' &&
                    (_employee.Experience / 12).ToString().Last() != '3' &&
                    (_employee.Experience / 12).ToString().Last() != '4')
                    {
                        EmployeeExperience.Text = $" {_employee.Experience / 12} лет";
                    }
                    else if ((_employee.Experience / 12).ToString().Last() == '1')
                    {
                        EmployeeExperience.Text = $" {_employee.Experience / 12} год";
                    }
                    else
                    {
                        EmployeeExperience.Text = $" {_employee.Experience / 12} года";
                    }
                }
                else
                {
                    EmployeeExperience.Text = $" {_employee.Experience / 12} лет";
                }
            }
            else if( _employee.Experience < 12 && _employee.Experience >= 5 || _employee.Experience == 0)
            {
                EmployeeExperience.Text = $" {_employee.Experience} месяцев";
            }
            else if(_employee.Experience == 1)
            {
                EmployeeExperience.Text = $" {_employee.Experience} месяц";
            }
            else
            {
                EmployeeExperience.Text = $" {_employee.Experience} месяца";
            }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadInfo();
        }

        private void ChangeAccount_Click(object sender, RoutedEventArgs e)
        {
            if (StartEdit())
                this.DialogResult = true;
        }

        private bool StartEdit()
        {
            AddEmployeeWindow addEmployeeWindow = new AddEmployeeWindow(_employee);
            if (addEmployeeWindow.ShowDialog() == true)
            {
                _employee = addEmployeeWindow.GetEmployee;

                _workWithConnection = new WorkWithConnection();
                if (_workWithConnection.Connect())
                {
                    string command = $"UPDATE Employees SET ID_position = (SELECT ID FROM Positions WHERE Position = '{_employee.Position}'), " +
                                     $"Name = '{_employee.Name}', Last_name = '{_employee.LastName}', Middle_name = '{_employee.MiddleName}', " +
                                     $"Day_of_birth = '{_employee.DateOfBirth}', Age = {_employee.Ages}, Experience_in_months = {_employee.Experience}, Salary = {_employee.Salary}, " +
                                     $"Account_number = '{_employee.AccountNumber}', Date_of_employment = '{_employee.DateOfEmployment}', Phone_number = '{_employee.PhoneNumber}', " +
                                     $"Email = '{_employee.Email}' WHERE ID = {_employee.ID} " +
                                     $"UPDATE Credentials SET Login = '{_employee.Login}', Password = '{_employee.Password}' WHERE ID_employee = {_employee.ID}";
                    _workWithConnection.NewCommand(command, returnValue: false);

                    command = $@"UPDATE Employee_images SET Image = @Image, Name = @Name WHERE ID_employee = {_employee.ID}";

                    _workWithConnection.GetCommand.Parameters.Add("@Image", SqlDbType.Image, 1000000);
                    _workWithConnection.GetCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 1000000);
                    _workWithConnection.GetCommand.Parameters["@Image"].Value = _employee.Photo.Photo;
                    _workWithConnection.GetCommand.Parameters["@Name"].Value = _employee.Photo.Name;
                    _workWithConnection.NewCommand(command, returnValue: false);
                    _workWithConnection.GetCommand.Parameters.Clear();
                    _workWithConnection.Disconnect();


                    MessageBox.Show("Данные успешно сохранены", "Сохранено", MessageBoxButton.OK, MessageBoxImage.Information);
                    return true;
                }
                else
                    MessageBox.Show("Ошибка подключения.\nПопробуйте ещё раз позже", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                return false;
            }
            else
                return false;
        }

        private bool DeleteAccountFromDataBase()
        {
            _workWithConnection = new WorkWithConnection();
            if (_workWithConnection.Connect())
            {
                string command = $"DELETE FROM Employees WHERE ID = {_employee.ID}";
                _workWithConnection.NewCommand(command, false);
                _workWithConnection.Disconnect();
                return true;
            }
            else
                MessageBox.Show("Ошибка подключения.\nПопробуйте ещё раз позже", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            
            return false;
        }

        private void DeleteAccount_Click(object sender, RoutedEventArgs e)
        {
            if(MessageBox.Show("Вы уверены, что хотите удалить этот аккаунт?", "Удаление аккаунта", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if (DeleteAccountFromDataBase())
                    this.DialogResult = true;
            }
        }

        private void BackToMainMenu_Click(object sender, RoutedEventArgs e)
        {
            this.DialogResult = false;
        }

        private void EmployeePhoto_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

        }
    }
}
