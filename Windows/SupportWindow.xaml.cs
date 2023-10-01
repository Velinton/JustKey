using JustKey.Pages;
using System.Windows;
using System.Windows.Input;

namespace JustKey.Windows
{
    public partial class SupportWindow : Window
    {
        private readonly int _IDEmployee;

        public SupportWindow(int IDEmployee)
        {
            InitializeComponent();
            _IDEmployee = IDEmployee;
            MainFrame.Content = new TreatmentsPage();
        }

        private void Window_MouseDown(object sender, MouseButtonEventArgs e)
        {
            if (e.LeftButton == MouseButtonState.Pressed)
                DragMove();
        }
    }
}
