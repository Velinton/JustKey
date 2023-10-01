using JustKey.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Net.Mail;
using System.Net;
using System.Windows;
using System.Windows.Input;

namespace JustKey.Windows
{
    public partial class AddPurchaseWindow : Window
    {
        private Purchase _purchase = new Purchase();
        private WorkWithConnection _workWithConnection;
        private bool _isEditing = false;

        public AddPurchaseWindow()
        {
            InitializeComponent();
            LoadProvidersAndProducts();
        }

        public AddPurchaseWindow(Purchase purchase)
        {
            InitializeComponent();
            _purchase = purchase;
            LoadProvidersAndProducts();
            LoadInfoAboutPurchase();
            PurchaseStatus.Visibility = Visibility.Visible;
            _isEditing = true;
        }

        private void LoadInfoAboutPurchase()
        {
            AddNewPurchase.Content = "Готово";
            ProviderName.SelectedIndex = ProviderName.Items.IndexOf(_purchase.ProviderName);
            ProductName.SelectedIndex = ProductName.Items.IndexOf(_purchase.GameName);
            KeysCount.Text = _purchase.KeysCount.ToString();
            KeysPrice.Text = (_purchase.FullPrice / _purchase.KeysCount).ToString();
            PurchaseStatus.SelectedIndex = PurchaseStatus.Items.IndexOf(_purchase.Status);
        }

        public void LoadProvidersAndProducts()
        {
            _workWithConnection = new WorkWithConnection();
            if (_workWithConnection.Connect())
            {
                string command = "SELECT Name FROM Providers";
                _workWithConnection.NewCommand(command, returnValue: false);

                SqlDataReader dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    ProviderName.Items.Add(dataReader.GetString(0));
                }
                dataReader.Close();

                command = "SELECT Name FROM Products";
                _workWithConnection.NewCommand(command, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    ProductName.Items.Add(dataReader.GetString(0));
                }
                dataReader.Close();

                command = "SELECT Status_name FROM Purchase_statuses";
                _workWithConnection.NewCommand(command, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    PurchaseStatus.Items.Add(dataReader.GetString(0));
                }
                dataReader.Close();
                _workWithConnection.Disconnect();
            }
        }

        private void SavePurchaseData()
        {
            if (PurchaseStatus.SelectedIndex == -1)
                PurchaseStatus.SelectedIndex = 1;

            _purchase = new Purchase
            {
                ID = _purchase.ID,
                ProviderName = ProviderName.Text,
                GameName = ProductName.Text,
                KeysCount = int.Parse(KeysCount.Text),
                FullPrice = int.Parse(KeysPrice.Text) * int.Parse(KeysCount.Text),
                Date = _purchase.Date,
                EmployeeFullName = _purchase.EmployeeFullName,
                Status = PurchaseStatus.Text
            };
        }

        private void IncrementCount_Click(object sender, RoutedEventArgs e)
        {
            KeysCount.Text = Increment(KeysCount.Text);
        }

        private void DecrementCount_Click(object sender, RoutedEventArgs e)
        {
            KeysCount.Text = Decrement(KeysCount.Text);
        }

        private void DecrementPrice_Click(object sender, RoutedEventArgs e)
        {
            KeysPrice.Text = Decrement(KeysPrice.Text);
        }

        private void IncrementPrice_Click(object sender, RoutedEventArgs e)
        {
            KeysPrice.Text = Increment(KeysPrice.Text);
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

        public Purchase GetPurchase
        {
            get { return _purchase; }
        }

        private int SendPurchaseToProvider()
        {
            WorkWithConnection connection = new WorkWithConnection();

            if (connection.Connect())
            {
                string providerEmail = (string)connection.NewCommand($"SELECT Email From Providers WHERE Name = '{_purchase.ProviderName}'", returnValue: true);

                connection.Disconnect();

                //Отправитель
                MailAddress myEmail = new MailAddress("RandomGam2@ma.com", "JustKey - Game Shop");
                //Получатель
                MailAddress clientEmail = new MailAddress(providerEmail);
                //Объект письма
                MailMessage message = new MailMessage(myEmail, clientEmail)
                {
                    //Тема письма
                    Subject = "Деловое предложение от JustKey",
                    //Текст письма
                    Body = $"<h2>Здравствуйте, {_purchase.ProviderName}</h2><br/><p> Мы бы хотели закупить ваш продукт {_purchase.GameName} в количестве {_purchase.KeysCount} ключей за {_purchase.FullPrice} ₽ <br/>" +
                          $"<h3>Если согласны на наше предложение, связжитесь с нами по адресу - JustKey@gmail.com, в течении 3-х недель.</h3></p>"
                };

                try
                {
                    SendEmail(message);
                    return 200; //STATUS - OK
                }
                catch (Exception ex) 
                { 
                    Console.WriteLine(ex.Message);
                    return 502; //STATUS - Connection Error
                }

            }
            else
            {
                return 500; //STATUS - Server Error
            }
            
        }

        private void SendEmail(MailMessage message)
        {
            //Письмо это код html
            message.IsBodyHtml = true;
            // адрес smtp-сервера и порт, с которого будем отправлять письмо
            SmtpClient smtp = new SmtpClient("smtp.gmail.com", 587)
            {
                // логин и пароль
                Credentials = new NetworkCredential("Random@malc.o", "mccv kzlw fqxg hqsr"),
                EnableSsl = true
            };
            smtp.Send(message);
        }

        private void AddNewPurchase_Click(object sender, RoutedEventArgs e)
        {
            if (KeysCount.Text != "0" && ProviderName.SelectedIndex >= 0 && KeysPrice.Text != "0" &&
                KeysCount.Text.Length > 0 && ProductName.SelectedIndex >= 0 && KeysPrice.Text.Length > 0)
            {
                int status = -1;
                
                SavePurchaseData();

                if(_isEditing == true)
                    status = 200;
                else
                    status = SendPurchaseToProvider();

                if (status == 200)
                {
                    this.DialogResult = true;
                }
                else
                {
                    if(status == 500)
                    {
                        MessageBox.Show("Ошибка связи с БД. Свяжитесь с вашим системным администратором", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                    else
                    {
                        MessageBox.Show("Ошибка соединения с интернетом. Проверьте подключение и повторите попытку", "Ошибка подключения", MessageBoxButton.OK, MessageBoxImage.Error);
                    }
                }
            }
        }

        private void Window_PreviewExecuted(object sender, ExecutedRoutedEventArgs e)
        {
            OptionsOfText.DisableCommands(sender, e);
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            this.PreviewTextInput += OptionsOfText.OnlyNumber;
            this.PreviewKeyDown += OptionsOfText.LockSpace;
        }
    }
}
