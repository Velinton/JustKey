using JustKey.Classes;
using System;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;

namespace JustKey.Windows
{
    /// <summary>
    /// Логика взаимодействия для AddProviderWindow.xaml
    /// </summary>
    public partial class AddProviderWindow : Window
    {
        private GameProvider _provider = new GameProvider();

        public AddProviderWindow()
        {
            InitializeComponent();
        }

        public AddProviderWindow(GameProvider provider)
        {
            InitializeComponent();
            _provider = provider;
            LoadInfoAboutProvider();
        }

        private void LoadInfoAboutProvider()
        {
            AddNewProvider.Content = "Готово";
            ProviderName.Text = _provider.Name;
            ProviderAddress.Text = _provider.Address;
            ProviderEmail.Text = _provider.Email;
            ProviderPhoneNumber.Text = _provider.PhoneNumber;
        }

        private void SaveProviderData()
        {
            _provider = new GameProvider()
            {
                ID = _provider.ID,
                Name = ProviderName.Text,
                Address = ProviderAddress.Text,
                Email = ProviderEmail.Text,
                PhoneNumber = ProviderPhoneNumber.Text,
                LastPurchaseDate = _provider.LastPurchaseDate
            };
        }

        private void CheckPhoneNumber(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsNumber(e.Text[0]) && e.Text[0] != '-' && e.Text[0] != '+')
                e.Handled = true;
        }

        private void OnlyLettersAndNumbers(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.Text[0]))
            {
                e.Handled = true;
            }
        }

        private void OnlyRussianKeybord(object sender, TextCompositionEventArgs e)
        {
            Regex GetReguliarReplase = new Regex("([а-яА-Я]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (GetReguliarReplase.Match(e.Text).Value != e.Text && e.Text[0] != 'ё')
            {
                e.Handled = true;
            }
        }

        private void StopRussianKeybord(object sender, TextCompositionEventArgs e)
        {
            Regex GetReguliarReplase = new Regex("([а-яА-Я]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (GetReguliarReplase.Match(e.Text).Value == e.Text)
            {
                e.Handled = true;
            }
        }

        private bool CheckEmail(string email)
        {
            if (Regex.IsMatch(email, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase) &&
                !string.IsNullOrEmpty(email))
                return true;
            else
                return false;
        }

        private void ProviderEmail_TextChanged(object sender, TextChangedEventArgs e)
        {
            TextBox email = sender as TextBox;

            if (!Regex.IsMatch(email.Text, @"^[^@\s]+@[^@\s]+\.[^@\s]+$", RegexOptions.IgnoreCase))
                email.Foreground = Brushes.Red;
            else
                email.Foreground = Brushes.Black;
        }

        private void TextBox_PreviewTextInput(object sender, TextCompositionEventArgs e)
        {
            if (!char.IsLetterOrDigit(e.Text[0]) && e.Text[0] != ',')
            {
                e.Handled = true;
            }
        }

        public GameProvider GetProvider
        {
            get { return _provider; }
        }

        private void Window_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OptionsOfText.DisableCommands(sender, e);
        }

        private void AddNewProvider_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(ProviderName.Text) && !string.IsNullOrEmpty(ProviderAddress.Text) &&
                CheckEmail(ProviderEmail.Text) && !string.IsNullOrEmpty(ProviderPhoneNumber.Text))
            {
                SaveProviderData();
                this.DialogResult = true;
            }
        }
    }
}
