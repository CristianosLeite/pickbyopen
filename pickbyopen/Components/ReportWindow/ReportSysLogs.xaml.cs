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
    /// Interação lógica para ReportSysLogs.xam
    /// </summary>
    public partial class ReportSysLogs : UserControl
    {
        private readonly Db db;
        private readonly ExportReport<SysLog> exportReport;
        public ObservableCollection<SysLog> SysLogs { get; set; }
        private readonly List<string> Header = ["Data e hora", "Evento", "Alvo", "Dispositivo"];

        private readonly List<string> DataHeader = ["CreatedAt", "Event", "Target", "Device"];

        public ReportSysLogs()
        {
            InitializeComponent();

            DgSysLogs.AutoGenerateColumns = false;
            DgSysLogs.CanUserAddRows = false;
            DgSysLogs.CanUserDeleteRows = false;
            DgSysLogs.CanUserReorderColumns = false;
            DgSysLogs.CanUserResizeColumns = false;
            DgSysLogs.CanUserResizeRows = false;
            DgSysLogs.CanUserSortColumns = false;

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            foreach (var header in Header)
            {
                DgSysLogs.Columns.Add(
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

            SysLogs = [];
            DataContext = this;
            GetSysLogsByDate();

            exportReport = new(Header, DataHeader, SysLogs);
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InitialDate.Text) || string.IsNullOrEmpty(FinalDate.Text))
                return;

            GetSysLogsByDate();
        }

        private async void GetSysLogsByDate()
        {
            var sysLogs = await db.GetSysLogsByDate(
                InitialDate.SelectedDate.ToString()!,
                FinalDate.SelectedDate.ToString()!
            );

            SysLogs.Clear();
            foreach (var log in sysLogs)
            {
                SysLogs.Add(log);
            }

            DataContext = this;
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            exportReport.ExportExcel();
        }
    }
}
