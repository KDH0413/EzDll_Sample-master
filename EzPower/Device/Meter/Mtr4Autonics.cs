using EzPower.Ctrl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static EzPower.Global_Pwr;

namespace EzPower.Device.Meter
{
    public class Mtr4Autonics : IPower
    {
        PwrModbus _PwrBus = null;
        private System.Timers.Timer _StateChk = new System.Timers.Timer();
        public bool _bConected => (null == _PwrBus) ? false : _PwrBus._Connected ;
        public ePWRTYPE eType => ePWRTYPE.AutonicMeter;
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<JCVT_PWR> Evt_UpdateData;
        public Mtr4Autonics()
        {
            _PwrBus = new PwrModbus(ePWRTYPE.AutonicMeter);
            _PwrBus.Evt_ModBusConnection += On_Connection;

            _StateChk.Interval = 1000;
            _StateChk.Elapsed += _State_Elapsed;
        }

        private async void _State_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (false == _bConected) return;
            _StateChk.Stop();
            var status = await _PwrBus.ReadItemsAll();
            RoportUpdateData(status);
            _StateChk.Start();
        }

        private void On_Connection(object sender, bool conn)
        {
            Evt_Connection?.Invoke(this, conn);
        }

        public bool Open(ePWR_PortName port)
        {
            //이 순서대로 넣어야 함
            //Current
            //Voltage
            //Temp_1st
            //Temp_2nd
            //Temp_3th
            //Error
            //State
            //Ver

            var ary = new int[] { -1, -1, 1000, 1001, 1002, -1, -1, -1 };
            var bit = new Any64();
            bit[(int)ePWRREAD_ITEMS.Current] = false;
            bit[(int)ePWRREAD_ITEMS.Voltage] = false;
            bit[(int)ePWRREAD_ITEMS.Temp_1st] = true;
            bit[(int)ePWRREAD_ITEMS.Temp_2nd] = true;
            bit[(int)ePWRREAD_ITEMS.Temp_3th] = true;
            bit[(int)ePWRREAD_ITEMS.Error] = false;
            bit[(int)ePWRREAD_ITEMS.State] = false;
            bit[(int)ePWRREAD_ITEMS.Ver] = false;
            var chk = _PwrBus.Open(port, ary, bit.INT32_0);
            if ( null != chk.msg )
            {
                ReportLog(chk.msg);
            }
            _StateChk.Start();
            return chk.rtn;
        }

        public void Close()
        {
            _StateChk.Stop();
            if ( null != _PwrBus)
            {
                _PwrBus.Close();
                _PwrBus = null;
            }
        }

        private void RoportUpdateData(PWRSTATUS data)
        {
            var jsonLog = JCVT_PWR.Set(data);
            Evt_UpdateData?.Invoke(this, jsonLog);            
        }

        private void ReportLog(string msg, bool bSend = true)
        {
            var jsonLog = JCVT_PWR.Set(new LOG() { bSend = bSend, msg = msg });            
            Evt_UpdateData?.Invoke(this, jsonLog);
            Debug.WriteLine(msg, false);
        }

    }
}
