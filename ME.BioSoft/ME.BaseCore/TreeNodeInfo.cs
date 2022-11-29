using ME.Language;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ME.BaseCore
{
    public class TreeNodeInfo
    {
        public TreeNodeInfo()
        {
            ParentNodeId = "";
            NodeId = "";
            NodeName = "";
            IsEnabled = true;
            IsRollback = false;
            RollbackFlag = "";
            csNodeMode = NodeModel.LoopMode;
        }

        public NodeModel csNodeMode { set; get; }

        /// <summary>
        /// 节点ID 例如：[Node2-ChildList1-Param0]
        /// </summary>
        public string NodeId { set; get; }
        /// <summary>
        /// 父节点
        /// </summary>
        public string ParentNodeId { set; get; }

        /// <summary>
        /// 节点文本
        /// </summary>
        public string NodeName { set; get; }
        /// <summary>
        /// 节点是否启用
        /// </summary>
        public bool IsEnabled { set; get; }

        /// <summary>
        /// 指令回滚
        /// </summary>
        public bool IsRollback { set; get; }
        /// <summary>
        /// 回滚标识
        /// </summary>
        public string RollbackFlag { set; get; }
        #region IObjectSerialize Members

        public virtual System.Xml.Linq.XElement ToXElement()
        {
            XElement result = new XElement(csNodeMode.ToString());
            result.Add(new XAttribute("NodeId", NodeId));
            result.Add(new XAttribute("ParentNodeId", ParentNodeId));
            result.Add(new XAttribute("NodeName", NodeName));
            result.Add(new XAttribute("IsEnabled", IsEnabled));

            result.Add(new XAttribute("IsRollback", IsRollback));
            result.Add(new XAttribute("RollbackFlag", RollbackFlag));
            result.Add(new XAttribute("csNodeMode", (int)csNodeMode));
            return result;
        }

        public virtual object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            NodeId = XMLHelper.GetAttributeStringValue(xml, "NodeId");
            ParentNodeId = XMLHelper.GetAttributeStringValue(xml, "ParentNodeId");
            NodeName = XMLHelper.GetAttributeStringValue(xml, "NodeName");
            IsEnabled = XMLHelper.GetAttributeBoolValue(xml, "IsEnabled");
            IsRollback = XMLHelper.GetAttributeBoolValue(xml, "IsRollback");
            RollbackFlag = XMLHelper.GetAttributeStringValue(xml, "RollbackFlag");
            csNodeMode = (NodeModel)XMLHelper.GetAttributeIntValue(xml, "csNodeMode", 0);

            return this;
        }
    }
    /// <summary>
    /// 步骤信息
    /// </summary>
    public class StepInfo
    {
        /// <summary>
        /// IS默认存放的位置
        /// </summary>
        private string commandInfoISFilePath = string.Format("{0}{1}", AppDomain.CurrentDomain.BaseDirectory + "Config" + System.IO.Path.DirectorySeparatorChar, "ActCmd.xml");
        public StepInfo()
        {
            StepName = "";
            Operator = "";
            CmdTCA = new CommandUnit();
            CmdACN = new CommandUnit();
            CmdCapA = new CommandUnit();
            CmdCapB = new CommandUnit();
            CmdO = new CommandUnit();
            CmdT = new CommandUnit();
            CmdA = new CommandUnit();
            CmdC = new CommandUnit();
            CmdG = new CommandUnit();
            CmdACT = new CommandUnit();
        }
        public bool SaveISFile(string commandInfoFilePath = null)
        {
            try
            {
                if (string.IsNullOrEmpty(commandInfoFilePath))
                {
                    commandInfoFilePath = commandInfoISFilePath;
                }
                //保存文件
                this.ToXElement().Save(commandInfoFilePath);
                //ABTEncrypt.Encode_Xml(commandInfoFilePath);
                return true;
            }
            catch (Exception ex)
            {
                return false;
            }
        }
        /// <summary>
        /// 初始化指令集文件
        /// </summary>
        /// <param name="strInstructionSetFilePath"></param>
        public void InitIS(string strInstructionSetFilePath = null)
        {
            if (string.IsNullOrEmpty(strInstructionSetFilePath))
            {
                strInstructionSetFilePath = commandInfoISFilePath;
            }
            if (System.IO.File.Exists(strInstructionSetFilePath))
            {
                //commandInfoISFilePath = strInstructionSetFilePath;
                this.GetFromXElement(System.Xml.Linq.XElement.Load(strInstructionSetFilePath));//加载到对象       
            }
        }
        /// <summary>
        /// 设备初始化命令单元
        /// </summary>
        public CommandUnit CmdTCA { set; get; }
        public CommandUnit CmdACN { set; get; }
        public CommandUnit CmdCapA { set; get; }
        public CommandUnit CmdCapB { set; get; }
        public CommandUnit CmdO { set; get; }
        public CommandUnit CmdT { set; get; }
        public CommandUnit CmdA { set; get; }
        public CommandUnit CmdC { set; get; }
        public CommandUnit CmdG { set; get; }
        public CommandUnit CmdACT { set; get; }
        public object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            StepName = XMLHelper.GetAttributeStringValue(xml, "StepName");
            Operator = XMLHelper.GetAttributeStringValue(xml, "Operator");


            CmdTCA = new CommandUnit();
            XElement xml_CmdInit = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdTCA".ToLower());
            if (xml_CmdInit != null)
            {
                XElement xml_CommandUnit = xml_CmdInit.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdTCA.GetFromXElement(xml_CommandUnit);
            }


            CmdACN = new CommandUnit();
            XElement xml_CmdACN = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdACN".ToLower());
            if (xml_CmdACN != null)
            {
                XElement xml_CommandUnit = xml_CmdACN.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdACN.GetFromXElement(xml_CommandUnit);
            }


            CmdCapA = new CommandUnit();
            XElement xml_CmdCapA = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdCapA".ToLower());
            if (xml_CmdCapA != null)
            {
                XElement xml_CommandUnit = xml_CmdCapA.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdCapA.GetFromXElement(xml_CommandUnit);
            }

            CmdCapB = new CommandUnit();
            XElement xml_CmdCapB = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdCapB".ToLower());
            if (xml_CmdCapB != null)
            {
                XElement xml_CommandUnit = xml_CmdCapB.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdCapB.GetFromXElement(xml_CommandUnit);
            }

            CmdO = new CommandUnit();
            XElement xml_CmdO = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdO".ToLower());
            if (xml_CmdO != null)
            {
                XElement xml_CommandUnit = xml_CmdO.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdO.GetFromXElement(xml_CommandUnit);
            }

            CmdT = new CommandUnit();
            XElement xml_CmdT = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdT".ToLower());
            if (xml_CmdT != null)
            {
                XElement xml_CommandUnit = xml_CmdT.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdT.GetFromXElement(xml_CommandUnit);
            }

            CmdA = new CommandUnit();
            XElement xml_CmdA = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdA".ToLower());
            if (xml_CmdA != null)
            {
                XElement xml_CommandUnit = xml_CmdA.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdA.GetFromXElement(xml_CommandUnit);
            }


            CmdC = new CommandUnit();
            XElement xml_CmdC = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdC".ToLower());
            if (xml_CmdC != null)
            {
                XElement xml_CommandUnit = xml_CmdC.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdC.GetFromXElement(xml_CommandUnit);
            }

            CmdG = new CommandUnit();
            XElement xml_CmdG = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdG".ToLower());
            if (xml_CmdG != null)
            {
                XElement xml_CommandUnit = xml_CmdG.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdG.GetFromXElement(xml_CommandUnit);
            }

            CmdACT = new CommandUnit();
            XElement xml_CmdACT = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CmdACT".ToLower());
            if (xml_CmdACT != null)
            {
                XElement xml_CommandUnit = xml_CmdACT.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == "CommandUnit".ToLower());
                CmdACT.GetFromXElement(xml_CommandUnit);
            }
            return this;
        }
        /// <summary>
        /// 命令步骤名称
        /// </summary>
        public string StepName { set; get; }
        /// <summary>
        /// 操作人
        /// </summary>
        public string Operator { set; get; }
        public System.Xml.Linq.XElement ToXElement()
        {
            XElement result = new XElement("StepInfo");
            result.Add(new XAttribute("StepName", StepName));
            result.Add(new XAttribute("Operator", Operator));

            XElement element_CmdInit = new XElement("CmdTCA");
            if (CmdTCA != null)
            {
                element_CmdInit.Add(CmdTCA.ToXElement());
            }
            result.Add(element_CmdInit);

            XElement element_CmdACN = new XElement("CmdACN");
            if (CmdACN != null)
            {
                element_CmdACN.Add(CmdACN.ToXElement());
            }
            result.Add(element_CmdACN);

            XElement element_CmdCapA = new XElement("CmdCapA");
            if (CmdCapA != null)
            {
                element_CmdCapA.Add(CmdCapA.ToXElement());
            }
            result.Add(element_CmdCapA);

            XElement element_CmdCapB = new XElement("CmdCapB");
            if (CmdCapB != null)
            {
                element_CmdCapB.Add(CmdCapB.ToXElement());
            }
            result.Add(element_CmdCapB);

            XElement element_CmdO = new XElement("CmdO");
            if (CmdO != null)
            {
                element_CmdO.Add(CmdO.ToXElement());
            }
            result.Add(element_CmdO);

            XElement element_CmdT = new XElement("CmdT");
            if (CmdT != null)
            {
                element_CmdT.Add(CmdT.ToXElement());
            }
            result.Add(element_CmdT);

            XElement element_CmdA = new XElement("CmdA");
            if (CmdA != null)
            {
                element_CmdA.Add(CmdA.ToXElement());
            }
            result.Add(element_CmdA);

            XElement element_CmdC = new XElement("CmdC");
            if (CmdC != null)
            {
                element_CmdC.Add(CmdC.ToXElement());
            }
            result.Add(element_CmdC);

            XElement element_CmdG = new XElement("CmdG");
            if (CmdG != null)
            {
                element_CmdG.Add(CmdG.ToXElement());
            }
            result.Add(element_CmdG);

            XElement element_CmdACT = new XElement("CmdACT");
            if (CmdACT != null)
            {
                element_CmdACT.Add(CmdACT.ToXElement());
            }
            result.Add(element_CmdACT);

            return result;
        }

    }
    /// <summary>
    /// 命令组
    /// </summary>
    public class CommandUnit
    {
        public CommandUnit()
        {
            Number = 0;
            CmdUnitId = "";
            CmdUnitName = "";
            CycleNumber = 1;
            IsEnable = true;
            NodeInfos = new List<NodeInfo>();
            Times = 0;
        }

        /// <summary>
        /// 编号
        /// </summary>
        public int Number { set; get; }

        /// <summary>
        /// 命令单元ID
        /// </summary>
        public string CmdUnitId { set; get; }

        /// <summary>
        /// 命令单元名称
        /// </summary>
        public string CmdUnitName { set; get; }
        /// <summary>
        /// 循环数
        /// </summary>
        public int CycleNumber { set; get; }

        /// <summary>
        /// 使能
        /// </summary>
        public bool IsEnable { set; get; }

        public List<NodeInfo> NodeInfos { set; get; }

        /// <summary>
        /// 总耗时 单位s
        /// </summary>
        public decimal Times { set; get; }

        #region IObjectSerialize Members

        public System.Xml.Linq.XElement ToXElement()
        {
            XElement result = new XElement("CommandUnit");

            result.Add(new XAttribute("Number", Number));

            result.Add(new XAttribute("CmdUnitId", CmdUnitId));
            result.Add(new XAttribute("CmdUnitName", CmdUnitName));
            result.Add(new XAttribute("CycleNumber", CycleNumber));

            result.Add(new XAttribute("IsEnable", IsEnable));

            result.Add(new XAttribute("Times", Times));

            if (NodeInfos != null && NodeInfos.Count > 0)
            {
                XElement xElementList = new XElement("NodeInfos");
                foreach (var item in NodeInfos)
                {
                    xElementList.Add(item.ToXElement());
                }
                result.Add(xElementList);
            }
            return result;
        }

        public object GetFromXElement(System.Xml.Linq.XElement xml)
        {

            Number = XMLHelper.GetAttributeIntValue(xml, "Number", 0);

            CmdUnitId = XMLHelper.GetAttributeStringValue(xml, "CmdUnitId");
            CmdUnitName = XMLHelper.GetAttributeStringValue(xml, "CmdUnitName");
            CycleNumber = XMLHelper.GetAttributeIntValue(xml, "CycleNumber", 1);

            IsEnable = XMLHelper.GetAttributeBoolValue(xml, "IsEnable");
            Times = XMLHelper.GetAttributeDecimalValue(xml, "Times", 0);
            NodeInfos.Clear();
            if (xml != null)
            {
                var xmlSelect = xml.Elements().Where(p => p.Name.LocalName.ToLower() == "NodeInfos".ToLower());
                if (xmlSelect != null)
                {
                    List<XElement> itemsElements = xmlSelect.ToList();
                    foreach (XElement element in itemsElements.Elements().ToList())
                    {
                        NodeInfo item = new NodeInfo();
                        item.GetFromXElement(element);
                        NodeInfos.Add(item);
                    }
                }
            }
            return this;
        }
        #endregion
    }
    /// <summary>
    /// 节点信息
    /// </summary>
    public class NodeInfo
    {
        public NodeInfo()
        {
            csNodeMode = NodeModel.LoopMode;
            IsEnabled = true;

            NodeName = "";
            NodeId = UtilsFun.GetNewGuid();
            ParentNodeId = "0";
        }
        public NodeModel csNodeMode { set; get; }
        public bool IsEnabled { set; get; }
        /// <summary>
        /// 节点文本
        /// </summary>
        public string NodeName { set; get; }
        /// <summary>
        /// 当前节点
        /// </summary>
        public string NodeId
        {
            get;
            set;
        }
        // 父节点
        public string ParentNodeId
        {
            get;
            set;
        }

        public XElement XInfo { get; set; }

        public string GetNodeName = "";
        public object objParent
        {
            get
            {
                if (XInfo != null)
                {
                    if (csNodeMode == NodeModel.CommonMode)
                    {
                        CommonMode mode = new CommonMode();
                        mode.GetFromXElement(XInfo);
                        GetNodeName = mode.GetNodeName;
                        return mode;
                    }
                    else if (csNodeMode == NodeModel.WaitMode)
                    {
                        WaitMode mode = new WaitMode();
                        mode.GetFromXElement(XInfo);
                        GetNodeName = mode.GetNodeName;
                        return mode;
                    }
                    else if (csNodeMode == NodeModel.LoopMode)
                    {
                        LoopMode mode = new LoopMode();
                        mode.GetFromXElement(XInfo);
                        GetNodeName = mode.GetNodeName;
                        return mode;
                    }

                }
                return null;
            }
        }

        #region IObjectSerialize 成员

        public virtual System.Xml.Linq.XElement ToXElement()
        {
            XElement result = new XElement("NodeInfo");
            result.Add(new XAttribute("csNodeMode", (int)csNodeMode));
            result.Add(new XAttribute("IsEnabled", IsEnabled));


            result.Add(new XAttribute("NodeName", NodeName));
            result.Add(new XAttribute("NodeId", NodeId));
            result.Add(new XAttribute("ParentNodeId", ParentNodeId));
            if (XInfo != null)
            {
                result.Add(XInfo);
            }
            return result;
        }

        public virtual object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            csNodeMode = (NodeModel)XMLHelper.GetAttributeIntValue(xml, "csNodeMode", 0);
            IsEnabled = XMLHelper.GetAttributeBoolValue(xml, "IsEnabled");

            NodeName = XMLHelper.GetAttributeStringValue(xml, "NodeName");
            NodeId = XMLHelper.GetAttributeStringValue(xml, "NodeId");
            ParentNodeId = XMLHelper.GetAttributeStringValue(xml, "ParentNodeId");
            XInfo = xml.Elements().FirstOrDefault(p => p.Name.LocalName.ToLower() == csNodeMode.ToString().ToLower());

            return this;
        }
        #endregion
    }
    /// <summary>
    /// 命令 通用模型
    /// </summary>
    public class CommonMode : TreeNodeInfo
    {
        public CommonMode() : base()
        {
            csNodeMode = NodeModel.CommonMode;
            IsReadAnswerData = true;
            Overtime = 15;
            WaitTime = 0;
            CheckType = 1;
            CommandParas = new List<CommandPara>();
        }

        /// <summary>
        /// 读取应答数据
        /// </summary>
        public bool IsReadAnswerData { set; get; }
        /// <summary>
        /// 应答超时参数  单位是s
        /// </summary>
        public decimal Overtime { set; get; }

        /// <summary>
        /// 发送命令后的总等待时间  单位是ms
        /// </summary>
        public int WaitTime { set; get; }

        /// <summary>
        /// 校验类型  0:无校验  1:校验和  2:crc
        /// </summary>
        public int CheckType { set; get; }

        /// <summary>
        /// 命令
        /// </summary>
        public List<CommandPara> CommandParas { set; get; }

        public string GetNodeName
        {
            get
            {
                return string.Format("(超时:{0}s)({1})", Overtime, StrCommands10);
            }
        }
        public string StrCommands10
        {
            get
            {
                string cmd = "";
                foreach (var cmdItem in CommandParas)
                {
                    if (string.IsNullOrEmpty(cmdItem.ParaName))
                    {
                        continue;
                    }
                    cmd += cmdItem.ParaName + ";";
                }
                return cmd == "" ? "" : cmd.Remove(cmd.Count() - 1, 1);
            }
        }

        #region IObjectSerialize Members

        public override System.Xml.Linq.XElement ToXElement()
        {
            XElement result = base.ToXElement();// new XElement("CommonMode");
            result.Add(new XAttribute("IsReadAnswerData", IsReadAnswerData));
            result.Add(new XAttribute("Overtime", Overtime));
            result.Add(new XAttribute("WaitTime", WaitTime));
            result.Add(new XAttribute("CheckType", CheckType));

            if (CommandParas != null && CommandParas.Count > 0)
            {
                XElement xElementList = new XElement("CommandParas");
                foreach (var item in CommandParas)
                {
                    xElementList.Add(item.ToXElement());
                }
                result.Add(xElementList);
            }

            return result;
        }

        public override object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            base.GetFromXElement(xml);
            IsReadAnswerData = XMLHelper.GetAttributeBoolValue(xml, "IsReadAnswerData");
            Overtime = XMLHelper.GetAttributeDecimalValue(xml, "Overtime", 2);
            WaitTime = XMLHelper.GetAttributeIntValue(xml, "WaitTime", 0);
            CheckType = XMLHelper.GetAttributeIntValue(xml, "CheckType", 0);
            IsEnabled = XMLHelper.GetAttributeBoolValue(xml, "IsEnabled");
            CommandParas.Clear();
            List<XElement> itemsElements = xml.Elements().Where(p => p.Name.LocalName.ToLower() == "CommandParas".ToLower()).ToList();
            foreach (XElement element in itemsElements.Elements().ToList())
            {
                CommandPara item = new CommandPara();
                item.GetFromXElement(element);
                CommandParas.Add(item);
            }
            return this;
        }
        #endregion
    }
    /// <summary>
    /// 指令参数
    /// </summary>
    public class CommandPara
    {
        public CommandPara()
        {
            ParaName = "";
        }

        /// <summary>
        /// 编号
        /// </summary>
        public int Number { set; get; }
        /// <summary>
        /// 参数名称
        /// </summary>
        public string ParaName { set; get; }

        /// <summary>
        /// 参数类型  0:数值   1:校准  2:Assi
        /// </summary>
        public int ParaType { set; get; }
        #region IObjectSerialize

        public System.Xml.Linq.XElement ToXElement()
        {
            XElement result = new XElement("CommandPara");
            result.Add(new XAttribute("Number", Number));
            result.Add(new XAttribute("ParaName", ParaName));
            result.Add(new XAttribute("ParaType", ParaType));
            return result;
        }

        public object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            Number = XMLHelper.GetAttributeIntValue(xml, "Number", 0);
            ParaName = XMLHelper.GetAttributeStringValue(xml, "ParaName");
            ParaType = XMLHelper.GetAttributeIntValue(xml, "ParaType", 0);
            return this;
        }
        #endregion  


    }
    public class LoopMode : TreeNodeInfo
    {
        public LoopMode() : base()
        {
            csNodeMode = NodeModel.LoopMode;
            CycleNumber = 1;
        }
        /// <summary>
        /// 循环数
        /// </summary>
        public int CycleNumber { set; get; }

        public string GetNodeName
        {
            get
            {
                string str = "";
                str = string.Format("循环数:{0}", CycleNumber);

                return str;
            }
        }
        #region IObjectSerialize Members
        public override System.Xml.Linq.XElement ToXElement()
        {
            XElement result = base.ToXElement();
            result.Add(new XAttribute("CycleNumber", CycleNumber));
            return result;
        }

        public override object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            base.GetFromXElement(xml);
            CycleNumber = XMLHelper.GetAttributeIntValue(xml, "CycleNumber", 1);
            return this;
        }

        #endregion
    }
    public class WaitMode : TreeNodeInfo
    {
        public WaitMode() : base()
        {
            csNodeMode = NodeModel.WaitMode;
            NodeName = "WaitNode";
            WaitTime = 100;
        }
        /// <summary>
        /// 发送命令后的总等待时间  单位是ms
        /// </summary>
        public int WaitTime { set; get; }

        public string GetNodeName
        {
            get
            {
                string str = string.Format(LangHelper.LangRes("WaittingTime") + "{0}ms", WaitTime);
                if (WaitTime >= 1000 && WaitTime < 60000)
                {
                    str = string.Format(LangHelper.LangRes("WaittingTime") + "{0}s", Math.Round((double)WaitTime / 1000, 2));
                }
                else if (WaitTime >= 60000 && WaitTime < 3600000)
                {
                    str = string.Format(LangHelper.LangRes("WaittingTime") + "{0}min", Math.Round((double)WaitTime / 1000 / 60, 2));
                }
                else if (WaitTime >= 3600000 && WaitTime < 86400000)
                {
                    str = string.Format(LangHelper.LangRes("WaittingTime") + "{0}" + LangHelper.LangRes("hour"), Math.Round((double)WaitTime / 1000 / 60 / 60, 2));
                }
                return str;
            }
        }
        #region IObjectSerialize Members

        public override System.Xml.Linq.XElement ToXElement()
        {
            XElement result = base.ToXElement();
            result.Add(new XAttribute("WaitTime", WaitTime));
            return result;
        }

        public override object GetFromXElement(System.Xml.Linq.XElement xml)
        {
            base.GetFromXElement(xml);
            WaitTime = XMLHelper.GetAttributeIntValue(xml, "WaitTime", 1);
            return this;
        }
        #endregion
    }
    #endregion
}
