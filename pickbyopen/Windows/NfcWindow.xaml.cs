﻿using Pickbyopen.Components;
using Pickbyopen.Database;
using Pickbyopen.Devices.Nfc;
using Pickbyopen.Models;
using Pickbyopen.Services;
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
        private readonly Db db = new(DatabaseConfig.ConnectionString!);
        private readonly string Context;
        private readonly User? User = null;
        public event EventHandler<bool>? WorkDone;
        public bool IsWorkDone { get; private set; }

        public NfcWindow(string context, User? user = null)
        {
            InitializeComponent();

            Context = context;
            User = user;

            Main.Children.Add(new NfcStd(Context));
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
                acr122u.Init(false, 50, 4, 4, 200);
                acr122u.CardInserted += Acr122u_CardInserted;
                acr122u.CardRemoved += Acr122u_CardRemoved;

                // wait for a signal
                ManualResetEvent waitHandle = new(false);
                waitHandle.WaitOne();

                acr122u.CardInserted -= Acr122u_CardInserted;
                acr122u.CardRemoved -= Acr122u_CardRemoved;
                Thread.Sleep(1000);
            }
            catch (Exception e)
            {
                HandleNfcInitializationError(e);
            }
        }

        private void HandleNfcInitializationError(Exception e)
        {
            if (Context == "login")
                Application.Current.Dispatcher.Invoke(() =>
                {
                    Login login = new();
                    Main.Children.Clear();
                    Main.Children.Add(login);
                    IsWorkDone = true;
                });
            else if (Context == "createUser")
                Application.Current.Dispatcher.Invoke(async () =>
                {
                    if (User != null)
                    {
                        User.Id = GenerateRandomId();
                        if (await SaveToDatabase(User))
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
                    NfcWindow nfcWindow = new(Context, User);
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
            if (Context == "login")
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
            else if (Context == "createUser")
            {
                Dispatcher.Invoke(async () =>
                {
                    await ShowLoadingAndError("Crachá de identificação já cadastrado");
                    NfcStd nfcStd = new("createUser");
                    Main.Children.Clear();
                    Main.Children.Add(nfcStd);
                });
            }
            else
                HandleContextError();
        }

        private void HandleNewUser(string id)
        {
            if (Context == "login")
            {
                Dispatcher.Invoke(async () =>
                {
                    await ShowLoadingAndUserNotFound();
                    NfcStd nfcStd = new("login");
                    Main.Children.Clear();
                    Main.Children.Add(nfcStd);
                });
            }
            else if (Context == "createUser")
            {
                Dispatcher.Invoke(async () =>
                {
                    if (User != null)
                    {
                        User.Id = id;

                        await SaveToDatabase(User);

                        Close();
                    }
                });
            }
            else
                HandleContextError();
        }

        private async Task<bool> SaveToDatabase(User user)
        {
            bool isSaved = await db.SaveUser(user);
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
            MessageBox.Show("Contexto inválido");
        }

        private string GenerateRandomId()
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