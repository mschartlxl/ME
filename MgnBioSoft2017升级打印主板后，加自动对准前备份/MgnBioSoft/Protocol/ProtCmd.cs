using MGNBIO.CommExceptions;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Protocol
{
    /// <summary>
    /// 通信协议基类
    /// </summary>
    public abstract class ProtCmd
    {
        /// <summary>
        /// 指令的最小长度
        /// </summary>
        public const int MINI_LENGTH = 8;

        /// <summary>
        /// 指令的最大长度
        /// </summary>
        internal const int MAX_LENGTH = 255;

        /// <summary>
        /// CRC（奇偶校验码）的长度
        /// </summary>
        internal const int CRC_LENGTH = 2;

        /// <summary>
        /// 表示指令长度的参数在指令中的位置
        /// </summary>
        internal const int LENGTH_PARAMETER_INDEX = 4;

        /// <summary>
        /// 帧头
        /// </summary>
        public byte[] Head { get; set; } = new byte[2];

        /// <summary>
        /// 目标地址：下位机发送给上位机=0；上位机发送给下位机=1。
        /// </summary>
        public byte Target { get; set; }

        /// <summary>
        /// 指令ID
        /// </summary>
        public byte Identity { get; set; }

        /// <summary>
        /// 流水号,0~255
        /// </summary>
        public byte FlowId { get; set; }

        /// <summary>
        /// 指令长度
        /// </summary>
        public byte Length { get; private set; }

        /// <summary>
        /// 奇偶校验码
        /// </summary>
        public byte[] CRC { get; private set; } = new byte[CRC_LENGTH];

        /// <summary>
        /// 指令的数据
        /// </summary>
        private byte[] _data;

        /// <summary>
        /// 指令的数据
        /// </summary>
        public byte[] Data
        {
            get
            {
                _data = BuildData(_data);
                ResetLength();
                ResetCRC();
                return _data;
            }
            private set
            {
                _data = value;
                ResetLength();
                ResetCRC();
                ParseData(_data);
            }
        }

        /// <summary>
        /// 不含奇偶校验码的指令
        /// </summary>
        private byte[] _noCRC = new byte[MINI_LENGTH - CRC_LENGTH];

        /// <summary>
        /// 指令的字节数组表示
        /// </summary>
        private byte[] _bytes = new byte[MINI_LENGTH];

        /// <summary>
        /// 指令的字节数组表示
        /// </summary>
        public byte[] Bytes
        {
            get
            {
                lock (_locker)
                {
                    // 数据体长度一致时不再重新申请内存空间
                    byte[] data = Data;
                    int len = data == null ? MINI_LENGTH : data.Length + MINI_LENGTH;
                    if (_bytes.Length != len)
                    {
                        _bytes = new byte[len];
                    }
                    // 填充
                    Array.Copy(_noCRC, _bytes, _noCRC.Length);
                    Array.Copy(CRC, 0, _bytes, _noCRC.Length, CRC_LENGTH);
                }
                return _bytes;
            }
            private set
            {
                lock (_locker)
                {
                    if (value == null)
                    {
                        throw new ProtException("指令数据为空.");
                    }
                    if (value.Length < MINI_LENGTH || value.Length > MAX_LENGTH)
                    {
                        throw new ProtException("指令的长度错误.");
                    }
                    if (value.Length != value[LENGTH_PARAMETER_INDEX])
                    {
                        throw new ProtException("指令的长度与实际不符.");
                    }

                    _bytes = value;
                    Head[0] = _bytes[0];
                    Head[1] = _bytes[1];
                    Target = _bytes[2];
                    Identity = _bytes[3];
                    Length = _bytes[LENGTH_PARAMETER_INDEX];
                    FlowId = _bytes[5];
                    if (_bytes.Length > MINI_LENGTH)
                    {
                        Data = _bytes.Skip(MINI_LENGTH - CRC_LENGTH).Take(_bytes.Length - MINI_LENGTH).ToArray();
                    }
                    else
                    {
                        ResetCRC();
                    }

                    if (CRC[0] != _bytes[_bytes.Length - 2] || CRC[1] != _bytes[_bytes.Length - 1])
                    {
                        throw new ProtException("指令的CRC码错误.");
                    }
                }
            }
        }

        /// <summary>
        /// 与当前指令对应的请求或应答
        /// 当前指令是请求时，此属性为应答指令
        /// 当前指令是应答时，此属性为请求指令
        /// </summary>
        public ProtCmd Partner { get; set; }

        //public string HexString => Bytes.ToHexString();

        private readonly object _locker = new object();

        public ProtCmd()
        {
        }

        public ProtCmd(byte[] bytes)
            : this()
        {
            Bytes = bytes;
            Validate();
        }

        private void ResetLength()
        {
            Length = (byte)(MINI_LENGTH + (_data == null ? 0 : _data.Length));
        }

        private void ResetCRC()
        {
            // 数据体长度一致时不再重新申请内存空间
            int len = Length - CRC_LENGTH;
            if (_noCRC.Length != len)
            {
                _noCRC = new byte[len];
            }
            // 填充
            _noCRC[0] = Head[0];
            _noCRC[1] = Head[1];
            _noCRC[2] = Target;
            _noCRC[3] = Identity;
            _noCRC[LENGTH_PARAMETER_INDEX] = Length;
            _noCRC[5] = FlowId;
            if (_data != null)
            {
                Array.Copy(_data, 0, _noCRC, MINI_LENGTH - CRC_LENGTH, _data.Length);
            }
            // 计算CRC码
            CRC = MGNBIO.Check.CRC.Check(_noCRC);
        }

        /// <summary>
        /// 验证
        /// </summary>
        protected virtual void Validate()
        {
        }

        /// <summary>
        /// 构建指令的数据
        /// </summary>
        /// <param name="data">构建前的指令数据</param>
        /// <returns>构建后的指令数据</returns>
        protected abstract byte[] BuildData(byte[] data);
        //{
        //    // 数据体长度一致时不再重新申请内存空间
        //    var len = 当前指令的数据体的长度;
        //    if (data == null || data.Length != len)
        //        data = new byte[len];
        //    // ...
        //    return data;
        //}

        /// <summary>
        /// 解析数据
        /// </summary>
        /// <param name="data">指令数据</param>
        protected abstract void ParseData(byte[] data);
        //{
        //    if (data == null || data.Length < 当前指令的数据体的长度)
        //        throw new ProtException("指令数据长度不符");
        //}
    }
}
