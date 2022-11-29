using System;
using System.Collections.Generic;
using System.IO.Ports;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Xml.Serialization;

namespace ME.BaseCore
{
    public class SerialConfig
    {
        public SerialConfig()
        {
        }
        private string _serialPort;

        public string SerialPort
        {
            set
            {
                if (this._serialPort != value)
                {
                    this._serialPort = value;
                }
            }
            get
            {
                return this._serialPort;
            }
        }

        private int _bautRate;

        public int BautRate
        {
            set
            {
                if (this._bautRate != value)
                {
                    this._bautRate = value;
                }
            }
            get
            {
                return this._bautRate;
            }
        }

        private Parity _parity;

        public Parity Parity
        {
            set
            {
                if (this._parity != value)
                {
                    this._parity = value;
                }
            }
            get
            {
                return this._parity;
            }
        }

        private int _dataBits;
 
        public int DataBits
        {
            set
            {
                if (this._dataBits != value)
                {
                    this._dataBits = value;
                }
            }
            get
            {
                return this._dataBits;
            }
        }

        private StopBits _stopBits;

        public StopBits StopBits
        {
            set
            {
                if (this._stopBits != value)
                {
                    this._stopBits = value;
                }
            }
            get
            {
                return this._stopBits;
            }
        }

    }
}
