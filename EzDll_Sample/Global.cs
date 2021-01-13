using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzDll_Sample
{
    internal static class ExtensionMethods
    {
        public static T ToEnum<T>(this string value)
        {
            if (!System.Enum.IsDefined(typeof(T), value)) return default(T);
            return (T)System.Enum.Parse(typeof(T), value, true);
        }
    }

    public class Global
    {
        public enum ePAGE : int
        {
              Vehicle
            , Power
        }

        public enum eVIWER : int
        {
              None
            , Pnl_Vec
            , Pnl_Pwr
        }

        public enum eDATAEXCHANGE
        {
              Load
            , Save
            , StatusUpdate
            , Data2UI
            , UI2Data
        }

        public class UDATESTATUS
        {
            public eVIWER id;
            public string mthd;
            public string msg;

            public UDATESTATUS()
            {
                id = eVIWER.None;
                mthd = msg = string.Empty;
            }
        }
        

        public class TIMEARG
        {
            public long nStart = 0;
            public long nDelay = 0;
            public long nCurr = 0;

            private void SetCheckTime(ref long nTime)
            {
                nTime = DateTime.Now.Ticks;
            }

            private bool GetCheckTimeOver(long nStartTm, long nDelay, ref long CurrTm)
            {
                bool rtn = false; long nCurrTime = 0; double dCurr = 0.0f;
                nCurrTime = System.DateTime.Now.Ticks;
                dCurr = (nCurrTime - nStartTm) / 10000.0f;
                rtn = (nDelay < dCurr) ? true : false;
                CurrTm = (long)dCurr;
                return rtn;
            }

            private void ChkCurrTime(ref long CurrTm)
            {
                long nCurrTime = 0; double dCurr = 0.0f;
                nCurrTime = System.DateTime.Now.Ticks;
                dCurr = (nCurrTime - this.nStart) / 10000.0f;
                CurrTm = (long)dCurr;
                this.nStart = 0;
            }

            public void Check()
            {
                switch (this.nStart)
                {
                    case 0: SetCheckTime(ref this.nStart); break;
                    default: ChkCurrTime(ref this.nCurr); break;
                }
            }

            public void Reset()
            {
                this.nStart = 0;
            }

            public bool IsOver(long Delay)
            {
                bool rtn = false;
                switch (this.nStart)
                {
                    case 0:
                        SetCheckTime(ref this.nStart);
                        this.nDelay = Delay;
                        break;
                    default:
                        rtn = GetCheckTimeOver(this.nStart, this.nDelay, ref this.nCurr);
                        if (true == rtn)
                        {
                            this.nStart = 0;
                            return true;
                        }
                        break;
                }
                return rtn;
            }
        }
    }
    
}
