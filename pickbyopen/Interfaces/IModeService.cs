using System.Windows;
using System.Windows.Input;

namespace Pickbyopen.Interfaces
{
    public interface IModeService
    {
        void SetMode();
        void SwitchMode(object sender, RoutedEventArgs e);
        void ModeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e);
        void ModeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e);
    }
}
