using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Media;
using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using Pickbyopen.Database;
using Pickbyopen.Models;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para ReportOperations.xam
    /// </summary>
    public partial class ReportOperations : UserControl
    {
        public readonly Db db;
        public ObservableCollection<Operation> Operations { get; set; }
        private readonly List<string> Header =
        [
            "Data e hora",
            "Evento",
            "Desenho",
            "Porta",
            "Modo",
            "Usuário",
        ];

        private readonly List<string> DataHeader =
        [
            "CreatedAt",
            "Event",
            "Target",
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
            ExportExcel();
        }

        private void ExportExcel()
        {
            try
            {
                string fullPath = GetSaveFilePath();
                if (string.IsNullOrEmpty(fullPath))
                    return;

                if (DgOperations.Items.Count > 0)
                {
                    ExportDataToExcel(fullPath);
                    MessageBox.Show("Concluído. Verifique em " + fullPath);
                }
                else
                {
                    MessageBox.Show("Não existem dados para serem exportados.", "Atenção");
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Ocorreu um erro ao exportar planilha. " + ex.ToString(),
                    "Atenção"
                );
            }
        }

        private string GetSaveFilePath()
        {
            string path = "C:\\Relatórios";
            string now = DateTime
                .Now.ToString("")
                .Replace("/", "")
                .Replace(":", "")
                .Replace(" ", "");
            string fullPath = $"{path}\\{now}.xlsx";

            SaveFileDialog saveFileDialog1 =
                new()
                {
                    InitialDirectory = path,
                    Title = "Salvar arquivos excel",
                    CheckFileExists = false,
                    CheckPathExists = true,
                    DefaultExt = "xlsx",
                    Filter = "Arquivos de excel (*.xlsx)|*.xlsx|Todos os arquivos (*.*)|*.*",
                    FilterIndex = 1,
                    RestoreDirectory = true,
                    FileName = fullPath,
                };

            return saveFileDialog1.ShowDialog() == true ? saveFileDialog1.FileName : string.Empty;
        }

        private void ExportDataToExcel(string fullPath)
        {
            Application xlApp = new();
            Workbook xlWorkBook = xlApp.Workbooks.Add(System.Reflection.Missing.Value);
            Worksheet xlWorkSheet = (Worksheet)xlWorkBook.Worksheets.get_Item(1);

            SetExcelHeader(xlWorkSheet);
            FillExcelData(xlWorkSheet);

            xlWorkBook.SaveAs(
                fullPath,
                XlFileFormat.xlWorkbookDefault,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                XlSaveAsAccessMode.xlExclusive,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value
            );
            xlWorkBook.Close(
                true,
                System.Reflection.Missing.Value,
                System.Reflection.Missing.Value
            );
            xlApp.Quit();
        }

        private void SetExcelHeader(Worksheet xlWorkSheet)
        {
            xlWorkSheet.Cells.Range["A1", "F1"].Font.Bold = true;
            for (var i = 0; i < Header.Count; i++)
            {
                xlWorkSheet.Cells[1, i + 1] = Header[i];
            }
        }

        private void FillExcelData(Worksheet xlWorkSheet)
        {
            for (var i = 0; i < Operations.Count; i++)
            {
                xlWorkSheet.Cells[i + 2, 1] = Operations[i].CreatedAt.ToString();
                xlWorkSheet.Cells[i + 2, 2] = Operations[i].Event.ToString();
                xlWorkSheet.Cells[i + 2, 3] = Operations[i].Target.ToString();
                xlWorkSheet.Cells[i + 2, 4] = Operations[i].Door.ToString();
                xlWorkSheet.Cells[i + 2, 5] = Operations[i].Mode.ToString();
                xlWorkSheet.Cells[i + 2, 6] = Operations[i].UserName!.ToString();

                // Formata a célula do Target como número com 0 casas decimais
                xlWorkSheet.Cells[i + 2, 3].NumberFormat = "0";
            }
        }
    }
}
