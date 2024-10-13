using Pickbyopen.Components;
using Pickbyopen.Database;
using Pickbyopen.Windows;
using System.Windows;

namespace Pickbyopen
{
    /// <summary>
    /// Interaction logic for MainWindow.xaml
    /// </summary>
    public partial class MainWindow : Window
    {
        private readonly Db db;

        public MainWindow()
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            PartnumberRepository partnumberRepository = new(connectionFactory);
            UserRepository userRepository = new(connectionFactory);
            LogRepository logRepository = new(connectionFactory);

            db = new(connectionFactory, partnumberRepository, userRepository, logRepository);

            Task.Run(() =>
            {
                var existingUsers = db.LoadUsersList();

                if (existingUsers.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NfcWindow nfcWindow = new("login");
                        nfcWindow.ShowDialog();
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UserWindow userWindow = new();
                        EditUser editUser = new(new("", "", "", []), "create");
                        userWindow.Main.Children.Clear();
                        userWindow.Main.Children.Add(editUser);
                        userWindow.ShowDialog();

                        NfcWindow nfcWindow = new("login");
                        nfcWindow.ShowDialog();
                    });
                }
            });
        }

        private void Window_Loaded(object sender, RoutedEventArgs e)
        {
            Header.Children.Add(new Header());
            Main.Children.Add(new MainApplication());
            Toolbar.Children.Add(new Toolbar());
            Footer.Children.Add(new Footer());
        }
    }
}
