using Pickbyopen.Models;
using System.Windows;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para PlcConfiguration.xam
    /// </summary>
    public partial class PlcConfig : UserControl
    {
        public PlcConfig()
        {
            InitializeComponent();
            DataContext = new PlcSettingsViewModel();
        }

        private void SaveButton_Click(object send, EventArgs e)
        {
            MessageBox.Show(
                "As alterações entrarão em vigor após a aplicação ser reiniciada.",
                "Aviso"
            );
            Close();
        }

        private void ChangeSaveButtonVisibility(object send, EventArgs e)
        {
            SaveButton.Visibility = Visibility.Visible;
        }

        private void Close()
        {
            Window.GetWindow(this).Close();
        }
    }
}
