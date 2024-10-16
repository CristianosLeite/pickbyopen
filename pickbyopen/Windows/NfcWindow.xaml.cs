using Pickbyopen.Components;
using Pickbyopen.Database;
using Pickbyopen.Devices.Nfc;
using Pickbyopen.Models;
using Pickbyopen.Services;
using Pickbyopen.Types;
using Pickbyopen.Utils;
using System.Diagnostics;
using System.Windows;
using System.Windows.Media.Effects;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para NfcWindow.xaml
    /// </summary>
    public partial class NfcWindow : Window
    {
        private readonly Db db;
        private readonly Context context;
        private readonly User? User = null;
        public event EventHandler<bool>? WorkDone;
        public bool IsWorkDone { get; private set; }

        public NfcWindow(Context context, User? user = null)
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            this.context = context;
            User = user;

            Main.Children.Add(new NfcStd(this.context));
            InitializeNfc();

            SetProperties();
        }

        private void SetProperties()
        {
            Topmost = true;
            App.Current.MainWindow.IsEnabled = false;
            App.Current.MainWindow.Effect = new BlurEffect();
        }

        private void InitializeNfc()
        {
            Task.Run(InitializeNfcReader);
        }

        private void InitializeNfcReader()
        {
            ACR122U acr122u = new();

            try
            {
                acr122u.Init(true);
                acr122u.CardInserted += Acr122u_CardInserted;
                acr122u.CardRemoved += Acr122u_CardRemoved;

                // wait for a signal
                ManualResetEvent waitHandle = new(false);
                waitHandle.WaitOne();

                acr122u.CardInserted -= Acr122u_CardInserted;
                acr122u.CardRemoved -= Acr122u_CardRemoved;
                Thread.Sleep(1000);
            }
            catch
            {
                HandleNfcInitializationError();
            }
        }

        private void HandleNfcInitializationError()
        {
            if (context == Context.Login)
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Login login = new();
                    Main.Children.Clear();
                    Main.Children.Add(login);
                    IsWorkDone = true;
                });
            else if (context == Context.Create)
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    if (User != null)
                    {
                        User.Id = GenerateRandomId();
                        if (await SaveToDatabase(User, Context.Create))
                            Close();
                    }
                });
            else
                HandleContextError();
        }

        private void ReloadWindow(object sernder, RoutedEventArgs e)
        {
            if (!IsWorkDone)
                Dispatcher.Invoke(() =>
                {
                    App.Current.MainWindow.Effect = new BlurEffect();
                    NfcWindow nfcWindow = new(context, User);
                    nfcWindow.Show();
                });
            App.Current.MainWindow.IsEnabled = true;
            App.Current.MainWindow.Effect = null;
        }

        private async void Acr122u_CardInserted(PCSC.ICardReader reader)
        {
            var uid = ACR122U.GetUID(reader);
            if (uid != null)
            {
                string id = BitConverter.ToString(uid).Replace("-", "");
                User? user = await db.GetUserById(id);

                if (user != null)
                {
                    HandleExistingUser(user);
                }
                else
                {
                    HandleNewUser(id);
                }
            }
        }

        private void HandleExistingUser(User user)
        {
            if (context == Context.Login)
            {
                Dispatcher.Invoke(async () =>
                {
                    IsWorkDone = true;
                    Auth.SetLoggedInUser(user);
                    Auth.SetLoggedAt(DateTime.Now.ToString("dd/MM/yyyy HH:mm:ss"));
                    await ShowLoadingAndSuccess();
                    Close();
                });
            }
            else if (context == Context.Create)
            {
                Dispatcher.Invoke(async () =>
                {
                    await ShowLoadingAndError("Crachá de identificação já cadastrado");
                    NfcStd nfcStd = new(Context.Create);
                    Main.Children.Clear();
                    Main.Children.Add(nfcStd);
                });
            }
            else
                HandleContextError();
        }

        private void HandleNewUser(string id)
        {
            if (context == Context.Login)
            {
                Dispatcher.Invoke(async () =>
                {
                    await ShowLoadingAndUserNotFound();
                    NfcStd nfcStd = new(Context.Login);
                    Main.Children.Clear();
                    Main.Children.Add(nfcStd);
                });
            }
            else if (context == Context.Create)
            {
                Dispatcher.Invoke(async () =>
                {
                    if (User != null)
                    {
                        User.Id = id;

                        IsWorkDone = await SaveToDatabase(User, Context.Create);

                        Close();
                    }
                });
            }
            else
                HandleContextError();
        }

        private async Task<bool> SaveToDatabase(User user, Context context)
        {
            bool isSaved = await db.SaveUser(user, context);
            if (isSaved)
            {
                IsWorkDone = true;
                await ShowLoadingAndSuccess();
                Dispatcher.Invoke(() => WorkDone?.Invoke(this, true));
                return true;
            }
            else
            {
                IsWorkDone = false;
                await ShowLoadingAndError("Usuário já cadastrado.");
                return false;
            }
        }

        private static void HandleContextError()
        {
            ErrorMessage.Show("Contexto inválido.");
        }

        private static string GenerateRandomId()
        {
            Random random = new();
            byte[] buffer = new byte[8];
            random.NextBytes(buffer);
            return BitConverter.ToString(buffer).Replace("-", "");
        }

        private async Task ShowLoadingAndSuccess()
        {
            Main.Children.Clear();
            Main.Children.Add(new NfcLoading());
            await Task.Delay(1000);

            Main.Children.Add(new NfcSuccess());
            await Task.Delay(1000);

            Main.Children.Clear();
        }

        private async Task ShowLoadingAndError(string errorMessage)
        {
            Main.Children.Clear();
            Main.Children.Add(new NfcLoading());
            await Task.Delay(1000);

            Main.Children.Add(new NfcError(errorMessage));
            await Task.Delay(1000);

            Main.Children.Clear();
        }

        private async Task ShowLoadingAndUserNotFound()
        {
            Main.Children.Add(new NfcLoading());
            await Task.Delay(1000);

            Main.Children.Add(new NfcUserNotFound());
            await Task.Delay(1000);

            Main.Children.Clear();
        }

        private void Acr122u_CardRemoved()
        {
            Debug.WriteLine("NFC tag removed.");
        }
    }
}
