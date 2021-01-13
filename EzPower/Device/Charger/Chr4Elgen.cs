using EzPower.Ctrl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static EzPower.Global_Pwr;

namespace EzPower.Device.Charger
{
    public class Chr4Elgen : IPower
    {
        PwrModbus _PwrBus = null;
        private System.Timers.Timer _StateChk = new System.Timers.Timer();
        public bool _bConected => (null == _PwrBus) ? false : _PwrBus._Connected;
        public ePWRTYPE eType => ePWRTYPE.ElgenCharger;
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<JCVT_PWR> Evt_UpdateData;

        public Chr4Elgen()
        {
            _PwrBus = new PwrModbus(ePWRTYPE.ElgenCharger);
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

            //이건 잘못된 정보
            //30001 버전 마이너 버전을 소수점 2자리로 표시 ex) 입력값 : 101 -> 버전: v1.01
            //30002 상태 1 = 충전중 , 2 = 에러, 3 = 충전완료, 4 = 배터리이상, 5 = 대기
            //30003 충전 시간 충전 시작부터 경과 시간
            //30004 충전 전압 1000 => 1000V
            //30005 충전 전류 1000 => 1000A
            //var ary = new int[8] { 30005, 30004, -1, -1, -1, 30006, 30002, 30001 };

            //이게 맞는 정보
            //1) 0x5000   :  version을 소수두자리로 표시  , ex. 101->버전 :v1.01 의미
            //2)  0x5001   :  status 표시(1 = 충전중,2 = 에러,3 = 충전완료,4 = 배터리이상,5 = 대기)
            //3)  0x5002  :   온도(소수두자리로 표시)
            //4)  0x5003  :   충전전압(소수두자리로 표시)
            //5)  0x5004  :   충전전류(소수두자리로 표시)
            //6)  0x5005  :   오류코드(사용안함)
            var ary = new int[] { 0x5004, 0x5003, 1000, 1001, 1002, -1, 0x5001, 0x5000 };

            var bit = new Any64();
            bit[(int)ePWRREAD_ITEMS.Current] = true;
            bit[(int)ePWRREAD_ITEMS.Voltage] = true;
            bit[(int)ePWRREAD_ITEMS.Temp_1st] = true;
            bit[(int)ePWRREAD_ITEMS.Temp_2nd] = true;
            bit[(int)ePWRREAD_ITEMS.Temp_3th] = false;
            bit[(int)ePWRREAD_ITEMS.Error] = true;
            bit[(int)ePWRREAD_ITEMS.State] = true;
            bit[(int)ePWRREAD_ITEMS.Ver] = true;            
            var chk = _PwrBus.Open(port, ary, bit.INT32_0);
            if (null != chk.msg)
            {
                ReportLog(chk.msg);
            }
            _StateChk.Start();
            return chk.rtn;
        }

        public void Close()
        {
            _StateChk.Stop();
            if (null != _PwrBus)
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
