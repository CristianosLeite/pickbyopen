using Pickbyopen.Services;
using System.Windows;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    /// <summary>
    /// Interação lógica para InfoUser.xam
    /// </summary>
    public partial class InfoUser : UserControl
    {
        public InfoUser()
        {
            InitializeComponent();

            Name.Text = Auth.LoggedInUser?.Username;
            BadgeNumber.Text = Auth.LoggedInUser?.BadgeNumber;
            TimeLogin.Text = Auth.LoggedAt;
        }

        private void Logout_Click(object sender, RoutedEventArgs e)
        {
            Close();
            Auth.Logout();
        }

        private void Close()
        {
            Window.GetWindow(this).Close();
        }
    }
}
