using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace ClinicApp.Helpers
{
    public static class UIService
    {
        public static void ShowError(TextBlock textBlock, string message)
        {
            textBlock.Text = message;
            textBlock.Foreground = new SolidColorBrush(Colors.Red);
        }

        public static void ShowSuccess(TextBlock textBlock, string message)
        {
            textBlock.Text = message;
            textBlock.Foreground = new SolidColorBrush(Colors.Green);
        }

        public static void ShowWarning(TextBlock textBlock, string message)
        {
            textBlock.Text = message;
            textBlock.Foreground = new SolidColorBrush(Colors.Orange);
        }

        public static void ShowInfo(TextBlock textBlock, string message)
        {
            textBlock.Text = message;
            textBlock.Foreground = new SolidColorBrush(Colors.Blue);
        }

        public static void ShowError(string message)
        {
            MessageBox.Show(message, "Ошибка", MessageBoxButton.OK, MessageBoxImage.Error);
        }

        public static void ShowSuccess(string message)
        {
            MessageBox.Show(message, "Успех", MessageBoxButton.OK, MessageBoxImage.Information);
        }
    }
}