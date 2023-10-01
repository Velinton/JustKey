using iTextSharp.text.pdf;
using JustKey.Classes;
using System;
using System.Collections.Generic;
using System.Data.SqlClient;
using System.IO;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using iTextSharp.text;
using JustKey.Windows;
using System.Linq;
using System.Windows.Data;
using Microsoft.Win32;
using OfficeOpenXml;
using OfficeOpenXml.Table;
using OfficeOpenXml.Style;
using System.Globalization;

namespace JustKey.Pages
{
    public partial class RaportsPage : Page
    {
        
        private const string SEARCH_PLACEHOLDER = "Поиск...";
        private const string ERROR_PHOTO_PATH = "\\Диплом\\Photos\\errorImage.png";
        private int _oldIndex = -1;
        private int _currentTypeData = -1;
        private int _startEmployeesIndex = 0;
        private int _endEmployeesIndex = 21;
        private int _startIndex = 0;
        private int _endIndex = 15;
        private bool _disableEmployees;
        private WorkWithConnection _workWithConnection;
        private readonly List<Sale> _sales = new List<Sale>();
        private readonly List<GameProvider> _providers = new List<GameProvider>();
        private readonly List<PurchaseStatus> _statuses = new List<PurchaseStatus>();
        private readonly List<Position> _positionsName = new List<Position>();
        private readonly List<Purchase> _purchases = new List<Purchase>();
        private readonly List<Employee> _employees = new List<Employee>();

        public RaportsPage()
        {
            InitializeComponent();
            Load(true, -1);
        }

        private void Load(bool loadDataFromDB, int typeOfData)
        {
            if (loadDataFromDB)
                LoadDataFromDB();
            if(typeOfData != -1)
            {
                if (typeOfData == 0)
                {
                    LoadEmployeeViews();
                }
                else if (typeOfData == 1)
                {
                    LoadPurchasesTable();
                }
                else
                {
                    LoadSalesTable();
                }
                DisplaySettings();
            }
        }

        private void LoadDataFromDB()
        {
            List<Product> products = new List<Product>();
            List<EmployeePhoto> photos = new List<EmployeePhoto>();

            _employees.Clear();
            _purchases.Clear();
            _sales.Clear();
            _statuses.Clear();
            _positionsName.Clear();
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
                    _positionsName.Add(new Position
                    {
                        ID = dataReader.GetInt32(0),
                        Name = dataReader.GetString(1)
                    });
                }
                dataReader.Close();

                commnad = "SELECT * FROM Purchases";
                _workWithConnection.NewCommand(commnad, returnValue: false);
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

                commnad = "SELECT * FROM Purchase_statuses";
                _workWithConnection.NewCommand(commnad, returnValue: false);
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

                commnad = "SELECT * FROM Providers";
                _workWithConnection.NewCommand(commnad, returnValue: false);
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
                    });
                }
                dataReader.Close();

                commnad = "SELECT ID, Name, Count, Price FROM Products";
                _workWithConnection.NewCommand(commnad, returnValue: false);
                dataReader = _workWithConnection.GetCommand.ExecuteReader();

                while (dataReader.Read())
                {
                    products.Add(new Product
                    {
                        ID = dataReader.GetInt32(0),
                        GameName = dataReader.GetString(1),
                        Count = dataReader.GetInt32(2),
                        Price = dataReader.GetInt32(3)
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
                    _employees[i].Position = _positionsName.Find(x => x.ID.ToString() == _employees[i].Position).Name;
                    _employees[i].Photo = photos[i];
                }

                for (int i = 0; i < _purchases.Count; i++)
                {
                    int employeeID = int.Parse(_purchases[i].EmployeeFullName);
                    Employee employee = _employees.Find(x => x.ID == employeeID);

                    _purchases[i].EmployeeFullName = $"{employee.LastName} {employee.Name} {employee.MiddleName}";
                    _purchases[i].ProviderName = _providers.Find(x => x.ID.ToString() == _purchases[i].ProviderName).Name;
                    _purchases[i].GameName = products.Find(x => x.ID.ToString() == _purchases[i].GameName).GameName;
                    _purchases[i].Status = _statuses.Find(x => x.ID.ToString() == _purchases[i].Status).StatusName;
                }

                for(int i = 0; i < products.Count; i++)
                {
                    Purchase purchase = _purchases.Find(x => x.GameName == products[i].GameName);

                    if (purchase == null)
                        continue;

                    _sales.Add(new Sale()
                    {
                        GameName = products[i].GameName,
                        ProviderName = purchase.ProviderName,
                        Price = products[i].Price
                    });

                    foreach (var item in _purchases.FindAll(x => x.GameName == purchase.GameName))
                    {
                        _sales.Find(x => x.GameName == purchase.GameName).KeysInPurchase += item.KeysCount;
                        _sales.Find(x => x.GameName == purchase.GameName).SalesFullSum += item.FullPrice;
                    }

                    _sales.Find(x => x.GameName == purchase.GameName).Saled = _sales.Find(x => x.GameName == purchase.GameName).KeysInPurchase - products[i].Count;
                    _sales.Find(x => x.GameName == purchase.GameName).PriceDifference = _sales.Find(x => x.GameName == purchase.GameName).Saled * products[i].Price;
                }
            }
            else
                MessageBox.Show("Ошибка подключения", "Ошибка");
        }

        private void CreateExcel(List<object> dataForRaport)
        {
            int row = 5;
            string path = string.Empty;
            string tableHeader = "Список закупок ";
            int tableCountRow;

            if (dataForRaport[0].GetType() == typeof(Purchase))
            {
                tableHeader += $"с {dataForRaport.Min(x => DateTime.Parse(x.GetType().GetProperty("Date").GetValue(x).ToString()).ToShortDateString())} " +
                               $"по {dataForRaport.Max(x => DateTime.Parse(x.GetType().GetProperty("Date").GetValue(x).ToString()).ToShortDateString())}";
            }
            else if(dataForRaport[0].GetType() == typeof(Sale))
            {
                tableHeader = $"Список продаж на {DateTime.Today.ToShortDateString()}";
            }
            tableCountRow = dataForRaport.Count + 4;

            if (dataForRaport.Count > 0)
            {
                using (var package = new ExcelPackage())
                {
                    ExcelWorksheet worksheet = package.Workbook.Worksheets.Add("my");
                    using (ExcelRange ran = worksheet.Cells[$"B4:H{tableCountRow}"])
                    {
                        ExcelTable table = worksheet.Tables.Add(ran, "table");
                        table.Columns[0].Name = $"{TableWithData.Columns[0].Header}";
                        table.Columns[1].Name = $"{TableWithData.Columns[1].Header}";
                        table.Columns[2].Name = $"{TableWithData.Columns[2].Header}";
                        table.Columns[3].Name = $"{TableWithData.Columns[3].Header}";
                        table.Columns[4].Name = $"{TableWithData.Columns[4].Header}";
                        table.Columns[5].Name = $"{TableWithData.Columns[5].Header}";
                        table.Columns[6].Name = $"{TableWithData.Columns[6].Header}";
                    }

                    if (dataForRaport[0].GetType() == typeof(Purchase))
                    {
                        foreach (var item in dataForRaport.OfType<Purchase>())
                        {
                            using (ExcelRange Rng = worksheet.Cells[$"B{row}"])
                                Rng.Value = item.EmployeeFullName;
                            using (ExcelRange Rng = worksheet.Cells[$"C{row}"])
                                Rng.Value = item.ProviderName;
                            using (ExcelRange Rng = worksheet.Cells[$"D{row}"])
                                Rng.Value = item.GameName;
                            using (ExcelRange Rng = worksheet.Cells[$"E{row}"])
                                Rng.Value = item.KeysCount;
                            using (ExcelRange Rng = worksheet.Cells[$"F{row}"])
                                Rng.Value = item.FullPrice;
                            using (ExcelRange Rng = worksheet.Cells[$"G{row}"])
                                Rng.Value = item.Date;
                            using (ExcelRange Rng = worksheet.Cells[$"H{row}"])
                                Rng.Value = item.Status;
                            row++;
                        }
                        worksheet.Cells[$"G5:G{tableCountRow}"].Style.Numberformat.Format = DateTimeFormatInfo.CurrentInfo.ShortDatePattern;
                    }
                    else if (dataForRaport[0].GetType() == typeof(Sale))
                    {
                        foreach (var item in dataForRaport.OfType<Sale>())
                        {
                            using (ExcelRange Rng = worksheet.Cells[$"B{row}"])
                                Rng.Value = item.GameName;
                            using (ExcelRange Rng = worksheet.Cells[$"C{row}"])
                                Rng.Value = item.ProviderName;
                            using (ExcelRange Rng = worksheet.Cells[$"D{row}"])
                                Rng.Value = item.Price;
                            using (ExcelRange Rng = worksheet.Cells[$"E{row}"])
                                Rng.Value = item.KeysInPurchase;
                            using (ExcelRange Rng = worksheet.Cells[$"F{row}"])
                                Rng.Value = item.Saled;
                            using (ExcelRange Rng = worksheet.Cells[$"G{row}"])
                                Rng.Value = item.SalesFullSum;
                            using (ExcelRange Rng = worksheet.Cells[$"H{row}"])
                                Rng.Value = item.PriceDifference;
                            row++;
                        }
                    }

                    worksheet.Cells.Style.Font.Name = "Times New Roman";
                    worksheet.Cells.Style.Font.Size = 14;
                    worksheet.Cells[$"B3:H{tableCountRow}"].Style.Border.Top.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[$"B3:H{tableCountRow}"].Style.Border.Bottom.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[$"B3:H{tableCountRow}"].Style.Border.Right.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells[$"B3:H{tableCountRow}"].Style.Border.Left.Style = ExcelBorderStyle.Thin;
                    worksheet.Cells.Style.HorizontalAlignment = ExcelHorizontalAlignment.Center;
                    worksheet.Cells.AutoFitColumns();
                    worksheet.Cells[$"B3:H3"].Merge = true;
                    worksheet.Cells[$"B3:H3"].Style.Fill.PatternType = ExcelFillStyle.Solid;
                    worksheet.Cells[$"B3:H3"].Style.Fill.BackgroundColor.SetColor(System.Drawing.Color.FromArgb(1,240,248,255));
                    worksheet.Cells[$"B3:H3"].Value = tableHeader;

                    SaveFileDialog fileDialog = new SaveFileDialog
                    {
                        Filter = "xlsx файлы (*.xlsx)|*.xlsx",
                        FilterIndex = 0,
                        RestoreDirectory = true,
                        Title = "Сохранить отчёт по сотруднику"
                    };

                    if (fileDialog.ShowDialog() == true)
                    {
                        path = fileDialog.FileName;
                        byte[] excelData = package.GetAsByteArray();

                        try
                        {
                            File.WriteAllBytes(path, excelData);
                            MessageBox.Show($"Отчёт успешно сформирован и расположен по пути: <{path}>", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                        }
                        catch (Exception)
                        {
                            MessageBox.Show($"Произошла ошибка.\n", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
                        }
                    }
                }
            }
            else
            {
                MessageBox.Show($"В таблице отсутствуют данные.", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Information);
            }
        }

        

        private void CreateRaportByFullTable_Click(object sender, RoutedEventArgs e)
        {
            List<object> tmpList = new List<object>();

            if(_currentTypeData == 1)
                tmpList.AddRange(_purchases);
            else if(_currentTypeData == 2)
                tmpList.AddRange(_sales);

            if (_disableEmployees == true || (_disableEmployees == false && RaportBy.SelectedIndex > 0))
            {
                CreateExcel(tmpList);
            }
        }

        private void LoadSalesTable()
        {
            CreateRaport.IsEnabled = true;
            CreateRaportByFullTable.IsEnabled = true;
            _currentTypeData = 2;
            if (_purchases.Count > 0)
            {
                TableWithData.ItemsSource = null;

                TableWithData.Columns.Clear();

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Наименование игры",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("GameName")
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
                    Header = "Текущий ценник",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Price")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Закуплено копий",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("KeysInPurchase")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Продано копий",
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("Saled")
                    }
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Закуплено на сумму",
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("SalesFullSum")
                    }
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Разница",
                    Width = new DataGridLength(2, DataGridLengthUnitType.Star),
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("PriceDifference")
                    }
                });

                FilterRequest.Items.Clear();
                FilterRequest.Text = "Поставщик";
                foreach (var item in _providers)
                {
                    FilterRequest.Items.Add(item.Name);
                }
                LoadViews();
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void LoadPurchasesTable()
        {
            CreateRaport.IsEnabled = true;
            CreateRaportByFullTable.IsEnabled = true;
            _currentTypeData = 1;
            if (_purchases.Count > 0)
            {
                TableWithData.ItemsSource = null;

                TableWithData.Columns.Clear();

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "ФИО сотрудника",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("EmployeeFullName")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Auto)
                });

                TableWithData.Columns.Add(new DataGridTextColumn()
                {
                    Header = "Поставщик",
                    Binding = new Binding()
                    {
                        Path = new PropertyPath("ProviderName")
                    },
                    Width = new DataGridLength(2, DataGridLengthUnitType.Auto)
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

                FilterRequest.Items.Clear();
                FilterRequest.Text = "Статус закупки";
                foreach (var item in _statuses)
                {
                    FilterRequest.Items.Add(item.StatusName);
                }

                LoadViews();
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void LoadEmployeeViews()
        {
            CreateRaport.IsEnabled = false;
            CreateRaportByFullTable.IsEnabled = false;
            _currentTypeData = 0;
            employeesViews.Children.Clear();
            if (_employees.Count > 0)
            {
                for (int i = 0; i < _employees.Count; i++)
                {
                    if (i < _startEmployeesIndex || i >= _endEmployeesIndex)
                        continue;
                    else if (i > _endEmployeesIndex)
                        break;

                    string path = string.Empty;

                    StackPanel employeeView = new StackPanel
                    {
                        Background = Brushes.AliceBlue,
                        Margin = new Thickness(25, 15, 0, 0),
                        Cursor = Cursors.Hand
                    };

                    if (_employees[i].Photo.Photo.Length == 0)
                        path = $"{AppOptions.CurrentDisk}{ERROR_PHOTO_PATH}";
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
                    image.UriSource = new Uri($"{path}");
                    image.EndInit();

                    System.Windows.Controls.Image employeePhoto = new System.Windows.Controls.Image
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
                        Margin = new Thickness(0, 5, 0, 0)
                    };

                    if (employeeNames.Text.Length >= 17)
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

                    FilterRequest.Items.Clear();
                    FilterRequest.Text = "Должность";
                    foreach (var item in _positionsName)
                    {
                        FilterRequest.Items.Add(item.Name);
                    }
                }
            }
            else
            {
                MessageBox.Show("Нечего загружать", "Ошибка");
            }
        }

        private void CreatePdf(Employee employee)
        {
            SaveFileDialog ofd = new SaveFileDialog
            {
                Filter = "Pdf File |*.pdf",
                Title = "Сохранить отчёт по сотруднику",
                FileName = $"{employee.LastName} {employee.Name} {employee.MiddleName}",
                RestoreDirectory = true
            };
            if(ofd.ShowDialog() == true)
            {
                var document = new Document(PageSize.A4, 20, 20, 30, 20);
                string ttf = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.Fonts), "TIMES.TTF");
                var baseFont = BaseFont.CreateFont(ttf, BaseFont.IDENTITY_H, BaseFont.NOT_EMBEDDED);
                var font = new Font(baseFont, 16, Font.NORMAL);

                using (var writer = PdfWriter.GetInstance(document, new FileStream(ofd.FileName, FileMode.Create)))
                {
                    iTextSharp.text.Image employeePhoto;
                    var image = new BitmapImage();

                    image.BeginInit();
                    image.CacheOption = BitmapCacheOption.OnLoad;

                    using (FileStream fs = new FileStream($"{AppOptions.CurrentDisk}\\Диплом\\images\\{employee.Photo.Name}", FileMode.OpenOrCreate))
                    {
                        fs.Write(employee.Photo.Photo,0,employee.Photo.Photo.Length);
                        image.UriSource = new Uri(fs.Name);
                    }

                    image.EndInit();

                    document.Open();

                    using (FileStream stream = new FileStream($"{image.UriSource.LocalPath}", FileMode.Open))
                    {
                        employeePhoto = iTextSharp.text.Image.GetInstance(stream);
                    }

                    employeePhoto.Border = Rectangle.BOX;
                    employeePhoto.BorderWidth = 1;
                    employeePhoto.Alignment = iTextSharp.text.Image.ALIGN_LEFT | iTextSharp.text.Image.PARAGRAPH;
                    employeePhoto.BorderColor = BaseColor.BLACK;
                    employeePhoto.SpacingAfter = 0;
                    employeePhoto.ScalePercent(50); //ширина и высота

                    document.Add(employeePhoto);

                    Paragraph paragraph = new Paragraph($"   ФИО: {employee.LastName} {employee.Name} {employee.MiddleName}\n   Дата рождения: {employee.DateOfBirth}\n   " +
                                                        $"Возраст(полных лет): {employee.Ages}\n   Стаж(полных лет): {employee.Experience / 12}\n   Должность: {employee.Position}\n   " +
                                                        $"Зарплата сотрудника: {employee.Salary}\n   Работает с: {employee.DateOfEmployment}\n   " +
                                                        $"Номер счёта: {employee.AccountNumber}\n   Действующая почта: {employee.Email}", font)
                    {
                        Alignment = Element.ALIGN_JUSTIFIED,

                    };

                    paragraph.SetLeading(0f, 2f);
                    document.Add(paragraph);

                    document.Close();
                    writer.Close();

                    MessageBox.Show($"Отчёт успешно сформирован и расположен по пути: <{ofd.FileName}>", "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
                }
            }
        }

        private void EmployeeView_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            CreatePdf(_employees.Find(x => x.ID == int.Parse(((sender as StackPanel).Children[0] as Label).Content.ToString())));
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

        private void OpenEmployeesPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new EmployeesPage());
        }

        private void OpenGamesPage_Click(object sender, RoutedEventArgs e)
        {
            NavigationService.Navigate(new GamesPage());
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

        private void MoveToRight_Click(object sender, RoutedEventArgs e)
        {
            MoveToLeft.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToStartList.Style = (Style)FindResource("RoundedDefaultButton");

            _startEmployeesIndex += 21;
            _endEmployeesIndex += 21;

            LoadEmployeeViews();

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

            _startEmployeesIndex -= 21;
            _endEmployeesIndex -= 21;
            LoadEmployeeViews();

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

            _startEmployeesIndex = 0;
            _endEmployeesIndex = 21;
            LoadEmployeeViews();

            MoveToLeft.Style = (Style)FindResource("BlockButton");
            MoveToStartList.Style = (Style)FindResource("BlockButton");
        }

        private void MoveToEndList_Click(object sender, RoutedEventArgs e)
        {
            MoveToLeft.Style = (Style)FindResource("RoundedDefaultButton");
            MoveToStartList.Style = (Style)FindResource("RoundedDefaultButton");

            if (_employees.Count % 21 != 0)
                _startEmployeesIndex = _employees.Count - (_employees.Count % 21);
            else
                _startEmployeesIndex = _employees.Count - 21;

            _endEmployeesIndex = _startEmployeesIndex + 21;

            LoadEmployeeViews();

            MoveToRight.Style = (Style)FindResource("BlockButton");
            MoveToEndList.Style = (Style)FindResource("BlockButton");
        }

        private void Filter(int typeData)
        {
            List<object> editedData = new List<object>();

            if(typeData == 0) //employees
            {
                foreach (var item in _employees)
                {
                    if (item.Position == FilterRequest.SelectedValue.ToString())
                        editedData.Add(item);
                }
                _employees.Clear();
                _employees.AddRange(editedData.OfType<Employee>());

            }
            else if (typeData == 1) //purchases
            {
                foreach (var item in _purchases)
                {
                    if (item.Status == FilterRequest.SelectedValue.ToString())
                        editedData.Add(item);
                }
                _purchases.Clear();
                _purchases.AddRange(editedData.OfType<Purchase>());
            }
            else if (typeData == 2) //sales
            {
                foreach (var item in _sales)
                {
                    if(item.ProviderName == FilterRequest.SelectedValue.ToString())
                        editedData.Add(item);
                }
                _sales.Clear();
                _sales.AddRange(editedData.OfType<Sale>());
            }
            Load(false, typeData);
        }

        private void Search(int typeData)
        {
            List<object> editedData = new List<object>();

            if (typeData == 0) //employees
            {
                foreach (var employee in _employees)
                    if (Regex.IsMatch($"{employee.Name.ToLower()} {employee.LastName.ToLower()} {employee.MiddleName.ToLower()}", $"{SearchRequest.Text.ToLower()}.*"))
                        editedData.Add(employee);
                _employees.Clear();
                _employees.AddRange(editedData.OfType<Employee>());

            }
            else if (typeData == 1) //purchases
            {
                foreach (var purchase in _purchases)
                    if (Regex.IsMatch($"{purchase.ProviderName.ToLower()} {purchase.GameName.ToLower()} {purchase.EmployeeFullName.ToLower()}", $"{SearchRequest.Text.ToLower()}.*"))
                        editedData.Add(purchase);
                _purchases.Clear();
                _purchases.AddRange(editedData.OfType<Purchase>());
            }
            else if (typeData == 2) //sales
            {
                foreach (var sale in _sales)
                    if (Regex.IsMatch($"{sale.GameName.ToLower()}", $"{SearchRequest.Text.ToLower()}.*"))
                        editedData.Add(sale);
                _sales.Clear();
                _sales.AddRange(editedData.OfType<Sale>());
            }

            Load(false, typeData);
        }

        private void FilterRequest_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (FilterRequest.SelectedIndex != -1)
            {
                Filter(_currentTypeData);
            }
        }

        private void SearchStart_Click(object sender, RoutedEventArgs e)
        {
            Search(_currentTypeData);
        }

        private void SearchRequest_KeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                Search(_currentTypeData);
            }
        }

        private void DisplaySettings()
        {
            if(RaportBy.SelectedIndex == 0)
            {
                MoveToStartList.Visibility = Visibility.Visible;
                MoveToLeft.Visibility = Visibility.Visible;
                MoveToRight.Visibility = Visibility.Visible;
                MoveToEndList.Visibility = Visibility.Visible;
                BorderForPagesButtons.Visibility = Visibility.Collapsed;

                if (_employees.Count > 21)
                {
                    MoveToRight.Style = (Style)FindResource("RoundedDefaultButton");
                    MoveToEndList.Style = (Style)FindResource("RoundedDefaultButton");
                }
            }
            else
            {
                MoveToStartList.Visibility = Visibility.Collapsed;
                MoveToLeft.Visibility = Visibility.Collapsed;
                MoveToRight.Visibility = Visibility.Collapsed;
                MoveToEndList.Visibility = Visibility.Collapsed;

                BorderForPagesButtons.Visibility = Visibility.Visible;
            }
        }

        private void LoadViews()
        {
            int selectedIndex = RaportBy.SelectedIndex;

            if(_disableEmployees == true)
            {
                selectedIndex = RaportBy.SelectedIndex + 1;
            }

            TableWithData.ItemsSource = null;
            List<object> tempList = new List<object>();

            if (selectedIndex == 1 && _purchases.Count > 0)
            {
                for (int i = 0; i < _purchases.Count; i++)
                {
                    if (i < _startIndex || i >= _endIndex)
                        continue;
                    else if (i > _endIndex)
                        break;

                    tempList.Add(_purchases[i]);
                }
                LastPageNumber.Text = (_purchases.Count / 15 + 1).ToString();
            }
            else if (selectedIndex == 2 && _sales.Count > 0)
            {
                for (int i = 0; i < _sales.Count; i++)
                {
                    if (i < _startIndex || i >= _endIndex)
                        continue;
                    else if (i > _endIndex)
                        break;

                    tempList.Add(_sales[i]);
                }
                LastPageNumber.Text = (_sales.Count / 15 + 1).ToString();
            }

            TableWithData.ItemsSource = tempList;
        }

        private void CurrentPageNumber_PreviewKeyDown(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Enter)
            {
                LoadInputedPage();
            }
            else if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }

        private void PreviousPage_Click(object sender, RoutedEventArgs e)
        {
            int current = int.Parse(CurrentPageNumber.Text.ToString());
            int maxPage = int.Parse(LastPageNumber.Text.ToString());

            if (current > 1 && current <= maxPage)
            {
                current--;
                CurrentPageNumber.Text = current.ToString();
                _startIndex -= 15;
                _endIndex -= 15;

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
                _startIndex += 15;
                _endIndex += 15;
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

                _endIndex = 15 * maxPage;
                _startIndex = _endIndex - 15;
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
                _endIndex = 15;
                LoadViews();
            }
            else
                MessageBox.Show("Открыта первая страница");
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

        private void LoadInputedPage()
        {
            if (string.IsNullOrEmpty(CurrentPageNumber.Text))
            {
                CurrentPageNumber.Text = "1";
                _startIndex = 0;
                _endIndex = 15;
                MessageBox.Show("Будет загружена первая страница т.к. номер страницы не введён");
                LoadViews();
                return;
            }

            int current = int.Parse(CurrentPageNumber.Text.ToString());
            int maxPage = int.Parse(LastPageNumber.Text.ToString());

            if (current <= maxPage && current >= 1)
            {
                _endIndex = current * 15;
                _startIndex = _endIndex - 15;
            }
            else
                MessageBox.Show("Введён номер не существующей страницы");

            LoadViews();
        }

        private void CurrentPageNumber_LostFocus(object sender, RoutedEventArgs e)
        {
            LoadInputedPage();
        }

        private void ReloadPage_Click(object sender, RoutedEventArgs e)
        {
            FilterRequest.SelectedIndex = -1;
            RaportBy.SelectedIndex = -1;
            FilterRequest.Items.Clear();
            CreateRaport.IsEnabled = false;
            CreateRaportByFullTable.IsEnabled = false;
            FilterRequest.Text = "Фильтровать по...";
            RaportBy.Text = "Отчёт по...";

            if (_oldIndex > 0)
            {
                TableWithData.Visibility = Visibility.Collapsed;
            }
            else
            {
                foreach (var item in employeesViews.Children)
                {
                    (item as UIElement).Visibility = Visibility.Collapsed;
                }
            }

            _oldIndex = -1;
            LoadDataFromDB();
        }

        private void RaportBy_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if(RaportBy.SelectedIndex != -1)
            {
                if (RaportBy.SelectedIndex == 0)
                {
                    if(_oldIndex != 0)
                    {
                        TableWithData.Visibility = Visibility.Collapsed;
                        foreach (var item in employeesViews.Children)
                        {
                            (item as UIElement).Visibility = Visibility.Visible;
                        }
                    }
                }
                else
                {
                    if (_oldIndex == 0)
                    {
                        foreach (var item in employeesViews.Children)
                        {
                            (item as UIElement).Visibility = Visibility.Collapsed;
                        }
                    }
                    TableWithData.Visibility = Visibility.Visible;
                }

                if(_disableEmployees == true)
                {
                    _oldIndex = RaportBy.SelectedIndex + 1;
                    Load(false, RaportBy.SelectedIndex + 1);
                    TableWithData.Visibility = Visibility.Visible;
                }
                else
                {
                    _oldIndex = RaportBy.SelectedIndex;
                    Load(false, RaportBy.SelectedIndex);
                }
                
            }
        }

        private void CreateRaport_Click(object sender, RoutedEventArgs e)
        {
            List<object> tmpList = new List<object>();
            tmpList.AddRange(TableWithData.ItemsSource.OfType<object>());

            if(_disableEmployees == true || (_disableEmployees == false && RaportBy.SelectedIndex > 0))
            {
                CreateExcel(tmpList);
            }   
        }

        private void Page_Loaded(object sender, RoutedEventArgs e)
        {
            if ((int)Window.GetWindow(this).GetType().GetProperty("GetTypeEmployee").GetValue(Window.GetWindow(this)) > 1)
            {
                NavigationButtons.Children.Remove(OpenEmployeesPage);
                OpenKeysPage.Margin = new Thickness(0, 150, 0, 0);
                RaportBy.Items.RemoveAt(0);
                _disableEmployees = true;
            }
        }
    }
}