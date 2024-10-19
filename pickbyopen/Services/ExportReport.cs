using Microsoft.Office.Interop.Excel;
using Microsoft.Win32;
using System.Collections.ObjectModel;
using System.Windows;
using Application = Microsoft.Office.Interop.Excel.Application;

namespace Pickbyopen.Services
{
    internal class ExportReport<T>(
        List<string> header,
        List<string> dataHeader,
        ObservableCollection<T> data
    )
    {
        private List<string> Header { get; set; } = header;
        private List<string> DataHeader { get; set; } = dataHeader;
        private ObservableCollection<T> Data { get; set; } = data;

        public void ExportExcel()
        {
            try
            {
                string fullPath = GetSaveFilePath();
                if (string.IsNullOrEmpty(fullPath))
                    return;

                if (Data.Count > 0)
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

        private static string GetSaveFilePath()
        {
            string path = "C:\\Relatórios";
            string now = DateTime
                .Now.ToString("")
                .Replace("/", "")
                .Replace(":", "")
                .Replace(" ", "");
            string fullPath = $"{path}\\{now}.xlsx";

            SaveFileDialog saveFileDialog =
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

            return saveFileDialog.ShowDialog() == true ? saveFileDialog.FileName : string.Empty;
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
            for (var i = 0; i < Data.Count; i++)
            {
                for (var j = 0; j < DataHeader.Count; j++)
                {
                    var value = ExportReport<T>.GetNestedPropertyValue(Data[i]!, DataHeader[j]);
                    xlWorkSheet.Cells[i + 2, j + 1] = value?.ToString() ?? string.Empty;
                }

                xlWorkSheet.Cells[i + 2, 3].NumberFormat = "0";
            }
        }

        private static object? GetNestedPropertyValue(object obj, string propertyPath)
        {
            var properties = propertyPath.Split('.');
            foreach (var property in properties)
            {
                if (obj == null)
                    return null;
                var propInfo = obj.GetType().GetProperty(property);
                if (propInfo == null)
                    return null;
                obj = propInfo.GetValue(obj)!;
            }
            return obj;
        }
    }
}
