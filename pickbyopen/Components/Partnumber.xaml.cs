using Pickbyopen.Database;
using Pickbyopen.Models;
using Pickbyopen.Windows;
using System.Collections.ObjectModel;
using System.Collections.Specialized;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;

namespace Pickbyopen.Components
{
    public partial class AppPartnumber : UserControl
    {
        private readonly Db db = new(DatabaseConfig.ConnectionString!);
        public ObservableCollection<Partnumber> _partnumberList = [];
        public int Index { get; set; }

        public AppPartnumber()
        {
            InitializeComponent();
            LoadPartnumberList();

            _partnumberList.CollectionChanged += (sender, e) =>
            {
                if (e.Action == NotifyCollectionChangedAction.Add)
                    dgPartnumber.Items.Refresh();
            };
        }

        private void LoadPartnumberList()
        {
            _partnumberList.Clear();
            _partnumberList = db.LoadPartnumberList();
            dgPartnumber.ItemsSource ??= _partnumberList;

            DataContext = this;
        }

        public void UpdatePartnumberList()
        {
            try
            {
                _partnumberList.Clear();
                _partnumberList = db.LoadPartnumberList();

                dgPartnumber.ItemsSource = _partnumberList;
                dgPartnumber.Items.Refresh();
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro inexperado ao atulizar lista de partnumbers. " + ex.Message,
                    "Atenção"
                );
            }
        }

        private void GetItem()
        {
            try
            {
                var filter = tbFilter.Text;

                IEnumerable<Partnumber> item = _partnumberList.Where(p => p.Code == filter);

                if (item.Any())
                {
                    dgPartnumber.ItemsSource = item;
                }
                else
                {
                    MessageBox.Show("Desenho não encontrado.", "Atenção");
                }
            }
            catch (Exception)
            {
                MessageBox.Show("Erro inexperado ao buscar desenho.", "Erro");
            }
        }

        private void OnTbFilterChange(object sender, RoutedEventArgs e)
        {
            if (tbFilter.Text.Length > 0)
            {
                BtnFindPartnumber.Foreground = Brushes.LawnGreen;
            }
            else
            {
                BtnFindPartnumber.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.Foreground = Brushes.DarkGray;
                BtnRemoveFilter.IsEnabled = false;
            }
        }

        private void ChangeSelection(object sender, RoutedEventArgs e)
        {
            Button button = (Button)sender;
            Partnumber partnumber = (Partnumber)button.DataContext;

            Index = _partnumberList.IndexOf(partnumber);
        }

        private void DeletePartnumber(object sender, RoutedEventArgs e)
        {
            ChangeSelection(sender, e);
            MessageBoxResult result = MessageBox.Show(
                "Deseja realmente excluir este partnumber?",
                "Excluir partnumber",
                MessageBoxButton.YesNo
            );
            if (result == MessageBoxResult.Yes)
            {
                bool isDeleted = db.DeletePartnumber(_partnumberList[Index].Code);
                if (!isDeleted)
                    return;

                _partnumberList.RemoveAt(Index);

                MessageBox.Show("Partnumber excluído com sucesso!");
            }

            dgPartnumber.Items.Refresh();
        }

        private async void EditPartnumber(object sender, RoutedEventArgs e)
        {
            try
            {
                var pnindex = (Button)e.OriginalSource;

                if (pnindex.DataContext is Partnumber parnumberObj)
                {
                    var door = await db.GetAssociatedDoor(parnumberObj.Code);

                    EditPartnumber editPartnumber = new(this, parnumberObj, door, "update");
                    var parentWindow = Window.GetWindow(this) as PartnumberWindow;
                    parentWindow?.Main?.Children.Clear();
                    parentWindow?.Main.Children.Add(editPartnumber);
                }
            }
            catch (Exception ex)
            {
                MessageBox.Show("" + ex, Name, MessageBoxButton.OK);
            }
        }

        private void SearchPartnumber(object sender, RoutedEventArgs e)
        {
            if (tbFilter.Text.Length == 0)
                return;

            BtnRemoveFilter.IsEnabled = true;
            BtnRemoveFilter.Foreground = Brushes.Red;

            GetItem();
        }

        private void ClearFilter(object sender, RoutedEventArgs e)
        {
            tbFilter.Clear();
            UpdatePartnumberList();
        }

        private void CreatePartnumber(object sender, RoutedEventArgs e)
        {
            EditPartnumber editPartnumber = new(this, new Partnumber("", ""), 0, "create");
            var parentWindow = Window.GetWindow(this) as PartnumberWindow;
            parentWindow?.Main?.Children.Clear();
            parentWindow?.Main.Children.Add(editPartnumber);
        }
    }
}
