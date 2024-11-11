using Pickbyopen.Components;
using System.Windows;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para ReportWindow.xaml
    /// </summary>
    public partial class ReportWindow : Window
    {
        public ReportWindow()
        {
            InitializeComponent();
        }

        private void LoadChildren(object? sender, RoutedEventArgs? e)
        {
            switch (TcData.SelectedIndex)
            {
                case 0:
                    ReportOperations reportOperations = new();
                    Operations.Children.Clear();
                    Operations.Children.Add(reportOperations);
                    break;
                case 1:
                    ReportSysLogs reportSysLogs = new();
                    SysLogs.Children.Clear();
                    SysLogs.Children.Add(reportSysLogs);
                    break;
                case 2:
                    ReportUserLogs reportUserLogs = new();
                    UserLogs.Children.Clear();
                    UserLogs.Children.Add(reportUserLogs);
                    break;
                default:
                    break;
            }
        }
    }
}
