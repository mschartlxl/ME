using MGNBIO.Model;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Communication
{
    public static class ContentTrasforByteOrderCls
    {
        /// <summary>
        /// 
        /// </summary>
        public static int iaaa = 0; 
        /// <summary>
        /// 运行指令
        /// </summary>
        /// <param name="Order_content"></param>
        /// <returns></returns>
        public static byte[] ContentTrasforByteOrder(string Order_content)
        {
            //string save_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
            //Console.Write(iaaa + "发送指令：" + Order_content + " time:" + save_time + "\r\n");
            //Console.Write(iaaa + ":" + Order_content + "\r\n");
            //iaaa++;
            byte[] order_content_byte_array = new byte[8];
            string order_sz = Order_content;
            int device_id = -1;
            int hole = -1;
            int speed = -1;
            int offset = -1;
            //int switch_s = -1;

            if (order_sz.Length > 0)
            {
                string[] order_sz_array_switch = order_sz.Split(':');
                if (order_sz_array_switch.Length == 2)
                {
                    //电磁阀和开关和延时
                    int a = int.Parse(order_sz_array_switch[1]);
                    string b = order_sz_array_switch[0];
                    int delay_time = a;

                    if (b.Equals("iDelay"))
                    {
                        //添加延时动作
                        //AddRunOrderList(delay_time + "", 1000);
                        device_id = delay_time;
                    }
                    else
                    {
                        byte[] a_array = ConvertHex4(a);
                        //添加开动作
                        order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH_New1(4, 31, 4, a_array);//填充写单个线圈寄存器//CoilOFFAndON(33, a);
                    }
                }
                else
                {
                    string[] order_sz_array = order_sz.Split(',');

                    if (order_sz_array[0].Length > 1)
                    {
                        device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                    }

                    if (order_sz_array[1].Length > 1)
                    {
                        hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                    }

                    if (order_sz_array[2].Length > 1)
                    {
                        speed = int.Parse(order_sz_array[2].Substring(1, order_sz_array[2].Length - 1));
                    }

                    if (order_sz_array[3].Length > 1)
                    {
                        offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                    }

                    //if (order_sz_array[4].Length > 1)
                    //{
                    //    switch_s = int.Parse(order_sz_array[4].Substring(1, order_sz_array[4].Length - 1));
                    //}

                    //泵
                    if (device_id > 2 && device_id < 21)
                    {
                        order_content_byte_array = AddPumpOrder(order_sz, device_id);
                    }
                    //阀
                    //if (device_id > 2 && device_id < 5)
                    if (device_id < 3)
                    {
                        int target_hole = -1;
                        string[] order_sz_array1 = order_sz.Split(',');
                        if (order_sz_array1[1].Length > 1)
                        {
                            target_hole = int.Parse(order_sz_array1[1].Substring(1, order_sz_array1[1].Length - 1));
                        }
                        order_content_byte_array = AddValveOrder(device_id, target_hole);
                        //AddRunOrderList(order_sz, device_id);
                    }
                }
            }
            return order_content_byte_array;
        }

        private static byte[] ConvertHex4(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[4];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            hex[2] = (byte)((velocity >> 16) & 0xff);
            hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }
        //linshixxxxxx
        private static byte[] ConverHex4Speed(int vel)
        {
            TokenValue value = new TokenValue();
            value.f_data = vel;
            byte[] hex = new byte[4];
            hex[3] = (byte)(value.u_data & 0xff);
            hex[2] = (byte)((value.u_data >> 8) & 0xff);   //先右移再与操作
            hex[1] = (byte)((value.u_data >> 16) & 0xff);
            hex[0] = (byte)((value.u_data >> 24) & 0xff);
            return hex;
        }

        //float 数据变为 byte 数组
        private static byte[] ToByte(float data)
        {
            unsafe
            {
                byte* pdata = (byte*)&data;
                byte[] byteArray = new byte[sizeof(float)];
                for (int i = 0; i < sizeof(float); ++i)
                    byteArray[i] = *pdata++;
                return byteArray;
            }
        }

        private static byte[] ConverHex4Speed(float vel)
        {
            ToByte(vel);
            TokenValue value = new TokenValue();
            value.f_data = vel;
            byte[] hex = new byte[4];
            hex[3] = (byte)(value.u_data & 0xff);
            hex[2] = (byte)((value.u_data >> 8) & 0xff);   //先右移再与操作
            hex[1] = (byte)((value.u_data >> 16) & 0xff);
            hex[0] = (byte)((value.u_data >> 24) & 0xff);
            return hex;
        }

        /// <summary>
        /// 生成查询指令
        /// </summary>
        /// <param name="Order_content"></param>
        /// <returns></returns>
        public static byte[] QueryContentTrasforByteOrder(string Order_content)
        {
            //string save_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
            //Console.Write(iaaa + "发送指令：" + Order_content + " time:" + save_time + "\r\n");
            //Console.Write(iaaa + ":" + Order_content + "\r\n");
            //iaaa++;
            byte[] order_content_byte_array = new byte[8];
            string order_sz = Order_content;
            int device_id = -1;
            int hole = -1;
            int speed = -1;
            int offset = -1;
            //int switch_s = -1;
            if (order_sz.Length > 0)
            {
                string[] order_sz_array_switch = order_sz.Split(':');
                if (order_sz_array_switch.Length == 2)
                {
                    //电磁阀和开关和延时
                    UInt16 a = UInt16.Parse(order_sz_array_switch[1]);
                    string b = order_sz_array_switch[0];
                    int delay_time = a;

                    if (b.Equals("iDelay"))
                    {
                        //添加延时动作
                        //AddRunOrderList(delay_time + "", 1000);
                        device_id = delay_time;
                    }
                    else
                    {
                        //添加开动作
                        order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH(4, 31, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);
                    }
                }
                else
                {
                    string[] order_sz_array = order_sz.Split(',');

                    if (order_sz_array[0].Length > 1)
                    {
                        device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                    }

                    if (order_sz_array[1].Length > 1)
                    {
                        hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                    }

                    if (order_sz_array[2].Length > 1)
                    {
                        speed = int.Parse(order_sz_array[2].Substring(1, order_sz_array[2].Length - 1));
                    }

                    if (order_sz_array[3].Length > 1)
                    {
                        offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                    }

                    //if (order_sz_array[4].Length > 1)
                    //{
                    //    switch_s = int.Parse(order_sz_array[4].Substring(1, order_sz_array[4].Length - 1));
                    //}

                    //泵
                    if (device_id > 2 && device_id < 21)
                    {
                        order_content_byte_array = AddPumpOrder(order_sz, device_id);
                    }

                    //阀
                    //if (device_id > 2 && device_id < 5)
                    if (device_id < 3)
                    {
                        int target_hole = -1;
                        string[] order_sz_array1 = order_sz.Split(',');
                        if (order_sz_array1[1].Length > 1)
                        {
                            target_hole = int.Parse(order_sz_array1[1].Substring(1, order_sz_array1[1].Length - 1));
                        }
                        order_content_byte_array = AddValveOrder(device_id, target_hole);
                        //AddRunOrderList(order_sz, device_id);
                    }
                }
            }
            return order_content_byte_array;
        }


        public static byte[] AddPumpOrder(string content, int device_id)
        {
            string[] array1 = content.Split(',');
            int offset = int.Parse(array1[3].Substring(1, array1[3].Length - 1));
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4E, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] tmp_array = ConvertHex2(offset);
            package_content1[3] = tmp_array[0];
            package_content1[4] = tmp_array[1];
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            return package_content_crc1;
        }

        private static byte[] ConvertHex2(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[2];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            //hex[2] = (byte)((velocity >> 16) & 0xff);
            //hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

        public static byte[] AddValveOrder(int device_id, int target_hole)
        {

            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0xA4, 0x00, 0x00, 0xdd };
            //int target_hole = int.Parse(comboBoxFH.SelectedItem.ToString());
            int up_target_hole = target_hole - 1;
            if (up_target_hole == 0)
            {
                up_target_hole = target_hole + 1;
            }


            package_content1[1] = (byte)device_id;
            package_content1[3] = (byte)target_hole;
            package_content1[4] = (byte)up_target_hole;

            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);

            //Console.Write(iaaa + ":" + package_content1);

            string step_name = string.Format("阀{0:D},开{1:D}", device_id, target_hole);
            return package_content_crc1;
            //AddRunList(device_id, step_name, 0, 0, 0, package_content_crc1);


        }



        public static byte[] CoilOFFAndON(ushort device_id, UInt16 _sta)
        {
            byte[] Send_order = WriteCoil_05H(4, device_id, _sta);
            return Send_order;
        }

        public static byte[] WriteCoil_05H(byte _addr, UInt16 _reg, UInt16 _sta)
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
        public static byte[] WriteCoil_0FH(byte _addr, UInt16 _reg, UInt16 _length, UInt16 _data)
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

        /** 
        * 函数功能: 写多个线圈状态(CoilStatue)
        * 输入参数: _addr:从站地址,_reg:寄存器地址,_length:待写入的线圈数量,_data:待写入数据
        * 返 回 值: 无
        * 说    明: 填充数据发送缓存区,然后发送
        */
        public static byte[] WriteCoil_0FH_New1(byte _addr, UInt16 _reg, UInt16 _length, Byte[] _data)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[13];
            Tx_Buf[TxCount++] = _addr;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x0F;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)(_reg >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(_reg);           /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)((_length * 8) >> 8);      /* 数据bit长度 高字节 */
            Tx_Buf[TxCount++] = (byte)(_length * 8);           /* 数据bit长度 低字节 */
            Tx_Buf[TxCount++] = (byte)(_length);           /*  数据字节数 */
            for (int i = 0; i < _length; i++)//写入_length长度的数据
            {
                Tx_Buf[TxCount++] = (byte)(_data[i]);            /*  数据 */
            }
            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }
        //01 10 00 01   00 02     04   88 B8 00 00 99 E6
        public static byte[] WriteCoil_0FH10(byte device_id, UInt16 data_length,int start_addr,int end_addr,int fun_code, UInt16 byte_length, int _data)
        {
            UInt16 crc = 0;

            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[30];

            Tx_Buf[TxCount++] = device_id;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x10;                   /* 功能码 */

            Tx_Buf[TxCount++] = (byte)start_addr;
            Tx_Buf[TxCount++] = (byte)end_addr;

            Tx_Buf[TxCount++] = (byte)(data_length >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(data_length);           /* 寄存器地址 低字节 */

            //Tx_Buf[TxCount++] = (byte)(byte_length >> 8);      /* 数据长度 高字节 */
            Tx_Buf[TxCount++] = (byte)(byte_length);           /* 数据长度 低字节 */

            //1 61A0
            //90528
            //Tx:01 10 00 01 00 02 04 61 A0 00 01 EC 7D
            byte[] data_array = ConvertHex4(_data);

            Tx_Buf[TxCount++] = data_array[1];//(byte)(_data >> 8);    /* 寄存器(16bits)个数 高字节 */
            Tx_Buf[TxCount++] = data_array[0];//(byte)(_data);       /*  低字节 */

            Tx_Buf[TxCount++] = data_array[3];//(byte)0x00;       /*  低字节 */
            Tx_Buf[TxCount++] = data_array[2];//(byte)0x00;       /*  低字节 */


            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }


        public static byte[] WriteCoil_0FH06_1(byte device_id, UInt16 _reg,int start_addr,int end_addr,int fun_code)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[13];
            Tx_Buf[TxCount++] = device_id;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x06;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)start_addr;
            Tx_Buf[TxCount++] = (byte)end_addr;


            Tx_Buf[TxCount++] = (byte)(fun_code);           /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)(fun_code >> 8);      /* 寄存器地址 高字节 */


            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }
        public static byte[] WriteCoil_0FH06_Speed_Set(byte device_id, UInt16 data_length, int start_addr, int end_addr, int fun_code, UInt16 byte_length, float _data)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[30];
            Tx_Buf[TxCount++] = device_id;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x10;                   /* 功能码 */
            //01 10 00 03   00 02     04   00 00 47 7A   01 A9
            Tx_Buf[TxCount++] = (byte)start_addr;
            Tx_Buf[TxCount++] = (byte)end_addr;
            Tx_Buf[TxCount++] = (byte)(data_length >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(data_length);           /* 寄存器地址 低字节 */
            //Tx_Buf[TxCount++] = (byte)(byte_length >> 8);      /* 数据长度 高字节 */
            Tx_Buf[TxCount++] = (byte)(byte_length);           /* 数据长度 低字节 */
            //1 61A0
            //90528
            //Tx:01 10 00 01 00 02 04 61 A0 00 01 EC 7D
            byte[] data_array = ConverHex4Speed(_data);
            //Tx_Buf[TxCount++] = data_array[0];//(byte)0x00;       /*  低字节 */
            //Tx_Buf[TxCount++] = data_array[1];//(byte)0x00;       /*  低字节 */
            //Tx_Buf[TxCount++] = data_array[2];//(byte)(_data >> 8);    /* 寄存器(16bits)个数 高字节 */
            //Tx_Buf[TxCount++] = data_array[3];//(byte)(_data);       /*  低字节 */

            Tx_Buf[TxCount++] = data_array[3];//(byte)0x00;       /*  低字节 */
            Tx_Buf[TxCount++] = data_array[2];//(byte)0x00;       /*  低字节 */
            Tx_Buf[TxCount++] = data_array[0];//(byte)(_data >> 8);    /* 寄存器(16bits)个数 高字节 */
            Tx_Buf[TxCount++] = data_array[1];//(byte)(_data);       /*  低字节 */
            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }
        public static byte[] WriteCoil_0FH06_Speed_Set(byte device_id, UInt16 data_length, int start_addr, int end_addr, int fun_code, UInt16 byte_length, int _data)
        {
            UInt16 crc = 0;

            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[30];

            Tx_Buf[TxCount++] = device_id;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x10;                   /* 功能码 */

            //01 10 00 03   00 02     04   00 00 47 7A   01 A9

            Tx_Buf[TxCount++] = (byte)start_addr;
            Tx_Buf[TxCount++] = (byte)end_addr;




            Tx_Buf[TxCount++] = (byte)(data_length >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(data_length);           /* 寄存器地址 低字节 */


            //Tx_Buf[TxCount++] = (byte)(byte_length >> 8);      /* 数据长度 高字节 */
            Tx_Buf[TxCount++] = (byte)(byte_length);           /* 数据长度 低字节 */

            //1 61A0
            //90528
            //Tx:01 10 00 01 00 02 04 61 A0 00 01 EC 7D
            byte[] data_array = ConverHex4Speed(_data);


            Tx_Buf[TxCount++] = data_array[0];//(byte)0x00;       /*  低字节 */
            Tx_Buf[TxCount++] = data_array[1];//(byte)0x00;       /*  低字节 */


            Tx_Buf[TxCount++] = data_array[2];//(byte)(_data >> 8);    /* 寄存器(16bits)个数 高字节 */
            Tx_Buf[TxCount++] = data_array[3];//(byte)(_data);       /*  低字节 */




            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }

        public static byte[] WriteCoil_0FH06_Find_Zero(byte device_id, UInt16 _reg, int start_addr, int end_addr, int fun_code)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[13];
            Tx_Buf[TxCount++] = device_id;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x06;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)start_addr;
            Tx_Buf[TxCount++] = (byte)end_addr;


            Tx_Buf[TxCount++] = (byte)(fun_code >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(fun_code);           /* 寄存器地址 低字节 */



            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }


        public static byte[] WriteCoil_0FH06_12(byte device_id, UInt16 _reg, int start_addr, int end_addr, int fun_code)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[13];
            Tx_Buf[TxCount++] = device_id;                  /* 从站地址 */
            Tx_Buf[TxCount++] = 0x06;                   /* 功能码 */
            Tx_Buf[TxCount++] = (byte)start_addr;
            Tx_Buf[TxCount++] = (byte)end_addr;


            Tx_Buf[TxCount++] = (byte)(fun_code >> 24);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(fun_code >> 16);           /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)(fun_code >> 8);      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(fun_code);           /* 寄存器地址 低字节 */


            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }
        /** 
        * 函数功能: 读输入寄存器(InputRegister)
        * 输入参数: _addr:从站地址,_reg:寄存器地址,_num:待读取的寄存器数量
        * 返 回 值: 无
        * 说    明: 填充数据发送缓存区,然后发送
        */
        public static byte[] ReadInputReg_04H(byte _addr, UInt16 _reg, UInt16 _num)
        {
            UInt16 crc = 0;
            UInt16 TxCount = 0;
            byte[] Tx_Buf = new byte[13];
            Tx_Buf[TxCount++] = _addr;                    /* 从站地址 */
            Tx_Buf[TxCount++] = 0x04;                     /* 功能码 */
            Tx_Buf[TxCount++] = (byte)(_reg >> 8); ;      /* 寄存器地址 高字节 */
            Tx_Buf[TxCount++] = (byte)(_reg);             /* 寄存器地址 低字节 */
            Tx_Buf[TxCount++] = (byte)(_num >> 8);        /* 寄存器(16bits)个数 高字节 */
            Tx_Buf[TxCount++] = (byte)(_num);             /*  低字节 */

            crc = FreeModbus.MB_CRC16(Tx_Buf, TxCount);
            Tx_Buf[TxCount++] = (byte)crc;                     /* crc 低字节 */
            Tx_Buf[TxCount++] = (byte)(crc >> 8);              /* crc 高字节 */
            return Tx_Buf;
        }


    }
}
