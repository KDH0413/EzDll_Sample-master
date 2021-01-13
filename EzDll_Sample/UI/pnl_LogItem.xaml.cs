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
using static EzDll_Sample.Global;

namespace EzDll_Sample.UI
{
    /// <summary>
    /// pnl_LogItem.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class pnl_LogItem : UserControl
    {
        CmdLogType type;
        public CmdLogType _Type => type;
        public pnl_LogItem(CmdLogType Type)
        {
            InitializeComponent();
            type = Type;
        }

        private const int maxLine = 100;
        public void AddMsg(string msg)
        {
            txt_msg.AppendText(msg);
            if (maxLine < txt_msg.LineCount)
            {
                txt_msg.SelectionStart = 0;
                txt_msg.SelectionLength = txt_msg.GetFirstVisibleLineIndex();
                txt_msg.SelectedText = "";
            }
            txt_msg.SelectionStart = txt_msg.Text.Length;
            txt_msg.ScrollToLine(txt_msg.LineCount - 1);
        }
    }
}
