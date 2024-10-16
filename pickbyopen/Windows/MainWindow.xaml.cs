using Pickbyopen.Components;
using Pickbyopen.Database;
using Pickbyopen.Types;
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
            db = new(connectionFactory);

            Task.Run(async () =>
            {
                var existingUsers = await db.LoadUsersList();

                if (existingUsers.Count > 0)
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        NfcWindow nfcWindow = new(Context.Login);
                        nfcWindow.ShowDialog();
                    });
                }
                else
                {
                    Application.Current.Dispatcher.Invoke(() =>
                    {
                        UserWindow userWindow = new();
                        EditUser editUser = new(new("", "", "", []), Context.Create);
                        userWindow.Main.Children.Clear();
                        userWindow.Main.Children.Add(editUser);
                        userWindow.ShowDialog();

                        NfcWindow nfcWindow = new(Context.Login);
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
