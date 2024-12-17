using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Services;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para ReportOperations.xam
    /// </summary>
    public partial class ReportOperations : UserControl
    {
        public readonly Db db;
        private readonly ExportReport<Operation> exportReport;
        public ObservableCollection<Operation> Operations { get; set; }
        private readonly List<string> Header =
        [
            "Data e hora",
            "Evento",
            "VP / Partnumber",
            "Van",
            "Porta",
            "Modo",
            "Usuário",
        ];

        private readonly List<string> DataHeader =
        [
            "CreatedAt",
            "Event",
            "Target",
            "Van",
            "Door",
            "Mode",
            "UserName",
        ];

        public ReportOperations()
        {
            InitializeComponent();

            DgOperations.AutoGenerateColumns = false;
            DgOperations.CanUserAddRows = false;
            DgOperations.CanUserDeleteRows = false;
            DgOperations.CanUserReorderColumns = false;
            DgOperations.CanUserResizeColumns = false;
            DgOperations.CanUserResizeRows = false;
            DgOperations.CanUserSortColumns = false;

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            foreach (var header in Header)
            {
                DgOperations.Columns.Add(
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

            Operations = [];
            DataContext = this;
            GetOperationsByDate();

            exportReport = new(Header, DataHeader, Operations);
        }

        private void Search(object sender, RoutedEventArgs e)
        {
            if (string.IsNullOrEmpty(InitialDate.Text) || string.IsNullOrEmpty(FinalDate.Text))
                return;

            if (TbPartnumber.Text.Length != 0 || TbDoor.Text.Length != 0)
            {
                BtnRemoveFilter.IsEnabled = true;
                BtnRemoveFilter.Foreground = Brushes.Red;
            }

            GetOperationsByDate();
        }

        private async void GetOperationsByDate()
        {
            var operations = await db.GetOperationsByDate(
                TbPartnumber.Text,
                string.Empty,
                TbDoor.Text,
                InitialDate.SelectedDate.ToString()!,
                FinalDate.SelectedDate.ToString()!
            );

            Operations.Clear();
            foreach (var operation in operations)
            {
                Operations.Add(operation);
            }

            DataContext = this;
        }

        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            TbPartnumber.Clear();
            TbDoor.Clear();
            GetOperationsByDate();
        }

        private void OnTbFilterChange(object sender, RoutedEventArgs e)
        {
            if (TbPartnumber.Text.Length > 0 || TbDoor.Text.Length > 0)
            {
                BtnFind.Foreground = Brushes.LawnGreen;
            }
            else
            {
                BtnFind.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.IsEnabled = false;
            }
        }

        private void Export(object sender, RoutedEventArgs e)
        {
            exportReport.ExportExcel();
        }
    }
}
