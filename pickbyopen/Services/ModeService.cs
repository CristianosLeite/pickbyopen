using Pickbyopen.Components;
using Pickbyopen.Interfaces;
using Pickbyopen.Utils;
using System.Windows;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Threading;

namespace Pickbyopen.Services
{
    public class ModeService : IModeService
    {
        private readonly DispatcherTimer _buttonPressTimer;
        private bool _isButtonPressed;
        private readonly MainApplication MainApplication;
        private readonly LogService LogService;
        public bool IsAutomatic = false;
        public bool IsMaintenance = false;

        public ModeService(MainApplication mainApplication, LogService logService)
        {
            MainApplication = mainApplication;
            LogService = logService;
            _buttonPressTimer = new DispatcherTimer { Interval = TimeSpan.FromSeconds(3) };
            _buttonPressTimer.Tick += ButtonPressTimer_Tick;
        }

        public void SetMode()
        {
            if (MainApplication == null)
                return;

            if (!IsAutomatic)
            {
                MainApplication.ModeButton.Foreground = Brushes.LightBlue;
                MainApplication.ModeButton.ToolTip = "Modo manual";
                MainApplication.ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Hand;
                MainApplication.PartnumberInput.IsEnabled = true;
                MainApplication.VPInput.IsEnabled = true;
                MainApplication.ChassiInput.IsEnabled = true;
                IsMaintenance = false;
                return;
            }
            try
            {
                MainApplication.InitializeCodeBarsReader();
                MainApplication.ModeButton.Foreground = Brushes.LightGreen;
                MainApplication.ModeButton.ToolTip = "Modo automático";
                MainApplication.ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Automatic;
                MainApplication.PartnumberInput.IsEnabled = false;
                MainApplication.VPInput.IsEnabled = false;
                MainApplication.ChassiInput.IsEnabled = false;
                IsMaintenance = false;
            }
            catch (Exception e)
            {
                ErrorMessage.Show(
                    "Erro ao conectar o leitor de código de barras. Operação será executada em modo manual.\n"
                        + e
                );

                IsAutomatic = false;

                if (!IsMaintenance)
                    SetMode();

                return;
            }
        }

        public async void SwitchMode(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("M"))
            {
                ErrorMessage.Show("Usuário não tem permissão para alterar o modo de operação.");
                return;
            }
            if (!IsMaintenance)
                IsAutomatic = !IsAutomatic;

            SetMode();
            await LogService.LogSysSwitchedMode(IsAutomatic ? "Automático" : "Manual");
        }

        public void ModeButton_MouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            _isButtonPressed = true;
            _buttonPressTimer.Start();
        }

        public void ModeButton_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
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

        private async void EnterMaintenanceMode()
        {
            if (MainApplication == null)
                return;

            MainApplication.ModeIcon.Kind = MaterialDesignThemes.Wpf.PackIconKind.Tools;
            MainApplication.ModeButton.Foreground = Brushes.Orange;
            MainApplication.ModeButton.ToolTip = "Modo de manutenção";
            MainApplication.PartnumberInput.IsEnabled = true;
            MainApplication.VPInput.IsEnabled = true;
            MainApplication.ChassiInput.IsEnabled = true;
            IsAutomatic = false;
            IsMaintenance = true;

            MessageBox.Show(
                "Modo de manutenção ativado. Logs e registros de operações não serão salvos.",
                "Aviso!"
            );

            await LogService.LogSysSwitchedMode("Manutenção");
        }
    }
}
