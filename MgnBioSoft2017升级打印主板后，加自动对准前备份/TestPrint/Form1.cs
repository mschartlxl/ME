using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using Jyt.Sdk.PrintEngine;
using System.Threading;
using System.Threading.Tasks;
using System.Runtime.InteropServices;
using Jyt.Sdk.Shared;

namespace TestPrint
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        public static JytPrintEngineWrapper jprint;
        static JobInfo jobInfo;

        int width_g = 1280;
        int height_g = 1000;
        public int num_pages_g = 100;
        //public delegate void CallbackDelegate(); //声明委托
        void PrintResult(string param)
        {
            int num_complete_pages = 0;
            while (num_complete_pages < num_pages_g)
            {
                jprint.GetPageCompleteNum(ref num_complete_pages);
                Console.Write("打印进度：" + num_complete_pages + "num_pages：" + num_pages_g + "\r\n");
                //JytPrintEngine_GetPageCompleteNum(handle, num_complete_pages);
                //std::cout << "打印进度：" << num_complete_pages << " / " << num_pages << std::endl;
                //std::this_thread::sleep_for(std::chrono::seconds(1));
                Thread.Sleep(1000);
            }
        }

        // 回调函数  
        public void CallbackPrintStarted()
        {
            var result = Task.Factory.StartNew(() => PrintResult("xxx"));
            //Console.WriteLine("参数1: {0}", a);
            //Console.WriteLine("参数2:{0}", param);
            //jprint.JytPrintEngine_GetPageCompleteNum
        }

        public void CallbackPrintStoped(StopCode stop_code, int err_code)
        {
            Console.WriteLine("参数1: {0}\r\n", stop_code);
            Console.WriteLine("参数2:{0}\r\n", err_code);
        }
        public void CallbackBoardError(int err_code, bool is_fatal)
        {
            Console.WriteLine("参数1: {0}\r\n", err_code);
            Console.WriteLine("参数2:{0}\r\n", is_fatal);
        }

        public const int JYT_RESULT_SUCCESS = 0;   //!< 函数执行成功
        public const int JYT_RESULT_ERROR = -1;  //!< 函数执行失败
        public const int JYT_RESULT_TIMEOUT = 1;   //!< 函数执行超时
        public const int NUM_CHILD_HEADS_MAX = 16;
        public const int NUM_JOB_CHANNELS_MAX = 16;



        static void MonitorWork()
        {
            int n = 0;
            while (n < jobInfo.num_pages)
            {
                jprint.GetPageCompleteNum(ref n);
                Console.WriteLine(string.Format("{0:D3}", n));
                Thread.Sleep(1000);
            }
        }

        static void OnPrintStarted()
        {
            Console.WriteLine("打印开始");
            //Task.Run(() => { MonitorWork(); });
            Task.Factory.StartNew(() => { MonitorWork(); });
            
        }

        private void button1_Click(object sender, EventArgs e)
        {

            int mainBoardId = 0;
            int headBoardId = 0;

            jprint = new JytPrintEngineWrapper(MainBoardType.Eth_Gen5_4H, 1, HeadType.Gen5);

            jprint.SetHeadRowEnabled(0, 0, 0, true);
            jprint.SetHeadRowEnabled(0, 0, 1, true);
            jprint.SetHeadRowEnabled(0, 0, 2, true);
            jprint.SetHeadRowEnabled(0, 0, 3, true);


            jprint.PrintStarted += new PrintStartedCallback(OnPrintStarted);
            jprint.PrintStoppd += new PrintStoppedCallback(CallbackPrintStoped);




            jobInfo = new JobInfo();
            jobInfo.width = 1280;
            jobInfo.height = 1000;
            jobInfo.num_channels = 1;
            jobInfo.channels = new ColorChannel[16];
            jobInfo.channels[0] = ColorChannel.K;
            jobInfo.x_reslution = 600;
            jobInfo.y_reslution = 600;
            jobInfo.bits_per_pixel = 1;
            jobInfo.num_pages = 100;

            PhysicalHead ph = PhysicalHead.Default;
            ph.y_offset = 500;      // 电眼距离必须大于100
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph);

            PrintEnvironment pe = PrintEnvironment.Default;
            pe.encoder_page_start_en = EncoderPageStartEn.Internal;
            pe.sim_move_speed = 10;
            jprint.SetPrintEnvironment(pe);


            do
            {
                bool mbIsConnected = jprint.MainBoardIsConnected(mainBoardId);
                bool hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId);
                if (mbIsConnected == false || hbIsConnected == false)
                {
                    Console.WriteLine("板卡连接失败");
                    break;
                }

                Console.WriteLine("板卡连接成功");

                var rc = jprint.Start(jobInfo);
                if (rc != JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    //Console.WriteLine($"启动打印失败: {rc}");
                    break;
                }

                Console.WriteLine("启动打印成功");
                //Task.Factory.StartNew
                //Task.Run(() => { SendPageDataWork(); });
                Task.Factory.StartNew(() => { SendPageDataWork(); });

            } while (false);



            /*
            //JytPrintEngineWrapper jprint = new JytPrintEngineWrapper(Jyt.Sdk.PrintEngine.MainBoardType.Eth_Gen5_4H, 1, Jyt.Sdk.PrintEngine.HeadType.KY300);
            jprint = new JytPrintEngineWrapper(Jyt.Sdk.PrintEngine.MainBoardType.Eth_Gen5_4H, 1, Jyt.Sdk.PrintEngine.HeadType.Gen5);
            // 设置打印过程回调函数
            jprint.PrintStarted = CallbackPrintStarted;
            jprint.PrintStoppd = CallbackPrintStoped;
            jprint.BoardError = CallbackBoardError;
            PhysicalHead ph = PhysicalHead.Default;
            ph.y_offset = 500;      // 电眼距离必须大于100
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;
            ph.child_enables[0] = true;
            // 配置喷头信息
            jprint.SetPhysicalHead(0, 0, ph);
            bool is_mb_connected = jprint.MainBoardIsConnected(0);
            bool is_hb_connected = jprint.HeadBoardIsConnected(0, 0);
            // 检查板卡连接状态
            if (is_mb_connected && is_hb_connected)
            {
                PrintEnvironment pe = PrintEnvironment.Default;
                pe.encoder_page_start_en = EncoderPageStartEn.Internal;
                pe.sim_move_speed = 10;
                jprint.SetPrintEnvironment(pe);
                //启动打印
                JobInfo job_info = new JobInfo();
                job_info.width = width_g;
                job_info.height = height_g;
                job_info.num_pages = num_pages_g;
                job_info.num_channels = 1;
                job_info.bits_per_pixel = 1;
                job_info.x_reslution = 600;
                job_info.y_reslution = 600;
                job_info.channels = new ColorChannel[16];
                job_info.channels[0] = ColorChannel.K;
                int rc = jprint.Start(job_info);
                if (rc == JYT_RESULT_SUCCESS)
                {
                    //std::cout << "启动打印成功" << std::endl;
                    Console.Write("启动打印成功\r\n");
                    // 开启线程发送打印数据
                    var result = Task.Factory.StartNew(() => SendPrintDataWork());
                }
                else
                {
                    //std::cout << "启动打印失败，错误代码：" << rc << std::endl;
                }
            }
            else
            {
                Console.Write("连接板卡失败\r\n");
            }

            */
        }


        static void SendPageDataWork()
        {
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;

            IntPtr data = Marshal.AllocHGlobal(dataSize);

            for (int i = 0; i < jobInfo.num_pages; )
            {
                var rc = jprint.AddPageDataPure(ColorChannel.K, data, dataSize);
                if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    i++;
                }
                else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    continue;
                }
            }

            Marshal.FreeHGlobal(data);
        }



        void SendPrintDataWork()
        {

            int bytes_per_line = (width_g + 31) / 32 * 4;
            int data_size = bytes_per_line * height_g;

            int loop_num = 0;
            byte[] send_data_array_one1601 = new byte[data_size];
            for (int i = 0; i < send_data_array_one1601.Length; i++)
            {
                send_data_array_one1601[i] = 255;
            }
            string send_data = System.Text.Encoding.Default.GetString(send_data_array_one1601);
            IntPtr p = Marshal.StringToHGlobalAnsi(send_data);
            //每次取出160有效的但是要组成160,只是前40个字节有效
            while (true)
            {
                int rc = jprint.AddPageDataPure(ColorChannel.K, p, data_size);//JytPrintEngine_AddPageDataPure(handle, ColorChannel::K, data.get(), data_size);
                if (rc == JYT_RESULT_SUCCESS)
                {
                    // 发送成功
                    //break;
                }
                else if (rc == JYT_RESULT_TIMEOUT)
                {
                    // 超时需要重新发送
                    continue;
                }

                Thread.Sleep(1000);
            }
            Marshal.FreeHGlobal(p);
        }

        private void button2_Click(object sender, EventArgs e)
        {
            byte[] send_data_array = new byte[160];
            for (int i = 0; i < send_data_array.Length; i++)
            {
                send_data_array[i] = 255;
            }

            int loop_num = 0;

            //如果小于1280  补齐1280，都补0
            if (send_data_array.Length < 160)
            {
                int need_add_0_number = 160 - send_data_array.Length;

                byte[] byteAll = new byte[160];
                byte[] send_data_array_need_add = new byte[need_add_0_number];

                for (int i = 0; i < send_data_array_need_add.Length; i++)
                {
                    send_data_array_need_add[i] = 0;
                }
                Array.Copy(send_data_array, 0, byteAll, 0, send_data_array.Length);
                Array.Copy(send_data_array_need_add, 0, byteAll, send_data_array.Length, send_data_array_need_add.Length);
                send_data_array = byteAll;
            }

            //如果大于1280，跟1280求余，再用1280减去余数得到的数，给数组补上0
            if (send_data_array.Length > 160)
            {
                int need_add_0_number = 160 - send_data_array.Length % 160;
                byte[] send_data_array_need_add = new byte[need_add_0_number];
                byte[] byteAll = new byte[send_data_array.Length + need_add_0_number];

                for (int i = 0; i < send_data_array_need_add.Length; i++)
                {
                    send_data_array_need_add[i] = 0;
                }

                Array.Copy(send_data_array, 0, byteAll, 0, send_data_array.Length);
                Array.Copy(send_data_array_need_add, 0, byteAll, send_data_array.Length, send_data_array_need_add.Length);

                send_data_array = byteAll;

            }


            for (int i = 0; i < send_data_array.Length; i = i + 160)
            {
                byte[] send_data_array_one160 = new byte[160];
                for (int j = 0; j < 160; j++)
                {
                    if (j < 160)
                    {
                        //send_data_array_one320[j] = (byte)((send_data_array[j+i])- loop_num);
                        send_data_array_one160[j] = (byte)((send_data_array[j + i]));
                    }
                    else
                    {
                        send_data_array_one160[j] = 0;
                    }
                }
                Console.Write("send_data_array_one160[0]:" + send_data_array_one160[0] + "--loop_num" + loop_num + "\r\n");
                string send_data = System.Text.Encoding.ASCII.GetString(send_data_array_one160);
                IntPtr p = Marshal.StringToHGlobalAnsi(send_data);
                //每次取出320有效的但是要组成1280只是前320个字节有效
                loop_num++;
            }

        }

    }
}
