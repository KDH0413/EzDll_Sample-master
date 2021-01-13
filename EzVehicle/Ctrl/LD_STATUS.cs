using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using static EzVehicle.Global_Vec;

namespace EzVehicle.Ctrl
{
    public enum eSTATE_CMD_TYPE : int
    {
          NONE = -1
        , PASSWORD = 10
        , END_OF_COMMAND
        , STATEOFCHARGE
        , LOCATION
        , LOCALIZATIONSCORE
        , TEMPERATURE
        , PARKING

        , EXTENDEDSTATUSFORHUMANS = 100
        , STATUS
        , ERROR
        , PAUSETASK

        , COMMANDERRORDESCRIPTION = 200
        , DISTANCEFROMHERE
        , DISTANCEBETWEEN
        , RANGEDEVICEGETCURRENT

    }
    public enum eSTATE : int
    {
          NONE = -1
        , IDLE_PROCESSING
        , STOPPING
        , STOPPED
        , AFTER_GOAL
        , ARRIVED_AT
        , ARRIVED_AT_POINT
        , COMPLETED_DOING_TASK_MOVE
        , COMPLETED_DOING_TASK_DELTAHEADING
        , COMPLETED_DOING_TASK_SETHEADING
        , DOCKINGSTATE
        , GOING_TO
        , GOING_TO_POINT
        , GOING_TO_DOCK_AT
        , DRIVING
        , DRIVING_INTO_DOCK
        , TELEOP_DRIVING
        , MOVING
        , DOING_TASK_DELTAHEADING
        , DOING_TASK_MOVE
        , DOING_TASK_PAUSE
        , DONE_DRIVING
        , DOCKING
        , DOCKED
        , UNDOCKING
        , ROBOT_LOST
        , FAILED_GOING_TO
        , ESTOP_PRESSED
        , ESTOP_RELIEVED
        , CANNOT_FIND_PATH
        , NO_ENTER
        , PAUSING
        , PAUSE_CANCELLED
        , PAUSE_INTERRUPTED
        , PARKING
        , PARKED
    }

    public class LD_STATUS
    {
        public eSTATE_CMD_TYPE cmd { get; set; }
        public LD_STATE state { get; set; }
        public POS pos { get; set; }
        public double temp { get; set; }
        public double local { get; set; }
        public double batt { get; set; }
        public string dest { get; set; }

        public LD_STATUS(string rcvMsg)
        {
            double dTemp = 0;
            pos = new POS();
            temp = local = batt = 0;
            dest = string.Empty;
            if (null != rcvMsg)
            {
                string[] splitStr = rcvMsg.Split(':');
                cmd = GetCmd(rcvMsg);
                switch (cmd)
                {
                    case eSTATE_CMD_TYPE.EXTENDEDSTATUSFORHUMANS:
                    case eSTATE_CMD_TYPE.STATUS:
                    case eSTATE_CMD_TYPE.ERROR:
                    case eSTATE_CMD_TYPE.PAUSETASK:
                        {
                            var chk = new LD_STATE(cmd, rcvMsg);
                            var st = chk.Get();
                            state = new LD_STATE(cmd, rcvMsg);
                            state.st = st.st;
                            state.dst = st.dst;
                            state.subMsg = st.subMsg;
                            break;
                        }
                    case eSTATE_CMD_TYPE.PASSWORD:
                        break;
                    case eSTATE_CMD_TYPE.END_OF_COMMAND:
                        break;
                    case eSTATE_CMD_TYPE.STATEOFCHARGE:
                        if (true == double.TryParse(splitStr[1], out dTemp))
                        {
                            batt = dTemp;
                        }
                        break;
                    case eSTATE_CMD_TYPE.LOCALIZATIONSCORE:
                        if (true == double.TryParse(splitStr[1], out dTemp))
                        {
                            local = dTemp * 100.0f;
                        }
                        break;
                    case eSTATE_CMD_TYPE.TEMPERATURE:
                        if (true == double.TryParse(splitStr[1], out dTemp))
                        {
                            temp = dTemp * 100.0f;
                        }
                        break;
                    case eSTATE_CMD_TYPE.LOCATION:
                        {
                            var data = splitStr[1].Split(' ');
                            if (true == double.TryParse(splitStr[1], out dTemp))
                            {
                                pos.x = dTemp;
                            }
                            if (true == double.TryParse(splitStr[2], out dTemp))
                            {
                                pos.y = dTemp;
                            }
                            if (true == double.TryParse(splitStr[3], out dTemp))
                            {
                                pos.ang = dTemp;
                            }
                            break;
                        }
                    case eSTATE_CMD_TYPE.PARKING:
                        state.st = eSTATE.PARKING;
                        break;
                    default: break;
                }
            }
            else
            {
                cmd = eSTATE_CMD_TYPE.NONE;
                state = new LD_STATE(eSTATE_CMD_TYPE.NONE, null);
            }
        }

        internal eSTATE_CMD_TYPE GetCmd(string rcvMsg)
        {
            eSTATE_CMD_TYPE rtn = eSTATE_CMD_TYPE.NONE;
            string[] splitStr = rcvMsg.Replace(" ", "_").ToUpper().Split(':');
            rtn = splitStr[0].ToEnum<eSTATE_CMD_TYPE>();
            return rtn;
        }
    }
}
