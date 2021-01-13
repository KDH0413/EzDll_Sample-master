using MaterialDesignThemes.Wpf;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Controls;
using static EzDll_Sample.Global;

namespace EzDll_Sample.CommCtrl
{
    public class SubItem
    { 
        public SubItem(eVIWER name, PackIconKind icon, UserControl screen = null)
        {
            Name = name;
            Icon = icon;
            Screen = screen;
        }         
        public eVIWER Name { get; private set; }
        public PackIconKind Icon { get; private set; }
        public UserControl Screen { get; private set; }
    }
}
