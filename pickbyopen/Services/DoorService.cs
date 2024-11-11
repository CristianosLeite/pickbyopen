using Pickbyopen.Components;
using Pickbyopen.Devices.Plc;
using Pickbyopen.Interfaces;
using Pickbyopen.Settings;
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
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen1, Doors[0], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen2, Doors[1], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen3, Doors[2], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen4, Doors[3], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen5, Doors[4], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen6, Doors[5], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen7, Doors[6], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen8, Doors[7], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen9, Doors[8], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen10, Doors[9], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen11, Doors[10], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen12, Doors[11], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen13, Doors[12], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen14, Doors[13], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen15, Doors[14], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen16, Doors[15], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen17, Doors[16], Context.Open);
            DoorSubscription(SPlcAddresses.Default.ReadIsOpen18, Doors[17], Context.Open);

            // Refill doors
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill10, Doors[9], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill11, Doors[10], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill12, Doors[11], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill13, Doors[12], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill14, Doors[13], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill15, Doors[14], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill16, Doors[15], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill17, Doors[16], Context.Refill);
            DoorSubscription(SPlcAddresses.Default.ReadNeedsRefill18, Doors[17], Context.Refill);
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
                1 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen1),
                2 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen2),
                3 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen3),
                4 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen4),
                5 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen5),
                6 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen6),
                7 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen7),
                8 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen8),
                9 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen9),
                10 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen10),
                11 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen11),
                12 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen12),
                13 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen13),
                14 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen14),
                15 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen15),
                16 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen16),
                17 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen17),
                18 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadIsOpen18),
                _ => false,
            };
        }

        public async Task<bool> NeedsReffil(int door)
        {
            return door switch
            {
                10 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill10),
                11 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill11),
                12 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill12),
                13 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill13),
                14 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill14),
                15 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill15),
                16 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill16),
                17 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill17),
                18 => (bool)await _plc.ReadFromPlc(SPlcAddresses.Default.ReadNeedsRefill18),
                _ => false,
            };
        }
    }
}
