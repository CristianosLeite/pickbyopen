using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Windows;
using System.Collections.ObjectModel;
using System.Reactive.Linq;
using System.Text.RegularExpressions;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Input;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para EditRecipe.xam
    /// </summary>
    public partial class EditRecipe : UserControl
    {
        private readonly Db db;
        private readonly AppRecipe _createRecipe;
        private readonly Context context;
        public ObservableCollection<Partnumber> Partnumbers = [];
        private readonly List<Partnumber> AssociatedPartnumbers = [];
        private List<Partnumber> FilteredPartnumbers = [];
        private readonly long? RecipeId = null;
        private int Index;

        public EditRecipe(AppRecipe createRecipe, Recipe recipe, Context context)
        {
            InitializeComponent();
            DataContext = this;
            this.context = context;

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            if (context == Context.Update)
            {
                Title.Content = "Editar receita";
            }
            else
            {
                Title.Content = "Cadastrar nova receita";
            }

            _createRecipe = createRecipe;

            RecipeId = recipe.RecipeId;
            TbVP.Text = recipe.VP;
            Description.Text = recipe.Description;
            AssociatedPartnumbers = recipe.Partnumbers;

            var describeAssociatedPartnumbers = AssociatedPartnumbers.Select(p =>
                p.Code + " - " + p.Description
            );
            lbAssociatedPartnumbers.ItemsSource ??= describeAssociatedPartnumbers;

            LoadPartnumbers();
        }

        public void LoadPartnumbers()
        {
            Partnumbers = db.LoadPartnumberList();

            // Filtra os partnumbers que não estão em AssociatedPartnumbers
            FilteredPartnumbers = Partnumbers
                .Where(p => !AssociatedPartnumbers.Any(ap => ap.Code == p.Code))
                .ToList();

            var describePartnumbers = FilteredPartnumbers.Select(p =>
                p.Code + " - " + p.Description
            );
            lbPartnumbers.ItemsSource ??= describePartnumbers;
        }

        public void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            Index = lbPartnumbers.SelectedIndex;
        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            var describeAssociatedPartnumbers = AssociatedPartnumbers.Select(p =>
                p.Code + " - " + p.Description
            );
            lbAssociatedPartnumbers.ClearValue(ItemsControl.ItemsSourceProperty);
            lbAssociatedPartnumbers.ItemsSource ??= describeAssociatedPartnumbers;

            lbPartnumbers.ClearValue(ItemsControl.ItemsSourceProperty);
            LoadPartnumbers();
        }

        private void AssociateBtnClick(object sender, RoutedEventArgs e)
        {
            if (lbPartnumbers.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Selecione um partnumber para associar",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            if (Index < 0 || Index >= Partnumbers.Count)
            {
                MessageBox.Show(
                    "Índice fora do intervalo",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            AssociatedPartnumbers.Add(FilteredPartnumbers[Index]);
            Index = -1;

            SelectionChanged(sender, e);
        }

        private void RemoveAssociation(object sender, RoutedEventArgs e)
        {
            if (lbAssociatedPartnumbers.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Selecione um partnumber para desassociar",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            AssociatedPartnumbers.RemoveAt(lbAssociatedPartnumbers.SelectedIndex);

            SelectionChanged(sender, e);

            Index = -1;
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
                if (string.IsNullOrEmpty(TbVP.Text) || string.IsNullOrEmpty(Description.Text))
                {
                    MessageBox.Show("Preencha todos os campos antes de salvar!", "Atenção");
                    return;
                }

                if (TbVP.Text.Length != 14)
                {
                    MessageBox.Show("O VP deve conter 14 caracteres.", "Atenção");
                    return;
                }

                if (AssociatedPartnumbers.Count == 0)
                {
                    MessageBox.Show("Adicione ao menos um partnumber à receita.", "Atenção");
                    return;
                }

                bool result = await db.SaveRecipe(
                    new(RecipeId, TbVP.Text, Description.Text),
                    AssociatedPartnumbers,
                    context
                );

                if (!result)
                    return;

                MessageBox.Show(
                    $"Receita {(context == Context.Create ? "cadastrada" : "atualizada")} com sucesso.",
                    "Sucesso"
                );

                _createRecipe.UpdateRecipeList();
                SetInitialState();
                Close();
            }
            catch (Exception ex)
            {
                MessageBox.Show("Erro inexperado. " + ex.Message, "Erro");
            }
        }

        private void CloseButton_Click(object sender, RoutedEventArgs e)
        {
            Close();
        }

        private void SetInitialState()
        {
            TbVP.Text = string.Empty;
            Description.Text = string.Empty;
            lbAssociatedPartnumbers.ClearValue(ItemsControl.ItemsSourceProperty);
        }

        private void Close()
        {
            AppRecipe recipe = new();
            var parentWindow = Window.GetWindow(this) as RecipeWindow;
            parentWindow?.Main?.Children.Clear();
            parentWindow?.Main.Children.Add(recipe);
        }
    }
}
