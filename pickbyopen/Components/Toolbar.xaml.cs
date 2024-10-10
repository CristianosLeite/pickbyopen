using Pickbyopen.Services;
using Pickbyopen.Windows;
using System.Windows;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para Toolbar.xaml
    /// </summary>
    public partial class Toolbar : UserControl
    {
        public Toolbar()
        {
            InitializeComponent();
        }

        private void Partnumbers_Click(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("P"))
            {
                MessageBox.Show(
                    "Você não tem permissão para acessar essa funcionalidade.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            PartnumberWindow partnumbers = new();
            partnumbers.Show();
        }

        private void Users_Click(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("U"))
            {
                MessageBox.Show(
                    "Você não tem permissão para acessar essa funcionalidade.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            UserWindow users = new();
            users.Show();
        }

        private void Associations_Click(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("P"))
            {
                MessageBox.Show(
                    "Você não tem permissão para acessar essa funcionalidade.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            AssociationWindow associations = new();
            associations.Show();
        }

        private void Reports_Click(object sender, RoutedEventArgs e)
        {
            if (!Auth.UserHasPermission("R"))
            {
                MessageBox.Show(
                    "Você não tem permissão para acessar essa funcionalidade.",
                    "Acesso negado",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }
            ReportWindow reports = new();
            reports.Show();
        }
    }
}
