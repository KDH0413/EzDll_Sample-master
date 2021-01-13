using EzVehicle.Ctrl;
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
using static EzVehicle.Global_Vec;

namespace EzDll_Sample.UI
{
    /// <summary>
    /// pge_Moni_Vec.xaml에 대한 상호 작용 논리
    /// </summary>
    public partial class pge_Moni_Vec : UserControl
    {
        MainCtrl _ctrl;
        DispatcherTimer _tmrUpdate;
        public pge_Moni_Vec(eVIWER name, MainCtrl ctrl)
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
                    if ( null != status)
                    {
                        txt_State.Text = status.state.st.ToString();
                        txt_SubMsg.Text = status.state.subMsg;
                        txt_Dest.Text = status.dest;
                        txt_PosX.Text = status.pos.x.ToString();
                        txt_PosY.Text = status.pos.y.ToString();
                        txt_PosAng.Text = status.pos.ang.ToString();
                        txt_Loc.Text = status.local.ToString();
                        txt_Temp.Text = status.temp.ToString();
                        txt_Bat.Text = status.batt.ToString();
                    }
                    else
                    {
                        txt_State.Text = "NONE";
                        txt_SubMsg.Text = "------";
                        txt_Dest.Text = "NONE";
                        txt_PosX.Text = txt_PosY.Text = txt_PosAng.Text = "---";
                        txt_Loc.Text = txt_Temp.Text = txt_Bat.Text = "---";
                    }
                    break;
            }
        }

        LD_STATUS status = null;
        private void Tmr_Tick(object sender, EventArgs e)
        {
            DoingExchange(eDATAEXCHANGE.StatusUpdate);
        }

        public void SetStatus(string rcvMsg)
        {
            JCVT_VEC.Get(rcvMsg, out status);
        }

        public void SetConnection(bool conected)
        {
            txt_Connect.Text = !conected ? "Disconnected" : "Connected";
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

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            SENDARG arg = new SENDARG();
            var cmd = (sender as Button).Content.ToString().ToEnum<eVEC_CMD>();
            arg.msg = txt_SayMsg.Text;
            arg.goal_1st = txt_Goal1st.Text;
            arg.goal_2nd = txt_Goal2nd.Text;
            arg.pos.x = Convert.ToDouble(txt_TargetPosX.Text);
            arg.pos.y = Convert.ToDouble(txt_TargetPosY.Text);
            arg.pos.ang = Convert.ToDouble(txt_TargetPosAng.Text);
            arg.spd = Convert.ToInt32(txt_TargetSpd.Text);
            arg.acc = Convert.ToInt32(txt_TargetAcc.Text);
            arg.dec = Convert.ToInt32(txt_TargetDcc.Text);
            _ctrl.Vec_SendCmd(cmd, arg);
        }

        private void Button_Click_1(object sender, RoutedEventArgs e)
        {
            var uid = Convert.ToInt32( (sender as Button).Uid );
            switch (uid)
            {
                case 0 : _ctrl.Vec_Open(eVEHICLETYPE.LD, txt_VecIP.Text); break;
                case 1: _ctrl.Vec_Close(); break;
            }
        }
    }
}
