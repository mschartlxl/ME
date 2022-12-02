using HandyControl.Expression.Shapes;
using ME.BaseCore;
using ME.BaseCore.Instrument;
using ME.ControlLibrary.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ME.BioSoft
{
    public class AppContext
    {
        private static AppContext _instance;
        public static AppContext Instance
        {
            get
            {
                if (null == _instance)
                    _instance = new AppContext();
                return _instance;
            }
        }
        #region 实例属性
        private AppContext()
        { }

        #endregion
        private static string sysPath = Environment.CurrentDirectory + @"\Config\infos.ini";
        private static string iniPath = Environment.CurrentDirectory + @"\Config\config.ini";
        public static StepInfo _stepInfo = new StepInfo();
        /// <summary>
        /// 将16进制的字符串转为byte[]
        /// </summary>
        /// <param name="hexString"></param>
        /// <returns></returns>
        public static byte[] StrToHexByte(string hexString)
        {
            hexString = hexString.Replace(" ", "");
            if ((hexString.Length % 2) != 0)
                hexString += " ";
            byte[] returnBytes = new byte[hexString.Length / 2];
            for (int i = 0; i < returnBytes.Length; i++)
                returnBytes[i] = Convert.ToByte(hexString.Substring(i * 2, 2), 16);
            return returnBytes;
        }
        public static byte[] GetByteArray(string shex)
        {
            string[] ssArray = shex.Split(' ');
            List<byte> bytList = new List<byte>();
            foreach (var s in ssArray)
            {                //将十六进制的字符串转换成数值  
                bytList.Add(Convert.ToByte(s, 16));
            }    //返回字节数组          
            return bytList.ToArray();
        }

        static AppContext()
        {
            try
            {
           
                _instance = new AppContext();
                _stepInfo.InitIS();
                var ds = ABTInstrument.float2String(6000).ToByteArray(16);
               var ddsds= UInt32.Parse("45BB8000", System.Globalization.NumberStyles.AllowHexSpecifier);
                var di = BitConverter.GetBytes(ddsds);
                var g = new byte[] { 0x00, 0x80, 0xbb, 0x45 };
                var bg=BitConverter.ToSingle(g,0);

                //actdistance = Convert.ToInt32(strlist[4] + strlist[3], 16);
                //GlobalVariables.Instance.InitReset();
                if (!UtilsFun._AbtInstrument.IsOpen(UtilsFun._AbtInstrument.SerialPump) || !UtilsFun._AbtInstrument.IsOpen(UtilsFun._AbtInstrument.SerialSwitch))
                {
                    if (!UtilsFun._AbtInstrument.OpenIfRequirement(UtilsFun._AbtInstrument.SerialPump)|| !UtilsFun._AbtInstrument.OpenIfRequirement(UtilsFun._AbtInstrument.SerialSwitch))
                    {
                        //MessageBox.Show("串口打开失败！", "提示");
                        ////DialogExtension.ShowDialog(DialogType.Warning, LangHelper.LangRes("serialPortFailed"));
                        //return;
                    }
                    else
                    {
                        GlobalVariables.Instance.InitReset();
                    }
                }
            }
            catch (Exception ex)
            {
                //LogHelper.Error(ex.Message, ex);
            }
        }
       
    }
}
