using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace MFC.CommCtrl
{
    /// <summary>
    /// UsCtrl_MenuItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_MenuItem : UserControl
    {
        MainWindow _context;
        ItemMenu _itemMenu;
        public UsCtrl_MenuItem(ItemMenu itemMenu, MainWindow context)
        {
            InitializeComponent();
            _Initialize(itemMenu, context);            
        }

        private void _Initialize(ItemMenu itemMenu, MainWindow context)
        {
            _context = context;
            _itemMenu = itemMenu;
            ExpanderMenu.Visibility = itemMenu.SubItems == null ? Visibility.Collapsed : Visibility.Visible;
            ListViewItemMenu.Visibility = itemMenu.SubItems == null ? Visibility.Visible : Visibility.Collapsed;

            this.DataContext = itemMenu;
            MenuBar_ENB(false);
        }
        
        private void ListVeiewMenu_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            // var item = ((SubItem)((ListView)sender).SelectedItem).Screen;
            // _context.Pnl_Sel(item);
        }

        private void ListVeiewMenu_MouseLeftButtonUp(object sender, MouseButtonEventArgs e)
        {
            var sel = (((SubItem)((ListView)sender).SelectedItem)).Name;
            var item = ((SubItem)((ListView)sender).SelectedItem).Screen;
            _context.Pnl_Sel(sel, item);
        }

        private void ListVeiewMenu_TouchUp(object sender, TouchEventArgs e)
        {
            var sel = (((SubItem)((ListView)sender).SelectedItem)).Name;
            var item = ((SubItem)((ListView)sender).SelectedItem).Screen;
            _context.Pnl_Sel(sel, item);
        }

        public void MenuBar_ENB(bool btn)
        {
             if (btn)
             {
                ExpanderMenu.Visibility = Visibility.Visible;
                ListVeiewMenu.Visibility = Visibility.Visible;
                ListVeiewMenu.ItemsSource = _itemMenu.SubItems;
             }
             else
             {
                ExpanderMenu.Visibility = Visibility.Collapsed;
                ExpanderMenu.IsExpanded = false;
                ListVeiewMenu.Visibility = Visibility.Collapsed;                
             }
        }

    }
}
