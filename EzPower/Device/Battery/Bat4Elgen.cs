using EzPower.Ctrl;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Timers;
using static EzPower.Global_Pwr;

namespace EzPower.Device.Battery
{
    public class Bat4Elgen : IPower
    {
        bool bUseModbus = true;
        PwrModbus _PwrBus = null;
        SerialPort _PwrCom;
        private System.Timers.Timer _StateChk = new System.Timers.Timer();
        public bool _bConected => (false == bUseModbus) ? ((null == _PwrCom) ? false : _PwrCom.IsOpen) 
                                                        : ((null == _PwrBus) ? false : _PwrBus._Connected );
        public ePWRTYPE eType => ePWRTYPE.ElgenBattery;
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<JCVT_PWR> Evt_UpdateData;

        public Bat4Elgen()
        {
            if ( false == bUseModbus)
            {
                _PwrCom = new SerialPort();
                _PwrCom.BaudRate = 9600;
                _PwrCom.DataBits = 8;
                _PwrCom.Parity = Parity.None;
                _PwrCom.StopBits = StopBits.One;
                _PwrCom.DataReceived += SerialPort_DataReceived;
            }
            else
            {
                _PwrBus = new PwrModbus(ePWRTYPE.ElgenBattery);
                _PwrBus.Evt_ModBusConnection += On_Connection;
            }
            _StateChk.Interval = 1000;
            _StateChk.Elapsed += _State_Elapsed;
        }

        private async void _State_Elapsed(object sender, ElapsedEventArgs e)
        {
            if (false == _bConected) return;
            _StateChk.Stop();
            PWRSTATUS status = new PWRSTATUS();
            if (false == bUseModbus)
            {
                RequestData();
            }
            else
            {
                status = await _PwrBus.ReadItemsAll();
            }
            RoportUpdateData(status);
            _StateChk.Start();
        }

        private void On_Connection(object sender, bool conn)
        {
            Evt_Connection?.Invoke(this, conn);
        }

        public bool Open(ePWR_PortName port) // 에러는 보호상태
        {
            bool rtn = false;
            if ( false == bUseModbus)
            {
                _PwrCom.PortName = port.ToString();
                _PwrCom.Open();
                rtn = _PwrCom.IsOpen;
            }
            else
            {
                // 모드버스 Data 주소 Curr Volt 온도1 온도2  온도3  보호상태(Error) 사용안함 버젼 == ePWRREAD_ITEMS
                var ary = new int[8] { 86, 73, -1, -1, -1, -1, -1, 1 };
                var bit = new Any64();
                bit[(int)ePWRREAD_ITEMS.Current] = true;
                bit[(int)ePWRREAD_ITEMS.Voltage] = true;
                bit[(int)ePWRREAD_ITEMS.Error] = false;
                bit[(int)ePWRREAD_ITEMS.Ver] = false;
                bit[(int)ePWRREAD_ITEMS.Temp_1st] = false;
                bit[(int)ePWRREAD_ITEMS.Temp_2nd] = false;
                bit[(int)ePWRREAD_ITEMS.Temp_3th] = false;
                bit[(int)ePWRREAD_ITEMS.State] = false;

                var chk = _PwrBus.Open(port, ary, bit.INT32_0);
                if (null != chk.msg)
                {
                    ReportLog(chk.msg);
                }
                rtn = chk.rtn;
            }
            _StateChk.Start();
            Evt_Connection?.Invoke(this, rtn);
            return rtn;
        }

        public void Close()
        {
            _StateChk.Stop();
            if (false == bUseModbus)
            {
                _PwrCom.Close();
                _PwrCom = null;
            }
            else
            {
                if (null != _PwrBus)
                {
                    _PwrBus.Close();
                    _PwrBus = null;
                }
            }
        }

        private void RequestData()
        {
            if (false == _bConected) return;
            List<byte> sendByte = new List<byte>();

//             IsReceived = false;
//             _PwrCom.Write(sendByte.ToArray(), 0, sendByte.Count);
        }

        private List<byte> recvStr = new List<byte>();
        private bool IsReceived = false;
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var rtn = new PWRSTATUS();
                byte[] recv = new byte[_PwrCom.BytesToRead];
                _PwrCom.Read(recv, 0, _PwrCom.BytesToRead);

                RoportUpdateData(rtn);
            }
            catch (Exception err)
            {
                ReportLog(err.ToString(), false);
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
