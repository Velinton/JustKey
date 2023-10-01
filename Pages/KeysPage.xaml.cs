using JustKey.Classes;
using JustKey.Windows;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Navigation;

namespace JustKey.Pages
{
    public partial class KeysPage : Page
    {
        private int _currentTab = 1;
        private int _startIndex = 0;
        private int _endIndex = 9;
        private object FilterRequest;
        private const string SEARCH_PLACEHOLDER = "Найти...";
        private WorkWithConnection _workWithConnection;
        private readonly List<object> _dataEditedLists = new List<object>();
        private readonly List<Purchase> _purchases = new List<Purchase>();
        private readonly List<PurchaseStatus> _statuses = new List<PurchaseStatus>();
        private readonly List<GameProvider> _providers = new List<GameProvider>();

        public KeysPage()
        {
            InitializeComponent();
        }

        private void LoadViews()
        {
            TableWithData.ItemsSource = null;
            List<object> tempList = new List<object>();

            if (_currentTab == 1 && _providers.Count > 0)
            {
                for (int i = 0; i < _providers.Count; i++)
                {
                    if (i < _startIndex || i >= _endIndex)
                        continue;
                    else if (i > _endIndex)
                        break;

                    tempList.Add(_providers[i]);
                }
                LastPageNumber.Text = (_providers.Count / 9 + 1).ToString();
            }
            else if(_currentTab == 2 && _purchases.Count > 0)
            {
                for (int i = 0; i < _purchases.Count; i++)
                {
                    if (i < _startIndex || i >= _endIndex)
                        continue;
                    else if (i > _endIndex)
                        break;

                    tempList.Add(_purchases[i]);
                }
                LastPageNumber.Text = (_purchases.Count / 9 + 1).ToString();
            }

            TableWithData.ItemsSource = tempList;
        }

        private void LoadEditedViews()
        {
            List<object> tempList = new List<object>();
            TableWithData.ItemsSource = null;

            for (int i = 0; i < _dataEditedLists.Count; i++)
            {
                if (i < _startIndex || i >= _endIndex)
                    continue;
                else if (i > _endIndex)
                    break;

                tempList.Add(_dataEditedLists[i]);
            }
            LastPageNumber.Text = (_providers.Count / 9 + 1).ToString();

            TableWithData.ItemsSource = tempList;
        }

        private void Load()
        {
            LoadDataFromDB();
            LoadProviders();
            LoadViewForFilter(_currentTab);
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if ((int)Window.GetWindow(this).GetType().GetProperty("GetTypeEmployee").GetValue(Window.GetWindow(this)) > 1)
            {
                NavigationButtons.Children.Remove(OpenEmployeesPage);
                OpenKeysPage.Margin = new Thickness(0, 150, 0, 0);
            }

            Load();
            LoadViews();
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            AutorisateWindow main = new AutorisateWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Window.GetWindow(this).Close();
        }

        private void OpenEmployeesPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesPage());
        }

        private void OpenGamesPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamesPage());
        }

        private void OpenNewRaportPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RaportsPage());
        }

        private void ColorizeButton(object sender, MouseEventArgs e)
        {
            (sender as Button).Style = (Style)FindResource("RoundedGradientButton");
        }

        private void UnColorizeButton(object sender, MouseEventArgs e)
        {
            (sender as Button).Style = (Style)FindResource("RoundedDefaultButton");
        }

        private void LoadDataFromDB()
        {
            List<Product> products = new List<Product>();
            List<string> employeesFullNames = new List<string>();
            _providers.Clear();
            _purchases.Clear();
            _workWithConnection = new WorkWithConnection();

            if (_workWithConnection.Connect())
            {
                string command = "SELECT ID, Name FROM Products";
                _workWithConnection.NewCommand(command, returnValue: false);
                SqlDataReader dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    products.Add(new Product
                    {
                        ID = dataReader.GetInt32(0),
                        GameName = dataReader.GetString(1)
                    });
                }
                dataReader.Close();

                command = "SELECT * FROM Providers";
                _workWithConnection.NewCommand(command, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    _providers.Add(new GameProvider
                    {
                        ID = dataReader.GetInt32(0),
                        Name = dataReader.GetString(1),
                        Address = dataReader.GetString(2),
                        Email = dataReader.GetString(3),
                        PhoneNumber = dataReader.GetString(4),
                        LastPurchaseDate = (dataReader.GetValue(5) as string) ?? "Закупки не было"
                    }) ;
                }
                dataReader.Close();

                command = "SELECT * FROM Purchases";
                _workWithConnection.NewCommand(command, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    _purchases.Add(new Purchase
                    {
                        ID = dataReader.GetInt32(0),
                        EmployeeFullName = dataReader.GetValue(1).ToString(),
                        ProviderName = dataReader.GetValue(2).ToString(),
                        GameName = dataReader.GetValue(3).ToString(),
                        KeysCount = dataReader.GetInt32(4),
                        FullPrice = dataReader.GetInt32(5),
                        Date = dataReader.GetString(6),
                        Status = dataReader.GetValue(7).ToString(),
                    });
                }
                dataReader.Close();

                command = "SELECT * FROM Purchase_statuses";
                _workWithConnection.NewCommand(command, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    _statuses.Add(new PurchaseStatus
                    {
                        ID = dataReader.GetInt32(0),
                        StatusName = dataReader.GetString(1)
                    });
                }
                dataReader.Close();

                foreach (var item in _purchases)
                {
                    command = $"SELECT Last_name + ' ' + Name + ' ' + Middle_name FROM Employees WHERE ID = {item.EmployeeFullName}";
                    employeesFullNames.Add(_workWithConnection.NewCommand(command, returnValue: true).ToString());
                }
                _workWithConnection.Disconnect();

                for(int i = 0; i < _purchases.Count; i++)
                {
                    _purchases[i].EmployeeFullName = employeesFullNames[i];
                    _purchases[i].ProviderName = _providers.Find(x => x.ID.ToString() == _purchases[i].ProviderName).Name;
                    _purchases[i].GameName = products.Find(x => x.ID.ToString() == _purchases[i].GameName).GameName;
                    _purchases[i].Status = _statuses.Find(x => x.ID.ToString() == _purchases[i].Status).StatusName;
                }
            }
            else
                MessageBox.Show("Ошибка подключения", "Ошибка");
        }

        private void LoadViewForFilter(int currentTable)
        {
            ContentGrid.Children.Remove((UIElement)FilterRequest);

            if (currentTable == 1)
            {
                FilterName.Text = "Дата последней закупки";

                DatePicker datePickerForFilter = new DatePicker
                {
                    Height = 25,
                    Width = 250,
                    BorderBrush = Brushes.Black,
                    Background = Brushes.White,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    SelectedDateFormat = DatePickerFormat.Short
                };

                datePickerForFilter.SelectedDateChanged += FilterRequest_SelectedDateChanged;
                Grid.SetRow(datePickerForFilter, 3);
                Grid.SetColumn(datePickerForFilter, 1);
                FilterRequest = datePickerForFilter;
                ContentGrid.Children.Add((DatePicker)FilterRequest);
            }
            else
            {
                FilterName.Text = "Статус закупки";

                ComboBox statusesForFilter = new ComboBox
                {
                    Height = 25,
                    Width = 250,
                    BorderBrush = Brushes.Black,
                    Background = Brushes.White,
                    VerticalContentAlignment = VerticalAlignment.Center,
                    HorizontalAlignment = HorizontalAlignment.Right,
                    VerticalAlignment = VerticalAlignment.Center,
                    FontSize = 16,
                    IsReadOnly = true,
                    IsEditable = true,
                    Text = "Выбрать из списка..."
                };

                statusesForFilter.SelectionChanged += Filter_SelectionChanged;
                Grid.SetRow(statusesForFilter, 3);
                Grid.SetColumn(statusesForFilter, 1);

                for (int i = 0; i < _statuses.Count; i++)
                {
                    statusesForFilter.Items.Add(_statuses[i].StatusName);
                }

                statusesForFilter.Items.Add("Сброс фильтра");
                FilterRequest = statusesForFilter;
                ContentGrid.Children.Add((ComboBox)FilterRequest);
            }
        }

        private void LoadProviders()
        {
            if (_providers.Count > 0)
            {
                _currentTab = 1;
                TableName.Text = "Поставщики";
                AddNew.Content = "Новый поставщик";
                TableWithData.ItemsSource = null;
                PurchasesTable.BorderBrush = Brushes.Transparent;
                
                DataGridColumn firstColumn = TableWithData.Columns[0];
                DataGridColumn lastColumn = TableWithData.Columns.Last();

                TableWithData.Columns.Clear();
                TableWithData.Columns.Add(firstColumn);

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Наименование",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Name")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Адрес",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Address")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Почта",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Email")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Телефон",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PhoneNumber")
                    },
                    Width = DataGridLength.Auto
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = $"Последняя закупка",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("LastPurchaseDate")
                    },
                    Width = DataGridLength.Auto
                });

                TableWithData.Columns.Add(lastColumn);
                LoadViews();
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void LoadPurchases()
        {
            if (_purchases.Count > 0)
            {
                _currentTab = 2;
                TableName.Text = "Поставки";
                AddNew.Content = "Оформить новую заявку";
                TableWithData.ItemsSource = null;
                ProvidersTable.BorderBrush = Brushes.Transparent;
                DataGridColumn firstColumn = TableWithData.Columns[0];
                DataGridColumn lastColumn = TableWithData.Columns.Last();

                TableWithData.Columns.Clear();
                TableWithData.Columns.Add(firstColumn);

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "ФИО сотрудника",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("EmployeeFullName")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Поставщик",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("ProviderName")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Игра",
                    Width = new DataGridLength(2, DataGridLengthUnitType.Auto),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("GameName")
                    }
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Копии",
                    Width = DataGridLength.Auto,
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("KeysCount")
                    }
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Cумма",
                    Width = DataGridLength.Auto,
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("FullPrice")
                    }
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Дата",
                    Width = DataGridLength.Auto,
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Date")
                    }
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Статус",
                    Width = DataGridLength.Auto,
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Status")
                    }
                });

                TableWithData.Columns.Add(lastColumn);
                LoadViews();
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void Filter_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if ((FilterRequest as ComboBox).SelectedIndex != -1 && (FilterRequest as ComboBox).SelectedValue.ToString() == "Сброс фильтра")
            {
                LoadPurchases();
                (FilterRequest as ComboBox).SelectedIndex = -1;
            }
            else if ((FilterRequest as ComboBox).SelectedIndex == -1)
            {
                LoadViewForFilter(_currentTab);
            }
            else
                Filter(_currentTab);
        }

        private void LoadViewForData_Click(object sender, RoutedEventArgs e)
        {
            (sender as Button).BorderBrush = (Brush)new BrushConverter().ConvertFrom("#784ff2");

            if ((sender as Button).Name == "ProvidersTable" && _currentTab != 1)
            {
                LoadProviders();
                LoadViewForFilter(_currentTab);
            } 
            else if((sender as Button).Name == "PurchasesTable" && _currentTab != 2)
            {
                LoadPurchases();
                LoadViewForFilter(_currentTab);
            } 
        }

        private void Search(int currentTable)
        {
            _dataEditedLists.Clear();
            if (currentTable == 1)
            {
                foreach (var item in _providers)
                    if (Regex.IsMatch($"{item.GetType().GetProperty("Name").GetValue(item).ToString().ToLower()} " +
                                      $"{item.GetType().GetProperty("Email").GetValue(item).ToString().ToLower()} " +
                                      $"{item.GetType().GetProperty("PhoneNumber").GetValue(item).ToString().ToLower()}", $"{SearchRequest.Text.ToLower()}.*"))
                        _dataEditedLists.Add(item);
            }
            else
            {
                foreach (var item in _purchases)
                    if (Regex.IsMatch($"{item.GetType().GetProperty("Date").GetValue(item).ToString().ToLower()} " +
                                      $"{item.GetType().GetProperty("ProviderName").GetValue(item).ToString().ToLower()} " +
                                      $"{item.GetType().GetProperty("GameName").GetValue(item).ToString().ToLower()}", $"{SearchRequest.Text.ToLower()}.*"))
                        _dataEditedLists.Add(item);
            }

            _startIndex = 0;
            _endIndex = 9;
            LastPageNumber.Text = (_dataEditedLists.Count / 9 + 1).ToString();
            LoadEditedViews();
        }

        private void SearchRequest_TextChanged(object sender, TextChangedEventArgs e)
        {
            if ((sender as TextBox).Text != SEARCH_PLACEHOLDER &&
                !string.IsNullOrEmpty((sender as TextBox).Text))
                Search(_currentTab);
            else if (string.IsNullOrEmpty((sender as TextBox).Text))
            {
                if (_currentTab == 1)
                    LoadProviders();
                else
                    LoadPurchases();
            }
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

        private void Filter(int currentTable)
        {
            _dataEditedLists.Clear();
            if (currentTable == 1)
            {
                foreach (var item in _providers)
                    if (item.LastPurchaseDate == (FilterRequest as DatePicker).SelectedDate.Value.ToShortDateString())
                        _dataEditedLists.Add(item);
            }
            else
            {
                foreach (var item in _purchases)
                    if ((FilterRequest as ComboBox).SelectedValue.ToString() == item.Status)
                        _dataEditedLists.Add(item);
            }

            _startIndex = 0;
            _endIndex = 9;
            LastPageNumber.Text = (_dataEditedLists.Count / 9 + 1).ToString();
            LoadEditedViews();
        }

        private void EditRow_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTab == 1)
            {
                AddProviderWindow addProvider = new AddProviderWindow((GameProvider)TableWithData.SelectedItem);
                if (addProvider.ShowDialog() == true)
                {
                    UpdateDataInDataBase(addProvider.GetProvider, 1);
                    LoadDataFromDB();
                    LoadProviders();
                }
            }
            else
            {
                AddPurchaseWindow addPurchase = new AddPurchaseWindow((Purchase)TableWithData.SelectedItem);
                if (addPurchase.ShowDialog() == true)
                {
                    UpdateDataInDataBase(addPurchase.GetPurchase, 2);
                    LoadDataFromDB();
                    LoadPurchases();
                }
            }
        }

        private void UpdateDataInDataBase(object data, int tableNumber)
        {
            string command;

            if (tableNumber == 1)
            {
                command = $"UPDATE Providers SET Name = '{data.GetType().GetProperty("Name").GetValue(data)}', Address = '{data.GetType().GetProperty("Address").GetValue(data)}', " +
                          $"Email ='{data.GetType().GetProperty("Email").GetValue(data)}', Phone_Number = '{data.GetType().GetProperty("PhoneNumber").GetValue(data)}', " +
                          $"Last_purchase_date = '{data.GetType().GetProperty("LastPurchaseDate").GetValue(data)}' " +
                          $"WHERE ID = {data.GetType().GetProperty("ID").GetValue(data)}";

            }
            else
            {
                command = $"UPDATE Purchases Set ID_employee = {(Window.GetWindow(this) as MainAdministratorWindow).GetIdAutorizateEmployee}, " +
                          $"ID_provider = (SELECT ID FROM Providers WHERE Name = '{data.GetType().GetProperty("ProviderName").GetValue(data)}'), " +
                          $"ID_product = (SELECT ID FROM Products WHERE Name = '{data.GetType().GetProperty("GameName").GetValue(data)}'), " +
                          $"Count = {data.GetType().GetProperty("KeysCount").GetValue(data)}," +
                          $"Price = {data.GetType().GetProperty("FullPrice").GetValue(data)}," +
                          $"Date = '{data.GetType().GetProperty("Date").GetValue(data)}', " +
                          $"ID_status = (SELECT ID FROM Purchase_statuses WHERE Status_name = '{data.GetType().GetProperty("Status").GetValue(data)}') " +
                          $"WHERE ID = {data.GetType().GetProperty("ID").GetValue(data)}";
            }

            if (_workWithConnection.Connect())
            {
                _workWithConnection.NewCommand(command, returnValue: false);
                _workWithConnection.Disconnect();
                MessageBox.Show("Данные были успешно обновлены", "Данные обновлены", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Не удаётся подключиться к базе данных", "Ошибка обновления", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteDataFromDB(List<int> data, int tableNumber)
        {
            string command;
            TableWithData.ItemsSource = null;

            if (tableNumber == 1)
            {
                command = "DELETE FROM Providers WHERE ID IN (";
                for (int i = 0; i < data.Count; i++)
                {
                    _providers.Remove(_providers.Find(x => x.ID == data[i]));
                }
                LoadProviders();
            }
            else
            {
                command = "DELETE FROM Purchases WHERE ID IN (";
                for (int i = 0; i < data.Count; i++)
                {
                    _purchases.Remove(_purchases.Find(x => x.ID == data[i]));
                }
                LoadPurchases();
            }  

            foreach (int ID in data)
            {
                command += $"{ID}";
                if (data.Last() != ID)
                    command += ", ";
                else
                    command += ')';
            }

            if (_workWithConnection.Connect())
            {
                _workWithConnection.NewCommand(command, returnValue: false);
                _workWithConnection.Disconnect();
                MessageBox.Show("Данные были успешно удалены", "Данные удалены", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Не удаётся подключиться к базе данных", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void DeleteRow_Click(object sender, RoutedEventArgs e)
        {
            List<int> dataForDelete = new List<int>();
            string message;

            foreach (var item in TableWithData.Items)
            {
                if ((bool)item.GetType().GetProperty("IsChecked").GetValue(item)) //Получить значение свойства IsChecked у объекта Item
                    dataForDelete.Add((int)item.GetType().GetProperty("ID").GetValue(item));
            }

            if (dataForDelete.Count > 0)
            {
                if(_currentTab == 1)
                {
                    if (dataForDelete.Count == 1)
                        message = "Вы уверены, что хотите безвозвратно удалить этого поставщика?\nВсе данные, связанные с ним будут удалены!";
                    else
                        message = "Вы уверены, что хотите безвозвратно удалить этих поставщиков?\nВсе данные, связанные с ними будут удалены!";
                }
                else
                {
                    if (dataForDelete.Count == 1)
                        message = "Вы уверены, что хотите безвозвратно удалить данные об этой закупке?\nВсе данные, связанные с ней будут удалены!";
                    else
                        message = "Вы уверены, что хотите безвозвратно удалить данные об этих закупках?\nВсе данные, связанные с ними будут удалены!";
                }

                if (MessageBox.Show(message, "Удаление данных", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                    DeleteDataFromDB(dataForDelete, _currentTab);
            }
            else if(TableWithData.SelectedItems.Count > 0)
            {
                if (_currentTab == 1)
                    message = "Вы уверены, что хотите безвозвратно удалить этого поставщика?\nВсе данные, связанные с ним будут удалены!";
                else
                    message = "Вы уверены, что хотите безвозвратно удалить данные об этой закупке?\nВсе данные, связанные с ней будут удалены!";

                if (MessageBox.Show(message, "Удаление данных", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
                {
                    dataForDelete.Add((int)TableWithData.SelectedItem.GetType().GetProperty("ID").GetValue(TableWithData.SelectedItem));
                    DeleteDataFromDB(dataForDelete, _currentTab);
                }  
            }
            else
                MessageBox.Show("Строки для удаления не выбраны", "Ошибка удаления", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void ViewForCheck(bool setValue)
        {
            List<object> tempList = new List<object>();

            foreach (var item in TableWithData.ItemsSource)
            {
                item.GetType().GetProperty("IsChecked").SetValue(item, setValue);
                tempList.Add(item);
            }

            TableWithData.ItemsSource = tempList;
        }

        private void CheckBox_Checked(object sender, RoutedEventArgs e)
        {
            ViewForCheck(true);
        }

        private void CheckBox_Unchecked(object sender, RoutedEventArgs e)
        {
            ViewForCheck(false);
        }

        private void AddNew_Click(object sender, RoutedEventArgs e)
        {
            if (_currentTab == 1)
            {
                AddProviderWindow addProvider = new AddProviderWindow();
                if (addProvider.ShowDialog() == true)
                {
                    AddNewDataInDataBase(addProvider.GetProvider, 1);
                    LoadDataFromDB();
                    LoadProviders();
                }
            }
            else
            {
                AddPurchaseWindow addPurchase = new AddPurchaseWindow();
                if (addPurchase.ShowDialog() == true)
                {
                    AddNewDataInDataBase(addPurchase.GetPurchase, 2);
                    LoadDataFromDB();
                    LoadPurchases();
                }
            }

        }

        private void AddNewDataInDataBase(object data, int tableNumber)
        {
            string command;

            if (tableNumber == 1)
            {
                command = $"INSERT Providers VALUES('{data.GetType().GetProperty("Name").GetValue(data)}', '{data.GetType().GetProperty("Address").GetValue(data)}', " +
                          $"'{data.GetType().GetProperty("Email").GetValue(data)}', '{data.GetType().GetProperty("PhoneNumber").GetValue(data)}', NULL)";
            }
            else
            {
                command = $"INSERT Purchases VALUES ({(Window.GetWindow(this) as MainAdministratorWindow).GetIdAutorizateEmployee}, " +
                          $"(SELECT ID FROM Providers WHERE Name = '{data.GetType().GetProperty("ProviderName").GetValue(data)}'), " +
                          $"(SELECT ID FROM Products WHERE Name = '{data.GetType().GetProperty("GameName").GetValue(data)}'), " +
                          $"{data.GetType().GetProperty("KeysCount").GetValue(data)}," +
                          $"{data.GetType().GetProperty("FullPrice").GetValue(data)}," +
                          $"'{DateTime.Now.ToShortDateString()}', " +
                          $"1)";
            }

            if (_workWithConnection.Connect())
            {
                _workWithConnection.NewCommand(command, returnValue: false);
                _workWithConnection.Disconnect();
                MessageBox.Show("Данные были успешно добавлены", "Данные добавлены", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Не удаётся подключиться к базе данных", "Ошибка добавления", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        private void FilterRequest_SelectedDateChanged(object sender, SelectionChangedEventArgs e)
        {
            if (!string.IsNullOrEmpty((sender as DatePicker).SelectedDate.ToString()))
                Filter(_currentTab);
            else
                LoadProviders();
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            int current = int.Parse(CurrentPageNumber.Text.ToString());
            int maxPage = int.Parse(LastPageNumber.Text.ToString());

            if (current > 1 && current <= maxPage)
            {
                current--;
                CurrentPageNumber.Text = current.ToString();
                _startIndex -= 9;
                _endIndex -= 9;
                LoadViews();
            }
            else
                MessageBox.Show("Дальше некуда");
        }

        private void NextPage_Click(object sender, RoutedEventArgs e)
        {
            int current = int.Parse(CurrentPageNumber.Text.ToString());
            int maxPage = int.Parse(LastPageNumber.Text.ToString());

            if (current < maxPage && current >= 1)
            {
                current++;
                CurrentPageNumber.Text = current.ToString();
                _startIndex += 9;
                _endIndex += 9;
                LoadViews();
            }
            else
                MessageBox.Show("Дальше некуда");
        }

        private void ToEnd_Click(object sender, RoutedEventArgs e)
        {
            int current = int.Parse(CurrentPageNumber.Text.ToString());
            int maxPage = int.Parse(LastPageNumber.Text.ToString());

            if (current != maxPage)
            {
                current = maxPage;
                CurrentPageNumber.Text = current.ToString();

                _endIndex = 9 * maxPage;
                _startIndex = _endIndex - 9;                
                LoadViews();
            }
            else
                MessageBox.Show("Открыта последняя страница");
        }

        private void ToStart_Click(object sender, RoutedEventArgs e)
        {
            int current = int.Parse(CurrentPageNumber.Text.ToString());

            if (current != 1)
            {
                current = 1;
                CurrentPageNumber.Text = current.ToString();

                _startIndex = 0;
                _endIndex = 9;
                LoadViews();
            }
            else
                MessageBox.Show("Открыта первая страница");
        }

        private void LoadInputedPage()
        {
            if (string.IsNullOrEmpty(CurrentPageNumber.Text))
            {
                CurrentPageNumber.Text = "1";
                _startIndex = 0;
                _endIndex = 9;
                MessageBox.Show("Будет загружена первая страница т.к. введён недопустимый номер страницы");
                LoadViews();
                return;
            }

            int current = int.Parse(CurrentPageNumber.Text.ToString());
            int maxPage = int.Parse(LastPageNumber.Text.ToString());

            if (current <= maxPage && current >= 1)
            {
                _endIndex = current * 9;
                _startIndex = _endIndex - 9;
            }
            else
                MessageBox.Show("Введён номер не существующей страницы");

            LoadViews();
        }

        private void CurrentPageNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if(e.Key == Key.Enter)
            {
                LoadInputedPage();
            }
            else if(e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsNumber(e.Text[0]))
                e.Handled = true;
        }

        private void CurrentPageNumber_TextChanged(object sender, TextChangedEventArgs e)
        {
            if (CurrentPageNumber.Text == "0")
                CurrentPageNumber.Text = "1";
        }

        private void CurrentPageNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            LoadInputedPage();
        }
    }
}