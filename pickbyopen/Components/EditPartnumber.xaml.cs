﻿using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Windows;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Lógica interna para EditPartnumber.xaml
    /// </summary>
    public partial class EditPartnumber : UserControl
    {
        private readonly Db db = new(DatabaseConfig.ConnectionString!);
        private readonly List<int> _doors;
        private readonly AppPartnumber _createPartNumber;
        private readonly string context;
        public readonly int _selectedDoor;

        public EditPartnumber(
            AppPartnumber createPartnumber,
            Partnumber partnumber,
            int door,
            string context
        )
        {
            InitializeComponent();
            DataContext = this;
            this.context = context;

            if (context == "update")
            {
                Title.Content = "Editar Partnumber";
            }
            else
            {
                Title.Content = "Cadastrar Partnumber";
            }

            _createPartNumber = createPartnumber;
            _selectedDoor = door;

            _doors = Enumerable.Range(1, 18).ToList();
            Doors.ItemsSource = _doors;

            TbPartnumber.Text = partnumber.Code;
            Description.Text = partnumber.Description;
            Doors.SelectedValue = door;
        }

        private void SetInitialState()
        {
            TbPartnumber.Text = string.Empty;
            Description.Text = string.Empty;
            Doors.SelectedIndex = -1;
        }

        private void Close()
        {
            AppPartnumber partnumber = new();
            var parentWindow = Window.GetWindow(this) as PartnumberWindow;
            parentWindow?.Main?.Children.Clear();
            parentWindow?.Main.Children.Add(partnumber);
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void NumberValidationTextBox(object sender, TextCompositionEventArgs e)
        {
            Regex regex = new("[^0-9]+");
            e.Handled = regex.IsMatch(e.Text);
        }

        private async void BtnSave_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                string? door =
                    Doors.SelectedValue != null ? Doors.SelectedValue.ToString() : string.Empty;

                if (
                    string.IsNullOrEmpty(TbPartnumber.Text)
                    || string.IsNullOrEmpty(Description.Text)
                    || string.IsNullOrEmpty(door)
                )
                {
                    MessageBox.Show("Preencha todos os campos antes de salvar!", "Atenção");
                    return;
                }

                bool result = false;

                if (context == "update")
                    result = await db.UpdateAssociation(TbPartnumber.Text, Description.Text, door);
                else
                    result = await db.InsertPartNumber(TbPartnumber.Text, Description.Text, door);

                if (!result)
                    return;

                MessageBox.Show(
                    $"Desenho {TbPartnumber.Text} {(context == "create" ? "cadastrado" : "atualizado")} com sucesso.",
                    "Sucesso"
                );

                _createPartNumber.UpdatePartnumberList();
                SetInitialState();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inexperado. " + ex.Message, "Erro");
            }
        }
    }
}