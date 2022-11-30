using System;
public static class FreeModbus
{

    //公有变量
    public static ushort Coil_ON = 0xFF00;
    public static ushort Coil_OFF = 0x0000;
    /* 私有变量 ------------------------------------------------------------------*/
    public static UInt16 Rx_MSG;   // 接收报文状态

    public static byte[] Tx_Buf = new byte[20];// 发送缓存,最大256字节
    public static byte[] Rx_Buf = new byte[200];// 接收缓存,最大256字节
    public static UInt16 TxCount = 0;
    public static UInt16 RxCount = 0;


    // CRC 高位字节值表
    public static byte[] auchCRCHi = new byte[] {
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40, 0x00, 0xC1, 0x81, 0x40,
                0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0, 0x80, 0x41, 0x00, 0xC1,
                0x81, 0x40, 0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41,
                0x00, 0xC1, 0x81, 0x40, 0x01, 0xC0, 0x80, 0x41, 0x01, 0xC0,
                0x80, 0x41, 0x00, 0xC1, 0x81, 0x40
        };

    // CRC 低位字节值表
    public static byte[] auchCRCLo = new byte[] {
                0x00, 0xC0, 0xC1, 0x01, 0xC3, 0x03, 0x02, 0xC2, 0xC6, 0x06,
                0x07, 0xC7, 0x05, 0xC5, 0xC4, 0x04, 0xCC, 0x0C, 0x0D, 0xCD,
                0x0F, 0xCF, 0xCE, 0x0E, 0x0A, 0xCA, 0xCB, 0x0B, 0xC9, 0x09,
                0x08, 0xC8, 0xD8, 0x18, 0x19, 0xD9, 0x1B, 0xDB, 0xDA, 0x1A,
                0x1E, 0xDE, 0xDF, 0x1F, 0xDD, 0x1D, 0x1C, 0xDC, 0x14, 0xD4,
                0xD5, 0x15, 0xD7, 0x17, 0x16, 0xD6, 0xD2, 0x12, 0x13, 0xD3,
                0x11, 0xD1, 0xD0, 0x10, 0xF0, 0x30, 0x31, 0xF1, 0x33, 0xF3,
                0xF2, 0x32, 0x36, 0xF6, 0xF7, 0x37, 0xF5, 0x35, 0x34, 0xF4,
                0x3C, 0xFC, 0xFD, 0x3D, 0xFF, 0x3F, 0x3E, 0xFE, 0xFA, 0x3A,
                0x3B, 0xFB, 0x39, 0xF9, 0xF8, 0x38, 0x28, 0xE8, 0xE9, 0x29,
                0xEB, 0x2B, 0x2A, 0xEA, 0xEE, 0x2E, 0x2F, 0xEF, 0x2D, 0xED,
                0xEC, 0x2C, 0xE4, 0x24, 0x25, 0xE5, 0x27, 0xE7, 0xE6, 0x26,
                0x22, 0xE2, 0xE3, 0x23, 0xE1, 0x21, 0x20, 0xE0, 0xA0, 0x60,
                0x61, 0xA1, 0x63, 0xA3, 0xA2, 0x62, 0x66, 0xA6, 0xA7, 0x67,
                0xA5, 0x65, 0x64, 0xA4, 0x6C, 0xAC, 0xAD, 0x6D, 0xAF, 0x6F,
                0x6E, 0xAE, 0xAA, 0x6A, 0x6B, 0xAB, 0x69, 0xA9, 0xA8, 0x68,
                0x78, 0xB8, 0xB9, 0x79, 0xBB, 0x7B, 0x7A, 0xBA, 0xBE, 0x7E,
                0x7F, 0xBF, 0x7D, 0xBD, 0xBC, 0x7C, 0xB4, 0x74, 0x75, 0xB5,
                0x77, 0xB7, 0xB6, 0x76, 0x72, 0xB2, 0xB3, 0x73, 0xB1, 0x71,
                0x70, 0xB0, 0x50, 0x90, 0x91, 0x51, 0x93, 0x53, 0x52, 0x92,
                0x96, 0x56, 0x57, 0x97, 0x55, 0x95, 0x94, 0x54, 0x9C, 0x5C,
                0x5D, 0x9D, 0x5F, 0x9F, 0x9E, 0x5E, 0x5A, 0x9A, 0x9B, 0x5B,
                0x99, 0x59, 0x58, 0x98, 0x88, 0x48, 0x49, 0x89, 0x4B, 0x8B,
                0x8A, 0x4A, 0x4E, 0x8E, 0x8F, 0x4F, 0x8D, 0x4D, 0x4C, 0x8C,
                0x44, 0x84, 0x85, 0x45, 0x87, 0x47, 0x46, 0x86, 0x82, 0x42,
                0x43, 0x83, 0x41, 0x81, 0x80, 0x40
        };

    /** 
    * 函数功能: Modbus CRC16 校验计算函数
    * 输入参数: pushMsg:待计算的数据首地址,usDataLen:数据长度
    * 返 回 值: CRC16 计算结果
    * 说    明: 计算结果是高位在前,需要转换才能发送
    */
    public static UInt16 MB_CRC16(byte[] pushMsg, UInt16 usDataLen)
    {
        byte uchCRCHi = 0xFF;
        byte uchCRCLo = 0xFF;
        UInt16 uchCount = 0;
        UInt16 uIndex;
        while (usDataLen > 0)
        {
            uIndex = (UInt16)(uchCRCLo ^ pushMsg[uchCount]);
            uchCRCLo = (byte)(uchCRCHi ^ auchCRCHi[uIndex]);
            uchCRCHi = auchCRCLo[uIndex];
            uchCount++;
            usDataLen--;
        }
        return (UInt16)((UInt16)(uchCRCHi) << 8 | uchCRCLo);
    }

    public static bool CheckCRC(byte[] data, int arrayLength)
    {
        bool result = false;
        UInt16 crc = 0;
        CRC16(data, arrayLength);
        return result;
    }
    /// <summary>
    /// CRC校验
    /// </summary>
    /// <param name="data">校验的字节数组</param>
    /// <param name="length">校验的数组长度</param>
    /// <returns>该字节数组的奇偶校验字节</returns>
    public static Int16 CRC16(byte[] data, int arrayLength)
    {
        byte CRCHigh = 0xFF;
        byte CRCLow = 0xFF;
        byte index;
        int i = 0;
        while (arrayLength-- > 0)
        {
            index = (System.Byte)(CRCHigh ^ data[i++]);
            CRCHigh = (System.Byte)(CRCLow ^ auchCRCHi[index]);
            CRCLow = auchCRCLo[index];
        }
        return (Int16)(CRCHigh << 8 | CRCLow);
    }

    /** 
    * 函数功能: 写单个线圈状态(CoilStatue)
    * 输入参数: _addr:从站地址,_reg:寄存器地址,_sta:待写入的线圈状态(0,1)
    * 返 回 值: 无
    * 说    明: 填充数据发送缓存区,然后发送
    */
    public static void WriteCoil_05H(byte _addr, UInt16 _reg, UInt16 _sta)
    {
        UInt16 crc = 0;
        TxCount = 0;

        Tx_Buf[TxCount++] = _addr;                  /* 从站地址 */
        Tx_Buf[TxCount++] = 0x05;                   /* 功能码 */
        Tx_Buf[TxCount++] = (byte)(_reg >> 8);      /* 寄存器地址 高字节 */
        Tx_Buf[TxCount++] = (byte)(_reg);           /* 寄存器地址 低字节 */
        Tx_Buf[TxCount++] = (byte)(_sta >> 8);      /* 线圈(bit)个数 高字节 */
        Tx_Buf[TxCount++] = (byte)(_sta);           /* 线圈(bit)个数 低字节 */

        crc = MB_CRC16(Tx_Buf, TxCount);
        Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
        Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */

    }

    public static void WriteCoil_0FH(byte _addr, UInt16 _reg, UInt16 _sta)
    {
        UInt16 crc = 0;
        TxCount = 0;

        Tx_Buf[TxCount++] = _addr;                  /* 从站地址 */
        Tx_Buf[TxCount++] = 0x0f;                   /* 功能码 */
        Tx_Buf[TxCount++] = (byte)(_reg >> 8);      /* 寄存器地址 高字节 */
        Tx_Buf[TxCount++] = (byte)(_reg);           /* 寄存器地址 低字节 */
        Tx_Buf[TxCount++] = (byte)(_sta >> 8);      /* 线圈(bit)个数 高字节 */
        Tx_Buf[TxCount++] = (byte)(_sta);           /* 线圈(bit)个数 低字节 */
                                                    //Tx_Buf[TxCount++] = 4;//字节数
        crc = MB_CRC16(Tx_Buf, TxCount);
        Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
        Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */


    }

    /** 
    * 函数功能: 写单个保持寄存器(HoldingRegister)
    * 输入参数: _addr:从站地址,_reg:寄存器地址,_data:待写入的寄存器数据
    * 返 回 值: 无
    * 说    明: 填充数据发送缓存区,然后发送
    */
    public static void WriteHoldingReg_06H(byte _addr, UInt16 _reg, UInt16 _data)
    {
        UInt16 crc = 0;
        TxCount = 0;
        Tx_Buf[TxCount++] = _addr;                    /* 从站地址 */
        Tx_Buf[TxCount++] = 0x06;                     /* 功能码 */
        Tx_Buf[TxCount++] = (byte)(_reg >> 8); ;      /* 寄存器地址 高字节 */
        Tx_Buf[TxCount++] = (byte)(_reg);             /* 寄存器地址 低字节 */
        Tx_Buf[TxCount++] = (byte)(_data >> 8);       /* 寄存器(16bits)个数 高字节 */
        Tx_Buf[TxCount++] = (byte)(_data);            /*  低字节 */

        crc = MB_CRC16(Tx_Buf, TxCount);
        Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
        Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
    }

    /** 
    * 函数功能: 写多个保持寄存器(HoldingRegister)
    * 输入参数: _addr:从站地址,_reg:寄存器地址,_length写入数据长度，_data[]:待写入的寄存器数据
    * 返 回 值: 无
    * 说    明: 填充数据发送缓存区,然后发送
    */
    public static void WriteHoldingReg_10H(byte _addr, UInt16 _reg, UInt16 _length, UInt16[] _data)
    {
        UInt16 crc = 0;
        TxCount = 0;
        Tx_Buf[TxCount++] = _addr;                    /* 从站地址 */
        Tx_Buf[TxCount++] = 0x10;                     /* 功能码 */
        Tx_Buf[TxCount++] = (byte)(_reg >> 8); ;      /* 寄存器地址 高字节 */
        Tx_Buf[TxCount++] = (byte)(_reg);             /* 寄存器地址 低字节 */

        Tx_Buf[TxCount++] = (byte)(_length >> 8); ;   /* 数据长度 高字节 */
        Tx_Buf[TxCount++] = (byte)(_length);          /* 数据长度 低字节 */

        Tx_Buf[TxCount++] = (byte)(_length * 2);           /*  数据字节数 */

        for (int i = 0; i < _length; i++)//写入_length长度的数据
        {
            Tx_Buf[TxCount++] = (byte)(_data[i] >> 8);       /* 寄存器(16bits)个数 高字节 */
            Tx_Buf[TxCount++] = (byte)(_data[i]);            /*  低字节 */
        }

        crc = MB_CRC16(Tx_Buf, TxCount);
        Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
        Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */

    }

    /** 
    * 函数功能: 读保持寄存器(HoldingRegister)
    * 输入参数: _addr:从站地址,_reg:寄存器地址,_num:待读取的寄存器数量
    * 返 回 值: 无
    * 说    明: 填充数据发送缓存区,然后发送
    */
    public static void ReadHoldingReg_03H(byte _addr, UInt16 _reg, UInt16 _num)
    {
        UInt16 crc = 0;
        TxCount = 0;
        Tx_Buf[TxCount++] = _addr;                    /* 从站地址 */
        Tx_Buf[TxCount++] = 0x03;                     /* 功能码 */
        Tx_Buf[TxCount++] = (byte)(_reg >> 8); ;      /* 寄存器地址 高字节 */
        Tx_Buf[TxCount++] = (byte)(_reg);             /* 寄存器地址 低字节 */
        Tx_Buf[TxCount++] = (byte)(_num >> 8);        /* 寄存器(16bits)个数 高字节 */
        Tx_Buf[TxCount++] = (byte)(_num);             /*  低字节 */

        crc = MB_CRC16(Tx_Buf, TxCount);
        Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
        Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
    }

    /** 
    * 函数功能: 读输入寄存器(InputRegister)
    * 输入参数: _addr:从站地址,_reg:寄存器地址,_num:待读取的寄存器数量
    * 返 回 值: 无
    * 说    明: 填充数据发送缓存区,然后发送
    */
    public static void ReadInputReg_04H(byte _addr, UInt16 _reg, UInt16 _num)
    {
        UInt16 crc = 0;
        TxCount = 0;
        Tx_Buf[TxCount++] = _addr;                    /* 从站地址 */
        Tx_Buf[TxCount++] = 0x04;                     /* 功能码 */
        Tx_Buf[TxCount++] = (byte)(_reg >> 8); ;      /* 寄存器地址 高字节 */
        Tx_Buf[TxCount++] = (byte)(_reg);             /* 寄存器地址 低字节 */
        Tx_Buf[TxCount++] = (byte)(_num >> 8);        /* 寄存器(16bits)个数 高字节 */
        Tx_Buf[TxCount++] = (byte)(_num);             /*  低字节 */

        crc = MB_CRC16(Tx_Buf, TxCount);
        Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
        Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
    }
}