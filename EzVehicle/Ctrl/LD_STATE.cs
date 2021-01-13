using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace EzVehicle.Ctrl
{
    public class LD_STATE
    {
        public eSTATE_CMD_TYPE cmd { get; }
        public eSTATE st { get; set; }
        public string dst { get; set; }
        public string subMsg { get; set; }
        public string msg { get; }

        public LD_STATE(eSTATE_CMD_TYPE Cmd, string rcvMsg)
        {
            cmd = Cmd; msg = rcvMsg;
            st = eSTATE.NONE;
            dst = subMsg = string.Empty;
        }

        public LD_STATE Get()
        {
            switch (cmd)
            {
                case eSTATE_CMD_TYPE.EXTENDEDSTATUSFORHUMANS:
                    {
                        var chk = EXTENDEDSTATUSFORHUMANS();
                        st = chk.st;
                        dst = chk.dst;
                        subMsg = chk.subStr;
                        break;
                    }
                case eSTATE_CMD_TYPE.STATUS:
                    {
                        var chk = STATUS();
                        st = chk.st;
                        dst = chk.dst;
                        break;
                    }
                case eSTATE_CMD_TYPE.ERROR:
                case eSTATE_CMD_TYPE.PAUSETASK:
                    {
                        st = ERROR_PAUSETASK();
                        break;
                    }
                default: break;
            }
            return this;
        }

        private enum eSTATE_TYPE
        {
              NONE
            , GOING     // 4 EXTENDEDSTATUSFORHUMANS
            , FALIED
            , ESTOP
            , DRIVING
            , MOVING

            , STOPPING // 4 STATUS
            , STOPPED
            , ARRIVED
            , TELEOP
            , DOCKINGSTATE
            , DOING
            , COMPLETED

            , CANNOT // 4 ERROR
            , NO

            , PAUSING // 4 PAUSETASK
            , PAUSE
        }

        private (eSTATE st, string dst, string subStr) EXTENDEDSTATUSFORHUMANS()
        {
            var rtn = eSTATE.NONE; string destination = null; string sub = null;
            string[] splitWord = msg.ToUpper().Split(':');
            string[] splitStr = splitWord[1].ToUpper().Split('|');
            string[] splitState = splitStr[0].Split(' ');

            for (int i = 0; i < splitStr.Count(); i++)
            {
                if (i == 0) continue;
                sub += $"{splitWord[i]}";
            }

            switch (splitState.Count())
            {
                case 1: rtn = splitState[0].ToEnum<eSTATE>(); break;
                case 2:
                    {
                        var chkstr = splitStr[0].Replace(" ", "_");
                        rtn = chkstr.ToEnum<eSTATE>();
                        break;
                    }
                default:
                    if (2 < splitState.Count())
                    {
                        var type = splitState[0].ToEnum<eSTATE_TYPE>();
                        switch (type)
                        {
                            case eSTATE_TYPE.GOING:
                                switch (splitState.Count())
                                {
                                    case 3:
                                        rtn = $"{splitState[0]}_{splitState[1]}".ToEnum<eSTATE>();
                                        destination = splitState[2];
                                        break;
                                    default:
                                        switch (splitState[2])
                                        {
                                            case "POINT":
                                                rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eSTATE>();
                                                destination = $"{splitState[3]},{splitState[4]}";
                                                break;
                                            case "DOCK":
                                                rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}_{splitState[3]}".ToEnum<eSTATE>();
                                                destination = $"{splitState[4]}";
                                                break;
                                        }
                                        break;
                                }
                                break;
                            case eSTATE_TYPE.FALIED:
                                switch (splitState.Count())
                                {
                                    case 4:
                                        rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eSTATE>();
                                        destination = $"{splitState[3]}";
                                        break;
                                    case 5:
                                        rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}_{splitState[3]}".ToEnum<eSTATE>();
                                        destination = $"{splitState[4]}";
                                        break;
                                }
                                break;
                            case eSTATE_TYPE.ESTOP: rtn = eSTATE.ESTOP_RELIEVED; break;
                            case eSTATE_TYPE.DRIVING: rtn = eSTATE.DRIVING_INTO_DOCK; break;
                            case eSTATE_TYPE.MOVING:
                                rtn = eSTATE.MOVING;
                                destination = splitState[1];
                                break;
                        }
                    }
                    break;
            }
            return (rtn, destination, sub);
        }

        internal (eSTATE st, string dst) STATUS()
        {
            var rtn = eSTATE.NONE; string destination = null;
            string[] splitWord = msg.ToUpper().Split(':');
            string[] splitState = splitWord[1].Split(' ');
            var type = splitState[0].ToEnum<eSTATE_TYPE>();
            switch (type)
            {
                case eSTATE_TYPE.STOPPING:
                case eSTATE_TYPE.STOPPED: rtn = splitState[0].ToEnum<eSTATE>(); break;
                case eSTATE_TYPE.TELEOP: rtn = eSTATE.TELEOP_DRIVING; break;
                case eSTATE_TYPE.ESTOP: rtn = (2 >= splitState.Count()) ? eSTATE.ESTOP_PRESSED : eSTATE.ESTOP_RELIEVED; break;
                case eSTATE_TYPE.GOING:
                case eSTATE_TYPE.ARRIVED:
                    switch (splitState.Count())
                    {
                        case 3:
                            rtn = $"{splitState[0]}_{splitState[1]}".ToEnum<eSTATE>();
                            destination = splitState[2];
                            break;
                        default:
                            switch (splitState[2])
                            {
                                case "POINT":
                                    rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eSTATE>();
                                    destination = $"{splitState[3]},{splitState[4]}";
                                    break;
                                default: break;
                            }
                            break;
                    }
                    break;
                case eSTATE_TYPE.DOING:
                    rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}".ToEnum<eSTATE>();
                    switch (rtn)
                    {
                        case eSTATE.DOING_TASK_DELTAHEADING:
                            destination = $"{splitState[3]},{splitState[4]},{splitState[5]},{splitState[6]}";
                            break;
                        case eSTATE.DOING_TASK_MOVE:
                            destination = $"{splitState[3]},{splitState[4]},{splitState[5]},{splitState[6]}_{splitState[5]}";
                            break;
                        case eSTATE.DOING_TASK_PAUSE:
                            break;
                        default: break;
                    }
                    break;
                case eSTATE_TYPE.COMPLETED:
                    rtn = $"{splitState[0]}_{splitState[1]}_{splitState[2]}_{splitState[3]}".ToEnum<eSTATE>();
                    switch (rtn)
                    {
                        case eSTATE.COMPLETED_DOING_TASK_DELTAHEADING:
                            destination = $"{splitState[4]},{splitState[5]},{splitState[6]},{splitState[7]}";
                            break;
                        case eSTATE.COMPLETED_DOING_TASK_MOVE:
                            destination = $"{splitState[4]},{splitState[5]},{splitState[6]},{splitState[7]},{splitState[8]}";
                            break;
                        case eSTATE.COMPLETED_DOING_TASK_SETHEADING: destination = $"{splitState[4]}"; break;
                        default: break;
                    }
                    break;
                case eSTATE_TYPE.DOCKINGSTATE:
                    {
                        string[] splitdock = splitWord[2].Split(' ');
                        switch (splitdock[0])
                        {
                            case "DOCKED":
                            case "DOCKING":
                            case "UNDOCKING":
                                rtn = splitdock[0].ToEnum<eSTATE>();
                                break;
                            default: break;
                        }
                        break;
                    }
                default: break;
            }
            return (rtn, destination);
        }

        internal eSTATE ERROR_PAUSETASK()
        {
            var rtn = eSTATE.NONE;
            string[] splitWord = msg.ToUpper().Split(':');
            string[] splitState = splitWord[1].Split(' ');
            var type = splitState[0].ToEnum<eSTATE_TYPE>();
            switch (type)
            {
                case eSTATE_TYPE.FALIED: rtn = eSTATE.FAILED_GOING_TO; break;
                case eSTATE_TYPE.CANNOT: rtn = eSTATE.CANNOT_FIND_PATH; break;
                case eSTATE_TYPE.NO: rtn = eSTATE.NO_ENTER; break;
                case eSTATE_TYPE.PAUSING: rtn = eSTATE.PAUSING; break;
                case eSTATE_TYPE.PAUSE:
                    rtn = $"{splitState[0]}_{splitState[1]}".ToEnum<eSTATE>();
                    break;
                default: break;
            }
            return rtn;
        }
    }
}
