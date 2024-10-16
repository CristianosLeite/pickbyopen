using Pickbyopen.Database;
using System.Collections.ObjectModel;
using System.Windows;
using System.Windows.Controls;

namespace Pickbyopen.Components
{
    public partial class AssociatePartnumber : UserControl
    {
        private readonly Db db;

        public string SelectedDoor { get; set; }
        public string SelectedPartnumber { get; set; }
        private int Index;

        public List<int> _doors = [];
        public ObservableCollection<string> AvailablePartnumbers = [];
        public ObservableCollection<string> AssociatedPartnumber = [];

        public AssociatePartnumber()
        {
            InitializeComponent();
            DataContext = this;

            DbConnectionFactory connectionFactory = new();
            db = new(connectionFactory);

            SelectedDoor = "";
            SelectedPartnumber = "";
            _doors = Enumerable.Range(1, 18).ToList();
            Doors.ItemsSource = _doors;
        }

        public async void LoadAvailablePartnumbers()
        {
            AvailablePartnumbers = await db.LoadAvailablePartnumbers();
            lbAvailablePartnumbers.ItemsSource ??= AvailablePartnumbers;
        }

        public async void LoadAssociatedPartnumbers()
        {
            AssociatedPartnumber = await db.LoadAssociatedPartnumbers(SelectedDoor);
            lbAssociatedPartnumbers.ItemsSource ??= AssociatedPartnumber;
        }

        public void SelectedItemChanged(object sender, RoutedEventArgs e)
        {
            Index = lbAvailablePartnumbers.SelectedIndex;
        }

        private void SelectionChanged(object sender, RoutedEventArgs e)
        {
            SelectedDoor = (string)Doors.SelectedItem;

            lbAvailablePartnumbers.ClearValue(ItemsControl.ItemsSourceProperty);
            lbAssociatedPartnumbers.ClearValue(ItemsControl.ItemsSourceProperty);
            LoadAvailablePartnumbers();
            LoadAssociatedPartnumbers();
        }

        private async void AssociateBtnClick(object sender, RoutedEventArgs e)
        {
            if (lbAvailablePartnumbers.SelectedIndex == -1)
            {
                MessageBox.Show(
                    "Selecione um partnumber para associar",
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
                return;
            }

            try
            {
                await db.CreateAssociation(AvailablePartnumbers[Index], SelectedDoor);

                SelectionChanged(sender, e);

                Index = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao associar partnumber: " + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }

        private async void RemoveAssociation(object sender, RoutedEventArgs e)
        {
            try
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

                await db.DeletePartnumberIndex(
                    AssociatedPartnumber[lbAssociatedPartnumbers.SelectedIndex]
                );

                SelectionChanged(sender, e);

                Index = -1;
            }
            catch (Exception ex)
            {
                MessageBox.Show(
                    "Erro ao desassociar partnumber: " + ex.Message,
                    "Erro",
                    MessageBoxButton.OK,
                    MessageBoxImage.Error
                );
            }
        }
    }
}
