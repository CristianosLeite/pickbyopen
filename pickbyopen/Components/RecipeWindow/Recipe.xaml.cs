using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;


namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para AppRecipe.xam
    /// </summary>
    public partial class AppRecipe : UserControl
    {
        private readonly Db db;
        public ObservableCollection<Recipe> _recipeList = [];
        public int Index { get; set; }

        public AppRecipe()
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            LoadRecipeList();

            _recipeList.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    dgRecipe.Items.Refresh();
            };
        }

        private void LoadRecipeList()
        {
            _recipeList.Clear();
            _recipeList = db.LoadRecipeList();
            dgRecipe.ItemsSource ??= _recipeList;

            DataContext = this;
        }

        private void CreateRecipe(object sender, RoutedEventArgs e)
        {
            EditRecipe editRecipe = new(this, new Recipe(null, "", ""), Context.Create);
            var parentWindow = Window.GetWindow(this) as RecipeWindow;
            parentWindow?.Main?.Children.Clear();
            parentWindow?.Main.Children.Add(editRecipe);
        }

        private void OnTbFilterChange(object sender, RoutedEventArgs e)
        {
            if (tbFilter.Text.Length > 0)
            {
                BtnFindRecipe.Foreground = Brushes.LawnGreen;
            }
            else
            {
                BtnFindRecipe.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.IsEnabled = false;
            }
        }

        private void SearchRecipe(object sender, RoutedEventArgs e)
        {
            if (tbFilter.Text.Length == 0)
                return;

            BtnRemoveFilter.IsEnabled = true;
            BtnRemoveFilter.Foreground = Brushes.Red;

            GetItem();
        }

        private void GetItem()
        {
            try
            {
                var filter = tbFilter.Text;

                IEnumerable<Recipe> item = _recipeList.Where(p => p.VP == filter);

                if (item.Any())
                {
                    dgRecipe.ItemsSource = item;
                }
                else
                {
                    MessageBox.Show("VP não encontrado.", "Atenção");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Erro inexperado ao buscar receita.", "Erro");
            }
        }

        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            tbFilter.Clear();
            UpdateRecipeList();
        }

        public void UpdateRecipeList()
        {
            try
            {
                _recipeList.Clear();
                _recipeList = db.LoadRecipeList();

                dgRecipe.ItemsSource = _recipeList;
                dgRecipe.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro inexperado ao atulizar lista de partnumbers. " + ex.Message,
                    "Atenção"
                );
            }
        }

        private async void EditRecipe(object sender, RoutedEventArgs e)
        {
            try
            {
                var rcpindex = (Button)e.OriginalSource;

                if (rcpindex.DataContext is Recipe recipeObj)
                {
                    var door = await db.GetAssociatedDoor(recipeObj.VP);

                    EditRecipe editRecipe = new(this, recipeObj, Context.Update);
                    var parentWindow = Window.GetWindow(this) as RecipeWindow;
                    parentWindow?.Main?.Children.Clear();
                    parentWindow?.Main.Children.Add(editRecipe);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex, Name, MessageBoxButton.OK);
            }
        }

        private async void DeleteRecipe(object sender, RoutedEventArgs e)
        {
            ChangeSelection(sender, e);
            MessageBoxResult result = MessageBox.Show(
                "Deseja realmente excluir esta receita?",
                "Excluir receita",
                MessageBoxButton.YesNo
            );
            if (result == MessageBoxResult.Yes)
            {
                bool isDeleted = await db.DeleteRecipe(_recipeList[Index].VP);
                if (!isDeleted)
                    return;

                _recipeList.RemoveAt(Index);

                MessageBox.Show("Partnumber excluído com sucesso!");
            }

            dgRecipe.Items.Refresh();
        }

        private void ChangeSelection(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Recipe recipe = (Recipe)button.DataContext;

            Index = _recipeList.IndexOf(recipe);
        }
    }
}
