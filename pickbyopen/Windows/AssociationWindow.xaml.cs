using Pickbyopen.Components;
using System.Windows;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para AssociationWindow.xaml
    /// </summary>
    public partial class AssociationWindow : Window
    {
        public AssociationWindow()
        {
            InitializeComponent();
            Main.Children.Add(new AssociatePartnumber());
        }
    }
}
