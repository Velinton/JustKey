using JustKey.Classes;
using JustKey.Windows;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Net;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;

namespace JustKey.Pages
{
    public partial class GamesPage : Page
    {
        private const string SEARCH_PLACEHOLDER = "Поиск...";
        private int _startIndex = 0;
        private int _endIndex = 10;
        private WorkWithConnection _workWithConnection;
        private List<Product> _products = new List<Product>();

        public GamesPage()
        {
            InitializeComponent();
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if ((int)Window.GetWindow(this).GetType().GetProperty("GetTypeEmployee").GetValue(Window.GetWindow(this)) > 1)
            {
                NavigationButtons.Children.Remove(OpenEmployeesPage);
                OpenKeysPage.Margin = new Thickness(0, 150, 0, 0);
            }
            Load(true);
        }

        private void Load(bool loadDataFromDB)
        {
            if (loadDataFromDB)
                LoadDataFromDB();
            LoadViews();
            DisplaySettings();
        }

        private void LoadDataFromDB()
        {
            _products.Clear();
            _workWithConnection = new WorkWithConnection();

            if (_workWithConnection.Connect())
            {
                string commnad = "SELECT * FROM Products";
                List<GameImage> tmp = new List<GameImage>();
                _workWithConnection.NewCommand(commnad, returnValue: false);
                SqlDataReader dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    _products.Add(new Product
                    {
                        ID = dataReader.GetInt32(0),
                        GameName = dataReader.GetString(2),
                        Price = dataReader.GetInt32(3),
                        Count = dataReader.GetInt32(4),
                        GameDescription = dataReader.GetString(5),
                        SystemRequirements = dataReader.GetString(6)
                    });
                }
                dataReader.Close();

                commnad = "SELECT * FROM Game_images";
                _workWithConnection.NewCommand(commnad, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    tmp.Add(new GameImage { ImageType = dataReader.GetInt32(1), Game_ID = dataReader.GetInt32(2), ImageData = (byte[])dataReader.GetValue(3), Name = dataReader.GetString(4)});
                }
                dataReader.Close();
                _workWithConnection.Disconnect();

                foreach (var product in _products)
                {
                    product.Images = new List<GameImage>();
                    product.Images.AddRange(tmp.FindAll(p => p.Game_ID == product.ID));
                }
            }
            else
                MessageBox.Show("Ошибка подключения", "Ошибка");
        }

        private void LoadViews()
        {
            gamesViews.Children.Clear();
            if (_products.Count > 0)
            {
                for (int i = 0; i < _products.Count; i++)
                {
                    if (i < _startIndex || i >= _endIndex)
                        continue;
                    else if (i > _endIndex)
                        break;

                    string path = string.Empty;

                    StackPanel gameView = new StackPanel
                    {
                        Background = Brushes.AliceBlue,
                        Margin = new Thickness(15, 15, 0, 0),
                        Cursor = Cursors.Hand
                    };

                    if (_products[i].Images.Count == 0)
                        path = $"{AppOptions.ErrorImage}";
                    else
                    {
                        if (!Directory.Exists($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_products[i].GameName.Replace(":", " ")}"))
                            Directory.CreateDirectory($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_products[i].GameName.Replace(":"," ")}");

                        foreach (var image in _products[i].Images.FindAll(p => p.ImageType == 1))
                        {
                            using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_products[i].GameName.Replace(":", " ")}\\{image.Name}", FileMode.OpenOrCreate))
                            {
                                fs.Write(image.ImageData, 0, image.ImageData.Length);
                                path = $"{fs.Name}";
                            }
                        }
                    }

                    Label ID = new Label
                    {
                        Content = _products[i].ID,
                        Visibility = Visibility.Collapsed
                    };

                    Image gamePreview = new Image
                    {
                        Margin = new Thickness(5, 5, 5, 0),
                        Stretch = Stretch.Uniform,
                        Height = 220
                    };

                    var pathToCover = new BitmapImage();
                    pathToCover.BeginInit();
                    pathToCover.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                    pathToCover.CacheOption = BitmapCacheOption.OnLoad;
                    if (path == AppOptions.ErrorImage)
                        pathToCover.UriSource = new Uri($"{path}", UriKind.Relative);
                    else
                        pathToCover.UriSource = new Uri($"{path}");
                    pathToCover.EndInit();

                    gamePreview.Source = pathToCover;

                    TextBlock gameName = new TextBlock
                    {
                        HorizontalAlignment = HorizontalAlignment.Center,
                        TextWrapping = TextWrapping.Wrap,
                        FontSize = 16,
                        TextAlignment = TextAlignment.Center,
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    if (_products[i].GameName.Length > 20)
                    {
                        gameName.Text = $"{_products[i].GameName.Remove(21)}...";
                    }
                    else
                    {
                        gameName.Text = $"{_products[i].GameName}";
                    }

                    gameView.Children.Add(ID);
                    gameView.Children.Add(gamePreview);
                    gameView.Children.Add(gameName);
                    gameView.MouseLeftButtonDown += GameView_MouseLeftButtonDown;

                    gamesViews.Children.Add(gameView);

                }
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void GameView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            InfoAboutGameWindow infoAboutGame = new InfoAboutGameWindow
            {
                SetGame = _products.Find(x => x.ID == int.Parse(((sender as StackPanel).Children[0] as Label).Content.ToString()))
            };

            if (infoAboutGame.ShowDialog() == true)
                Load(true);
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

        private void ReloadPage_Click(object sender, RoutedEventArgs e)
        {
            FilterRequest.SelectedIndex = -1;
            FilterRequest.Text = "Фильтровать по...";
            Load(loadDataFromDB: true);
        }

        private void DisplaySettings()
        {
            if (_products.Count > 10)
            {
                MoveToRight.Style = (Style)FindResource("RoundedDefaultButton");
                MoveToEndList.Style = (Style)FindResource("RoundedDefaultButton");
            }
        }

        private void AddNewGame_Click(object sender, RoutedEventArgs e)
        {
            InfoAboutGameWindow addNewGame = new InfoAboutGameWindow(isNewGame: true);
            if (addNewGame.ShowDialog() == true)
                Load(true);
        }

        private void LogOut_Click(object sender, RoutedEventArgs e)
        {
            AutorisateWindow main = new AutorisateWindow();
            Application.Current.MainWindow = main;
            main.Show();
            Window.GetWindow(this).Close();
        }

        private void MoveToRight_Click(object sender, RoutedEventArgs e)
        {
            MoveToLeft.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToStartList.Style = (Style)FindResource("RoundedDefaultButton");

            _startIndex += 10;
            _endIndex += 10;

            LoadViews();

            if (gamesViews.Children.Count <= 10 &&
            _endIndex >= _products.Count)
            {
                MoveToRight.Style = (Style)FindResource("BlockButton");
                MoveToEndList.Style = (Style)FindResource("BlockButton");
            }
        }

        private void MoveToLeft_Click(object sender, RoutedEventArgs e)
        {
            MoveToRight.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToEndList.Style = (Style)FindResource("RoundedDefaultButton");

            _startIndex -= 10;
            _endIndex -= 10;
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
            _endIndex = 10;
            LoadViews();

            MoveToLeft.Style = (Style)FindResource("BlockButton");
            MoveToStartList.Style = (Style)FindResource("BlockButton");
        }

        private void MoveToEndList_Click(object sender, RoutedEventArgs e)
        {
            MoveToLeft.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToStartList.Style = (Style)FindResource("RoundedDefaultButton");

            if (_products.Count % 10 != 0)
                _startIndex = _products.Count - (_products.Count % 10);
            else
                _startIndex = _products.Count - 10;

            _endIndex = _startIndex + 10;

            LoadViews();

            MoveToRight.Style = (Style)FindResource("BlockButton");
            MoveToEndList.Style = (Style)FindResource("BlockButton");
        }

        private List<Product> Filter(List<Product> games, int selectedIndex)
        {
            if (selectedIndex == 0)
                games.RemoveAll(x => x.Images.Count == 0);
            else if (selectedIndex == 1)
                games.RemoveAll(x => x.Images.Count != 0);
            return games;
        }

        private List<Product> Search(List<Product> games)
        {
            List<Product> editedListWithGames = new List<Product>();

            foreach (var game in games)
                if (Regex.IsMatch($"{game.GameName.ToLower()}", $"{SearchRequest.Text.ToLower()}.*"))
                    editedListWithGames.Add(game);

            return editedListWithGames;
        }

        private void FilterRequest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterRequest.SelectedIndex != -1)
            {
                _products = Filter(_products, FilterRequest.SelectedIndex);
                Load(loadDataFromDB: false);
            }
        }

        private void SearchStart_Click(object sender, RoutedEventArgs e)
        {
            _products = Search(_products);
            Load(loadDataFromDB: false);
        }

        private void SearchRequest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
                _products = Search(_products);
            Load(loadDataFromDB: false);
        }

        private void OpenEmployeesPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesPage());
        }

        private void OpenKeysPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new KeysPage());
        }

        private void OpenNewRaportPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new RaportsPage());
        }
    }
}
