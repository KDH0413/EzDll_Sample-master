using MaterialDesignThemes.Wpf;
using EzDll_Sample.CommCtrl;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static EzDll_Sample.Global;

namespace EzDll_Sample.CommCtrl
{
    public class ItemMenu
    {
        public ItemMenu(ePAGE header, List<SubItem> subitems, PackIconKind icon)
        {
            Header = header;
            SubItems = subitems;            
            Icon = icon;
        }
        
        public ePAGE Header { get; private set; }
        public PackIconKind Icon { get; private set; }
        public List<SubItem> SubItems { get; private set; }
        public UserControl Screen { get; private set; }
    }
}
