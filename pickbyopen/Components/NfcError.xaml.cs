using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para NfcError.xam
    /// </summary>
    public partial class NfcError : UserControl
    {
        public readonly string Message;
        public NfcError(string message)
        {
            InitializeComponent();

            Message = message;
            Text.Text = message;
        }
    }
}
