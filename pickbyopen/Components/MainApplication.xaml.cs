using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;
using Pickbyopen.Database;
using Pickbyopen.Devices.CodebarsReader;
using Pickbyopen.Devices.Plc;
using Pickbyopen.Models;
using Pickbyopen.Services;
using Pickbyopen.Types;
using Pickbyopen.Windows;

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
        private readonly Dictionary<Control, bool> IsOrangeStates = [];
        private bool IsAutomatic = true;
        private bool IsMaintenance = true;
        private readonly Plc _plc = new();
        private readonly DispatcherTimer _buttonPressTimer;
        private bool _isButtonPressed;

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

            _buttonPressTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _buttonPressTimer.Tick += ButtonPressTimer_Tick;
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
                    string data = codebarsReader.GetData().TrimEnd();
                    if (!string.IsNullOrEmpty(data))
                    {
                        if (data.Length == 10)
                            PartnumberChanged(data);
                        else if (data.Length == 14)
                            VPOrChassiChanged(data, "");
                        else if (data.Length == 17)
                            VPOrChassiChanged("", data);
                        else
                            StatusInput.Text = "Leitura inválida.";

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
            int intDoor = int.Parse(associatedDoor.Content.ToString()!);
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
                                    CloseDoor(associatedDoor, intDoor);
                                    DoorInput.Text = "0";
                                    StatusInput.Text = "Aguardando leitura.";
                                })
                            );
                            return;
                        }

                        Dispatcher.BeginInvoke(
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
            string vp = VPInput.Text;
            string chassi = ChassiInput.Text;

            if (PartnumberInput.Text is string partnumber && partnumber.Length == 10)
                PartnumberChanged(partnumber);
            else if (vp.Length == 14 || chassi.Length == 17)
                VPOrChassiChanged(vp, chassi);
        }

        private async void PartnumberChanged(string partnumber)
        {
            PartnumberInput.Text = partnumber;

            bool isPlcConnected = await EnsurePlcConnection();

            if (isPlcConnected)
            {
                if (partnumber.Length < 10)
                {
                    StatusInput.Text = "Leitura inválida.";
                    return;
                }
                try
                {
                    int door = await db.GetAssociatedDoor(partnumber);
                    if (door == 0)
                    {
                        StatusInput.Text = "Desenho não encontrado.";
                        return;
                    }

                    _ = door switch
                    {
                        1 => door = 10,
                        2 => door = 11,
                        3 => door = 12,
                        4 => door = 13,
                        5 => door = 14,
                        6 => door = 15,
                        7 => door = 16,
                        8 => door = 17,
                        9 => door = 18,
                        _ => door = 0,
                    };

                    await WriteToPlc(door, partnumber, Event.Reading);
                }
                catch (Exception e)
                {
                    MessageBox.Show("Erro ao interagir com o PLC. " + e, "Erro");
                }
            }
        }

        private async void VPOrChassiChanged(string vp, string chassi)
        {
            if (string.IsNullOrEmpty(vp))
            {
                ChassiInput.Text = chassi;
                StatusInput.Text = "Aguardando leitura do VP.";
                return;
            }

            if (string.IsNullOrEmpty(chassi))
            {
                VPInput.Text = vp;
                StatusInput.Text = "Aguardando leitura do chassi.";
                return;
            }

            if (vp.Length != 0 && vp.Length != 14)
            {
                StatusInput.Text = "Leitura inválida.";
                return;
            }

            if (chassi.Length != 0 && chassi.Length != 17)
            {
                StatusInput.Text = "Leitura inválida.";
                return;
            }

            List<int> doors = Enumerable.Range(1, 9).ToList();
            if (await CheckForOpenDoors(doors))
            {
                StatusInput.Text = "Aguardando fechamento das portas.";
                return;
            }

            Recipe recipe = await db.GetRecipeByVp(vp);

            if (recipe == null)
            {
                StatusInput.Text = "Receita não cadastrada.";
                return;
            }

            RecipeInput.Text = recipe.Description;

            var doorsToOpen = await db.GetRecipeAssociatedDoors(vp);
            foreach (var door in doorsToOpen)
            {
                await WriteToPlc(door, vp, Event.Reading);
            }

            StatusInput.Text = "Receita carregada com sucesso!";
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

            if (door < 10)
                await _plc.WriteToPlc("DB1.BYTE0", door.ToString()); // Frontside intDoor
            else
                await _plc.WriteToPlc("DB1.BYTE1", door.ToString()); // Backside intDoor

            if (!IsMaintenance)
                await db.LogUserOperate(
                    @event == Event.Reading ? "Leitura" : "Seleção",
                    target,
                    door.ToString(),
                    IsAutomatic ? "Automático" : "Manual",
                    Auth.GetUserId()
                );
        }

        private void StartFlashing(Control control, Command command, int door)
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

        private void FlashOrange(Control control)
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

        private static void SetOrange(Control control)
        {
            control.Background = Brushes.Orange;
        }

        private async void ManualDoorOpen(object sender, RoutedEventArgs e)
        {
            if (IsAutomatic)
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

                if (!IsMaintenance) // Bypass verification in maintenance mode
                {
                    if (door < 10)
                    {
                        List<int> doors = Enumerable.Range(1, 9).ToList();
                        if (await CheckForOpenDoors(doors))
                            return;
                    }
                }
                // In case of direct selection, there will be no partnumber, target will be port number
                _ = WriteToPlc(door, door.ToString(), Event.Selection);
                SetOpen(button, door);
            }
        }

        private async Task<bool> CheckForOpenDoors(List<int> doors)
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

        private async void SetOpen(Control control, int door)
        {
            if (!await IsDoorOpen(door))
                return;

            var command = new Command();
            command.SetOpen();
            StartFlashing(control, command, door);
            DoorInput.Text = door.ToString();
            StatusInput.Text = "Aberta";
        }

        private void CloseDoor(Control control, int door)
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

        private void Refill(Control control, int door)
        {
            var command = new Command();
            command.SetRefill();
            StartFlashing(control, command, door);
        }

        private async Task<bool> IsDoorOpen(int door)
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

        private async Task<bool> NeedsReffil(int door)
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

        private void SetMode()
        {
            if (!IsAutomatic)
            {
                ModeButton.Foreground = Brushes.LightBlue;
                ModeButton.ToolTip = "Modo manual";
                ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Hand;
                PartnumberInput.IsEnabled = true;
                VPInput.IsEnabled = true;
                ChassiInput.IsEnabled = true;
                IsMaintenance = false;
                return;
            }
            try
            {
                InitializeCodeBarsReader();
                ModeButton.Foreground = Brushes.LightGreen;
                ModeButton.ToolTip = "Modo automático";
                ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Automatic;
                PartnumberInput.IsEnabled = false;
                VPInput.IsEnabled = false;
                ChassiInput.IsEnabled = false;
                IsMaintenance = false;
            }
            catch
            {
                IsAutomatic = false;

                if (!IsMaintenance)
                    SetMode();

                return;
            }
        }

        private async void SwitchMode(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("M"))
            {
                MessageBox.Show(
                    "Usuário não tem permissão para alterar o modo de operação.",
                    "Erro"
                );
                return;
            }

            if (!IsMaintenance)
                IsAutomatic = !IsAutomatic;

            SetMode();
            await db.LogSysSwitchedMode(IsAutomatic ? "Automático" : "Manual");
        }

        private void ShowLogs(object sender, RoutedEventArgs e)
        {
            LogsWindow logs = new();
            logs.Show();
        }

        private void ModeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonPressed = true;
            _buttonPressTimer.Start();
        }

        private void ModeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            _isButtonPressed = false;
            _buttonPressTimer.Stop();
        }

        private void ButtonPressTimer_Tick(object? sender, EventArgs e)
        {
            if (_isButtonPressed)
            {
                _buttonPressTimer.Stop();
                EnterMaintenanceMode();
            }
        }

        private void EnterMaintenanceMode()
        {
            SetMaitenanceMode();
        }

        private void SetMaitenanceMode()
        {
            ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Tools;
            ModeButton.Foreground = Brushes.Orange;
            ModeButton.ToolTip = "Modo de manutenção";
            PartnumberInput.IsEnabled = true;
            VPInput.IsEnabled = true;
            ChassiInput.IsEnabled = true;
            IsAutomatic = false;
            IsMaintenance = true;
            MessageBox.Show(
                "Modo de manutenção ativado. Logs e registros de operações não serão salvos.",
                "Aviso!"
            );
        }
    }
}
