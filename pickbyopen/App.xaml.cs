using System.Globalization;
using System.Windows;

namespace Pickbyopen
{
    public partial class App : Application
    {
        protected override void OnStartup(StartupEventArgs e)
        {
            base.OnStartup(e);
            SetCulture();
        }

        private void SetCulture()
        {
            CultureInfo culture = new("pt-BR");
            Thread.CurrentThread.CurrentCulture = culture;
            Thread.CurrentThread.CurrentUICulture = culture;
        }
    }
}
