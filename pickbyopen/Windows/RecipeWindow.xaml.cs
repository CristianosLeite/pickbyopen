using Pickbyopen.Components;
using System.Windows;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para RecipeWindow.xaml
    /// </summary>
    public partial class RecipeWindow : Window
    {
        public RecipeWindow()
        {
            InitializeComponent();
            Topmost = true;
            Main.Children.Add(new AppRecipe());
        }
    }
}
