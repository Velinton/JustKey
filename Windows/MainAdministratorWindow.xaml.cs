using JustKey.Pages;
using System.Windows;
using System.Windows.Input;

namespace JustKey.Windows
{
    /// <summary>
    /// Логика взаимодействия для MainAdministratorWindow.xaml
    /// </summary>
    public partial class MainAdministratorWindow : Window
    {
        private readonly int _idEmployee;
        private readonly int _type;

        public MainAdministratorWindow(int IDEmployee, int type)
        {
            InitializeComponent();
            _idEmployee = IDEmployee;
            MainFrame.Content = new EmployeesPage();
            _type = type;
        }

        public MainAdministratorWindow()
        {
            InitializeComponent();
            _idEmployee = 1;
            MainFrame.Content = new EmployeesPage();
        }

        public int GetTypeEmployee
        {
            get { return _type; }
        }

        public int GetIdAutorizateEmployee
        {
            get { return _idEmployee; }
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
