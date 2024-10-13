using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Services;
using Pickbyopen.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media.Effects;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para Login.xam
    /// </summary>
    public partial class Login : UserControl
    {
        private readonly Db db;
        public bool IsWorkDone { get; private set; }

        public Login()
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            PartnumberRepository partnumberRepository = new(connectionFactory);
            UserRepository userRepository = new(connectionFactory);
            LogRepository logRepository = new(connectionFactory);

            db = new(connectionFactory, partnumberRepository, userRepository, logRepository);
        }

        private void ReloadWindow(object sernder, RoutedEventArgs e)
        {
            if (!IsWorkDone)
                Dispatcher.Invoke(() =>
                {
                    App.Current.MainWindow.Effect = new BlurEffect();
                    NfcWindow nfcWindow = new(Types.Context.Login, new("", "", "", []));
                    nfcWindow.Show();
                });
            App.Current.MainWindow.IsEnabled = true;
            App.Current.MainWindow.Effect = null;
        }

        private async void Login_Click(object sender, RoutedEventArgs e)
        {
            User? user = await db.FindUserByBadgeNumber(BadgeNumber.Text);

            if (user != null)
            {
                IsWorkDone = true;
                Auth.SetLoggedInUser(user);
                Window.GetWindow(this)!.Close();
            }
            else
            {
                MessageBox.Show(
                    "Usuário não encontrado",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
