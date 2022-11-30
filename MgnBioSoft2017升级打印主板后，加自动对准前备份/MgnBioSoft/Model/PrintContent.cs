using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;

namespace MGNBIO.Model
{
    public class PrintContent
    {
        private int id = 0;

        public int Id
        {
            get { return id; }
            set { id = value; }
        }
        private byte[] print_array_a;

        public byte[] Print_array_a
        {
            get { return print_array_a; }
            set { print_array_a = value; }
        }

        private byte[] print_array_t;

        public byte[] Print_array_t
        {
            get { return print_array_t; }
            set { print_array_t = value; }
        }
        private byte[] print_array_c;

        public byte[] Print_array_c
        {
            get { return print_array_c; }
            set { print_array_c = value; }
        }
        private byte[] print_array_g;

        public byte[] Print_array_g
        {
            get { return print_array_g; }
            set { print_array_g = value; }
        }

        private int loop_index = 0;

        /// <summary>
        /// 活化剂
        /// </summary>
        private byte[] print_array_h;

        public byte[] Print_array_h
        {
            get { return print_array_h; }
            set { print_array_h = value; }
        }

        private bool a_finish;

        public bool A_finish
        {
            get { return a_finish; }
            set { a_finish = value; }
        }
        private bool t_finish;

        public bool T_finish
        {
            get { return t_finish; }
            set { t_finish = value; }
        }
        private bool c_finish;

        public bool C_finish
        {
            get { return c_finish; }
            set { c_finish = value; }
        }
        private bool g_finish;

        public bool G_finish
        {
            get { return g_finish; }
            set { g_finish = value; }
        }
        
        /// <summary>
        /// 是否洗脱完成
        /// </summary>
        private bool elution_finish;

        public bool Elution_finish
        {
            get { return elution_finish; }
            set { elution_finish = value; }
        }

        /// <summary>
        /// 是否喷活化剂完成
        /// </summary>
        private bool activator_finish;

        public bool Activator_finish
        {
            get { return activator_finish; }
            set { activator_finish = value; }
        }

        private int x_offset;

        public int X_offset
        {
            get { return x_offset; }
            set { x_offset = value; }
        }
        private int y_offset;

        public int Y_offset
        {
            get { return y_offset; }
            set { y_offset = value; }
        }

        public int Loop_index { get => loop_index; set => loop_index = value; }
    }
}
