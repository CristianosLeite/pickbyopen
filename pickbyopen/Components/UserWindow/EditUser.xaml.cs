﻿using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Windows;
using System.Windows;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para EditUser.xaml
    /// </summary>
    public partial class EditUser : UserControl
    {
        private readonly Db db;
        private readonly Context Context;
        public string UserId { get; set; }

        public EditUser(User user, Context context)
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            DataContext = this;
            Context = context;

            UserId = user.Id;

            SetTitle(context);

            TbBadgeNumber.Text = user.BadgeNumber;
            TbUserName.Text = user.Username;

            CheckPermissions(user.Permissions);
        }

        private void SetTitle(Context context)
        {
            if (context == Context.Create)
            {
                Title.Content = "Cadastrar novo usuário";
            }
            else
            {
                Title.Content = "Editar usuário";
            }
        }

        private void CheckPermissions(object Permissions)
        {
            if (Permissions is IEnumerable<string> permissions)
            {
                foreach (var permission in permissions)
                {
                    if (permission == "O")
                    {
                        Operate.IsChecked = true;
                    }
                    else if (permission == "P")
                    {
                        CreatePartnumber.IsChecked = true;
                    }
                    else if (permission == "U")
                    {
                        CreateUser.IsChecked = true;
                    }
                    else if (permission == "R")
                    {
                        ExportReports.IsChecked = true;
                    }
                    else if (permission == "M")
                    {
                        Maintenance.IsChecked = true;
                    }
                }
            }
        }

        private void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                if (!ValidateInputs())
                {
                    return;
                }

                List<string> permissions = GetSelectedPermissions();

                User user = new(UserId, TbBadgeNumber.Text, TbUserName.Text, permissions);

                if (Context == Context.Create)
                    HandleCreateUser(user);
                else if (Context == Context.Update)
                    SaveUser(user.Id, permissions, Context);
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inesperado. " + ex.Message, "Erro");
            }
        }

        private void HandleCreateUser(User user)
        {
            Window parentWindow = Window.GetWindow(this);
            parentWindow.Hide();

            NfcWindow nfc = new(Context.Create, user);
            nfc.WorkDone += (sender, isWorkDone) =>
            {
                if (isWorkDone)
                {
                    // User created
                    ShowSuccessMessage();
                    parentWindow?.Close();
                }
                else
                {
                    // User already exists
                    parentWindow?.Show();
                }
            };
            nfc.ShowDialog();

            if (!nfc.IsWorkDone)
            {
                parentWindow?.Show();
            }
        }

        private void ShowSuccessMessage()
        {
            MessageBox.Show(
                $"Usuário {TbUserName.Text} {(Context == Context.Create ? "cadastrado" : "atualizado")} com sucesso.",
                "Sucesso"
            );
        }

        private bool ValidateInputs()
        {
            if (string.IsNullOrEmpty(TbBadgeNumber.Text) || string.IsNullOrEmpty(TbUserName.Text))
            {
                MessageBox.Show("Preencha todos os campos antes de salvar!", "Atenção");
                return false;
            }

            if (
                !Operate.IsChecked == true
                && !CreatePartnumber.IsChecked == true
                && !CreateUser.IsChecked == true
                && !ExportReports.IsChecked == true
            )
            {
                MessageBox.Show("Necessário atribuir ao menos uma permissão ao usuário");
                return false;
            }

            return true;
        }

        private List<string> GetSelectedPermissions()
        {
            List<string> permissions = [];

            if (Operate.IsChecked == true)
            {
                permissions.Add("O");
            }

            if (CreatePartnumber.IsChecked == true)
            {
                permissions.Add("P");
            }

            if (CreateUser.IsChecked == true)
            {
                permissions.Add("U");
            }

            if (ExportReports.IsChecked == true)
            {
                permissions.Add("R");
            }

            if (Maintenance.IsChecked == true)
            {
                permissions.Add("M");
            }

            return permissions;
        }

        private async void SaveUser(string id, List<string> permissions, Context context)
        {
            User newUser = new(id, TbBadgeNumber.Text, TbUserName.Text, permissions);
            await db.SaveUser(newUser, context);
            ShowSuccessMessage();
        }

        private void Close()
        {
            AppUser user = new();
            var parentWindow = Window.GetWindow(this) as UserWindow;
            parentWindow?.Main?.Children.Clear();
            parentWindow?.Main.Children.Add(user);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }
    }
}
