using EzVehicle.Ctrl;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzVehicle
{
    internal static class ExtensionMethods
    {
        public static T ToEnum<T>(this string value)
        {
            if (!System.Enum.IsDefined(typeof(T), value)) return default(T);
            return (T)System.Enum.Parse(typeof(T), value, true);
        }
    }

    public class Global_Vec
    {        
        public class JCVT_VEC
        {
            public string typeName { get; set; }
            public string json { get; set; }
            public static JCVT_VEC Set<T>(T data)
            {
                var obj = new JCVT_VEC();
                obj.typeName = $"{typeof(T).Name}";
                obj.json = JsonConvert.SerializeObject(data, Formatting.Indented);
                return obj;
            }

            public static void Get(string data, out SENDARG arg)
            {
                arg = JsonConvert.DeserializeObject<SENDARG>(data);
            }

            public static void Get(string data, out LD_STATUS arg)
            {
                arg = JsonConvert.DeserializeObject<LD_STATUS>(data);
            }
            public static void Get(string data, out LOG arg)
            {
                arg = JsonConvert.DeserializeObject<LOG>(data);
            }
        }

        public enum eVEHICLETYPE
        {
              LD
            , MIR
        }

        public enum eVEC_CMD
        {
              None = -1
            , Stop
            , Say
            , PauseCancel
            , Go2Goal
            , Go2Point
            , Go2Straight
            , Dock
            , Undock
            , MoveDeltaHeading
            , MoveFront
            , GetDistBetween
            , GetDistFromHere
            , LocalizeAtGoal
        }

        public class POS
        {
            public double x { get; set; }
            public double y { get; set; }
            public double ang { get; set; }
        }

        public class SENDARG
        {
            public string goal_1st { get; set; }
            public string goal_2nd { get; set; }
            public POS pos { get; set; }
            public string msg { get; set; }
            public int move { get; set; }
            public int spd { get; set; }
            public int acc { get; set; }
            public int dec { get; set; }
            public SENDARG()
            {
                goal_1st = goal_2nd = msg = string.Empty; pos = new POS();
                move = 0; spd = 30; acc = 10; dec = 10;
            }
        }
    }

    public class LOG
    {
        public bool bSend { get; set; }
        public string msg { get; set; }
        public string Get()
        {
            return $"{GetTime()} : {(true == bSend ? "S" : "R")} = {msg}";
        }
        private string GetTime(bool bNeed2MilSec = true)
        {
            DateTime NowTime = DateTime.Now;
            if (bNeed2MilSec) return NowTime.ToString("HH:mm:ss.") + NowTime.Millisecond.ToString("000");
            else return NowTime.ToString("HH_mm_ss");
        }
    }
}
