using Pickbyopen.Components;
using Pickbyopen.Devices.Plc;
using Pickbyopen.Interfaces;
using Pickbyopen.Types;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pickbyopen.Services
{
    public class DoorService(
        MainApplication mainApplication,
        ModeService modeService,
        Plc plc,
        PlcService plcService
    ) : IDoorService
    {
        private readonly ModeService _modeService = modeService;
        private readonly Plc _plc = plc;
        private readonly PlcService _plcService = plcService;
        private readonly MainApplication MainApplication = mainApplication;
        private List<Button> Doors = [];
        private readonly Dictionary<Control, DispatcherTimer> Timers = [];
        private readonly Dictionary<Control, bool> IsGreenStates = [];
        private readonly Dictionary<Control, bool> IsOrangeStates = [];
        public readonly List<IDisposable> Subscriptions = [];

        public void SetDoors(List<Button> doors)
        {
            Doors = doors;
        }

        public void SubscribeDoors()
        {
            // Open doors
            DoorSubscription("DB1.DBX2.0", Doors[0], Context.Open);
            DoorSubscription("DB1.DBX2.1", Doors[1], Context.Open);
            DoorSubscription("DB1.DBX2.2", Doors[2], Context.Open);
            DoorSubscription("DB1.DBX2.3", Doors[3], Context.Open);
            DoorSubscription("DB1.DBX2.4", Doors[4], Context.Open);
            DoorSubscription("DB1.DBX2.5", Doors[5], Context.Open);
            DoorSubscription("DB1.DBX2.6", Doors[6], Context.Open);
            DoorSubscription("DB1.DBX2.7", Doors[7], Context.Open);
            DoorSubscription("DB1.DBX3.0", Doors[8], Context.Open);
            DoorSubscription("DB1.DBX3.1", Doors[9], Context.Open);
            DoorSubscription("DB1.DBX3.2", Doors[10], Context.Open);
            DoorSubscription("DB1.DBX3.3", Doors[11], Context.Open);
            DoorSubscription("DB1.DBX3.4", Doors[12], Context.Open);
            DoorSubscription("DB1.DBX3.5", Doors[13], Context.Open);
            DoorSubscription("DB1.DBX3.6", Doors[14], Context.Open);
            DoorSubscription("DB1.DBX3.7", Doors[15], Context.Open);
            DoorSubscription("DB1.DBX4.0", Doors[16], Context.Open);
            DoorSubscription("DB1.DBX4.1", Doors[17], Context.Open);

            // Refill doors
            DoorSubscription("DB1.DBX4.2", Doors[9], Context.Refill);
            DoorSubscription("DB1.DBX4.3", Doors[10], Context.Refill);
            DoorSubscription("DB1.DBX4.4", Doors[11], Context.Refill);
            DoorSubscription("DB1.DBX4.5", Doors[12], Context.Refill);
            DoorSubscription("DB1.DBX4.6", Doors[13], Context.Refill);
            DoorSubscription("DB1.DBX4.7", Doors[14], Context.Refill);
            DoorSubscription("DB1.DBX5.0", Doors[15], Context.Refill);
            DoorSubscription("DB1.DBX5.1", Doors[16], Context.Refill);
            DoorSubscription("DB1.DBX5.2", Doors[17], Context.Refill);
        }

        public void DoorSubscription(string address, Button associatedDoor, Context context)
        {
            int intDoor = int.Parse(associatedDoor.Content.ToString()!);
            Subscriptions.Add(
                _plc.SubscribeAddress<bool>(
                    address,
                    value =>
                    {
                        if (!value)
                        {
                            Application.Current.Dispatcher.BeginInvoke(
                                new Action(() =>
                                {
                                    CloseDoor(associatedDoor, intDoor);
                                    MainApplication.DoorInput.Text = "0";
                                    MainApplication.StatusInput.Text = "Aguardando leitura.";
                                })
                            );
                            return;
                        }

                        Application.Current.Dispatcher.BeginInvoke(
                            new Action(() =>
                            {
                                if (context == Context.Open)
                                    SetOpen(associatedDoor, intDoor);
                                else if (context == Context.Refill)
                                    Refill(associatedDoor, intDoor);
                                else
                                    MessageBox.Show("Comando não reconhecido.", "Erro");
                            })
                        );
                    }
                )
            );
        }

        public void UnsubscribeAll(object sender, RoutedEventArgs e)
        {
            foreach (var subscription in Subscriptions)
            {
                subscription.Dispose();
            }
            Subscriptions.Clear();
        }

        public void StartFlashing(Control control, Command command, int door)
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
                    {
                        if (door < 10) // Frontside intDoor
                        {
                            timer.Tick += (sender, e) => FlashGreen(control);
                        }
                        else
                        {
                            timer.Tick += (sender, e) => FlashOrange(control);
                        }
                    }
                    break;
                case Command c when c.Refill:
                    timer.Tick += (sender, e) => SetOrange(control);
                    break;
                case Command c when c.Empty:
                    timer.Tick += (sender, e) => FlashOrange(control);
                    break;
            }
            Timers[control] = timer;
            IsGreenStates[control] = false;
            IsOrangeStates[control] = false;
            timer.Start();
        }

        public void FlashGreen(Control control)
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

        public void FlashOrange(Control control)
        {
            if (IsOrangeStates[control])
            {
                control.Background = Brushes.Transparent;
                control.Foreground = Brushes.White;
            }
            else
            {
                control.Background = Brushes.Orange;
                control.Foreground = Brushes.Black;
            }
            IsOrangeStates[control] = !IsOrangeStates[control];
        }

        public void SetOrange(Control control)
        {
            control.Background = Brushes.Orange;
        }

        public async void ManualDoorOpen(object sender, RoutedEventArgs e)
        {
            if (_modeService.IsAutomatic)
            {
                MessageBox.Show("Operação não permitida em modo automático.", "Atenção");
                return;
            }

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

                if (!_modeService.IsMaintenance) // Bypass verification in maintenance mode
                {
                    if (door < 10)
                    {
                        List<int> doors = Enumerable.Range(1, 9).ToList();
                        if (await CheckForOpenDoors(doors))
                            return;
                    }
                }

                if (Subscriptions.Count == 0)
                    SubscribeDoors();

                // In case of direct selection, there will be no partnumber, target will be null
                _ = _plcService.WriteToPlc(door, null, null, Event.Selection);
                SetOpen(button, door);
            }
        }

        public async Task<bool> CheckForOpenDoors(List<int> doors)
        {
            foreach (int i in doors)
            {
                if (await IsDoorOpen(i))
                {
                    MessageBox.Show(
                        "Necessário fechar todas as portas antes de solicitar uma nova abertura.",
                        "Porta aberta"
                    );
                    return true;
                }
            }
            return false;
        }

        public async void SetOpen(Control control, int door)
        {
            if (!await IsDoorOpen(door))
                return;

            var command = new Command();
            command.SetOpen();
            StartFlashing(control, command, door);

            if (door > 10)
                MainApplication.DoorInput.Text = door.ToString();
        }

        public void CloseDoor(Control control, int door)
        {
            if (Timers.TryGetValue(control, out DispatcherTimer? value))
            {
                value.Stop();
                Timers.Remove(control);
            }
            control.Background = Brushes.LightGray;
            control.Foreground = Brushes.White;

            Task.Run(async () =>
            {
                await NeedsReffil(door);
            });
        }

        public void Refill(Control control, int door)
        {
            var command = new Command();
            command.SetRefill();
            StartFlashing(control, command, door);
        }

        public async Task<bool> IsDoorOpen(int door)
        {
            return door switch
            {
                1 => (bool)await _plc.ReadFromPlc("DB1.DBX2.0"),
                2 => (bool)await _plc.ReadFromPlc("DB1.DBX2.1"),
                3 => (bool)await _plc.ReadFromPlc("DB1.DBX2.2"),
                4 => (bool)await _plc.ReadFromPlc("DB1.DBX2.3"),
                5 => (bool)await _plc.ReadFromPlc("DB1.DBX2.4"),
                6 => (bool)await _plc.ReadFromPlc("DB1.DBX2.5"),
                7 => (bool)await _plc.ReadFromPlc("DB1.DBX2.6"),
                8 => (bool)await _plc.ReadFromPlc("DB1.DBX2.7"),
                9 => (bool)await _plc.ReadFromPlc("DB1.DBX3.0"),
                10 => (bool)await _plc.ReadFromPlc("DB1.DBX3.1"),
                11 => (bool)await _plc.ReadFromPlc("DB1.DBX3.2"),
                12 => (bool)await _plc.ReadFromPlc("DB1.DBX3.3"),
                13 => (bool)await _plc.ReadFromPlc("DB1.DBX3.4"),
                14 => (bool)await _plc.ReadFromPlc("DB1.DBX3.5"),
                15 => (bool)await _plc.ReadFromPlc("DB1.DBX3.6"),
                16 => (bool)await _plc.ReadFromPlc("DB1.DBX3.7"),
                17 => (bool)await _plc.ReadFromPlc("DB1.DBX4.0"),
                18 => (bool)await _plc.ReadFromPlc("DB1.DBX4.1"),
                _ => false,
            };
        }

        public async Task<bool> NeedsReffil(int door)
        {
            return door switch
            {
                10 => (bool)await _plc.ReadFromPlc("DB1.DBX4.2"),
                11 => (bool)await _plc.ReadFromPlc("DB1.DBX4.3"),
                12 => (bool)await _plc.ReadFromPlc("DB1.DBX4.4"),
                13 => (bool)await _plc.ReadFromPlc("DB1.DBX4.5"),
                14 => (bool)await _plc.ReadFromPlc("DB1.DBX4.6"),
                15 => (bool)await _plc.ReadFromPlc("DB1.DBX4.7"),
                16 => (bool)await _plc.ReadFromPlc("DB1.DBX5.0"),
                17 => (bool)await _plc.ReadFromPlc("DB1.DBX5.1"),
                18 => (bool)await _plc.ReadFromPlc("DB1.DBX5.2"),
                _ => false,
            };
        }
    }
}
