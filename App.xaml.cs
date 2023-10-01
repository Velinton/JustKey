using JustKey.Classes;
using System.IO;
using System.Windows;

namespace JustKey
{
    /// <summary>
    /// Логика взаимодействия для App.xaml
    /// </summary>
    public partial class App : Application
    {
        private void DeleteAllTmpFiles()
        {
            Directory.Delete($"{AppOptions.CurrentDisk}\\Диплом\\images", true);
        }

        private void Application_Exit(object sender, ExitEventArgs e)
        {
            DeleteAllTmpFiles();
        }

        private void Application_SessionEnding(object sender, SessionEndingCancelEventArgs e)
        {
            DeleteAllTmpFiles();
        }

        private void Application_DispatcherUnhandledException(object sender, System.Windows.Threading.DispatcherUnhandledExceptionEventArgs e)
        {
            MessageBox.Show("Произошла ошибка. Приложение будет закрыто", "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
            App.Current.Shutdown();
        }
    }
}
