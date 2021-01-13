using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Timers;
using static EzPower.Global_Pwr;

namespace EzPower.Device.Battery
{
    public class Bat4Tabos : IPower
    {
        SerialPort serialPort ;
        public bool _bConected => (null == serialPort) ? false : serialPort.IsOpen;
        internal System.Timers.Timer _StateChk = new System.Timers.Timer();
        public ePWRTYPE eType => ePWRTYPE.TabosBattery;        
        public event EventHandler<bool> Evt_Connection;
        public event EventHandler<JCVT_PWR> Evt_UpdateData;
        public Bat4Tabos()
        {
            serialPort = new SerialPort();
            serialPort.BaudRate = 19200;
            serialPort.DataBits = 8;
            serialPort.Parity = Parity.None;
            serialPort.StopBits = StopBits.One;            
            serialPort.DataReceived += SerialPort_DataReceived;

            _StateChk.Interval = 1000;
            _StateChk.Elapsed += _State_Elapsed;
        }

        private void _State_Elapsed(object sender, ElapsedEventArgs e)
        {
            _StateChk.Stop();
            RequestData();
            _StateChk.Start();
        }

        public bool Open(ePWR_PortName port)
        {
            try
            {
                serialPort.PortName = port.ToString();
                serialPort.Open();
                _StateChk.Start();
                Evt_Connection?.Invoke(this, serialPort.IsOpen);                
                return serialPort.IsOpen;
            }
            catch (Exception e)
            {                
                ReportLog(e.ToString(), false);
                return false;
            }
        }

        public void Close()
        {
            if (null != serialPort)
            {
                _StateChk.Stop();
                serialPort.Close();
                serialPort = null;
            }
        }

        private List<byte> recvStr = new List<byte>();
        private bool IsReceived = false;
        private void SerialPort_DataReceived(object sender, SerialDataReceivedEventArgs e)
        {
            try
            {
                var rtn = new PWRSTATUS();
                byte[] recv = new byte[serialPort.BytesToRead];
                serialPort.Read(recv, 0, serialPort.BytesToRead);
                recvStr.AddRange(recv);
                if (recvStr.Count > 300)
                {
                    recvStr.Clear();
                }

                for (int i = 0; i < recvStr.Count - 1; i++)
                {
                    if (recvStr[i] == 0xAF && recvStr[i + 1] == 0xA0)
                    {
                        byte[] val1 = new byte[] { recvStr[7], recvStr[6] };
                        var Volt = BitConverter.ToInt16(val1, 0);
                        byte[] val2 = new byte[] { recvStr[9], recvStr[8] };
                        var Curr = BitConverter.ToInt16(val2, 0);
                        byte[] val3 = new byte[] { recvStr[11], recvStr[10] };
                        var interVal = BitConverter.ToInt16(val3, 0);
                        rtn.volt = Volt * 0.01;
                        rtn.curr = Curr * 0.01;
                        rtn.soc = interVal;                        
                        recvStr.RemoveRange(0, i + 2);
                        if (rtn.soc <= 0 || rtn.soc >= 100)
                        {
                            rtn.soc = -999;
                        }
                        IsReceived = true;
                        RoportUpdateData(rtn);                        
                    }
                }
            }
            catch (Exception ex)
            {                
                ReportLog(ex.ToString(), false);
            }
        }

        private void RequestData()
        {
            List<byte> sendByte = new List<byte>();

            sendByte.Add(0xAF);
            sendByte.Add(0xFA);

            byte[] data = MakeSendByte(1);
            sendByte.AddRange(data);
            sendByte.Add(ChecksumCalc(data));

            sendByte.Add(0xAF);
            sendByte.Add(0xA0);

            if (!serialPort.IsOpen)
            {
                serialPort.Open();
            }

            if (serialPort.IsOpen)
            {
                IsReceived = false;
                serialPort.Write(sendByte.ToArray(), 0, sendByte.Count);
            }
            else
            {
                //todo
            }
        }

        private byte ChecksumCalc(byte[] src)
        {
            byte checkSum = 0x00;
            foreach (var item in src)
            {
                checkSum += item;
            }
            return checkSum;
        }

        private byte[] MakeSendByte(int id)
        {
            var aaa = new byte[] { Convert.ToByte(id + 0x60), Convert.ToByte(0x05), 0x01, Convert.ToByte(id + 0x60), 0x07, 0x00 }; //0x04, 0x00 };
            return aaa;
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
