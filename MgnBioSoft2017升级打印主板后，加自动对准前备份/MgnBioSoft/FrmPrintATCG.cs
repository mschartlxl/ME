using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using System.Threading;
using Jyt.Sdk.PrintEngine;
using Jyt.Sdk.Shared;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MGNBIO.Model;
using MGNChipMatrix;
using MGNBIO.Common;
using HZH_Controls.Forms;
using System.IO;
using System.Xml.Serialization;
using System.Xml;
using System.Management;
using System.IO.Ports;
using System.Diagnostics;
using MGNBIO.Communication;
using TestLog4Net;
using ColorChannel = Jyt.Sdk.Shared.ColorChannel;
using MainBoardType = Jyt.Sdk.PrintEngine.MainBoardType;
using EncoderPageStartEn = Jyt.Sdk.PrintEngine.EncoderPageStartEn;
using WaveformKind = Jyt.Sdk.PrintEngine.WaveformKind;
using HeadType = Jyt.Sdk.PrintEngine.HeadType;
using StopCode = Jyt.Sdk.PrintEngine.StopCode;
using System.Drawing.Imaging;
using DustCollector.Model;
using DustCollector.protocol;
using GxIAPINET;
using GxIAPINET.Sample.Common;

namespace MGNBIO
{
    public partial class FrmPrintATCG : Form
    {

        public static string system_type = "2"; //1为三英精控，2为奥控
        gclib g_Controller = new gclib();
        gclib g_ControllerPLC = new gclib();
        //实例化一个timer  
        System.Windows.Forms.Timer myTimer = new System.Windows.Forms.Timer();

        private Uvcsam cam_ = null;
        private Bitmap bmp_ = null;
        private IntPtr buf_ = IntPtr.Zero;
        private int width_ = 0, height_ = 0;
        private int count_ = 0;

        public FrmPrintATCG()
        {
            InitializeComponent();
            mainFrm = this;
        }

        public static JytPrintEngineWrapper jprint;
        static JobInfo jobInfo;

        /// <summary>
        /// 打印的宽度
        /// </summary>
        public static int width_g = 1280;
        /// <summary>
        /// 每一页打印的高度
        /// </summary>
        int height_g = 80;
        /// <summary>
        /// 每一次打印的页数
        /// </summary>
        public static int num_pages_g = 1;
        //
        /// <summary>
        /// 多少层的碱基
        /// </summary>
        int cycle_number = 0;
        //
        /// <summary>
        /// 有多少层的图片这个地方就有多少个List,值为1就绪准备，2正在发送，3完成
        /// </summary>
        List<int> cycle_finish_status_list;

        //
        /// <summary>
        /// 开始运行标识,false,true,如果为true表示正在运行，false表示一次运行完成
        /// </summary>
        static bool run_flage = false;

        /// <summary>
        /// 表示全部运行完成
        /// </summary>
        static bool run_finish_flage = false;
        /// <summary>
        /// 打印前延时时间
        /// </summary>
        static int print_front_delay_time_g = 1000;

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
                Thread.Sleep(10);
            }
        }

        public void CallbackPrintStoped(StopCode stop_code, int err_code)
        {
            Console.WriteLine("参数1: {0}\r\n", stop_code);
            Console.WriteLine("参数2:{0}\r\n", err_code);

            //TriggerPrint();
        }
        public void CallbackBoardError(int err_code, bool is_fatal)
        {
            Console.WriteLine("参数1: {0}\r\n", err_code);
            Console.WriteLine("参数2:{0}\r\n", is_fatal);
        }

        // 回调函数  
        public void CallbackPrintStarted()
        {

            var result = Task.Factory.StartNew(() => PrintResult("xxx"));
            //Console.WriteLine("参数1: {0}", a);
            //Console.WriteLine("参数2:{0}", param);
            //jprint.JytPrintEngine_GetPageCompleteNum
        }

        public const int JYT_RESULT_SUCCESS = 0;   //!< 函数执行成功
        public const int JYT_RESULT_ERROR = -1;  //!< 函数执行失败
        public const int JYT_RESULT_TIMEOUT = 1;   //!< 函数执行超时
        public const int NUM_CHILD_HEADS_MAX = 16;
        public const int NUM_JOB_CHANNELS_MAX = 16;

        static void MonitorWork()
        {
            int n = 0;
            int black_flage_number = 0;
            double move_speed = 0;
            //jprint.GetPageCompleteNum(ref n);
            //Console.WriteLine(string.Format("{0:D3}", n));
            //运动到A喷头位置
            //OneActive_Static("A喷头");
            while (n < jobInfo.num_pages)
            {
                black_flage_number = jprint.GetPageStartRxNum(0);
                jprint.GetMoveSpeed(2400, out move_speed);
                Console.WriteLine(string.Format("速度{0:f}", move_speed));
                Console.WriteLine(string.Format("黑标数量{0:D}", black_flage_number));
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

        static bool PrintInitian()
        {
            int mainBoardId = 0;
            int headBoardId = 0;
            bool mbIsConnected = jprint.MainBoardIsConnected(mainBoardId);
            bool hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId);
            if (mbIsConnected == false || hbIsConnected == false)
            {
                Console.WriteLine("板卡连接失败");
                return false;
            }
            Console.WriteLine("板卡连接成功");
            var rc = jprint.Start(jobInfo);
            if (rc != JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
            {
                //Console.WriteLine($"启动打印失败: {rc}");
                return false;
            }
            Console.WriteLine("启动打印成功");
            return true;
        }

        /// <summary>
        /// 
        /// </summary>
        /// <param name="print_content_list_para2">打印内容</param>
        /// <param name="print_type">打印类型 A,T，C，G</param>
        public void StartPrint(List<PrintContent> print_content_list_para2, int print_type)
        {
            int mainBoardId = 0;
            int headBoardId = 0;
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
                int stride = (jobInfo.width + 31) / 32 * 4;
                int dataSize = stride * jobInfo.height;
                byte[] send_data_array_one160 = new byte[dataSize];
                //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
                //int stride = (jobInfo.width + 31) / 32 * 4;
                //int dataSize = stride * jobInfo.height;
                //Task.Factory.StartNew(() => { SendPageDataWork(send_data_array_one160); });
                //SendPageDataWork();
                Task.Factory.StartNew(() => { SendPageDataWork(); });
                //Task.Factory.StartNew(() => { SendPageDataWork(); });
                //Task.Factory.StartNew(() => { SendPageDataWork__1(); });


            } while (false);
        }


        private void button1_Click(object sender, EventArgs e)
        {
            print_content_list_g = CommonData.PrintContentList_g;
            int mainBoardId = 0;
            int headBoardId = 0;
            //PhysicalHead ph = PhysicalHead.Default;
            //int y_offset = (int)(((32 - 10) / 2.54) * 2400);
            //ph.y_offset = y_offset;      // 电眼距离必须大于100  这个值的换算问一下张工
            //ph.head_type = HeadType.Gen5;
            //ph.num_child_heads = 1;
            //ph.child_channels[0] = ColorChannel.K;  //A
            //ph.child_enables[0] = true;
            //jprint.SetPhysicalHead(mainBoardId, headBoardId, ph);

            //PhysicalHead ph_c = PhysicalHead.Default;
            //ph_c.y_offset = (int)(((32 - 10.5) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            //ph_c.head_type = HeadType.Gen5;
            //ph_c.num_child_heads = 1;
            //ph_c.child_channels[0] = ColorChannel.C; //T
            //ph_c.child_enables[0] = true;
            //jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_c);

            //PhysicalHead ph_m = PhysicalHead.Default;
            //ph_m.y_offset = (int)(((32 - 14.8) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            //ph_m.head_type = HeadType.Gen5;
            //ph_m.num_child_heads = 1;
            //ph_m.child_channels[0] = ColorChannel.M; //C
            //ph_m.child_enables[0] = true;
            //jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_m);

            //PhysicalHead ph_y = PhysicalHead.Default;
            //ph_y.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            //ph_y.head_type = HeadType.Gen5;
            //ph_y.num_child_heads = 1;
            //ph_y.child_channels[0] = ColorChannel.Y; //G
            //ph_y.child_enables[0] = true;
            //jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_y);

            //PhysicalHead ph_w = PhysicalHead.Default;
            //ph_w.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            //ph_w.head_type = HeadType.Gen5;
            //ph_w.num_child_heads = 1;
            //ph_w.child_channels[0] = ColorChannel.White; //all
            //ph_w.child_enables[0] = true;
            //jprint.SetPhysicalHead(1, 0, ph_w);
            do
            {
                bool mbIsConnected = jprint.MainBoardIsConnected(mainBoardId);
                //mbIsConnected = jprint.MainBoardIsConnected(mainBoardId+1);
                bool hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId);
                //hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId + 1);
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
                int stride = (jobInfo.width + 31) / 32 * 4;
                int dataSize = stride * jobInfo.height;
                byte[] send_data_array_one160 = new byte[dataSize];
                //for (int i = 0; i < send_data_array_one160.Length; i++)
                //{
                //    send_data_array_one160[i] = 255;
                //}
                //for (int i = 0; i < 160; i++)
                //{
                //    send_data_array_one160[i] = 0x88;
                //}
                //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴

                //int stride = (jobInfo.width + 31) / 32 * 4;
                //int dataSize = stride * jobInfo.height;
                for (int i = 0; i < dataSize; i++)
                {
                    send_data_array_one160[i] = 0x01;
                }
                //Task.Factory.StartNew(() => { SendPageDataWork(send_data_array_one160); });
                //SendPageDataWork();

                //打印内容添加
                //Task.Factory.StartNew(() => { SendPageDataWork_back20211227(); });

                //Task.Factory.StartNew(() => { PrintOnePageData(0); });



            } while (false);

        }

        public void StartPrintA()
        {
            int mainBoardId = 0;
            int headBoardId = 0;
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
                int stride = (jobInfo.width + 31) / 32 * 4;
                int dataSize = stride * jobInfo.height;
                byte[] send_data_array_one160 = new byte[dataSize];
                //for (int i = 0; i < send_data_array_one160.Length; i++)
                //{
                //    send_data_array_one160[i] = 255;
                //}
                //for (int i = 0; i < 160; i++)
                //{
                //    send_data_array_one160[i] = 0x88;
                //}
                //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
                for (int i = 0; i < 160000; i++)
                {
                    send_data_array_one160[i] = 0x80;
                }
                Task.Factory.StartNew(() => { SendPageDataWork(send_data_array_one160); });
            } while (false);
        }
        public void InitializePrintSystem1(int main_board_number)
        {
            int dip_value = 140;
            //读打印配置
            List<string> print_configuration_list = ReadFileToList("PrintConfiguration.ini");
            if (print_configuration_list.Count > 0)
            {
                for (int i = 0; i < print_configuration_list.Count; i++)
                {
                    string system_sz = print_configuration_list[i];
                    string[] print_sz_array = system_sz.Split(':');
                    if (print_sz_array[0].Equals("dpi"))
                    {
                        dip_value = int.Parse(print_sz_array[1]);
                    }
                }
            }
            JytPrintEngineWrapper.InitialLogger();
            int mainBoardId = 0;
            int headBoardId = 0;
            //int a = 0;
            //Console.WriteLine(a);
            main_board_number = 2;//这个参数是主板数量
            jprint = new JytPrintEngineWrapper(MainBoardType.Eth_Gen5_4H, main_board_number, HeadType.Gen5);
            //jprint.Reset(MainBoardType.Eth_Gen5_4H, 2);
            jprint.SetHeadRowEnabled(0, 0, 0, true);
            jprint.SetHeadRowEnabled(0, 0, 1, true);
            jprint.SetHeadRowEnabled(0, 0, 2, true);
            jprint.SetHeadRowEnabled(0, 0, 3, true);

            jprint.SetHeadRowEnabled(0, 1, 0, true);
            jprint.SetHeadRowEnabled(0, 1, 1, true);
            jprint.SetHeadRowEnabled(0, 1, 2, true);
            jprint.SetHeadRowEnabled(0, 1, 3, true);

            jprint.SetHeadRowEnabled(0, 2, 0, true);
            jprint.SetHeadRowEnabled(0, 2, 1, true);
            jprint.SetHeadRowEnabled(0, 2, 2, true);
            jprint.SetHeadRowEnabled(0, 2, 3, true);

            jprint.SetHeadRowEnabled(0, 3, 0, true);
            jprint.SetHeadRowEnabled(0, 3, 1, true);
            jprint.SetHeadRowEnabled(0, 3, 2, true);
            jprint.SetHeadRowEnabled(0, 3, 3, true);

            //第二块板，第一个喷头
            jprint.SetHeadRowEnabled(1, 0, 0, true);
            jprint.SetHeadRowEnabled(1, 0, 1, true);
            jprint.SetHeadRowEnabled(1, 0, 2, true);
            jprint.SetHeadRowEnabled(1, 0, 3, true);

            jprint.PrintStarted += new PrintStartedCallback(OnPrintStarted);
            jprint.PrintStoppd += new PrintStoppedCallback(CallbackPrintStoped);

            jobInfo = new JobInfo();
            jobInfo.width = 1280;
            jobInfo.height = height_g;
            jobInfo.num_channels = 5;
            jobInfo.channels = new ColorChannel[16];
            jobInfo.channels[0] = ColorChannel.K;
            jobInfo.channels[1] = ColorChannel.C;
            jobInfo.channels[2] = ColorChannel.M;
            jobInfo.channels[3] = ColorChannel.Y;
            jobInfo.channels[4] = ColorChannel.White;
            jobInfo.x_reslution = 600;
            jobInfo.y_reslution = dip_value;//140; //dpi
            jobInfo.bits_per_pixel = 1;
            jobInfo.num_pages = num_pages_g;

            /*
            PhysicalHead ph = PhysicalHead.Default;
            int y_offset = (int)(((32 - 10) / 2.54) * 2400);
            ph.y_offset = y_offset;      // 电眼距离必须大于100  这个值的换算问一下张工
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph);

            PhysicalHead ph_c = PhysicalHead.Default;
            ph_c.y_offset = (int)(((32 - 13.3) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_c);



            PhysicalHead ph_m = PhysicalHead.Default;
            ph_m.y_offset = (int)(((32 - 14.8) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            ph_y.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            ph_w.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);
            */

            //List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("A喷头"));
            //float y_offset_a = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            //move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("T喷头"));
            //float y_offset_t = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            //move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("C喷头"));
            //float y_offset_c = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            //move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("G喷头"));
            //float y_offset_g = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            //move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("ALL喷头"));
            //float y_offset_all = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);


            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("A喷头"));
            float y_offset_a = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("T喷头"));
            float y_offset_t = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("C喷头"));
            float y_offset_c = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("G喷头"));
            float y_offset_g = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("ALL喷头"));
            float y_offset_all = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);



            PhysicalHead ph = PhysicalHead.Default;
            //int y_offset = (int)(((32 - 11.3) / 2.54) * 2400);                //这个为基准，下面的值都必须比这个值大，如果打出来的偏右了，把值调小一些，如果打出来靠左一些，把值高大一些。
            int y_offset = (int)(((32 - y_offset_a) / 2.54) * 2400);
            ph.y_offset = y_offset;
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 0, ph);


            PhysicalHead ph_c = PhysicalHead.Default;
            //ph_c.y_offset = (int)(((32 - 13.8) / 2.54) * 2400);
            ph_c.y_offset = (int)(((32 - y_offset_t) / 2.54) * 2400);
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 1, ph_c);

            PhysicalHead ph_m = PhysicalHead.Default;
            //ph_m.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);
            ph_m.y_offset = (int)(((32 - y_offset_c) / 2.54) * 2400);
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 2, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            //ph_y.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);
            ph_y.y_offset = (int)(((32 - y_offset_g) / 2.54) * 2400);
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 3, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            //ph_w.y_offset = (int)(((32 - 21.3) / 2.54) * 2400);
            ph_w.y_offset = (int)(((32 - y_offset_all) / 2.54) * 2400);
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);


            PrintEnvironment pe = PrintEnvironment.Default;
            //pe.encoder_page_start_en = EncoderPageStartEn.Internal;//内部的就不用触发了。
            pe.encoder_page_start_en = EncoderPageStartEn.External; //外部的，得结合触发打印TriggerPrint()，电眼距离进行打印
            pe.pagestart_fil_factor = 0;
            pe.encoder_fil_factor = 99;
            pe.encoder_resolution = 2400;
            pe.encoder_adjust_factor = 1084;

            //External
            pe.debug_dump_enabled = true;
            pe.endless_pages = false;//马上就打印
            //float print_speed = (float)0.0005;
            //pe.sim_move_speed = print_speed; //10米每分
            pe.waveform_kind = WaveformKind.Small;
            pe.endless_pages = true;

            pe.speed_limitation = 0;
            jprint.SetPrintEnvironment(pe);



        }

        public void InitializePrint(int main_board_number)
        {
            int dip_value = 140;
            //读打印配置
            List<string> print_configuration_list = ReadFileToList("PrintConfiguration.ini");
            if (print_configuration_list.Count > 0)
            {
                for (int i = 0; i < print_configuration_list.Count; i++)
                {
                    string system_sz = print_configuration_list[i];
                    string[] print_sz_array = system_sz.Split(':');
                    if (print_sz_array[0].Equals("dpi"))
                    {
                        dip_value = int.Parse(print_sz_array[1]);
                    }
                }
            }

            JytPrintEngineWrapper.InitialLogger();
            int mainBoardId = 0;
            int headBoardId = 0;
            //int a = 0;
            //Console.WriteLine(a);
            main_board_number = 2;//这个参数是主板数量
            jprint = new JytPrintEngineWrapper(MainBoardType.Eth_Gen5_4H, main_board_number, HeadType.Gen5);
            //jprint.Reset(MainBoardType.Eth_Gen5_4H, 2);

            jprint.SetHeadRowEnabled(0, 0, 0, true);
            jprint.SetHeadRowEnabled(0, 0, 1, true);
            jprint.SetHeadRowEnabled(0, 0, 2, true);
            jprint.SetHeadRowEnabled(0, 0, 3, true);

            jprint.SetHeadRowEnabled(0, 1, 0, true);
            jprint.SetHeadRowEnabled(0, 1, 1, true);
            jprint.SetHeadRowEnabled(0, 1, 2, true);
            jprint.SetHeadRowEnabled(0, 1, 3, true);

            jprint.SetHeadRowEnabled(0, 2, 0, true);
            jprint.SetHeadRowEnabled(0, 2, 1, true);
            jprint.SetHeadRowEnabled(0, 2, 2, true);
            jprint.SetHeadRowEnabled(0, 2, 3, true);

            jprint.SetHeadRowEnabled(0, 3, 0, true);
            jprint.SetHeadRowEnabled(0, 3, 1, true);
            jprint.SetHeadRowEnabled(0, 3, 2, true);
            jprint.SetHeadRowEnabled(0, 3, 3, true);

            //第二块板，第一个喷头
            jprint.SetHeadRowEnabled(1, 0, 0, true);
            jprint.SetHeadRowEnabled(1, 0, 1, true);
            jprint.SetHeadRowEnabled(1, 0, 2, true);
            jprint.SetHeadRowEnabled(1, 0, 3, true);

            jprint.PrintStarted += new PrintStartedCallback(OnPrintStarted);
            jprint.PrintStoppd += new PrintStoppedCallback(CallbackPrintStoped);

            jobInfo = new JobInfo();
            jobInfo.width = 1280;
            jobInfo.height = height_g;
            jobInfo.num_channels = 5;
            jobInfo.channels = new ColorChannel[16];
            jobInfo.channels[0] = ColorChannel.K;
            jobInfo.channels[1] = ColorChannel.C;
            jobInfo.channels[2] = ColorChannel.M;
            jobInfo.channels[3] = ColorChannel.Y;
            jobInfo.channels[4] = ColorChannel.White;
            jobInfo.x_reslution = 600;
            jobInfo.y_reslution = dip_value;//140; //dpi
            jobInfo.bits_per_pixel = 1;
            jobInfo.num_pages = num_pages_g;




            /*
            PhysicalHead ph = PhysicalHead.Default;
            int y_offset = (int)(((32 - 10) / 2.54) * 2400);
            ph.y_offset = y_offset;      // 电眼距离必须大于100  这个值的换算问一下张工
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph);

            PhysicalHead ph_c = PhysicalHead.Default;
            ph_c.y_offset = (int)(((32 - 13.3) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_c);



            PhysicalHead ph_m = PhysicalHead.Default;
            ph_m.y_offset = (int)(((32 - 14.8) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            ph_y.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, headBoardId, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            ph_w.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);    // 电眼距离必须大于100  这个值的换算问一下张工
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);
            */

            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("A喷头"));
            float y_offset_a = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);

            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("T喷头"));
            float y_offset_t = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);

            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("C喷头"));
            float y_offset_c = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);

            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("G喷头"));
            float y_offset_g = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);

            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("ALL喷头"));
            float y_offset_all = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);

            PhysicalHead ph = PhysicalHead.Default;
            //int y_offset = (int)(((32 - 11.3) / 2.54) * 2400);                //这个为基准，下面的值都必须比这个值大，如果打出来的偏右了，把值调小一些，如果打出来靠左一些，把值高大一些。
            //int y_offset = (int)(((32 - y_offset_a) / 2.54) * 2400);
            int y_offset = (int)(((28 - Math.Abs(y_offset_a)) / 2.54) * 2400);
            ph.y_offset = y_offset;
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 0, ph);



            PhysicalHead ph_c = PhysicalHead.Default;
            //ph_c.y_offset = (int)(((32 - 13.8) / 2.54) * 2400);
            ph_c.y_offset = (int)(((28 - Math.Abs(y_offset_t)) / 2.54) * 2400);
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 1, ph_c);

            PhysicalHead ph_m = PhysicalHead.Default;
            //ph_m.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);
            ph_m.y_offset = (int)(((28 - Math.Abs(y_offset_c)) / 2.54) * 2400);
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 2, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            //ph_y.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);
            ph_y.y_offset = (int)(((28 - Math.Abs(y_offset_g)) / 2.54) * 2400);
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 3, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            //ph_w.y_offset = (int)(((32 - 21.3) / 2.54) * 2400);
            ph_w.y_offset = (int)(((28 - Math.Abs(y_offset_all)) / 2.54) * 2400);
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);


            PrintEnvironment pe = PrintEnvironment.Default;
            //pe.encoder_page_start_en = EncoderPageStartEn.Internal;//内部的就不用触发了。
            pe.encoder_page_start_en = EncoderPageStartEn.External; //外部的，得结合触发打印TriggerPrint()，电眼距离进行打印
            pe.pagestart_fil_factor = 0;
            pe.encoder_fil_factor = 99;
            pe.encoder_resolution = 2400;
            pe.encoder_adjust_factor = 1084;
            //pe.sim_pagestart_cycle = 1000;

            //External
            pe.debug_dump_enabled = true;
            pe.endless_pages = false;//马上就打印
            //float print_speed = (float)0.0005;
            //pe.sim_move_speed = print_speed; //10米每分
            pe.waveform_kind = WaveformKind.Small;
            pe.endless_pages = true;

            pe.speed_limitation = 0;
            jprint.SetPrintEnvironment(pe);
        }

        static bool SendPageDataWork()
        {
            print_content_list_g = CommonData.PrintContentList_g;
            if (print_content_list_g.Count == 0)
            {
                return false;
            }

            //每页打印的高度，也就是行数
            jobInfo.height = print_content_list_g.Count;
            jobInfo.num_pages = num_pages_g;

            bool result = false;
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            byte[] send_data_array_one40 = new byte[40];
            var p = Marshal.AllocHGlobal(dataSize);
            //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
            //IntPtr data = Marshal.AllocHGlobal(dataSize);
            //喷的区域的高度
            int length = 40;
            byte[] send_data_array_one160_new = new byte[160];
            byte[] send_data_array_one10_k = new byte[length];
            byte[] send_data_array_one10_c = new byte[length];

            byte[] send_data_array_one10_m = new byte[length];
            byte[] send_data_array_one10_y = new byte[length];
            byte[] send_data_array_one10_all = new byte[length];

            for (int i = 0; i < print_content_list_g.Count;)
            {
                int i_line_number = 0;
                //添加A数据
                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(print_content_list_g[i_line_number].Print_array_a, 0, send_data_array_one160, ij, length);
                    i_line_number++;
                    ij += 160;
                }
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_k = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);

                //添加C数据
                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                i_line_number = 0;
                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(print_content_list_g[i].Print_array_c, 0, send_data_array_one160, ij, length);
                    i_line_number++;
                    ij += 160;
                }
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_c = jprint.AddPageDataPure(ColorChannel.C, p, dataSize);

                //添加T数据
                //添加G数据
                //添加ALL数据

                if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS && rc_c == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    i++;
                    //Thread.Sleep(200);
                    Console.WriteLine("发送k打印数据成功:" + i);
                }
                else if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    Console.WriteLine("发送k打印数据超时:" + i);
                    //Thread.Sleep(200);
                    continue;
                }


                Thread.Sleep(20);
            }
            Marshal.FreeHGlobal(p);
            result = true;
            return result;
        }

        static bool SendPageDataWork_back20211229()
        {
            print_content_list_g = CommonData.PrintContentList_g;
            if (print_content_list_g.Count == 0)
            {
                return false;
            }
            //每页打印的高度，也就是行数
            jobInfo.height = print_content_list_g.Count;
            jobInfo.num_pages = num_pages_g;


            bool result = false;
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            byte[] send_data_array_one40 = new byte[40];
            var p = Marshal.AllocHGlobal(dataSize);
            //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
            //IntPtr data = Marshal.AllocHGlobal(dataSize);
            //喷的区域的高度
            int length = 10;
            byte[] send_data_array_one160_new = new byte[160];
            byte[] send_data_array_one10_k = new byte[length];
            byte[] send_data_array_one10_c = new byte[length];

            byte[] send_data_array_one10_m = new byte[length];
            byte[] send_data_array_one10_y = new byte[length];
            byte[] send_data_array_one10_white = new byte[length];


            for (int i = 0; i < num_pages_g;)
            {

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij, length);
                //    ij += 160;
                //}


                int i_line_number = 0;
                //添加A数据
                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(print_content_list_g[i_line_number].Print_array_a, 0, send_data_array_one160, ij, print_content_list_g[i_line_number].Print_array_a.Length);
                    i_line_number++;
                    ij += 160;
                }


                //A
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_k = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}


                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 30, length);
                //    ij += 160;
                //}


                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                //i_line_number = 0;
                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(print_content_list_g[i_line_number].Print_array_t, 0, send_data_array_one160, ij, print_content_list_g[i_line_number].Print_array_t.Length);
                //    i_line_number++;
                //    ij += 160;
                //}

                //T
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_c = jprint.AddPageDataPure(ColorChannel.C, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 50, length);
                //    ij += 160;
                //}

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                //i_line_number = 0;
                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(print_content_list_g[i_line_number].Print_array_c, 0, send_data_array_one160, ij, print_content_list_g[i_line_number].Print_array_c.Length);
                //    i_line_number++;
                //    ij += 160;
                //}


                //C
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_m = jprint.AddPageDataPure(ColorChannel.M, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 70, length);
                //    ij += 160;
                //}


                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                //i_line_number = 0;
                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(print_content_list_g[i_line_number].Print_array_g, 0, send_data_array_one160, ij, print_content_list_g[i_line_number].Print_array_g.Length);
                //    i_line_number++;
                //    ij += 160;
                //}
                //G
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_y = jprint.AddPageDataPure(ColorChannel.Y, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0xff;
                //}


                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 100, length);
                //    ij += 160;
                //}


                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }
                //i_line_number = 0;
                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(print_content_list_g[i_line_number].Print_array_h, 0, send_data_array_one160, ij, print_content_list_g[i_line_number].Print_array_h.Length);
                //    i_line_number++;
                //    ij += 160;
                //}

                //White
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_all = jprint.AddPageDataPure(ColorChannel.White, p, dataSize);

                if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    i++;
                    //Thread.Sleep(200);
                    Console.WriteLine("发送k打印数据成功:" + i);
                }
                else if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    Console.WriteLine("发送k打印数据超时:" + i);
                    //Thread.Sleep(200);
                    continue;
                }


                Thread.Sleep(20);
            }
            Marshal.FreeHGlobal(p);
            result = true;
            return result;
        }
        static bool SendPageDataWork_back20211227()
        {


            //print_content_list_g = CommonData.PrintContentList_g;

            //if (print_content_list_g.Count == 0)
            //{
            //    return false;
            //}

            ////每页打印的高度，也就是行数
            //jobInfo.height = print_content_list_g.Count;
            //jobInfo.num_pages = num_pages_g;


            bool result = false;
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            byte[] send_data_array_one40 = new byte[40];
            var p = Marshal.AllocHGlobal(dataSize);
            //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
            //IntPtr data = Marshal.AllocHGlobal(dataSize);
            //喷的区域的高度
            int length = 160;
            byte[] send_data_array_one160_new = new byte[160];
            byte[] send_data_array_one10_k = new byte[length];
            byte[] send_data_array_one10_c = new byte[length];

            byte[] send_data_array_one10_m = new byte[length];
            byte[] send_data_array_one10_y = new byte[length];
            byte[] send_data_array_one10_white = new byte[length];


            for (int i = 0; i < num_pages_g;)
            {

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij, length);
                //    ij += 160;
                //}

                //A
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_k = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;//0b10001000;
                }

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 20, length);
                //    ij += 160;
                //}


                //T
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_c = jprint.AddPageDataPure(ColorChannel.C, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;//0b10001000;
                }

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 40, length);
                //    ij += 160;
                //}



                //C
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_m = jprint.AddPageDataPure(ColorChannel.M, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0x00;
                //}

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;//0b10001000;
                }

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 60, length);
                //    ij += 160;
                //}



                //G
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_y = jprint.AddPageDataPure(ColorChannel.Y, p, dataSize);

                //for (int ij = 0; ij < dataSize; ij++)
                //{
                //    send_data_array_one160[ij] = 0xff;
                //}


                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0b10001000;
                }

                //for (int c = 0; c < send_data_array_one10_c.Length; c++)
                //{
                //    send_data_array_one10_c[c] = 0xff;
                //}

                //for (int ij = 0; ij < dataSize;)
                //{
                //    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 80, length);
                //    ij += 160;
                //}
                //White
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_all = jprint.AddPageDataPure(ColorChannel.White, p, dataSize);


                if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    i++;
                    //Thread.Sleep(200);
                    Console.WriteLine("发送k打印数据成功:" + i);
                }
                else if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    Console.WriteLine("发送k打印数据超时:" + i);
                    //Thread.Sleep(200);
                    continue;
                }


                Thread.Sleep(20);
            }
            Marshal.FreeHGlobal(p);
            result = true;
            return result;
        }


        /// <summary>
        /// 调试Y的位置
        /// </summary>
        /// <returns></returns>
        static bool SendPageDataWork_Test_Y_PositionAll()
        {
            //print_content_list_g = CommonData.PrintContentList_g;
            //if (print_content_list_g.Count == 0)
            //{
            //    return false;
            //}
            ////每页打印的高度，也就是行数
            //jobInfo.height = print_content_list_g.Count;
            //jobInfo.num_pages = num_pages_g;

            num_pages_g = 12;

            bool result = false;
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            byte[] send_data_array_one40 = new byte[40];
            var p = Marshal.AllocHGlobal(dataSize);
            //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
            //IntPtr data = Marshal.AllocHGlobal(dataSize);

            //喷的区域的高度
            int length = line_length_g;

            byte[] send_data_array_one160_new = new byte[160];
            byte[] send_data_array_one10_k = new byte[length];
            byte[] send_data_array_one10_c = new byte[length];
            byte[] send_data_array_one10_m = new byte[length];
            byte[] send_data_array_one10_y = new byte[length];
            byte[] send_data_array_one10_white = new byte[length];

            for (int c = 0; c < send_data_array_one10_c.Length; c++)
            {
                send_data_array_one10_c[c] = 0x00;
            }

            for (int i = 0; i < num_pages_g;)
            {
                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }

                if (a_valid_a == 1)
                {
                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        //间隔1
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b10001000;
                            //print_content += "10001000-";
                        }

                        if (space_point_g == 1)
                        {
                            send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                        }

                        if (space_point_g == 2)
                        {

                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            if (c == 1)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }
                            if (c == 2)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 2)
                            {
                                if (c % 3 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 3 == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 3 == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }

                        }

                        if (space_point_g == 3)
                        {
                            if (c % 2 == 0)
                            {
                                //Console.Write("x10000000");
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            else
                            {
                                send_data_array_one10_c[c] = 0b00000000;
                                //print_content += "00000000-";
                            }
                        }

                        if (space_point_g == 4)
                        {
                            //10000000 00000000 00001000 00000000 00000000
                            //    0       1        2        3        4
                            //10000000 00000000 00001000 00000000  00000000
                            //    5        6        7        8        9
                            //10000000 00000000 00001000 00000000  00000000
                            //    10        11      12        13      14
                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }

                            if (c == 1)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 2)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }

                            if (c == 3)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 4)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 4)
                            {
                                if (c % 5 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 5 == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 5 == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }
                        }

                        /*
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b11111111;
                        }
                        if (space_point_g == 1)
                        {   if (c % 2 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 2)
                        {
                            if (c % 3 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 3)
                        {
                            if (c % 4 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 4)
                        {
                            if (c % 5 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        */


                    }
                }

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij, length);
                    ij += 160;
                }

                //A
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_k = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);

                for (int c = 0; c < send_data_array_one10_c.Length; c++)
                {
                    send_data_array_one10_c[c] = 0x00;
                }

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }


                if (a_valid_t == 1)
                {
                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        //间隔1
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b10001000;
                            //print_content += "10001000-";
                        }

                        if (space_point_g == 1)
                        {
                            send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                        }

                        if (space_point_g == 2)
                        {

                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            if (c == 1)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }
                            if (c == 2)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 2)
                            {
                                if (c % 3 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 3 == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 3 == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }

                        }

                        if (space_point_g == 3)
                        {
                            if (c % 2 == 0)
                            {
                                //Console.Write("x10000000");
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            else
                            {
                                send_data_array_one10_c[c] = 0b00000000;
                                //print_content += "00000000-";
                            }
                        }

                        if (space_point_g == 4)
                        {
                            //10000000 00000000 00001000 00000000 00000000
                            //    0       1        2        3        4
                            //10000000 00000000 00001000 00000000  00000000
                            //    5        6        7        8        9
                            //10000000 00000000 00001000 00000000  00000000
                            //    10        11      12        13      14
                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }

                            if (c == 1)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 2)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }

                            if (c == 3)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 4)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 4)
                            {
                                if (c % 5 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 5 == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 5 == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }
                        }



                        /*
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b11111111;
                        }
                        if (space_point_g == 1)
                        {
                            if (c % 2 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 2)
                        {
                            if (c % 3 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 3)
                        {
                            if (c % 4 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 4)
                        {
                            if (c % 5 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }

                        */
                    }
                }

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                    ij += 160;
                }

                //T
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_c = jprint.AddPageDataPure(ColorChannel.C, p, dataSize);


                for (int c = 0; c < send_data_array_one10_c.Length; c++)
                {
                    send_data_array_one10_c[c] = 0x00;
                }

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }

                if (a_valid_c == 1)
                {
                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {

                        //间隔1
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b10001000;
                            //print_content += "10001000-";
                        }

                        if (space_point_g == 1)
                        {
                            send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                        }

                        if (space_point_g == 2)
                        {

                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            if (c == 1)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }
                            if (c == 2)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 2)
                            {
                                if (c % 3 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 3 == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 3 == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }

                        }

                        if (space_point_g == 3)
                        {
                            if (c % 2 == 0)
                            {
                                //Console.Write("x10000000");
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            else
                            {
                                send_data_array_one10_c[c] = 0b00000000;
                                //print_content += "00000000-";
                            }
                        }

                        if (space_point_g == 4)
                        {
                            //10000000 00000000 00001000 00000000 00000000
                            //    0       1        2        3        4
                            //10000000 00000000 00001000 00000000  00000000
                            //    5        6        7        8        9
                            //10000000 00000000 00001000 00000000  00000000
                            //    10        11      12        13      14
                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }

                            if (c == 1)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 2)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }

                            if (c == 3)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 4)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 4)
                            {
                                if (c % 5 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 5 == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 5 == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }
                        }






                        /*
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b11111111;
                            }
                            if (space_point_g == 1)
                            {
                                if (c % 2 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            if (space_point_g == 2)
                            {
                                if (c % 3 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            if (space_point_g == 3)
                            {
                                if (c % 4 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            if (space_point_g == 4)
                            {
                                if (c % 5 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }

                            */
                    }
                }

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                    ij += 160;
                }
                //C
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_m = jprint.AddPageDataPure(ColorChannel.M, p, dataSize);

                for (int c = 0; c < send_data_array_one10_c.Length; c++)
                {
                    send_data_array_one10_c[c] = 0x00;
                }

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }

                if (a_valid_g == 1)
                {
                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {

                        //间隔1
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b10001000;
                            //print_content += "10001000-";
                        }

                        if (space_point_g == 1)
                        {
                            send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                        }

                        if (space_point_g == 2)
                        {

                            //10000000 00001000 00000000 
                            // 0          1       2
                            //10000000 00001000 00000000 
                            // 3         4        5
                            //10000000 00001000 00000000  
                            // 6         7        8


                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            if (c == 1)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }
                            if (c == 2)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 2)
                            {
                                if (c % 3 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 3 == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 3 == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }

                        }

                        if (space_point_g == 3)
                        {
                            if (c % 2 == 0)
                            {
                                //Console.Write("x10000000");
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            else
                            {
                                send_data_array_one10_c[c] = 0b00000000;
                                //print_content += "00000000-";
                            }
                        }

                        if (space_point_g == 4)
                        {
                            //10000000 00000000 00001000 00000000 00000000
                            //    0       1        2        3        4
                            //10000000 00000000 00001000 00000000  00000000
                            //    5        6        7        8        9
                            //10000000 00000000 00001000 00000000  00000000
                            //    10        11      12        13      14
                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }

                            if (c == 1)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 2)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }

                            if (c == 3)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 4)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 4)
                            {
                                if (c % 5 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 5 == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 5 == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }
                        }



                        /*
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b11111111;
                        }
                        if (space_point_g == 1)
                        {
                            if (c % 2 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 2)
                        {
                            if (c % 3 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 3)
                        {
                            if (c % 4 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 4)
                        {
                            if (c % 5 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        */
                    }
                }

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                    ij += 160;
                }

                //G
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_y = jprint.AddPageDataPure(ColorChannel.Y, p, dataSize);

                for (int c = 0; c < send_data_array_one10_c.Length; c++)
                {
                    send_data_array_one10_c[c] = 0x00;
                }


                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }


                if (a_valid_all == 1)
                {
                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        //间隔1
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b10001000;
                            //print_content += "10001000-";
                        }

                        if (space_point_g == 1)
                        {
                            send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                        }

                        if (space_point_g == 2)
                        {

                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            if (c == 1)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }
                            if (c == 2)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 2)
                            {
                                if (c % 3 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 3 == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 3 == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }

                        }

                        if (space_point_g == 3)
                        {
                            if (c % 2 == 0)
                            {
                                //Console.Write("x10000000");
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                            else
                            {
                                send_data_array_one10_c[c] = 0b00000000;
                                //print_content += "00000000-";
                            }
                        }

                        if (space_point_g == 4)
                        {
                            //10000000 00000000 00001000 00000000 00000000
                            //    0       1        2        3        4
                            //10000000 00000000 00001000 00000000  00000000
                            //    5        6        7        8        9
                            //10000000 00000000 00001000 00000000  00000000
                            //    10        11      12        13      14
                            if (c == 0)
                            {
                                //print_content += "10000000-";
                                send_data_array_one10_c[c] = 0b10000000;
                            }

                            if (c == 1)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 2)
                            {
                                //print_content += "00001000-";
                                send_data_array_one10_c[c] = 0b00001000;
                            }

                            if (c == 3)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c == 4)
                            {
                                //print_content += "00000000-";
                                send_data_array_one10_c[c] = 0b00000000;
                            }

                            if (c > 4)
                            {
                                if (c % 5 == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c % 5 == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c % 5 == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c % 5 == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                            }
                        }



                        /*
                        if (space_point_g == 0)
                        {
                            send_data_array_one10_c[c] = 0b11111111;
                        }
                        if (space_point_g == 1)
                        {
                            if (c % 2 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 2)
                        {
                            if (c % 3 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 3)
                        {
                            if (c % 4 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        if (space_point_g == 4)
                        {
                            if (c % 5 == 0)
                            {
                                send_data_array_one10_c[c] = 0b10000000;
                            }
                        }
                        */



                    }
                }

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                    ij += 160;
                }
                //White
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                var rc_all = jprint.AddPageDataPure(ColorChannel.White, p, dataSize);


                if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    i++;
                    //Thread.Sleep(200);
                    Console.WriteLine("发送k打印数据成功:" + i);
                }
                else if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    Console.WriteLine("发送k打印数据超时:" + i);
                    //Thread.Sleep(200);
                    continue;
                }


                Thread.Sleep(20);
            }
            Marshal.FreeHGlobal(p);
            result = true;
            return result;
        }

        /// <summary>
        /// 调试Y的位置
        /// </summary>
        /// <returns></returns>
        void SendPageDataWork_Test_Y_Position()
        {
            //print_content_list_g = CommonData.PrintContentList_g;
            //if (print_content_list_g.Count == 0)
            //{
            //    return false;
            //}
            ////每页打印的高度，也就是行数
            //jobInfo.height = print_content_list_g.Count;
            //jobInfo.num_pages = num_pages_g;

            while (true)
            {
                while (m_test_print_add_content_flage)
                {
                    Thread.Sleep(1);
                }
                num_pages_g = 1;
                bool result = false;
                int stride = (jobInfo.width + 31) / 32 * 4;
                int dataSize = stride * jobInfo.height;
                byte[] send_data_array_one160 = new byte[dataSize];
                byte[] send_data_array_one40 = new byte[40];
                var p = Marshal.AllocHGlobal(dataSize);
                //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
                //IntPtr data = Marshal.AllocHGlobal(dataSize);
                //喷的区域的高度
                int length = line_length_g;
                byte[] send_data_array_one160_new = new byte[160];
                byte[] send_data_array_one10_k = new byte[length];
                byte[] send_data_array_one10_c = new byte[length];
                byte[] send_data_array_one10_m = new byte[length];
                byte[] send_data_array_one10_y = new byte[length];
                byte[] send_data_array_one10_white = new byte[length];

                for (int c = 0; c < send_data_array_one10_c.Length; c++)
                {
                    send_data_array_one10_c[c] = 0x00;
                }

                for (int i = 0; i < num_pages_g;)
                {
                    for (int ij = 0; ij < dataSize; ij++)
                    {
                        send_data_array_one160[ij] = 0x00;
                    }

                    if (a_valid_a == 1)
                    {
                        for (int c = 0; c < send_data_array_one10_c.Length; c++)
                        {
                            //间隔1
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b10001000;
                                //print_content += "10001000-";
                            }
                            if (space_point_g == 1)
                            {
                                send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                            }
                            if (space_point_g == 2)
                            {
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }
                                if (c > 2)
                                {
                                    if (c % 3 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 3 == 1)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 3 == 2)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }
                            if (space_point_g == 3)
                            {
                                if (c % 2 == 0)
                                {
                                    //Console.Write("x10000000");
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                else
                                {
                                    send_data_array_one10_c[c] = 0b00000000;
                                    //print_content += "00000000-";
                                }
                            }
                            if (space_point_g == 4)
                            {
                                //10000000 00000000 00001000 00000000 00000000
                                //    0       1        2        3        4
                                //10000000 00000000 00001000 00000000  00000000
                                //    5        6        7        8        9
                                //10000000 00000000 00001000 00000000  00000000
                                //    10        11      12        13      14
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }

                                if (c == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 4)
                                {
                                    if (c % 5 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 5 == 1)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 2)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 5 == 3)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 4)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }
                        }
                    }

                    for (int ij = 0; ij < dataSize;)
                    {
                        Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij, length);
                        ij += 160;
                    }

                    //A
                    Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                    var rc_k = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);

                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        send_data_array_one10_c[c] = 0x00;
                    }

                    for (int ij = 0; ij < dataSize; ij++)
                    {
                        send_data_array_one160[ij] = 0x00;
                    }


                    if (a_valid_t == 1)
                    {
                        for (int c = 0; c < send_data_array_one10_c.Length; c++)
                        {
                            //间隔1
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b10001000;
                                //print_content += "10001000-";
                            }

                            if (space_point_g == 1)
                            {
                                send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                            }

                            if (space_point_g == 2)
                            {

                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 2)
                                {
                                    if (c % 3 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 3 == 1)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 3 == 2)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }

                            }

                            if (space_point_g == 3)
                            {
                                if (c % 2 == 0)
                                {
                                    //Console.Write("x10000000");
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                else
                                {
                                    send_data_array_one10_c[c] = 0b00000000;
                                    //print_content += "00000000-";
                                }
                            }

                            if (space_point_g == 4)
                            {
                                //10000000 00000000 00001000 00000000 00000000
                                //    0       1        2        3        4
                                //10000000 00000000 00001000 00000000  00000000
                                //    5        6        7        8        9
                                //10000000 00000000 00001000 00000000  00000000
                                //    10        11      12        13      14
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }

                                if (c == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }

                                if (c == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 4)
                                {
                                    if (c % 5 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 5 == 1)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 2)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 5 == 3)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 4)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }
                        }
                    }

                    for (int ij = 0; ij < dataSize;)
                    {
                        Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                        ij += 160;
                    }

                    //T
                    Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                    var rc_c = jprint.AddPageDataPure(ColorChannel.C, p, dataSize);


                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        send_data_array_one10_c[c] = 0x00;
                    }

                    for (int ij = 0; ij < dataSize; ij++)
                    {
                        send_data_array_one160[ij] = 0x00;
                    }

                    if (a_valid_c == 1)
                    {
                        for (int c = 0; c < send_data_array_one10_c.Length; c++)
                        {

                            //间隔1
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b10001000;
                                //print_content += "10001000-";
                            }

                            if (space_point_g == 1)
                            {
                                send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                            }

                            if (space_point_g == 2)
                            {

                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 2)
                                {
                                    if (c % 3 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 3 == 1)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 3 == 2)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }

                            }

                            if (space_point_g == 3)
                            {
                                if (c % 2 == 0)
                                {
                                    //Console.Write("x10000000");
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                else
                                {
                                    send_data_array_one10_c[c] = 0b00000000;
                                    //print_content += "00000000-";
                                }
                            }

                            if (space_point_g == 4)
                            {
                                //10000000 00000000 00001000 00000000 00000000
                                //    0       1        2        3        4
                                //10000000 00000000 00001000 00000000  00000000
                                //    5        6        7        8        9
                                //10000000 00000000 00001000 00000000  00000000
                                //    10        11      12        13      14
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }

                                if (c == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }

                                if (c == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 4)
                                {
                                    if (c % 5 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 5 == 1)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 2)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 5 == 3)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 4)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }

                        }
                    }

                    for (int ij = 0; ij < dataSize;)
                    {
                        Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                        ij += 160;
                    }
                    //C
                    Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                    var rc_m = jprint.AddPageDataPure(ColorChannel.M, p, dataSize);

                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        send_data_array_one10_c[c] = 0x00;
                    }

                    for (int ij = 0; ij < dataSize; ij++)
                    {
                        send_data_array_one160[ij] = 0x00;
                    }

                    if (a_valid_g == 1)
                    {
                        for (int c = 0; c < send_data_array_one10_c.Length; c++)
                        {

                            //间隔1
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b10001000;
                                //print_content += "10001000-";
                            }

                            if (space_point_g == 1)
                            {
                                send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                            }

                            if (space_point_g == 2)
                            {
                                //10000000 00001000 00000000 
                                // 0          1       2
                                //10000000 00001000 00000000 
                                // 3         4        5
                                //10000000 00001000 00000000  
                                // 6         7        8
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 2)
                                {
                                    if (c % 3 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 3 == 1)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 3 == 2)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }
                            if (space_point_g == 3)
                            {
                                if (c % 2 == 0)
                                {
                                    //Console.Write("x10000000");
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                else
                                {
                                    send_data_array_one10_c[c] = 0b00000000;
                                    //print_content += "00000000-";
                                }
                            }

                            if (space_point_g == 4)
                            {
                                //10000000 00000000 00001000 00000000 00000000
                                //    0       1        2        3        4
                                //10000000 00000000 00001000 00000000  00000000
                                //    5        6        7        8        9
                                //10000000 00000000 00001000 00000000  00000000
                                //    10        11      12        13      14
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }

                                if (c == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }

                                if (c == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 4)
                                {
                                    if (c % 5 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 5 == 1)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 2)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 5 == 3)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 4)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }
                        }
                    }

                    for (int ij = 0; ij < dataSize;)
                    {
                        Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                        ij += 160;
                    }

                    //G
                    Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                    var rc_y = jprint.AddPageDataPure(ColorChannel.Y, p, dataSize);

                    for (int c = 0; c < send_data_array_one10_c.Length; c++)
                    {
                        send_data_array_one10_c[c] = 0x00;
                    }


                    for (int ij = 0; ij < dataSize; ij++)
                    {
                        send_data_array_one160[ij] = 0x00;
                    }


                    if (a_valid_all == 1)
                    {
                        for (int c = 0; c < send_data_array_one10_c.Length; c++)
                        {
                            //间隔1
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b10001000;
                                //print_content += "10001000-";
                            }

                            if (space_point_g == 1)
                            {
                                send_data_array_one10_c[c] = 0b10000000;// "10000000-";
                            }

                            if (space_point_g == 2)
                            {

                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                if (c == 1)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }
                                if (c == 2)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 2)
                                {
                                    if (c % 3 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 3 == 1)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 3 == 2)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }

                            }

                            if (space_point_g == 3)
                            {
                                if (c % 2 == 0)
                                {
                                    //Console.Write("x10000000");
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                                else
                                {
                                    send_data_array_one10_c[c] = 0b00000000;
                                    //print_content += "00000000-";
                                }
                            }

                            if (space_point_g == 4)
                            {
                                //10000000 00000000 00001000 00000000 00000000
                                //    0       1        2        3        4
                                //10000000 00000000 00001000 00000000  00000000
                                //    5        6        7        8        9
                                //10000000 00000000 00001000 00000000  00000000
                                //    10        11      12        13      14
                                if (c == 0)
                                {
                                    //print_content += "10000000-";
                                    send_data_array_one10_c[c] = 0b10000000;
                                }

                                if (c == 1)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 2)
                                {
                                    //print_content += "00001000-";
                                    send_data_array_one10_c[c] = 0b00001000;
                                }

                                if (c == 3)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c == 4)
                                {
                                    //print_content += "00000000-";
                                    send_data_array_one10_c[c] = 0b00000000;
                                }

                                if (c > 4)
                                {
                                    if (c % 5 == 0)
                                    {
                                        //print_content += "10000000-";
                                        send_data_array_one10_c[c] = 0b10000000;
                                    }
                                    if (c % 5 == 1)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 2)
                                    {
                                        //print_content += "00001000-";
                                        send_data_array_one10_c[c] = 0b00001000;
                                    }
                                    if (c % 5 == 3)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                    if (c % 5 == 4)
                                    {
                                        //print_content += "00000000-";
                                        send_data_array_one10_c[c] = 0b00000000;
                                    }
                                }
                            }



                            /*
                            if (space_point_g == 0)
                            {
                                send_data_array_one10_c[c] = 0b11111111;
                            }
                            if (space_point_g == 1)
                            {
                                if (c % 2 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            if (space_point_g == 2)
                            {
                                if (c % 3 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            if (space_point_g == 3)
                            {
                                if (c % 4 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            if (space_point_g == 4)
                            {
                                if (c % 5 == 0)
                                {
                                    send_data_array_one10_c[c] = 0b10000000;
                                }
                            }
                            */



                        }
                    }

                    for (int ij = 0; ij < dataSize;)
                    {
                        Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 0, length);
                        ij += 160;
                    }


                    //White
                    Marshal.Copy(send_data_array_one160, 0, p, dataSize);
                    var rc_all = jprint.AddPageDataPure(ColorChannel.White, p, dataSize);


                    if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                    {
                        i++;
                        //Thread.Sleep(200);
                        Console.WriteLine("发送k打印数据成功:" + i);
                    }
                    else if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                    {
                        Console.WriteLine("发送k打印数据超时:" + i);
                        //Thread.Sleep(200);
                        continue;
                    }


                    Thread.Sleep(20);
                }
                Marshal.FreeHGlobal(p);
                result = true;
                //return result;

                m_test_print_add_content_flage = true;
                Thread.Sleep(500);
            }


        }

        static void SendPageDataWork__2()
        {
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            //for (int i = 0; i < send_data_array_one160.Length; i++)
            //{
            //    send_data_array_one160[i] = 255;
            //}
            for (int i = 0; i < 80; i++)
            {
                send_data_array_one160[i] = 0x88;
            }

            for (int jj = 0; jj < 10000; jj++)
            {
                for (int i = 0; i < 80; i++)
                {
                    send_data_array_one160[i] = 0x88;
                }

                string send_data = System.Text.Encoding.ASCII.GetString(send_data_array_one160);
                IntPtr p = Marshal.StringToHGlobalAnsi(send_data);
                var rc = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);
                if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                }
                Marshal.FreeHGlobal(p);
                Thread.Sleep(10);
            }

            /*
            string send_data = System.Text.Encoding.ASCII.GetString(send_data_array_one160);
            IntPtr p = Marshal.StringToHGlobalAnsi(send_data);
            //IntPtr data = Marshal.AllocHGlobal(dataSize);
            for (int i = 0; i < jobInfo.num_pages;)
            {
                var rc = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);
                if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    i++;
                }
                else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    continue;
                }
            }

            Marshal.FreeHGlobal(p);
            */
            //}

        }
        //bool CheckRunCycle(List<PrintContent> print_content_list_para2, int index)
        //{

        //}
        /// <summary>
        /// 打印前准备
        /// </summary>
        public void ReadPrint()
        {
            int mainBoardId = 0;
            int headBoardId = 0;
            bool mbIsConnected = jprint.MainBoardIsConnected(mainBoardId);
            bool hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId);
            if (mbIsConnected == false || hbIsConnected == false)
            {
                Console.WriteLine("板卡连接失败");
                //break;
            }
            Console.WriteLine("板卡连接成功");
            var rc = jprint.Start(jobInfo);
            if (rc != JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
            {
                Console.WriteLine($"启动打印失败: {rc}");
                //break;
            }
            Console.WriteLine("启动打印成功");
        }

        //添加每页打印内容
        public bool PrintOnePageData(int page_index)
        {
            bool result = false;
            try
            {
                if (print_content_list_g.Count == 0)
                    return false;
                num_pages_g = print_content_list_g.Count;

                //int stride = (jobInfo.width + 31) / 32 * 4;
                //int dataSize = stride * jobInfo.height;
                //byte[] send_data_array_one160 = new byte[dataSize];
                //byte[] send_data_array_one40 = new byte[40];
                int dataSize = print_content_list_g[page_index].Print_array_a.Length;
                byte[] one_color_print_content = new byte[dataSize];
                var p = Marshal.AllocHGlobal(dataSize);
                //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
                //IntPtr data = Marshal.AllocHGlobal(dataSize);
                //喷的区域的高度
                //调试用
                num_pages_g = 1;
                for (int i = 0; i < num_pages_g;)
                {
                    //byte[] print_all = new byte[print_content_list_g[page_index].Print_array_a.Length];
                    //Array.Copy(print_all, 0, one_color_print_content, 0, dataSize);
                    //A
                    Array.Copy(print_content_list_g[page_index].Print_array_a, 0, one_color_print_content, 0, dataSize);
                    Marshal.Copy(one_color_print_content, 0, p, dataSize);
                    var rc_k = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);
                    //T
                    //print_all = new byte[print_content_list_g[page_index].Print_array_a.Length];
                    //Array.Copy(print_all, 0, one_color_print_content, 0, dataSize);
                    Array.Copy(print_content_list_g[page_index].Print_array_t, 0, one_color_print_content, 0, dataSize);
                    Marshal.Copy(one_color_print_content, 0, p, dataSize);
                    var rc_c = jprint.AddPageDataPure(ColorChannel.C, p, dataSize);
                    //Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij + 40, length);
                    //C
                    //print_all = new byte[print_content_list_g[page_index].Print_array_a.Length];
                    //Array.Copy(print_all, 0, one_color_print_content, 0, dataSize);
                    Array.Copy(print_content_list_g[page_index].Print_array_c, 0, one_color_print_content, 0, dataSize);
                    Marshal.Copy(one_color_print_content, 0, p, dataSize);
                    var rc_m = jprint.AddPageDataPure(ColorChannel.M, p, dataSize);
                    //G
                    //print_all = new byte[print_content_list_g[page_index].Print_array_a.Length];
                    //Array.Copy(print_all, 0, one_color_print_content, 0, dataSize);
                    Array.Copy(print_content_list_g[page_index].Print_array_g, 0, one_color_print_content, 0, dataSize);
                    Marshal.Copy(one_color_print_content, 0, p, dataSize);
                    var rc_y = jprint.AddPageDataPure(ColorChannel.Y, p, dataSize);

                    //White 
                    Array.Copy(print_content_list_g[page_index].Print_array_h, 0, one_color_print_content, 0, dataSize);
                    Marshal.Copy(one_color_print_content, 0, p, dataSize);
                    var rc_all = jprint.AddPageDataPure(ColorChannel.White, p, dataSize);
                    if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                    {
                        i++;
                        //Thread.Sleep(200);
                        Console.WriteLine("发送k打印数据成功:" + i);
                    }
                    else if (rc_k == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                    {
                        Console.WriteLine("发送k打印数据超时:" + i);
                        //Thread.Sleep(200);
                        continue;
                    }
                    Thread.Sleep(20);
                }

                Marshal.FreeHGlobal(p);

            }
            catch (Exception ex1)
            {
                var st = new StackTrace(ex1, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                var sw = new System.IO.StreamWriter("打印异常11.txt", true);
                sw.WriteLine(
                    DateTime.Now.ToString() + "\r\n"
                    + "打印异常11:" + "\r\n"
                    + ex1.Message + "\r\n"
                    + ex1.InnerException + "\r\n"
                    + ex1.Source + "\r\n"
                    + page_index + "\r\n"
                    + frame + "\r\n"
                    + line);
                sw.Close();
                return true;
            }

            result = true;
            return result;
        }

        public struct COPYDATASTRUCT
        {
            public IntPtr dwData;
            public int cbData;
            [MarshalAs(UnmanagedType.LPStr)] public string lpData;
        }
        const int WM_Lbutton = 0x201; //定义了鼠标的左键点击消息
        public const int USER = 0x0400; // 是windows系统定义的用户消息
        const int WM_MYMSG = 10000; //定义了鼠标的左键点击消息
        const int WM_COPYDATA = 0x004A;

        [DllImport("User32.dll", EntryPoint = "SendMessage")]
        private static extern int SendMessage(int hWnd, int Msg, int wParam, ref COPYDATASTRUCT lParam);

        [DllImport("User32.dll")]
        public static extern int FindWindow(string lpClassName, string lpWindowName);

        /// <summary>
        /// 发送完成
        /// </summary>
        public void SendMessageToQT(string msg_type)
        {
            String strSent = msg_type;//"发送完成";
            //如果为实时模式的，那么发送  strSent = "实时模式发送完成";
            int WINDOW_HANDLE = FindWindow(null, "ShowPic");
            if (WINDOW_HANDLE != 0)
            {
                byte[] sarr = System.Text.Encoding.Default.GetBytes(strSent);
                int len = sarr.Length;
                COPYDATASTRUCT cds;
                cds.dwData = (IntPtr)Convert.ToInt16(strSent.Length);//可以是任意值
                cds.cbData = len + 1;//指定lpData内存区域的字节数
                cds.lpData = strSent;//发送给目标窗口所在进程的数据
                SendMessage(WINDOW_HANDLE, WM_COPYDATA, 0, ref cds);
                SendMessage(WINDOW_HANDLE, WM_MYMSG, 0, ref cds);
            }
        }


        public void SendShowPicPath(string path)
        {
            SendMessageToQT(path);
        }

        static List<SendOffset> send_offset_list_g_query = new List<SendOffset>();
        void StartToRun_New()
        {
            while (true)
            {
            //跳到这里来
            RESTART:
                while (m_total_run_stop_flage)
                {
                    try
                    {
                        print_content_list_g = CommonData.PrintContentList_g;
                        if (print_content_list_g.Count == 0)
                        {
                            return;
                        }
                        cycle_number = print_content_list_g.Count;
                        //cycle_number = cycle_finish_status_list.Count;
                        cycle_finish_status_list = new List<int>();
                        for (int j = 0; j < cycle_number; j++)
                        {
                            cycle_finish_status_list.Add(1);
                        }

                        for (int i = 0; i < cycle_number;)
                        {
                            string loop_status_show = "";

                            FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                            {
                                int current_number = i + 1;
                                loop_status_show = String.Format("循环状态显示：总{0},当前{1}", cycle_number, current_number);
                                label64.Text = loop_status_show;
                            }));
                            Console.WriteLine("总共循环数" + cycle_number + "当前第几个循环" + i + "***********************************************************************************************************************");
                            OneActive("下压芯片");
                            Thread.Sleep(1000);
                            bool return_value = true;
                            while (return_value)
                            {
                                //进行条件判断，直到OneActive完成了，跳出等待
                                if (send_offset_list_g.Count == 0 && run_flage)
                                {
                                    return_value = false;
                                }

                                if (!m_total_run_stop_flage)
                                {
                                    send_offset_list_g = new List<SendOffset>();
                                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                    {
                                        int current_numbera = i + 1;
                                        loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                        label64.Text = loop_status_show;
                                        ucBtnImg41.Enabled = true;
                                    }));
                                    goto RESTART;
                                }
                                Thread.Sleep(100);
                            }

                            //TCA
                            //走TCA液路
                            return_value = true;
                            this.Invoke(new Action(() =>
                            {
                                RunOther("TCA");
                            }));

                            while (return_value)
                            {
                                int total_number = task_list_g.Count;
                                int finish_number = task_list_g.FindAll(o => o.Status == 3).Count;
                                if (total_number == finish_number && run_flage)
                                {
                                    return_value = false;
                                }

                                if (!m_total_run_stop_flage)
                                {
                                    task_list_g = new List<GetOrderClsSimple>();
                                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                    {
                                        int current_numbera = i + 1;
                                        loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                        label64.Text = loop_status_show;
                                        ucBtnImg41.Enabled = true;
                                    }));
                                    goto RESTART;
                                }

                                Thread.Sleep(100);
                            }

                            //抬Z2，走Z2复位
                            SendZAxisMotorFindZero(2);

                            Thread.Sleep(7000);

                            for (int jjjj = 0; jjjj < one_page_loop_number; jjjj++)
                            {
                                OneActive("打印开始");
                                Thread.Sleep(1000);
                                return_value = false;
                                return_value = true;
                                //打印开始位置（A）
                                while (return_value)
                                {
                                    //进行条件判断，直到OneActive完成了，跳出等待
                                    if (send_offset_list_g.Count == 0 && run_flage)
                                    {
                                        return_value = false;
                                    }

                                    if (!m_total_run_stop_flage)
                                    {
                                        send_offset_list_g = new List<SendOffset>();
                                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                        {
                                            int current_numbera = i + 1;
                                            loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                            label64.Text = loop_status_show;
                                            ucBtnImg41.Enabled = true;
                                        }));
                                        goto RESTART;
                                    }
                                    Thread.Sleep(200);
                                }


                                try
                                {

                                    //ReadPrint();
                                    AddPrintOneStepSystem2(true);
                                    if (!m_total_run_stop_flage)
                                    {
                                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                        {
                                            int current_numbera = i + 1;
                                            loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                            label64.Text = loop_status_show;
                                            ucBtnImg41.Enabled = true;
                                        }));
                                        goto RESTART;
                                    }

                                    PrintOnePageData(i);

                                }
                                catch (Exception ex1)
                                {

                                    var st = new StackTrace(ex1, true);
                                    var frame = st.GetFrame(0);
                                    var line = frame.GetFileLineNumber();
                                    var sw = new System.IO.StreamWriter("打印异常.txt", true);

                                    sw.WriteLine(
                                        DateTime.Now.ToString() + "\r\n"
                                         + "打印异常:" + "\r\n"
                                        + ex1.Message + "\r\n"
                                        + ex1.InnerException + "\r\n"
                                        + ex1.Source + "\r\n"
                                        + frame + "\r\n"
                                        + line);
                                    sw.Close();

                                }


                                //等待打印内容添加完成
                                Thread.Sleep(5000);
                                if (!m_total_run_stop_flage)
                                {
                                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                    {
                                        int current_numbera = i + 1;
                                        loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                        label64.Text = loop_status_show;
                                        ucBtnImg41.Enabled = true;
                                    }));
                                    goto RESTART;
                                }
                                //m_test_print_add_content_flage = false;
                                //SendPageDataWork_Test_Y_Position(); 
                                send_offset_list_g = new List<SendOffset>();
                                Thread.Sleep(160);
                                TriggerPrint();
                                Thread.Sleep(260);
                                TriggerPrint();
                                send_offset_list_g = new List<SendOffset>();
                                //打印结束位置
                                Thread.Sleep(260);
                                //loop_send_query_z_axis_position = false;
                                OneActive("打印结束");
                                //清空查询指令
                                return_value = false;
                                return_value = true;
                                Thread.Sleep(2000);
                                //loop_send_query_z_axis_position = true;
                                //print_front_delay_time_g = int.Parse(textBox_PrintFrontDelayTime.Text);
                                //Thread.Sleep(print_front_delay_time_g);
                                //开始打印
                                //StartPrint(print_content_list_para2, 0);
                                while (return_value)
                                {
                                    //进行条件判断，直到OneActive完成了，跳出等待
                                    if (send_offset_list_g.Count == 0 && run_flage)
                                    {
                                        return_value = false;
                                    }

                                    if (!m_total_run_stop_flage)
                                    {
                                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                        {
                                            send_offset_list_g = new List<SendOffset>();
                                            int current_numbera = i + 1;
                                            loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                            label64.Text = loop_status_show;
                                            ucBtnImg41.Enabled = true;
                                        }));
                                        goto RESTART;
                                    }
                                    Thread.Sleep(200);
                                }
                                Thread.Sleep(1000);
                            }

                            Thread.Sleep(2000);

                            if (system_type.Equals("1"))
                            {
                                //再回到打印开始位置
                                send_offset_list_g = new List<SendOffset>();
                                OneActive("打印开始");
                                Thread.Sleep(1000);
                                return_value = true;
                                while (return_value)
                                {
                                    //进行条件判断，直到OneActive完成了，跳出等待
                                    if (send_offset_list_g.Count == 0 && run_flage)
                                    {
                                        return_value = false;
                                    }
                                    if (!m_total_run_stop_flage)
                                    {
                                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                        {
                                            send_offset_list_g = new List<SendOffset>();
                                            int current_numbera = i + 1;
                                            loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                            label64.Text = loop_status_show;
                                            ucBtnImg41.Enabled = true;
                                        }));
                                        goto RESTART;
                                    }
                                    Thread.Sleep(100);
                                }
                            }



                            //走显微镜位置
                            return_value = true;
                            OneActive("显微镜观察");
                            Thread.Sleep(1000);
                            while (return_value)
                            {
                                //进行条件判断，直到OneActive完成了，跳出等待
                                if (send_offset_list_g.Count == 0 && run_flage)
                                {
                                    return_value = false;
                                }
                                if (!m_total_run_stop_flage)
                                {
                                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                    {
                                        send_offset_list_g = new List<SendOffset>();
                                        int current_numbera = i + 1;
                                        loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                        label64.Text = loop_status_show;
                                        ucBtnImg41.Enabled = true;
                                    }));
                                    goto RESTART;
                                }
                                Thread.Sleep(100);
                            }

                            Thread.Sleep(4000);

                            //CamSavePic();

                            m_save_pic_flage = true;


                            //反应时间4 - 5分钟
                            Thread.Sleep(coupling_g);
                            if (system_type.Equals("1"))
                            {
                                System1FindXYRZero();
                            }
                            else
                            {
                                //System2FindXYRZero();
                            }
                            Thread.Sleep(find_zero_delay_time);
                            //  显微镜复位
                            //抬Z3，走Z3复位
                            //SendZAxisMotorFindZero(3);
                            //Thread.Sleep(8000);
                            //Thread.Sleep(2000);
                            //下压反应腔位置
                            OneActive("下压芯片");
                            return_value = true;
                            Thread.Sleep(2000);
                            while (return_value)
                            {
                                //进行条件判断，直到OneActive完成了，跳出等待
                                if (send_offset_list_g.Count == 0 && run_flage)
                                {
                                    return_value = false;
                                }
                                if (!m_total_run_stop_flage)
                                {
                                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                    {
                                        send_offset_list_g = new List<SendOffset>();
                                        int current_numbera = i + 1;
                                        loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                        label64.Text = loop_status_show;
                                        ucBtnImg41.Enabled = true;
                                    }));
                                    goto RESTART;
                                }
                                Thread.Sleep(200);
                            }
                            return_value = true;
                            this.Invoke(new Action(() =>
                            {
                                RunOther("WASHCAPACAPBOXI");
                            }));
                            while (return_value)
                            {
                                int total_number = task_list_g.Count;
                                int finish_number = task_list_g.FindAll(o => o.Status == 3).Count;
                                if (total_number == finish_number && run_flage)
                                {
                                    return_value = false;
                                }

                                if (!m_total_run_stop_flage)
                                {
                                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                                    {
                                        task_list_g = new List<GetOrderClsSimple>();//new List<SendOffset>();
                                        int current_numbera = i + 1;
                                        loop_status_show = String.Format("循环状态显示：总{0},当前{1},{2}", cycle_number, current_numbera, "手动");
                                        label64.Text = loop_status_show;
                                        ucBtnImg41.Enabled = true;
                                    }));
                                    goto RESTART;
                                }
                                Thread.Sleep(100);
                            }
                            //抬Z3，走Z3复位
                            //SendZAxisMotorFindZero(2);
                            //Thread.Sleep(8000);
                            //走WASHCAPACAPBOXI
                            run_flage = true;
                            //每一个cycle完成
                            if (cycle_finish_status_list.Count > 0)
                            {
                                cycle_finish_status_list[i] = 3;

                            }
                            i++;
                            //return return_value;
                        }

                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                        {
                            //MessageBox.Show("运行完成");
                            FrmDialog.ShowDialog(this, "运行完成", "提示");
                            ucBtnImg41.Enabled = true;
                        }));
                        m_total_run_stop_flage = false;
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                        var st = new StackTrace(ex, true);
                        var frame = st.GetFrame(0);
                        var line = frame.GetFileLineNumber();
                        var sw = new System.IO.StreamWriter("StartToRunNEW异常.txt", true);
                        sw.WriteLine(
                            DateTime.Now.ToString() + "\r\n"
                             + "异常:" + "\r\n"
                            + ex.Message + "\r\n"
                            + "read_sx:" + read_sx + "\r\n"
                            + ex.Source + "\r\n"
                            + frame + "\r\n"
                            + line);
                        sw.Close();
                        //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                    }
                }
                Thread.Sleep(10);
            }
        }
        static void SendPageDataWork_1(List<PrintContent> print_content_list_para2)
        {
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            for (int jj = 0; jj < print_content_list_para2.Count;)
            {
                //for (int i = 0; i < send_data_array_one160.Length; i++)
                //{
                //    send_data_array_one160[i] = 0xFF;
                //}
                PrintInitian();
                Array.Copy(print_content_list_para2[jj].Print_array_a, send_data_array_one160, print_content_list_para2[jj].Print_array_a.Length);

                string send_data = System.Text.Encoding.ASCII.GetString(send_data_array_one160);
                IntPtr p = Marshal.StringToHGlobalAnsi(send_data);
                var rc = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);
                if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    jj++;
                    Console.WriteLine("发送打印数据成功:" + jj);
                }
                else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    Console.WriteLine("发送打印数据超时:" + jj);
                    Marshal.FreeHGlobal(p);
                    //Thread.Sleep(100);
                    continue;
                }
                Marshal.FreeHGlobal(p);
            }
        }

        static bool SendPageDataWorkNew(List<PrintContent> aa)
        {
            bool result = false;

            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;

            byte[] send_data_array_one160 = new byte[dataSize];

            //for (int i = 0; i < send_data_array_one160.Length; i++)
            //{
            //    send_data_array_one160[i] = 255;
            //}

            for (int i = 0; i < 40; i++)
            {
                send_data_array_one160[i] = 0x88;
            }
            //Array.Copy(send_data_array_one160, send_data_array_one160_new, send_data_array_one160.Length);

            ////IntPtr data = Marshal.AllocHGlobal(dataSize);

            //var rc = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);

            //if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
            //{
            //    Console.WriteLine("打印成功");
            //    result = true;
            //}
            //else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
            //{
            //    Console.WriteLine("打印失败");
            //    result = false;
            //}

            for (int jj = 0; jj < aa.Count; jj++)
            {
                string send_data = System.Text.Encoding.ASCII.GetString(send_data_array_one160);
                IntPtr p = Marshal.StringToHGlobalAnsi(send_data);

                var rc = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);
                if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    //i++;
                }
                else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    continue;
                }
                Marshal.FreeHGlobal(p);
            }
            //for (int i = 0; i < jobInfo.num_pages;)
            //{
            //    var rc = jprint.AddPageDataPure(ColorChannel.K, p, dataSize);
            //    if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
            //    {
            //        i++;
            //    }
            //    else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
            //    {
            //        continue;
            //    }
            //}

            return result;
        }
        void SendPageDataWork(byte[] send_data_array_one160)
        {
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(send_data_array_one160.Length);
            Marshal.Copy(send_data_array_one160, 0, unmanagedPointer, send_data_array_one160.Length);
            // Call unmanaged code
            for (int i = 0; i < 100; i++)
            {
                var rc = jprint.AddPageDataPure(ColorChannel.K, unmanagedPointer, send_data_array_one160.Length);
                if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    Console.WriteLine("打印成功");
                }
                else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
                {
                    Console.WriteLine("打印失败");
                }
                Thread.Sleep(100);
            }
            Marshal.FreeHGlobal(unmanagedPointer);
        }
        static bool SendPageDataWorkA(byte[] send_data_array_one160)
        {
            bool result = false;
            IntPtr unmanagedPointer = Marshal.AllocHGlobal(send_data_array_one160.Length);
            Marshal.Copy(send_data_array_one160, 0, unmanagedPointer, send_data_array_one160.Length);
            // Call unmanaged code
            var rc = jprint.AddPageDataPure(ColorChannel.K, unmanagedPointer, send_data_array_one160.Length);
            if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
            {
                Console.WriteLine("打印成功");
                result = true;
            }
            else if (rc == JytPrintEngineWrapper.PRINTENGINE_RESULT_TIMEOUT)
            {
                Console.WriteLine("打印失败");
                result = false;
            }

            Thread.Sleep(1000);//加大发送间隔，可以看到效果，正式运行时这个时间要可以调节，通过他来控制发送的运动的协调
            Marshal.FreeHGlobal(unmanagedPointer);
            return result;
        }

        void SendPrintDataWork()
        {
            byte[] send_data_array = new byte[170];
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
            for (int i = 0; i < send_data_array.Length; i = i + 40)
            {
                byte[] send_data_array_one160 = new byte[160];
                for (int j = 0; j < 160; j++)
                {
                    if (j < 40)
                    {
                        //send_data_array_one160[j] = (byte)((send_data_array[j + i]) - loop_num);
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

                //每次取出160有效的但是要组成160,只是前40个字节有效
                while (true)
                {
                    int rc = jprint.AddPageDataPure(ColorChannel.K, p, 160);//JytPrintEngine_AddPageDataPure(handle, ColorChannel::K, data.get(), data_size);
                    if (rc == JYT_RESULT_SUCCESS)
                    {
                        // 发送成功
                        break;
                    }
                    else if (rc == JYT_RESULT_TIMEOUT)
                    {
                        // 超时需要重新发送
                        continue;
                    }
                }
                Marshal.FreeHGlobal(p);
                loop_num++;
            }

        }
        private void button2_Click(object sender, EventArgs e)
        {

            byte[] send_data_array = new byte[170];
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
            //byte[] send_data_array = new byte[1280*1000];
            //for (int i = 0; i < 1280*1000; i++)
            //{
            //    send_data_array[i] = 255;
            //}
            //string send_data = System.Text.Encoding.ASCII.GetString(send_data_array);
        }

        private void ucBtnImg1_BtnClick(object sender, EventArgs e)
        {
            //InitializePrint(1);
            InitializePrint(2);
        }

        private void ucBtnImg2_BtnClick(object sender, EventArgs e)
        {
            int main_board_number = int.Parse(ucTextBoxExMainBoard.InputText.ToString());
            int head_board_number = int.Parse(ucTextBoxExHeadBoard.InputText.ToString());
            double temperature = 0;
            bool temp_result = jprint.GetHeadTemperature(main_board_number, head_board_number, out temperature);
            temp_result = jprint.GetTankTemperature(main_board_number, head_board_number, out temperature);
            //获取喷头当前温度
        }

        private void ucBtnImg3_BtnClick(object sender, EventArgs e)
        {
            int main_board_number = int.Parse(ucTextBoxExMainBoard.InputText.ToString());
            int np_board_number = int.Parse(ucTextBoxExHeadBoard.InputText.ToString());
            int channel_index = int.Parse(ucTextBoxExChannel.InputText.ToString());
            double np_value = double.Parse(ucTextBoxEx2.InputText.ToString());
            bool temp_result = jprint.SetNpTargetValue(main_board_number, np_board_number, channel_index, np_value);
        }
        //开始闪喷标识
        /// <summary>
        /// 开始闪喷标识
        /// </summary>
        public static bool start_flash_print_flage = true;
        private void ucBtnImg4_BtnClick(object sender, EventArgs e)
        {
            //20220514 将写寄存器改为闪喷
            //int main_board_number = int.Parse(ucTextBoxExMainBoard.InputText.ToString());
            //int head_board_number = int.Parse(ucTextBoxExHeadBoard.InputText.ToString());
            //int value = int.Parse(ucTextBoxEx2.InputText.ToString());
            //int channel = int.Parse(ucTextBoxExChannel.InputText.ToString());
            //bool temp_result = jprint.WriteReg(0, 0, 0, value, true);
            if (start_flash_print_flage)
            {
                jprint.SetFlashSettings(0, 0, 10000, 1000, 1);
                jprint.StartFlash(0, 0);
                jprint.SetFlashSettings(0, 1, 10000, 1000, 1);
                jprint.StartFlash(0, 1);
                jprint.SetFlashSettings(0, 2, 10000, 1000, 1);
                jprint.StartFlash(0, 2);
                jprint.SetFlashSettings(0, 3, 10000, 1000, 1);
                jprint.StartFlash(0, 3);
                jprint.SetFlashSettings(1, 0, 10000, 1000, 1);
                jprint.StartFlash(1, 0);
                ucBtnImg4.BtnText = "停止闪喷";
                start_flash_print_flage = false;
            }
            else
            {
                jprint.StopFlash(0, 0);
                jprint.StopFlash(0, 1);
                jprint.StopFlash(0, 2);
                jprint.StopFlash(0, 3);
                jprint.StopFlash(1, 0);
                ucBtnImg4.BtnText = "开始闪喷";
                start_flash_print_flage = true;
            }
        }

        private void ucBtnImg5_BtnClick(object sender, EventArgs e)
        {
            int mainBoardId = 0;
            int npBoardId = 0;
            int main_board_number = int.Parse(ucTextBoxExMainBoard.InputText.ToString());
            npBoardId = int.Parse(ucTextBoxExHeadBoard.InputText.ToString());
            //bool temp_result = true;
            //jprint.Stop();
            //获取头板是否连接成功
            //参数
            //main_board_id 主板id
            //head_board_id 头板id
            //bool temp_result = jprint.NpBoardIsConnected(main_board_number, npBoardId);
            bool temp_result = ConnectNpBoard(npBoardId);
            if (temp_result)
            {
                label1.Text = "连接成功";
            }
            else
            {
                label1.Text = "连接失败";
            }
        }
        /// <summary>
        /// 连接负压板
        /// </summary>
        /// <param name="np_board_id">负压板id</param>
        /// <returns></returns>
        public bool ConnectNpBoard(int np_board_id)
        {
            bool connect_status = false;
            int mainBoardId = 0;
            int npBoardId = np_board_id;
            bool temp_result = jprint.NpBoardIsConnected(mainBoardId, np_board_id);
            if (temp_result)
            {
                connect_status = true;
            }
            else
            {
                connect_status = false;
            }
            return connect_status;
        }

        private void ucBtnImg6_BtnClick(object sender, EventArgs e)
        {
            bool main_tank_level; //主墨壶液位[false:不缺, true:缺墨]
            bool sub_tank_level; //副墨壶液位[false:不缺, true:缺墨]
            bool warning; //倒吸报警[0:没有发生, 1:发生倒吸]

            jprint.GetMainSubTankLevel_20(0, 0, 0, out main_tank_level, out sub_tank_level);
            label14.Text = main_tank_level ? "缺" : "不缺";
            label20.Text = sub_tank_level ? "缺" : "不缺";

            jprint.GetMainSubTankLevel_20(0, 0, 1, out main_tank_level, out sub_tank_level);
            label13.Text = main_tank_level ? "缺" : "不缺";
            label19.Text = sub_tank_level ? "缺" : "不缺";

            jprint.GetMainSubTankLevel_20(0, 0, 2, out main_tank_level, out sub_tank_level);
            label12.Text = main_tank_level ? "缺" : "不缺";
            label18.Text = sub_tank_level ? "缺" : "不缺";

            jprint.GetMainSubTankLevel_20(0, 0, 3, out main_tank_level, out sub_tank_level);
            label11.Text = main_tank_level ? "缺" : "不缺";
            label16.Text = sub_tank_level ? "缺" : "不缺";

            jprint.GetMainSubTankLevel_20(0, 0, 4, out main_tank_level, out sub_tank_level);
            label10.Text = main_tank_level ? "缺" : "不缺";
            label15.Text = sub_tank_level ? "缺" : "不缺";

            //GetMainSubTankLevel(int     main_board_id,
            //int     np_board_id,
            //int     ch_index,
            //int & main_tank_level,
            //int & sub_tank_level,
            //int & warning
            //)

            //main_board_id 主板id
            //np_board_id 负压板id
            //ch_index 通道索引(ch1 -> 0, ch2-> 1)
            //[out]
            //        main_tank_level 主墨壶液位[0:不缺, 1:缺墨]
            //[out]
            //        sub_tank_level 副墨壶液位[0:不缺, 1:缺墨]
            //[out]
            //        warning 倒吸报警[0:没有发生, 1:发生倒吸]

        }

        private void ucBtnImg7_BtnClick(object sender, EventArgs e)
        {
            double npvalue = 0;
            jprint.GetNpActualValue(0, 0, 0, out npvalue);
            label1.Text = "负压值为：" + npvalue;
            //            GetNpActualValue(int     main_board_id,
            //int     np_board_id,
            //int     ch_index,
            //int & bcd_np_value
            //)

            //            main_board_id 主板id
            //np_board_id 负压板id
            //ch_index 通道索引(ch1 -> 0, ch2-> 1)
            //[out] bcd_np_value 负压值，采用bcd形式
        }
        private void ucBtnImg8_BtnClick(object sender, EventArgs e)
        {
            //GetTankTemperature(int     main_board_id,
            //int     head_board_id,
            //int & bcd_temperature
            //)
        }
        private void ucBtnImg9_BtnClick(object sender, EventArgs e)
        {
            double value = double.Parse(ucTextBoxEx2.InputText.ToString());
            int value1 = (int)value;
            jprint.WriteWaveform(0, 0, value1);
        }
        private void ucBtnImg10_BtnClick(object sender, EventArgs e)
        {
            //double value = double.Parse(ucTextBoxEx2.InputText.ToString());
            //int value1 = (int)value;
            //jprint.SetHeadTemperature(0, 0, 40);
        }
        public static List<PrintContent> print_content_list_g = new List<PrintContent>();
        private void ucBtnImg11_BtnClick(object sender, EventArgs e)
        {
            print_content_list_g = CommonData.PrintContentList_g;
            if (print_content_list_g.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请行生成打印序列", "提示");
                return;
            }

            //if (!PrintInitian())
            //{
            //    FrmDialog.ShowDialog(this, "启动打印失败", "提示");
            //    return;
            //}

            //每行运行时间隔
            int line_offset = 1;

            for (int i = 0; i < print_content_list_g.Count; i++)
            {
                //每行的间隔
                print_content_list_g[i].X_offset = i * line_offset;
                print_content_list_g[i].Y_offset = 0;
            }

            //最大层数
            int layout_max = print_content_list_g[print_content_list_g.Count - 1].Loop_index;
            List<PrintContent> print_content_list_per_layout = new List<PrintContent>();
            //遍历所有的层
            cycle_number = layout_max;
            cycle_finish_status_list = new List<int>();

            for (int ii = 0; ii < layout_max; ii++)
            {
                cycle_finish_status_list.Add(1);
            }

            run_finish_flage = true;
            //for (int i = 0; i < layout_max; i++)
            for (int i = 0; i < 1;) //先用一层试试看
            {
                if (!run_flage)
                {
                    run_flage = true;
                    print_content_list_per_layout = FindPerLayoutPrintContent(print_content_list_g, i);
                    //PrintATCG(print_content_list_per_layout);
                    //Task.Factory.StartNew(() => { StartToRun(print_content_list_per_layout, i); });
                }

                if (!run_flage && cycle_finish_status_list[i] == 3)
                {
                    i++;
                    Thread.Sleep(100);
                    //continue;
                }



                //Task.Factory.StartNew(() => { CheckRunCycle(print_content_list_per_layout, i); });
                //while (true)
                //{
                //    //如果当前的完成了，开始下一个循环
                //    if (!run_flage && cycle_finish_status_list[i] == 3)
                //    {
                //        i++;
                //        Thread.Sleep(100);
                //        //continue;
                //    }
                //    Thread.Sleep(10000);
                //}
                //StartToRun(print_content_list_per_layout, i);
            }
        }

        public static int iloop = 0;
        public static int ilevel = 0;
        public static int ilevel_first = 0;
        public static int list_index = 0;
        public static string parent_text = "";
        private string GetParentsText(TreeNode tn, int level)
        {
            string parent_text1 = "";// tn.Parent.Text;
            if (level != 1)
            {
                string[] order_sz_array = tn.Parent.Text.Split(',');
                string loop_flag = order_sz_array[0].Substring(0, 1);
                string loop_number = order_sz_array[0].Substring(1, order_sz_array[0].Length - 1) + " ";

                parent_text += loop_number;//tn.Parent.Text;

                parent_text1 = tn.Parent.Text;
                GetParentsText(tn.Parent, tn.Parent.Level);
            }
            return parent_text1;
        }
        private void GetParentsName(TreeNode Node, ref List<String> NameList)
        {
            NameList.Add(Node.Text);
            if (Node.Parent != null)
            {
                //递归
                GetParentsName(Node.Parent, ref NameList);
            }
        }
        public class tb_SensorRecordModel
        {
            public int ID { get; set; }
            public decimal Value1 { get; set; }
        }

        public static List<tb_SensorRecordModel> list = new List<tb_SensorRecordModel>();
        public static int order_count_index_g = 0;

        public static string start_time_g = "";
        public static string current_time_g = "";
        public void RunOther(string active_name)
        {
            OpenFile_Preload_Clean(active_name, treeView1);
            treeView1.SelectedNode = treeView1.Nodes[0];
            if (treeView1.Nodes.Count == 0)
            {
                FrmDialog.ShowDialog(this, "请输入合成序列,或是打开运行脚本", "提示");
                return;
            }
            iloop = 0;
            list_index = 0;
            List<GetOrderClsSimple> node_name_list = new List<GetOrderClsSimple>();
            parent_text = "";
            if (treeView1.Nodes.Count > 1)
            {
                return;
            }
            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();
            List<GetOrderClsSimple> task_listxxx = new List<GetOrderClsSimple>();
            //TreeNode node_selected = treeView1.SelectedNode;
            for (int i = 0; i < treeView1.Nodes.Count; i++)
            {
                TreeNode node = treeView1.Nodes[i];
                DiGui1(node, ref node_name_list);
            }

            var groupList = node_name_list.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
            //分组循环
            for (int i = 0; i < groupList.Count; i++)
            {
                Int32 sam_count = groupList[i].samcount;
                string task_id = groupList[i].Keys.Task_id;
                List<GetOrderClsSimple> node_name_list_by_task_id = new List<GetOrderClsSimple>();
                node_name_list_by_task_id = node_name_list.FindAll(o => o.Task_id.Equals(task_id));
                //再找出线程ID相同的
                var groupList_by_thread_id = node_name_list_by_task_id.GroupBy(x => new { x.Thread_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();
                for (int jj = 0; jj < groupList_by_thread_id.Count; jj++)
                {
                    string thread_id = groupList_by_thread_id[jj].Keys.Thread_id;
                    List<GetOrderClsSimple> node_name_by_loop_number_list = new List<GetOrderClsSimple>();
                    node_name_by_loop_number_list = node_name_list_by_task_id.FindAll(o => o.Thread_id.Equals(thread_id));
                    if (node_name_by_loop_number_list.Count > 0)
                    {
                        int aaa = node_name_by_loop_number_list[0].ILoop;
                        int bbb = node_name_by_loop_number_list[0].ILoopNext;
                        for (int ILoop = 0; ILoop < aaa; ILoop++)
                        {
                            for (int ILoopNext = 0; ILoopNext < bbb; ILoopNext++)
                            {
                                for (int aii = 0; aii < node_name_by_loop_number_list.Count; aii++)
                                {
                                    //Console.Write(node_name_by_loop_number_list[aii].Order_content + "--" +node_name_by_loop_number_list[aii].Task_id + "\r\n");
                                    //GetOrderClsSimple clss = new GetOrderClsSimple();
                                    //clss = node_name_by_loop_number_list[aii];
                                    //clss.Id = task_list.Count + 1;
                                    //task_list.Add(clss);

                                    string[] tmp_array = node_name_by_loop_number_list[aii].Order_content.Split(',');
                                    //如果是阀和泵一起的指令那么拆开来，否则还按原来的方式
                                    if (tmp_array.Length == 4 && tmp_array[3].Length > 1)
                                    {
                                        //one order to two order,one valve,two pump
                                        string aii_content = node_name_by_loop_number_list[aii].Order_content;
                                        //valve
                                        GetOrderClsSimple clss = new GetOrderClsSimple();
                                        //node_name_by_loop_number_list[aii].Order_content = tmp_array[0] + "," + tmp_array[1] + "" + "V,A";
                                        //clss = node_name_by_loop_number_list[aii];
                                        clss.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss.Order_content = node_name_by_loop_number_list[aii].Order_content;//tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss);

                                    }
                                    else
                                    {
                                        string aii_content1 = node_name_by_loop_number_list[aii].Order_content;
                                        GetOrderClsSimple clss4 = new GetOrderClsSimple();
                                        clss4.Thread_id = node_name_by_loop_number_list[aii].Thread_id;
                                        clss4.Task_id = node_name_by_loop_number_list[aii].Task_id;
                                        clss4.Order_content = aii_content1;// tmp_array[0] + "," + tmp_array[1] + "," + "V-1,A0";
                                        clss4.Id = task_list_g.Count + 1;
                                        task_listxxx.Add(clss4);
                                    }
                                }
                            }
                        }
                    }
                }
                //Console.Write("---------------------------------------------------------------------------" + "\r\n");
            }

            //MultTask();
            for (int jja = 0; jja < task_listxxx.Count; jja++)
            {
                GetOrderClsSimple clss = new GetOrderClsSimple();
                clss.Id = jja;
                clss.ILoop = task_listxxx[jja].ILoop;
                clss.ILoopNext = task_listxxx[jja].ILoopNext;
                clss.Order_content = task_listxxx[jja].Order_content;
                clss.Status = task_listxxx[jja].Status;
                clss.Task_id = task_listxxx[jja].Task_id;
                clss.Thread_id = task_listxxx[jja].Thread_id;
                task_list_new_g.Add(clss);
            }

            task_list_g = new List<GetOrderClsSimple>();
            task_list_g = task_list_new_g;
            var groupList1 = task_list_g.GroupBy(x => new { x.Task_id }).Select(group => new { Keys = group.Key, samcount = group.Count() }).ToList();

            //重新生成编号
            for (int i = 0; i < groupList1.Count; i++)
            {
                string task_id = groupList1[i].Keys.Task_id;
                int number = groupList1[i].samcount;
                task_id_list.Add(task_id);
                task_id_number_list.Add(number);
            }

            //OpenFile_Preload_Clean("Clean", treeView_clean);
            //ActionConversionOrder(treeView_clean);
            start_time_g = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
            //label19.Text = "运行时间：" + "00:00:00";
            ucBtnImg13.Enabled = true;
            ucBtnImg15.Enabled = true;
            ucBtnImg13.FillColor = Color.Black;
            ucBtnImg15.FillColor = Color.Black;
            //ucBtnImg23.Enabled = false;
            //ucBtnImg24.Enabled = false;
            //ucBtnImg23.FillColor = Color.Gray;
            //ucBtnImg24.FillColor = Color.Gray;
            query_sub0_pause_flage = false;
            sub01_pause_flage = true;
            sub11_pause_flage = true;
            sub21_pause_flage = true;
            //buffer = new List<byte>(4096);
            sub0_pause_flage = false;
            sub1_pause_flage = true;
            sub2_pause_flage = true;
            task_scheduling_pause_flage = false;
            is_receive_data_g = true;
            show_run_finish_flage_g = true;
            number_current_thread0_status1 = 0;
            //ucBtnImg14.Enabled = false;
            //ucBtnImg14.FillColor = Color.Gray;
            //ucBtnImg7.Enabled = false;
            //ucBtnImg7.FillColor = Color.Gray;
            not_equal_error_number = 0;
            //UpdateListUi();
            //var model = task_list.Where(c => c.Task_id.Equals("1") && c.Status.Equals("0"));//.FirstOrDefault();
            //var model = task_list.Where(c => c.Status == 2 && c.Task_id.Equals("0") && c.Thread_id.Equals(0)).FirstOrDefault();
            //model.Status = 3;
            //启动时，线程号2，暂时停状态，只有线程1启动了，
            //thread_order_flage_continue_send = true;
        }

        public static bool can_add_iloop_1 = false;
        private void DiGui1(TreeNode tn, ref List<GetOrderClsSimple> simlpe_order_list)
        {
            int level = tn.Level;
            Console.Write(level);
            //1.将当前节点显示到lable上
            //label1.Text += "aaa" + "    " + tn.Text + "\r\n";
            string tn_text = tn.Text;
            if (level == 1 && tn.Text.IndexOf("串行") != -1)
            {
                //Console.Write("一次循环开始");
                if (can_add_iloop_1)
                {
                    iloop++;
                    can_add_iloop_1 = false;
                }
            }

            if (level == 1 && tn.Text.IndexOf("并行") != -1)
            {
                //Console.Write("可以加1了");
                can_add_iloop_1 = true;
            }

            if (tn_text.IndexOf("D") != -1 || tn_text.IndexOf("Switch") != -1 || tn_text.IndexOf("iDelay") != -1)
            {
                //Console.Write(tn.Level + tn.FullPath + tn.Parent.Tag + "\r\n");
                //Console.Write("tn.Parent.Parent:" + tn.Parent.Parent.Text + "--tn.Parent.Text:" + tn.Parent.Text + "--tn.Text:" + tn.Text + "" + "\r\n");
                //tn.Parent.Parent:aaaa--tn.Parent.Text:G1,串行--tn.Text:D1,O1,V1200,A1000
                //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,1--tn.Text:Switch:1
                //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,1--tn.Text:Switch:2
                //tn.Parent.Parent:G2,并行--tn.Parent.Text:G1,串行,2--tn.Text:Switch:2
                GetOrderClsSimple simple_order = new GetOrderClsSimple();
                simple_order.Task_id = iloop + "";
                simple_order.Thread_id = GetThread_id(tn.Parent.Text);
                simple_order.ILoop = GetILoop_Number(tn.Parent.Parent.Text);
                simple_order.ILoopNext = GetILoopNext_Number(tn.Parent.Text);
                simple_order.Order_content = tn.Text;
                //Console.Write(simple_order.Task_id + "-" + simple_order.Thread_id + "" + "-" + simple_order.ILoop + "-" + simple_order.ILoopNext + "-" + simple_order.Order_content + "\r\n");
                simlpe_order_list.Add(simple_order);
            }
            for (int i = 0; i < tn.Nodes.Count; i++)
            {
                TreeNode node = tn.Nodes[i];
                DiGui1(node, ref simlpe_order_list);
            }
        }

        /// <summary>
        /// 得到线程编号
        /// </summary>
        /// <param name="sz"></param>
        /// <returns></returns>
        public string GetThread_id(string sz)
        {
            string value = "0";
            string[] sz_array = sz.Split(',');
            if (sz_array.Length == 3)
            {
                value = sz_array[2];
            }
            return value;
        }

        /// <summary>
        /// 第一层循环
        /// </summary>
        public int GetILoop_Number(string sz)
        {
            int value = 1;
            string[] sz_array = sz.Split(',');
            if (sz_array.Length > 1)
            {
                value = int.Parse(sz_array[0].Substring(1, sz_array[0].Length - 1));
            }
            return value;
        }

        /// <summary>
        /// 第二层循环
        /// </summary>
        public int GetILoopNext_Number(string sz)
        {
            int value = 1;
            string[] sz_array = sz.Split(',');
            if (sz_array.Length > 1)
            {
                value = int.Parse(sz_array[0].Substring(1, sz_array[0].Length - 1));
            }
            return value;
        }

        XmlDocument doc = new XmlDocument();
        StringBuilder sb = new StringBuilder();

        //XML每行的内容
        private string xmlLine = "";
        public void OpenFile_Preload_Clean(string file_name, TreeView tv)
        {
            tv.Nodes.Clear();
            doc.Load(file_name + ".scr"); //我是把xml放到debug里面了.你的路径就随便啦.不过这样方便一些.
            RecursionTreeControl(doc.DocumentElement, tv.Nodes);//将加载完成的XML文件显示在TreeView控件中
            tv.ExpandAll();//展开TreeView控件中的所有项
            //label13.Text = file_name;
        }

        public static int a_g = 0;
        /// <summary>
        /// RecursionTreeControl:表示将XML文件的内容显示在TreeView控件中
        /// </summary>
        /// <param name="xmlNode">将要加载的XML文件中的节点元素</param>
        /// <param name="nodes">将要加载的XML文件中的节点集合</param>
        private void RecursionTreeControl(XmlNode xmlNode, TreeNodeCollection nodes)
        {
            Console.Write("a_g" + a_g + "\r\n");
            a_g++;
            foreach (XmlNode node in xmlNode.ChildNodes)//循环遍历当前元素的子元素集合
            {
                string value = node.Attributes["Text"].Value;
                TreeNode new_child = new TreeNode();//定义一个TreeNode节点对象
                new_child.Name = node.Attributes["Name"].Value;
                new_child.Text = value;
                new_child.Tag = a_g;
                nodes.Add(new_child);//向当前TreeNodeCollection集合中添加当前节点
                RecursionTreeControl(node, new_child.Nodes);//调用本方法进行递归
            }
        }

        /// <summary>
        /// 生成的打印序列转换成每一层的打印矩阵
        /// </summary>
        /// <param name="print_content_list">所有序列</param>
        /// <param name="layout_index">哪一层</param>
        public List<PrintContent> FindPerLayoutPrintContent(List<PrintContent> print_content_list_para1, int layout_index)
        {
            List<PrintContent> print_content_list_per_layout = new List<PrintContent>();
            print_content_list_per_layout = print_content_list_para1.FindAll(o => o.Loop_index == layout_index);
            return print_content_list_per_layout;
        }

        /// <summary>
        /// 把要打印的一层序列传进来。
        /// </summary>
        /// <param name="print_content_list"></param>
        public bool PrintATCG(List<PrintContent> print_content_list_para2)
        {
            bool print_result = false;
            //Task.Factory.StartNew(() => { SendPageDataWork_1(print_content_list_para2); });
            return print_result;
            for (int i = 0; i < print_content_list_para2.Count; i++)
            {
                if (!print_content_list_para2[i].A_finish)
                {
                    Console.WriteLine(i);
                    //return;

                    for (int jj = 0; jj < print_content_list_para2[i].Print_array_a.Length; jj++)
                    {
                        print_content_list_para2[i].Print_array_a[jj] = 0x80;
                    }
                    //print_result = SendPageDataWorkA(print_content_list_para2[i].Print_array_a);
                    //print_result = SendPageDataWorkNew(print_content_list_para2[i].Print_array_a);
                    if (print_result)
                    {
                        //todo
                        //改变发送的标志位
                        print_content_list_para2[i].A_finish = true;
                    }
                }
            }
            print_result = true;
            return print_result;
        }

        public static SerialPort serialPort_Send;//= new SerialPort(;
        public static SerialPort serialPortvalve_Send;
        SerialPort serialPort_ASD_X;
        SerialPort serialPort_ASD_Y;
        SerialPort serialPort_ASD_R;

        /// <summary>
        /// 添加测试打印内容标识，点添加打印按扭后，把标识改为true,发送内容成功后改为false
        /// </summary>
        public static bool m_test_print_add_content_flage = true;

        /// <summary>
        /// 总流程加入暂停和停止  true时停，false继续
        /// </summary>
        public static bool m_total_run_pause_flage = true;
        public static bool m_total_run_stop_flage = false;

        //Int32 Curse_Count = 0;
        //Galil.Galil g_Controller;
        private void FrmPrintATCG_Load(object sender, EventArgs e)
        {
            //读配置信息
            List<string> system_configuration_list = ReadFileToList("SystemConfiguration.ini");
            //读打印配置
            //List<string> print_configuration_list = ReadFileToList("PrintConfiguration.ini");
            if (system_configuration_list.Count > 0)
            {
                for (int i = 0; i < system_configuration_list.Count; i++)
                {
                    string system_sz = system_configuration_list[i];
                    string[] system_sz_array = system_sz.Split(':');
                    if (system_sz_array[0].Equals("SystemType"))
                    {
                        system_type = system_sz_array[1];
                        if (system_type.Equals("2"))
                        {
                            label33.Text = "mm" + system_type;
                            textBox4.Text = "50";
                            textBox6.Text = "30";
                            ucBtnImg16.BtnText = "清除故障";
                        }
                        if (system_type.Equals("1"))
                        {
                            label33.Text = "pulse" + system_type;
                            textBox4.Text = "76666";
                            textBox6.Text = "1000";
                            ucBtnImg16.BtnText = "急停";
                        }
                    }
                }
            }
            LoadMoveStartPositionModel();
            getPortDeviceName();
            serialPort_Send = new SerialPort();
            serialPortvalve_Send = new SerialPort();
            serialPort_ASD_X = new SerialPort();
            serialPort_ASD_Y = new SerialPort();
            serialPort_ASD_R = new SerialPort();
            List<string> com_list = ReadFileToList("ComConfiguration.ini");
            if (com_list.Count > 0)
            {
                for (int i = 0; i < com_list.Count; i++)
                {
                    string com_sz = com_list[i];
                    string[] com_sz_array = com_sz.Split(':');
                    if (com_sz_array.Length > 1)
                    {
                        if (com_sz_array[0].Equals("Pump"))
                        {
                            //pump connect
                            Console.WriteLine(String.Format("Pump-{0}", com_sz_array[1]));
                            OpenCommuniction(serialPort_Send, com_sz_array[1], 9600);
                        }
                        if (com_sz_array[0].Equals("ZAxisAndSwitch"))
                        {
                            //pump connect
                            Console.WriteLine(String.Format("ZAxisAndSwitch-{0}", com_sz_array[1]));
                            OpenCommuniction(serialPortvalve_Send, com_sz_array[1], 9600);
                        }

                        if (system_type.Equals("2"))
                        {
                            if (com_sz_array[0].Equals("ASD_XCOM"))
                            {
                                OpenCommuniction(serialPort_ASD_X, com_sz_array[1], 115200);
                            }
                            else
                            {
                                move_connect_status_g = 0;
                            }
                            if (com_sz_array[0].Equals("ASD_YCOM"))
                            {
                                OpenCommuniction(serialPort_ASD_Y, com_sz_array[1], 115200);
                            }
                            else
                            {
                                move_connect_status_g = 0;
                            }
                            if (com_sz_array[0].Equals("ASD_RCOM"))
                            {
                                OpenCommuniction(serialPort_ASD_R, com_sz_array[1], 115200);
                            }
                            else
                            {
                                move_connect_status_g = 0;
                            }
                        }
                    }
                }
            }
            Thread.Sleep(1000);
            if (serialPortvalve_Send.IsOpen)
            {
                ZAxisFindeZero();
            }
            if (serialPort_Send.IsOpen)
            {
                PumpReset();
            }
            if (serialPortvalve_Send.IsOpen && serialPort_Send.IsOpen)
            {
                //开启线程
                try
                {
                    StartThreadPool();
                }
                catch (Exception eee)
                {
                    var st = new StackTrace(eee, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("Exception.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "StartThreadPool:" + "\r\n"
                        + eee.Message + "\r\n"
                        + eee.InnerException + "\r\n"
                        + eee.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                }
            }

            if (system_type.Equals("1"))
            {
                if (ucBtnImg17.BtnText.Equals("连接"))
                {
                    StartMoveController();
                }
                else
                {
                    EndMoveController();
                }
            }

            if (system_type.Equals("1"))
            {
                System1Enable();
            }

            if (system_type.Equals("2"))
            {
                System2Enable();
            }

            //打印初始化
            try
            {
                if (system_type.Equals("2"))
                {
                    InitializePrint(2);
                }
                if (system_type.Equals("1"))
                {
                    InitializePrintSystem1(2);
                }
            }
            catch (Exception ea)
            {
                FrmDialog.ShowDialog(this, "打印初始化异常" + ea.ToString(), "提示");
            }

            //20220504  暂时关闭  2022-05-14根据配置文件开关功能
            if (system_type.Equals("1"))
            {
                //StarCam(); //2022-06-18不再使用
            }

            try
            {
                //刷新界面
                //__UpdateUI();
                m_objIGXFactory = IGXFactory.GetInstance();
                m_objIGXFactory.Init();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

            //
            StartGalaxy();
            CollectionCamPic();
            read_data_flage = true;
            read_data_flageX = true;
            read_data_flageY = true;
            read_data_flageR = true;
        }
        public List<string> ReadFileToList(String _fileName)
        {
            List<string> sz_list = new List<string>();
            Stopwatch sw = new Stopwatch();
            var path = _fileName;
            //按行读取
            sw.Restart();
            using (var sr = new StreamReader(path))
            {
                var ls = "";
                while ((ls = sr.ReadLine()) != null)
                {
                    sz_list.Add(ls);
                }
            }
            sw.Stop();
            return sz_list;
        }

        public void StartThreadPool()
        {
            //分发任务 
            //Thread ThreadTaskScheduling = new Thread(TaskScheduling);
            //ThreadTaskScheduling.Start();
            //Thread ThreadSendOrderSub0 = new Thread(SendOrderSub0);
            //ThreadSendOrderSub0.Start();
            //Thread ThreadSendOrderSub1 = new Thread(SendOrderSub1);
            //ThreadSendOrderSub1.Start();
            //Thread ThreadSendOrderSub2 = new Thread(SendOrderSub2);
            //ThreadSendOrderSub2.Start();
            //Thread ThreadSendOrderQuerySub0 = new Thread(SendOrderQuerySub0);
            //ThreadSendOrderQuerySub0.Start();
            //Thread ThreadRefreshUiInfo = new Thread(RefreshUiInfo);
            //ThreadRefreshUiInfo.Start();
            //Thread ThreadSendMovePlateCommand = new Thread(SendMovePlateCommand);
            //ThreadSendMovePlateCommand.Start();
            //Thread ThreadQueryRx = new Thread(QueryRx);
            //ThreadQueryRx.Start();
            //Thread ThreadSendQueryZAxisPositionloop = new Thread(SendQueryZAxisPositionloop);
            //ThreadSendQueryZAxisPositionloop.Start();
            //Thread ThreadReadZAxisRx = new Thread(ReadZAxisRx);
            //ThreadReadZAxisRx.Start();
            //Thread ThreadReadPumpRx = new Thread(ReadPumpRx);
            //ThreadReadPumpRx.Start();
            //m_test_print_add_content_flage = true;
            //Thread ThreadSendPageDataWork_Test_Y_Position = new Thread(SendPageDataWork_Test_Y_Position);
            //ThreadSendPageDataWork_Test_Y_Position.Start();
            //Thread ThreadStartToRun_New = new Thread(StartToRun_New);
            //ThreadStartToRun_New.Start();
            bool pool = ThreadPool.SetMaxThreads(15, 15);
            if (pool)
            {
                //分发任务
                ThreadPool.QueueUserWorkItem(o =>
                {
                    TaskScheduling();
                });
                //0
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderSub0();
                });
                ////1
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderSub1();
                });
                ////2
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderSub2();
                });
                //增加三个查询状态的线程，如果不是开关阀，发送完指令后要进行查询
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendOrderQuerySub0();
                });
                ThreadPool.QueueUserWorkItem(o =>
                {
                    RefreshUiInfo();
                });
                //分送位移平台指令
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendMovePlateCommand();
                });
                ThreadPool.QueueUserWorkItem(o =>
                {
                    QueryRx();
                });
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendQueryZAxisPositionloop();
                });
                ThreadPool.QueueUserWorkItem(o =>
                {
                    ReadZAxisRx();
                });
                ThreadPool.QueueUserWorkItem(o =>
                {
                    ReadPumpRx();
                });
                m_test_print_add_content_flage = true;
                //测试打印线程
                ThreadPool.QueueUserWorkItem(o =>
                {
                    SendPageDataWork_Test_Y_Position();
                });
                //总流程
                ThreadPool.QueueUserWorkItem(o =>
                {
                    StartToRun_New();
                });
            };

            if (system_type.Equals("2"))
            {
                Thread thread_rex = new Thread(DataReceivedX);
                thread_rex.Start();
                Thread thread_rey = new Thread(DataReceivedY);
                thread_rey.Start();
                Thread thread_rez = new Thread(DataReceivedR);
                thread_rez.Start();
                //Thread thread_query_xyz_position = new Thread(SendQueryXYRPosition);
                //thread_query_xyz_position.Start();
                Thread thread_query_x_position = new Thread(SendQueryXPosition);
                thread_query_x_position.Start();
                Thread thread_query_y_position = new Thread(SendQueryYPosition);
                thread_query_y_position.Start();
                Thread thread_query_z_position = new Thread(SendQueryRPosition);
                thread_query_z_position.Start();
            }
        }

        public bool x_query_flage = false; //false表示不发查询，true表示该发查询
        public bool y_query_flage = false;
        public bool r_query_flage = false;
        public static DateTime m_dtS;//= DateTime.Now.AddSeconds(iOuttimeto);
        public bool read_data_flage = false;
        public bool read_data_flageX = false;
        public bool read_data_flageY = false;
        public bool read_data_flageR = false;
        public void SendQueryXYRPosition()
        {
            while (true)
            {
                if (sendQueryXYR_Models_list.Count > 0)
                {
                    List<SendQueryXYR_Model> sendQueryX_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("X"));
                    if (sendQueryX_Models_list.Count > 0)
                    {
                        while (x_query_flage)
                        {
                            if (sendQueryX_Models_list[0].Send_status == 1)
                            {
                                byte[] buf = Displacement.PFB();
                                serialPort_ASD_X.Write(buf, 0, buf.Length);
                                Thread.Sleep(300);
                            }
                            Thread.Sleep(500);
                        }
                    }
                    List<SendQueryXYR_Model> sendQueryY_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("Y"));
                    if (sendQueryY_Models_list.Count > 0)
                    {
                        while (y_query_flage)
                        {
                            if (sendQueryY_Models_list[0].Send_status == 1)
                            {
                                byte[] buf = Displacement.PFB();
                                serialPort_ASD_Y.Write(buf, 0, buf.Length);
                                Thread.Sleep(300);
                            }
                            Thread.Sleep(500);
                        }
                    }
                    List<SendQueryXYR_Model> sendQueryR_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("R"));
                    if (sendQueryR_Models_list.Count > 0)
                    {
                        while (r_query_flage)
                        {
                            if (sendQueryR_Models_list[0].Send_status == 1)
                            {
                                byte[] buf = Displacement.PFB();
                                serialPort_ASD_R.Write(buf, 0, buf.Length);
                                Thread.Sleep(300);
                            }
                            Thread.Sleep(500);
                        }
                    }
                }
                Thread.Sleep(100);
            }
        }
        public void SendQueryXPosition()
        {
            while (true)
            {
                try
                {
                    if (sendQueryXYR_Models_list.Count > 0)
                    {
                        List<SendQueryXYR_Model> sendQueryX_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("X"));
                        if (sendQueryX_Models_list.Count > 0)
                        {
                            while (x_query_flage)
                            {
                                if (sendQueryX_Models_list[0].Send_status == 1)
                                {
                                    byte[] buf = Displacement.PFB();
                                    serialPort_ASD_X.Write(buf, 0, buf.Length);
                                    Thread.Sleep(50);
                                }
                                Thread.Sleep(300);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("SendQueryXPosition异常.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "异常:" + "\r\n"
                        + ex.Message + "\r\n"
                        + "read_sx:" + read_sx + "\r\n"
                        + ex.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                    //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                }
                Thread.Sleep(100);
            }
        }
        public void SendQueryYPosition()
        {
            while (true)
            {
                try
                {
                    if (sendQueryXYR_Models_list.Count > 0)
                    {
                        List<SendQueryXYR_Model> sendQueryY_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("Y"));
                        if (sendQueryY_Models_list.Count > 0)
                        {
                            while (y_query_flage)
                            {
                                if (sendQueryY_Models_list[0].Send_status == 1)
                                {
                                    byte[] buf = Displacement.PFB();
                                    serialPort_ASD_Y.Write(buf, 0, buf.Length);
                                    Thread.Sleep(300);
                                }
                                Thread.Sleep(500);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("SendQueryYPosition异常.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "异常:" + "\r\n"
                        + ex.Message + "\r\n"
                        + "read_sx:" + read_sx + "\r\n"
                        + ex.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                    //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                }
                Thread.Sleep(100);
            }
        }
        public void SendQueryRPosition()
        {
            while (true)
            {
                try
                {
                    if (sendQueryXYR_Models_list.Count > 0)
                    {
                        List<SendQueryXYR_Model> sendQueryR_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("R"));
                        if (sendQueryR_Models_list.Count > 0)
                        {
                            while (r_query_flage)
                            {
                                if (sendQueryR_Models_list[0].Send_status == 1)
                                {
                                    byte[] buf = Displacement.PFB();
                                    serialPort_ASD_R.Write(buf, 0, buf.Length);
                                    Thread.Sleep(300);
                                }
                                Thread.Sleep(500);
                            }
                        }
                    }
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("SendQueryRPosition异常.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "异常:" + "\r\n"
                        + ex.Message + "\r\n"
                        + "read_sx:" + read_sx + "\r\n"
                        + ex.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                    //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                }
                Thread.Sleep(100);
            }
        }

        public void DataReceivedX()
        {
            while (true)
            {
                while (read_data_flage)
                {
                    if (serialPort_ASD_X.IsOpen)
                    {
                        try
                        {
                            double iOuttimeto = 10000;
                            DateTime dtS = DateTime.Now.AddSeconds(iOuttimeto);
                            for (; true;)
                            {
                                int DataLength = serialPort_ASD_X.BytesToRead;
                                //如果超时退出
                                //1秒要是没返回，就退出，否则一直去读
                                //if (dtS < DateTime.Now)
                                //{
                                //    break;
                                //}
                                StringBuilder sb = new StringBuilder();
                                if (DataLength > 1)
                                {
                                    dtS = DateTime.Now.AddSeconds(iOuttimeto);
                                    byte[] ds = new byte[DataLength];
                                    int len = serialPort_ASD_X.Read(ds, 0, DataLength);
                                    sb.Append(Encoding.ASCII.GetString(ds, 0, len));
                                    DataLength = 0;
                                }
                                read_sx += sb.ToString();
                                int a_lenth = read_sx.Length;
                                if (read_sx.Length > 0)
                                {
                                    //如果收到这个后个，说明发送运动指令成功
                                    int MOVEABS_position = read_sx.IndexOf("MOVEABS");
                                    if (MOVEABS_position != -1)
                                    {
                                        //开始发送查询位置指令
                                        x_query_flage = true;
                                        read_sx = "";
                                    }
                                }

                                Invoke(new EventHandler(delegate
                                {
                                    List<SendQueryXYR_Model> sendQueryX_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("X"));
                                    if (sendQueryX_Models_list.Count > 0)
                                    {
                                        if (read_sx.Length > 0 && read_sx.Length < 31)
                                        {
                                            int pfb_position = read_sx.IndexOf("PFB");
                                            if (pfb_position == 0)
                                            {
                                                string[] read_s_array = read_sx.Split('\n');
                                                if (read_s_array.Length > 2)
                                                {
                                                    string[] read_s_array_array = read_s_array[1].Split('[');
                                                    if (read_s_array.Length == 4)
                                                    {
                                                        read_s_array_array = read_s_array[2].Split('[');
                                                    }
                                                    if (read_s_array_array.Length > 0)
                                                    {
                                                        //label7.Text = read_s_array_array[0];
                                                        //float fi = Convert.ToSingle(read_s_array_array[0]);
                                                        float fi = 0;
                                                        float.TryParse(read_s_array_array[0], out fi);

                                                        for (int i = 0; i < sendQueryX_Models_list.Count; i++)
                                                        {
                                                            int id = sendQueryX_Models_list[i].Id;
                                                            float value = sendQueryX_Models_list[i].offset;
                                                            labelXOffset.Text = "Xs:" + value + "r:" + fi;
                                                            if (fi == value)
                                                            {
                                                                valuex_g = "" + (float)value;
                                                                sendQueryXYR_Models_list.Remove(sendQueryXYR_Models_list.Find(o => o.Id == id));
                                                                x_query_flage = false;
                                                                //sendQueryX_Models_list[0].Send_status = 3;
                                                                //label7.Text = read_s_array_array[0] + "查询到位";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            read_sx = "";
                                        }
                                    }
                                }));
                                read_sx = "";
                                Thread.Sleep(1);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                            var st = new StackTrace(ex, true);
                            var frame = st.GetFrame(0);
                            var line = frame.GetFileLineNumber();
                            var sw = new System.IO.StreamWriter("ReadX异常.txt", true);
                            sw.WriteLine(
                                DateTime.Now.ToString() + "\r\n"
                                 + "异常:" + "\r\n"
                                + ex.Message + "\r\n"
                                + "read_sx:" + read_sx + "\r\n"
                                + ex.Source + "\r\n"
                                + frame + "\r\n"
                                + line);
                            sw.Close();
                            //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                        }
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(10);
            }
        }

        public void DataReceivedY()
        {
            while (true)
            {
                while (read_data_flage)
                {
                    if (serialPort_ASD_Y.IsOpen)
                    {
                        try
                        {
                            double iOuttimeto = 10000;
                            DateTime dtS = DateTime.Now.AddSeconds(iOuttimeto);
                            for (; true;)
                            {
                                int DataLength = serialPort_ASD_Y.BytesToRead;
                                //如果超时退出
                                //6秒要是没返回，就退出，否则一直去读
                                //if (dtS < DateTime.Now)
                                //{
                                //    break;
                                //}
                                StringBuilder sb = new StringBuilder();
                                if (DataLength > 1)
                                {
                                    dtS = DateTime.Now.AddSeconds(iOuttimeto);
                                    byte[] ds = new byte[DataLength];
                                    int len = serialPort_ASD_Y.Read(ds, 0, DataLength);
                                    sb.Append(Encoding.ASCII.GetString(ds, 0, len));
                                    DataLength = 0;
                                }
                                read_sy += sb.ToString();
                                int a_lenth = read_sy.Length;

                                if (read_sy.Length > 0)
                                {
                                    //如果收到这个后个，说明发送运动指令成功
                                    int MOVEABS_position = read_sy.IndexOf("MOVEABS");
                                    if (MOVEABS_position != -1)
                                    {
                                        //开始发送查询位置指令
                                        y_query_flage = true;
                                        read_sy = "";
                                    }
                                }
                                Invoke(new EventHandler(delegate
                                {
                                    List<SendQueryXYR_Model> sendQueryY_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("Y"));
                                    if (sendQueryY_Models_list.Count > 0)
                                    {
                                        if (read_sy.Length > 0 && read_sy.Length < 31)
                                        {
                                            int pfb_position = read_sy.IndexOf("PFB");
                                            if (pfb_position == 0)
                                            {
                                                string[] read_s_array = read_sy.Split('\n');
                                                if (read_s_array.Length > 2)
                                                {
                                                    string[] read_s_array_array = read_s_array[1].Split('[');
                                                    if (read_s_array.Length == 4)
                                                    {
                                                        read_s_array_array = read_s_array[2].Split('[');
                                                    }
                                                    if (read_s_array_array.Length > 0)
                                                    {
                                                        try
                                                        {
                                                            //float fi = Convert.ToSingle(read_s_array_array[0]);
                                                            float fi = 0;
                                                            float.TryParse(read_s_array_array[0], out fi);
                                                            for (int i = 0; i < sendQueryY_Models_list.Count; i++)
                                                            {
                                                                int id = sendQueryY_Models_list[i].Id;
                                                                float value = sendQueryY_Models_list[i].offset;
                                                                labelYOffset.Text = "Ys:" + value + "r:" + fi;
                                                                if (fi == value)
                                                                {
                                                                    valuey_g = "" + (float)value;
                                                                    sendQueryXYR_Models_list.Remove(sendQueryXYR_Models_list.Find(o => o.Id == id));
                                                                    //sendQueryY_Models_list[0].Send_status = 3;
                                                                    y_query_flage = false;
                                                                    //label10.Text = read_s_array_array[0] + "查询到位";
                                                                }
                                                            }
                                                        }
                                                        catch
                                                        {

                                                        }

                                                    }
                                                }
                                            }
                                            read_sy = "";
                                        }
                                    }
                                }));
                                read_sy = "";
                                Thread.Sleep(1);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                            var st = new StackTrace(ex, true);
                            var frame = st.GetFrame(0);
                            var line = frame.GetFileLineNumber();
                            var sw = new System.IO.StreamWriter("ReadY异常.txt", true);
                            sw.WriteLine(
                                DateTime.Now.ToString() + "\r\n"
                                 + "异常:" + "\r\n"
                                + ex.Message + "\r\n"
                                   + "read_sx:" + read_sx + "\r\n"
                                + ex.Source + "\r\n"
                                + frame + "\r\n"
                                + line);
                            sw.Close();
                            //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                        }
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(10);
            }
        }

        public void DataReceivedR()
        {
            while (true)
            {
                while (read_data_flage)
                {
                    if (serialPort_ASD_R.IsOpen)
                    {
                        try
                        {
                            double iOuttimeto = 10000;
                            DateTime dtS = DateTime.Now.AddSeconds(iOuttimeto);
                            for (; true;)
                            {
                                int DataLength = serialPort_ASD_R.BytesToRead;
                                //如果超时退出
                                //6秒要是没返回，就退出，否则一直去读
                                //if (dtS < DateTime.Now)
                                //{
                                //    break;
                                //}
                                StringBuilder sb = new StringBuilder();
                                if (DataLength > 1)
                                {
                                    dtS = DateTime.Now.AddSeconds(iOuttimeto);
                                    byte[] ds = new byte[DataLength];
                                    int len = serialPort_ASD_R.Read(ds, 0, DataLength);
                                    sb.Append(Encoding.ASCII.GetString(ds, 0, len));
                                    DataLength = 0;
                                }
                                read_sr += sb.ToString();
                                int a_lenth = read_sr.Length;

                                if (read_sr.Length > 0)
                                {
                                    //如果收到这个后个，说明发送运动指令成功
                                    int MOVEABS_position = read_sr.IndexOf("MOVEABS");
                                    if (MOVEABS_position != -1)
                                    {
                                        //开始发送查询位置指令
                                        r_query_flage = true;
                                        read_sr = "";
                                    }
                                }
                                Invoke(new EventHandler(delegate
                                {
                                    List<SendQueryXYR_Model> sendQueryR_Models_list = sendQueryXYR_Models_list.FindAll(o => o.Xyr_type.Equals("R"));
                                    if (sendQueryR_Models_list.Count > 0)
                                    {
                                        if (read_sr.Length > 0 && read_sr.Length < 31)
                                        {
                                            int pfb_position = read_sr.IndexOf("PFB");
                                            if (pfb_position == 0)
                                            {
                                                string[] read_s_array = read_sr.Split('\n');
                                                if (read_s_array.Length > 2)
                                                {
                                                    string[] read_s_array_array = read_s_array[1].Split('[');
                                                    if (read_s_array.Length == 4)
                                                    {
                                                        read_s_array_array = read_s_array[2].Split('[');
                                                    }
                                                    if (read_s_array_array.Length > 0)
                                                    {
                                                        //float fi = Convert.ToSingle(read_s_array_array[0]);
                                                        float fi = 0;
                                                        float.TryParse(read_s_array_array[0], out fi);
                                                        for (int i = 0; i < sendQueryR_Models_list.Count; i++)
                                                        {
                                                            int id = sendQueryR_Models_list[i].Id;
                                                            float value = sendQueryR_Models_list[i].offset;
                                                            labelZOffset.Text = "Rs:" + value + "r:" + fi;
                                                            if (fi == value)
                                                            {
                                                                valuer_g = "" + (float)value;
                                                                sendQueryXYR_Models_list.Remove(sendQueryXYR_Models_list.Find(o => o.Id == id));
                                                                r_query_flage = false;
                                                                //sendQueryR_Models_list[0].Send_status = 3;
                                                                //label11.Text = read_s_array_array[0] + "查询到位";
                                                            }
                                                        }
                                                    }
                                                }
                                            }
                                            read_sr = "";
                                        }
                                    }
                                }));

                                Thread.Sleep(1);
                            }
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                            var st = new StackTrace(ex, true);
                            var frame = st.GetFrame(0);
                            var line = frame.GetFileLineNumber();
                            var sw = new System.IO.StreamWriter("ReadR异常.txt", true);
                            sw.WriteLine(
                                DateTime.Now.ToString() + "\r\n"
                                 + "异常:" + "\r\n"
                                + ex.Message + "\r\n"
                                  + "read_sx:" + read_sx + "\r\n"
                                + ex.Source + "\r\n"
                                + frame + "\r\n"
                                + line);
                            sw.Close();
                            //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                        }
                    }
                    Thread.Sleep(10);
                }
                Thread.Sleep(10);
            }
        }

        //初始化控制器参数，默认电机类型：位置方式 编码器类型：差分四倍频
        private void InitialController()
        {
            try
            {
                //g_Controller.GCommand("MT2,-2;CE0,0"); //初始化控制器参数 MT:电机类型   CE:编码器类型
                //g_Controller.GCommand("MT2,-2;2.0"); //初始化控制器参数 MT:电机类型   CE:编码器类型
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("控制器初始化参数失败，请检查！");
            }
        }
        private void timer1_Tick(object sender, EventArgs e)
        {

            //若使用为C640A控制器，输入点1-8路输入信息读取为:g_Controller.GCommand("MG_TI0")
            //若使用为C640A控制器，输入点9-16路输入信息读取为:g_ControllerPLC.GCommand("TI0")
            //若使用为C640A控制器，输入点17-26路输入信息读取为:g_ControllerPLC.GCommand("TI1")
            //若使用为C640A控制器，输出点1-8路输入信息读取为:g_Controller.GCommand("MG_OP0")
            //若使用为C640A控制器，输出点9-16路输入信息读取为:g_ControllerPLC.GCommand("OP0")
            //若使用为C640A控制器，输出点17-26路输入信息读取为:g_ControllerPLC.GCommand("OP1")
            //输入点状态(1-8), 
            string str_Input_Bank0 = g_Controller.GCommand("MG_TI0");
            double dInput_Bank0 = Convert.ToDouble(str_Input_Bank0);
            for (int j = 0; j < 8; j++)
            {
                int iRes = ((int)dInput_Bank0 & (1 << j)) >> j;
                //switch (j)
                //{
                //    case 0:
                //        if (0 == iRes)
                //            label16.Text = "IN1有效";
                //        else
                //            label16.Text = "IN1无效";
                //        break;
                //    case 1:
                //        if (0 == iRes)
                //            label17.Text = "IN2有效";
                //        else
                //            label17.Text = "IN2无效";
                //        break;
                //    case 2:
                //        if (0 == iRes)
                //            label29.Text = "IN3有效";
                //        else
                //            label29.Text = "IN3无效";
                //        break;
                //    case 3:
                //        if (0 == iRes)
                //            label18.Text = "IN4有效";
                //        else
                //            label18.Text = "IN4无效";
                //        break;
                //    case 4:
                //        if (0 == iRes)
                //            label33.Text = "IN5有效";
                //        else
                //            label33.Text = "IN5无效";
                //        break;
                //    case 5:
                //        if (0 == iRes)
                //            label32.Text = "IN6有效";
                //        else
                //            label32.Text = "IN6无效";
                //        break;
                //    case 6:
                //        if (0 == iRes)
                //            label31.Text = "IN7有效";
                //        else
                //            label31.Text = "IN7无效";
                //        break;
                //    case 7:
                //        if (0 == iRes)
                //            label30.Text = "IN8有效";
                //        else
                //            label30.Text = "IN8无效";
                //        break;
                //    default: break;
                //}
                //X和Y轴命令位置
                label39.Text = g_Controller.GCommand("MG_RPX");
                label38.Text = g_Controller.GCommand("MG_RPY");
                //X和Y轴反馈位置
                label37.Text = g_Controller.GCommand("MG_TPX");
                label36.Text = g_Controller.GCommand("MG_TPY");
                //X和Y轴使能状态
                label35.Text = g_Controller.GCommand("MG_MOX");
                label34.Text = g_Controller.GCommand("MG_MOY");
                //X和Y轴零点状态
                string strHMX = g_Controller.GCommand("MG_HMX");
                string strHMY = g_Controller.GCommand("MG_HMY");
                //X和Y轴正反限位状态
                string strLFX = g_Controller.GCommand("MG_LFX");
                string strLFY = g_Controller.GCommand("MG_LFY");
                string strLRX = g_Controller.GCommand("MG_LRX");
                string strLRY = g_Controller.GCommand("MG_LRY");
            }
            //timer1.Enabled = false;              //先关定时器，防止任务重叠：函数没执行完，下一次时间周期已到
            //if (g_Controller != null)
            //{
            //    Int32 Line_TP_Count = 0;
            //    Int32 Rorate_TP_Count = 0;
            /*
            //没选择轴就开启定时器，然后返回
            if (Str_LineAxis == "" && Str_RorateAxis == "")
            {
                timer1.Enabled = true;
                return;
            }

            if (Str_LineAxis != "")
            {
                Line_TP_Count = Return_Encode_Count_Line(Str_LineAxis + Str_RorateAxis);  //一次只能一个轴运动，所以 Str_LineAxis + Str_RorateAxis
            }

            if (Str_RorateAxis != "")
            {
                Line_TP_Count = Return_Encode_Count_Rorate(Str_LineAxis + Str_RorateAxis);
            }

            Rorate_TP_Count = Line_TP_Count;

            //光栅尺位置--脉冲
            label_LineEncodePosition_Count.Text = Line_TP_Count.ToString();
            label_RorateEncodePosition_Count.Text = Rorate_TP_Count.ToString();


            double Line_temp = 0;
            double Rorate_temp = 0;

            Line_temp = Line_TP_Count;
            Line_temp = Line_temp / Common_Data.Line_LinearEncoderPluseCount;

            Rorate_temp = Rorate_TP_Count;
            Rorate_temp = Rorate_temp / Common_Data.Rotate_RotateEncoderPluseCount;

            //光栅尺位置--um
            label_LineEncodePosition.Text = Line_temp.ToString("F4") + " um";
            label_RorateEncodePosition.Text = Rorate_temp.ToString("F4") + " deg";

            //label15.Text = g_Controller.commandValue("RPA").ToString();    //调试
            //-----------------------
            Int32 LineEncode_List_Position_Count = 0;
            Int32 RorateEncode_List_Position_Count = 0;

            double LinePhy_List_Position = 0;
            double RoratePhy_List_Position = 0;

            double Double_K = 0;
            double Double_B = 0;


            //根据编码器反馈脉冲
            Int32 Ret = Get_Phy_Position_ByLineEncodeList(Line_TP_Count, ref LineEncode_List_Position_Count, ref LinePhy_List_Position);
            Ret = Get_CurseKB_ByLineEncodeList(Line_TP_Count, ref Double_K, ref Double_B);
            if (Ret == 0)
            {
                Phy_Position_Um = LinePhy_List_Position + (Line_TP_Count - LineEncode_List_Position_Count) * Double_K;
            }

            Ret = Get_Phy_Position_ByRorateEncodeList(Rorate_TP_Count, ref RorateEncode_List_Position_Count, ref RoratePhy_List_Position);
            Ret = Get_CurseKB_ByRorateEncodeList(Rorate_TP_Count, ref Double_K, ref Double_B);
            if (Ret == 0)
            {
                Phy_Position_Deg = RoratePhy_List_Position + (Rorate_TP_Count - RorateEncode_List_Position_Count) * Double_K;
            }

            //物理位置--um
            label_LinePhyPosition.Text = Phy_Position_Um.ToString("F4") + "um";
            label_RoratePhyPosition.Text = Phy_Position_Deg.ToString("F4") + "deg";


            //====查询电机停止原因，是否是回零
            //X轴
            double X_Location = g_Controller.commandValue("SCA");
            if (X_Location == 10)
            {
                X_L_Zero.Image = imageList1.Images[1];
                X_R_Zero.Image = imageList1.Images[1];
            }
            if (X_Location != 10)
            {
                X_L_Zero.Image = imageList1.Images[0];
                X_R_Zero.Image = imageList1.Images[0];
            }
            //Y轴
            double Y_Location = g_Controller.commandValue("SCB");
            if (Y_Location == 10)
            {
                Y_L_Zero.Image = imageList1.Images[1];
                Y_R_Zero.Image = imageList1.Images[1];
            }
            if (Y_Location != 10)
            {
                Y_L_Zero.Image = imageList1.Images[0];
                Y_R_Zero.Image = imageList1.Images[0];
            }
            //Z轴
            double Z_Location = g_Controller.commandValue("SCC");
            if (Z_Location == 10)
            {
                Z_L_Zero.Image = imageList1.Images[1];
                Z_R_Zero.Image = imageList1.Images[1];
            }
            if (Z_Location != 10)
            {
                Z_L_Zero.Image = imageList1.Images[0];
                Z_R_Zero.Image = imageList1.Images[0];
            }
            //U轴
            double U_Location = g_Controller.commandValue("SCD");
            if (U_Location == 10)
            {
                U_L_Zero.Image = imageList1.Images[1];
                U_R_Zero.Image = imageList1.Images[1];
            }
            if (U_Location != 10)
            {
                U_L_Zero.Image = imageList1.Images[0];
                U_R_Zero.Image = imageList1.Images[0];
            }

            //====查询开关状态
            //X轴
            double X_Status = 0;
            X_Status = g_Controller.commandValue("TSA");
            int X_Data = (int)X_Status;

            if ((int)(X_Data & 0x04) == 0)
            {
                X_L_Limit_L.Image = imageList1.Images[1];
                X_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(X_Data & 0x04) != 0)
            {
                X_L_Limit_L.Image = imageList1.Images[0];
                X_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(X_Data & 0x08) == 0)
            {
                X_L_Limit_R.Image = imageList1.Images[1];
                X_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(X_Data & 0x08) != 0)
            {
                X_L_Limit_R.Image = imageList1.Images[0];
                X_R_Limit_R.Image = imageList1.Images[0];
            }
            //Y轴
            double Y_Status = 0;
            Y_Status = g_Controller.commandValue("TSB");
            int Y_Data = (int)Y_Status;

            if ((int)(Y_Data & 0x04) == 0)
            {
                Y_L_Limit_L.Image = imageList1.Images[1];
                Y_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(Y_Data & 0x04) != 0)
            {
                Y_L_Limit_L.Image = imageList1.Images[0];
                Y_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(Y_Data & 0x08) == 0)
            {
                Y_L_Limit_R.Image = imageList1.Images[1];
                Y_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(Y_Data & 0x08) != 0)
            {
                Y_L_Limit_R.Image = imageList1.Images[0];
                Y_R_Limit_R.Image = imageList1.Images[0];
            }
            //Z轴
            double Z_Status = 0;
            Z_Status = g_Controller.commandValue("TSC");
            int Z_Data = (int)Z_Status;

            if ((int)(Z_Data & 0x04) == 0)
            {
                Z_L_Limit_L.Image = imageList1.Images[1];
                Z_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(Z_Data & 0x04) != 0)
            {
                Z_L_Limit_L.Image = imageList1.Images[0];
                Z_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(Z_Data & 0x08) == 0)
            {
                Z_L_Limit_R.Image = imageList1.Images[1];
                Z_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(Z_Data & 0x08) != 0)
            {
                Z_L_Limit_R.Image = imageList1.Images[0];
                Z_R_Limit_R.Image = imageList1.Images[0];
            }
            //U轴
            double U_Status = 0;
            U_Status = g_Controller.commandValue("TSD");
            int U_Data = (int)U_Status;

            if ((int)(U_Data & 0x04) == 0)
            {
                U_L_Limit_L.Image = imageList1.Images[1];
                U_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(U_Data & 0x04) != 0)
            {
                U_L_Limit_L.Image = imageList1.Images[0];
                U_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(U_Data & 0x08) == 0)
            {
                U_L_Limit_R.Image = imageList1.Images[1];
                U_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(U_Data & 0x08) != 0)
            {
                U_L_Limit_R.Image = imageList1.Images[0];
                U_R_Limit_R.Image = imageList1.Images[0];
            }

            */

            //}
            //timer1.Enabled = true;
        }
        private void ucBtnImg12_BtnClick(object sender, EventArgs e)
        {
            if (system_type.Equals("1"))
            {
                System1FindXYRZero();
            }
            if (system_type.Equals("2"))
            {
                System2FindXYRZero();
            }
        }

        public void System1FindXYRZero()
        {
            int idest = Convert.ToInt32(textBox6.Text);
            int ispeed = Convert.ToInt32(textBox4.Text);
            int iacc = Convert.ToInt32(textBoxAC.Text);
            int idcc = Convert.ToInt32(textBox3.Text);
            int ipulse = Convert.ToInt32(textBox5.Text);
            string find_zero = "SH;";
            string three = String.Format("AC{0};DC{1};SP{2};", iacc * ipulse * 10, idcc * ipulse * 10, ispeed * ipulse);
            g_Controller.GCommand(three);
            string find_zero_end = "";
            if (checkBoxXAxis.Checked)
            {
                //Thread.Sleep(send_delay_time_g);
                //g_Controller.GCommand("HMX;BGX");
                find_zero += "HMX;";
                find_zero_end += "X";
            }
            if (checkBoxYAxis.Checked)
            {
                //Thread.Sleep(send_delay_time_g);
                //g_Controller.GCommand("HMY" + ";BGY");
                find_zero += "HMY;";
                find_zero_end += "Y";
            }
            if (checkBoxZAxis.Checked)
            {
                //Thread.Sleep(send_delay_time_g);
                //g_Controller.GCommand("FIZ" + ";BGZ");
                find_zero += "FIZ;";
                find_zero_end += "Z";
            }
            find_zero += "BG" + find_zero_end;
            g_Controller.GCommand(find_zero);
            //手动清零
            g_Controller.GCommand("DP0,0,0");
            //g_Controller.GCommand("DE0,0,0");
        }

        public void ReConnectonXYR()
        {
            List<string> com_list = ReadFileToList("ComConfiguration.ini");
            if (com_list.Count > 0)
            {
                for (int i = 0; i < com_list.Count; i++)
                {
                    string com_sz = com_list[i];
                    string[] com_sz_array = com_sz.Split(':');
                    if (com_sz_array.Length > 1)
                    {
                        //if (com_sz_array[0].Equals("Pump"))
                        //{
                        //    //pump connect
                        //    Console.WriteLine(String.Format("Pump-{0}", com_sz_array[1]));
                        //    OpenCommuniction(serialPort_Send, com_sz_array[1], 9600);
                        //}
                        //if (com_sz_array[0].Equals("ZAxisAndSwitch"))
                        //{
                        //    //pump connect
                        //    Console.WriteLine(String.Format("ZAxisAndSwitch-{0}", com_sz_array[1]));
                        //    OpenCommuniction(serialPortvalve_Send, com_sz_array[1], 9600);
                        //}

                        if (system_type.Equals("2"))
                        {
                            if (com_sz_array[0].Equals("ASD_XCOM"))
                            {
                                if (!serialPort_ASD_X.IsOpen)
                                {
                                    OpenCommuniction(serialPort_ASD_X, com_sz_array[1], 115200);
                                }
                            }
                            else
                            {
                                move_connect_status_g = 0;
                            }

                            if (com_sz_array[0].Equals("ASD_YCOM"))
                            {
                                if (!serialPort_ASD_Y.IsOpen)
                                {
                                    OpenCommuniction(serialPort_ASD_Y, com_sz_array[1], 115200);
                                }

                            }
                            else
                            {
                                move_connect_status_g = 0;
                            }

                            if (com_sz_array[0].Equals("ASD_RCOM"))
                            {
                                if (!serialPort_ASD_R.IsOpen)
                                {
                                    OpenCommuniction(serialPort_ASD_R, com_sz_array[1], 115200);
                                }
                            }
                            else
                            {
                                move_connect_status_g = 0;
                            }
                        }
                    }
                }
            }
            Thread.Sleep(1000);
        }



        public void System2FindXYRZero()
        {
            read_s = "";
            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口—平台2");
                //return;
                ReConnectonXYR();
            }
            int offset = 60;//;
            int speed = 50;
            byte[] buf = Displacement.MoveABS(offset, speed);
            //serialPort1.Write(buf, 0, buf.Length);
            if (checkBoxXAxis.Checked)
            {
                serialPort_ASD_X.Write(buf, 0, buf.Length);
            }
            if (checkBoxYAxis.Checked)
            {
                offset = -50;
                buf = Displacement.MoveABS(offset, speed);
                serialPort_ASD_Y.Write(buf, 0, buf.Length);
            }
            if (checkBoxZAxis.Checked)
            {
                offset = 10;
                buf = Displacement.MoveABS(offset, speed);
                serialPort_ASD_R.Write(buf, 0, buf.Length);
            }

            Thread.Sleep(3000);

            buf = Displacement.HomeCmd();
            //serialPort1.Write(buf, 0, buf.Length);
            if (checkBoxXAxis.Checked)
            {
                serialPort_ASD_X.Write(buf, 0, buf.Length);
            }
            if (checkBoxYAxis.Checked)
            {
                serialPort_ASD_Y.Write(buf, 0, buf.Length);
            }
            if (checkBoxZAxis.Checked)
            {
                serialPort_ASD_R.Write(buf, 0, buf.Length);
            }
        }

        private void ucBtnImg13_BtnClick(object sender, EventArgs e)
        {

            if (system_type.Equals("1"))
            {
                System1Disable();
            }

            if (system_type.Equals("2"))
            {
                System2Disable();
            }

            //read_s = "";
            //if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            //{
            //    MessageBox.Show("请打开串口");
            //    return;
            //}
            //byte[] buf = Displacement.DisEnableNeed();
            ////serialPort1.Write(buf, 0, buf.Length);
            //serialPort_ASD_X.Write(buf, 0, buf.Length);
            //serialPort_ASD_Y.Write(buf, 0, buf.Length);
            //serialPort_ASD_R.Write(buf, 0, buf.Length);
        }

        public void System1Disable()
        {
            if (move_connect_status_g == 0)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            //g_Controller.GCommand("STY;MOY");
            if (checkBoxXAxis.Checked)
            {
                Thread.Sleep(send_delay_time_g);
                g_Controller.GCommand("STX;MOX");
            }

            if (checkBoxYAxis.Checked)
            {
                Thread.Sleep(send_delay_time_g);
                g_Controller.GCommand("STY;MOY");
            }

            if (checkBoxZAxis.Checked)
            {
                Thread.Sleep(send_delay_time_g);
                g_Controller.GCommand("STZ;MOZ");
            }
        }
        public void System2Disable()
        {
            read_s = "";
            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口");
                //return;
                ReConnectonXYR();
            }
            byte[] buf = Displacement.DisEnableNeed();
            //serialPort1.Write(buf, 0, buf.Length);
            serialPort_ASD_X.Write(buf, 0, buf.Length);
            serialPort_ASD_Y.Write(buf, 0, buf.Length);
            serialPort_ASD_R.Write(buf, 0, buf.Length);
        }
        public static int send_delay_time_g = 120;

        public List<SendOffset> GetSendOffsetList(List<MoveStartPosition> move_start_position_list)
        {
            List<SendOffset> send_offset_list = new List<SendOffset>();
            for (int i = 0; i < move_start_position_list.Count; i++)
            {
                SendOffset send_offset_y = new SendOffset();
                send_offset_y.Offset = move_start_position_list[i].Y_axis;
                send_offset_y.Type = 2;
                send_offset_y.Status = 0;
                send_offset_list.Add(send_offset_y);
                SendOffset send_offset_x = new SendOffset();
                send_offset_x.Offset = move_start_position_list[i].X_axis;
                send_offset_x.Type = 1;
                send_offset_x.Status = 0;
                send_offset_list.Add(send_offset_x);
                //SendOffset send_offset_r = new SendOffset();
                //send_offset_r.Offset = move_start_position_list[i].R_axis;
                //send_offset_r.Type = 3;
                //send_offset_r.Status = 0;
                //send_offset_list.Add(send_offset_r);
                //SendOffset send_offset_z1 = new SendOffset();
                //send_offset_z1.Offset = move_start_position_list_g[i].Z1_axis;
                //send_offset_z1.Type = 4;
                //send_offset_z1.Status = 0;
                //send_offset_list.Add(send_offset_z1);
                //SendOffset send_offset_z2 = new SendOffset();
                //send_offset_z2.Offset = move_start_position_list_g[i].Z2_axis;
                //send_offset_z2.Type = 5;
                //send_offset_z2.Status = 0;
                //send_offset_list.Add(send_offset_z2);
                //SendOffset send_offset_z3 = new SendOffset();
                //send_offset_z3.Offset = move_start_position_list_g[i].Z3_axis;
                //send_offset_z3.Type = 6;
                //send_offset_z3.Status = 0;
                //send_offset_list.Add(send_offset_z3);
            }
            return send_offset_list;
        }
        /// <summary>
        /// 
        /// </summary>
        /// <param name="move_start_position_list"></param>
        /// <param name="active_type">动作类型，这地方去判断哪个轴先运动，比如往下压的，要先动Y，再动x，再动Z，比如抬起一的。要先动Z再动Y，和X</param>
        /// <returns></returns>
        public List<SendOffset> GetSendOffsetListInital(List<MoveStartPosition> move_start_position_list, string active_type)
        {
            List<SendOffset> send_offset_list = new List<SendOffset>();
            for (int i = 0; i < move_start_position_list.Count; i++)
            {
                //平台初始化
                //装载芯片
                //下压芯片
                //打印开始
                //打印结束
                //显微镜观察
                if (active_type.Equals("下压芯片") || active_type.Equals("显微镜观察") || active_type.Equals("测试打印"))
                {

                    SendOffset send_offset_y = new SendOffset();
                    send_offset_y.Offset = move_start_position_list[i].Y_axis;
                    send_offset_y.Type = 2;
                    send_offset_y.Status = 0;
                    send_offset_y.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_y);
                    SendOffset send_offset_x = new SendOffset();
                    send_offset_x.Offset = move_start_position_list[i].X_axis;
                    send_offset_x.Type = 1;
                    send_offset_x.Id = send_offset_list_g.Count + 1;
                    send_offset_x.Status = 0;
                    send_offset_list_g.Add(send_offset_x);
                    SendOffset send_offset_z1 = new SendOffset();
                    send_offset_z1.Offset = move_start_position_list[i].Z1_axis;
                    send_offset_z1.Type = 4;
                    send_offset_z1.Status = 0;
                    send_offset_z1.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z1);
                    SendOffset send_offset_z2 = new SendOffset();
                    send_offset_z2.Offset = move_start_position_list[i].Z2_axis;
                    send_offset_z2.Type = 5;
                    send_offset_z2.Status = 0;
                    send_offset_z2.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z2);

                    SendOffset send_offset_z3 = new SendOffset();
                    send_offset_z3.Offset = move_start_position_list[i].Z3_axis;
                    send_offset_z3.Type = 6;
                    send_offset_z3.Status = 0;
                    send_offset_z3.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z3);




                    //if (system_type.Equals("2"))
                    //{
                    //}
                    //else
                    //{

                    //}
                }
                else
                {
                    SendOffset send_offset_z1 = new SendOffset();
                    send_offset_z1.Offset = move_start_position_list[i].Z1_axis;
                    send_offset_z1.Type = 4;
                    send_offset_z1.Status = 0;
                    send_offset_z1.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z1);
                    SendOffset send_offset_z2 = new SendOffset();
                    send_offset_z2.Offset = move_start_position_list[i].Z2_axis;
                    send_offset_z2.Type = 5;
                    send_offset_z2.Status = 0;
                    send_offset_z2.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z2);

                    SendOffset send_offset_z3 = new SendOffset();
                    send_offset_z3.Offset = move_start_position_list[i].Z3_axis;
                    send_offset_z3.Type = 6;
                    send_offset_z3.Status = 0;
                    send_offset_z3.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z3);

                    //if (system_type.Equals("2"))
                    //{
                    //}
                    //else
                    //{

                    //}
                    SendOffset send_offset_y = new SendOffset();
                    send_offset_y.Offset = move_start_position_list[i].Y_axis;
                    send_offset_y.Type = 2;
                    send_offset_y.Status = 0;
                    send_offset_y.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_y);
                    SendOffset send_offset_x = new SendOffset();
                    send_offset_x.Offset = move_start_position_list[i].X_axis;
                    send_offset_x.Type = 1;
                    send_offset_x.Id = send_offset_list_g.Count + 1;
                    send_offset_x.Status = 0;
                    send_offset_list_g.Add(send_offset_x);
                }

                //SendOffset send_offset_r = new SendOffset();
                //send_offset_r.Offset = move_start_position_list[i].R_axis;
                //send_offset_r.Type = 3;
                //send_offset_r.Status = 0;
                //send_offset_list.Add(send_offset_r);

            }
            return send_offset_list;
        }

        public List<SendOffset> GetSendOffsetListInitalQuery(List<MoveStartPosition> move_start_position_list, string active_type)
        {
            List<SendOffset> send_offset_list = new List<SendOffset>();
            for (int i = 0; i < move_start_position_list.Count; i++)
            {
                //平台初始化
                //装载芯片
                //下压芯片
                //打印开始
                //打印结束
                //显微镜观察
                if (active_type.Equals("下压芯片") || active_type.Equals("显微镜观察") || active_type.Equals("测试打印"))
                {
                    SendOffset send_offset_y = new SendOffset();
                    send_offset_y.Offset = move_start_position_list[i].Y_axis;
                    send_offset_y.Type = 2;
                    send_offset_y.Status = 0;
                    send_offset_y.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_y);
                    SendOffset send_offset_x = new SendOffset();
                    send_offset_x.Offset = move_start_position_list[i].X_axis;
                    send_offset_x.Type = 1;
                    send_offset_x.Id = send_offset_list_g.Count + 1;
                    send_offset_x.Status = 0;
                    send_offset_list_g.Add(send_offset_x);
                    SendOffset send_offset_z1 = new SendOffset();
                    send_offset_z1.Offset = move_start_position_list[i].Z1_axis;
                    send_offset_z1.Type = 4;
                    send_offset_z1.Status = 0;
                    send_offset_z1.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z1);
                    SendOffset send_offset_z2 = new SendOffset();
                    send_offset_z2.Offset = move_start_position_list[i].Z2_axis;
                    send_offset_z2.Type = 5;
                    send_offset_z2.Status = 0;
                    send_offset_z2.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z2);
                    SendOffset send_offset_z3 = new SendOffset();
                    send_offset_z3.Offset = move_start_position_list[i].Z3_axis;
                    send_offset_z3.Type = 6;
                    send_offset_z3.Status = 0;
                    send_offset_z3.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z3);
                }
                else
                {
                    SendOffset send_offset_z1 = new SendOffset();
                    send_offset_z1.Offset = move_start_position_list[i].Z1_axis;
                    send_offset_z1.Type = 4;
                    send_offset_z1.Status = 0;
                    send_offset_z1.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z1);
                    SendOffset send_offset_z2 = new SendOffset();
                    send_offset_z2.Offset = move_start_position_list[i].Z2_axis;
                    send_offset_z2.Type = 5;
                    send_offset_z2.Status = 0;
                    send_offset_z2.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z2);
                    SendOffset send_offset_z3 = new SendOffset();
                    send_offset_z3.Offset = move_start_position_list[i].Z3_axis;
                    send_offset_z3.Type = 6;
                    send_offset_z3.Status = 0;
                    send_offset_z3.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z3);

                    SendOffset send_offset_y = new SendOffset();
                    send_offset_y.Offset = move_start_position_list[i].Y_axis;
                    send_offset_y.Type = 2;
                    send_offset_y.Status = 0;
                    send_offset_y.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_y);
                    SendOffset send_offset_x = new SendOffset();
                    send_offset_x.Offset = move_start_position_list[i].X_axis;
                    send_offset_x.Type = 1;
                    send_offset_x.Id = send_offset_list_g.Count + 1;
                    send_offset_x.Status = 0;
                    send_offset_list_g_query.Add(send_offset_x);
                }


                //SendOffset send_offset_r = new SendOffset();
                //send_offset_r.Offset = move_start_position_list[i].R_axis;
                //send_offset_r.Type = 3;
                //send_offset_r.Status = 0;
                //send_offset_list.Add(send_offset_r);

            }
            return send_offset_list;
        }

        public static List<SendOffset> GetSendOffsetListInital_Static(List<MoveStartPosition> move_start_position_list, string active_type)
        {
            List<SendOffset> send_offset_list = new List<SendOffset>();
            for (int i = 0; i < move_start_position_list.Count; i++)
            {
                //平台初始化
                //装载芯片
                //下压芯片
                //打印开始
                //打印结束
                //显微镜观察
                if (active_type.Equals("下压芯片") || active_type.Equals("显微镜观察") || active_type.Equals("测试打印"))
                {
                    SendOffset send_offset_y = new SendOffset();
                    send_offset_y.Offset = move_start_position_list[i].Y_axis;
                    send_offset_y.Type = 2;
                    send_offset_y.Status = 0;
                    send_offset_y.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_y);
                    SendOffset send_offset_x = new SendOffset();
                    send_offset_x.Offset = move_start_position_list[i].X_axis;
                    send_offset_x.Type = 1;
                    send_offset_x.Id = send_offset_list_g.Count + 1;
                    send_offset_x.Status = 0;
                    send_offset_list_g.Add(send_offset_x);
                    SendOffset send_offset_z1 = new SendOffset();
                    send_offset_z1.Offset = move_start_position_list[i].Z1_axis;
                    send_offset_z1.Type = 4;
                    send_offset_z1.Status = 0;
                    send_offset_z1.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z1);
                    SendOffset send_offset_z2 = new SendOffset();
                    send_offset_z2.Offset = move_start_position_list[i].Z2_axis;
                    send_offset_z2.Type = 5;
                    send_offset_z2.Status = 0;
                    send_offset_z2.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z2);


                    SendOffset send_offset_z3 = new SendOffset();
                    send_offset_z3.Offset = move_start_position_list[i].Z3_axis;
                    send_offset_z3.Type = 6;
                    send_offset_z3.Status = 0;
                    send_offset_z3.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z3);

                    //Z3暂时先不要管
                    //Z3暂时先不要管
                    if (system_type.Equals("2"))
                    {


                    }
                    else
                    {

                    }

                }
                else
                {
                    SendOffset send_offset_z1 = new SendOffset();
                    send_offset_z1.Offset = move_start_position_list[i].Z1_axis;
                    send_offset_z1.Type = 4;
                    send_offset_z1.Status = 0;
                    send_offset_z1.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z1);
                    SendOffset send_offset_z2 = new SendOffset();
                    send_offset_z2.Offset = move_start_position_list[i].Z2_axis;
                    send_offset_z2.Type = 5;
                    send_offset_z2.Status = 0;
                    send_offset_z2.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z2);

                    //Z3暂时先不要管
                    if (system_type.Equals("2"))
                    { }
                    else
                    {

                    }

                    SendOffset send_offset_z3 = new SendOffset();
                    send_offset_z3.Offset = move_start_position_list[i].Z3_axis;
                    send_offset_z3.Type = 6;
                    send_offset_z3.Status = 0;
                    send_offset_z3.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_z3);

                    SendOffset send_offset_y = new SendOffset();
                    send_offset_y.Offset = move_start_position_list[i].Y_axis;
                    send_offset_y.Type = 2;
                    send_offset_y.Status = 0;
                    send_offset_y.Id = send_offset_list_g.Count + 1;
                    send_offset_list_g.Add(send_offset_y);
                    SendOffset send_offset_x = new SendOffset();
                    send_offset_x.Offset = move_start_position_list[i].X_axis;
                    send_offset_x.Type = 1;
                    send_offset_x.Id = send_offset_list_g.Count + 1;
                    send_offset_x.Status = 0;
                    send_offset_list_g.Add(send_offset_x);
                }

                //SendOffset send_offset_r = new SendOffset();
                //send_offset_r.Offset = move_start_position_list[i].R_axis;
                //send_offset_r.Type = 3;
                //send_offset_r.Status = 0;
                //send_offset_list.Add(send_offset_r);

            }
            return send_offset_list;
        }

        private void ucBtnImg15_BtnClick(object sender, EventArgs e)
        {
            if (system_type.Equals("1"))
            {
                XYRMoveSystem1();
            }
            if (system_type.Equals("2"))
            {
                XYRMoveSystem2();
            }
        }

        /// <summary>
        /// 清故障
        /// </summary>
        public void ClearXYRFault()
        {
            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口");
                //return;
                ReConnectonXYR();
            }
            byte[] buf = Displacement.Clearfaults();
            //serialPort1.Write(buf, 0, buf.Length);
            if (checkBoxXAxis.Checked)
            {
                serialPort_ASD_X.Write(buf, 0, buf.Length);
            }
            if (checkBoxYAxis.Checked)
            {
                serialPort_ASD_Y.Write(buf, 0, buf.Length);
            }
            if (checkBoxZAxis.Checked)
            {
                serialPort_ASD_R.Write(buf, 0, buf.Length);
            }
        }

        public List<SendQueryXYR_Model> sendQueryXYR_Models_list = new List<SendQueryXYR_Model>();
        public void XYRMoveSystem3(int axis_type, float offset)
        {
            read_sx = "";
            read_sy = "";
            read_sr = "";
            sendQueryXYR_Models_list = new List<SendQueryXYR_Model>();

            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口");
                //return;
                ReConnectonXYR();
            }

            //float offset = float.Parse(textBox6.Text);//;
            int speed = int.Parse(textBox4.Text);

            //byte[] buf = Displacement.MoveABS(offset, speed);
            byte[] buf = Displacement.MoveABSFloat(offset, speed);
            //serialPort1.Write(buf, 0, buf.Length);

            if (axis_type == 1)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "X";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_X.Write(buf, 0, buf.Length);
                x_query_flage = true;
            }

            if (axis_type == 2)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "Y";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_Y.Write(buf, 0, buf.Length);
                y_query_flage = true;
            }

            if (axis_type == 3)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "R";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_R.Write(buf, 0, buf.Length);
                r_query_flage = true;
            }

        }

        public void XYRMoveSystem2()
        {
            read_s = "";
            sendQueryXYR_Models_list = new List<SendQueryXYR_Model>();

            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口");
                //return;
                ReConnectonXYR();
            }
            float offset = float.Parse(textBox6.Text);//;
            int speed = int.Parse(textBox4.Text);

            //byte[] buf = Displacement.MoveABS(offset, speed);
            byte[] buf = Displacement.MoveABSFloat(offset, speed);
            //serialPort1.Write(buf, 0, buf.Length);

            if (checkBoxXAxis.Checked)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "X";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_X.Write(buf, 0, buf.Length);
            }

            if (checkBoxYAxis.Checked)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "Y";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_Y.Write(buf, 0, buf.Length);
            }

            if (checkBoxZAxis.Checked)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "R";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_R.Write(buf, 0, buf.Length);
            }

        }

        public void XYRMoveSystem1()
        {
            if (move_connect_status_g == 0)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            //g_Controller.GCommand("DP0,0,0");
            //g_Controller.GCommand("DE0,0,0");
            int idest = Convert.ToInt32(textBox6.Text);
            int ispeed = Convert.ToInt32(textBox4.Text);
            int iacc = Convert.ToInt32(textBoxAC.Text);
            int idcc = Convert.ToInt32(textBox3.Text);
            int ipulse = Convert.ToInt32(textBox5.Text);
            //string move_offset = "DP0,0,0;DE0,0,0;";
            string move_offset = "PA";
            string move_offset_end = "";
            //string three = String.Format("DP0,0,0;DE0,0,0;AC{0};DC{1};SP{2};", iacc * ipulse, idcc * ipulse, ispeed * ipulse);
            string three = String.Format("ACX={0};DCX={1};SPX={2};", iacc * ipulse, idcc * ipulse, ispeed * ipulse);
            string y_speed = String.Format("ACY={0};DCY={1};JGY={2};", iacc * ipulse, idcc * ipulse, ispeed * ipulse);
            if (checkBoxXAxis.Checked)
            {
                //Thread.Sleep(send_delay_time_g);
                string strText = String.Format("{0}", idest * ipulse);
                move_offset += strText;
                move_offset_end += "X";
                //g_Controller.GCommand(strText);
            }

            move_offset += ",";
            if (checkBoxYAxis.Checked)
            {
                //Thread.Sleep(send_delay_time_g);
                string strText = String.Format("{0}", idest * ipulse);
                move_offset += strText;
                move_offset_end += "Y";
                //g_Controller.GCommand(strText);
            }

            move_offset += ",";
            if (checkBoxZAxis.Checked)
            {
                //Thread.Sleep(send_delay_time_g);
                string strText = String.Format("{0}", idest * ipulse);
                move_offset += strText;
                move_offset_end += "Z";
                //g_Controller.GCommand(strText);
            }

            move_offset += ";";
            move_offset += "BG" + move_offset_end;
            move_offset = three + move_offset;
            g_Controller.GCommand(move_offset);
        }

        /// <summary>
        /// 位移平台运动
        /// </summary>
        /// <param name="axis_type">1=>x,2=>y,3=>z</param>
        public void MoveAxisValue(int axis_type, int offset)
        {
            if (system_type.Equals("1"))
            {
                System1MoveAxisValue(axis_type, offset);
            }
            float offset1 = ((float)offset / 10000);
            if (system_type.Equals("2"))
            {
                XYRMoveSystem3(axis_type, offset1);
            }

        }
        public void System1MoveAxisValue(int axis_type, int offset)
        {
            if (move_connect_status_g == 0)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            //g_Controller.GCommand("DP0,0,0");
            //g_Controller.GCommand("DE0,0,0");
            int idest = offset;//Convert.ToInt32(textBox6.Text);
            int ispeed = Convert.ToInt32(textBox4.Text);
            int iacc = Convert.ToInt32(textBoxAC.Text);
            int idcc = Convert.ToInt32(textBox3.Text);
            int ipulse = Convert.ToInt32(textBox5.Text);
            //string move_offset = "DP0,0,0;DE0,0,0;";
            string move_offset = "PA";
            string move_offset_end = "";
            //string three = String.Format("DP0,0,0;DE0,0,0;AC{0};DC{1};SP{2};", iacc * ipulse, idcc * ipulse, ispeed * ipulse);
            string three = String.Format("ACX={0};DCX={1};SPX={2};", iacc * ipulse, idcc * ipulse, ispeed * ipulse);
            string y_speed = String.Format("ACY={0};DCY={1};JGY={2};", iacc * ipulse, idcc * ipulse, ispeed * ipulse);
            if (axis_type == 1)
            {
                //Thread.Sleep(send_delay_time_g);
                string strText = String.Format("{0}", idest * ipulse);
                move_offset += strText;
                move_offset_end += "X";
                //g_Controller.GCommand(strText);
            }
            move_offset += ",";
            if (axis_type == 2)
            {
                //Thread.Sleep(send_delay_time_g);
                string strText = String.Format("{0}", idest * ipulse);
                move_offset += strText;
                move_offset_end += "Y";
                //g_Controller.GCommand(strText);
            }
            move_offset += ",";
            if (axis_type == 3)
            {
                //Thread.Sleep(send_delay_time_g);
                string strText = String.Format("{0}", idest * ipulse);
                move_offset += strText;
                move_offset_end += "Z";
                //g_Controller.GCommand(strText);
            }
            move_offset += ";";
            move_offset += "BG" + move_offset_end;
            move_offset = three + y_speed + move_offset;
            g_Controller.GCommand(move_offset);
        }
        public void System2MoveAxisValue(int axis_type, int offset)
        {
            read_sx = "";
            read_sy = "";
            read_sr = "";
            sendQueryXYR_Models_list = new List<SendQueryXYR_Model>();

            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口");
                //return;
                ReConnectonXYR();
            }

            int speed = int.Parse(textBox4.Text);

            //byte[] buf = Displacement.MoveABS(offset, speed);
            byte[] buf = Displacement.MoveABSFloat(offset, speed);
            //serialPort1.Write(buf, 0, buf.Length);

            if (axis_type == 1)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "X";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_X.Write(buf, 0, buf.Length);
            }

            if (axis_type == 2)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "Y";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_Y.Write(buf, 0, buf.Length);
            }

            if (axis_type == 3)
            {
                SendQueryXYR_Model m = new SendQueryXYR_Model();
                m.Send_status = 1;
                m.Xyr_type = "R";
                m.offset = offset;
                m.Id = sendQueryXYR_Models_list.Count + 1;
                sendQueryXYR_Models_list.Add(m);
                serialPort_ASD_R.Write(buf, 0, buf.Length);
            }


        }

        private void ucBtnImg14_BtnClick(object sender, EventArgs e)
        {
            if (system_type.Equals("1"))
            {
                System1Enable();
            }

            if (system_type.Equals("2"))
            {
                System2Enable();
            }
        }

        public void System1Enable()
        {
            if (move_connect_status_g == 0)
            {
                MessageBox.Show("位移平台未上电，或是未连接");
                return;
            }
            if (checkBoxXAxis.Checked)
            {
                Thread.Sleep(send_delay_time_g);
                g_Controller.GCommand("SHX");
            }
            if (checkBoxYAxis.Checked)
            {
                Thread.Sleep(send_delay_time_g);
                g_Controller.GCommand("SHY");
            }
            if (checkBoxZAxis.Checked)
            {
                Thread.Sleep(send_delay_time_g);
                g_Controller.GCommand("SHZ");
            }
        }

        public void System2Enable()
        {
            read_s = "";
            if (!serialPort_ASD_X.IsOpen || !serialPort_ASD_Y.IsOpen || !serialPort_ASD_R.IsOpen)
            {
                //MessageBox.Show("请打开串口");
                //return;
                ReConnectonXYR();
            }
            byte[] buf = Displacement.EnableNeed();
            //serialPort1.Write(buf, 0, buf.Length);
            serialPort_ASD_X.Write(buf, 0, buf.Length);
            serialPort_ASD_Y.Write(buf, 0, buf.Length);
            serialPort_ASD_R.Write(buf, 0, buf.Length);
        }

        private void ucBtnImg16_BtnClick(object sender, EventArgs e)
        {
            if (system_type.Equals("1"))
            {
                if (move_connect_status_g == 0)
                {
                    MessageBox.Show("位移平台未上电，或是未连接");
                    return;
                }
                g_Controller.GCommand("AB");
            }

            if (system_type.Equals("2"))
            {
                ClearXYRFault();
            }

        }

        public void CloseCam()
        {
            try
            {
                // 如果未停采则先停止采集
                if (m_bIsSnap)
                {
                    if (null != m_objIGXFeatureControl)
                    {
                        m_objIGXFeatureControl.GetCommandFeature("AcquisitionStop").Execute();
                    }
                }
            }
            catch (Exception)
            {

            }

            try
            {
                //停止流通道、注销采集回调和关闭流
                if (null != m_objIGXStream)
                {
                    m_objIGXStream.StopGrab();
                    m_objIGXStream.UnregisterCaptureCallback();
                    m_objIGXStream.Close();
                    m_objIGXStream = null;
                }
            }
            catch (Exception)
            {

            }

            try
            {
                //关闭设备
                if (null != m_objIGXDevice)
                {
                    m_objIGXDevice.Close();
                    m_objIGXDevice = null;
                }
            }
            catch (Exception)
            {

            }

            try
            {
                //反初始化
                if (null != m_objIGXFactory)
                {
                    m_objIGXFactory.Uninit();
                }
            }
            catch (Exception)
            {

            }
        }

        private void FrmPrintATCG_FormClosing(object sender, FormClosingEventArgs e)
        {
            CloseCam();
            System.Environment.Exit(0);
            //myTimer.Stop();
            //g_Controller.GClose();
            //System.Environment.Exit(System.Environment.ExitCode); //等等
        }
        public bool ConnectMoveSystem()
        {
            try
            {
                g_Controller.GOpen("192.168.0.101 -d"); //通过控制器IP地址连接控制器
                return true;
            }
            catch (System.Exception ex)
            {
                Console.WriteLine("位移平台连接失败，请检查！");
                return false;
            }
        }
        private void StartTimer()
        {
            //启动定时器，开始计时
            //给timer挂起事件  
            myTimer.Tick += new EventHandler(Callback);
            //使timer可用  
            myTimer.Enabled = true;
            //设置时间间隔，以毫秒为单位  
            myTimer.Interval = 50;//50ms
        }
        //回调函数  
        private void Callback(object sender, EventArgs e)
        {
            //若使用为C640A控制器，输入点1-8路输入信息读取为:g_Controller.GCommand("MG_TI0")
            //若使用为C640A控制器，输入点9-16路输入信息读取为:g_ControllerPLC.GCommand("TI0")
            //若使用为C640A控制器，输入点17-26路输入信息读取为:g_ControllerPLC.GCommand("TI1")
            //若使用为C640A控制器，输出点1-8路输入信息读取为:g_Controller.GCommand("MG_OP0")
            //若使用为C640A控制器，输出点9-16路输入信息读取为:g_ControllerPLC.GCommand("OP0")
            //若使用为C640A控制器，输出点17-26路输入信息读取为:g_ControllerPLC.GCommand("OP1")
            //输入点状态(1-8), 
            string str_Input_Bank0 = g_Controller.GCommand("MG_TI0");
            if ("".Equals(str_Input_Bank0))
            {
                EndMoveController();
                MessageBox.Show("通讯异常");
                return;
            }
            double dInput_Bank0 = Convert.ToDouble(str_Input_Bank0);
            for (int j = 0; j < 8; j++)
            {
                int iRes = ((int)dInput_Bank0 & (1 << j)) >> j;
                //switch (j)
                //{
                //    case 0:
                //        if (0 == iRes)
                //            label16.Text = "IN1有效";
                //        else
                //            label16.Text = "IN1无效";
                //        break;
                //    case 1:
                //        if (0 == iRes)
                //            label17.Text = "IN2有效";
                //        else
                //            label17.Text = "IN2无效";
                //        break;
                //    case 2:
                //        if (0 == iRes)
                //            label29.Text = "IN3有效";
                //        else
                //            label29.Text = "IN3无效";
                //        break;
                //    case 3:
                //        if (0 == iRes)
                //            label18.Text = "IN4有效";
                //        else
                //            label18.Text = "IN4无效";
                //        break;
                //    case 4:
                //        if (0 == iRes)
                //            label33.Text = "IN5有效";
                //        else
                //            label33.Text = "IN5无效";
                //        break;
                //    case 5:
                //        if (0 == iRes)
                //            label32.Text = "IN6有效";
                //        else
                //            label32.Text = "IN6无效";
                //        break;
                //    case 6:
                //        if (0 == iRes)
                //            label31.Text = "IN7有效";
                //        else
                //            label31.Text = "IN7无效";
                //        break;
                //    case 7:
                //        if (0 == iRes)
                //            label30.Text = "IN8有效";
                //        else
                //            label30.Text = "IN8无效";
                //        break;
                //    default: break;
                //}

                //X和Y轴命令位置
                label39.Text = g_Controller.GCommand("MG_RPX");
                label38.Text = g_Controller.GCommand("MG_RPY");
                label48.Text = g_Controller.GCommand("MG_RPZ");
                //X和Y轴反馈位置
                label37.Text = g_Controller.GCommand("MG_TPX");
                label36.Text = g_Controller.GCommand("MG_TPY");
                label47.Text = g_Controller.GCommand("MG_TPZ");
                //X和Y轴使能状态
                label35.Text = g_Controller.GCommand("MG_MOX");
                label34.Text = g_Controller.GCommand("MG_MOY");
                label46.Text = g_Controller.GCommand("MG_MOZ");
                //X和Y轴零点状态
                string strHMX = g_Controller.GCommand("MG_HMX");
                string strHMY = g_Controller.GCommand("MG_HMY");
                //X和Y轴正反限位状态
                string strLFX = g_Controller.GCommand("MG_LFX");
                string strLFY = g_Controller.GCommand("MG_LFY");
                string strLRX = g_Controller.GCommand("MG_LRX");
                string strLRY = g_Controller.GCommand("MG_LRY");
            }
            //timer1.Enabled = false;              //先关定时器，防止任务重叠：函数没执行完，下一次时间周期已到
            //if (g_Controller != null)
            //{
            //    Int32 Line_TP_Count = 0;
            //    Int32 Rorate_TP_Count = 0;
            /*
            //没选择轴就开启定时器，然后返回
            if (Str_LineAxis == "" && Str_RorateAxis == "")
            {
                timer1.Enabled = true;
                return;
            }
            if (Str_LineAxis != "")
            {
                Line_TP_Count = Return_Encode_Count_Line(Str_LineAxis + Str_RorateAxis);  //一次只能一个轴运动，所以 Str_LineAxis + Str_RorateAxis
            }
            if (Str_RorateAxis != "")
            {
                Line_TP_Count = Return_Encode_Count_Rorate(Str_LineAxis + Str_RorateAxis);
            }
            Rorate_TP_Count = Line_TP_Count;
            //光栅尺位置--脉冲
            label_LineEncodePosition_Count.Text = Line_TP_Count.ToString();
            label_RorateEncodePosition_Count.Text = Rorate_TP_Count.ToString();
            double Line_temp = 0;
            double Rorate_temp = 0;
            Line_temp = Line_TP_Count;
            Line_temp = Line_temp / Common_Data.Line_LinearEncoderPluseCount;
            Rorate_temp = Rorate_TP_Count;
            Rorate_temp = Rorate_temp / Common_Data.Rotate_RotateEncoderPluseCount;
            //光栅尺位置--um
            label_LineEncodePosition.Text = Line_temp.ToString("F4") + " um";
            label_RorateEncodePosition.Text = Rorate_temp.ToString("F4") + " deg";
            //label15.Text = g_Controller.commandValue("RPA").ToString();    //调试
            //-----------------------
            Int32 LineEncode_List_Position_Count = 0;
            Int32 RorateEncode_List_Position_Count = 0;
            double LinePhy_List_Position = 0;
            double RoratePhy_List_Position = 0;
            double Double_K = 0;
            double Double_B = 0;
            //根据编码器反馈脉冲
            Int32 Ret = Get_Phy_Position_ByLineEncodeList(Line_TP_Count, ref LineEncode_List_Position_Count, ref LinePhy_List_Position);
            Ret = Get_CurseKB_ByLineEncodeList(Line_TP_Count, ref Double_K, ref Double_B);
            if (Ret == 0)
            {
                Phy_Position_Um = LinePhy_List_Position + (Line_TP_Count - LineEncode_List_Position_Count) * Double_K;
            }
            Ret = Get_Phy_Position_ByRorateEncodeList(Rorate_TP_Count, ref RorateEncode_List_Position_Count, ref RoratePhy_List_Position);
            Ret = Get_CurseKB_ByRorateEncodeList(Rorate_TP_Count, ref Double_K, ref Double_B);
            if (Ret == 0)
            {
                Phy_Position_Deg = RoratePhy_List_Position + (Rorate_TP_Count - RorateEncode_List_Position_Count) * Double_K;
            }
            //物理位置--um
            label_LinePhyPosition.Text = Phy_Position_Um.ToString("F4") + "um";
            label_RoratePhyPosition.Text = Phy_Position_Deg.ToString("F4") + "deg";
            //====查询电机停止原因，是否是回零
            //X轴
            double X_Location = g_Controller.commandValue("SCA");
            if (X_Location == 10)
            {
                X_L_Zero.Image = imageList1.Images[1];
                X_R_Zero.Image = imageList1.Images[1];
            }
            if (X_Location != 10)
            {
                X_L_Zero.Image = imageList1.Images[0];
                X_R_Zero.Image = imageList1.Images[0];
            }
            //Y轴
            double Y_Location = g_Controller.commandValue("SCB");
            if (Y_Location == 10)
            {
                Y_L_Zero.Image = imageList1.Images[1];
                Y_R_Zero.Image = imageList1.Images[1];
            }
            if (Y_Location != 10)
            {
                Y_L_Zero.Image = imageList1.Images[0];
                Y_R_Zero.Image = imageList1.Images[0];
            }
            //Z轴
            double Z_Location = g_Controller.commandValue("SCC");
            if (Z_Location == 10)
            {
                Z_L_Zero.Image = imageList1.Images[1];
                Z_R_Zero.Image = imageList1.Images[1];
            }
            if (Z_Location != 10)
            {
                Z_L_Zero.Image = imageList1.Images[0];
                Z_R_Zero.Image = imageList1.Images[0];
            }
            //U轴
            double U_Location = g_Controller.commandValue("SCD");
            if (U_Location == 10)
            {
                U_L_Zero.Image = imageList1.Images[1];
                U_R_Zero.Image = imageList1.Images[1];
            }
            if (U_Location != 10)
            {
                U_L_Zero.Image = imageList1.Images[0];
                U_R_Zero.Image = imageList1.Images[0];
            }

            //====查询开关状态
            //X轴
            double X_Status = 0;
            X_Status = g_Controller.commandValue("TSA");
            int X_Data = (int)X_Status;

            if ((int)(X_Data & 0x04) == 0)
            {
                X_L_Limit_L.Image = imageList1.Images[1];
                X_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(X_Data & 0x04) != 0)
            {
                X_L_Limit_L.Image = imageList1.Images[0];
                X_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(X_Data & 0x08) == 0)
            {
                X_L_Limit_R.Image = imageList1.Images[1];
                X_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(X_Data & 0x08) != 0)
            {
                X_L_Limit_R.Image = imageList1.Images[0];
                X_R_Limit_R.Image = imageList1.Images[0];
            }
            //Y轴
            double Y_Status = 0;
            Y_Status = g_Controller.commandValue("TSB");
            int Y_Data = (int)Y_Status;

            if ((int)(Y_Data & 0x04) == 0)
            {
                Y_L_Limit_L.Image = imageList1.Images[1];
                Y_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(Y_Data & 0x04) != 0)
            {
                Y_L_Limit_L.Image = imageList1.Images[0];
                Y_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(Y_Data & 0x08) == 0)
            {
                Y_L_Limit_R.Image = imageList1.Images[1];
                Y_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(Y_Data & 0x08) != 0)
            {
                Y_L_Limit_R.Image = imageList1.Images[0];
                Y_R_Limit_R.Image = imageList1.Images[0];
            }
            //Z轴
            double Z_Status = 0;
            Z_Status = g_Controller.commandValue("TSC");
            int Z_Data = (int)Z_Status;

            if ((int)(Z_Data & 0x04) == 0)
            {
                Z_L_Limit_L.Image = imageList1.Images[1];
                Z_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(Z_Data & 0x04) != 0)
            {
                Z_L_Limit_L.Image = imageList1.Images[0];
                Z_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(Z_Data & 0x08) == 0)
            {
                Z_L_Limit_R.Image = imageList1.Images[1];
                Z_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(Z_Data & 0x08) != 0)
            {
                Z_L_Limit_R.Image = imageList1.Images[0];
                Z_R_Limit_R.Image = imageList1.Images[0];
            }
            //U轴
            double U_Status = 0;
            U_Status = g_Controller.commandValue("TSD");
            int U_Data = (int)U_Status;

            if ((int)(U_Data & 0x04) == 0)
            {
                U_L_Limit_L.Image = imageList1.Images[1];
                U_R_Limit_L.Image = imageList1.Images[1];
            }
            if ((int)(U_Data & 0x04) != 0)
            {
                U_L_Limit_L.Image = imageList1.Images[0];
                U_R_Limit_L.Image = imageList1.Images[0];
            }

            if ((int)(U_Data & 0x08) == 0)
            {
                U_L_Limit_R.Image = imageList1.Images[1];
                U_R_Limit_R.Image = imageList1.Images[1];
            }
            if ((int)(U_Data & 0x08) != 0)
            {
                U_L_Limit_R.Image = imageList1.Images[0];
                U_R_Limit_R.Image = imageList1.Images[0];
            }

            */

            //}
            //timer1.Enabled = true;
        }
        /// <summary>
        /// 位移平台连接状态 0为未连接，1为连接成功
        /// </summary>
        public static int move_connect_status_g = 0;
        /// <summary>
        /// 启动位移平台服务
        /// </summary>
        public void StartMoveController()
        {

            if (ConnectMoveSystem())
            {
                ucBtnImg17.BtnText = "断开";
                //InitialController();
                //暂时关闭查询返回
                //StartTimer();
                move_connect_status_g = 1;
            }
            else
            {
                MessageBox.Show("连接异常，设备为上电或是，通讯异常，请查看故障灯及设备是否正常运行");
            }
        }
        /// <summary>
        /// 结束位移平台服务
        /// </summary>
        public void EndMoveController()
        {
            ucBtnImg17.BtnText = "连接";
            myTimer.Stop();
            g_Controller.GClose();
            move_connect_status_g = 0;
        }
        private void ucBtnImg17_BtnClick(object sender, EventArgs e)
        {
            if (ucBtnImg17.BtnText.Equals("连接"))
            {
                StartMoveController();
            }
            else
            {
                EndMoveController();
            }
        }
        public static List<MoveStartPosition> move_start_position_list_g = new List<MoveStartPosition>();
        public static MoveStartPosition move_start_position_selected_g = new MoveStartPosition();
        /// <summary>
        /// 发送指令
        /// </summary>
        static List<SendOffset> send_offset_list_g = new List<SendOffset>();
        /// <summary>
        /// 获取位移平台开始位置配置文件
        /// </summary>
        public void LoadMoveStartPositionModel()
        {

            //try
            //{
            //    string xml = LSXmlSerializer.ReadXmlFromFile("MoveStartPositionModel.xml");
            //    //反序列化为list
            //    move_start_position_list_g = LSXmlSerializer.DESerializer<List<MoveStartPosition>>(xml);
            //    dataGridView2.DataSource = move_start_position_list_g;
            //    dataGridView2.Columns["Id"].HeaderCell.Value = "编号";
            //    dataGridView2.Columns["Name"].HeaderCell.Value = "名称";
            //    dataGridView2.Columns["X_axis"].HeaderCell.Value = "X";
            //    dataGridView2.Columns["Y_axis"].HeaderCell.Value = "Y";
            //    dataGridView2.Columns["R_axis"].HeaderCell.Value = "R";
            //    dataGridView2.Columns["Z1_axis"].HeaderCell.Value = "Z1";
            //    dataGridView2.Columns["Z2_axis"].HeaderCell.Value = "Z2";
            //    dataGridView2.Columns["Z3_axis"].HeaderCell.Value = "Z3";
            //    dataGridView2.Columns["Stutas"].Visible = false;
            //}
            //catch
            //{
            //}


            if (system_type.Equals("1"))
            {
                try
                {
                    string xml = LSXmlSerializer.ReadXmlFromFile("MoveStartPositionModelSystem1.xml");
                    //反序列化为list
                    move_start_position_list_g = LSXmlSerializer.DESerializer<List<MoveStartPosition>>(xml);
                    dataGridView2.DataSource = move_start_position_list_g;
                    dataGridView2.Columns["Id"].HeaderCell.Value = "编号";
                    dataGridView2.Columns["Name"].HeaderCell.Value = "名称";
                    dataGridView2.Columns["X_axis"].HeaderCell.Value = "X";
                    dataGridView2.Columns["Y_axis"].HeaderCell.Value = "Y";
                    dataGridView2.Columns["R_axis"].HeaderCell.Value = "R";
                    dataGridView2.Columns["Z1_axis"].HeaderCell.Value = "Z1";
                    dataGridView2.Columns["Z2_axis"].HeaderCell.Value = "Z2";
                    dataGridView2.Columns["Z3_axis"].HeaderCell.Value = "Z3";
                    dataGridView2.Columns["Stutas"].Visible = false;
                }
                catch
                {
                }
            }

            if (system_type.Equals("2"))
            {
                try
                {
                    string xml = LSXmlSerializer.ReadXmlFromFile("MoveStartPositionModel.xml");
                    //反序列化为list
                    move_start_position_list_g = LSXmlSerializer.DESerializer<List<MoveStartPosition>>(xml);
                    dataGridView2.DataSource = move_start_position_list_g;
                    dataGridView2.Columns["Id"].HeaderCell.Value = "编号";
                    dataGridView2.Columns["Name"].HeaderCell.Value = "名称";
                    dataGridView2.Columns["X_axis"].HeaderCell.Value = "X";
                    dataGridView2.Columns["Y_axis"].HeaderCell.Value = "Y";
                    dataGridView2.Columns["R_axis"].HeaderCell.Value = "R";
                    dataGridView2.Columns["Z1_axis"].HeaderCell.Value = "Z1";
                    dataGridView2.Columns["Z2_axis"].HeaderCell.Value = "Z2";
                    dataGridView2.Columns["Z3_axis"].HeaderCell.Value = "Z3";
                    dataGridView2.Columns["Stutas"].Visible = false;
                }
                catch
                {
                }
            }





        }
        public void SaveMoveStartPositionModelToFile()
        {


            if (system_type.Equals("1"))
            {

                ////序列化为xml
                bool strXML = LSXmlSerializer.XmlSerializeSaveFile<List<MoveStartPosition>>(move_start_position_list_g, "MoveStartPositionModelSystem1.xml");
            }

            if (system_type.Equals("2"))
            {
                ////序列化为xml
                bool strXML = LSXmlSerializer.XmlSerializeSaveFile<List<MoveStartPosition>>(move_start_position_list_g, "MoveStartPositionModel.xml");
            }

        }

        private void ucBtnImg19_BtnClick(object sender, EventArgs e)
        {
            if (textBoxName.Text == String.Empty || textBoxXAxis.Text == String.Empty || textBoxYAxis.Text == String.Empty || textBoxRAxis.Text == String.Empty || textBoxZ1.Text == String.Empty)
            {
                return;
            }
            try
            {
                string name = textBoxName.Text.ToString();
                int x_axis = int.Parse(textBoxXAxis.Text.ToString());
                int y_xis = int.Parse(textBoxYAxis.Text.ToString());
                int r_Axis = int.Parse(textBoxRAxis.Text.ToString());

                int z1_Axis = int.Parse(textBoxZ1.Text.ToString());
                int z2_Axis = int.Parse(textBoxZ2.Text.ToString());
                int z3_Axis = int.Parse(textBoxZ3.Text.ToString());

                //int equivalent = int.Parse(textBoxEquivalent.Text.ToString());

                //int stutas = 1;
                MoveStartPosition move_start_position = new MoveStartPosition();
                int id = 0;
                id = move_start_position_list_g.FindLast(o => o.Id != -1).Id + 1;
                move_start_position.Id = move_start_position_list_g.Count == 0 ? 0 : id;//(move_start_position_list_g.Count);
                move_start_position.Name = name;
                move_start_position.X_axis = x_axis;
                move_start_position.Y_axis = y_xis;
                move_start_position.R_axis = r_Axis;
                move_start_position.Z1_axis = z1_Axis;
                move_start_position.Z2_axis = z2_Axis;
                move_start_position.Z3_axis = z3_Axis;
                //move_start_position.Equivalent = equivalent;
                //move_start_position.Stutas = stutas;
                move_start_position_list_g.Add(move_start_position);
                SaveMoveStartPositionModelToFile();
                //Console.Write(strXML);
                LoadMoveStartPositionModel();
            }
            catch (System.Exception ex)
            {
                System.Console.WriteLine("====================================");
                System.Console.WriteLine(ex.Message);
                System.Console.WriteLine(ex.StackTrace);
            }
        }
        private void ucBtnImg18_BtnClick(object sender, EventArgs e)
        {
            if (textBoxName.Text == String.Empty || textBoxXAxis.Text == String.Empty || textBoxYAxis.Text == String.Empty || textBoxRAxis.Text == String.Empty || textBoxZ1.Text == String.Empty)
            {
                return;
            }
            string name = textBoxName.Text.ToString();
            int x_axis = int.Parse(textBoxXAxis.Text.ToString());
            int y_xis = int.Parse(textBoxYAxis.Text.ToString());
            int r_Axis = int.Parse(textBoxRAxis.Text.ToString());
            int z1 = int.Parse(textBoxZ1.Text.ToString());
            int z2 = int.Parse(textBoxZ2.Text.ToString());
            int z3 = int.Parse(textBoxZ3.Text.ToString());

            move_start_position_selected_g.Name = name;
            move_start_position_selected_g.X_axis = x_axis;
            move_start_position_selected_g.Y_axis = y_xis;
            move_start_position_selected_g.R_axis = r_Axis;
            move_start_position_selected_g.Z1_axis = z1;
            move_start_position_selected_g.Z2_axis = z2;
            move_start_position_selected_g.Z3_axis = z3;
            //move_start_position_selected_g.Equivalent = equivalent;
            if (move_start_position_selected_g.Name == null)
            {
                //MessageBox.Show("xxxxxxxxxxx");
            }
            else
            {
                var move_start_position_model = move_start_position_list_g.Where(c => c.Id == move_start_position_selected_g.Id).FirstOrDefault();
                move_start_position_model.Name = move_start_position_selected_g.Name;
                move_start_position_model.X_axis = move_start_position_selected_g.X_axis;
                move_start_position_model.Y_axis = move_start_position_selected_g.Y_axis;
                move_start_position_model.R_axis = move_start_position_selected_g.R_axis;
                move_start_position_model.Z1_axis = move_start_position_selected_g.Z1_axis;
                move_start_position_model.Z2_axis = move_start_position_selected_g.Z2_axis;
                move_start_position_model.Z3_axis = move_start_position_selected_g.Z3_axis;
                //move_start_position_model.Equivalent = move_start_position_selected_g.Equivalent;
                SaveMoveStartPositionModelToFile();
                //Console.Write(strXML);
                LoadMoveStartPositionModel();
                //置空输入框，在修改关加上判断，如果输入框都为空，那么返回不进行修改。
                textBoxName.Text = "";
                textBoxXAxis.Text = "";
                textBoxYAxis.Text = "";
                textBoxRAxis.Text = "";
                textBoxZ1.Text = "";
            }
        }
        public static object LoadFromXml(string filePath, Type type)
        {
            object result = null;

            if (File.Exists(filePath))
            {
                using (StreamReader reader = new StreamReader(filePath))
                {
                    System.Xml.Serialization.XmlSerializer xmlSerializer = new System.Xml.Serialization.XmlSerializer(type);
                    result = xmlSerializer.Deserialize(reader);
                }
            }
            return result;
        }

        private void dataGridView2_CellMouseDoubleClick(object sender, DataGridViewCellMouseEventArgs e)
        {
            int id = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[0].Value.ToString());
            string name = this.dataGridView2.Rows[e.RowIndex].Cells[1].Value.ToString();
            int x_axis = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[2].Value.ToString());
            int y_axis = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[3].Value.ToString());
            int z_axis = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[4].Value.ToString());
            int equivalent = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[5].Value.ToString());

            int z1 = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[5].Value.ToString());
            int z2 = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[6].Value.ToString());
            int z3 = int.Parse(this.dataGridView2.Rows[e.RowIndex].Cells[7].Value.ToString());

            textBoxName.Text = name;
            textBoxXAxis.Text = "" + x_axis;
            textBoxYAxis.Text = "" + y_axis;
            textBoxRAxis.Text = "" + z_axis;
            textBoxZ1.Text = "" + z1;
            textBoxZ2.Text = "" + z2;
            textBoxZ3.Text = "" + z3;


            move_start_position_selected_g.Id = id;
            move_start_position_selected_g.Name = name;
            move_start_position_selected_g.X_axis = x_axis;
            move_start_position_selected_g.Y_axis = y_axis;
            move_start_position_selected_g.R_axis = z_axis;

            move_start_position_selected_g.Z1_axis = z1;
            move_start_position_selected_g.Z2_axis = z2;
            move_start_position_selected_g.Z3_axis = z3;

            //move_start_position_selected_g.Equivalent = equivalent;
        }
        private void ucBtnImg21_BtnClick(object sender, EventArgs e)
        {
            if (move_start_position_selected_g.Name == null)
            {
                MessageBox.Show("xxxxxxxxxxx");
            }
            else
            {
                MoveStartPosition delete_object = new MoveStartPosition();
                delete_object = move_start_position_list_g.Find(o => o.Id == move_start_position_selected_g.Id);
                move_start_position_list_g.Remove(delete_object);
                SaveMoveStartPositionModelToFile();
                LoadMoveStartPositionModel();
            }
        }
        //public void AddList(List<SendOffset> send_offset_list)
        //{
        //    if(send_offset_list_g.FindAll(o=>o.Status==1).Count>0)
        //    {
        //        MessageBox.Show("有未完成发送的指令");
        //    }else
        //    {
        //        send_offset_list_g.AddRange(send_offset_list);
        //    }
        //    //send_offset_list_g
        //}
        private void ucBtnImg20_BtnClick(object sender, EventArgs e)
        {
            //send_offset_list_g = new List<SendOffset>();
            //List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("平台初始化"));
            //List<SendOffset> send_offset_list = GetSendOffsetListInital(move_start_position_list);
            //send_offset_list_g.AddRange(send_offset_list);
            //send_wait_flage = false;
            //send_offset_list_flage = true;
            OneActive("平台初始化");
        }
        public static void OneActive_Static(string active_name)
        {
            send_offset_list_g = new List<SendOffset>();
            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals(active_name));
            List<SendOffset> send_offset_list = GetSendOffsetListInital_Static(move_start_position_list, active_name);
            //send_offset_list_g.AddRange(send_offset_list);
            send_wait_flage = false;
            send_offset_list_flage = true;
        }
        public void OneActive(string active_name)
        {
            send_offset_list_g = new List<SendOffset>();
            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals(active_name));
            List<SendOffset> send_offset_list = GetSendOffsetListInital(move_start_position_list, active_name);
            //send_offset_list_g.AddRange(send_offset_list);
            send_wait_flage = false;
            send_offset_list_flage = true;
        }

        public void OneActiveQuery(string active_name)
        {
            send_offset_list_g = new List<SendOffset>();
            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals(active_name));
            List<SendOffset> send_offset_list = GetSendOffsetListInital(move_start_position_list, active_name);
            //send_offset_list_g.AddRange(send_offset_list);
            send_wait_flage = false;
            send_offset_list_flage = true;
        }
        public void AddSendOffsetList(string active_name)
        {
            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals(active_name));
            List<SendOffset> send_offset_list = GetSendOffsetListInital(move_start_position_list, active_name);
            send_offset_list_g.AddRange(send_offset_list);
        }

        public static bool send_wait_flage = false;
        public static bool send_offset_list_flage = false;
        public void SendMovePlateCommand()
        {
            while (true)
            {
                while (send_offset_list_flage)
                {
                    try
                    {
                        //Console.WriteLine("dddd");
                        if (send_offset_list_g.Count > 0)
                        {
                            if (send_offset_list_g[0].Status == 0)
                            {
                                while (send_wait_flage)
                                {
                                    Thread.Sleep(1);
                                }
                                if (send_offset_list_g.Count > 0)
                                {
                                    //如果是x，y，R那么
                                    if (send_offset_list_g[0].Type == 1 || send_offset_list_g[0].Type == 2 || send_offset_list_g[0].Type == 3)
                                    {
                                        MoveAxisValue(send_offset_list_g[0].Type, send_offset_list_g[0].Offset);
                                        send_offset_list_g[0].Status = 1;
                                        send_wait_flage = true;
                                        Thread.Sleep(150);
                                    }

                                    //如果是z
                                    if (send_offset_list_g[0].Type == 4 || send_offset_list_g[0].Type == 5 || send_offset_list_g[0].Type == 6)
                                    {
                                        int type = send_offset_list_g[0].Type;
                                        byte z_index = 1;
                                        switch (type)
                                        {
                                            case 4:
                                                z_index = 1;
                                                break;
                                            case 5:
                                                z_index = 2;
                                                break;
                                            case 6:
                                                z_index = 3;
                                                break;
                                        }

                                        SendZAxismotorMove(z_index, send_offset_list_g[0].Offset);
                                        send_offset_list_g[0].Status = 1;
                                        send_wait_flage = true;
                                        Thread.Sleep(150);
                                    }
                                }
                            }
                        }
                        Thread.Sleep(150);
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                        var st = new StackTrace(ex, true);
                        var frame = st.GetFrame(0);
                        var line = frame.GetFileLineNumber();
                        var sw = new System.IO.StreamWriter("发送平台指令异常.txt", true);
                        sw.WriteLine(
                            DateTime.Now.ToString() + "\r\n"
                             + "异常:" + "\r\n"
                            + ex.Message + "\r\n"
                            + "read_sx:" + read_sx + "\r\n"
                            + ex.Source + "\r\n"
                            + frame + "\r\n"
                            + line);
                        sw.Close();
                        //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                    }
                }

                Thread.Sleep(100);
            }
        }
        /// <summary>
        /// 处理system1平台返回的xyr结果
        /// </summary>
        public static string valuex_g = "";
        public static string valuey_g = "";
        public static string valuer_g = "";

        public void QueryRx()
        {
            while (true)
            {
                if (send_offset_list_g.Count > 0)
                {
                    for (int i = 0; i < send_offset_list_g.Count; i++)
                    {
                        try
                        {



                            if (send_offset_list_g[0].Status == 1)
                            {
                                try
                                {
                                    if (send_offset_list_g.Find(o => o.Status == 1) != null)
                                    {
                                        int offset = send_offset_list_g.Find(o => o.Status == 1).Offset;
                                        int type = send_offset_list_g.Find(o => o.Status == 1).Type;
                                        int id = send_offset_list_g.Find(o => o.Status == 1).Id;
                                        switch (type)
                                        {
                                            case 1:
                                                string valuex = "";
                                                if (system_type.Equals("1"))
                                                {
                                                    valuex = g_Controller.GCommand("MG_RPX");
                                                }

                                                if (system_type.Equals("2"))
                                                {
                                                    valuex = valuex_g;
                                                }

                                                if (system_type.Equals("1"))
                                                {
                                                    if (offset == DoubleStringToInt(valuex))
                                                    {
                                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                        send_wait_flage = false;
                                                    }
                                                }

                                                if (system_type.Equals("2"))
                                                {
                                                    if (((float)offset / 10000) == DoubleStringTofloat(valuex))
                                                    {
                                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                        send_wait_flage = false;
                                                    }
                                                }


                                                break;
                                            case 2:
                                                string valuey = "";
                                                if (system_type.Equals("1"))
                                                {
                                                    valuey = g_Controller.GCommand("MG_RPY");
                                                }
                                                if (system_type.Equals("2"))
                                                {
                                                    valuey = valuey_g;
                                                }


                                                if (system_type.Equals("1"))
                                                {
                                                    if (offset == DoubleStringToInt(valuey))
                                                    {
                                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                        send_wait_flage = false;
                                                    }
                                                }

                                                if (system_type.Equals("2"))
                                                {
                                                    if (((float)offset / 10000) == DoubleStringTofloat(valuey))
                                                    {
                                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                        send_wait_flage = false;
                                                    }
                                                }




                                                break;
                                            case 3:

                                                string valuer = "";
                                                if (system_type.Equals("1"))
                                                {
                                                    valuer = g_Controller.GCommand("MG_RPZ");
                                                }
                                                if (system_type.Equals("2"))
                                                {
                                                    valuer = valuer_g;
                                                }

                                                if (system_type.Equals("1"))
                                                {
                                                    if (offset == DoubleStringToInt(valuer))
                                                    {
                                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                        send_wait_flage = false;
                                                    }
                                                }

                                                if (system_type.Equals("2"))
                                                {
                                                    if (((float)offset / 10000) == DoubleStringTofloat(valuer))
                                                    {
                                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                        send_wait_flage = false;
                                                    }
                                                }



                                                break;
                                            case 4:
                                                //发送查询Z位置指令
                                                loop_send_query_z_axis_position = true;
                                                break;
                                            case 5:
                                                loop_send_query_z_axis_position = true;
                                                break;
                                            case 6:
                                                loop_send_query_z_axis_position = true;
                                                break;
                                        }
                                    }
                                    /*
                                    switch (type)
                                    {
                                        case 1:
                                            string valuex = g_Controller.GCommand("MG_RPX");
                                            if (offset == DoubleStringToInt(valuex))
                                            {
                                                send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                send_wait_flage = false;
                                            }
                                            break;
                                        case 2:
                                            string valuey = g_Controller.GCommand("MG_RPY");
                                            if (offset == DoubleStringToInt(valuey))
                                            {
                                                send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                send_wait_flage = false;
                                            }
                                            break;
                                        case 3:
                                            string valuer = g_Controller.GCommand("MG_RPZ");
                                            if (offset == DoubleStringToInt(valuer))
                                            {
                                                send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id));
                                                send_wait_flage = false;
                                            }
                                            break;
                                        case 4:
                                            //发送查询Z位置指令
                                            loop_send_query_z_axis_position = true;
                                            break;
                                        case 5:
                                            loop_send_query_z_axis_position = true;
                                            break;
                                        case 6:
                                            loop_send_query_z_axis_position = true;
                                            break;
                                    }

                                    */
                                }
                                catch
                                {

                                }
                            }
                            Thread.Sleep(200);


                        }
                        catch (Exception ex)
                        {


                            var st = new StackTrace(ex, true);
                            var frame = st.GetFrame(0);
                            var line = frame.GetFileLineNumber();
                            var sw = new System.IO.StreamWriter("QueryRx.txt", true);

                            sw.WriteLine(
                                DateTime.Now.ToString() + "\r\n"
                                 + "接收异常:" + "\r\n"
                                + ex.Message + "\r\n"
                                + ex.InnerException + "\r\n"
                                + ex.Source + "\r\n"
                                + frame + "\r\n"
                                + line);
                            sw.Close();


                        }

                    }

                }
                Thread.Sleep(200);
            }
        }
        public void DealZReturn(byte[] receive_data)
        {
            UInt16 length = (UInt16)(receive_data.Length - 2);
            UInt16 crc = FreeModbus.MB_CRC16(receive_data, length);
            if (receive_data[1] == 4 && receive_data[2] == 4)
            {
                byte a = (byte)crc;                     /* crc 低字节 */
                byte b = (byte)(crc >> 8);              /* crc 高字节 */
                //进行CRC检验
                if (receive_data[7] == a && receive_data[8] == b)
                {
                    //int c = 0;
                    Console.WriteLine("----------------------");
                    if (send_offset_list_g.Count > 0)
                    {
                        byte[] offset_array = new byte[4];
                        //receive_data.CopyTo(offset_array, 10);//
                        Array.Copy(receive_data, 3, offset_array, 0, 4);
                        if (send_offset_list_g.Find(o => o.Status == 1) != null)
                        {
                            int id1 = send_offset_list_g.Find(o => o.Status == 1).Id;
                            int offset1 = send_offset_list_g.Find(o => o.Status == 1).Offset;
                            int offset2 = bytes44ToInt(offset_array, 0);

                            if ((offset2 == offset1) || offset2 == 0)
                            {
                                send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id1));
                                loop_send_query_z_axis_position = false;
                                send_wait_flage = false;
                            }
                            //记录 

                            var sw = new System.IO.StreamWriter("Z记录.txt", true);

                            sw.WriteLine(
                                DateTime.Now.ToString() + "\r\n"
                                 + "接收:" + "\r\n"
                                + receive_data[0] + "设置："
                                + offset1 + "接收："
                                + offset2 + "\r\n"
                                + m_count_z_value + "\r\n"
                                + "");
                            sw.Close();

                            if (receive_data[0] == 2)
                            {
                                if ((0 < Math.Abs(offset1 - offset2)) && Math.Abs(offset1 - offset2) < 5)
                                {
                                    m_count_z_value++;
                                    if (m_count_z_value == 100)
                                    {
                                        if (offset1 < 100)
                                        {
                                            SendZAxisMotorFindZero(2);
                                            Thread.Sleep(20000);
                                            send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id1));
                                            loop_send_query_z_axis_position = false;
                                            send_wait_flage = false;
                                            m_count_z_value = 0;
                                        }
                                    }
                                }
                                else
                                {
                                    m_count_z_value = 0;
                                }
                            }

                            if (receive_data[0] != 2)
                            {
                                if (0 < (Math.Abs(offset1 - offset2)) && (Math.Abs(offset1 - offset2) < 15))
                                {
                                    m_count_z_value++;
                                    if (m_count_z_value == 30)
                                    {
                                        send_offset_list_g.Remove(send_offset_list_g.Find(o => o.Id == id1));
                                        loop_send_query_z_axis_position = false;
                                        send_wait_flage = false;
                                        Thread.Sleep(1000);
                                        m_count_z_value = 0;
                                    }
                                }
                                else
                                {
                                    m_count_z_value = 0;
                                }
                            }
                        }
                    }
                }
            }
        }
        public static int m_count_z_value = 0;
        /// <summary>
        /// 只要为true，就一直去查，直到查到的和发送的位置一样，就置为false
        /// </summary>
        public bool loop_send_query_z_axis_position = false;
        /// <summary>
        /// 发送查询Z轴位置指令
        /// </summary>
        public void SendQueryZAxisPositionloop()
        {
            while (true)
            {
                while (loop_send_query_z_axis_position)
                {
                    try
                    {
                        if (send_offset_list_g.Count > 0)
                        {
                            if (send_offset_list_g.Find(o => o.Status == 1) != null)
                            {
                                //一发一查
                                if (send_wait_flage)
                                {
                                    Thread.Sleep(260);
                                    if (send_offset_list_g.Count > 0)
                                    {
                                        SendOffset send_offset = send_offset_list_g.Find(o => o.Status == 1);
                                        if (send_offset != null)
                                        {
                                            int offset = send_offset_list_g.Find(o => o.Status == 1).Offset;
                                            int type = send_offset_list_g.Find(o => o.Status == 1).Type;
                                            int id = send_offset_list_g.Find(o => o.Status == 1).Id;
                                            switch (type)
                                            {
                                                case 4:
                                                    //发送查询Z位置指令
                                                    SendReadZAXisPosition(1);
                                                    break;
                                                case 5:
                                                    //发送查询Z位置指令
                                                    SendReadZAXisPosition(2);
                                                    break;
                                                case 6:
                                                    //发送查询Z位置指令
                                                    SendReadZAXisPosition(3);
                                                    break;
                                            }
                                            Thread.Sleep(280);
                                        }
                                    }

                                }

                            }
                        }

                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                        var st = new StackTrace(ex, true);
                        var frame = st.GetFrame(0);
                        var line = frame.GetFileLineNumber();
                        var sw = new System.IO.StreamWriter("SendQueryZPosition异常.txt", true);
                        sw.WriteLine(
                            DateTime.Now.ToString() + "\r\n"
                             + "异常:" + "\r\n"
                            + ex.Message + "\r\n"
                            + "read_sx:" + read_sx + "\r\n"
                            + ex.Source + "\r\n"
                            + frame + "\r\n"
                            + line);
                        sw.Close();
                        //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                    }

                }
                Thread.Sleep(150);
            }
        }

        public float DoubleStringTofloat(string value)
        {
            float a = float.Parse(value);
            return a;
        }

        public int DoubleStringToInt(string value)
        {
            int value_new = 0;
            double a = double.Parse(value);
            value_new = (int)a;
            return value_new;
        }

        private void ucBtnImg22_BtnClick(object sender, EventArgs e)
        {
            //send_offset_list_g = new List<SendOffset>();
            //List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("装载芯片"));
            //List<SendOffset> send_offset_list = GetSendOffsetList(move_start_position_list);
            //send_offset_list_g.AddRange(send_offset_list);
            //send_wait_flage = false;
            //send_offset_list_flage = true;
            OneActive("装载芯片");
        }

        private void ucBtnImg23_BtnClick(object sender, EventArgs e)
        {


            //send_offset_list_g = new List<SendOffset>();
            //List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("下压芯片"));
            //List<SendOffset> send_offset_list = GetSendOffsetList(move_start_position_list);
            //send_offset_list_g.AddRange(send_offset_list);
            //send_wait_flage = false;
            //send_offset_list_flage = true;
            OneActive("下压芯片");
        }


        /// <summary>
        /// 获取串口完整名字（包括驱动名字）
        /// 如果找不到类，需要添加System.Management引用，添加引用->程序集->System.Management
        /// </summary>
        Dictionary<String, String> coms = new Dictionary<String, String>();
        /// <summary>
        /// 获得电脑串口列表
        /// </summary>
        private void getPortDeviceName()
        {
            coms.Clear();
            using (ManagementObjectSearcher searcher = new ManagementObjectSearcher
            ("select * from Win32_PnPEntity where Name like '%(COM%'"))
            {
                var hardInfos = searcher.Get();
                foreach (var hardInfo in hardInfos)
                {
                    if (hardInfo.Properties["Name"].Value != null)
                    {
                        string deviceName = hardInfo.Properties["Name"].Value.ToString();
                        int startIndex = deviceName.IndexOf("(");
                        int endIndex = deviceName.IndexOf(")");
                        string key = deviceName.Substring(startIndex + 1, deviceName.Length - startIndex - 2);
                        string name = deviceName.Substring(0, startIndex - 1);
                        Console.WriteLine("key:" + key + ",name:" + name + ",deviceName:" + deviceName);
                        coms.Add(key, name);
                    }
                }
                //创建一个用来更新UI的委托 (主线程更新)
                this.Invoke(
                     new Action(() =>
                     {
                         comboBox1.Items.Clear();
                         foreach (KeyValuePair<string, string> kvp in coms)
                         {
                             comboBox1.Items.Add(kvp.Key + " " + kvp.Value);//更新下拉列表中的串口
                         }
                     })
                 );
            }
        }
        private void ucBtnImg24_BtnClick(object sender, EventArgs e)
        {
            //OpenCommuniction();
        }
        public void OpenCommuniction(SerialPort serial, string com_sz, int BaudRate)
        {
            if (com_sz.Length > 0)
            {
                if (!serial.IsOpen)
                {
                    try
                    {
                        serial.BaudRate = BaudRate;
                        serial.PortName = com_sz;//com_number_array[0];
                        //serialPort_Send.DataBits = 8;
                        serial.Open();//打开串口
                        serial.DataBits = 8;
                        serial.Parity = (Parity)0;
                        //serialPort1.StopBits =1;
                        serial.ReadTimeout = 2000;
                        serial.WriteTimeout = 2000;
                        serial.RtsEnable = true;
                        serial.DtrEnable = true;
                        serial.ReceivedBytesThreshold = 1;
                    }
                    catch
                    {
                        FrmDialog.ShowDialog(this, "连接失败，端口已经被别的程序占用", "提示");
                    }
                }
            }
        }


        private void ucBtnImg27_BtnClick(object sender, EventArgs e)
        {
            if (comboBox1.SelectedItem == null)
            {
                //MessageBox.Show("请选择串口");
                FrmDialog.ShowDialog(this, "请选择串口", "提示");
                return;
            }
            if (serialPortvalve_Send.IsOpen)
            {
                FrmDialog.ShowDialog(this, "已连接设置，无需再次连接", "提示");
            }

            string com_number = comboBox1.SelectedItem.ToString();

            //string com_number = "COM4 USB Serial Port";

            string[] com_number_array = com_number.Split(' ');
            if (com_number_array.Length > 0)
            {
                if (!serialPortvalve_Send.IsOpen)
                {
                    try
                    {
                        serialPortvalve_Send.BaudRate = 9600;
                        serialPortvalve_Send.PortName = com_number_array[0];
                        //serialPort_Send.DataBits = 8;
                        serialPortvalve_Send.Open();//打开串口
                        //serialPort1.Write(buf, 0, buf.Length);
                        //serialPortvalve_Send.DataReceived += new System.IO.Ports.SerialDataReceivedEventHandler(this.OnDataReceivedValve);
                        //serialPort_Send.WriteBufferSize = 1024;
                        //serialPort_Send.ReadBufferSize = 1024;
                        //serialPort1.ReadBufferSize = 1024;
                        serialPortvalve_Send.DataBits = 8;
                        serialPortvalve_Send.Parity = (Parity)0;
                        //serialPort1.StopBits =1;
                        //serialPortvalve_Send.ReadTimeout = 120000;
                        serialPortvalve_Send.WriteTimeout = 120000;
                        serialPortvalve_Send.ReadTimeout = 120000;
                        serialPortvalve_Send.RtsEnable = true;
                        serialPortvalve_Send.DtrEnable = true;
                        serialPortvalve_Send.ReceivedBytesThreshold = 1;

                        //串口默认的ReceivedBytesThreshold是1
                        //https://www.cnblogs.com/haofaner/p/3402307.html
                        ucBtnImg27.Enabled = false;
                        ucBtnImg27.FillColor = Color.Gray;
                    }
                    catch
                    {
                        ucBtnImg27.Enabled = true;
                        ucBtnImg27.FillColor = Color.Black;
                        FrmDialog.ShowDialog(this, "连接失败，端口已经被别的程序占用", "提示");
                    }
                }
            }
        }
        private List<byte> buffer = new List<byte>(140960);

        public byte[] buf_new_g_z_return_array = new byte[9];

        private List<byte> buffer_pump_list = new List<byte>(140960);
        /// <summary>
        /// 是否接收数据
        /// </summary>
        public static bool is_receive_data_g = true;
        public byte[] g_RS232DataBuff;
        public byte[] g_RS232DataBuff1;
        public int g_i_DataBuffLen = 0;
        public int g_l_ReceivedCount = 0;
        public static string read_s = "";
        public static string read_sx = "";
        public static string read_sy = "";
        public static string read_sr = "";

        public void PrintBuffer(byte[] readBuffer)
        {
            string recev_data = "";
            //拼成一个完整指令
            if (readBuffer.Length > 1)
            {
                recev_data += string.Join(" ", readBuffer);
            }
            else
            {
                recev_data += string.Join(" ", readBuffer);
            }
            byte[] recev_ds = StringToByteArray(recev_data);
            //byte[] recev_ds = StringToByteArray(recev_data);
            //recev_data = string.Join(" ", recev_ds);
            //string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
            //Console.Write(" time:" + log_time + "收到指令：" + recev_data + "\r\n");
            //处理返回结果

            try
            {
                DealReceiveData(recev_ds);
            }
            catch (Exception eee)
            {

                var st = new StackTrace(eee, true);
                var frame = st.GetFrame(0);
                var line = frame.GetFileLineNumber();
                var sw = new System.IO.StreamWriter("Exception.txt", true);

                sw.WriteLine(
                    DateTime.Now.ToString() + "\r\n"
                     + "DealReceiveData:" + "\r\n"
                    + eee.Message + "\r\n"
                    + eee.InnerException + "\r\n"
                    + eee.Source + "\r\n"
                    + frame + "\r\n"
                    + line);
                sw.Close();

            }
        }
        public byte[] StringToByteArray(string rec)
        {
            string[] rec_s = rec.Split(' ');
            byte[] rec_byte_array = new byte[rec_s.Length];
            for (int i = 0; i < rec_s.Length; i++)
            {
                //int num = 255;
                if (!rec_s[i].Equals(""))
                {
                    byte b = Convert.ToByte(int.Parse(rec_s[i]));
                    rec_byte_array[i] = b;//Convert.ToByte(rec_s[i], 16);
                }
            }
            return rec_byte_array;
        }
        //private void OnDataReceived(object sender, SerialDataReceivedEventArgs e)
        //{
        //    if (is_receive_data_g)
        //    {
        //        byte[] readBuffer = null;
        //        int n = serialPort_Send.BytesToRead;
        //        byte[] buf = new byte[n];
        //        serialPort_Send.Read(buf, 0, n);
        //        //1.缓存数据           
        //        buffer.AddRange(buf);
        //        if (buf.Length >= 7)
        //        {
        //            PrintBuffer(buf);
        //        }
        //    }
        //}

        /// <summary>
        /// 查找一个byte数组在另一个byte数组第一次出现位置
        /// </summary>
        /// <param name="array">被查找的数组</param>
        /// <param name="array2">要查找的数组</param>
        /// <returns>找到返回索引，找不到返回-1</returns>
        static int FindIndex(byte[] array, byte[] array2)
        {
            int i, j;

            for (i = 0; i < array.Length; i++)
            {
                if (i + array2.Length <= array.Length)
                {
                    for (j = 0; j < array2.Length; j++)
                    {
                        if (array[i + j] != array2[j]) break;
                    }

                    if (j == array2.Length) return i;
                }
                else
                    break;
            }

            return -1;
        }
        public static int a_idex = 0;



        public void ReadPumpRx()
        {

            while (true)
            {
                if (serialPort_Send.IsOpen)
                {

                    //接收数据
                    do
                    {
                        if (is_receive_data_g)
                        {
                            //byte[] readBuffer = null;
                            //int n = serialPort_Send.BytesToRead;
                            //byte[] buf = new byte[n];
                            //serialPort_Send.Read(buf, 0, n);
                            int count = serialPort_Send.BytesToRead;
                            if (count > 0)
                            {
                                g_RS232DataBuff1 = new byte[count];
                                serialPort_Send.Read(g_RS232DataBuff1, 0, count);
                                //1.缓存数据           
                                //buffer_pump_list.AddRange(buf);
                                if (g_RS232DataBuff1.Length >= 7)
                                {
                                    PrintBuffer(g_RS232DataBuff1);
                                }
                            }
                        }
                    } while (serialPort_Send.BytesToRead > 0);

                    //处理接收后的命令
                    //TranslateCommand2();

                }
                Thread.Sleep(200);
            }
        }

        public void ReadZAxisRx()
        {
            while (true)
            {
                if (serialPortvalve_Send.IsOpen)
                {
                    try
                    {
                        //接收数据
                        do
                        {
                            int count = serialPortvalve_Send.BytesToRead;
                            if (count > 0)
                            {
                                g_RS232DataBuff = new byte[count];
                                serialPortvalve_Send.Read(g_RS232DataBuff, 0, count);
                                //1.缓存数据           
                                //buffer.AddRange(g_RS232DataBuff);
                                byte[] value4_4 = new byte[2];
                                value4_4[0] = 4;
                                value4_4[1] = 4;
                                string recev_data = "";
                                //拼成一个完整指令
                                if (g_RS232DataBuff.Length > 1)
                                {
                                    recev_data += string.Join(" ", g_RS232DataBuff);
                                }
                                else
                                {
                                    recev_data += string.Join(" ", g_RS232DataBuff);
                                }
                                //byte[] recev_ds = StringToByteArray(buf.ToString());
                                //byte[] recev_ds = StringToByteArray(recev_data);
                                ////recev_data = string.Join(" ", recev_ds);
                                ///
                                string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
                                Console.Write("Z轴电机收到指令 time:" + log_time + "收到指令：" + recev_data + "\r\n");
                                var sw = new System.IO.StreamWriter("Z轴电机收到指令.txt", true);
                                sw.WriteLine(
                                    DateTime.Now.ToString() + "\r\n"
                                     + "Z轴电机收到指令:" + "\r\n"
                                    + log_time + "\r\n"
                                    + recev_data);
                                sw.Close();


                                //处理查询
                                try
                                {
                                    if (g_RS232DataBuff.Length >= 9)
                                    {
                                        if (g_RS232DataBuff[1] == 4 && g_RS232DataBuff[2] == 4)
                                        {
                                            buf_new_g_z_return_array[0] = g_RS232DataBuff[0];
                                            buf_new_g_z_return_array[1] = g_RS232DataBuff[1];
                                            buf_new_g_z_return_array[2] = g_RS232DataBuff[2];
                                            buf_new_g_z_return_array[3] = g_RS232DataBuff[3];
                                            buf_new_g_z_return_array[4] = g_RS232DataBuff[4];
                                            buf_new_g_z_return_array[5] = g_RS232DataBuff[5];
                                            buf_new_g_z_return_array[6] = g_RS232DataBuff[6];
                                            buf_new_g_z_return_array[7] = g_RS232DataBuff[7];
                                            buf_new_g_z_return_array[8] = g_RS232DataBuff[8];
                                            DealZReturn(buf_new_g_z_return_array);
                                        }
                                    }



                                    //int index = FindIndex(g_RS232DataBuff.ToArray(), value4_4);
                                    //if (index != -1)
                                    //{
                                    //    //index = buffer.FindLastIndex(o => o == 4);
                                    //    if (index != -1 && g_RS232DataBuff.Length >= index + 7)
                                    //    {
                                    //        if (g_RS232DataBuff[index] == 4 && g_RS232DataBuff[index + 1] == 4)
                                    //        {
                                    //            Console.Write("sss");
                                    //            buf_new_g_z_return_array[0] = g_RS232DataBuff[index - 1];
                                    //            buf_new_g_z_return_array[1] = g_RS232DataBuff[index - 0];
                                    //            buf_new_g_z_return_array[2] = g_RS232DataBuff[index + 1];
                                    //            buf_new_g_z_return_array[3] = g_RS232DataBuff[index + 2];
                                    //            buf_new_g_z_return_array[4] = g_RS232DataBuff[index + 3];
                                    //            buf_new_g_z_return_array[5] = g_RS232DataBuff[index + 4];
                                    //            buf_new_g_z_return_array[6] = g_RS232DataBuff[index + 5];
                                    //            buf_new_g_z_return_array[7] = g_RS232DataBuff[index + 6];
                                    //            buf_new_g_z_return_array[8] = g_RS232DataBuff[index + 7];
                                    //            //buf_new[0] = buffer[index - 2];
                                    //            //buf_new[1] = buffer[index - 1];
                                    //            //buf_new[2] = buffer[index - 0];
                                    //            //buf_new[3] = buffer[index + 1];
                                    //            //buf_new[4] = buffer[index + 2];
                                    //            //buf_new[5] = buffer[index + 3];
                                    //            //buf_new[6] = buffer[index + 4];
                                    //            //buf_new[7] = buffer[index + 5];
                                    //            //buf_new[8] = buffer[index + 6];
                                    //            if (buf_new_g_z_return_array.Length >= 7)
                                    //            {
                                    //                //处理Z返回
                                    //                DealZReturn(buf_new_g_z_return_array);
                                    //            }
                                    //        }
                                    //    }
                                    //}
                                }
                                catch (Exception ee)
                                {
                                    Console.WriteLine("异常。。。。。。。。。。。。");
                                    var sw1 = new System.IO.StreamWriter("Z轴电机收到指令异常.txt", true);
                                    sw1.WriteLine(
                                        DateTime.Now.ToString() + "\r\n"
                                         + "异常:" + "\r\n"
                                        + log_time + "\r\n"
                                        + ee.ToString() + "\r\n"
                                        + recev_data);
                                    sw1.Close();
                                }



                                //g_i_DataBuffLen += count;
                                //g_l_ReceivedCount += count;
                                ////g_Data += Encoding.ASCII.GetString(g_RS232DataBuff);

                                //foreach (byte b in g_RS232DataBuff)
                                //{
                                //    g_s_Data += Convert.ToChar(b);
                                //    //g_Data += Encoding.Unicode.GetString(g_RS232DataBuff);
                                //    //queue.Enqueue(g_RS232DataBuff);
                                //}

                            }
                            //if (count <= 0)
                            //    return;



                        } while (serialPortvalve_Send.BytesToRead > 0);

                        //处理接收后的命令
                        //TranslateCommand2();
                    }
                    catch (Exception ex)
                    {
                        Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);
                        //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                        var sw1 = new System.IO.StreamWriter("异常退出.txt", true);
                        sw1.WriteLine(
                            DateTime.Now.ToString() + "\r\n"
                             + "异常:" + "\r\n"
                            + "" + "\r\n"
                            + ex.Message.ToString());
                        sw1.Close();
                    }




                }

                Thread.Sleep(260);
            }
        }




        private void OnDataReceivedValve(object sender, SerialDataReceivedEventArgs e)
        {


            //try
            //{
            //    //接收数据
            //    do
            //    {
            //        int count = serialPortvalve_Send.BytesToRead;
            //        if (count <= 0)
            //            break;
            //        g_RS232DataBuff = new byte[count];
            //        serialPortvalve_Send.Read(g_RS232DataBuff, 0, count);
            //        //1.缓存数据           
            //        buffer.AddRange(g_RS232DataBuff);


            //        byte[] value4_4 = new byte[2];
            //        value4_4[0] = 4;
            //        value4_4[1] = 4;

            //        string recev_data = "";
            //        //拼成一个完整指令
            //        if (g_RS232DataBuff.Length > 1)
            //        {
            //            recev_data += string.Join(" ", g_RS232DataBuff);
            //        }
            //        else
            //        {
            //            recev_data += string.Join(" ", g_RS232DataBuff);
            //        }
            //        //byte[] recev_ds = StringToByteArray(buf.ToString());
            //        //byte[] recev_ds = StringToByteArray(recev_data);
            //        ////recev_data = string.Join(" ", recev_ds);
            //        ///
            //        string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
            //        Console.Write("Z轴电机收到指令 time:" + log_time + "收到指令：" + recev_data + "\r\n");


            //        //处理查询
            //        byte[] buf_new = new byte[9];
            //        try
            //        {
            //            int index = FindIndex(buffer.ToArray(), value4_4);
            //            index = buffer.FindLastIndex(o => o == 4);
            //            if (index != -1 && buffer.Count >= index + 7)
            //            {
            //                if (buffer[index] == 4 && buffer[index - 1] == 4)
            //                {
            //                    Console.Write("sss");
            //                    buf_new[0] = buffer[index - 2];
            //                    buf_new[1] = buffer[index - 1];
            //                    buf_new[2] = buffer[index - 0];
            //                    buf_new[3] = buffer[index + 1];
            //                    buf_new[4] = buffer[index + 2];
            //                    buf_new[5] = buffer[index + 3];
            //                    buf_new[6] = buffer[index + 4];
            //                    buf_new[7] = buffer[index + 5];
            //                    buf_new[8] = buffer[index + 6];
            //                    if (buf_new.Length >= 7)
            //                    {
            //                        //处理Z返回
            //                        DealZReturn(buf_new);
            //                    }
            //                }
            //            }
            //        }
            //        catch (Exception ee)
            //        {
            //            Console.WriteLine("异常。。。。。。。。。。。。");
            //        }



            //        //g_i_DataBuffLen += count;
            //        //g_l_ReceivedCount += count;
            //        ////g_Data += Encoding.ASCII.GetString(g_RS232DataBuff);

            //        //foreach (byte b in g_RS232DataBuff)
            //        //{
            //        //    g_s_Data += Convert.ToChar(b);
            //        //    //g_Data += Encoding.Unicode.GetString(g_RS232DataBuff);
            //        //    //queue.Enqueue(g_RS232DataBuff);
            //        //}


            //    } while (serialPortvalve_Send.BytesToRead > 0);

            //    //处理接收后的命令
            //    //TranslateCommand2();
            //}
            //catch (Exception ex)
            //{
            //    //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
            //}







            /*
                try
                {
                    if (is_receive_data_g)
                    {
                        if (a_idex==160)
                        {
                            serialPortvalve_Send.DiscardInBuffer();
                            serialPortvalve_Send.DiscardOutBuffer();
                            a_idex = 0;
                        }
                        a_idex++;



                        byte[] readBuffer = null;
                        int n = serialPortvalve_Send.BytesToRead;
                        byte[] buf = new byte[n];
                        serialPortvalve_Send.Read(buf, 0, n);




                        //1.缓存数据           
                        buffer.AddRange(buf);


                        byte[] value4_4 = new byte[2];
                        value4_4[0] = 4;
                        value4_4[1] = 4;

                        string recev_data = "";
                        //拼成一个完整指令
                        if (buf.Length > 1)
                        {
                            recev_data += string.Join(" ", buf);
                        }
                        else
                        {
                            recev_data += string.Join(" ", buf);
                        }
                        //byte[] recev_ds = StringToByteArray(buf.ToString());
                        //byte[] recev_ds = StringToByteArray(recev_data);
                        ////recev_data = string.Join(" ", recev_ds);
                        ///
                        string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】 ";
                        Console.Write("Z轴电机收到指令 time:" + log_time + "收到指令：" + recev_data + "\r\n");
                        //处理查询
                        byte[] buf_new = new byte[9];
                        try
                        {
                            int index = FindIndex(buffer.ToArray(), value4_4);
                            index = buffer.FindLastIndex(o => o == 4);
                            if (index != -1 && buffer.Count >= index + 7)
                            {
                                if (buffer[index] == 4 && buffer[index - 1] == 4)
                                {
                                    Console.Write("sss");
                                    buf_new[0] = buffer[index - 2];
                                    buf_new[1] = buffer[index - 1];
                                    buf_new[2] = buffer[index - 0];
                                    buf_new[3] = buffer[index + 1];
                                    buf_new[4] = buffer[index + 2];
                                    buf_new[5] = buffer[index + 3];
                                    buf_new[6] = buffer[index + 4];
                                    buf_new[7] = buffer[index + 5];
                                    buf_new[8] = buffer[index + 6];
                                    if (buf_new.Length >= 7)
                                    {
                                        //处理Z返回
                                        DealZReturn(buf_new);
                                    }
                                }
                            }
                        }
                        catch (Exception ee)
                        {
                            Console.WriteLine("异常。。。。。。。。。。。。");
                        }
                        //if (buf.Length >= 7)
                        //{
                        //    //处理Z返回
                        //    DealZReturn(buf);
                        //}
                    }
                }
                catch (Exception eee)
                {
                    var st = new StackTrace(eee, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("ExceptionDealZReturn.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "DealReceiveData:" + "\r\n"
                        + eee.Message + "\r\n"
                        + eee.InnerException + "\r\n"
                        + eee.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                }

                */
        }

        public byte[] FindFirst3Position(byte[] receive_data)
        {
            byte[] tt = new byte[1];
            switch (receive_data.Length)
            {
                case 11:
                    tt = receive_data.Skip(4).Take(4).ToArray();
                    break;
                case 10:
                    tt = receive_data.Skip(4).Take(3).ToArray();
                    break;
                case 9:
                    tt = receive_data.Skip(4).Take(2).ToArray();
                    break;
                case 8:
                    tt = receive_data.Skip(4).Take(1).ToArray();
                    break;
            }
            return tt;
        }
        public static List<GetOrderClsSimple> task_list_g;
        public static List<GetOrderClsSimple> task_list_new_g;
        public static List<GetOrderClsSimpleQuery> query_task_list = new List<GetOrderClsSimpleQuery>();


        /// <summary>
        /// 保存收到记录日志
        /// </summary>
        /// <param name="receive_data"></param>
        /// <param name="task_id"></param>
        /// <param name="thread_id"></param>
        /// <param name="set_value"></param>
        /// <param name="receive_value"></param>
        public void SaveReceiveLog(byte[] receive_data, int task_id, int thread_id, int set_value, int receive_value, int order_id)
        {
            string recev_data = "";
            string recev_data1 = "";
            //拼成一个完整指令
            if (receive_data.Length > 1)
            {
                recev_data += string.Join(" ", receive_data);
                recev_data1 += byteToHexStr(receive_data);
            }
            else
            {
                recev_data += string.Join(" ", receive_data);
                recev_data1 += byteToHexStr(receive_data);
            }
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";

            //Console.Write("-------------------------Time:" + log_time + "收到指令：" + recev_data + "\r\n");

            string receive = "Time:" + log_time + " 收到指令：" + recev_data1 + "\r\n";
            //Console.WriteLine(receive);
            FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
            {
                FrmPrintATCG.mainFrm.textBox2.AppendText(receive);
            }));

            LogHelper.WriteLog(typeof(FrmPrintATCG), "Time:" + log_time + " 收到指令：" + recev_data1 + " task_id:" + task_id + " thread_id:" + thread_id + " set_value:" + set_value + " receive_value:" + receive_value);

            if (!set_value.Equals(receive_value))
            {
                ReSendRunOrder(task_id, thread_id, set_value, receive_value, order_id);
            }
        }

        /// <summary>
        /// 重发脚本
        /// </summary>
        /// <param name="task_id"></param>
        /// <param name="thread_id"></param>
        /// <param name="set_value"></param>
        /// <param name="receive_value"></param>
        public void ReSendRunOrder(int task_id, int thread_id, int set_value, int receive_value, int order_id)
        {
            if (task_list_g.Count > 0)
            {
                not_equal_error_number++;
                if (not_equal_error_number == 50)
                {
                    Console.Write(".....................................................................\r\n");
                    GetOrderClsSimple error_order = task_list_g.Find(o => o.Status == 2 && o.Task_id.Equals(task_id + "") && o.Thread_id.Equals(thread_id + "") && o.Id.Equals(order_id));
                    if (error_order != null)
                    {
                        byte[] error_order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(error_order.Order_content);
                        SendCammand(error_order_content_byte_array, error_order.Order_content);
                        Thread.Sleep(120);
                        not_equal_error_number = 0;
                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                        {
                            FrmPrintATCG.mainFrm.textBox2.AppendText("重发\r\n");
                            LogHelper.WriteLog(typeof(FrmPrintATCG), "重发" + error_order.Order_content + "task_id" + task_id + "thread_id" + thread_id + "order_id" + order_id);
                            error_resend_number++;
                            if (error_resend_number == 5)
                            {
                                error_resend_number = 0;
                                not_equal_error_number = 0;
                                //电机异常，运行结束
                                Stop();
                                FrmPrintATCG.mainFrm.textBox2.AppendText("电机异常，运行结束\r\n");
                            }
                        }));
                    }

                }
            }
        }

        public static List<GetOrderClsSimple> thread0_orderlist_g = new List<GetOrderClsSimple>();
        public static List<GetOrderClsSimple> thread1_orderlist_g = new List<GetOrderClsSimple>();
        public static List<GetOrderClsSimple> thread2_orderlist_g = new List<GetOrderClsSimple>();

        //开始
        public static bool sub0_flage = true;
        public static bool sub1_flage = true;
        public static bool sub2_flage = true;

        //停止
        public static bool sub0_stop_flage = true;
        public static bool sub1_stop_flage = true;
        public static bool sub2_stop_flage = true;
        public static bool query_sub0_stop_flage = false;
        public static bool task_scheduling_stop_flage = false;

        /// <summary>
        /// 查询线程
        /// </summary>
        public static bool query_sub0_flage = true;
        public static bool query_sub1_flage = true;
        public static bool query_sub2_flage = true;

        //查询暂停
        public static bool query_sub0_pause_flage = false;
        public static bool query_sub1_pause_flage = true;
        public static bool query_sub2_pause_flage = true;

        //暂停
        public static bool sub0_pause_flage = false;
        public static bool sub1_pause_flage = true;
        public static bool sub2_pause_flage = true;


        public static bool sub01_pause_flage = false;
        public static bool sub11_pause_flage = false;
        public static bool sub21_pause_flage = false;

        //总调度
        public static bool task_scheduling_flage = true;
        public static bool task_scheduling_pause_flage = false;
        public static string task_id_g = "1";

        public static bool refresh_gridview_flage_g = false;
        public static List<string> task_id_list = new List<string>();

        public static List<int> task_id_number_list = new List<int>();
        /// <summary>
        /// 显示运行完成窗口
        /// </summary>
        public static bool show_run_finish_flage_g = true;
        /// <summary>
        /// 用于与查询结果做比较，如果数据相等，那移除发送队列中发送查询的指令，把自己重置为-1，并且把队列中发送的暂停标识改为false可以进行下一条指令的发送，
        /// </summary>
        //public static int valus_g = -1;
        //public static int valus1_g = -1;
        //public static int valus2_g = -1;

        public List<GetRecevieData> set_value_list_g = new List<GetRecevieData>();
        public static int send_order_number_g = 0;
        public void Stop()
        {
            query_task_list = new List<GetOrderClsSimpleQuery>();
            send_order_number_g = 0;
            query_task_list = new List<GetOrderClsSimpleQuery>();
            task_list_g = new List<GetOrderClsSimple>();
            task_list_new_g = new List<GetOrderClsSimple>();
            set_value_list_g = new List<GetRecevieData>();
            task_id_number_list = new List<int>();
            task_id_list = new List<string>();
            sub01_pause_flage = false;
            sub11_pause_flage = false;
            sub21_pause_flage = false;
            sub0_pause_flage = true;
            sub1_pause_flage = true;
            sub2_pause_flage = true;
            //先停止接收数据，查询进入暂停状态，并且停止接收数据
            is_receive_data_g = false;
            task_scheduling_pause_flage = true;
            query_sub0_pause_flage = true;
            ucBtnImg13.Enabled = false;
            ucBtnImg13.FillColor = Color.Gray;
            ucBtnImg15.Enabled = false;
            ucBtnImg15.FillColor = Color.Gray;
            ucBtnImg14.Enabled = false;
            ucBtnImg14.FillColor = Color.Gray;
            ucBtnImg7.Enabled = false;
            ucBtnImg7.FillColor = Color.Gray;
            ucBtnImg23.Enabled = true;
            //ucBtnImg24.Enabled = true;
            ucBtnImg23.FillColor = Color.Black;
            //ucBtnImg24.FillColor = Color.Black;
        }
        /// <summary>
        /// 得到要查询的值，如果为阀，那么值是目标孔，如果为泵，那么值为目标注液泵和行程
        /// </summary>
        /// <param name="order_content"></param>
        /// <param name="thread_id">0->query_value</param>
        /// <returns></returns>
        public int QueryValue(string order_content)
        {
            int value = -1;
            int device_id = -1;
            int hole = -1;
            //int speed = -1;
            int offset = -1;
            string[] order_sz_array_switch = order_content.Split(':');
            if (order_sz_array_switch.Length == 2)
            {
            }
            else
            {
                string[] order_sz_array = order_content.Split(',');
                if (order_sz_array[0].Length > 1)
                {
                    device_id = int.Parse(order_sz_array[0].Substring(1, order_sz_array[0].Length - 1));
                }
                if (order_sz_array[1].Length > 1)
                {
                    hole = int.Parse(order_sz_array[1].Substring(1, order_sz_array[1].Length - 1));
                }
                if (order_sz_array[3].Length > 1)
                {
                    offset = int.Parse(order_sz_array[3].Substring(1, order_sz_array[3].Length - 1));
                }
                //泵
                if (device_id > 2 && device_id < 21)
                {
                    //生成泵的查询指令
                    if (order_sz_array[2].Equals("V-1"))
                    {
                        value = hole;
                    }
                    else
                    {
                        value = offset;
                    }
                }
                //生成阀的查询指令
                //if (device_id > 2 && device_id < 5)
                if (device_id < 3)
                {
                    value = hole;
                }
            }
            return value;
        }
        public static FrmPrintATCG mainFrm;

        /// <summary>
        /// 多少次不相等就重发指令,如果收到的相等了，就把他置为0
        /// </summary>
        public static int not_equal_error_number = 0;
        /// <summary>
        ///  如果还一直不相等，重发多少次。
        /// </summary>
        public static int error_resend_number = 0;
        //查询任务完成
        public static bool thread1_query_finish = false;
        public static bool thread2_query_finish = false;


        //字节数组转16进制字符串
        /// <summary>
        /// 字节数组转16进制字符串
        /// </summary>
        /// <param name="bytes"></param>
        /// <returns></returns>
        public static string byteToHexStr(byte[] bytes)
        {
            string returnStr = "";
            if (bytes != null)
            {
                for (int i = 0; i < bytes.Length; i++)
                {
                    returnStr += bytes[i].ToString("X2") + " ";
                }
            }
            return returnStr;
        }
        /// <summary>
        /// 多少次不相等就重发指令,如果收到的相等了，就把他置为0
        /// </summary>
        //public static int not_equal_error_number = 0;
        /// <summary>
        ///  如果还一直不相等，重发多少次。
        /// </summary>
        //public static int error_resend_number = 0;
        /// <summary>
        /// 重发脚本
        /// </summary>
        /// <param name="task_id"></param>
        /// <param name="thread_id"></param>
        /// <param name="set_value"></param>
        /// <param name="receive_value"></param>
        //public void ReSendRunOrder(int task_id, int thread_id, int set_value, int receive_value, int order_id)
        //{
        //    if (task_list_g.Count > 0)
        //    {
        //        not_equal_error_number++;
        //        if (not_equal_error_number == 15)
        //        {
        //            not_equal_error_number = 0;
        //            Console.Write(".....................................................................\r\n");
        //            GetOrderClsSimple error_order = task_list_g.Find(o => o.Status == 2 && o.Task_id.Equals(task_id + "") && o.Thread_id.Equals(thread_id + "") && o.Id.Equals(order_id));
        //            if (error_order != null)
        //            {
        //                byte[] error_order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(error_order.Order_content);
        //                SendCammand(error_order_content_byte_array, error_order.Order_content);
        //                Thread.Sleep(120);

        //                FormMain.mainFrm.Invoke((EventHandler)(delegate
        //                {
        //                    textBox2.AppendText("重发\r\n");
        //                    LogHelper.WriteLog(typeof(FormMain), "重发" + error_order.Order_content + "task_id" + task_id + "thread_id" + thread_id + "order_id" + order_id);
        //                    error_resend_number++;
        //                    if (error_resend_number == 5)
        //                    {
        //                        error_resend_number = 0;
        //                        not_equal_error_number = 0;
        //                        //电机异常，运行结束
        //                        Stop();
        //                        textBox2.AppendText("电机异常，运行结束\r\n");
        //                    }
        //                }));
        //            }
        //        }
        //    }
        //}
        /// <summary>
        /// 保存发送指令
        /// </summary>
        /// <param name="buf_array"></param>
        /// <param name="order_content"></param>
        public static void SaveSendOrderLog(byte[] buf_array, string order_content)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            //Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + "-" + order_content + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            send_log = "Time:" + log_time + " 发送指令：" + byteToHexStr(buf_array) + "-" + order_content + " 序号：" + send_order_number_g + "";
            LogHelper.WriteLog(typeof(FrmPrintATCG), send_log);
        }
        public static void SendCammand(byte[] buf_array, string order_content)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            //Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            send_log = "Time:" + log_time + "发送指令：" + byteToHexStr(buf_array) + " 序号：" + send_order_number_g + "";

            //LogHelper.WriteLog(typeof(FormMain), send_log);
            FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
            {
                SaveSendOrderLog(buf_array, order_content);
                FrmPrintATCG.mainFrm.textBox2.AppendText(send_log + "\r\n");
            }));

            //var sw = new System.IO.StreamWriter("RunLog.txt", true);
            //sw.WriteLine(
            //    send_log
            //     + "" + ""
            //    + "");
            //sw.Close();
            send_order_number_g++;
            string[] order_content_array = order_content.Split(':');
            //判断是否有开关指令
            if (order_content_array.Length == 2 && order_content_array[0].Equals("Switch"))
            {
                serialPortvalve_Send.Write(buf_array, 0, buf_array.Length);
            }
            else
            {
                if (order_content_array.Length != 2)
                {
                    serialPort_Send.Write(buf_array, 0, buf_array.Length);
                }
            }
        }

        public static void SendCammandSwitch(byte[] buf_array)
        {
            string log_time = "【" + DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss fff") + "】";
            Console.Write("Time:" + log_time + "发送指令：" + string.Join(" ", buf_array) + " 序号：" + send_order_number_g + "\r\n");
            string send_log = "";
            send_log = "Time:" + log_time + "发送指令：" + byteToHexStr(buf_array) + " 序号：" + send_order_number_g + "";

            LogHelper.WriteLog(typeof(FrmPrintATCG), send_log);

            FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
            {
                FrmPrintATCG.mainFrm.textBox2.AppendText(send_log + "\r\n");
            }));

            //var sw = new System.IO.StreamWriter("RunLog.txt", true);
            //sw.WriteLine(
            //    send_log
            //     + "" + ""
            //    + "");
            //sw.Close();
            send_order_number_g++;
            serialPortvalve_Send.Write(buf_array, 0, buf_array.Length);
        }

        public void SendCammandString(string buf)
        {
            byte[] decBytes = System.Text.Encoding.UTF8.GetBytes(buf);
            SendCammand(decBytes, "");
        }
        /// <summary>
        /// 两个字转成int 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int bytesToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8));
            //| ((src[offset + 2] & 0xFF) << 16)
            //| ((src[offset + 3] & 0xFF) << 24));
            return value;
        }
        /// <summary>
        /// 4个字转成int 
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public int bytes4ToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8))
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24);
            return value;
        }
        /// <summary>
        /// byte 数据转成 int 值
        /// </summary>
        /// <param name="src"></param>
        /// <param name="offset"></param>
        /// <returns></returns>
        public static int bytes44ToInt(byte[] src, int offset)
        {
            int value;
            value = (int)((src[offset] & 0xFF)
                    | ((src[offset + 1] & 0xFF) << 8)
                    | ((src[offset + 2] & 0xFF) << 16)
                    | ((src[offset + 3] & 0xFF) << 24));

            int value1 = (src[3] & 0xFF);
            int value2 = ((src[2] & 0xFF) << 8);
            int value3 = ((src[1] & 0xFF) << 16);
            int value4 = ((src[0] & 0xFF) << 24);
            value = (value3 >> 16) | (value4 >> 16 & 0xff00) | (value1 << 16) | (value2 << 16);
            return value;
        }
        /// <summary>
        /// 查询阀当前孔位置
        /// </summary>
        /// <param name="receive_data"></param>
        public void QueryValveHole(byte[] receive_data)
        {
            //
            //CC 02 00 05 00 DD B0 01 
            //CC 02 00 01 08 DD B4 01 
            if (receive_data.Length == 8)
            {
                //孔1 复位后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x01 && receive_data[3] == 0x01 && receive_data[4] == 0x08 && receive_data[5] == 0xDD)
                {
                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                    {
                        //comboBox_Hole.SelectedItem = 8 + "";
                    }));
                }

                //孔1 运动后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x01 && receive_data[4] == 0x00 && receive_data[5] == 0xDD)
                {
                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                    {
                        string hole = "" + receive_data[3];
                        //comboBox_Hole.SelectedItem = hole;
                    }));
                }

                //孔2 复位后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x02 && receive_data[3] == 0x01 && receive_data[4] == 0x08 && receive_data[5] == 0xDD)
                {
                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                    {
                        //comboBox_Hole1.SelectedItem = 8 + "";
                    }));
                }


                //孔2 运动后的
                if (receive_data[0] == 0xCC && receive_data[1] == 0x02 && receive_data[4] == 0x00 && receive_data[5] == 0xDD)
                {
                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                    {
                        string hole = "" + receive_data[3];
                        //comboBox_Hole1.SelectedItem = hole;
                    }));
                }
            }
        }

        private void ucBtnImg25_BtnClick(object sender, EventArgs e)
        {
            int a = int.Parse(textBox_A_Offset.Text);
            if (checkBox1.Checked)
            {
                SendZAxismotorMove(1, a);
            }
            Thread.Sleep(150);
            if (checkBox2.Checked)
            {
                SendZAxismotorMove(2, a);
            }
            Thread.Sleep(150);
            if (checkBox3.Checked)
            {
                SendZAxismotorMove(3, a);
            }
        }
        /// <summary>
        /// Z轴电机找零
        /// </summary>
        /// <param name="device_id"></param>
        private void SendZAxisMotorFindZero(byte device_id)
        {
            byte[] order_content_byte_array = new byte[8];
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Find_Zero(device_id, 1, 0, 0, 0x0601);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + 1000);
        }
        /// <summary>
        /// Z轴电机位移，绝对位置
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="offset"></param>
        private void SendZAxismotorMove(byte device_id, int offset)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = offset;// int.Parse(textBox_A_Offset.Text);
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH10(device_id, 2, 0, 1, 0x0300, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg36_BtnClick(object sender, EventArgs e)
        {

            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBox1.Checked)
            {
                SendZAxisMotorFindZero(1);
            }
            Thread.Sleep(150);
            if (checkBox2.Checked)
            {
                SendZAxisMotorFindZero(2);
            }
            Thread.Sleep(150);
            if (checkBox3.Checked)
            {
                SendZAxisMotorFindZero(3);
            }
        }
        public void ZAxisFindeZero()
        {
            if (!serialPortvalve_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBox1.Checked)
            {
                SendZAxisMotorFindZero(1);
            }
            Thread.Sleep(150);
            if (checkBox2.Checked)
            {
                SendZAxisMotorFindZero(2);
            }
            Thread.Sleep(150);
            if (checkBox3.Checked)
            {
                SendZAxisMotorFindZero(3);
            }
        }

        private void ucBtnImg37_BtnClick(object sender, EventArgs e)
        {
            byte[] order_content_byte_array = new byte[8];
            float a = float.Parse(textBox_A_Offset.Text);
            //添加开动作
            //01 10 00 03   00 02   04  40 00 46 1C   95 D3

            //01 06 00 03 72 10 5C A6

            //Tx:057-01 06 00 03 7D 00 58 9A
            //Tx:072-01 06 00 05 7D 00 B8 9B
            //          01 06 00 00 06 01 4B AA
            //Tx: 329 - 01 10 00 03 00 02 04 00 00 47 7A 01 A9
            //Tx:291-   01 10 00 03   00 02     04   00 00 47 7A   01 A9
            //          01 10 00 01   00 02     04   88 B8 00 00   99 E6  
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH10(1, 2, 0, 1, 0x0300, 4, a);//填充写单个线圈寄存器//CoilOFFAndON(33, a);

            //Tx: 379 - 01 10 00 03 00 02 04  00 00 45 C8  80 BC   //6400
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 3, 0x0300, 4, a);//
            SendCammand(order_content_byte_array, "Switch:" + a);
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(2, 2, 0, 3, 0x0300, 4, a);//
            SendCammand(order_content_byte_array, "Switch:" + a);
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(3, 2, 0, 3, 0x0300, 4, a);//
            SendCammand(order_content_byte_array, "Switch:" + a);

            //Thread.Sleep(100);
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 5, 0x0300, 4, a * 10);//
            //SendCammand(order_content_byte_array, "Switch:" + a);

            //Thread.Sleep(100);
            //order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_Speed_Set(1, 2, 0, 7, 0x0300, 4, a * 10);//
            //SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg34_BtnClick(object sender, EventArgs e)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = 1000;
            //添加开动作
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_1(1, 1, 0, 0, 0x0101);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            //Tx:01 06 00 00 01 01 49 9A

            //Tx:084-01 06 00 00 01 01 49 9A

            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg35_BtnClick(object sender, EventArgs e)
        {
            byte[] order_content_byte_array = new byte[8];
            int a = 1000;
            //添加开动作
            order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH06_1(1, 1, 0, 0, 0x0400);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }
        public void PumpReset()
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                ReSetPump(0x04);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump3.Checked)
            {
                ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump4.Checked)
            {
                ReSetPump(0x06);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump5.Checked)
            {
                ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump6.Checked)
            {
                ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump7.Checked)
            {
                ReSetPump(0x9);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump8.Checked)
            {
                ReSetPump(16);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump9.Checked)
            {
                ReSetPump(17);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump10.Checked)
            {
                ReSetPump(18);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump11.Checked)
            {
                ReSetPump(19);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump12.Checked)
            {
                ReSetPump(20);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBox_Valve1Reset.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x45, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1, "");
            }

            Thread.Sleep(100);
            if (checkBox_Valve2Reset.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1, "");
            }
        }
        private void ucBtnImg26_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                ReSetPump(0x04);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump3.Checked)
            {
                ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump4.Checked)
            {
                ReSetPump(0x06);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump5.Checked)
            {
                ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump6.Checked)
            {
                ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump7.Checked)
            {
                ReSetPump(0x9);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump8.Checked)
            {
                ReSetPump(16);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump9.Checked)
            {
                ReSetPump(17);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump10.Checked)
            {
                ReSetPump(18);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump11.Checked)
            {
                ReSetPump(19);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBoxPump12.Checked)
            {
                ReSetPump(20);
                Thread.Sleep(send_delay_time_g);
                //string offset_value = textBox_A_Offset.Text;
                //LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }
            if (checkBox_Valve1Reset.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x45, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1, "");
            }

            Thread.Sleep(100);
            if (checkBox_Valve2Reset.Checked)
            {
                byte[] package_content1 = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
                byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
                SendCammand(package_content_crc1, "");
            }
        }
        /// <summary>
        /// 复位
        /// </summary>
        /// <param name="device_id"></param>
        public void ReSetPump(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x45, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        private void ucBtnImg30_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            ReSetPump(0x03);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x04);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x05);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x06);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x07);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x08);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(0x9);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(16);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(17);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(18);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(19);
            Thread.Sleep(send_delay_time_g);

            ReSetPump(20);
            Thread.Sleep(send_delay_time_g);

            byte[] package_content1 = new byte[] { 0xcc, 0x01, 0x45, 0x00, 0x00, 0xdd };
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
            Thread.Sleep(send_delay_time_g);

            byte[] package_content2 = new byte[] { 0xcc, 0x02, 0x45, 0x00, 0x00, 0xdd };
            byte[] package_content_crc2 = CRC.GetNewCrcArray(package_content2);
            SendCammand(package_content_crc2, "");
        }

        private void ucBtnImg29_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                //ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                //ReSetPump(0x4);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x04);
            }

            if (checkBoxPump3.Checked)
            {
                //ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x05);
            }

            if (checkBoxPump4.Checked)
            {
                //ReSetPump(0x6);
                Thread.Sleep(120);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x06);
            }
            if (checkBoxPump5.Checked)
            {
                //ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x07);
            }

            if (checkBoxPump6.Checked)
            {
                //ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x08);
            }
            if (checkBoxPump7.Checked)
            {
                //ReSetPump(0x09);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 0x09);
            }

            if (checkBoxPump8.Checked)
            {
                //ReSetPump(0x10);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 16);
            }

            if (checkBoxPump9.Checked)
            {
                //ReSetPump(0x11);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 17);
            }

            if (checkBoxPump10.Checked)
            {
                //ReSetPump(0x12);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 18);
            }

            if (checkBoxPump11.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 19);
            }

            if (checkBoxPump12.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                LiquidOut_Or_In(int.Parse(offset_value), 20);
            }
        }

        /// <summary>
        /// 注液
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="device_id"></param>
        /// <param name="order_id"></param>
        public void LiquidOut_Or_In(int offset, int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4E, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] tmp_array = ConvertHex2(offset);
            package_content1[3] = tmp_array[0];
            package_content1[4] = tmp_array[1];
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        public void SetPumpSpeed(int speed, int device_id)
        {
            //byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4E, 0x00, 0x00, 0xdd };
            byte[] package_content1 = new byte[] { 0xCC, 0x13, 0x07, 0xFF, 0xEE, 0xBB, 0xAA, 0x64, 0x00, 0x00, 0x00, 0xDD };
            package_content1[1] = (byte)device_id;
            byte[] tmp_array = ConvertHex2(speed);
            package_content1[7] = tmp_array[0];
            package_content1[8] = tmp_array[1];
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        /// <summary>
        /// 吸液
        /// </summary>
        /// <param name="offset"></param>
        /// <param name="device_id"></param>
        public void LiquidIn(int offset, int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x4d, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }

        private byte[] ConvertHex2(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[2];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            //hex[2] = (byte)((velocity >> 16) & 0xff);
            //hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

        private StringBuilder ConvertHex1(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[4];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);
            hex[2] = (byte)((velocity >> 16) & 0xff);
            hex[3] = (byte)((velocity >> 24) & 0xff);
            StringBuilder tmp = new StringBuilder();
            for (int i = 0; i < hex.Length - 1; i++)
            {
                tmp.Append(hex[i].ToString("x2"));  //转为16进制，当只有一个字符时补0
            }
            return tmp;
        }

        public static byte[] ConvertHex4_1(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[4];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            hex[2] = (byte)((velocity >> 16) & 0xff);
            hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

        private byte[] ConvertHex4(int vel)
        {
            int velocity = vel;
            byte[] hex = new byte[4];
            hex[0] = (byte)(velocity & 0xff);
            hex[1] = (byte)((velocity >> 8) & 0xff);   //先右移再与操作
            hex[2] = (byte)((velocity >> 16) & 0xff);
            hex[3] = (byte)((velocity >> 24) & 0xff);
            return hex;
        }

        private void ucBtnImg31_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            Thread.Sleep(send_delay_time_g);
            string offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x03);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x04);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x05);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x06);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x07);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x08);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 0x09);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 16);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 17);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 18);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 19);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            LiquidOut_Or_In(int.Parse(offset_value), 20);
        }

        private void ucBtnImg28_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            if (checkBoxPump1.Checked)
            {
                //ReSetPump(0x03);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x03);
            }

            if (checkBoxPump2.Checked)
            {
                //ReSetPump(0x4);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x04);
            }

            if (checkBoxPump3.Checked)
            {
                //ReSetPump(0x05);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x05);
            }

            if (checkBoxPump4.Checked)
            {
                //ReSetPump(0x6);
                Thread.Sleep(120);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x06);
            }
            if (checkBoxPump5.Checked)
            {
                //ReSetPump(0x07);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x07);
            }

            if (checkBoxPump6.Checked)
            {
                //ReSetPump(0x08);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x08);
            }
            if (checkBoxPump7.Checked)
            {
                //ReSetPump(0x09);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 0x09);
            }

            if (checkBoxPump8.Checked)
            {
                //ReSetPump(0x10);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 16);
            }

            if (checkBoxPump9.Checked)
            {
                //ReSetPump(0x11);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 17);
            }

            if (checkBoxPump10.Checked)
            {
                //ReSetPump(0x12);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 18);
            }

            if (checkBoxPump11.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 19);
            }

            if (checkBoxPump12.Checked)
            {
                //ReSetPump(0x13);
                Thread.Sleep(send_delay_time_g);
                string offset_value = textBox_A_Offset.Text;
                SetPumpSpeed(int.Parse(offset_value), 20);
            }
        }
        private void ucBtnImg32_BtnClick(object sender, EventArgs e)
        {
            if (!serialPort_Send.IsOpen)
            {
                //MessageBox.Show("请连接设备");
                FrmDialog.ShowDialog(this, "请连接设备", "提示");
                return;
            }

            Thread.Sleep(send_delay_time_g);
            string offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x03);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x04);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x05);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x06);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x07);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x08);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 0x09);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 16);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 17);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 18);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 19);

            Thread.Sleep(send_delay_time_g);
            offset_value = textBox_A_Offset.Text;
            SetPumpSpeed(int.Parse(offset_value), 20);
        }

        public static byte[] QueryContentTrasforByteOrder(string Order_content)
        {
            byte[] order_content_byte_array = new byte[8];
            string order_sz = Order_content;
            int device_id = -1;
            int hole = -1;
            int speed = -1;
            int offset = -1;
            if (order_sz.Length > 0)
            {
                string[] order_sz_array_switch = order_sz.Split(':');
                if (order_sz_array_switch.Length == 2)
                {

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
                    //泵
                    if (device_id > 2 && device_id < 21)
                    {
                        //pump
                        order_content_byte_array = QueryPumpStatusOrder(device_id);
                    }
                    //CC 08 66 00 00 DD 17 02 
                    //生成阀的查询指令
                    //if (device_id > 2 && device_id < 5)
                    if (device_id < 3)
                    {
                        order_content_byte_array = QueryValveStatusOrder(device_id);
                    }
                }
            }
            return order_content_byte_array;
        }

        /// <summary>
        /// 查询阀状态
        /// </summary>
        /// <param name="device_id"></param>
        /// <param name="target_hole"></param>
        /// <returns></returns>
        public static byte[] QueryValveStatusOrder(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x3E, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            return package_content_crc1;
        }

        /// <summary>
        /// 查询泵状态
        /// </summary>
        /// <param name="device_id"></param>
        /// <returns></returns>
        public static byte[] QueryPumpStatusOrder(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x66, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            return package_content_crc1;
        }

        /// <summary>
        /// 查询泵状态
        /// </summary>
        /// <param name="device_id"></param>
        /// <returns></returns>
        public static byte[] QueryPumpStatusOrder6(int device_id)
        {
            string s = string.Format("/{0:D}", device_id);
            s += "?6\r";
            byte[] buf = Encoding.ASCII.GetBytes(s);
            //查询时不需要加   "\r"; 
            //string aa = "/1O3V1000A3000";
            //aa = "/1Z";
            //aa += "R\r";

            //string aa = "/1O3V1000A3000";
            //aa = "/1Z";
            //aa += "R\r";
            //byte[] buf = Encoding.ASCII.GetBytes(aa);
            //SendCammand(buf);
            return buf;
        }
        /// <summary>
        /// 初始化泵 /1ZR
        /// </summary>
        /// <param name="device_id"></param>
        public void InitializePump(int device_id)
        {
            string s = string.Format("/{0:D}", device_id);
            s += "ZR\r";
            //复位时,泵1目标孔位在第6
            //if (device_id == 1)
            //{
            // s += "Z0,2,3R\r";
            //}
            ////复位时,泵2目标孔位在第6
            //if (device_id == 2)
            //{
            //    s += "Z0,5,6R\r";
            //}
            byte[] buf = Encoding.ASCII.GetBytes(s);
            //查询时不需要加   "\r"; 
            //string aa = "/1O3V1000A3000";
            //aa = "/1Z";
            //aa += "R\r";
            SendCammand(buf, "");
        }

        public void InitializeValve(int device_id)
        {
            byte[] package_content1 = new byte[] { 0xcc, 0x03, 0x45, 0x00, 0x00, 0xdd };
            package_content1[1] = (byte)device_id;
            byte[] package_content_crc1 = CRC.GetNewCrcArray(package_content1);
            SendCammand(package_content_crc1, "");
        }
        public void RefreshUiInfo()
        {
            while (true)
            {
                try
                {
                    if (task_list_g != null)
                    {
                        if (task_list_g.Count > 0)
                        {
                            FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                            {
                                int total_number = task_list_g.Count;
                                this.progressBar1.Maximum = total_number;//设置进度条最大值
                                int finish_number = task_list_g.FindAll(o => o.Status == 3).Count;
                                this.progressBar1.Value = finish_number;//设置进度条当前值
                                current_time_g = DateTime.Now.ToString("yyyy-MM-dd hh:mm:ss");
                                if (!start_time_g.Equals("") && !current_time_g.Equals(""))
                                {
                                    DateTime dt1 = Convert.ToDateTime(start_time_g);
                                    DateTime dt2 = Convert.ToDateTime(current_time_g);
                                    string time_run = "";
                                    time_run = ExecDateDiff(dt1, dt2);

                                    label59.Text = "运行时间：" + time_run;
                                }

                                if (finish_number == total_number)
                                {
                                    if (show_run_finish_flage_g)
                                    {
                                        //FrmDialog.ShowDialog(this, "运行完成", "提示");
                                        show_run_finish_flage_g = false;
                                        start_time_g = "";
                                    }
                                }
                                label58.Text = "共" + total_number + "个脚本，当前：" + finish_number;
                                Console.Write("total_number:" + total_number + " finish_number:" + finish_number + "\r\n");
                            }));


                        }
                    }
                    while (refresh_gridview_flage_g)
                    {
                        FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                        {
                            //UpdateListUi();
                        }));
                        //refresh_gridview_flage_g = false;
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(500);
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("刷新界面异常.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "异常:" + "\r\n"
                        + ex.Message + "\r\n"
                        + "read_sx:" + read_sx + "\r\n"
                        + ex.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                    //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 程序执行时间测试
        /// </summary>
        /// <param name="dateBegin">开始时间</param>
        /// <param name="dateEnd">结束时间</param>
        /// <returns>返回(秒)单位，比如: 0.00239秒</returns>
        public static string ExecDateDiff(DateTime dateBegin, DateTime dateEnd)
        {
            TimeSpan ts1 = new TimeSpan(dateBegin.Ticks);
            TimeSpan ts2 = new TimeSpan(dateEnd.Ticks);
            TimeSpan ts3 = ts1.Subtract(ts2).Duration();
            //你想转的格式
            return ts3.ToString("c").Substring(0, 8);//   00:00:07
            //return ts3.TotalMilliseconds.ToString();
        }
        public void TaskScheduling()
        {
            while (task_scheduling_flage)
            {
                try
                {
                    //while (task_scheduling_stop_flage)
                    //{
                    if (task_list_g != null)
                    {
                        //暂停
                        while (task_scheduling_pause_flage)
                        {
                            Thread.Sleep(1);
                        }
                    A:
                        //分组循环,一组组的发送
                        //string task_id = task_id_list[0];//task_id_list[0].Keys.Task_id;
                        if (task_id_list.Count > 0)
                        {
                            task_id_g = task_id_list[0];//task_id_list[0].Keys.Task_id;
                        }
                        for (int jj = 0; jj < task_list_g.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            string id = task_list_g[jj].Task_id;
                            string status = task_list_g[jj].Status + "";
                            if (task_id_g.Equals(id) && status.Equals("0"))
                            {
                                task_list_g[jj].Status = 1;
                                //sub0_pause_flage = false;
                            }
                        }
                        for (int jjj = 0; jjj < task_id_number_list.Count; jjj++)
                        {
                            string task_id_sz = task_id_list[jjj];
                            int task_id_number = task_id_number_list[jjj];
                            int task_status3_number = task_list_g.FindAll(o => o.Task_id.Equals(task_id_sz) && o.Status == 3).Count;
                            int task_status1_number = task_list_g.FindAll(o => o.Task_id.Equals(task_id_sz) && o.Status == 1).Count;
                            if (task_status3_number == task_id_number)
                            {
                                if (task_id_list.Count > 0)
                                {
                                    sub1_pause_flage = true;
                                    sub2_pause_flage = true;
                                    sub0_pause_flage = true;
                                    task_id_list.Remove(task_id_list[0]);
                                    task_id_number_list.Remove(task_id_number_list[0]);
                                    goto A;
                                }
                            }
                            //第二个任务开始时
                            if (task_status1_number == task_id_number)
                            {
                                //第一个任务开始为0
                                if (!task_id_sz.Equals("0"))
                                {
                                    if (query_task_list.Count == 0)
                                    {
                                        sub1_pause_flage = true;
                                        sub2_pause_flage = true;
                                        sub0_pause_flage = false;
                                    }
                                }
                            }
                        }
                        //先计算出为这个队列中
                        //Console.Write("sub2_flage");
                        Thread.Sleep(1000);
                    }
                    Thread.Sleep(20);
                    //}
                }
                catch (Exception ex)
                {
                    Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                    var st = new StackTrace(ex, true);
                    var frame = st.GetFrame(0);
                    var line = frame.GetFileLineNumber();
                    var sw = new System.IO.StreamWriter("调度异常.txt", true);
                    sw.WriteLine(
                        DateTime.Now.ToString() + "\r\n"
                         + "异常:" + "\r\n"
                        + ex.Message + "\r\n"
                        + "read_sx:" + read_sx + "\r\n"
                        + ex.Source + "\r\n"
                        + frame + "\r\n"
                        + line);
                    sw.Close();
                    //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                }
            }
        }
        /// <summary>
        /// 线程0中当前状态为1的脚本数量
        /// </summary>
        public static int number_current_thread0_status1 = 0;
        public void SendOrderSub0()
        {
            while (sub0_flage)
            {
                if (task_list_g != null)
                {
                    while (sub01_pause_flage)
                    {
                        try
                        {
                            List<GetOrderClsSimple> tmp_list = task_list_g.FindAll(o => o.Status == 1);
                            for (int jj = 0; jj < tmp_list.Count; jj++)
                            {
                                //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                                int pkid = tmp_list[jj].Id;
                                string id = tmp_list[jj].Task_id;
                                string status = tmp_list[jj].Status + "";
                                string thread_id = tmp_list[jj].Thread_id;
                                //暂停
                                while (sub0_pause_flage)
                                {
                                    Thread.Sleep(1);
                                }
                                if (task_list_g.Count > 0)
                                {
                                    //算出总数
                                    if (number_current_thread0_status1 == 0)
                                    {
                                        number_current_thread0_status1 = tmp_list.FindAll(o => o.Status.Equals(1) && o.Thread_id.Equals("0")).Count;
                                    }
                                    //一开始统计一下当前线程，并且状态为1的指令一共有多少条
                                    int total_status_3 = tmp_list.FindAll(o => o.Status.Equals(3) && o.Thread_id.Equals("0")).Count;
                                    if (number_current_thread0_status1 == total_status_3)
                                    {
                                        Console.Write("--------------------------------------------------------------------" + "\r\n");
                                        sub1_pause_flage = false;
                                        Thread.Sleep(120);
                                        sub2_pause_flage = false;
                                        number_current_thread0_status1 = 0;
                                        sub0_pause_flage = true;
                                    }
                                    else
                                    {
                                        Console.Write("total_status_1:" + number_current_thread0_status1 + "total_status_3" + total_status_3 + "\r\n");
                                    }
                                    if (status.Equals("1") && thread_id.Equals("0"))
                                    {
                                        //todo
                                        byte[] order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(tmp_list[jj].Order_content);
                                        SendCammand(order_content_byte_array, tmp_list[jj].Order_content);
                                        //task_list[jj].Status = 3;
                                        //发送指令
                                        //task_list[jj].Status = 2;
                                        if (task_list_g.Count > 0)
                                        {
                                            var model_run2 = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                            model_run2.Status = 2;
                                        }
                                        if (JudgeSwitchOrDelay(tmp_list[jj].Order_content))
                                        {
                                            if (task_list_g.Count > 0)
                                            {
                                                var model_run = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                                model_run.Status = 3;
                                            }
                                            int delay_time = GetDelayTime(tmp_list[jj].Order_content);
                                            if (delay_time > 0)
                                            {
                                                Thread.Sleep(delay_time);
                                            }
                                        }
                                        //如果有开关，有延时的指令那么不用等待返回就可以继续发一下条指令
                                        string[] order_sz_array_switch = tmp_list[jj].Order_content.Split(':');
                                        if (order_sz_array_switch.Length == 2)
                                        {
                                            Thread.Sleep(50);
                                        }
                                        else
                                        {
                                            //Thread.Sleep(200);
                                            AddQueryOrder(tmp_list[jj]);
                                            sub0_pause_flage = true;
                                        }
                                        //如果下一条指令为并行，那么当前指令执行完成后，暂停当前线程，，跳到并发线程1和并发线程2，在线程1中查找对应的指令下发，在线程2中去列表查找对应的指令下发，所有
                                        //并发指令都执行完成后，查看队列中状态为1，是否还有别的指令未发送，如果为串行，则去掉本方法的暂停标识，继续发送并行后的串行命令
                                        //如果当前串行指令执行完成了。
                                        Thread.Sleep(10);
                                    }
                                }
                            }
                            Thread.Sleep(100);
                            Thread.Sleep(120);
                        }
                        catch (Exception ex)
                        {
                            Console.WriteLine("error:接收返回消息异常。详细原因：" + ex.Message);

                            var st = new StackTrace(ex, true);
                            var frame = st.GetFrame(0);
                            var line = frame.GetFileLineNumber();
                            var sw = new System.IO.StreamWriter("串行异常.txt", true);
                            sw.WriteLine(
                                DateTime.Now.ToString() + "\r\n"
                                 + "异常:" + "\r\n"
                                + ex.Message + "\r\n"
                                + "read_sx:" + read_sx + "\r\n"
                                + ex.Source + "\r\n"
                                + frame + "\r\n"
                                + line);
                            sw.Close();
                            //LogClass.WriteFile("error:接收返回消息异常。详细原因：" + ex.Message);
                        }
                    }


                }

            }
        }
        /// <summary>
        /// 得到延时时间
        /// </summary>
        /// <param name="sz"></param>
        /// <returns></returns>
        public int GetDelayTime(string sz)
        {
            int result = 0;
            string[] sz_array = sz.Split(':');
            if (sz_array.Length == 2)
            {
                if (sz_array[0].Equals("iDelay"))
                {
                    result = int.Parse(sz_array[1]);
                }
            }
            return result;
        }
        //判断是否为开关或是延时，如果是的话不需要查询指令
        public bool JudgeSwitchOrDelay(string sz)
        {
            bool result = false;
            string[] sz_array = sz.Split(':');
            if (sz_array.Length == 2)
            {
                result = true;
            }
            return result;
        }

        public void SendOrderSub1()
        {
            while (sub1_flage)
            {
                if (task_list_g != null)
                {

                    while (sub11_pause_flage)
                    {


                        List<GetOrderClsSimple> tmp_list = task_list_g.FindAll(o => o.Status == 1 && o.Thread_id.Equals("1"));
                        for (int jj = 0; jj < tmp_list.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            int pkid = tmp_list[jj].Id;
                            string id = tmp_list[jj].Task_id;
                            string status = tmp_list[jj].Status + "";
                            string thread_id = tmp_list[jj].Thread_id;
                            //暂停
                            while (sub1_pause_flage)
                            {
                                Thread.Sleep(1);
                            }

                            if (task_list_g.Count > 0)
                            {
                                if (status.Equals("1") && thread_id.Equals("1"))
                                {
                                    //todo
                                    //如果是泵的指令，要先查询孔位是不是已经
                                    byte[] order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(tmp_list[jj].Order_content);
                                    SendCammand(order_content_byte_array, tmp_list[jj].Order_content);
                                    //task_list[jj].Status = 3;
                                    //发送指令
                                    //task_list[jj].Status = 2;
                                    if (task_list_g.Count > 0)
                                    {
                                        var model_run2 = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                        model_run2.Status = 2;

                                    }
                                    if (JudgeSwitchOrDelay(tmp_list[jj].Order_content))
                                    {
                                        if (task_list_g.Count > 0)
                                        {
                                            var model_run = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                            model_run.Status = 3;
                                        }
                                        int delay_time = GetDelayTime(tmp_list[jj].Order_content);
                                        if (delay_time > 0)
                                        {
                                            Thread.Sleep(delay_time);
                                        }
                                    }

                                    //如果有开关，有延时的指令那么不用等待返回就可以继续发一下条指令
                                    string[] order_sz_array_switch = tmp_list[jj].Order_content.Split(':');
                                    if (order_sz_array_switch.Length == 2)
                                    {
                                        Thread.Sleep(50);
                                    }
                                    else
                                    {
                                        //Thread.Sleep(200);
                                        AddQueryOrder(tmp_list[jj]);
                                        sub1_pause_flage = true;
                                    }

                                    //如果下一条指令为并行，那么当前指令执行完成后，暂停当前线程，，跳到并发线程1和并发线程2，在线程1中查找对应的指令下发，在线程2中去列表查找对应的指令下发，所有
                                    //并发指令都执行完成后，查看队列中状态为1，是否还有别的指令未发送，如果为串行，则去掉本方法的暂停标识，继续发送并行后的串行命令
                                    //如果当前串行指令执行完成了。
                                    Thread.Sleep(50);
                                }
                            }
                        }

                        Thread.Sleep(120);

                    }

                }


            }
        }
        public void SendOrderSub2()
        {
            while (sub2_flage)
            {
                if (task_list_g != null)
                {

                    while (sub21_pause_flage)
                    {

                        List<GetOrderClsSimple> tmp_list = task_list_g.FindAll(o => o.Status == 1 && o.Thread_id.Equals("2"));
                        for (int jj = 0; jj < tmp_list.Count; jj++)
                        {
                            //找出任务编号为task_id，并且状态为0的，把他的状态改为1，等待发送
                            int pkid = tmp_list[jj].Id;
                            string id = tmp_list[jj].Task_id;
                            string status = tmp_list[jj].Status + "";
                            string thread_id = tmp_list[jj].Thread_id;
                            //暂停
                            while (sub2_pause_flage)
                            {
                                Thread.Sleep(1);
                            }
                            if (task_list_g.Count > 0)
                            {
                                if (status.Equals("1") && thread_id.Equals("2"))
                                {
                                    //todo
                                    byte[] order_content_byte_array = ContentTrasforByteOrderCls.ContentTrasforByteOrder(tmp_list[jj].Order_content);
                                    SendCammand(order_content_byte_array, tmp_list[jj].Order_content);
                                    //task_list[jj].Status = 3;
                                    //发送指令
                                    //task_list[jj].Status = 2;
                                    if (task_list_g.Count > 0)
                                    {
                                        var model_run2 = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                        model_run2.Status = 2;
                                    }
                                    if (JudgeSwitchOrDelay(tmp_list[jj].Order_content))
                                    {
                                        if (task_list_g.Count > 0)
                                        {
                                            //修改列表中某一个元素
                                            var model_run = task_list_g.Where(c => c.Id == pkid).FirstOrDefault();
                                            model_run.Status = 3;
                                        }
                                        int delay_time = GetDelayTime(tmp_list[jj].Order_content);
                                        if (delay_time > 0)
                                        {
                                            Thread.Sleep(delay_time);
                                        }
                                    }

                                    //如果有开关，有延时的指令那么不用等待返回就可以继续发一下条指令
                                    string[] order_sz_array_switch = tmp_list[jj].Order_content.Split(':');
                                    if (order_sz_array_switch.Length == 2)
                                    {
                                        Thread.Sleep(50);
                                    }
                                    else
                                    {
                                        Thread.Sleep(220);
                                        AddQueryOrder(tmp_list[jj]);
                                        sub2_pause_flage = true;
                                    }

                                    //如果下一条指令为并行，那么当前指令执行完成后，暂停当前线程，，跳到并发线程1和并发线程2，在线程1中查找对应的指令下发，在线程2中去列表查找对应的指令下发，所有
                                    //并发指令都执行完成后，查看队列中状态为1，是否还有别的指令未发送，如果为串行，则去掉本方法的暂停标识，继续发送并行后的串行命令
                                    //如果当前串行指令执行完成了。
                                    Thread.Sleep(50);
                                }

                            }

                        }
                        Thread.Sleep(120);

                    }


                }
            }
        }
        public void SendOrderQuerySub0()
        {
            while (query_sub0_flage)
            {
                if (query_task_list.Count > 0)
                {
                    ///暂停
                    while (query_sub0_pause_flage)
                    {
                        Thread.Sleep(1);
                    }
                    List<GetOrderClsSimpleQuery> query_task_tmp = query_task_list.FindAll(o => o.Status1 != 3);
                    if (query_task_tmp.Count > 0)
                    {
                        byte[] order_content_byte_array = QueryContentTrasforByteOrder(query_task_tmp[0].Order_content);
                        //Console.Write("SendOrderQuerySub0 ----query_task_tmp[0]" + query_task_tmp[0].Status1 + "\r\n");
                        Thread.Sleep(140);
                        SendCammand(order_content_byte_array, query_task_tmp[0].Order_content);
                        Thread.Sleep(140);

                    }

                    if (query_task_tmp.Count > 1)
                    {
                        byte[] order_content_byte_array1 = QueryContentTrasforByteOrder(query_task_tmp[1].Order_content);
                        //Console.Write("SendOrderQuerySub0 ----query_task_tmp[1]" + query_task_tmp[1].Status1 + "\r\n");
                        Thread.Sleep(140);
                        SendCammand(order_content_byte_array1, query_task_tmp[1].Order_content);
                        Thread.Sleep(140);
                    }
                }
                Thread.Sleep(100);
            }
        }

        /// <summary>
        /// 分别三个查询队列，查询完成后，从队列中移除掉
        /// </summary>
        /// <param name="order_query"></param>
        public void AddQueryOrder(GetOrderClsSimple order_query)
        {
            string order_content = order_query.Order_content;

            int index = -1;
            index = order_content.IndexOf("D");

            if (index == -1)
                return;
            //order_query.Status = 0;
            GetOrderClsSimpleQuery order_q = new GetOrderClsSimpleQuery();
            order_q.Order_content = order_query.Order_content;
            order_q.Status1 = 0;
            order_q.Task_id = order_query.Task_id;
            order_q.Thread_id = order_query.Thread_id;
            order_q.Id = order_query.Id;
            switch (order_query.Thread_id)
            {
                case "0":
                    query_task_list.Add(order_q);
                    break;
                case "1":
                    query_task_list.Add(order_q);
                    break;
                case "2":
                    query_task_list.Add(order_q);
                    break;
            }

            //var model_query = query_task_list.Where(c => c.Status1 == 0 && c.Task_id.Equals("1") && c.Thread_id.Equals("0")).FirstOrDefault();
            //model_query.Status1 = 2;
        }
        public void ContinueRun(string task_id, int thread_id, int set_value, int query_value, int id, byte[] receive_data)
        {
            //Console.Write("***************************************************id:" + id + "\r\n");

            if (set_value.Equals(query_value))
            {
                not_equal_error_number = 0;

                var model_query = query_task_list.Where(c => c.Status1 == 0 && c.Task_id.Equals(task_id) && c.Thread_id.Equals(thread_id + "") && c.Id == id).FirstOrDefault();
                if (model_query != null)
                {
                    model_query.Status1 = 3;
                    for (int ii = 0; ii < query_task_list.Count; ii++)
                    {
                        if (query_task_list[ii].Id == id)
                        {
                            query_task_list.Remove(query_task_list.Find(o => o.Id == id));
                        }
                    }
                    if (thread_id == 0)
                    {
                        sub0_pause_flage = false;
                    }

                    TreeNode node = new TreeNode();//= treeView1.SelectedNode;
                    FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
                    {
                        node = treeView1.SelectedNode;
                    }));

                    if (thread_id == 1)
                    {
                        //sub1_pause_flage = false;
                        if (node.Text.IndexOf("并行") == -1 && node.Parent != null)
                        {
                            sub1_pause_flage = false;
                        }
                        else
                        {
                            thread1_query_finish = true;
                        }
                    }
                    if (thread_id == 2)
                    {
                        //sub2_pause_flage = false;
                        //thread2_query_finish = true;
                        if (node.Text.IndexOf("并行") == -1 && node.Parent != null)
                        {
                            sub2_pause_flage = false;
                        }
                        else
                        {
                            thread2_query_finish = true;
                        }
                    }

                    if (thread1_query_finish && thread2_query_finish && query_task_list.Count == 0)
                    {
                        thread1_query_finish = false;
                        thread2_query_finish = false;
                        sub1_pause_flage = false;
                        Thread.Sleep(120);
                        sub2_pause_flage = false;
                    }
                }

                var model_run = task_list_g.Where(c => c.Status == 2 && c.Task_id.Equals(task_id) && c.Thread_id.Equals(thread_id + "") && c.Id == id).FirstOrDefault();
                if (model_run != null)
                {
                    model_run.Status = 3;
                }
            }
        }
        /// <summary>
        /// 解析接收到的数据
        /// </summary>
        /// <param name="receive_data"></param>
        public void DealReceiveData(byte[] receive_data)
        {
            string recev_data = string.Join(" ", receive_data);
            //Console.Write(recev_data);
            //阀控制    //泵控制   if返回失败，重发三次，还失败，指令从发送队列中移去
            //泵返回
            //如果没有查询指令返回


            QueryValveHole(receive_data);
            if (query_task_list.Count == 0)
                return;

            //如果返回数据包不够长度返回
            if (receive_data.Length < 5)
                return;

            if (receive_data[0] == 0xFF && receive_data[1] == 0x2F && receive_data[2] == 0x30 && receive_data[3] == 0x60)
            {
                int length = receive_data.Length;
                if (receive_data[length - 3] == 0x03)
                {
                    Console.Write("收到完整的数据包");
                }
                //查询指令处理，如果是运行类指令，那么需要结合查询指令来判断下一次指令是否下发。
                //if()
                //非查询类指令处理
                if (receive_data.Length == 7)
                {
                    Console.Write("应答指令\r\n");
                }
                try
                {
                    if (receive_data.Length > 7)
                    {
                        string result = Encoding.ASCII.GetString(receive_data, 0, receive_data.Length);
                        byte[] tt = FindFirst3Position(receive_data);
                        if (tt.Length == 1)
                        {
                            int a = tt[0];
                            a = a - 48;
                            result = a + "";
                        }
                        else
                        {
                            result = Encoding.ASCII.GetString(tt, 0, tt.Length);
                        }
                        if (result != null)
                        {
                            Console.Write("value:" + QueryValue(query_task_list[0].Order_content) + "--" + result + "\r\n");
                        }
                        int value = QueryValue(query_task_list[0].Order_content);
                        int value_return = int.Parse(result);
                        Console.Write("valus_g:" + value + "offset:" + result + "\r\n");
                        //怎么知道是哪一个PUMP的
                        SaveReceiveLog(receive_data, int.Parse(query_task_list[0].Task_id), int.Parse(query_task_list[0].Thread_id), value, value_return, query_task_list[0].Id);
                        if (query_task_list.Count > 0)
                        {
                            ContinueRun(query_task_list[0].Task_id, int.Parse(query_task_list[0].Thread_id), value, value_return, query_task_list[0].Id, receive_data);
                        }
                        //if ((query_task_list[0]+ "").Equals(result))
                        //{
                        //    Console.Write("一致\r\n");
                        //}
                    }
                }
                catch
                {
                }
            }
            //阀返回
            if ((receive_data[0] == 0xCC && receive_data[2] == 0xFE))
            {
                Console.Write("阀返回\r\n");
                //RemoveSendList(receive_data);
            }

            //阀查询返回
            if ((receive_data[0] == 0xCC && receive_data[2] == 0x00))
            {
                int receive_value = 0;
                int receive_device_id = receive_data[1];
                if (receive_device_id < 3)
                {
                    receive_value = receive_data[3];
                }
                else
                {
                    byte[] tt = new byte[2];
                    tt[0] = receive_data[3];
                    tt[1] = receive_data[4];
                    receive_value = bytesToInt(tt, 0);
                }

                //string result = Encoding.ASCII.GetString(tt, 0, tt.Length);
                int value = QueryValue(query_task_list[0].Order_content);
                //Console.Write("valus_g:" + value + "hole:" + hole + "\r\n");
                SaveReceiveLog(receive_data, int.Parse(query_task_list[0].Task_id), int.Parse(query_task_list[0].Thread_id), value, receive_value, query_task_list[0].Id);
                if (query_task_list.Count > 0)
                {
                    ContinueRun(query_task_list[0].Task_id, int.Parse(query_task_list[0].Thread_id), value, receive_value, query_task_list[0].Id, receive_data);
                }
                //RemoveSendList(receive_data);
            }
        }

        private void ucBtnImg38_BtnClick(object sender, EventArgs e)
        {
            RunOther("TCA");
        }

        private void ucBtnImg39_BtnClick(object sender, EventArgs e)
        {
            RunOther("WASHCAPACAPBOXI");
        }
        public static int switch_g = 0;
        private void ucBtnImg40_BtnClick(object sender, EventArgs e)
        {
            int space_number = Convert.ToInt32(textBoxTestSpacePoint.Text);

            line_length_g = Convert.ToInt32(textBoxTestLineNumber.Text);
            //RunOther("Switch");
            //TriggerPrint();


            string print_content = "";
            for (int i = 0; i < line_length_g; i++)
            {
                //间隔1
                if (space_number == 0)
                {
                    print_content += "10001000-";
                }
                if (space_number == 1)
                {
                    print_content += "10000000-";
                }
                if (space_number == 2)
                {

                    //10000000 00001000 00000000 
                    // 0          1       2
                    //10000000 00001000 00000000 
                    // 3         4        5
                    //10000000 00001000 00000000  
                    // 6         7        8
                    if (i == 0)
                    {
                        print_content += "10000000-";
                    }
                    if (i == 1)
                    {
                        print_content += "00001000-";
                    }
                    if (i == 2)
                    {
                        print_content += "00000000-";
                    }

                    if (i > 2)
                    {
                        if (i % 3 == 0)
                        {
                            print_content += "10000000-";
                        }
                        if (i % 3 == 1)
                        {
                            print_content += "00001000-";
                        }
                        if (i % 3 == 2)
                        {
                            print_content += "00000000-";
                        }
                    }

                }
                if (space_number == 3)
                {
                    if (i % 2 == 0)
                    {
                        //Console.Write("x10000000");
                        print_content += "10000000-";
                    }
                    else
                    {
                        print_content += "00000000-";
                    }
                }

                if (space_number == 4)
                {
                    //10000000 00000000 00001000 00000000 00000000
                    //    0       1        2        3        4
                    //10000000 00000000 00001000 00000000  00000000
                    //    5        6        7        8        9
                    //10000000 00000000 00001000 00000000  00000000
                    //    10        11      12        13      14
                    if (i == 0)
                    {
                        print_content += "10000000-";
                    }
                    if (i == 1)
                    {
                        print_content += "00000000-";
                    }
                    if (i == 2)
                    {
                        print_content += "00001000-";
                    }
                    if (i == 3)
                    {
                        print_content += "00000000-";
                    }
                    if (i == 4)
                    {
                        print_content += "00000000-";
                    }
                    if (i > 4)
                    {
                        if (i % 5 == 0)
                        {
                            print_content += "10000000-";
                        }
                        if (i % 5 == 1)
                        {
                            print_content += "00000000-";
                        }
                        if (i % 5 == 2)
                        {
                            print_content += "00001000-";
                        }
                        if (i % 5 == 3)
                        {
                            print_content += "00000000-";
                        }
                        if (i % 5 == 4)
                        {
                            print_content += "00000000-";
                        }
                    }
                }
            }
            Console.WriteLine(print_content);
        }

        /// <summary>
        /// trigger print 
        /// </summary>
        public static void TriggerPrint()
        {
            //16777216
            int a = 0b1000000000000000000000000;//int.Parse(textBox3.Text);
            if (switch_g == 0)
            {
                if (system_type.Equals("1"))
                {
                    switch_g = 16777216;
                }

                if (system_type.Equals("2"))
                {
                    switch_g = 16;
                }
                //switch_g = 16;
            }
            else
            {
                switch_g = 0;
            }
            a = switch_g;
            byte[] a_array = ConvertHex4_1(a);
            byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH_New1(4, 31, 4, a_array);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        public void CloseAllSwitch()
        {
            //16777216
            int a = 0b1000000000000000000000000;//int.Parse(textBox3.Text);
            if (switch_g == 0)
            {
                switch_g = 0;
            }
            else
            {
                switch_g = 0;
            }
            a = switch_g;
            byte[] a_array = ConvertHex4_1(a);
            byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH_New1(4, 31, 4, a_array);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }
        public void openSwitch(int value)
        {
            byte[] a_array = ConvertHex4_1(value);
            byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH_New1(4, 31, 4, a_array);
            SendCammand(order_content_byte_array, "Switch:" + value);
        }
        public void openSwitch()
        {
            //16777216
            int a = 0b11111111;//int.Parse(textBox3.Text);
            if (switch_g == 0)
            {
                switch_g = 0;
            }
            else
            {
                switch_g = 0;
            }
            a = switch_g;
            byte[] a_array = ConvertHex4_1(a);
            byte[] order_content_byte_array = ContentTrasforByteOrderCls.WriteCoil_0FH_New1(4, 31, 4, a_array);
            SendCammand(order_content_byte_array, "Switch:" + a);
        }

        private void ucBtnImg41_BtnClick(object sender, EventArgs e)
        {
            print_content_list_g = CommonData.PrintContentList_g;
            if (print_content_list_g.Count == 0)
            {
                FrmDialog.ShowDialog(this, "未生成序列", "提示");
                return;
            }
            a_valid_a = 0;
            a_valid_t = 0;
            a_valid_c = 0;
            a_valid_g = 0;
            a_valid_all = 0;
            line_length_g = 10;
            space_point_g = 0;
            one_page_loop_number = 0;
            line_length_g = Convert.ToInt32(textBoxTestLineNumber.Text);
            space_point_g = Convert.ToInt32(textBoxTestSpacePoint.Text);
            coupling_g = Convert.ToInt32(textBoxCoupingTime.Text);
            find_zero_delay_time = Convert.ToInt32(textBox7.Text);
            try
            {
                one_page_loop_number = Convert.ToInt32(textBoxLoopNumberG.Text);
                if (one_page_loop_number < 1)
                {
                    one_page_loop_number = 1;
                    MessageBox.Show("不要小于1");
                }
                if (one_page_loop_number > 10)
                {
                    one_page_loop_number = 1;
                    MessageBox.Show("不要大于10");
                }
            }
            catch
            {
                MessageBox.Show("输入异常");
                one_page_loop_number = 1;
            }
            InitialPrint();
            if (checkBoxAValid.Checked)
            {
                a_valid_a = 1;
            }
            if (checkBoxTValid.Checked)
            {
                a_valid_t = 1;
            }
            if (checkBoxCValid.Checked)
            {
                a_valid_c = 1;
            }
            if (checkBoxGValid.Checked)
            {
                a_valid_g = 1;
            }
            if (checkBoxAllValid.Checked)
            {
                a_valid_all = 1;
            }
            run_flage = true;
            //m_total_run_stop_flage = true;
            m_total_run_stop_flage = true;
            m_total_run_pause_flage = false;
            ucBtnImg41.Enabled = false;

            CloseAllSwitch();
            //Task.Factory.StartNew(() => { StartToRun_New(); });

        }

        private void ucBtnImg44_BtnClick(object sender, EventArgs e)
        {
            OneActive("打印开始");
            //StartPrintA();
        }

        private void ucBtnImg43_BtnClick(object sender, EventArgs e)
        {
            OneActive("打印结束");
        }

        private void ucBtnImg42_BtnClick(object sender, EventArgs e)
        {
            OneActive("显微镜观察");
        }

        private void ucBtnImg45_BtnClick(object sender, EventArgs e)
        {
            openSwitch();
            return;


            if (checkBox1.Checked)
            {
                SendReadZAXisPosition(1);
            }

            Thread.Sleep(150);
            if (checkBox2.Checked)
            {
                SendReadZAXisPosition(2);
            }

            Thread.Sleep(150);
            if (checkBox3.Checked)
            {
                SendReadZAXisPosition(3);
            }

        }
        public void SendReadZAXisPosition(byte z_index)
        {
            byte[] order_content_byte_array = new byte[8];
            order_content_byte_array = ContentTrasforByteOrderCls.ReadInputReg_04H((byte)z_index, 0x0002, 2);    //填充写单个线圈寄存器//CoilOFFAndON(33, a);
            SendCammand(order_content_byte_array, "Switch:" + 1000);
        }

        private void ucBtnImg46_BtnClick(object sender, EventArgs e)
        {
            OneActive("A喷头");
        }

        private void ucBtnImg47_BtnClick(object sender, EventArgs e)
        {
            OneActive("T喷头");
        }

        private void ucBtnImg48_BtnClick(object sender, EventArgs e)
        {
            OneActive("C喷头");
        }

        private void ucBtnImg49_BtnClick(object sender, EventArgs e)
        {
            OneActive("G喷头");
        }

        private void ucBtnImg50_BtnClick(object sender, EventArgs e)
        {
            OneActive("ALL喷头");
        }

        private void ucBtnImg51_BtnClick(object sender, EventArgs e)
        {
            //触发打印
            TriggerPrint();
            Thread.Sleep(210);
            TriggerPrint();

            OneActive_Static("打印结束");
            //Thread.Sleep(1500);

        }

        private void ucBtnImg52_BtnClick(object sender, EventArgs e)
        {
            int iLEN = serialPortvalve_Send.BytesToRead; byte[] b_read = new byte[iLEN];
            serialPortvalve_Send.Read(b_read, 0, iLEN);

            //SendPageDataWork_back20211229();

            print_content_list_g = CommonData.PrintContentList_g;
            //每页打印的高度，也就是行数
            jobInfo.height = print_content_list_g.Count;

            bool result = false;
            int stride = (jobInfo.width + 31) / 32 * 4;
            int dataSize = stride * jobInfo.height;
            byte[] send_data_array_one160 = new byte[dataSize];
            byte[] send_data_array_one40 = new byte[40];
            var p = Marshal.AllocHGlobal(dataSize);
            //1个字节，8位 1111 1111 分别对应ACBD  ACBD 喷头上的四排喷嘴
            //IntPtr data = Marshal.AllocHGlobal(dataSize);
            //喷的区域的高度
            int length = 40;
            byte[] send_data_array_one160_new = new byte[160];
            byte[] send_data_array_one10_k = new byte[length];
            byte[] send_data_array_one10_c = new byte[length];

            for (int i = 0; i < num_pages_g;)
            {
                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0xff;
                }

                for (int k = 0; k < send_data_array_one10_k.Length; k++)
                {
                    send_data_array_one10_k[k] = 0b10001000;
                }

                for (int c = 0; c < send_data_array_one10_c.Length; c++)
                {
                    send_data_array_one10_c[c] = 0b00010001;
                }

                for (int ij = 0; ij < dataSize; ij++)
                {
                    send_data_array_one160[ij] = 0x00;
                }

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_k, 0, send_data_array_one160, ij, length);
                    ij += 160;
                }

                Marshal.Copy(send_data_array_one160, 0, p, dataSize);

                for (int ij = 0; ij < dataSize;)
                {
                    Array.Copy(send_data_array_one10_c, 0, send_data_array_one160, ij, length);
                    ij += 160;
                }
                Marshal.Copy(send_data_array_one160, 0, p, dataSize);
            }
            Marshal.FreeHGlobal(p);
            result = true;
        }

        public static int a_valid_a = 0;
        public static int a_valid_t = 0;
        public static int a_valid_c = 0;
        public static int a_valid_g = 0;
        public static int a_valid_all = 0;

        public static int line_length_g = 0;
        public static int space_point_g = 0;
        /// <summary>
        /// 偶联时间
        /// </summary>
        public static int coupling_g = 10000;
        public static int find_zero_delay_time = 10000;
        public static int one_page_print_height = 10;
        public static int one_page_loop_number = 0;

        /// <summary>
        /// 单步里面添加打印内容  系统1
        /// </summary>
        public void AddPrintOneStepSystem1()
        {
            a_valid_a = 0;
            a_valid_t = 0;
            a_valid_c = 0;
            a_valid_g = 0;
            a_valid_all = 0;

            line_length_g = 10;
            space_point_g = 0;

            line_length_g = Convert.ToInt32(textBoxTestLineNumber.Text);
            space_point_g = Convert.ToInt32(textBoxTestSpacePoint.Text);
            one_page_print_height = Convert.ToInt32(textBox1.Text);

            if (checkBoxAValid.Checked)
            {
                a_valid_a = 1;
            }
            if (checkBoxTValid.Checked)
            {
                a_valid_t = 1;
            }
            if (checkBoxCValid.Checked)
            {
                a_valid_c = 1;
            }
            if (checkBoxGValid.Checked)
            {
                a_valid_g = 1;
            }
            if (checkBoxAllValid.Checked)
            {
                a_valid_all = 1;
            }

            int mainBoardId = 0;
            int headBoardId = 0;

            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("A喷头"));
            float y_offset_a = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("T喷头"));
            float y_offset_t = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("C喷头"));
            float y_offset_c = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("G喷头"));
            float y_offset_g = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("ALL喷头"));
            float y_offset_all = (move_start_position_list[0].Y_axis) * ((float)1 / 20000);

            PhysicalHead ph = PhysicalHead.Default;
            //int y_offset = (int)(((32 - 11.3) / 2.54) * 2400);                //这个为基准，下面的值都必须比这个值大，如果打出来的偏右了，把值调小一些，如果打出来靠左一些，把值高大一些。
            int y_offset = (int)(((32 - y_offset_a) / 2.54) * 2400);
            ph.y_offset = y_offset;
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 0, ph);

            PhysicalHead ph_c = PhysicalHead.Default;
            //ph_c.y_offset = (int)(((32 - 13.8) / 2.54) * 2400);
            ph_c.y_offset = (int)(((32 - y_offset_t) / 2.54) * 2400);
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 1, ph_c);

            PhysicalHead ph_m = PhysicalHead.Default;
            //ph_m.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);
            ph_m.y_offset = (int)(((32 - y_offset_c) / 2.54) * 2400);
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 2, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            //ph_y.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);
            ph_y.y_offset = (int)(((32 - y_offset_g) / 2.54) * 2400);
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 3, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            //ph_w.y_offset = (int)(((32 - 21.3) / 2.54) * 2400);
            ph_w.y_offset = (int)(((32 - y_offset_all) / 2.54) * 2400);
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);

            do
            {
                bool mbIsConnected = jprint.MainBoardIsConnected(mainBoardId);
                bool hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId);
                if (mbIsConnected == false || hbIsConnected == false)
                {
                    Console.WriteLine("板卡连接失败");
                    break;
                }

                jobInfo.height = one_page_print_height;
                Console.WriteLine("板卡连接成功");
                var rc = jprint.Start(jobInfo);
                if (rc != JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    Console.WriteLine($"启动打印失败: {rc}");
                    break;
                }
                Console.WriteLine("启动打印成功");

                //
                //打印内容添加
                ucBtnImg53.Enabled = false;
                m_test_print_add_content_flage = false;
                Task.Factory.StartNew(() => { SendPageDataWork_Test_Y_Position(); });
            } while (false);
        }

        public void AddPrintOneStepSystem2(bool isZong)
        {
            a_valid_a = 0;
            a_valid_t = 0;
            a_valid_c = 0;
            a_valid_g = 0;
            a_valid_all = 0;

            line_length_g = 10;
            space_point_g = 0;

            line_length_g = Convert.ToInt32(textBoxTestLineNumber.Text);
            space_point_g = Convert.ToInt32(textBoxTestSpacePoint.Text);
            one_page_print_height = Convert.ToInt32(textBox1.Text);

            if (checkBoxAValid.Checked)
            {
                a_valid_a = 1;
            }
            if (checkBoxTValid.Checked)
            {
                a_valid_t = 1;
            }
            if (checkBoxCValid.Checked)
            {
                a_valid_c = 1;
            }
            if (checkBoxGValid.Checked)
            {
                a_valid_g = 1;
            }
            if (checkBoxAllValid.Checked)
            {
                a_valid_all = 1;
            }

            int mainBoardId = 0;
            int headBoardId = 0;

            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("A喷头"));
            float y_offset_a = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("T喷头"));
            float y_offset_t = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("C喷头"));
            float y_offset_c = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("G喷头"));
            float y_offset_g = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("ALL喷头"));
            float y_offset_all = (move_start_position_list[0].Y_axis) * ((float)1 / 100000);

            PhysicalHead ph = PhysicalHead.Default;
            //int y_offset = (int)(((32 - 11.3) / 2.54) * 2400);                //这个为基准，下面的值都必须比这个值大，如果打出来的偏右了，把值调小一些，如果打出来靠左一些，把值高大一些。
            int y_offset = (int)(((28 - Math.Abs(y_offset_a)) / 2.54) * 2400);
            ph.y_offset = y_offset;
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 0, ph);

            PhysicalHead ph_c = PhysicalHead.Default;
            //ph_c.y_offset = (int)(((32 - 13.8) / 2.54) * 2400);
            ph_c.y_offset = (int)(((28 - Math.Abs(y_offset_t)) / 2.54) * 2400);
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 1, ph_c);

            PhysicalHead ph_m = PhysicalHead.Default;
            //ph_m.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);
            ph_m.y_offset = (int)(((28 - Math.Abs(y_offset_c)) / 2.54) * 2400);
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 2, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            //ph_y.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);
            ph_y.y_offset = (int)(((28 - Math.Abs(y_offset_g)) / 2.54) * 2400);
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 3, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            //ph_w.y_offset = (int)(((32 - 21.3) / 2.54) * 2400);
            ph_w.y_offset = (int)(((28 - Math.Abs(y_offset_all)) / 2.54) * 2400);
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);

            do
            {
                bool mbIsConnected = jprint.MainBoardIsConnected(mainBoardId);
                bool hbIsConnected = jprint.HeadBoardIsConnected(mainBoardId, headBoardId);
                if (mbIsConnected == false || hbIsConnected == false)
                {
                    Console.WriteLine("板卡连接失败");
                    break;
                }

                jobInfo.height = one_page_print_height;
                Console.WriteLine("板卡连接成功");
                var rc = jprint.Start(jobInfo);
                if (rc != JytPrintEngineWrapper.PRINTENGINE_RESULT_SUCCESS)
                {
                    Console.WriteLine($"启动打印失败: {rc}");
                    break;
                }
                Console.WriteLine("启动打印成功");

                //
                //打印内容添加
                if (!isZong)
                {
                    ucBtnImg53.Enabled = false;
                    m_test_print_add_content_flage = false;
                    Task.Factory.StartNew(() => { SendPageDataWork_Test_Y_Position(); });
                }
            } while (false);

        }

        private void ucBtnImg53_BtnClick(object sender, EventArgs e)
        {
            if (system_type.Equals("1"))
            {
                AddPrintOneStepSystem1();
            }

            if (system_type.Equals("2"))
            {
                AddPrintOneStepSystem2(false);
            }
        }

        public void InitialPrint()
        {
            int mainBoardId = 0;
            int headBoardId = 0;

            List<MoveStartPosition> move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("A喷头"));
            float y_offset_a = (move_start_position_list[0].Y_axis) * ((float)1 / 200000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("T喷头"));
            float y_offset_t = (move_start_position_list[0].Y_axis) * ((float)1 / 200000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("C喷头"));
            float y_offset_c = (move_start_position_list[0].Y_axis) * ((float)1 / 200000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("G喷头"));
            float y_offset_g = (move_start_position_list[0].Y_axis) * ((float)1 / 200000);
            move_start_position_list = move_start_position_list_g.FindAll(o => o.Name.Equals("ALL喷头"));
            float y_offset_all = (move_start_position_list[0].Y_axis) * ((float)1 / 200000);

            PhysicalHead ph = PhysicalHead.Default;
            //int y_offset = (int)(((32 - 11.3) / 2.54) * 2400);                //这个为基准，下面的值都必须比这个值大，如果打出来的偏右了，把值调小一些，如果打出来靠左一些，把值高大一些。
            int y_offset = (int)(((32 - y_offset_a) / 2.54) * 2400);
            ph.y_offset = y_offset;
            ph.head_type = HeadType.Gen5;
            ph.num_child_heads = 1;
            ph.child_channels[0] = ColorChannel.K;  //A
            ph.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 0, ph);


            PhysicalHead ph_c = PhysicalHead.Default;
            //ph_c.y_offset = (int)(((32 - 13.8) / 2.54) * 2400);
            ph_c.y_offset = (int)(((32 - y_offset_t) / 2.54) * 2400);
            ph_c.head_type = HeadType.Gen5;
            ph_c.num_child_heads = 1;
            ph_c.child_channels[0] = ColorChannel.C; //T
            ph_c.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 1, ph_c);

            PhysicalHead ph_m = PhysicalHead.Default;
            //ph_m.y_offset = (int)(((32 - 16.3) / 2.54) * 2400);
            ph_m.y_offset = (int)(((32 - y_offset_c) / 2.54) * 2400);
            ph_m.head_type = HeadType.Gen5;
            ph_m.num_child_heads = 1;
            ph_m.child_channels[0] = ColorChannel.M; //C
            ph_m.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 2, ph_m);

            PhysicalHead ph_y = PhysicalHead.Default;
            //ph_y.y_offset = (int)(((32 - 18.8) / 2.54) * 2400);
            ph_y.y_offset = (int)(((32 - y_offset_g) / 2.54) * 2400);
            ph_y.head_type = HeadType.Gen5;
            ph_y.num_child_heads = 1;
            ph_y.child_channels[0] = ColorChannel.Y; //G
            ph_y.child_enables[0] = true;
            jprint.SetPhysicalHead(mainBoardId, 3, ph_y);

            PhysicalHead ph_w = PhysicalHead.Default;
            //ph_w.y_offset = (int)(((32 - 21.3) / 2.54) * 2400);
            ph_w.y_offset = (int)(((32 - y_offset_all) / 2.54) * 2400);
            ph_w.head_type = HeadType.Gen5;
            ph_w.num_child_heads = 1;
            ph_w.child_channels[0] = ColorChannel.White; //all
            ph_w.child_enables[0] = true;
            jprint.SetPhysicalHead(1, 0, ph_w);


            /*
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
                    Console.WriteLine($"启动打印失败: {rc}");
                    break;
                }
                Console.WriteLine("启动打印成功");
                //打印内容添加
                //Task.Factory.StartNew(() => { SendPageDataWork_Test_Y_Position(); });
            } while (false);

            */
        }

        private void ucBtnImg54_BtnClick(object sender, EventArgs e)
        {
            TriggerPrint();
            Thread.Sleep(360);
            TriggerPrint();
            OneActive_Static("打印结束");

            FrmPrintATCG.mainFrm.Invoke((EventHandler)(delegate
            {
                ucBtnImg53.Enabled = true;
            }));
        }

        //定义水星电机要用到的全局变量
        bool m_bIsOpen = false;                           ///<设备打开状态
        bool m_bIsSnap = false;                           ///<发送开采命令标识
        bool m_bTriggerMode = false;                           ///<是否支持触发模式
        bool m_bTriggerActive = false;                           ///<是否支持触发极性
        bool m_bTriggerSource = false;                           ///<是否支持触发源 
        bool m_bWhiteAuto = false;                           ///<标识是否支持白平衡
        bool m_bBalanceRatioSelector = false;                           ///<标识是否支持白平衡通道
        bool m_bWhiteAutoSelectedIndex = true;                            ///<白平衡列表框转换标志
        IGXFactory m_objIGXFactory = null;                            ///<Factory对像
        IGXDevice m_objIGXDevice = null;                            ///<设备对像
        IGXStream m_objIGXStream = null;                            ///<流对像
        IGXFeatureControl m_objIGXFeatureControl = null;                            ///<远端设备属性控制器对像
        IGXFeatureControl m_objIGXStreamFeatureControl = null;                            ///<流层属性控制器对象
        string m_strBalanceWhiteAutoValue = "Off";                           ///<自动白平衡当前的值
        GxBitmap m_objGxBitmap = null;                            ///<图像显示类对象
        string m_strFilePath = "";                              ///<应用程序当前路径

        /// <summary>
        /// 设备打开后初始化界面
        /// </summary>
        private void __InitUI()
        {
            //__InitEnumComBoxUI(m_cb_TriggerMode, "TriggerMode", m_objIGXFeatureControl, ref m_bTriggerMode);                      //触发模式初始化
            //__InitEnumComBoxUI(m_cb_TriggerSource, "TriggerSource", m_objIGXFeatureControl, ref m_bTriggerSource);                //触发源初始化
            //__InitEnumComBoxUI(m_cb_TriggerActivation, "TriggerActivation", m_objIGXFeatureControl, ref m_bTriggerActive);        //触发极性初始化
            //__InitShutterUI();                                                                                                    //曝光初始化
            //__InitGainUI();                                                                                                       //增益的初始化
            //__InitWhiteRatioUI();                                                                                                 //初始化白平衡系数相关控件
            //__InitEnumComBoxUI(m_cb_AutoWhite, "BalanceWhiteAuto", m_objIGXFeatureControl, ref m_bWhiteAuto);                     //自动白平衡的初始化
            //__InitEnumComBoxUI(m_cb_RatioSelector, "BalanceRatioSelector", m_objIGXFeatureControl, ref m_bBalanceRatioSelector);  //白平衡通道选择


            //获取白平衡当前的值
            bool bIsImplemented = false;             //是否支持
            bool bIsReadable = false;                //是否可读
            // 获取是否支持
            if (null != m_objIGXFeatureControl)
            {
                bIsImplemented = m_objIGXFeatureControl.IsImplemented("BalanceWhiteAuto");
                bIsReadable = m_objIGXFeatureControl.IsReadable("BalanceWhiteAuto");
                if (bIsImplemented)
                {
                    if (bIsReadable)
                    {
                        //获取当前功能值
                        m_strBalanceWhiteAutoValue = m_objIGXFeatureControl.GetEnumFeature("BalanceWhiteAuto").GetValue();
                    }
                }
            }
        }

        public void SetGalaxyTimeOut(IGXFeatureControl objFeatureControl)
        {
#if DEBUG
            //IGXDevice objIGXDevice = m_objIGXFactory.OpenDeviceBySN("WM0210121052", GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
            //获取远端设备属性控制器
            //IGXFeatureControl objFeatureControl = objIGXDevice.GetRemoteFeatureControl();
            //设置心跳超时时间5分钟
            objFeatureControl.GetIntFeature("GevHeartbeatTimeout").SetValue(300000);
#endif
        }

        public void StartGalaxy()
        {
            try
            {
                List<IGXDeviceInfo> listGXDeviceInfo = new List<IGXDeviceInfo>();

                //关闭流
                __CloseStream();
                // 如果设备已经打开则关闭，保证相机在初始化出错情况下能再次打开
                __CloseDevice();

                m_objIGXFactory.UpdateDeviceList(200, listGXDeviceInfo);


                // 判断当前连接设备个数
                if (listGXDeviceInfo.Count <= 0)
                {
                    MessageBox.Show("未发现设备!");
                    return;
                }

                // 如果设备已经打开则关闭，保证相机在初始化出错情况下能再次打开
                if (null != m_objIGXDevice)
                {
                    m_objIGXDevice.Close();
                    m_objIGXDevice = null;
                }

                //打开列表第一个设备

                m_objIGXDevice = m_objIGXFactory.OpenDeviceBySN(listGXDeviceInfo[0].GetSN(), GX_ACCESS_MODE.GX_ACCESS_EXCLUSIVE);
                m_objIGXFeatureControl = m_objIGXDevice.GetRemoteFeatureControl();

                SetGalaxyTimeOut(m_objIGXFeatureControl);

                //打开流
                if (null != m_objIGXDevice)
                {
                    m_objIGXStream = m_objIGXDevice.OpenStream(0);
                    m_objIGXStreamFeatureControl = m_objIGXStream.GetFeatureControl();
                }

                // 建议用户在打开网络相机之后，根据当前网络环境设置相机的流通道包长值，
                // 以提高网络相机的采集性能,设置方法参考以下代码。
                GX_DEVICE_CLASS_LIST objDeviceClass = m_objIGXDevice.GetDeviceInfo().GetDeviceClass();
                if (GX_DEVICE_CLASS_LIST.GX_DEVICE_CLASS_GEV == objDeviceClass)
                {
                    // 判断设备是否支持流通道数据包功能
                    if (true == m_objIGXFeatureControl.IsImplemented("GevSCPSPacketSize"))
                    {
                        // 获取当前网络环境的最优包长值
                        uint nPacketSize = m_objIGXStream.GetOptimalPacketSize();
                        // 将最优包长值设置为当前设备的流通道包长值
                        m_objIGXFeatureControl.GetIntFeature("GevSCPSPacketSize").SetValue(nPacketSize);
                    }
                }

                //初始化相机参数
                __InitDevice();

                // 获取相机参数,初始化界面控件
                __InitUI();

                m_objGxBitmap = new GxBitmap(m_objIGXDevice, pictureBox1);

                // 更新设备打开标识
                m_bIsOpen = true;



                //刷新界面
                //__UpdateUI();

            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }

        }

        public void CollectionCamPic()
        {
            try
            {
                if (null != m_objIGXStreamFeatureControl)
                {
                    try
                    {
                        //设置流层Buffer处理模式为OldestFirst
                        m_objIGXStreamFeatureControl.GetEnumFeature("StreamBufferHandlingMode").SetValue("OldestFirst");
                    }
                    catch (Exception)
                    {
                    }
                }

                //开启采集流通道
                if (null != m_objIGXStream)
                {
                    //RegisterCaptureCallback第一个参数属于用户自定参数(类型必须为引用
                    //类型)，若用户想用这个参数可以在委托函数中进行使用
                    m_objIGXStream.RegisterCaptureCallback(this, __CaptureCallbackPro);
                    m_objIGXStream.StartGrab();
                }

                //发送开采命令
                if (null != m_objIGXFeatureControl)
                {
                    m_objIGXFeatureControl.GetCommandFeature("AcquisitionStart").Execute();
                }
                m_bIsSnap = true;

                // 更新界面UI
                //__UpdateUI();
            }
            catch (Exception ex)
            {
                MessageBox.Show(ex.Message);
            }
        }


        /// <summary>
        /// 回调函数,用于获取图像信息和显示图像
        /// </summary>
        /// <param name="obj">用户自定义传入参数</param>
        /// <param name="objIFrameData">图像信息对象</param>
        private void __CaptureCallbackPro(object objUserParam, IFrameData objIFrameData)
        {
            try
            {
                FrmPrintATCG objGxSingleCam = objUserParam as FrmPrintATCG;
                objGxSingleCam.ImageShowAndSave(objIFrameData);
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 图像的显示和存储
        /// </summary>
        /// <param name="objIFrameData">图像信息对象</param>
        void ImageShowAndSave(IFrameData objIFrameData)
        {
            try
            {
                //m_objGxBitmap.ReturnBmp(objIFrameData, "");//.ReturnBmp(objIFrameData).

                //m_objGxBitmap = new GxBitmap(m_objIGXDevice, pictureBox1);
                //pictureBox1.Image = bmp_;
                //pictureBox1.Invalidate();
                m_objGxBitmap.Show(objIFrameData);
            }
            catch (Exception)
            {
            }


            m_strFilePath = "CamFile";
            // 是否需要进行图像保存
            if (m_save_pic_flage)
            {
                m_save_pic_flage = false;

                DateTime dtNow = System.DateTime.Now;  // 获取系统当前时间
                string strDateTime = dtNow.Year.ToString() + "_"
                                   + dtNow.Month.ToString() + "_"
                                   + dtNow.Day.ToString() + "_"
                                   + dtNow.Hour.ToString() + "_"
                                   + dtNow.Minute.ToString() + "_"
                                   + dtNow.Second.ToString() + "_"
                                   + dtNow.Millisecond.ToString();
                string stfFileName = m_strFilePath + "\\" + strDateTime + ".jpg";  // 默认的图像保存名称
                m_objGxBitmap.SaveBmp(objIFrameData, stfFileName);




                //string send_path = "";
                //send_path = stfFileName;
                SendShowPicPath(stfFileName);

                var sw = new System.IO.StreamWriter("SaveBmp.txt", true);
                sw.WriteLine(DateTime.Now.ToString() + ":" + "SaveBmp:" + "" + stfFileName + "\r\n" + "");
                sw.Close();


                //m_save_pic_flage = false;
            }
        }

        /// <summary>
        /// 相机初始化
        /// </summary>
        void __InitDevice()
        {
            if (null != m_objIGXFeatureControl)
            {
                //设置采集模式连续采集
                m_objIGXFeatureControl.GetEnumFeature("AcquisitionMode").SetValue("Continuous");
            }
        }

        /// <summary>
        /// 关闭流
        /// </summary>
        private void __CloseStream()
        {
            try
            {
                //关闭流
                if (null != m_objIGXStream)
                {
                    m_objIGXStream.Close();
                    m_objIGXStream = null;
                    m_objIGXStreamFeatureControl = null;
                }
            }
            catch (Exception)
            {
            }
        }

        /// <summary>
        /// 关闭设备
        /// </summary>
        private void __CloseDevice()
        {
            try
            {
                //关闭设备
                if (null != m_objIGXDevice)
                {
                    m_objIGXDevice.Close();
                    m_objIGXDevice = null;
                }
            }
            catch (Exception)
            {
            }
        }


        /// <summary>
        /// 初始化目镜
        /// </summary>
        public void StarCam()
        {
            if (cam_ != null)
                return;

            Uvcsam.Device[] arr = Uvcsam.Enum();
            if (arr.Length <= 0)
                MessageBox.Show("No camera found.");
            else
            {
                cam_ = Uvcsam.open(arr[0].id);
                if (cam_ != null)
                {
                    cam_.put(Uvcsam.eCMD.AWB, 2);

                    //checkBox1.Enabled = trackBar1.Enabled = true;
                    //button3.Enabled = true;

                    cam_.get(Uvcsam.eCMD.WIDTH | 0, out width_);
                    cam_.get(Uvcsam.eCMD.HEIGHT | 0, out height_);
                    bmp_ = new Bitmap(width_, height_, PixelFormat.Format24bppRgb);
                    buf_ = Marshal.AllocHGlobal(Uvcsam.TDIBWIDTHBYTES(width_ * 24) * height_);
                    if (cam_.start(buf_, (Uvcsam.eEVENT nEvent) =>
                    {
                        /* this delegate is called by internal thread of uvcsam.dll which is NOT the same of UI thread.
                         * Why we use BeginInvoke, Please see:
                         * http://msdn.microsoft.com/en-us/magazine/cc300429.aspx
                         * http://msdn.microsoft.com/en-us/magazine/cc188732.aspx
                         * http://stackoverflow.com/questions/1364116/avoiding-the-woes-of-invoke-begininvoke-in-cross-thread-winform-event-handling
                         */
                        BeginInvoke((Action)(() =>
                        {
                            /* this run in the UI thread */
                            if (0 != (nEvent & Uvcsam.eEVENT.IMAGE))
                            {
                                if (bmp_ != null)
                                {
                                    BitmapData bmpdata = bmp_.LockBits(new Rectangle(0, 0, bmp_.Width, bmp_.Height), ImageLockMode.WriteOnly, bmp_.PixelFormat);
                                    Uvcsam.memcpy(bmpdata.Scan0, buf_, new IntPtr(Uvcsam.TDIBWIDTHBYTES(width_ * 24) * height_));
                                    bmp_.UnlockBits(bmpdata);
                                }
                                pictureBox1.Image = bmp_;
                                pictureBox1.Invalidate();
                            }
                            if (0 != (nEvent & Uvcsam.eEVENT.EXPOTIME))
                                UpdateExpoTime();
                            if (0 != (nEvent & Uvcsam.eEVENT.DISCONNECT))
                                MessageBox.Show("Camera disconnect.");
                            if (0 != (nEvent & Uvcsam.eEVENT.ERROR))
                                MessageBox.Show("Generic error.");
                        }));
                    }) < 0)
                        MessageBox.Show("Failed to start camera.");
                    else
                    {
                        int nMin = 0, nMax = 0, nDef = 0;
                        cam_.range(Uvcsam.eCMD.EXPOTIME, out nMin, out nMax, out nDef);
                        //trackBar1.SetRange(nMin, nMax);

                        UpdateExpoTime();
                        int val = 0;
                        cam_.get(Uvcsam.eCMD.AE_ONOFF, out val);
                        checkBox1.Checked = (val != 0);
                        //trackBar1.Enabled = !checkBox1.Checked;
                    }
                }
            }
        }

        /// <summary>
        /// 目镜保存图片
        /// </summary>
        public void CamSavePic()
        {
            BeginInvoke((Action)(() =>
            {
                string pic_name = DateTime.Now.ToString("yyyy-MM-dd-hh-mm-ss-fff");
                string file_path = string.Format("CamFile\\Cam_{0}.jpg", pic_name);
                if ((cam_ != null) && (bmp_ != null))
                    bmp_.Save(file_path, ImageFormat.Jpeg);
            }));

        }
        /// <summary>
        /// 保存图片，如果为TRUE时保存，为False不保存
        /// </summary>
        public static bool m_save_pic_flage = false;

        private void ucBtnImg55_BtnClick(object sender, EventArgs e)
        {
            //CamSavePic();
            m_save_pic_flage = true;
        }

        private void ucBtnImg56_BtnClick(object sender, EventArgs e)
        {
            RunOther("WASH");
        }

        private void ucBtnImg57_BtnClick(object sender, EventArgs e)
        {
            m_total_run_stop_flage = false;
        }

        private void ucBtnImg58_BtnClick(object sender, EventArgs e)
        {
            OneActive("废液槽");
        }

        private void ucBtnImg59_BtnClick(object sender, EventArgs e)
        {
            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            //fileDialog.Filter="所有文件(*.*)|*.*";
            fileDialog.Filter = "SCR files (*.scr)|*.scr";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = fileDialog.FileName;
                string file_name = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
                RunOther(file_name);
            }
        }

        private void ucBtnImg60_BtnClick(object sender, EventArgs e)
        {
            TriggerPrint();
            //Thread.Sleep(260);
            //TriggerPrint();


            //int a = 0b11111111111111111111111111111;
            //openSwitch(a);
        }

        private void ucBtnImg61_BtnClick(object sender, EventArgs e)
        {
            int a = 0x00;
            openSwitch(a);
        }

        private void FrmPrintATCG_FormClosed(object sender, FormClosedEventArgs e)
        {
            CloseCam();
            //System.Environment.Exit(System.Environment.ExitCode); //等等
            System.Environment.Exit(0);
        }

        private void UpdateExpoTime()
        {
            int val = 0;
            cam_.get(Uvcsam.eCMD.EXPOTIME, out val);
            //trackBar1.Value = val;
            label1.Text = val.ToString() + " ms";
        }
    }
}
