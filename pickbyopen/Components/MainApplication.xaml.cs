using Pickbyopen.Database;
using Pickbyopen.Devices.CodebarsReader;
using Pickbyopen.Devices.Plc;
using Pickbyopen.Services;
using Pickbyopen.Types;
using Pickbyopen.Windows;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para MainApplication.xaml
    /// </summary>
    public partial class MainApplication : UserControl
    {
        private readonly List<IDisposable> _subscriptions = [];
        private readonly List<Button> _doors = [];
        private readonly Db db;
        private readonly Dictionary<Control, DispatcherTimer> Timers = [];
        private readonly Dictionary<Control, bool> IsGreenStates = [];
        private readonly Dictionary<Control, bool> IsRedStates = [];
        private bool IsAutomatic = true;
        private readonly Plc _plc = new();

        public MainApplication()
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            _doors.AddRange(
                [
                    door1,
                    door2,
                    door3,
                    door4,
                    door5,
                    door6,
                    door7,
                    door8,
                    door9,
                    door10,
                    door11,
                    door12,
                    door13,
                    door14,
                    door15,
                    door16,
                    door17,
                    door18,
                ]
            );

            try
            {
                InitializeCodeBarsReader();
            }
            catch
            {
                IsAutomatic = false;
                SetMode();
            }

            Task.Run(async () =>
            {
                await ConnectPlc();
            });

            DoorInput.Text = "0";
            StatusInput.Text = "Aguardando leitura";
        }

        private void InitializeCodeBarsReader()
        {
            try
            {
                CodebarsReader codebarsReader = new();
                codebarsReader.Connect("COM3");

                var timer = new DispatcherTimer { Interval = TimeSpan.FromMilliseconds(500) };
                timer.Tick += (sender, e) =>
                {
                    string data = codebarsReader.GetData();
                    if (!string.IsNullOrEmpty(data))
                    {
                        PartnumberChanged(data);
                        codebarsReader.ClearComPort();
                    }
                };
                timer.Start();
            }
            catch (Exception e)
            {
                MessageBox.Show(
                    "Erro ao conectar o leitor de código de barras. Operação será executada em modo manual.\n"
                        + e,
                    "Erro"
                );
                throw;
            }
        }

        private async Task<bool> ConnectPlc()
        {
            bool isConnected = await _plc.InitializePlc();

            if (!isConnected)
            {
                MessageBox.Show("Falha ao conectar com o PLC, tente novamente.", "Erro");
                return false;
            }

            if (_subscriptions.Count == 0)
                SubscribeDoors();

            return isConnected;
        }

        private void SubscribeDoors()
        {
            // Open doors
            DoorSubscription("DB1.DBX2.0", door1, Context.Open);
            DoorSubscription("DB1.DBX2.1", door2, Context.Open);
            DoorSubscription("DB1.DBX2.2", door3, Context.Open);
            DoorSubscription("DB1.DBX2.3", door4, Context.Open);
            DoorSubscription("DB1.DBX2.4", door5, Context.Open);
            DoorSubscription("DB1.DBX2.5", door6, Context.Open);
            DoorSubscription("DB1.DBX2.6", door7, Context.Open);
            DoorSubscription("DB1.DBX2.7", door8, Context.Open);
            DoorSubscription("DB1.DBX3.0", door9, Context.Open);
            DoorSubscription("DB1.DBX3.1", door10, Context.Open);
            DoorSubscription("DB1.DBX3.2", door11, Context.Open);
            DoorSubscription("DB1.DBX3.3", door12, Context.Open);
            DoorSubscription("DB1.DBX3.4", door13, Context.Open);
            DoorSubscription("DB1.DBX3.5", door14, Context.Open);
            DoorSubscription("DB1.DBX3.6", door15, Context.Open);
            DoorSubscription("DB1.DBX3.7", door16, Context.Open);
            DoorSubscription("DB1.DBX4.0", door17, Context.Open);
            DoorSubscription("DB1.DBX4.1", door18, Context.Open);

            // Refill doors
            DoorSubscription("DB1.DBX4.2", door10, Context.Refill);
            DoorSubscription("DB1.DBX4.3", door11, Context.Refill);
            DoorSubscription("DB1.DBX4.4", door12, Context.Refill);
            DoorSubscription("DB1.DBX4.5", door13, Context.Refill);
            DoorSubscription("DB1.DBX4.6", door14, Context.Refill);
            DoorSubscription("DB1.DBX4.7", door15, Context.Refill);
            DoorSubscription("DB1.DBX5.0", door16, Context.Refill);
            DoorSubscription("DB1.DBX5.1", door17, Context.Refill);
            DoorSubscription("DB1.DBX5.2", door18, Context.Refill);
        }

        private void DoorSubscription(string address, Button associatedDoor, Context context)
        {
            _subscriptions.Add(
                _plc.SubscribeAddress<bool>(
                    address,
                    value =>
                    {
                        if (!value)
                        {
                            Dispatcher.BeginInvoke(
                                new Action(() =>
                                {
                                    CloseDoor(associatedDoor);
                                    DoorInput.Text = "0";
                                    StatusInput.Text = "Aguardando leitura";
                                })
                            );
                            return;
                        }

                        Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                if (context == Context.Open)
                                    OpenDoor(
                                        associatedDoor,
                                        int.Parse(associatedDoor.Content.ToString()!)
                                    );
                                else if (context == Context.Refill)
                                    Refill(associatedDoor);
                                else
                                    Empty(associatedDoor);
                            })
                        );
                    }
                )
            );
        }

        private void UnsubscribeAll(object sender, RoutedEventArgs e)
        {
            foreach (var subscription in _subscriptions)
            {
                subscription.Dispose();
            }
            _subscriptions.Clear();
        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            if (PartnumberInput.Text is string content)
            {
                if (content.Length == 15)
                    PartnumberChanged(content);
            }
        }

        private async void PartnumberChanged(string partnumber)
        {
            partnumber = partnumber.TrimEnd();
            PartnumberInput.Text = partnumber;

            bool isPlcConnected = await EnsurePlcConnection();

            if (isPlcConnected)
            {
                if (partnumber.Length < 15)
                {
                    StatusInput.Text = "Leitura inválida";
                    return;
                }
                try
                {
                    int door = await db.GetAssociatedDoor(partnumber);
                    if (door == 0)
                    {
                        StatusInput.Text = "Desenho não encontrado";
                        return;
                    }

                    await WriteToPlc(door, partnumber, Event.Reading);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Erro ao interagir com o PLC. " + e, "Erro");
                }
            }
        }

        private async Task<bool> EnsurePlcConnection()
        {
            bool isPlcConnected = await _plc.GetPlcStatus();

            if (!isPlcConnected)
            {
                isPlcConnected = await ConnectPlc();
            }

            return isPlcConnected;
        }

        private async Task WriteToPlc(int door, string target, Event @event)
        {
            if (_subscriptions.Count == 0)
                SubscribeDoors();

            if (!Auth.UserHasPermission("O"))
            {
                MessageBox.Show("Usuário não tem permissão para abrir portas.", "Erro");
                return;
            }

            await _plc.WriteToPlc("DB1.INT0", door.ToString());
            await db.LogUserOperate(
                @event == Event.Reading ? "Leitura" : "Seleção",
                target,
                door.ToString(),
                IsAutomatic ? "Automático" : "Manual",
                Auth.GetUserId()
            );
        }

        private void StartFlashing(Control control, Command command)
        {
            if (Timers.TryGetValue(control, out DispatcherTimer? value))
            {
                value.Stop();
                Timers.Remove(control);
            }

            var timer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(0.8) };

            switch (command)
            {
                case Command c when c.Open:
                    timer.Tick += (sender, e) => FlashGreen(control);
                    break;
                case Command c when c.Refill:
                    timer.Tick += (sender, e) => SetOrange(control);
                    break;
                case Command c when c.Empty:
                    timer.Tick += (sender, e) => FlashRed(control);
                    break;
            }
            Timers[control] = timer;
            IsGreenStates[control] = false;
            IsRedStates[control] = false;
            timer.Start();
        }

        private void FlashGreen(Control control)
        {
            if (IsGreenStates[control])
            {
                control.Background = Brushes.Transparent;
                control.Foreground = Brushes.White;
            }
            else
            {
                control.Background = Brushes.Green;
                control.Foreground = Brushes.Black;
            }
            IsGreenStates[control] = !IsGreenStates[control];
        }

        private void FlashRed(Control control)
        {
            if (IsRedStates[control])
            {
                control.Background = Brushes.Transparent;
                control.Foreground = Brushes.White;
            }
            else
            {
                control.Background = Brushes.Red;
                control.Foreground = Brushes.Black;
            }
            IsRedStates[control] = !IsRedStates[control];
        }

        private static void SetOrange(Control control)
        {
            control.Background = Brushes.Orange;
        }

        private async void ManualDoorOpen(object sender, RoutedEventArgs e)
        {
            if (IsAutomatic)
                return;

            if (!Auth.UserHasPermission("O"))
            {
                MessageBox.Show("Usuário não tem permissão para abrir portas.", "Erro");
                return;
            }

            if (!await _plc.GetPlcStatus())
            {
                MessageBox.Show("Erro ao conectar com o PLC. Tente novamente.", "Erro");
                return;
            }

            if (sender is Button button)
            {
                int door = int.Parse(button.Content.ToString()!);
                // In case of direct selection, there will be no partnumber, target will be port number
                _ = WriteToPlc(door, door.ToString(), Event.Selection);
                OpenDoor(button, door);
            }
        }

        private void OpenDoor(Control control, int door)
        {
            var command = new Command();
            command.SetOpen();
            StartFlashing(control, command);
            DoorInput.Text = door.ToString();
            StatusInput.Text = "Aberta";
        }

        private void CloseDoor(Control control)
        {
            if (Timers.TryGetValue(control, out DispatcherTimer? value))
            {
                value.Stop();
                Timers.Remove(control);
            }
            control.Background = Brushes.LightGray;
            control.Foreground = Brushes.White;
        }

        private void Refill(Control control)
        {
            var command = new Command();
            command.SetRefill();
            StartFlashing(control, command);
        }

        private void Empty(Control control)
        {
            var command = new Command();
            command.SetEmpty();
            StartFlashing(control, command);
        }

        private void SetMode()
        {
            if (!IsAutomatic)
            {
                ModeButton.Foreground = Brushes.LightBlue;
                ModeButton.ToolTip = "Modo manual";
                ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Hand;
                PartnumberInput.IsEnabled = true;
                return;
            }
            try
            {
                InitializeCodeBarsReader();
                ModeButton.Foreground = Brushes.LightGreen;
                ModeButton.ToolTip = "Modo automático";
                ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Automatic;
                PartnumberInput.IsEnabled = false;
            }
            catch
            {
                IsAutomatic = false;
                return;
            }
        }

        private async void SwitchMode(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("O"))
            {
                MessageBox.Show(
                    "Usuário não tem permissão para alterar o modo de operação.",
                    "Erro"
                );
                return;
            }

            IsAutomatic = !IsAutomatic;
            SetMode();
            await db.LogSysSwitchedMode(IsAutomatic ? "Automático" : "Manual");
        }

        private void ShowLogs(object sender, RoutedEventArgs e)
        {
            LogsWindow logs = new();
            logs.Show();
        }
    }
}
