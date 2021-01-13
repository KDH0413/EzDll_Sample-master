using EasyModbus;
using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzPower.Global_Pwr;

namespace EzPower.Ctrl
{
    internal class PwrModbus
    {
        ePWRTYPE type { get; }
        public ModbusClient modbusClnt = null;
        public bool _Connected => (null == modbusClnt) ? false : modbusClnt.Connected;
        public event EventHandler<bool> Evt_ModBusConnection;
        private System.Timers.Timer _StatusChk = new System.Timers.Timer();
        private Dictionary<ePWRREAD_ITEMS, READITEM> _dicItems = new Dictionary<ePWRREAD_ITEMS, READITEM>();
        public PwrModbus(ePWRTYPE Type)
        {
            type = Type;

            var Current = new READITEM() { bEnb = true };
            var Voltage = new READITEM() { bEnb = true };
            var Temp_1st = new READITEM() { bEnb = true };
            var Temp_2nd = new READITEM() { bEnb = true };

            var Temp_3th = new READITEM() { bEnb = true };
            var Temp_4th = new READITEM() { bEnb = true };
            var Error = new READITEM() { bEnb = true };
            var State = new READITEM() { bEnb = true };
            var Ver = new READITEM() { bEnb = true };

            var soc = new READITEM() { bEnb = true };

            _dicItems[ePWRREAD_ITEMS.Current] = Current;
            _dicItems[ePWRREAD_ITEMS.Voltage] = Voltage;
            _dicItems[ePWRREAD_ITEMS.Temp_1st] = Temp_1st;
            _dicItems[ePWRREAD_ITEMS.Temp_2nd] = Temp_2nd;
            _dicItems[ePWRREAD_ITEMS.Temp_3th] = Temp_3th;
            _dicItems[ePWRREAD_ITEMS.Temp_4th] = Temp_4th;
            _dicItems[ePWRREAD_ITEMS.Error] = Error;
            _dicItems[ePWRREAD_ITEMS.State] = State;
            _dicItems[ePWRREAD_ITEMS.Ver] = Ver;
        }

        public (bool rtn, string msg) Open(ePWR_PortName Port, int[] addrAry, int enb4Bit)
        {
            try
            {
                int idx = 0;
                Any64 EnbItems = new Any64();
                EnbItems.INT32_0 = enb4Bit;
                foreach (var addr in addrAry)
                {
                    var item = _dicItems[(ePWRREAD_ITEMS)idx];
                    item.bEnb = EnbItems[idx];
                    item.add = addr;
                    idx++;
                }
                modbusClnt = new ModbusClient();
                modbusClnt.ConnectedChanged += new EasyModbus.ModbusClient.ConnectedChangedHandler(On_Connected);
                modbusClnt.SerialPort = Port.ToString();
                modbusClnt.Baudrate = 38400;
                switch (type)
                {
                    case ePWRTYPE.ElgenBattery:
                        modbusClnt.ReceiveDataChanged += new ModbusClient.ReceiveDataChangedHandler(On_ReceiveData);
                        modbusClnt.SendDataChanged += new ModbusClient.SendDataChangedHandler(On_SendData);
                        modbusClnt.Baudrate = 9600;
                        modbusClnt.StopBits = System.IO.Ports.StopBits.One;
                        break;
                    case ePWRTYPE.ElgenCharger:
                        modbusClnt.StopBits = System.IO.Ports.StopBits.One;
                        break;
                    case ePWRTYPE.AutonicMeter:
                        modbusClnt.Baudrate = 9600;
                        modbusClnt.StopBits = System.IO.Ports.StopBits.Two;
                        break;
                    default: return (false, $"Wrong {type} setting!!!!");
                }
                modbusClnt.Parity = System.IO.Ports.Parity.None;
                modbusClnt.ConnectionTimeout = 500;
                if (!modbusClnt.Connected)
                {
                    modbusClnt.Connect();
                }
                return (true, null);
            }
            catch (Exception e)
            {
                return (false, e.ToString());
            }
        }

        private void On_Connected(object sender)
        {
            Evt_ModBusConnection?.Invoke(this, modbusClnt.Connected);
        }

        string receiveData = null;
        private void On_ReceiveData(object sender)
        {
            receiveData = "Rx: " + BitConverter.ToString(modbusClnt.receiveData).Replace("-", " ") + System.Environment.NewLine;

        }

        private void On_SendData(object sender)
        {

        }

        public void Close()
        {
            Evt_ModBusConnection?.Invoke(this, false);
            modbusClnt.ConnectedChanged -= On_Connected;
            modbusClnt.Disconnect();
            modbusClnt = null;
        }

        public string ReadItem(ePWRREAD_ITEMS key)
        {
            if (modbusClnt.Connected) goto LBL_ERROR;
            var item = _dicItems[key];
            if (false == item.bEnb) goto LBL_ERROR;
            string rtn = null;
            switch (type)
            {
                case ePWRTYPE.ElgenBattery:
                    break;
                case ePWRTYPE.ElgenCharger:
                    {
                        switch (key)
                        {
                            case ePWRREAD_ITEMS.Temp_1st:
                            case ePWRREAD_ITEMS.Temp_2nd:
                            case ePWRREAD_ITEMS.Temp_3th:
                            case ePWRREAD_ITEMS.Temp_4th:
                                {
                                    modbusClnt.UnitIdentifier = 1;
                                    var val = modbusClnt.ReadInputRegisters(item.add, 1).FirstOrDefault();
                                    rtn = val.ToString();
                                    break;
                                }
                            case ePWRREAD_ITEMS.Current:
                            case ePWRREAD_ITEMS.Voltage:
                            case ePWRREAD_ITEMS.Error:
                            case ePWRREAD_ITEMS.State:
                            case ePWRREAD_ITEMS.Ver:
                                {
                                    modbusClnt.UnitIdentifier = 2;
                                    var val = modbusClnt.ReadInputRegisters(item.add, 1).FirstOrDefault();
                                    switch (key)
                                    {
                                        case ePWRREAD_ITEMS.Current:
                                        case ePWRREAD_ITEMS.Voltage:
                                        case ePWRREAD_ITEMS.Temp_1st:
                                        case ePWRREAD_ITEMS.Temp_2nd:
                                        case ePWRREAD_ITEMS.Temp_3th:
                                        case ePWRREAD_ITEMS.Temp_4th:
                                        case ePWRREAD_ITEMS.Error:
                                            rtn = val.ToString();
                                            break;
                                        case ePWRREAD_ITEMS.State: rtn = val.ToString().ToEnum<ePWR_CHR_STATE>().ToString(); break;
                                        case ePWRREAD_ITEMS.Ver: rtn = (val / 100).ToString(); break;
                                        default: goto LBL_ERROR;
                                    }
                                    break;
                                }
                            default: goto LBL_ERROR;
                        }
                        break;
                    }
                case ePWRTYPE.AutonicMeter:
                    {
                        switch (key)
                        {
                            case ePWRREAD_ITEMS.Current: modbusClnt.UnitIdentifier = 2; break;
                            case ePWRREAD_ITEMS.Voltage: modbusClnt.UnitIdentifier = 1; break;
                            case ePWRREAD_ITEMS.Temp_1st:
                            case ePWRREAD_ITEMS.Temp_2nd:
                            case ePWRREAD_ITEMS.Temp_3th:
                            case ePWRREAD_ITEMS.Temp_4th:   modbusClnt.UnitIdentifier = 3; break;
                            default: goto LBL_ERROR;
                        }
                        var val = modbusClnt.ReadInputRegisters(item.add, 1).FirstOrDefault();
                        switch (key)
                        {
                            case ePWRREAD_ITEMS.Current: case ePWRREAD_ITEMS.Voltage: rtn = (val / 10.0).ToString(); break;
                            default: rtn = val.ToString(); break;
                        }
                        break;
                    }
                default: goto LBL_ERROR;
            }
            return rtn;
            LBL_ERROR:
            return null;
        }

        public async Task<PWRSTATUS> ReadItemsAll()
        {
            var rtn = new PWRSTATUS();
            if (!modbusClnt.Connected)
                return rtn;

            foreach (var item in _dicItems)
            {
                var src = item.Value as READITEM;
                if (src.add < 0)
                {
                    continue;
                }

                switch (item.Key)
                {
                    case ePWRREAD_ITEMS.Current:
                        switch (type)
                        {
                            case ePWRTYPE.ElgenBattery: modbusClnt.UnitIdentifier = 170; break;
                            case ePWRTYPE.ElgenCharger: modbusClnt.UnitIdentifier = 2; break;
                            case ePWRTYPE.AutonicMeter: modbusClnt.UnitIdentifier = 1; break;
                            default: return new PWRSTATUS();
                        }
                        break;
                    case ePWRREAD_ITEMS.Voltage:
                        switch (type)
                        {
                            case ePWRTYPE.ElgenBattery: modbusClnt.UnitIdentifier = 170; break;
                            case ePWRTYPE.ElgenCharger: modbusClnt.UnitIdentifier = 2; break;
                            case ePWRTYPE.AutonicMeter: modbusClnt.UnitIdentifier = 2; break;
                            default: return new PWRSTATUS();
                        }
                        break;
                    case ePWRREAD_ITEMS.Error:
                    case ePWRREAD_ITEMS.State:
                    case ePWRREAD_ITEMS.Ver:
                        switch (type)
                        {
                            case ePWRTYPE.ElgenBattery: modbusClnt.UnitIdentifier = 1; break;
                            case ePWRTYPE.ElgenCharger: modbusClnt.UnitIdentifier = 2; break;
                            case ePWRTYPE.AutonicMeter: continue;
                            default: return new PWRSTATUS();
                        }
                        break;
                    case ePWRREAD_ITEMS.Temp_1st:
                    case ePWRREAD_ITEMS.Temp_2nd:
                    case ePWRREAD_ITEMS.Temp_3th:
                    case ePWRREAD_ITEMS.Temp_4th:
                        switch (type)
                        {
                            case ePWRTYPE.ElgenBattery: modbusClnt.UnitIdentifier = 2; break;
                            case ePWRTYPE.ElgenCharger: modbusClnt.UnitIdentifier = 1; break;
                            case ePWRTYPE.AutonicMeter: modbusClnt.UnitIdentifier = 2; break;
                            default: return new PWRSTATUS();
                        }
                        break;
                }


                if (src.bEnb)
                {
                    await Task.Run(() =>
                    {
                        var val = 0;
                        try
                        {
                            val = modbusClnt.ReadInputRegisters(src.add, 1).FirstOrDefault();
                        }
                        catch (Exception ex)
                        {
                            Debug.WriteLine($"modbus read fail. add:{src.add}");
                        }
                        switch (type)
                        {
                            case ePWRTYPE.ElgenBattery:
                                switch (item.Key)
                                {
                                    case ePWRREAD_ITEMS.Current:
                                        src.val = (val / 1000f).ToString("0.00");
                                        break;
                                    case ePWRREAD_ITEMS.Voltage:
                                        src.val = (val / 100f).ToString("0.00");
                                        break;
                                    case ePWRREAD_ITEMS.Temp_1st:
                                    case ePWRREAD_ITEMS.Temp_2nd:
                                    case ePWRREAD_ITEMS.Temp_3th:
                                    case ePWRREAD_ITEMS.Temp_4th:
                                    case ePWRREAD_ITEMS.Error:
                                    case ePWRREAD_ITEMS.State:
                                    case ePWRREAD_ITEMS.Ver:
                                    default:
                                        src.val = val.ToString();
                                        break;
                                }
                                break;
                            case ePWRTYPE.ElgenCharger:
                                switch (item.Key)
                                {
                                    case ePWRREAD_ITEMS.Current:
                                    case ePWRREAD_ITEMS.Voltage:
                                        src.val = (val / 100f).ToString("0.00");
                                        break;
                                    case ePWRREAD_ITEMS.Temp_1st:
                                    case ePWRREAD_ITEMS.Temp_2nd:
                                    case ePWRREAD_ITEMS.Temp_3th:
                                    case ePWRREAD_ITEMS.Temp_4th:
                                    default:
                                        src.val = val.ToString();
                                        break;
                                }
                                break;
                            case ePWRTYPE.AutonicMeter:
                                switch (item.Key)
                                {
                                    case ePWRREAD_ITEMS.Current:
                                    case ePWRREAD_ITEMS.Voltage:
                                        src.val = (val / 10.0).ToString("0.00");
                                        break;
                                    default:
                                        src.val = val.ToString();
                                        break;
                                }
                                break;
                            default:
                                //return new PWRSTATUS();
                                break;
                        }
                    });
                }
                else
                {
                    src.val = "0";
                }

                Debug.WriteLine($"{DateTime.Now.ToString("hh:mm:ss.fff")}    read[{type}][{item.Key}][{src.add}] : {src.val}");
            }

            double dTemp = 0;
            int nTemp = 0;
            switch (type)
            {
                case ePWRTYPE.AutonicMeter:
                case ePWRTYPE.ElgenBattery:
                case ePWRTYPE.ElgenCharger:
                default:
                    if (double.TryParse(_dicItems[ePWRREAD_ITEMS.Current].val, out dTemp))
                    {
                        rtn.curr = dTemp;
                    }
                    if (double.TryParse(_dicItems[ePWRREAD_ITEMS.Voltage].val, out dTemp))
                    {
                        rtn.volt = dTemp;
                    }
                    break;
            }

            if (double.TryParse(_dicItems[ePWRREAD_ITEMS.Temp_1st].val, out dTemp))
            {
                rtn.temp_1st = dTemp;
            }

            if (double.TryParse(_dicItems[ePWRREAD_ITEMS.Temp_2nd].val, out dTemp))
            {
                rtn.temp_2nd = dTemp;
            }

            if (double.TryParse(_dicItems[ePWRREAD_ITEMS.Temp_3th].val, out dTemp))
            {
                rtn.temp_3th = dTemp;
            }

            if (double.TryParse(_dicItems[ePWRREAD_ITEMS.Temp_4th].val, out dTemp))
            {
                rtn.temp_4th = dTemp;
            }

            if (int.TryParse(_dicItems[ePWRREAD_ITEMS.Error].val, out nTemp))
            {
                rtn.errbit = nTemp;
            }
            if (int.TryParse(_dicItems[ePWRREAD_ITEMS.State].val, out nTemp))
            {
                rtn.state = (ePWR_CHR_STATE)nTemp;
            }

            switch (type)
            {
                case ePWRTYPE.ElgenBattery:
                    try
                    {
                        await Task.Run(() => 
                        {
                            modbusClnt.UnitIdentifier = 170;
                            var val = modbusClnt.ReadInputRegisters(44, 1).FirstOrDefault();
                            var volt = (int)(val / 100f);
                            rtn.soc = volt;
                        });
                    }
                    catch (Exception)
                    {
                        Debug.WriteLine($"modbus read fail. add:{44}");
                    }
                    break;
                default:
                    if (0 < rtn.volt)
                    {
                        int maxvolt = 56;
                        rtn.soc = Convert.ToInt32(rtn.volt / maxvolt * 100.0);
                    }
                    break;
            }
            return rtn;
        }
    }

    public class READITEM
    {
        public ePWRREAD_ITEMS eType { get; }
        public bool bEnb { get; set; }
        public int add { get; set; }
        public string val { get; set; }
    }
}
