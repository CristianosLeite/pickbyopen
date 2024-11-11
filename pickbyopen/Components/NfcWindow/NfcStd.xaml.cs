using Pickbyopen.Types;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para NfcStd.xaml
    /// </summary>
    public partial class NfcStd : UserControl
    {
        public Context Context;
        public NfcStd(Context context)
        {
            InitializeComponent();

            Context = context;

            SetTitle();
        }

        private void SetTitle()
        {
            if (Context == Context.Create)
            {
                Title.Text = "Cadastrar NFC";
                Subtitle.Text =
                    "Aproxime o crachá de identificação do usuário no leitor de nfc para efetuar o cadastro";
            }
            else
            {
                Title.Text = "Autenticar-se";
                Subtitle.Text =
                    "Aproxime seu crachá de identificação no leitor de nfc para efetuar o login";
            }
        }
    }
}
