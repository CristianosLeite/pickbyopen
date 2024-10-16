using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Types;
using Pickbyopen.Windows;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pickbyopen.Components
{
    public partial class AppUser : UserControl
    {
        public int Index { get; set; }
        private readonly Db db;
        public ObservableCollection<User> _usersList = [];
        public User User { get; set; }

        public AppUser()
        {
            InitializeComponent();

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            LoadUserList();

            User = new User("", "", "", []);
        }

        private void LoadUserList()
        {
            _usersList.Clear();
            _usersList = db.LoadUsersList();
            dgUser.ItemsSource ??= _usersList;

            DataContext = this;
        }

        private void CreateUser(object sender, RoutedEventArgs e)
        {
            EditUser editUser = new(User, Context.Create);
            var parentWindow = Window.GetWindow(this) as UserWindow;
            parentWindow?.Main?.Children.Clear();
            parentWindow?.Main?.Children.Add(editUser);
        }


        private void OnTbFilterChange(object sender, TextChangedEventArgs e)
        {
            if (tbFilter.Text.Length > 0)
            {
                BtnFindUser.Foreground = Brushes.LawnGreen;
            }
            else
            {
                BtnFindUser.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.IsEnabled = false;
            }
        }

        private void GetUser()
        {
            try
            {
                var filter = tbFilter.Text;

                IEnumerable<User> item = _usersList.Where(p => p.Username == filter);

                if (item.Any())
                {
                    dgUser.ItemsSource = item;
                }
                else
                {
                    MessageBox.Show("Usuário não encontrado.", "Atenção");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Erro inexperado ao buscar usuário.", "Erro");
            }
        }

        private void SearchUser(object sender, RoutedEventArgs e)
        {
            if (tbFilter.Text.Length == 0)
                return;

            BtnRemoveFilter.IsEnabled = true;
            BtnRemoveFilter.Foreground = Brushes.Red;

            GetUser();
        }

        private void UpdateUsersList()
        {
            try
            {
                _usersList.Clear();
                _usersList = db.LoadUsersList();

                dgUser.ItemsSource = _usersList;
                dgUser.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro inexperado ao atulizar lista de usuários. " + ex.Message,
                    "Atenção"
                );
            }
        }

        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            tbFilter.Clear();
            UpdateUsersList();
        }

        private void EditUser(object sender, RoutedEventArgs e)
        {
            try
            {
                var pnindex = (Button)e.OriginalSource;

                if (pnindex.DataContext is User user)
                {
                    EditUser editUser = new(user, Context.Update);
                    var parentWindow = Window.GetWindow(this) as UserWindow;
                    parentWindow?.Main?.Children.Clear();
                    parentWindow?.Main.Children.Add(editUser);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex, Name, MessageBoxButton.OK);
            }
        }

        private void ChangeSelection(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            User user = (User)button.DataContext;

            Index = _usersList.IndexOf(user);
        }

        private async void DeleteUser(object sender, RoutedEventArgs e)
        {
            ChangeSelection(sender, e);
            MessageBoxResult result = MessageBox.Show(
                "Deseja realmente excluir este usuário?",
                "Excluir usuário",
                MessageBoxButton.YesNo
            );
            if (result == MessageBoxResult.Yes)
            {
                try
                {
                    bool isDeleted = await db.DeleteUser(_usersList[Index]);
                    if (!isDeleted)
                        return;

                    _usersList.RemoveAt(Index);

                    MessageBox.Show("Usuário excluído com sucesso!");
                }
                catch (Exception ex)
                {
                    MessageBox.Show("Erro inexperado ao excluir usuário. " + ex.Message, "Erro");
                }
            }

            dgUser.Items.Refresh();
        }
    }
}
