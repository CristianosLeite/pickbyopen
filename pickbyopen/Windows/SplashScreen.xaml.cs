using System.Windows;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para SplashScreen.xaml
    /// </summary>
    public partial class SplashScreen : Window
    {
        public SplashScreen()
        {
            InitializeComponent();

            Task.Run(() =>
            {
                Thread.Sleep(3000);
            }).ContinueWith(t =>
            {
                Dispatcher.Invoke(() =>
                {
                    var mainWindow = new MainWindow();
                    Application.Current.MainWindow = mainWindow;
                    Close();
                    mainWindow.Show();
                });
            }, TaskScheduler.FromCurrentSynchronizationContext());
        }
    }
}
