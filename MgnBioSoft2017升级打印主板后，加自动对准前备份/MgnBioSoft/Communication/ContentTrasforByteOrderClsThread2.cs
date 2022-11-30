using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Communication
{
    public static class ContentTrasforByteOrderClsThread2
    {


        public static byte[] AddPumpOrder1(string content, int device_id)
        {
            string[] array1 = content.Split(',');
            string s = string.Format("/{0:D}", device_id);
            string hole = array1[1];//"O" + comboBox_H.SelectedItem.ToString();
            s += hole;
            string speed = array1[2];//"V" + textBox_V.Text.ToString();
            s += speed;
            string offset = array1[3]; //"A" + textBox_A.Text.ToString();
            s += offset;
            //string position = "";
            s += "R\r";
            //device_info.add_valve_pump_send_cammand_list("01", s);
            byte[] buf = Encoding.ASCII.GetBytes(s);
            string step_name = string.Format("泵{0:D}", device_id);
            return buf;
        }


        public static byte[] AddValveOrder1(int device_id, int target_hole)
        {

            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0xA4, 0x00, 0x00, 0xdd };
            //int target_hole = int.Parse(comboBoxFH.SelectedItem.ToString());
            int up_target_hole = target_hole - 1;
            if (target_hole == 0)
            {
                up_target_hole = target_hole + 1;
            }
            package_content1[3] = (byte)target_hole;
            package_content1[4] = (byte)up_target_hole;

            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            string step_name = string.Format("阀{0:D},开{1:D}", device_id, target_hole);
            return package_content_crc1;
            //AddRunList(device_id, step_name, 0, 0, 0, package_content_crc1);


        }



        public static byte[] CoilOFFAndON1(ushort device_id, UInt16 _sta)
        {
            byte[] Send_order = WriteCoil_05H1(4, device_id, _sta);
            return Send_order;
        }

        public static byte[] WriteCoil_05H1(byte _addr, UInt16 _reg, UInt16 _sta)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[8];

            Tx_Buf[TxCount++] = _addr;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x0F;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)(_reg >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(_reg);           /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)(_sta >> 8);      /* 线圈(bit)个数 高字节 */
            Tx_Buf[TxCount++] = (byte)(_sta);           /* 线圈(bit)个数 低字节 */

            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }

        //https://blog.csdn.net/xukai871105/article/details/16368567
        /** 
        * 函数功能: 写多个线圈状态(CoilStatue)
        * 输入参数: _addr:从站地址,_reg:寄存器地址,_length:待写入的线圈数量,
        * 返 回 值: 无
        * 说    明: 填充数据发送缓存区,然后发送
        */
        public static byte[] WriteCoil_0FH1(byte _addr, UInt16 _reg, UInt16 _length, UInt16 _data)
        {
            UInt16 crc = 0;

            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[30];

            Tx_Buf[TxCount++] = _addr;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x0F;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)(_reg >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(_reg);           /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)(_length >> 8);      /* 数据长度 高字节 */
            Tx_Buf[TxCount++] = (byte)(_length);           /* 数据长度 低字节 */
            //Tx_Buf[TxCount++] = (byte)(_length);           /*  数据字节数 */
            Tx_Buf[TxCount++] = 2;           /*  数据字节数 */


            Tx_Buf[TxCount++] = (byte)(_data);            /*  低字节 */
            Tx_Buf[TxCount++] = (byte)(_data >> 8);       /* 寄存器(16bits)个数 高字节 */

            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }




    }
}
