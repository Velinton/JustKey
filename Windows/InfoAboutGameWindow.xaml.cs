using JustKey.Classes;
using Microsoft.Win32;
using System;
using System.Data;
using System.Data.SqlClient;
using System.IO;
using System.Linq;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace JustKey.Windows
{
    public partial class InfoAboutGameWindow : Window
    {
        
        private bool _isChanged = false;
        private readonly bool _isNewGame = false;

        WorkWithConnection _workWithConnection;
        Product _game = new Product();

        public InfoAboutGameWindow()
        {
            InitializeComponent();
        }

        public InfoAboutGameWindow(bool isNewGame)
        {
            InitializeComponent();
            _isNewGame = isNewGame;
        }

        public Product SetGame
        {
            set { _game = value; }
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            LoadAll();
        }

        async private void LoadAll()
        {
            _workWithConnection = new WorkWithConnection();
            CategoriesView.Text = "Жанр: ";
            DevelopersView.Text = "Разработчики: ";

            if (_workWithConnection.Connect())
            {
                string command;
                SqlDataReader dataReader;
                if (!_isNewGame)
                {
                    command = $"SELECT Name_of_category FROM Categories WHERE ID IN (SELECT ID_category FROM Games_by_categories WHERE ID_product = {_game.ID})";
                    _workWithConnection.NewCommand(command, returnValue: false);
                    dataReader = _workWithConnection.GetCommand.ExecuteReader();
                    while (dataReader.Read())
                    {
                        CategoriesView.Text += $"{dataReader.GetString(0)},";
                    }
                    dataReader.Close();

                    command = $"SELECT Developer_name FROM Developers WHERE ID IN (SELECT ID_developer FROM Games_by_developers WHERE ID_product = {_game.ID})";
                    _workWithConnection.NewCommand(command, returnValue: false);
                    dataReader = _workWithConnection.GetCommand.ExecuteReader();

                    while (dataReader.Read())
                    {
                        DevelopersView.Text += $"{dataReader.GetString(0)},";
                    }
                    dataReader.Close();
                }
                    

                command = $"SELECT Name_of_category FROM Categories";
                _workWithConnection.NewCommand(command, false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    GameCategories.Items.Add(dataReader.GetString(0));
                }
                dataReader.Close();

                command = $"SELECT Developer_name FROM Developers";
                _workWithConnection.NewCommand(command, false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    GameDeveloper.Items.Add(dataReader.GetString(0));
                }
                dataReader.Close();
                _workWithConnection.Disconnect();

                CategoriesView.Text = CategoriesView.Text.Remove(CategoriesView.Text.Length - 1);
                DevelopersView.Text = DevelopersView.Text.Remove(DevelopersView.Text.Length - 1);
            }

            NameOfGameView.Text = $"{_game.GameName}";
            ProductCounter.Text = $"{_game.Count}";
            ProductPrice.Text = $"{_game.Price}";

            DescriptionView.Text = _game.GameDescription;
            SystemRequitementsView.Text = _game.SystemRequirements;
            await LoadPhotos();
        }

        private async Task LoadPhotos()
        {
            if (!_isNewGame)
            {
                var path = new BitmapImage();
                string tmpPath;
                path.BeginInit();
                path.CacheOption = BitmapCacheOption.OnLoad;

                if(_game.Images.Count > 0)
                {
                    using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_game.GameName.Replace(":"," ")}\\{_game.Images.First(x => x.ImageType == 1).Name}", FileMode.OpenOrCreate))
                    {
                        fs.Write(_game.Images.Find(image => image.ImageType == 1).ImageData, 0, _game.Images.Find(image => image.ImageType == 1).ImageData.Length);
                        path.UriSource = new Uri(fs.Name);
                    }
                }
                else
                {
                    path.UriSource = new Uri($"{AppOptions.CurrentDisk}\\VS_projects\\Диплом\\JustKey\\Images\\errorImage.png");
                }

                path.EndInit();

                GameCover.Source = path;

                if(_game.Images.Count > 0)
                {
                    await Task.Run(() =>
                    {
                        foreach (var screenshot in _game.Images.FindAll(x => x.ImageType == 2))
                        {
                            using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{_game.GameName.Replace(":"," ")}\\{screenshot.Name}", FileMode.OpenOrCreate))
                            {
                                fs.Write(screenshot.ImageData, 0, screenshot.ImageData.Length);
                                tmpPath = fs.Name;
                            }

                            Dispatcher.Invoke(() =>
                            {
                                path = new BitmapImage();
                                path.BeginInit();
                                path.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                                path.CacheOption = BitmapCacheOption.OnLoad;
                                path.UriSource = new Uri(tmpPath);
                                path.EndInit();

                                Image image = new Image
                                {
                                    Source = path,
                                    Margin = new Thickness(3),
                                    Cursor = Cursors.Hand,
                                    ToolTip = "Кликни, чтобы удалить"
                                };

                                image.MouseLeftButtonDown += DeleteImage;
                                GameImages.Children.Add(image);
                            });
                        }
                    });
                }
            }
           
            Border border = new Border
            {
                Width = 160,
                Margin = new Thickness(3),
                HorizontalAlignment = HorizontalAlignment.Left,
                BorderBrush = Brushes.Black,
                BorderThickness = new Thickness(2)
            };

            Canvas canvas = new Canvas();
            Image imagePlaceHolder = new Image
            {
                Source = new BitmapImage((Uri)new UriTypeConverter().ConvertFromString("\\Images\\addphoto.png")),
                Height = 60,
                Width = 60,
                Cursor = Cursors.Hand
            };
            Canvas.SetLeft(imagePlaceHolder, 47);
            Canvas.SetBottom(imagePlaceHolder, 110);

            canvas.Children.Add(imagePlaceHolder);
            border.Child = canvas;
            border.MouseLeftButtonDown += AddImage;
            GameImages.Children.Add(border);

        }

        private void AddImage(object sender, MouseButtonEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Files|*.jpg;*.jpeg;*.png;*.JPG;*.webp;",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                Border tmpBorder = (Border)sender;
                var path = new BitmapImage();

                GameImages.Children.Remove((Border)sender);
                
                path.BeginInit();
                path.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                path.CacheOption = BitmapCacheOption.OnLoad;
                path.UriSource = new Uri(openFileDialog.FileName);
                path.EndInit();

                Image image = new Image
                {
                    Source = path,
                    Margin = new Thickness(3),
                    Cursor = Cursors.Hand,
                    ToolTip = "Кликни, чтобы удалить"
                };

                image.MouseLeftButtonDown += DeleteImage;
                GameImages.Children.Add(image);
                GameImages.Children.Add(tmpBorder);
            }
        }

        private void DeleteImage(object sender, MouseButtonEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить это изображение?", "Удаление фотографии", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                GameImages.Children.Remove((Image)sender);
            }
        }

        private void DeleteGame_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("Вы уверены, что хотите удалить эту игру и все данные связанные с ней?", "Удаление игры", MessageBoxButton.YesNo, MessageBoxImage.Warning) == MessageBoxResult.Yes)
            {
                if(_workWithConnection.Connect())
                {
                    string command = $"DELETE FROM Products WHERE ID = {_game.ID}";
                    _workWithConnection.NewCommand(command, false);
                    _workWithConnection.Disconnect();
                    _isChanged = true;
                    BackToGameList(Back, null);
                }
            }
        }

        private void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsNumber(e.Text[0]))
                e.Handled = true;
        }

        private void EditFirstInfo_Click(object sender, RoutedEventArgs e)
        {
            if (EditedNameOfGame.Visibility == Visibility.Visible)
            {
                EditedNameOfGame.Visibility = Visibility.Collapsed;
                NameOfGameView.Text = EditedNameOfGame.Text;
                NameOfGameView.Visibility = Visibility.Visible;
                
            }
            else
            {
                if (EditedNameOfGame.Visibility == Visibility.Visible)
                {
                    MessageBox.Show("Необходимо закрыть предыдущее редактирование текста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                EditedNameOfGame.Visibility = Visibility.Visible;
                NameOfGameView.Visibility = Visibility.Collapsed;
                
            }
        }

        private void EditDescription_Click(object sender, RoutedEventArgs e)
        {
            if (EditedDescription.Visibility == Visibility.Visible)
            {
                EditedDescription.Visibility = Visibility.Collapsed;
                DescriptionView.Text = EditedDescription.Text;
                DescriptionView.Visibility = Visibility.Visible;
            }
            else
            {
                if (EditedDescription.Visibility == Visibility.Visible)
                {
                    MessageBox.Show("Необходимо закрыть предыдущее редактирование текста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }

                EditedDescription.Visibility = Visibility.Visible;
                DescriptionView.Visibility = Visibility.Collapsed;
                
            }
        }

        private void EditSystemRequitements_Click(object sender, RoutedEventArgs e)
        {
            if (EditedSystemRequitements.Visibility == Visibility.Visible)
            {
                EditedSystemRequitements.Visibility = Visibility.Collapsed;
                SystemRequitementsView.Text = EditedSystemRequitements.Text;
                SystemRequitementsView.Visibility = Visibility.Visible;
            }
            else
            {
                if (EditedSystemRequitements.Visibility == Visibility.Visible)
                {
                    MessageBox.Show("Необходимо закрыть предыдущее редактирование текста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                EditedSystemRequitements.Visibility = Visibility.Visible;
                SystemRequitementsView.Visibility = Visibility.Collapsed;
                
            }
        }

        private void EditGeneralInfo_Click(object sender, RoutedEventArgs e)
        {
            if (EditedCategories.Visibility == Visibility.Visible ||
                EditedDevelopers.Visibility == Visibility.Visible)
            {
                EditedCategories.Visibility = Visibility.Collapsed;
                EditedDevelopers.Visibility = Visibility.Collapsed; 

                CategoriesView.Text = EditedCategories.Text;
                DevelopersView.Text = EditedDevelopers.Text;

                CategoriesView.Visibility = Visibility.Visible;
                DevelopersView.Visibility = Visibility.Visible;
                
                
            }
            else
            {
                if (EditedCategories.Visibility == Visibility.Visible)
                {
                    MessageBox.Show("Необходимо закрыть предыдущее редактирование текста", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
                    return;
                }
                EditedCategories.Visibility = Visibility.Visible;
                EditedDevelopers.Visibility = Visibility.Visible;
                
                CategoriesView.Visibility = Visibility.Collapsed;
                DevelopersView.Visibility = Visibility.Collapsed;
            }
        }

        private void EditGameCover_Click(object sender, RoutedEventArgs e)
        {
            OpenFileDialog openFileDialog = new OpenFileDialog
            {
                Filter = "Files|*.jpg;*.jpeg;*.png;*.JPG;*.webp;",
                RestoreDirectory = true
            };

            if (openFileDialog.ShowDialog() == true)
            {
                var path = new BitmapImage();
                path.BeginInit();
                path.CacheOption = BitmapCacheOption.OnLoad;
                path.CreateOptions = BitmapCreateOptions.IgnoreImageCache;
                path.UriSource = new Uri(openFileDialog.FileName);
                path.EndInit();
                GameCover.Source = path;
            }
        }

        private void IncrementCount_Click(object sender, RoutedEventArgs e)
        {
            ProductCounter.Text = Increment(ProductCounter.Text);
        }

        private void DecrementCount_Click(object sender, RoutedEventArgs e)
        {
            ProductCounter.Text = Decrement(ProductCounter.Text);
        }

        private void DecrementPrice_Click(object sender, RoutedEventArgs e)
        {
            ProductPrice.Text = Decrement(ProductPrice.Text);
        }

        private void IncrementPrice_Click(object sender, RoutedEventArgs e)
        {
            ProductPrice.Text = Increment(ProductPrice.Text);
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

        private void SaveNewData()
        {
            if (EditedCategories.Text.Last() == ',')
            {
                EditedCategories.Text = EditedCategories.Text.Remove(EditedCategories.Text.Length - 1, 1);
            }
            else if (EditedDevelopers.Text.Last() == ',')
            {
                EditedDevelopers.Text = EditedDevelopers.Text.Remove(EditedDevelopers.Text.Length - 1, 1);
            }

            string[] cats = EditedCategories.Text.Split(new char[] { ',' });
            string[] devels = EditedDevelopers.Text.Split(new char[] { ',' });

            for (int i = 0; i < cats.Length; i++)
            {
                cats[i] = cats[i].Replace("Жанр:", "");

                if (cats[i].Last() == ',')
                    cats[i] = cats[i].Remove(cats.Length - 1, 1);
                if (cats[i].First() == ' ')
                    cats[i] = cats[i].Remove(0, 1);
            }
            for (int i = 0; i < devels.Length; i++)
            {
                devels[i] = devels[i].Replace("Разработчики:", "");

                if (devels[i].Last() == ',')
                    devels[i] = devels[i].Remove(devels.Length - 1, 1);
                if (devels[i].First() == ' ')
                    devels[i] = devels[i].Remove(0, 1);
            }

            if (!string.IsNullOrEmpty(EditedNameOfGame.Text) && !string.IsNullOrEmpty(EditedDescription.Text) &&
                !string.IsNullOrEmpty(EditedSystemRequitements.Text) && !string.IsNullOrEmpty(EditedCategories.Text) &&
                !string.IsNullOrEmpty(EditedDevelopers.Text) && ProductCounter.Text != "0" && ProductPrice.Text != "0")
            {
                if (_workWithConnection.Connect())
                {
                    string command;
                    byte[] imageData;

                    if (!_workWithConnection.GetCommand.Parameters.Contains("@Description"))
                    {
                        _workWithConnection.GetCommand.Parameters.Add("@Description", SqlDbType.NVarChar);
                    }
                    _workWithConnection.GetCommand.Parameters["@Description"].Value = EditedDescription.Text;

                    if (_isNewGame)
                    {
                        command = $"INSERT Products VALUES (0,'{EditedNameOfGame.Text}', " +
                                  $"{ProductPrice.Text}, {ProductCounter.Text}, @Description, '{EditedSystemRequitements.Text}')";
                    }
                    else
                    {
                        command = $"UPDATE Products SET Name = '{EditedNameOfGame.Text}', " +
                                  $"Price = {ProductPrice.Text}, Count = {ProductCounter.Text}, " +
                                  $"Description = @Description, System_requirements = '{EditedSystemRequitements.Text}' WHERE ID = {_game.ID}\n" +
                                  $"DELETE FROM Games_by_categories WHERE ID_product = {_game.ID}\n" +
                                  $"DELETE FROM Games_by_developers WHERE ID_product = {_game.ID}\n" +
                                  $"DELETE FROM Game_images WHERE ID_product = {_game.ID}\n";
                    }
                    for (int i = 0; i < cats.Length; i++)
                    {
                        if (_isNewGame)
                        {
                            command += $"INSERT Games_by_categories VALUES ((SELECT IDENT_CURRENT('Products')),((SELECT ID FROM Categories WHERE Name_of_category = '{cats[i]}')))\n";
                        }
                        else
                        {
                            command += $"INSERT Games_by_categories VALUES ({_game.ID},((SELECT ID FROM Categories WHERE Name_of_category = '{cats[i]}')))\n";
                        }
                    }
                    for (int i = 0; i < devels.Length; i++)
                    {
                        if (_isNewGame)
                        {
                            command += $"INSERT Games_by_developers VALUES ((SELECT IDENT_CURRENT('Products')),((SELECT ID FROM Developers WHERE Developer_name ='{devels[i]}')))\n";
                        }
                        else
                        {
                            command += $"INSERT Games_by_developers VALUES ({_game.ID},((SELECT ID FROM Developers WHERE Developer_name ='{devels[i]}')))\n";
                        }
                    }

                    try
                    {
                        _workWithConnection.NewCommand(command, false);
                    }
                    catch (Exception)
                    {
                        MessageBox.Show("Изменения не были сохранены из-за некорректных данных.\nПроверьте введённые данные и попробуйте ещё раз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                        _workWithConnection.Disconnect();
                        return;
                    }

                    _workWithConnection.GetCommand.Parameters.Add("@Image", SqlDbType.Image);
                    _workWithConnection.GetCommand.Parameters.Add("@Name", SqlDbType.NVarChar, 1000000);

                    if(GameCover.Source != null)
                    {
                        using (FileStream fs = new FileStream(new Uri(GameCover.Source.ToString()).LocalPath, FileMode.OpenOrCreate))
                        {
                            imageData = new byte[fs.Length];
                            fs.Read(imageData, 0, imageData.Length);

                            if (_isNewGame)
                            {
                                command = $"INSERT Game_images VALUES (1,IDENT_CURRENT('Products'), @Image, @Name)";
                            }
                            else
                            {
                                command = $"INSERT Game_images VALUES (1,{_game.ID}, @Image, @Name)";
                            }

                            _workWithConnection.GetCommand.Parameters["@Image"].Value = imageData;
                            _workWithConnection.GetCommand.Parameters["@Name"].Value = fs.Name.Substring(fs.Name.LastIndexOf("\\") + 1);
                        }

                        try
                        {
                            _workWithConnection.NewCommand(command, false);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show("Не удалось добавить изображения.\nПроверьте путь к изображениям и попробуйте ещё раз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                            _workWithConnection.GetCommand.Parameters.Clear();
                            _workWithConnection.Disconnect();
                            return;
                        }
                    }
                    if (GameImages.Children.Count > 1)
                    {
                        command = string.Empty;

                        foreach (var gameImage in GameImages.Children.OfType<Image>())
                        {
                            using (FileStream fs = new FileStream(new Uri(gameImage.Source.ToString()).LocalPath, FileMode.OpenOrCreate))
                            {
                                imageData = new byte[fs.Length];
                                fs.Read(imageData, 0, imageData.Length);

                                if (_isNewGame)
                                    command = $"INSERT Game_images VALUES (2,IDENT_CURRENT('Products'), @Image, @Name)\n";
                                else
                                    command = $"INSERT Game_images VALUES (2,{_game.ID}, @Image, @Name)\n";

                                _workWithConnection.GetCommand.Parameters["@Image"].Value = imageData;
                                _workWithConnection.GetCommand.Parameters["@Name"].Value = fs.Name.Substring(fs.Name.LastIndexOf("\\") + 1);
                            }

                            try
                            {
                                _workWithConnection.NewCommand(command, false);
                            }
                            catch (Exception)
                            {
                                MessageBox.Show("Изменения не были сохранены из-за некорректных данных.\nПроверьте введённые данные и попробуйте ещё раз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
                                _workWithConnection.GetCommand.Parameters.Clear();
                                _workWithConnection.Disconnect();
                                return;
                            }
                        }
                    }

                    MessageBox.Show("Изменения были успешно сохранены", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                    _workWithConnection.GetCommand.Parameters.Clear();
                    _workWithConnection.Disconnect();
                   
                }
                else
                    MessageBox.Show("Изменения не были сохранены из-за ошибки подключения.\nПопробуйте ещё раз позже", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
            else
                MessageBox.Show("Изменения не были сохранены из-за некорректных данных.\nПроверьте введённые данные и попробуйте ещё раз", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Warning);
        }

        private void BackToGameList(object sender, RoutedEventArgs e)
        {
            this.DialogResult = _isChanged;
        }

        private void GameCategories_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (CategoriesView.Text[CategoriesView.Text.Length - 1] != ',' && CategoriesView.Text != "Жанр:")
                CategoriesView.Text += ',';
            CategoriesView.Text += GameCategories.SelectedValue;
        }

        private void GameDeveloper_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (DevelopersView.Text[DevelopersView.Text.Length - 1] != ',' && DevelopersView.Text != "Разработчики:")
                DevelopersView.Text += ',';
            DevelopersView.Text += GameDeveloper.SelectedValue;
        }

        private void SaveChanges_Click(object sender, RoutedEventArgs e)
        {
            if (!string.IsNullOrEmpty(NameOfGameView.Text) && !string.IsNullOrEmpty(ProductCounter.Text) &&
                !string.IsNullOrEmpty(ProductPrice.Text) && !string.IsNullOrEmpty(DescriptionView.Text) &&
                !string.IsNullOrEmpty(SystemRequitementsView.Text) && !string.IsNullOrEmpty(CategoriesView.Text) &&
                !string.IsNullOrEmpty(DevelopersView.Text))
            {
                SaveNewData();
                _isChanged = true;
            }
            else
            {
                MessageBox.Show("Не заполнены одно или несколько полей. Проверьте поля и повторите попытку", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }
    }
}
