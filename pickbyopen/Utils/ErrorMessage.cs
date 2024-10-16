using System.Windows;

namespace Pickbyopen.Utils
{
    public static class ErrorMessage
    {
        public static void Show(string message)
        {
            MessageBox.Show(message, "Erro", MessageBoxButton.OK, MessageBoxImage.Error);
        }
    }
}
