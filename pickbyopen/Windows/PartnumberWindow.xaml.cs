using Pickbyopen.Components;
using System.Windows;

namespace Pickbyopen.Windows
{
    /// <summary>
    /// Lógica interna para PartnumberWindow.xaml
    /// </summary>
    public partial class PartnumberWindow : Window
    {
        public PartnumberWindow()
        {
            InitializeComponent();
            Main.Children.Add(new AppPartnumber());
        }
    }
}
