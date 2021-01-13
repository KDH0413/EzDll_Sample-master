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
using static EzPower.Global_Pwr;

namespace EzDll_Sample.UI
{
    /// <summary>
    /// Pge_Moni_Pwr.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class Pge_Moni_Pwr : UserControl
    {
        MainCtrl _ctrl;
        DispatcherTimer _tmrUpdate;
        public Pge_Moni_Pwr(eVIWER name, MainCtrl ctrl)
        {
            InitializeComponent();
            _Initialize(name, ctrl);
        }

        public void _Initialize(eVIWER name, MainCtrl ctrl)
        {
            _ctrl = ctrl;
            this.Name = name.ToString();
            DataContext = this;

            _tmrUpdate = new DispatcherTimer();
            _tmrUpdate.Interval = TimeSpan.FromMilliseconds(0.1);    //시간간격 설정
            _tmrUpdate.Tick += new EventHandler(Tmr_Tick);           //이벤트 추가         
            SetConnection(false);

            cmb_Dev.Items.Clear();
            foreach (ePWRTYPE item in Enum.GetValues(typeof(ePWRTYPE)))
            {
                cmb_Dev.Items.Add(item.ToString());
            }
            cmb_Dev.SelectedIndex = 0;

            cmb_Port.Items.Clear();
            foreach (ePWR_PortName item in Enum.GetValues(typeof(ePWR_PortName)))
            {
                cmb_Port.Items.Add(item.ToString());
            }
            cmb_Port.SelectedIndex = 0;
        }

        public void DoingExchange(eDATAEXCHANGE dir)
        {
            if (false == _ctrl.bLoaded && dir != eDATAEXCHANGE.Load) return;
            switch (dir)
            {
                case eDATAEXCHANGE.Data2UI:
                    break;
                case eDATAEXCHANGE.UI2Data:
                    break;
                case eDATAEXCHANGE.Load:
                    DoingExchange(eDATAEXCHANGE.Data2UI);
                    break;
                case eDATAEXCHANGE.Save:
                    break;
                case eDATAEXCHANGE.StatusUpdate:
                    if (null != status)
                    {
                        txt_Bat.Text = status.soc.ToString();
                        txt_Curr.Text = status.curr.ToString("F1");
                        txt_Volt.Text = status.volt.ToString("F1");
                        txt_temp_1st.Text = status.temp_1st.ToString();
                        txt_temp_2nd.Text = status.temp_2nd.ToString();
                        txt_temp_3th.Text = status.temp_3th.ToString();
                        txt_State.Text = status.state.ToString();
                        txt_ErrBit.Text = status.errbit.ToString();
                    }
                    else
                    {
                        txt_Bat.Text = "NONE";
                        txt_Curr.Text = txt_Volt.Text = "---";
                        txt_temp_1st.Text = txt_temp_2nd.Text = txt_temp_3th.Text = "---";
                        txt_State.Text = ePWR_CHR_STATE.None.ToString();
                        txt_ErrBit.Text = "---";
                    }
                    break;
            }
        }

        private void Tmr_Tick(object sender, EventArgs e)
        {
            DoingExchange(eDATAEXCHANGE.StatusUpdate);
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

        public void SetConnection(bool conected)
        {
            txt_Connect.Text = !conected ? "Disconnected" : "Connected";
        }

        PWRSTATUS status = null;
        public void SetStatus(string rcvMsg)
        {
            JCVT_PWR.Get(rcvMsg, out status);
        }

        private void Btn_Open_Click(object sender, RoutedEventArgs e)
        {
            var dev = cmb_Dev.Text.ToString().ToEnum<ePWRTYPE>();
            var port = cmb_Port.Text.ToString().ToEnum<ePWR_PortName>();
            _ctrl.Pwr_Open(dev, port);
        }

        private void Btn_Close_Click(object sender, RoutedEventArgs e)
        {
            _ctrl.Pwr_Close();
        }
    }
}
