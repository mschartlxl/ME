using ME.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ME.BaseCore
{
    public class UtilsFun
    {
        static UtilsFun()
        {
            //if (abtInstrument == null)
            //{
            //    abtInstrument = new Instrument.ABTInstrument();
            //    abtInstrument.NewSerialPort();
            //}
        }
        /// <summary>
        /// 时间差
        /// </summary>
        /// <param name="beginTime"></param>
        /// <param name="endTime"></param>
        /// <returns></returns>
        public static string GetTimeCha_End_Begin(DateTime beginTime, DateTime endTime)
        {
            decimal tDiff = 0;
            string tUnit = "";

            TimeSpan midTime = endTime - beginTime;
            int Days = midTime.Days;
            int Hours = midTime.Hours;
            int Minutes = midTime.Minutes;
            int Seconds = midTime.Seconds;
            decimal Mil = midTime.Milliseconds / 1000;
            decimal tS = midTime.Seconds + Mil;
            if (Days > 0 || Hours > 0)
            {
                tUnit = LangHelper.LangRes("hour"); //"时";
                tDiff = Math.Round(Days * 24 + Hours + Minutes / 60 + tS / 60 / 60, 2);
            }
            else if (Minutes > 0)
            {
                tUnit = LangHelper.LangRes("minute"); //"分钟";
                tDiff = Math.Round(Days * 24 * 60 + Hours * 60 + Minutes + tS / 60, 2);
            }
            else
            {
                tUnit = LangHelper.LangRes("second"); // "秒";
                tDiff = Math.Round(Days * 24 * 60 * 60 + Hours * 60 * 60 + Minutes * 60 + tS, 2);
            }
            return tDiff.ToString() + tUnit;

        }
        static ME.BaseCore.Instrument.ABTInstrument abtInstrument;
        public static ME.BaseCore.Instrument.ABTInstrument _AbtInstrument
        {
            get
            {
                if (abtInstrument == null)
                {
                    abtInstrument = new Instrument.ABTInstrument();
                    abtInstrument.ReadSerialConfig();
                }
                return abtInstrument;
            }
        }
        /// <summary>
        /// 获取时间ID
        /// </summary>
        /// <returns></returns>
        public static string GetNewGuid()
        {
            return Guid.NewGuid().ToString();//DateTime.Now.ToString("yyyyMMddHHmmssffff");
        }
        /// <summary>
        /// 获取节点项的xml
        /// </summary>
        /// <returns></returns>
        public static XElement GetNodeXml(TreeNodeInfo treeNode, NodeModel csNodeMode, string ParentNodeId, string NodeId)
        {
            if (null != treeNode)
            {
                treeNode.ParentNodeId = ParentNodeId;
                treeNode.NodeId = NodeId;
                return treeNode.ToXElement();
            }
            return null;
        }
        public static int GetIndex(List<NodeInfo> NodeInfos, NodeInfo nodeInfo)
        {
            int nodeinfoIndex = NodeInfos.IndexOf(nodeInfo);
            if (nodeinfoIndex == -1)
            {
                int index = 0;
                foreach (var item in NodeInfos)
                {
                    if (item.NodeId == nodeInfo.NodeId && item.ParentNodeId == nodeInfo.ParentNodeId && item.csNodeMode == nodeInfo.csNodeMode)// && item.XInfo == nodeInfo.XInfo)
                    {
                        return index;
                    }
                    index++;
                }
            }
            return nodeinfoIndex;
        }
    }
}
