using EzPower;
using EzPower.Device.Battery;
using EzPower.Device.Charger;
using EzPower.Device.Meter;
using EzVehicle;
using EzVehicle.Device;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzDll_Sample.Global;
using static EzPower.Global_Pwr;
using static EzVehicle.Global_Vec;

namespace EzDll_Sample
{
    public class MainCtrl
    {
        public bool bLoaded = false;
        public MainWindow frmMain;
        public event EventHandler<UDATESTATUS> EvtUpdateStatus;
        private IVehicle _Vec;
        private IPower _Pwr;
        private IPower _Mtr;
        public MainCtrl(MainWindow main)
        {
            frmMain = main;
        }

        public async Task Dispose()
        {
            CloseHW();
        }
        
        public bool Vec_Open(eVEHICLETYPE Type, string ip)
        {
            switch (Type)
            {
                case eVEHICLETYPE.LD: _Vec = new LD(ip); break;
                case eVEHICLETYPE.MIR: break;
                default: return false;
            }
            _Vec.Evt_Connection += On_VecConnection;
            _Vec.Evt_UpdateData += On_VecUpdateData;
            var chk = _Vec.Open();
            return chk;
        }

        public void Vec_Close()
        {
            if (null != _Vec)
            {
                _Vec.Evt_Connection -= On_VecConnection;
                _Vec.Evt_UpdateData -= On_VecUpdateData;

                _Vec.Close();
                _Vec = null;
            }
        }

        public void Vec_SendCmd(eVEC_CMD cmd, SENDARG s)
        {
            var arg = JCVT_VEC.Set(s);
            _Vec.Send(cmd, arg);
        }

        private void On_VecConnection( object sender, bool Connection )
        {
            EvtUpdateStatus?.Invoke(this, new UDATESTATUS() { id = eVIWER.Pnl_Vec, mthd = $"{System.Reflection.MethodBase.GetCurrentMethod().Name}", msg = $"{(true == Connection ? "1" : "0")}" });
        }

        private void On_VecUpdateData(object sender, JCVT_VEC data)
        {
            EvtUpdateStatus?.Invoke(this, new UDATESTATUS() { id = eVIWER.Pnl_Vec, mthd = data.typeName, msg = data.json });
        }
        

        public bool Pwr_Open(ePWRTYPE Type, ePWR_PortName port)
        {
            switch (Type)
            {
                case ePWRTYPE.TabosBattery: _Pwr = new Bat4Tabos(); break;
                case ePWRTYPE.ElgenBattery: _Pwr = new Bat4Elgen(); break;
                case ePWRTYPE.ElgenCharger: _Pwr = new Chr4Elgen(); break;
                case ePWRTYPE.AutonicMeter: _Pwr = new Mtr4Autonics(); break;
            }
            _Pwr.Evt_Connection += On_PwrConnection;
            _Pwr.Evt_UpdateData += On_PwrUpdateData;
            _Pwr.Open(port);
            return true;
        }

        public void Pwr_Close()
        {
            if (null != _Pwr)
            {
                _Pwr.Evt_Connection -= On_PwrConnection;
                _Pwr.Evt_UpdateData -= On_PwrUpdateData;
                _Pwr.Close();
                _Pwr = null;
            }
        }

        private void On_PwrConnection(object sender, bool Connection)
        {
            EvtUpdateStatus?.Invoke(this, new UDATESTATUS() { id = eVIWER.Pnl_Vec, mthd = $"{System.Reflection.MethodBase.GetCurrentMethod().Name}", msg = $"{(true == Connection ? "1" : "0")}" });
        }

        private void On_PwrUpdateData(object sender, JCVT_PWR data)
        {
            EvtUpdateStatus?.Invoke(this, new UDATESTATUS() { id = eVIWER.Pnl_Pwr, mthd = data.typeName, msg = data.json });
        }

        public async void CloseHW()
        {
            Vec_Close();
            Pwr_Close();
        }
    }
}
