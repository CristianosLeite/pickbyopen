using Pickbyopen.Services;
using System.Windows;
using System.Windows.Media.Effects;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para InfoWindow.xaml
    /// </summary>
    public partial class InfoWindow : Window
    {
        public string Context { get; set; }

        public InfoWindow(string context)
        {
            InitializeComponent();

            Context = context;

            SetProperties();

            Name.Text = Auth.LoggedInUser?.Username;
            BadgeNumber.Text = Auth.LoggedInUser?.BadgeNumber;
            TimeLogin.Text = Auth.LoggedAt;
        }

        private void SetProperties()
        {
            Topmost = true;
            App.Current.MainWindow.IsEnabled = false;
            App.Current.MainWindow.Effect = new BlurEffect();
        }

        private void ReSetProperties(object sender, RoutedEventArgs e)
        {
            App.Current.MainWindow.IsEnabled = true;
            App.Current.MainWindow.Effect = null;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Auth.Logout();
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
