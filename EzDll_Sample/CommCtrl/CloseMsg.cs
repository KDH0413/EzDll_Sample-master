using MFC.UI;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;

namespace MFC.CommCtrl
{
    public class CloseMsg
    {
        public static void CloseAllMsgBox()
        {
            foreach (frm_Msg window in Application.Current.Windows.OfType<frm_Msg>())            
                window.Close ();
                while (Application.Current.Windows.OfType<frm_Msg>().Count() > 0 ) { }            
        }

        public static void CloseAllShortNoti()
        {
            foreach (frm_Noti window in Application.Current.Windows.OfType<frm_Noti>())            
                window.FastClose();
                while (Application.Current.Windows.OfType<frm_Noti>().Count() > 0) { }
            
        }

        public static void CloseAllMessage()
        {
            CloseAllShortNoti();
            CloseAllMsgBox();
        }
    }
}
