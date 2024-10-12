using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Pickbyopen.Database;
using Pickbyopen.Devices.Plc;
using Pickbyopen.Windows;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para Header.xaml
    /// </summary>
    public partial class Header : UserControl
    {
        private readonly Db db = new(DatabaseConfig.ConnectionString!);
        private readonly Plc _plc = new();
        private bool IsPlcConnected = false;
        private bool PreviousPlcStatus = false;

        public Header()
        {
            InitializeComponent();

            SetTimer();
            SetLoggedUser();
            UpdateStatus();
        }

        private void SetTimer()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            timer.Tick += (sender, e) => Clock.Text = DateTime.Now.ToString("HH:mm");
            timer.Start();
        }

        private void SetLoggedUser()
        {
            var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(1000) };
            timer.Tick += (sender, e) =>
            {
                if (Services.Auth.LoggedInUser != null)
                {
                    UserButton.Foreground = Brushes.LightBlue;
                }
                else
                {
                    UserButton.Foreground = Brushes.DarkGray;
                }
            };
            timer.Start();
        }

        private void UpdateStatus()
        {
            Task.Run(async () =>
            {
                while (true)
                {
                    IsPlcConnected = _plc.GetPlcStatus().Result;
                    SetPlcMonitor();
                    Thread.Sleep(1000);

                    if (IsPlcConnected != PreviousPlcStatus)
                    {
                        PreviousPlcStatus = IsPlcConnected;
                        await db.LogSysPlcStatusChanged(
                            IsPlcConnected ? "Conectado" : "Desconectado"
                        );
                    }
                }
            });
        }

        private void UserControl_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.ChangedButton == MouseButton.Left)
            {
                Window.GetWindow(this)?.DragMove();
            }
        }

        public async void SetPlcMonitor()
        {
            if (IsPlcConnected)
            {
                _ = PlcMonitor.Dispatcher.BeginInvoke(
                    new Action(() => PlcMonitor.Foreground = Brushes.Green)
                );
            }
            else
            {
                _ = PlcMonitor.Dispatcher.BeginInvoke(
                    new Action(() => PlcMonitor.Foreground = Brushes.Red)
                );
                await _plc.InitializePlc();
            }
        }

        private void CloseApplication(object sender, RoutedEventArgs e)
        {
            (Application.Current.MainWindow).Close();
            App.Current.Shutdown();
        }

        private void UserButton_Click(object sender, RoutedEventArgs e)
        {
            InfoWindow infoWindow = new("user");
            infoWindow.Show();
        }
    }
}
