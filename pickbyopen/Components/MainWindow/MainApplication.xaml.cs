using Pickbyopen.Database;
using Pickbyopen.Devices.Plc;
using Pickbyopen.Services;
using Pickbyopen.Types;
using Pickbyopen.Utils;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pickbyopen.Components
{
    public partial class MainApplication : UserControl
    {
        private readonly Db _db;
        private readonly Plc _plc;
        private readonly LogService _logService;
        private readonly ModeService _modeService;
        private readonly PlcService _plcService;
        private readonly DoorService _doorService;
        private readonly CodeBarsReaderService _codeBarsReaderService;

        public MainApplication()
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();

            _db = new(connectionFactory);
            _plc = new Plc();
            _logService = new LogService(connectionFactory);
            _modeService = new ModeService(this, _logService);
            _plcService = new PlcService(_plc, _modeService, _logService);
            _doorService = new DoorService(this, _modeService, _plc, _plcService);
            _codeBarsReaderService = new CodeBarsReaderService();

            _codeBarsReaderService.SubscribeReader(OnDataReceived);

            SetDoors();

            try
            {
                InitializeCodeBarsReader();
            }
            catch
            {
                // Do not throw on constructor
            }

            if (!IsCodeBarsReaderConnected())
                SetMode(); // Manual mode will be set

            InitializePlc();
        }

        private void SetDoors()
        {
            _doorService.SetDoors(
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
        }

        public void InitializeCodeBarsReader()
        {
            try
            {
                _codeBarsReaderService.InitializeCodeBarsReader();
            }
            catch (Exception e)
            {
                ErrorMessage.Show(
                    "Erro ao conectar o leitor de código de barras. Operação será executada em modo manual.\n"
                        + e
                );
                throw; // Throw exception on ModeService
            }
        }

        private void InitializePlc()
        {
            Task.Run(async () =>
                {
                    if (!await ConnectPlc())
                        ErrorMessage.Show("Erro ao conectar com o PLC.");
                })
                .ContinueWith(async t =>
                {
                    if (t.IsCompleted)
                        if (await IsPlcConnected())
                            SubscribeDoors();
                });
        }

        private void OnDataReceived(object? sender, string data)
        {
            Dispatcher.Invoke(() =>
            {
                if (data.Length == 14)
                {
                    VPInput.Text = data;
                }
                else if (data.Length == 17)
                {
                    ChassiInput.Text = data;
                }
                else if (data.Length > 5 && data.Length < 11)
                {
                    // Add 0 until reach 10 characters
                    data = data.PadLeft(10, '0');
                    PartnumberInput.Text = data;
                }
                else
                {
                    data = "";
                }
            });
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
                    int door = await _db.GetAssociatedDoor(partnumber);
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

                    await WriteToPlc(door, partnumber, string.Empty, Event.Reading);
                }
                catch (Exception e)
                {
                    ErrorMessage.Show("Erro ao interagir com o PLC. " + e);
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

            if (!IsValidVp(vp) || !IsValidChassi(chassi))
            {
                StatusInput.Text = "Leitura inválida.";
                return;
            }

            if (await AreDoorsOpen())
            {
                StatusInput.Text = "Aguardando fechamento das portas.";
                return;
            }

            var recipe = await _db.GetRecipeByVp(vp);
            if (recipe == null)
            {
                StatusInput.Text = "Receita não cadastrada.";
                return;
            }

            RecipeInput.Text = recipe.Description;
            await OpenDoorsForRecipe(vp, chassi);
            StatusInput.Text = "Receita carregada com sucesso!";
        }

        private static bool IsValidVp(string vp) => vp.Length == 14;

        private static bool IsValidChassi(string chassi) => chassi.Length == 17;

        private async Task<bool> AreDoorsOpen()
        {
            List<int> doors = Enumerable.Range(1, 9).ToList();
            return await CheckForOpenDoors(doors);
        }

        private async Task OpenDoorsForRecipe(string vp, string chassi)
        {
            var doorsToOpen = await _db.GetRecipeAssociatedDoors(vp);
            foreach (var door in doorsToOpen)
            {
                if (_doorService.Subscriptions.Count == 0)
                    SubscribeDoors();
                await WriteToPlc(door, vp, chassi, Event.Reading);
                Thread.Sleep(100);
            }
        }

        private bool IsCodeBarsReaderConnected() =>
            _codeBarsReaderService.IsCodeBarsReaderConnected();

        private async Task<bool> ConnectPlc() => await _plcService.Connect();

        private void SubscribeDoors() => _doorService.SubscribeDoors();

        private void UnsubscribeAll(object sender, RoutedEventArgs e) =>
            _doorService.UnsubscribeAll(sender, e);

        private async Task<bool> EnsurePlcConnection() => await _plcService.EnsureConnection();

        private async Task<bool> IsPlcConnected() => await _plc.GetPlcStatus();

        private async Task WriteToPlc(int door, string target, string chassi, Event @event)
        {
            if (_doorService.Subscriptions.Count == 0)
                _doorService.SubscribeDoors();

            if (!Auth.UserHasPermission("O"))
            {
                ErrorMessage.Show("Usuário não tem permissão para abrir portas.");
                return;
            }
            await _plcService.WriteToPlc(door, target, chassi, @event);

            // Await for 3 seconds and reset the values
            _ = Task.Run(async () =>
            {
                await Task.Delay(3000);
                Dispatcher.Invoke(() =>
                {
                    PartnumberInput.Text = string.Empty;
                    VPInput.Text = string.Empty;
                    ChassiInput.Text = string.Empty;
                    PartnumberInput.Text = string.Empty;
                    RecipeInput.Text = "Nenhuma receita selecionada";
                    StatusInput.Text = "Aguardando leitura...";
                });
            });
        }

        private void ManualDoorOpen(object sender, RoutedEventArgs e) =>
            _doorService.ManualDoorOpen(sender, e);

        private async Task<bool> CheckForOpenDoors(List<int> doors) =>
            await _doorService.CheckForOpenDoors(doors);

        private void SetMode() => _modeService.SetMode();

        private void SwitchMode(object sender, RoutedEventArgs e) =>
            _modeService.SwitchMode(sender, e);

        private void ShowLogs(object sender, RoutedEventArgs e) => _logService.ShowLogs(sender, e);

        private void ModeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e) =>
            _modeService.ModeButton_MouseLeftButtonDown(sender, e);

        private void ModeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e) =>
            _modeService.ModeButton_MouseLeftButtonUp(sender, e);
    }
}
