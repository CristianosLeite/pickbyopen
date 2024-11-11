using Pickbyopen.Settings;
using System.Collections.ObjectModel;
using System.IO.Ports;
using System.Windows;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para CodeBarsReaderConfig.xam
    /// </summary>
    public partial class CodeBarsReaderConfig : UserControl
    {
        private readonly ObservableCollection<string> COMPorts = [];
        private readonly string ConnectedCOMPort = string.Empty;

        public CodeBarsReaderConfig()
        {
            InitializeComponent();

            ConnectedCOMPort = SCodeBarsReader.Default.COM;
            var availablePorts = SerialPort.GetPortNames();

            foreach (var port in availablePorts)
            {
                COMPorts.Add(port);
            }

            if (!COMPorts.Contains(ConnectedCOMPort))
            {
                COMPorts.Add(ConnectedCOMPort);
            }

            CbCOM.ItemsSource = COMPorts;
            CbCOM.SelectedItem = ConnectedCOMPort;
            DataContext = this;

            Task.Run(
                () =>
                    MessageBox.Show(
                        "As alterações entrarão em vigor após a aplicação ser reiniciada.",
                        "Aviso"
                    )
            );
        }

        private void Save(object send, EventArgs e)
        {
            SCodeBarsReader.Default.COM = CbCOM.SelectedItem.ToString();
            SCodeBarsReader.Default.Save();
        }
    }
}
