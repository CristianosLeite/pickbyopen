using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Utils;
using Pickbyopen.Windows;
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
        private readonly Db db;
        private readonly List<int> _doors;
        private readonly AppPartnumber _createPartNumber;
        private readonly Context context;
        public readonly int _selectedDoor;

        public EditPartnumber(
            AppPartnumber createPartnumber,
            Partnumber partnumber,
            int door,
            Context context
        )
        {
            InitializeComponent();
            DataContext = this;
            this.context = context;

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            if (context == Context.Update)
            {
                Title.Content = "Editar Partnumber";
            }
            else
            {
                Title.Content = "Cadastrar Partnumber";
            }

            _createPartNumber = createPartnumber;
            _selectedDoor = door;

            _doors = Enumerable.Range(1, 9).ToList();
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
            if (ValidateNumberInput.IsValideInput(e.Text))
                e.Handled = false;
            else
                e.Handled = true;
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

                bool result = await db.SavePartnumber(TbPartnumber.Text, Description.Text, door);

                if (!result)
                    return;

                MessageBox.Show(
                    $"Desenho {TbPartnumber.Text} {(context == Context.Create ? "cadastrado" : "atualizado")} com sucesso.",
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
