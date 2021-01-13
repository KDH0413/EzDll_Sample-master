using EzDll_Sample.UI;
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
using System.Windows.Threading;
using static EzDll_Sample.Global;

namespace EzDll_Sample
{
    /// <summary>
    /// MainWindow.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class MainWindow : Window
    {
        private Logger _log = Logger.Inst;
        private MainCtrl mainCtrl;
        public MainCtrl _MainCtrl => mainCtrl;

        eVIWER Pnl_Curr = eVIWER.None;
        public eVIWER _Pnl_Curr => Pnl_Curr;
        private Dictionary<eVIWER, UserControl> _dicPnl = new Dictionary<eVIWER, UserControl>();
        pge_Moni_Vec pgeMoni_Vec;
        Pge_Moni_Pwr pgeMoni_Pwr;
        pge_Logs pge_Log;
        DispatcherTimer _tmrUpdate;
        enum eBTN_POPUP
        {
              EzVehicle
            , EzPower
            , ShutDown
        }

        public MainWindow()
        {
            InitializeComponent();
            _Initialize();
        }

        private void _Initialize()
        {
            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(0.1);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가  

            this.Title = $"EzDll_Sample";
            Logger.Inst.MakeSrcHdl(this.Title);

            mainCtrl = new MainCtrl(this);
            mainCtrl.EvtUpdateStatus += On_UpdateStatus;

            pge_Log = new pge_Logs();
            pnl_Btm.Children.Add(pge_Log);

            pgeMoni_Vec = new pge_Moni_Vec(eVIWER.Pnl_Vec, mainCtrl);
            pgeMoni_Pwr = new Pge_Moni_Pwr(eVIWER.Pnl_Pwr, mainCtrl);
            _dicPnl[eVIWER.Pnl_Vec] = pgeMoni_Vec;
            _dicPnl[eVIWER.Pnl_Pwr] = pgeMoni_Pwr;

            DoingExchange(eDATAEXCHANGE.Load, eVIWER.None);
            mainCtrl.bLoaded = true;
        }

        private async void _Finalize()
        {
            Tmr_Work(false);
            await _MainCtrl.Dispose();
            await pge_Log.StopLog();
            Application.Current.Shutdown();
        }

        bool bTogle = false;
        TIMEARG togleTime = new TIMEARG();
        public void DoingExchange(eDATAEXCHANGE dir, eVIWER id)
        {
            switch (dir)
            {
                case eDATAEXCHANGE.Data2UI:
                    switch (id)
                    {
                        case eVIWER.None:
                            pgeMoni_Vec.DoingExchange(eDATAEXCHANGE.Data2UI);
                            pgeMoni_Pwr.DoingExchange(eDATAEXCHANGE.Data2UI);
                            break;
                        case eVIWER.Pnl_Vec: pgeMoni_Vec.DoingExchange(eDATAEXCHANGE.Data2UI); break;
                        case eVIWER.Pnl_Pwr: pgeMoni_Pwr.DoingExchange(eDATAEXCHANGE.Data2UI); break;
                    }
                    break;
                case eDATAEXCHANGE.UI2Data:
                    switch (id)
                    {
                        case eVIWER.None: break;
                        case eVIWER.Pnl_Vec: pgeMoni_Vec.DoingExchange(eDATAEXCHANGE.UI2Data); break;
                        case eVIWER.Pnl_Pwr: pgeMoni_Pwr.DoingExchange(eDATAEXCHANGE.UI2Data); break;
                    }
                    break;
                case eDATAEXCHANGE.Load:
                    Tmr_Work(true);
                    Pge_Sel(eVIWER.Pnl_Vec, pgeMoni_Vec);
                    DoingExchange(eDATAEXCHANGE.Data2UI, id);
                    break;
                case eDATAEXCHANGE.Save:
                    DoingExchange(eDATAEXCHANGE.UI2Data, id);
                    //_Data.Inst.SysSave();
                    DoingExchange(eDATAEXCHANGE.Data2UI, id);
                    break;
                case eDATAEXCHANGE.StatusUpdate:
                    if (togleTime.IsOver(1 * 1000))
                    {
                        
                        bTogle ^= true;
                    }
                    break;
            }
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            DoingExchange(eDATAEXCHANGE.StatusUpdate, eVIWER.None);
        }

        public void Tmr_Work(bool bTrg)
        {
            if (true == bTrg)
            {
                _tmrUpdate.Start();
            }
            else
            {
                _tmrUpdate.Stop();
            }
        }

        private void Tmr_Work(eVIWER id, bool bTrg = true)
        {
            pgeMoni_Vec.Tmr_Work(false);
            pgeMoni_Pwr.Tmr_Work(false);
            if (true == bTrg)
            {
                switch (id)
                {
                    case eVIWER.Pnl_Vec: pgeMoni_Vec.Tmr_Work(true); break;
                    case eVIWER.Pnl_Pwr: pgeMoni_Pwr.Tmr_Work(true); break;
                }
            }
        }


        private void Btn_Popup_Click(object sender, RoutedEventArgs e)
        {
            var btn = (sender as Button).Content.ToString().ToEnum<eBTN_POPUP>();
            switch (btn)
            {
                case eBTN_POPUP.EzVehicle: Pge_Sel(eVIWER.Pnl_Vec, pgeMoni_Vec); break;
                case eBTN_POPUP.EzPower: Pge_Sel(eVIWER.Pnl_Pwr, pgeMoni_Pwr); break;
                case eBTN_POPUP.ShutDown: _Finalize(); break;
            }
        }

        eVIWER _CurrView = eVIWER.None;
        internal void Pge_Sel(eVIWER id, object sender)
        {
            var pnl = (UserControl)sender;
            if (null == pnl) return;
            if (_CurrView == id) return;
            var userCtrl = _dicPnl[id];
            Tmr_Work(_CurrView, false);
            DoingExchange(eDATAEXCHANGE.UI2Data, Pnl_Curr);
            pnl_Main.Children.Clear();
            switch (id)
            {
                case eVIWER.None: break;
                case eVIWER.Pnl_Vec: break;
                case eVIWER.Pnl_Pwr: break;
            }
            DoingExchange(eDATAEXCHANGE.Data2UI, id);
            pnl_Main.Children.Add(userCtrl);
            _CurrView = id;
            Tmr_Work(_CurrView);
        }

        private void On_UpdateStatus(object sender, UDATESTATUS arg)
        {
            try
            {
                Dispatcher.Invoke(new Action(delegate () // this == Form 이다. Form이 아닌 컨트롤의 Invoke를 직접호출해도 무방하다.
                {
                    switch (arg.id)
                    {
                        case eVIWER.Pnl_Vec:
                            switch (arg.mthd)
                            {
                                case "On_VecConnection":
                                    {
                                        var connected = arg.msg.Contains("1") ? true : false;
                                        pgeMoni_Vec.SetConnection(connected);
                                        break;
                                    }
                                case "LS_STATUS": pgeMoni_Vec.SetStatus(arg.msg); break;
                                case "LOG": Logger.Inst.Write(CmdLogType.Vehicle, arg.msg); break;
                                default: break;
                            }
                            break;
                        case eVIWER.Pnl_Pwr:
                            switch (arg.mthd)
                            {
                                case "On_PwrConnection":
                                    {
                                        var connected = arg.msg.Contains("1") ? true : false;
                                        pgeMoni_Pwr.SetConnection(connected);
                                        break;
                                    }
                                case "PWRSTATUS": pgeMoni_Pwr.SetStatus(arg.msg); break;
                                case "LOG": Logger.Inst.Write(CmdLogType.Power, arg.msg); break;
                                default: break;
                            }
                            break;
                        default: break;
                    }
                }));
            }
            catch (Exception e)
            {
                switch (arg.id)
                {
                    case eVIWER.Pnl_Vec:
                        Logger.Inst.Write(CmdLogType.Vehicle, $"{arg.id.ToString()} : {e.ToString()}\r\n");
                        break;
                    case eVIWER.Pnl_Pwr:
                        Logger.Inst.Write(CmdLogType.Power, $"{arg.id.ToString()} : {e.ToString()}\r\n");
                        break;
                    default:  break;
                }
            }
        }
    }
}
