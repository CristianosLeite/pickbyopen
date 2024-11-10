using Pickbyopen.Database;
using Pickbyopen.Interfaces;
using Pickbyopen.Windows;
using System.Windows;

namespace Pickbyopen.Services
{
    public class LogService(IDbConnectionFactory connectionFactory)
        : LogRepository(connectionFactory)
    {
        public void ShowLogs(object sender, RoutedEventArgs e)
        {
            LogsWindow logs = new();
            logs.Show();
        }
    }
}
