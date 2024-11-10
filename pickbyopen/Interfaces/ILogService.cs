using System.Windows;

namespace Pickbyopen.Interfaces
{
    public interface ILogService : ILogRepository
    {
        void ShowLogs(object sender, RoutedEventArgs e);
    }
}
