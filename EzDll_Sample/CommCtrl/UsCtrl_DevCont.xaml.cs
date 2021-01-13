using MaterialDesignThemes.Wpf;
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
using static MFC.Global._Enum;

namespace MFC.UI
{
    /// <summary>
    /// UsCtrl_DevCont.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class UsCtrl_DevCont : UserControl
    {
        eDEV _dev;
        string _name;
        public eDEV _Dev => _dev;
        public string _Name => _name;
        public UsCtrl_DevCont(eDEV dev, string name)
        {
            InitializeComponent();
            _Initialize(dev, name);
        }

        private void _Initialize(eDEV dev, string name)
        {
            _dev = dev;
            _name = name;
            txt_Dev.Text = name;
            SetConnection(false);
        }

        public void SetConnection (bool connection)
        {            
            Icon_Dev.Kind = (true == connection) ? PackIconKind.CastConnected : PackIconKind.CastOff;
            Icon_Dev.Foreground = (true == connection) ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.SlateGray);
            txt_Dev.Foreground = (true == connection) ? new SolidColorBrush(Colors.WhiteSmoke) : new SolidColorBrush(Colors.SlateGray);
        }
    }
}
