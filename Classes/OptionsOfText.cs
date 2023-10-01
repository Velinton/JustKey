using System.Windows.Input;
using System;
using System.Text.RegularExpressions;

namespace JustKey.Classes
{
    internal class OptionsOfText
    {
        /// <summary>
        /// Ввод только цифр (Для previewTextChanged)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnlyNumber(object sender, TextCompositionEventArgs e)
        {
            if (!Char.IsNumber(e.Text[0]))
                e.Handled = true;
        }
        /// <summary>
        /// Ввод только русских символов (Для previewTextChanged)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void OnlyRussianKeybord(object sender, TextCompositionEventArgs e)
        {
            Regex GetReguliarReplase = new Regex("([а-яА-Я]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (GetReguliarReplase.Match(e.Text).Value != e.Text && e.Text[0] != 'ё')
            {
                e.Handled = true;
            }
        }

        /// <summary>
        /// Ввод чего угодно, кроме русских символов (Для previewTextChanged)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DisableRussianKeybord(object sender, TextCompositionEventArgs e)
        {
            Regex GetReguliarReplase = new Regex("([а-яА-Я]*)", RegexOptions.IgnoreCase | RegexOptions.Singleline);

            if (GetReguliarReplase.Match(e.Text).Value == e.Text || e.Text[0] == 'ё')
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// Заблокировать пробел (Для previewKeyDown)
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void LockSpace(object sender, KeyEventArgs e)
        {
            if (e.Key == Key.Space)
            {
                e.Handled = true;
            }
        }
        /// <summary>
        /// Заблокировать ввод специальных команд на странице
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public static void DisableCommands(object sender, ExecutedRoutedEventArgs e)
        {
            if (e.Command == ApplicationCommands.Cut ||
                e.Command == ApplicationCommands.Copy ||
                e.Command == ApplicationCommands.Paste)
            {
                e.Handled = true;
            }
        }
    }
}
