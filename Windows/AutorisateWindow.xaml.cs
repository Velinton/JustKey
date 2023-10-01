using JustKey.Classes;
using System.IO;
using System.Reflection;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JustKey.Windows
{
    public partial class AutorisateWindow : Window
    {
        private bool _passwordHidden = true;
        private const string LOGIN_PLACEHOLDER = "Введите здесь свой логин";
        public AutorisateWindow()
        {
            InitializeComponent();
        }

        private void SetSelection(PasswordBox passwordBox, int start, int length)
        {
            passwordBox.GetType().GetMethod("Select", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(passwordBox, new object[] { start, length });
        }

        private void AutorisateUser()
        {
            if (!string.IsNullOrEmpty(Login.Text))
            {
                string passwdTmp;

                if (_passwordHidden)
                    passwdTmp = OpenedPassword.Text;
                else
                    passwdTmp = Password.Password;

                if (!string.IsNullOrEmpty(passwdTmp))
                {
                    WorkWithConnection workWithConnection = new WorkWithConnection();

                    if (workWithConnection.Connect())
                    {
                        int position = workWithConnection.AutorisateUser(Login.Text, passwdTmp);

                        if (position <= 0)
                        {
                            MessageBox.Show("Неправельный логин или пароль", "Ошибка введённых данных");
                            return;
                        }

                        int employee = int.Parse(workWithConnection.NewCommand($"SELECT ID_employee FROM Credentials WHERE Login = '{Login.Text}' AND Password = '{passwdTmp}'", true).ToString());
                        workWithConnection.Disconnect();

                        RedirectEmployee(position, employee);
                    }
                    else
                        MessageBox.Show("Ошибка соединения с сервером.\nПожалуйста обратитесь к вашему системному администратору\nили попробуйте позже", "Ошибка подключения");
                }
            }
            else
            {
                MessageBox.Show("Неправельный логин или пароль", "Ошибка введённых данных");
            }
        }

        private void RedirectEmployee(int employeePosition, int employee)
        {
            Window window;

            if (employeePosition == 1)
            {
                window = new MainAdministratorWindow(employee, employeePosition);
            }
            else if (employeePosition == 2)
            {
                window = new AdministratorWindow(employee, employeePosition);
            }
            else if (employeePosition == 3)
            {
                window = new SupportWindow(employee);
            }
            else return;

            Application.Current.MainWindow = window;
            window.Show();
            this.Close();
        }

        private void Login_GotFocus(object sender, RoutedEventArgs e)
        {
            if(Login.Text == LOGIN_PLACEHOLDER)
                Login.Text = null;

            Login.Foreground = Brushes.Black;
        }

        private void Login_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(Login.Text) || Login.Text == LOGIN_PLACEHOLDER)
            {
                Login.Text = LOGIN_PLACEHOLDER;
                Login.Foreground = Brushes.Gray;
            }
        }

        private void Autorisate_Click(object sender, RoutedEventArgs e)
        {
            AutorisateUser();
        }

        private void CloseApplication_Click(object sender, RoutedEventArgs e)
        {
            this.Close();
        }

        private void LockSpace(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void Password_PasswordChanged(object sender, RoutedEventArgs e)
        {
            if(_passwordHidden)
                OpenedPassword.Text = Password.Password;
        }

        private void OpenedPassword_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (!_passwordHidden)
                Password.Password = OpenedPassword.Text;
        }

        private void ShowPassword_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {

            if (_passwordHidden)
            {
                ShowPassword.Text = "Скрыть пароль";
                Password.Visibility = Visibility.Collapsed;
                OpenedPassword.Visibility = Visibility.Visible;

                if (string.IsNullOrEmpty(OpenedPassword.Text))
                {
                    PasswordWaterMark.Visibility = Visibility.Visible;
                }
                
                _passwordHidden = false;
            }
            else
            {
                ShowPassword.Text = "Показать пароль";
                Password.Visibility = Visibility.Visible;
                OpenedPassword.Visibility = Visibility.Collapsed;

                if (string.IsNullOrEmpty(Password.Password))
                {
                    PasswordWaterMark.Visibility = Visibility.Visible;
                }

                _passwordHidden = true;
            }
        }

        private void OpenedPassword_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordWaterMark.Visibility = Visibility.Collapsed;
            OpenedPassword.SelectionStart = OpenedPassword.Text.Length;
        }

        private void Password_GotFocus(object sender, RoutedEventArgs e)
        {
            PasswordWaterMark.Visibility = Visibility.Collapsed;
            SetSelection(Password, Password.Password.Length, 0);
        }

        private void Password_LostFocus(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(Password.Password) || string.IsNullOrEmpty(OpenedPassword.Text))
            {
                PasswordWaterMark.Visibility = Visibility.Visible;
            }
        }

        private void OpenedPassword_LostFocus(object sender, RoutedEventArgs e)
        {
            if(string.IsNullOrEmpty(Password.Password) || string.IsNullOrEmpty(OpenedPassword.Text))
            {
                PasswordWaterMark.Visibility = Visibility.Visible;
            }
        }

        private void PasswordWaterMark_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (_passwordHidden)
            {
                Password.Visibility = Visibility.Visible;
                OpenedPassword.Visibility = Visibility.Collapsed;
                Password.Focus();
            }
            else
            {
                OpenedPassword.Visibility = Visibility.Visible;
                Password.Visibility = Visibility.Collapsed;
                OpenedPassword.Focus();
            }

            PasswordWaterMark.Visibility = Visibility.Collapsed;
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ButtonState == MouseButtonState.Pressed)
                DragMove();
        }

        private void EnterDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                AutorisateUser();
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Directory.CreateDirectory($"{AppOptions.CurrentDisk}\\Диплом\\Images");
            
            Login.PreviewTextInput += OptionsOfText.DisableRussianKeybord;
            Login.PreviewKeyDown += OptionsOfText.LockSpace;
            Password.PreviewTextInput += OptionsOfText.DisableRussianKeybord;
            Password.PreviewKeyDown += OptionsOfText.LockSpace;
            OpenedPassword.PreviewTextInput += OptionsOfText.DisableRussianKeybord;
            OpenedPassword.PreviewKeyDown += OptionsOfText.LockSpace;
        }
    }
}
