using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzPower.Global_Pwr;

namespace EzPower
{
    public interface IPower
    {
        bool _bConected { get; }
        ePWRTYPE eType { get; }         
        event EventHandler<bool> Evt_Connection;
        event EventHandler<JCVT_PWR> Evt_UpdateData;
        bool Open(ePWR_PortName port);
        void Close();
    }
}
