using System.IO;

namespace JustKey.Classes
{
    /// <summary>
    /// Настройки программы
    /// </summary>
    internal class AppOptions
    {
        /// <summary>
        /// Буква диска, на котором находится программа
        /// </summary>
        public static string CurrentDisk { get; } = Directory.GetCurrentDirectory().Remove(3);
        /// <summary>
        /// Путь к "Error Image"
        /// </summary>
        public static string ErrorImage { get; } = "./../../Images/errorImage.png";
    }
}
