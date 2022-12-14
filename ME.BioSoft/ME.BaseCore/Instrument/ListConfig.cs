using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Linq;

namespace ME.BaseCore.Instrument
{
    public class ListConfig
    {
        private ListConfig()
        {
            ReadConfig();
        }

        private static ListConfig _instance;
        private static readonly object PadLock = new object();

        public static ListConfig GetInstance()
        {
            if (_instance == null)
            {
                lock (PadLock)
                {
                    if (_instance == null)
                    {
                        _instance = new ListConfig();

                    }
                }
            }
            return _instance;
        }

        public  List<ComBoxItem> ListPumpNumber { get; set; } = new List<ComBoxItem>();
        public List<ComBoxItem> ListReCircleNumber { get; set; } = new List<ComBoxItem>();
        public List<ComBoxItem> ListSwitchNumber { get; set; } = new List<ComBoxItem>();
        public List<ComBoxItem> ListZAxisNumber { get; set; } = new List<ComBoxItem>();
        public  void ReadConfig()
        {
            var serialcfg = AppDomain.CurrentDomain.BaseDirectory + "Config/ConfigList.xml";
            if (System.IO.File.Exists(serialcfg))
            {
                XElement instrumentEEs = XElement.Load(serialcfg);
                List<XElement> instrumentXES = instrumentEEs.Elements().ToList();
                foreach (XElement instrumentEE in instrumentXES)
                {
                    var paramNum = XMLHelper.GetAttributeStringValue(instrumentEE, "Name");
                    var paramType = XMLHelper.GetAttributeStringValue(instrumentEE, "Type");
                    if (paramNum == "Pump")
                    {
                        ListPumpNumber = GetElement(instrumentEE,paramType);
                    }
                    if (paramNum == "ReCircle")
                    {
                        ListReCircleNumber = GetElement(instrumentEE, paramType);
                    }
                    if (paramNum == "Switch")
                    {
                        ListSwitchNumber = GetElement(instrumentEE,paramType);
                    }
                    if (paramNum == "ZAxis")
                    {
                        ListZAxisNumber = GetElement(instrumentEE, paramType);
                    }
                    
                }
            }
        }
        public static List<ComBoxItem> GetElement(XElement instrumentEE,string type)
        {
            List<ComBoxItem> comBoxItems = new List<ComBoxItem>();
            List<XElement> paramXES = instrumentEE.Elements().ToList();
          
            foreach (XElement param in paramXES)
            {
                ComBoxItem comBoxItem = new ComBoxItem();
                comBoxItem.Name = param.Value;
                comBoxItem.Number=Convert.ToInt32( param.Value);
                comBoxItem.Type = Convert.ToInt32(type);
                comBoxItems.Add(comBoxItem);
            }
            return comBoxItems;
        }
    }
}
