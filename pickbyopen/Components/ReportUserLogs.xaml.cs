using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para ReportUserLogs.xam
    /// </summary>
    public partial class ReportUserLogs : UserControl
    {
        private readonly Db db;
        private readonly ExportReport<UserLog> exportReport;
        public ObservableCollection<UserLog> UserLogs { get; set; }
        private readonly List<string> Header = ["Data e hora", "Evento", "Alvo", "Usuário"];

        private readonly List<string> DataHeader =
        [
            "CreatedAt",
            "Event",
            "Target",
            "User.Username",
        ];

        public ReportUserLogs()
        {
            InitializeComponent();

            DgUserLogs.AutoGenerateColumns = false;
            DgUserLogs.CanUserAddRows = false;
            DgUserLogs.CanUserDeleteRows = false;
            DgUserLogs.CanUserReorderColumns = false;
            DgUserLogs.CanUserResizeColumns = false;
            DgUserLogs.CanUserResizeRows = false;
            DgUserLogs.CanUserSortColumns = false;

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            foreach (var header in Header)
            {
                DgUserLogs.Columns.Add(
                    new DataGridTextColumn
                    {
                        Header = header,
                        Binding = new Binding(DataHeader[Header.IndexOf(header)]),
                        Width = new DataGridLength(1, DataGridLengthUnitType.Auto),
                    }
                );
            }

            InitialDate.SelectedDate = DateTime.Now;
            FinalDate.SelectedDate = DateTime.Now;

            UserLogs = [];
            DataContext = this;
            GetUserLogsByDate();

            exportReport = new(Header, DataHeader, UserLogs);
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InitialDate.Text) || string.IsNullOrEmpty(FinalDate.Text))
                return;

            GetUserLogsByDate();
        }

        private async void GetUserLogsByDate()
        {
            var userLogs = await db.GetUserLogsByDate(
                InitialDate.SelectedDate.ToString()!,
                FinalDate.SelectedDate.ToString()!
            );

            UserLogs.Clear();
            foreach (var log in userLogs)
            {
                UserLogs.Add(log);
            }

            DataContext = this;
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            exportReport.ExportExcel();
        }
    }
}
