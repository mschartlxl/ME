using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;


namespace MGNBIO.Communication
{
    public static class EnumClass
    {

        public static int JYT_RESULT_ERROR = 100;
        public static int JYT_RESULT_SUCCESS =  0;
        public static int JYT_RESULT_TIMEOUT = 1;
        public static int NUM_CHILD_HEADS_MAX = 16;
        public static int NUM_JOB_CHANNELS_MAX = 16;

        public static int PRINTENGINE_RESULT_SEND_DATA_ERROR = -1;
        public static int PRINTENGINE_RESULT_MAINBOARD_REPEAT = -2;
        public static int PRINTENGINE_RESULT_HEADLAYOUT_ERROR = -3;
        public static int PRINTENGINE_RESULT_INVALID_PARAMETER = -4;
        public static int PRINTENGINE_RESULT_NOT_RUNNING = -5;
        public static int PRINTENGINE_RESULT_HEADCOLORCHANNEL_ERROR = -6;
    }

    //enum ColorMode
    //{
    //    K = 1,
    //    CMYK = "K|C|M|Y"
    //}
    enum HeadType
    {
        Gen5 = 1,
        Gen6 = 2,
        Gen5s = 3,
        Gen5i = 4,
        Gen4 = 5,
        GenKYStandard = 6,
        KYHighSpeed = 7,
        KY300 = 8
    }
    
    //enum MainBoardType
    //{
    //    Eth_Gen5_4H = 1,
    //    Eth_Gen5_1H = 2
    //}
    /// <summary>
    /// 颜色数量
    ///警告
    ///最多支持16个颜色
    /// </summary>
    enum ColorChannel
    {
        K = 0x01, C = 0x02, M = 0x04, Y = 0x08,
        White = 0x10, Varnish = 0x20}

    //enum ColorMode { K = 0x01, CMYK = 0x0F }

    ////enum MainBoardType { Eth_Gen5_4H = 2, Eth_Gen5_1H = 3 }

    //enum HeadType
    //{
    //    Gen5 = 0, Gen6 = 1, Gen5s = 2, Gen5i = 3,
    //    Gen4 = 4, KYStandard = 8, KYHighSpeed = 9, KY300 = 10
    //}


    //enum ColorChannel
    //{
    //    K = 0x01,
    //    C = 0x02,
    //    M = 0x04,
    //    Y = 0x08,
    //    White = 0x10,
    //    Varnish = 0x20,
    //}

    //enum ColorMode
    //{
    //    K = 0x01,
    //    CMYK = 0x0F,
    //}

    //struct JobInfo
    //{

    //    int width = 4200;

    //    int height = 7200;

    //    int bits_per_pixel = 1;

    //    int num_pages = 1;

    //    int x_reslution = 600;

    //    int y_reslution = 600;


    //    int num_channels = 1;

    //    //ColorChannel channels[16] = { ColorChannel::K };
    //}

    enum MainBoardType
    {
        Eth_Gen5_4H = 2,
        Eth_Gen5_1H = 3
    }

    //enum HeadType
    //{
    //    Gen5 = 0,
    //    Gen6 = 1,
    //    Gen5s = 2,
    //    Gen5i = 3,
    //    Gen4 = 4,

    //    KYStandard = 8,
    //    KYHighSpeed = 9,
    //    KY300 = 10
    //}

    enum PrintDirection
    {
        Forward = 0,
        Reverse = 1,
        Bothway = 2
    };

    enum PageStartEdgeSel
    {
        Negedge = 0,
        Posedge = 1
    }

    enum EncoderPageStartEn
    {
        External,
        ExternalWithPageStartCompensation,
        Mixed,
        Internal,
        InternalOnlyPageStart,
        InternalOnlyEncoder
    }

    enum TakeLineMode
    {
        All,            // 截取全部行
        Even,           // 截取偶数行
        Odd            // 截取奇数行
    }

    enum WaveformKind
    {
        Small,
        Middle,
        Large
    }

    //struct PhysicalHead
    //{
    //    //HeadType head_type = HeadType::Gen5;


    //    int x_offset = 0;


    //    int y_offset = 500;

    //    bool reverse = false;


    //    int num_child_heads = 2;

    //    //bool child_enables[16] = { true, true };

    //    //ColorChannel child_channels[16] = { ColorChannel::K, ColorChannel::K };

    //    int num_disable_tail_dots = 0;

    //    bool duplex_speed = false;


    //    //TakeLineMode take_line_mode = TakeLineMode::All;
    //}

    //struct PrintEnvironment
    //{

    //    int encoder_fil_factor = 9;


    //    int pagestart_fil_factor = 9;

    //    //PrintDirection print_direction = PrintDirection::Bothway;


    //    int encoder_resolution = 4800;


    //    int encoder_adjust_factor = 1024;

    //    //PageStartEdgeSel pagestart_edge_sel = PageStartEdgeSel::Posedge;


    //    int pagestart_mask_distance = 1000;


    //    int sim_pagestart_cycle = 1000;


    //    int cue_tolernce = 1000;

    //    int pagestart_compensation_max_num = 5;

    //    //EncoderPageStartEn encoder_page_start_en = EncoderPageStartEn::External;


    //    bool duplex_speed = false;


    //    bool debug_dump_enabled = false;


    //    float sim_move_speed = 100;

    //    //int velocity_calibration_segments[16]{};

    //    //int encoder_adjust_segments[16]{};


    //    int speed_limitation = 6;


    //    bool endless_pages = false;

    //    //float board_cache_use_ratio = 1.0;

    //    //WaveformKind waveform_kind = WaveformKind::Small;


    //    int num_page_buffer_totals = 10;


    //    int num_logic_head_buffer_totals = 100;
    //}

    enum StopCode
    {
        None,
        Finish,
        Abort,
        Error,
    }



}
