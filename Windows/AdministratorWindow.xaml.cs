using JustKey.Pages;
using System.Windows;
using System.Windows.Input;

namespace JustKey.Windows
{
    /// <summary>
    /// Логика взаимодействия для AdministratorWindow.xaml
    /// </summary>
    public partial class AdministratorWindow : Window
    {
        private readonly int _idEmployee;
        private readonly int _type;

        public AdministratorWindow(int IDEmployee, int type)
        {
            InitializeComponent();
            _idEmployee = IDEmployee;
            _type = type;
            MainFrame.Content = new KeysPage();
        }

        public AdministratorWindow()
        {
            InitializeComponent();
            MainFrame.Content = new KeysPage();
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
