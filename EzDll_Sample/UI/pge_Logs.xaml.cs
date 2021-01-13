using System;
using System.Collections.Concurrent;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading;
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
    /// pge_Logs.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class pge_Logs : UserControl
    {
        private Thread m_thMsgAdder;
        public class LogMsg
        {
            public CmdLogType logUsr;
            public string msg;
        }
        private ConcurrentQueue<LogMsg> m_quLogMsg = new ConcurrentQueue<LogMsg>();
        Dictionary<CmdLogType, pnl_LogItem> _DicLogItems = new Dictionary<CmdLogType, pnl_LogItem>();

        public pge_Logs()
        {
            InitializeComponent();
            _Initialize();
        }

        public void _Initialize()
        {
            var txt_Vec= new pnl_LogItem(CmdLogType.Vehicle);
            var txt_Pwr = new pnl_LogItem(CmdLogType.Power);

            _DicLogItems[CmdLogType.Vehicle] = txt_Vec;
            _DicLogItems[CmdLogType.Power] = txt_Pwr;
            Sel_LogTxt(CmdLogType.Vehicle);
            m_thMsgAdder = new Thread(msgAddProcess)
            {
                IsBackground = true
            };
            m_thMsgAdder.Start();
        }

        public void Sel_LogTxt(CmdLogType sel)
        {
            switch (sel)
            {
                case CmdLogType.Vehicle:
                case CmdLogType.Power:
                    {
                        pnl_LogItem.Children.Clear();
                        var selLog = _DicLogItems[sel];
                        pnl_LogItem.Children.Add(selLog);
                        break;
                    }
                default: break;
            }
        }

        private void Lst_Logs_SelectionChanged(object sender, SelectionChangedEventArgs e)
        {
            if (null == pnl_LogItem) return;
            var sel = (CmdLogType)(sender as ListBox).SelectedIndex;
            Sel_LogTxt(sel);
        }

        public async Task StopLog()
        {
            isThRun = false;
            while (isThRun && m_quLogMsg.Count == 0)
            {
                await Task.Delay(1);
            }
        }

        public void AddQueue(LogMsg msg)
        {
            m_quLogMsg.Enqueue(msg);
        }

        private bool isThRun = true;
        private void msgAddProcess()
        {
            while (isThRun)
            {
                if (m_quLogMsg.Count > 0)
                {
                    LogMsg temp = new LogMsg();
                    if (!m_quLogMsg.TryDequeue(out temp))
                    {
                        Thread.Sleep(1);
                        continue;
                    }
                    if (temp == null) { return; }

                    if (temp.msg.IndexOf("\r") < 0)
                    {
                        temp.msg += "\r";
                    }
                    if (temp.msg.IndexOf("\n") < 0)
                    {
                        temp.msg += "\n";
                    }

                    try
                    {
                        Dispatcher.Invoke(new Action(delegate () // this == Form 이다. Form이 아닌 컨트롤의 Invoke를 직접호출해도 무방하다.
                        {
                            TextBox target = new TextBox();
                            var selLog = _DicLogItems[temp.logUsr];
                            selLog.AddMsg(temp.msg);
                        }));
                    }
                    catch (Exception e)
                    {
                        Debug.Write(false, $"{e.ToString()}\r\n");
                    }
                }
                Thread.Sleep(5);
            }
            isThRun = false;
        }

    }
}
