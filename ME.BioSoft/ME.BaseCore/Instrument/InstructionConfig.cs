using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using System.Text;
using System.Threading.Tasks;

namespace ME.BaseCore.Instrument
{
    /// <summary>
    /// 指令配置
    /// </summary>
    public class InstructionConfig
    {




        #region 状态关指令
      
        #endregion

    }
    #region 泵
    public class PumpResetConfig
    {
        public static byte[] cmdPumpReset = new byte[] { 0xcc, 0x03, 0x45, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 8;
        public static int type = 1;
    }
    public class PumpDistanceConfig
    {
        public static byte[] cmdPumpDistance = new byte[] { 0xcc, 0x03, 0x4E, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 8;
        public static int  type = 1;
    }
    public class PumpSpeedConfig
    {
        public static byte[] cmdPumpSpeed = new byte[] { 0xCC, 0x03, 0x07, 0xFF, 0xEE, 0xBB, 0xAA, 0x64, 0x00, 0x00, 0x00, 0xDD };//工厂指令
        public static byte header = 0xcc;
        public static int count = 8;
        public static int type = 1;
    }
    public class PumpDistanceGetConfig
    {
        public static byte[] cmdPumpDistanceGet = new byte[] { 0xcc, 0x03, 0x66, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 1;
    }
    public class PumpReadAddrConfig
    {
        public static byte[] cmdPumpReadAddr = new byte[] { 0xcc, 0x00, 0x20, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 1;
    }
    public class PumpWriteAddrConfig
    {
        public static byte[] cmdPumpWriteAddr = new byte[] { 0xcc, 0x13, 0x00, 0xff, 0xee, 0xbb, 0xaa, 0x0d, 0x00, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 1;
    }
    #endregion
    #region 旋切阀
    public class ReCircleResetConfig
    {
        public static byte[] cmdReCircleReset = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 2;
    }
    public class ReCircleHoleConfig
    {
        public static byte[] cmdReCircleHole = new byte[] { 0xcc, 0x02, 0xA4, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 2;
    }
    public class ReCircleHoleGetConfig
    {
        public static byte[] cmdReCircleHoleGet = new byte[] { 0xcc, 0x02, 0x3E, 0x00, 0x00, 0xdd };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 2;
    }
    #endregion
    #region 电磁阀
    public class SwitchResetConfig
    {
        public static byte[] cmdSwitchReset = new byte[] { 0x04, 0x0F, 0x00, 0x1F, 0x00, 0x20, 0x04, 0x00, 0x00, 0x00, 0x00 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 3;
    }
    #endregion
    #region Z轴
    public class ZAxisResetConfig
    {
        public static byte[] cmdZAxisReset = new byte[] { 0x01, 0x06, 0x00, 0x00, 0x06, 0x01 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }
    public class ZAxisMoveConfig
    {
        public static byte[] cmdZAxisMove = new byte[] { 0x01, 0x10, 0x00, 0x01, 0x00, 0x02, 0x04, 0x11, 0x30, 0x00, 0x00 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }
    public class ZAxisEnableConfig
    {
        public static byte[] cmdZAxisEnable = new byte[] { 0x01, 0x06, 0x00, 0x00, 0x01, 0x01 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }
    public class ZAxisNoEnableConfig
    {
        public static byte[] cmdZAxisNoEnable = new byte[] { 0x01, 0x06, 0x00, 0x00, 0x01, 0x00 };
        public static byte header = 0xcc;
        public static int count = 0;
    }
    public class ZAxisStopConfig
    {
        public static byte[] cmdZAxisStop = new byte[] { 0x01, 0x06, 0x00, 0x00, 0x04, 0x00 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }
    public class ZAxisPosConfig
    {
        public static byte[] cmdZAxisPos = new byte[] { 0x01, 0x04, 0x00, 0x02, 0x00, 0x02 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }
    public class ZAxisReadSpeedConfig
    {
        public static byte[] cmdZAxisReadSpeed = new byte[] { 0x01, 0x04, 0x00, 0x00, 0x00, 0x02 };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }

    public class ZAxisSetSpeedConfig
    {
        public static byte[] cmdZAxisSetSpeed = new byte[] { 0x01, 0x10, 0x00, 0x03, 0x00, 0x02, 0x04, 0x80, 0x00, 0x45, 0xBB };
        public static byte header = 0xcc;
        public static int count = 0;
        public static int type = 4;
    }
    #endregion
}
