using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;
using MGNBIO;
using System.Reflection;
using System.Diagnostics;
using System.IO;
using System.Collections;
using MGNBIO.Model;
using MGNBIO.Common;

namespace MGNChipMatrix
{
    public partial class FormChipMatrix : Form
    {
        public FormChipMatrix()
        {
            //设置窗体的双缓冲
            this.SetStyle(ControlStyles.OptimizedDoubleBuffer | ControlStyles.ResizeRedraw | ControlStyles.AllPaintingInWmPaint, true);
            this.UpdateStyles();

            InitializeComponent();

            //
            //利用反射设置DataGridView的双缓冲
            Type dgvType = this.dataGridView1.GetType();
            PropertyInfo pi = dgvType.GetProperty("DoubleBuffered",
            BindingFlags.Instance | BindingFlags.NonPublic);
            pi.SetValue(this.dataGridView1, true, null);
        }

        public static int row_g = 40;
        public static int column_g = 0;
        public static int space_g = 2;

        public static int[,] micro_array_g = new int[2, 3];

        //int[,] twoDArray = new int[10, 10];

        private void toolStripMenuItem2_Click(object sender, EventArgs e)
        {
            FrmaAddMatrix f4 = new FrmaAddMatrix();
            if (f4.ShowDialog() == DialogResult.OK)
            {
                string row = f4.row;
                string column = f4.column;
                string space = f4.space;
                Console.Write("column:" + column + "row:" + row + "space:" + space);
                int row1 = (row == "") ? 1 : int.Parse(row);
                int column1 = (column == "") ? 1 : int.Parse(column);
                int space1 = (space == "") ? 1 : int.Parse(space);
                GetDGVData(row1, column1, space1);
            }
        }

        private void Form1_Load(object sender, EventArgs e)
        {
            string row_number = ucTextBoxExRows.InputText.ToString();
            row_g = int.Parse(row_number);//

            GetDGVData(row_g, 10, 0);
        }


        //添加一行
        public void getDataFromList()
        {

            this.dataGridView1.DataSource = null;
            this.dataGridView1.Rows.Clear();
            for (int index = 0; index < 10000; index++)
            {
                index = this.dataGridView1.Rows.Add();
                this.dataGridView1.Rows[index].Cells[0].Value = index;
                this.dataGridView1.Rows[index].Cells[1].Value = index;
                this.dataGridView1.Rows[index].Cells[2].Value = index;
            }
        }

        private void GetDGVData(int row,int column,int space)
        {
            row_g = row;
            column_g = column;
            space_g = space;

            micro_array_g = new int[row,column];



            dataGridView1.DataSource = null;
            dataGridView1.Rows.Clear();
            //把 EnableHeaderVisualStyles 设置为false才可以。
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;//211, 223, 240
            dataGridViewCellStyle2.ForeColor = Color.Red;
            dataGridView1.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;

            dataGridView1.AllowUserToAddRows = false;

            dataGridView1.AllowUserToDeleteRows = false;

            dataGridView1.ReadOnly = true;

            //dataGridView1.RowHeadersVisible = false;
            dataGridView1 = DynamicCreationDGV(dataGridView1, row, column, space);
        }

        private DataGridView DynamicCreationDGV(DataGridView dgv,int row,int column,int space)
        {
            if (dgv == null)
                return null;
            if (dgv.ColumnCount != 0)
                dgv.Columns.Clear();    //清除现有列
            #region 组织 DataGridViewColumn
            for (int i = 1; i < column + 1; i++) //指定总列数
            {
                //列名和列头名
                string dgvColName = "" + i;
                string dgvColHeaderText = string.Empty;
                DataGridViewColumn dgvcolumn = new DataGridViewTextBoxColumn();    //列默认类型
                dgvcolumn.ValueType = typeof(string);           //列数据类型默认字符串
                dgvcolumn.HeaderText = dgvColName;
                dgvcolumn.Width = 50;
                dgvcolumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv.Columns.Add(dgvcolumn); //增加列
            }
            #endregion
            #region 组织DataGridViewRow

            //合共有多少个格
            int total_blank = 0;
            //需要显示的索引
            int show_index = 0;
            for (int i = 0; i < row; i++)    //组织指定数据行
            {
                DataGridViewRow dr = new DataGridViewRow();
                dgv.Rows.Add(dr);           //增加行
                for (int j = 0; j < dgv.ColumnCount; j++) //部分列
                {
                    if (total_blank == 0)
                    {
                        dgv.Rows[i].Cells[j].Value = "";//(i + 1) + "_" + (j + 1);
                        micro_array_g[i,j] = 0;
                    }
                    if (total_blank % (space + 1) == 0)
                    {
                        dgv.Rows[i].Cells[j].Value = "";//(i + 1) + "_" + (j + 1);
                        micro_array_g[i, j] = 0;
                    }
                    else
                    {
                        //dgv.Rows[i].Cells[j].Value = "不可用";
                        dgv.Rows[i].Cells[j].Style.BackColor = Color.CadetBlue;
                        micro_array_g[i, j] = 5;

                    }
                    total_blank++;
                }
            }

            Console.Write(total_blank);
            #endregion
            return dgv;
        }


        private DataGridView DynamicCreationDGV1(DataGridView dgv, int row, int column, int space,List<string> a)
        {
            int max_length = a.Select(c => c.Length).Max();


            micro_array_g = new int[row, column];
            if (dgv == null)
                return null;
            if (dgv.ColumnCount != 0)
                dgv.Columns.Clear();    //清除现有列
            #region 组织 DataGridViewColumn
            for (int i = 1; i < max_length + 1; i++) //指定总列数
            {
                //列名和列头名
                string dgvColName = "" + i;
                string dgvColHeaderText = string.Empty;
                DataGridViewColumn dgvcolumn = new DataGridViewTextBoxColumn();    //列默认类型
                dgvcolumn.ValueType = typeof(string);           //列数据类型默认字符串
                dgvcolumn.HeaderText = dgvColName;
                dgvcolumn.Width = 50;
                dgvcolumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv.Columns.Add(dgvcolumn); //增加列
            }
            #endregion
            #region 组织DataGridViewRow

            //合共有多少个格
            int total_blank = 0;
            //需要显示的索引
            int show_index = 0;

            for (int i = 0; i < a.Count; i++)    //组织指定数据行
            {
                DataGridViewRow dr = new DataGridViewRow();
                dgv.Rows.Add(dr);           //增加行
                for (int j = 0; j < max_length; j++) //部分列
                {
                    if (total_blank == 0)
                    {
                        var value = a[i].ToArray()[j];
                        if (value=='0')
                        {
                            dgv.Rows[i].Cells[j].Value = 0;//(i + 1) + "_" + (j + 1);
                            //micro_array_g[i, j] = 0;
                        }
                        else
                        {
                            dgv.Rows[i].Cells[j].Value = 1;//(i + 1) + "_" + (j + 1);
                            //micro_array_g[i, j] = 1;
                        }
                    }
                    if (total_blank % (space + 1) == 0)
                    {
                        var value = a[i].ToArray()[j];
                        if (value=='0')
                        {
                            dgv.Rows[i].Cells[j].Value = 0;//(i + 1) + "_" + (j + 1);
                            //micro_array_g[i, j] = 0;
                        }
                        else
                        {
                            dgv.Rows[i].Cells[j].Value = 1;//(i + 1) + "_" + (j + 1);
                            //micro_array_g[i, j] = 1;
                        }
                    }
                    else
                    {
                        //dgv.Rows[i].Cells[j].Value = "不可用";
                        dgv.Rows[i].Cells[j].Style.BackColor = Color.CadetBlue;
                        //micro_array_g[i, j] = 5;
                    }
                    total_blank++;
                }
            }

            Console.Write(total_blank);
            #endregion
            return dgv;
        }


        #region dataGridView1显示行号
        private void dataGridView1_RowPostPaint_1(object sender, DataGridViewRowPostPaintEventArgs e)
        {
            Rectangle rectangle = new Rectangle(e.RowBounds.Location.X,
                                    e.RowBounds.Location.Y,
                                    dataGridView1.RowHeadersWidth - 4,
                                    e.RowBounds.Height);
            Graphics hdc = e.Graphics;
            TextRenderer.DrawText(hdc, (e.RowIndex + 1).ToString(),
            dataGridView1.RowHeadersDefaultCellStyle.Font,
            rectangle,
            Color.Red,
            TextFormatFlags.VerticalCenter | TextFormatFlags.HorizontalCenter);
        }
        #endregion

        private void dataGridView1_ColumnAdded(object sender, DataGridViewColumnEventArgs e)
        {
             e.Column.FillWeight = 2; 
        }

        private void ucBtnImg5_BtnClick(object sender, EventArgs e)
        {
            //int row = (textBoxRow.Text == "") ? 1 : int.Parse(textBoxRow.Text);
            //int column = (textBoxColumn.Text == "") ? 1 : int.Parse(textBoxColumn.Text);
            //int space = (textBoxSpace.Text == "") ? 1 : int.Parse(textBoxSpace.Text);
            //GetDGVData(row, column, space);
        }

        private void dataGridView1_Paint(object sender, PaintEventArgs e)
        {
            //int rowHeight = this.dataGridView1.Height;

            //int h = dataGridView1.ColumnHeadersHeight + rowHeight * dataGridView1.RowCount;
            //int imgWidth = this.Width - 2;
            //Rectangle rFrame = new Rectangle(0, 0, imgWidth, rowHeight);
            //Rectangle rFill = new Rectangle(1, 1, imgWidth - 2, rowHeight);
            //Rectangle rowHeader = new Rectangle(2, 2, dataGridView1.RowHeadersWidth - 3, rowHeight);

            //Pen pen = new Pen(dataGridView1.GridColor, 1);

            //Bitmap rowImg = new Bitmap(imgWidth, rowHeight);
            //Graphics g = Graphics.FromImage(rowImg);
            //g.DrawRectangle(pen, rFrame);
            //g.FillRectangle(new SolidBrush(dataGridView1.DefaultCellStyle.BackColor), rFill);
            //g.FillRectangle(new SolidBrush(dataGridView1.RowHeadersDefaultCellStyle.BackColor), rowHeader);

            //Bitmap rowImgAAlternative = rowImg.Clone() as Bitmap;
            //Graphics g2 = Graphics.FromImage(rowImgAAlternative);
            //rFill.X += dataGridView1.RowHeadersWidth - 1;
            //g2.FillRectangle(new SolidBrush(dataGridView1.AlternatingRowsDefaultCellStyle.BackColor), rFill);

            //int w = dataGridView1.RowHeadersWidth - 1;
            //for (int j = 0; j < dataGridView1.ColumnCount; j++)
            //{
            //    g.DrawLine(pen, new Point(w, 0), new Point(w, rowHeight));
            //    g2.DrawLine(pen, new Point(w, 0), new Point(w, rowHeight));
            //    w += dataGridView1.Columns[j].Width;
            //}

            //int loop = (this.Height - h) / rowHeight;
            //for (int j = 0; j < loop + 1; j++)
            //{
            //    int index = dataGridView1.RowCount + j;
            //    if (index % 2 == 0)
            //    {
            //        e.Graphics.DrawImage(rowImg, 1, h + j * rowHeight);
            //    }
            //    else
            //    {
            //        e.Graphics.DrawImage(rowImgAAlternative, 1, h + j * rowHeight);
            //    }
            //}
        }

        private void button1_Click(object sender, EventArgs e)
        {
            List<string> sequencere_list = ReadTxtToList("打印序列.txt");

            if (sequencere_list == null || sequencere_list.Count == 0)
            {
                return;
            }

            string tmp1 = "";
            for (int i = 0; i < sequencere_list.Count; i++)
            {
                //Console.Write(sequencere_list[i].Length + "\r\n");
                tmp1 += sequencere_list[i].ToArray()[0];
            }
            Console.Write(tmp1.Length);
            //Test(sequencere_list);
            //sequencere_list.FindIndex(o=>o.
        }

        public string ListTurnToString(List<string> sequencere_list,int index)
        {
            string total_string = "";
            for (int j = 0; j < sequencere_list.Count; j++)
            {
                total_string += sequencere_list[j].ToArray()[index];
            }
            return total_string;

        }
        public List<char> mList_g = new List<char>();

        public List<string> mList_string_g = new List<string>();

        public string Test(List<string> sequencere_list)
        {
            mList_string_g = new List<string>();

            int max_length = sequencere_list.Select(c => c.Length).Max();
            string total_string = "";
            for (int i = 0; i < max_length; i++)
            {
                total_string += ListTurnToString(sequencere_list, i);
            }

            //阵列转换
            int xulie_length = 0;
            if (row_g < 320)
            {
                xulie_length = row_g;
            }
            else
            {
                xulie_length = 320;
            }

            string total_string_add_space = "";

            string space_string = "";

            space_g = int.Parse(ucTextBoxExSpace.InputText.ToString());

            for (int j = 0; j < space_g; j++)
            {
                space_string += "S";
            }

            if (space_g > 0)
            {
                for (int i = 0; i < total_string.Length; i++)
                {
                    total_string_add_space += total_string.Substring(i, 1) + space_string;
                }
            }
            else
            {
                total_string_add_space = total_string;
            }

            //List<string> tmp_list = new List<string>();
            for (int i = 0; i < total_string_add_space.Length; i++)
            {
                if (total_string_add_space.Length - i < xulie_length)
                {
                    int a = total_string_add_space.Length - i;
                    mList_string_g.Add(total_string_add_space.Substring(i, a));
                }
                else
                {
                    mList_string_g.Add(total_string_add_space.Substring(i, xulie_length));
                }
                i += xulie_length; 
            }

            mList_g = total_string_add_space.ToList();
            return total_string_add_space;
        }


        public string TestAddSpace(List<string> sequencere_list)
        {
            int max_length = sequencere_list.Select(c => c.Length).Max();
            string total_string = "";
            for (int i = 0; i < max_length; i++)
            {
                total_string += ListTurnToString(sequencere_list, i);
            }

            //阵列转换
            int xulie_length = 0;
            if (row_g < 320)
            {
                xulie_length = row_g;
            }
            else
            {
                xulie_length = 320;
            }

            string total_string_add_space = "";

            string space_string = "";
            for (int j = 0; j < space_g; j++)
            {
                space_string += "";
            }

            if (space_g > 0)
            {
                for (int i = 0; i < total_string.Length; i++)
                {
                    total_string_add_space += total_string.Substring(i, 1) + space_string;
                }
            }
            else
            {
                total_string_add_space = total_string;
            }

            //List<string> tmp_list = new List<string>();
            for (int i = 0; i < total_string_add_space.Length; i++)
            {
                if (total_string_add_space.Length - i < xulie_length)
                {
                    int a = total_string_add_space.Length - i;
                    mList_string_g.Add(total_string_add_space.Substring(i, a));
                }
                else
                {
                    mList_string_g.Add(total_string_add_space.Substring(i, xulie_length));
                }
                i += xulie_length;
            }

            mList_g = total_string_add_space.ToList();
            return total_string_add_space;
        }

        //读取txt文件中总行数的方法
        public static int ReadToatalLine(String _fileName)
        {
            Stopwatch sw = new Stopwatch();
            var path = _fileName;
            int lines = 0;
            //按行读取
            sw.Restart();
            using (var sr = new StreamReader(path))
            {
                var ls = "";
                while ((ls = sr.ReadLine()) != null)
                {
                    lines++;
                }
            }
            sw.Stop();
            return lines;
        }

        public static int ReadLineStringMaxLength(String _fileName)
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
                    ls = ls.Replace("\t", "");
                    sz_list.Add(ls.Split(':')[1]);
                }
            }
            int max_length = sz_list.Select(c => c.Length).Max();
            sw.Stop();
            return max_length;
        }
        public static int line_max_length_g = 0;
        public List<string> ReadTxtToList(String _fileName)
        {
            int line_max_length = ReadLineStringMaxLength(_fileName);
            line_max_length_g = line_max_length;
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
                    ls = ls.Replace("\t", "");
                    int ls_length = ls.Split(':')[1].Length;
                    if (ls_length < line_max_length)
                    {
                        for (int i = 0; i < line_max_length - ls_length; i++)
                        {
                            ls += "X";
                        }
                    }
                    sz_list.Add(ls.Split(':')[1]);
                }
            }
            sw.Stop();
            return sz_list;
        }

        private void AToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //GetPrintList();
            //return;

            //List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "A");
            //int max_length = tmp_list.Select(c => c.Length).Max();
            ////最后一行补上0
            //int last_line_length = tmp_list[tmp_list.Count - 1].Length;
            //if (last_line_length < max_length)
            //{
            //    for (int i = 0; i < (max_length - last_line_length); i++)
            //    {
            //        tmp_list[tmp_list.Count - 1] += "0";
            //    }
            //}
            //int byte_array_length = (tmp_list.Count * 320) / 8;
            //byte[] data = new byte[byte_array_length];
            ////data = BitToByteArray(tmp_list,byte_array_length);
            //DynamicCreationDGV1(dataGridView1,10,13,0,tmp_list); 
        }

        public List<PrintContent> GetPrintList(List<string> one_layout_string_list)
        {
            List<PrintContent> print_content_list = new List<PrintContent>();


            List<byte[]> tmp_listA = ConvertToPrintOrder(one_layout_string_list, "A");
            List<byte[]> tmp_listT = ConvertToPrintOrder(one_layout_string_list, "T");
            List<byte[]> tmp_listC = ConvertToPrintOrder(one_layout_string_list, "C");
            List<byte[]> tmp_listG = ConvertToPrintOrder(one_layout_string_list, "G");





            for (int i = 0; i < tmp_listA.Count; i++)
            {
                //string one_page_print_data_a = "";
                //string one_page_print_data_g = "";
                //string one_page_print_data_c = "";
                //string one_page_print_data_t = "";

                //one_page_print_data_a = tmp_listA[i];
                //one_page_print_data_t = tmp_listT[i];
                //one_page_print_data_c = tmp_listC[i];
                //one_page_print_data_g = tmp_listG[i];

                byte[] total_a_array_t1  = tmp_listT[i];//BitToByteArray(one_page_print_data_t, "T");
                byte[] total_a_array_a1  = tmp_listA[i];//BitToByteArray(one_page_print_data_a, "A");
                byte[] total_a_array_c1  = tmp_listC[i];//BitToByteArray(one_page_print_data_c, "C");
                byte[] total_a_array_gg1 = tmp_listG[i];//BitToByteArray(one_page_print_data_g, "G");
                byte[] total_act_array = new byte[total_a_array_gg1.Length];

                //算出ACT需要喷的矩阵值
                for (int ivalue = 0; ivalue < total_a_array_a1.Length; ivalue++)
                {
                    int a1 = total_a_array_a1[ivalue];
                    int t1 = total_a_array_t1[ivalue];
                    int c1 = total_a_array_c1[ivalue];
                    int gg1 = total_a_array_gg1[ivalue];
                    total_act_array[ivalue] = (byte)(a1 | t1 | c1 | gg1);
                }

                PrintContent print_content = new PrintContent();
                print_content.Print_array_a = total_a_array_a1;
                print_content.Print_array_t = total_a_array_t1;
                print_content.Print_array_c = total_a_array_c1;
                print_content.Print_array_g = total_a_array_gg1;
                print_content.Print_array_h = total_act_array;
                print_content.Loop_index = i;
                print_content.Id = print_content.Id + i;
                print_content_list.Add(print_content);

            }


            /*
            //byte[] tmp_a_array 
            byte[] total_a_array_a = BitToByteArray(tmp_listA, "A");
            byte[] total_a_array_t = BitToByteArray(tmp_listT, "T");
            byte[] total_a_array_c = BitToByteArray(tmp_listC, "C");
            byte[] total_a_array_gg = BitToByteArray(tmp_listG, "G");

            //一行打几个点
            int column_number = column_number_g;
            int line_number = row_g;
            int loop_index = 0;

            for (int i = 0; i < total_a_array_a.Length; i+=line_number)
            {
                loop_index = i / column_number;
                Console.WriteLine(loop_index);
                byte[] print_array_a = new byte[line_number];
                byte[] print_array_t = new byte[line_number];
                byte[] print_array_c = new byte[line_number];
                byte[] print_array_gg = new byte[line_number];

                //保证不溢出
                if ((i + line_number) < total_a_array_a.Length || (i + line_number) == total_a_array_a.Length)
                {
                    for (int j = 0; j < line_number; j++)
                    {
                        print_array_a[j] = total_a_array_a[i + j];
                        print_array_t[j] = total_a_array_t[i + j];
                        print_array_c[j] = total_a_array_c[i + j];
                        print_array_gg[j] = total_a_array_gg[i + j];
                    }

                    PrintContent print_content = new PrintContent();
                    print_content.Print_array_a = print_array_a;
                    print_content.Print_array_t = print_array_t;
                    print_content.Print_array_c = print_array_c;
                    print_content.Print_array_g = print_array_gg;
                    print_content.Id = print_content.Id + i;
                    print_content.Loop_index = loop_index;
                    print_content_list.Add(print_content);

                }else
                {
                    //剩下的部分也要补全了打印出来
                    int b = i + line_number;
                    if (i < total_a_array_a.Length)
                    {
                        int leftover_number = total_a_array_a.Length - i;
                        int need_number = line_number - leftover_number;
                        for (int j = 0; j < leftover_number; j++)
                        {
                            print_array_a[j] = total_a_array_a[i + j];
                            print_array_t[j] = total_a_array_t[i + j];
                            print_array_c[j] = total_a_array_c[i + j];
                            print_array_gg[j] = total_a_array_gg[i + j];
                        }
                        for (int j = 0; j < need_number; j++)
                        {
                            print_array_a[leftover_number + j] = 0;//total_a_array_a[i + j];
                            print_array_t[leftover_number + j] = 0;//total_a_array_t[i + j];
                            print_array_c[leftover_number + j] = 0;//total_a_array_c[i + j];
                            print_array_gg[leftover_number + j] = 0;//total_a_array_g[i + j];
                        }
                        PrintContent print_content = new PrintContent();
                        print_content.Print_array_a = print_array_a;
                        print_content.Print_array_t = print_array_t;
                        print_content.Print_array_c = print_array_c;
                        print_content.Print_array_g = print_array_gg;
                        print_content.Loop_index = loop_index;
                        print_content.Id = print_content.Id + i;
                        print_content_list.Add(print_content);
                    }
                }
            }

            loop_index = 0;
            print_content_list = new List<PrintContent>();

            for (int i = 0; i < total_a_array_a.Length; i += column_number)
            {
                loop_index = i / column_number;

                byte[] print_array_a = new byte[column_number];
                byte[] print_array_t = new byte[column_number];
                byte[] print_array_c = new byte[column_number];
                byte[] print_array_gg = new byte[column_number];


                for (int j = 0; j < column_number; j++)
                {
                    print_array_a[j] = total_a_array_a[i + j];
                    print_array_t[j] = total_a_array_t[i + j];
                    print_array_c[j] = total_a_array_c[i + j];
                    print_array_gg[j] = total_a_array_gg[i + j];
                }

                PrintContent print_content = new PrintContent();
                print_content.Print_array_a = print_array_a;
                print_content.Print_array_t = print_array_t;
                print_content.Print_array_c = print_array_c;
                print_content.Print_array_g = print_array_gg;
                print_content.Loop_index = loop_index;
                print_content.Id = print_content.Id + i;
                print_content_list.Add(print_content);

            }
            */



            //byte[] aa_array = new byte[160];

            //byte[] aaadff = print_content_list[0].Print_array_a;

            //int aadd = 0;
            //for (int i = 0; i < tmp_listA[0].Length; i += 160)
            //{
            //    Array.Copy(aaadff, aadd * 160, aa_array, 0, 160);

            //    string str = System.Text.Encoding.Unicode.GetString(aa_array);

            //    Console.WriteLine(aadd + ": " + str);

            //    aadd++;
            //    //Array.Copy(aaadff, (i * 160), aa_array, 160);
            //}






            //把处理完的数据存储起来，供打印窗口调用
            if (print_content_list.Count > 0)
            {
                CommonData.PrintContentList_g = print_content_list;
                CommonData.Lin_number = print_content_list.Count;
                CommonData.Line_point_number = print_content_list[0].Print_array_a.Length;
            }
            MessageBox.Show("已完成矩阵转换");
            return print_content_list;


        }



        public byte[] BitToByteArray(string one_page_print_data, string type)
        {
            byte[] data = new byte[one_page_print_data.Length];
            //间隔数量
            int space_number = space_g;

            string[] print_array = stringInArrays(one_page_print_data);

            switch (space_number)
            {
                case 1:
                    data = new byte[print_array.Length * 2];
                    break;
                case 2:
                    data = new byte[print_array.Length * 3];

                    int loop_number = 0;
                    for (int i = 0; i < print_array.Length; i += 4)
                    {
                        if ((i + 1) < print_array.Length)
                        {
                            string tmp1 = print_array[i];
                            string tmp2 = print_array[i + 1];
                            string tmp3 = print_array[i + 2];
                            string tmp4 = print_array[i + 3];

                            int byte1 = 0;//new byte();
                            int byte2 = 0;// new byte();
                            int byte3 = 0;// new byte();
                            int byte4 = 0;// new byte();
                            
                            if(loop_number%2==0)

                            if (tmp1.Equals("1"))
                            {
                                byte1 = 0b10000000;
                            }
                            else
                            {
                                byte1 = 0;
                            }

                            if (tmp2.Equals("1"))
                            {
                                byte2 = 0b00001000;
                            }
                            else
                            {
                                byte2 = 0;
                            }


                            /*
                            if (space_number > 0)
                            {
                                if (space_number == 1)
                                {
                                    if (tmp1.Equals("1"))
                                    {
                                        byte1 = 0b10000000;
                                    }
                                    else
                                    {
                                        byte1 = 0;
                                    }
                                    if (tmp2.Equals("1"))
                                    {
                                        byte2 = 0b00001000;
                                    }
                                    else
                                    {
                                        byte2 = 0;
                                    }
                                    data[i] = (byte)(byte1 | byte2);
                                }

                                if (space_number == 2)
                                {

                                }

                                if (space_number == 3)
                                {

                                }

                                if (space_number == 4)
                                {

                                }



                            }
                            else
                            {
                                if (tmp1.Equals("1"))
                                {
                                    byte1 = 0b10000000;
                                }
                                else
                                {
                                    byte1 = 0;
                                }
                                if (tmp2.Equals("1"))
                                {
                                    byte2 = 0b00001000;
                                }
                                else
                                {
                                    byte2 = 0;
                                }
                                data[i] = (byte)(byte1 | byte2);
                            }
                            */
                            loop_number++;
                        }
                    }





                    break;
                case 3:
                    data = new byte[print_array.Length * 4];
                    break;
                case 4:
                    data = new byte[print_array.Length * 5];
                    break;
            }





            /*
            for (int i = 0; i < print_array.Length; i += 8)
            {
                if ((i + 1) < print_array.Length)
                {
                    string tmp1 = print_array[i];
                    string tmp2 = print_array[i + 1];
                    string tmp3 = print_array[i + 2];
                    string tmp4 = print_array[i + 3];
                    string tmp5 = print_array[i + 4];
                    string tmp6 = print_array[i + 5];
                    string tmp7 = print_array[i + 6];
                    string tmp8 = print_array[i + 7];

                    if (tmp1.Equals("1"))
                    {
                        data[i] = 0b10000000;
                    }
                    else
                    {
                        data[i] = 0;
                    }

                    if (tmp2.Equals("1"))
                    {
                        data[i + 1] = 0b01000000;
                    }
                    else
                    {
                        data[i + 1] = 0;
                    }

                    if (tmp3.Equals("1"))
                    {
                        data[i + 2] = 0b00100000;
                    }
                    else
                    {
                        data[i + 2] = 0;
                    }

                    if (tmp4.Equals("1"))
                    {
                        data[i + 3] = 0b00010000;
                    }
                    else
                    {
                        data[i + 3] = 0;
                    }

                    if (tmp5.Equals("1"))
                    {
                        data[i + 4] = 0b00001000;
                    }
                    else
                    {
                        data[i + 4] = 0;
                    }

                    if (tmp6.Equals("1"))
                    {
                        data[i + 5] = 0b00000100;
                    }
                    else
                    {
                        data[i + 5] = 0;
                    }

                    if (tmp7.Equals("1"))
                    {
                        data[i + 6] = 0b00000010;
                    }
                    else
                    {
                        data[i + 6] = 0;
                    }

                    if (tmp8.Equals("1"))
                    {
                        data[i + 7] = 0b00000001;
                    }
                    else
                    {
                        data[i + 7] = 0;
                    }

                    //if (tmp1.Equals("1"))
                    //{
                    //    data[i] = 0x80;
                    //}
                    //else
                    //{
                    //    data[i] = 0x00;
                    //}

                    //if (tmp2.Equals("1"))
                    //{
                    //    if (tmp1.Equals("0"))
                    //    {
                    //        data[i + 1] = 0x08;
                    //    }
                    //    else
                    //    {
                    //        data[i + 1] = 0x80;
                    //    }
                    //}
                    //else
                    //{
                    //    data[i + 1] = 0x00;
                    //}
                    //if (tmp1.Equals("1") && tmp2.Equals("1"))
                    //{
                    //    data[i] = 0x88;
                    //    data[i + 1] = 0x00;
                    //}



                }
                else
                {
                    string tmp1 = print_array[i];
                    //最后一个数据处理
                }
                //if (print_array[i].Equals("1"))
                //{
                //    data[i] = 0x80;
                //}else if(print_array[i].Equals("1"))
                //else
                //{
                //    data[i] = 0x00;
                //}


            }


            */

            return data;
        }


        public byte[] BitToByteArray(List<string> tmp_list, string type)
        {
            int max_length = tmp_list.Select(c => c.Length).Max();
            //最后一行补上0
            int last_line_length = tmp_list[tmp_list.Count - 1].Length;
            if (last_line_length < max_length)
            {
                for (int i = 0; i < (max_length - last_line_length); i++)
                {
                    tmp_list[tmp_list.Count - 1] += "0";
                }
            }
            int line_point = 160;  //最大为320

            string row_number = ucTextBoxExRows.InputText.ToString();
            row_g = int.Parse(row_number);//每一排的碱基点数
            line_point = row_g;

            int byte_array_length = (tmp_list.Count * line_point) / 8;
            byte[] data = new byte[byte_array_length];

            int index = 0;
            string tmp_list_total = "";
            for (int i = 0; i < tmp_list.Count; i++)
            {
                tmp_list_total += tmp_list[i];
                /*
                for (int j = 0; j < tmp_list[i].Length; j=j+8)
                {
                    string bit8 = tmp_list[i].Substring(j, 8);


                    int value = Convert.ToInt32(bit8, 2);

                    value = PrintValueChange(bit8,type);

                    data[index] = (byte)value;
                    if((index+1) % 40 ==0 && index !=0)
                    {
                        Console.Write("----------" + index + "\r\n");
                    }
                    index++;
                }
                */
            }

            string[]  print_array = stringInArrays(tmp_list_total);
            data = new byte[print_array.Length];
            for (int i = 0; i < print_array.Length; i+=8)
            {
                if ((i + 1) < print_array.Length)
                {
                    string tmp1 = print_array[i];
                    string tmp2 = print_array[i + 1];
                    string tmp3 = print_array[i + 2];
                    string tmp4 = print_array[i + 3];
                    string tmp5 = print_array[i + 4];
                    string tmp6 = print_array[i + 5];
                    string tmp7 = print_array[i + 6];
                    string tmp8 = print_array[i + 7];

                    if (tmp1.Equals("1"))
                    {
                        data[i] = 0b10000000;
                    }
                    else
                    {
                        data[i] = 0;
                    }

                    if (tmp2.Equals("1"))
                    {
                        data[i + 1] = 0b01000000;
                    }
                    else
                    {
                        data[i + 1] = 0;
                    }

                    if (tmp3.Equals("1"))
                    {
                        data[i + 2] = 0b00100000;
                    }
                    else
                    {
                        data[i + 2] = 0;
                    }

                    if (tmp4.Equals("1"))
                    {
                        data[i + 3] = 0b00010000;
                    }
                    else
                    {
                        data[i + 3] = 0;
                    }

                    if (tmp5.Equals("1"))
                    {
                        data[i + 4] = 0b00001000;
                    }
                    else
                    {
                        data[i + 4] = 0;
                    }

                    if (tmp6.Equals("1"))
                    {
                        data[i + 5] = 0b00000100;
                    }
                    else
                    {
                        data[i + 5] = 0;
                    }

                    if (tmp7.Equals("1"))
                    {
                        data[i + 6] = 0b00000010;
                    }
                    else
                    {
                        data[i + 6] = 0;
                    }

                    if (tmp8.Equals("1"))
                    {
                        data[i + 7] = 0b00000001;
                    }
                    else
                    {
                        data[i + 7] = 0;
                    }

                    //if (tmp1.Equals("1"))
                    //{
                    //    data[i] = 0x80;
                    //}
                    //else
                    //{
                    //    data[i] = 0x00;
                    //}

                    //if (tmp2.Equals("1"))
                    //{
                    //    if (tmp1.Equals("0"))
                    //    {
                    //        data[i + 1] = 0x08;
                    //    }
                    //    else
                    //    {
                    //        data[i + 1] = 0x80;
                    //    }
                    //}
                    //else
                    //{
                    //    data[i + 1] = 0x00;
                    //}
                    //if (tmp1.Equals("1") && tmp2.Equals("1"))
                    //{
                    //    data[i] = 0x88;
                    //    data[i + 1] = 0x00;
                    //}



                }
                else
                {
                    string tmp1 = print_array[i];
                    //最后一个数据处理
                }
                //if (print_array[i].Equals("1"))
                //{
                //    data[i] = 0x80;
                //}else if(print_array[i].Equals("1"))
                //else
                //{
                //    data[i] = 0x00;
                //}


            }

            return data;
        }

        public static string[] stringInArrays(string fields)
        {
            string[] field_array = new string[fields.Length];
            if (fields != null && fields.Length > 0)
            {
                for (int i = 0; i < fields.Length; i++)
                {
                    field_array[i] = fields.Substring(i, 1);
                }
            }
            return field_array;
        }

        public int PrintValueChange(string bit8,string type)
        {
            int value = 0;

            if (type.Equals("A"))
            {
                string one_string = bit8.Substring(0, 1);
                string four_string = bit8.Substring(4, 1);
                if (one_string.Equals("1") && four_string.Equals("1"))
                {
                    value = 0x88;
                }
                if (one_string.Equals("0") && four_string.Equals("1"))
                {
                    value = 0x08;
                }
                if (one_string.Equals("1") && four_string.Equals("0"))
                {
                    value = 0x80;
                }
                if (one_string.Equals("0") && four_string.Equals("0"))
                {
                    value = 0x00;
                }
            }

            //if (type.Equals("T"))
            //{
            //    string one_string = bit8.Substring(0, 1);
            //    string four_string = bit8.Substring(4, 1);

            //    if (one_string.Equals("1") && four_string.Equals("1"))
            //    {
            //        value = 0x88;
            //    }
            //    if (one_string.Equals("0") && four_string.Equals("1"))
            //    {
            //        value = 0x08;
            //    }
            //    if (one_string.Equals("1") && four_string.Equals("0"))
            //    {
            //        value = 0x80;
            //    }
            //    if (one_string.Equals("0") && four_string.Equals("0"))
            //    {
            //        value = 0x00;
            //    }
            //}

            //if (type.Equals("C"))
            //{
            //    string one_string = bit8.Substring(0, 1);
            //    string four_string = bit8.Substring(4, 1);
            //    if (one_string.Equals("1") && four_string.Equals("1"))
            //    {
            //        value = 0x88;
            //    }
            //    if (one_string.Equals("0") && four_string.Equals("1"))
            //    {
            //        value = 0x08;
            //    }
            //    if (one_string.Equals("1") && four_string.Equals("0"))
            //    {
            //        value = 0x80;
            //    }
            //    if (one_string.Equals("0") && four_string.Equals("0"))
            //    {
            //        value = 0x00;
            //    }
            //}

            //if (type.Equals("G"))
            //{
            //    string one_string = bit8.Substring(0, 1);
            //    string four_string = bit8.Substring(4, 1);
            //    if (one_string.Equals("1") && four_string.Equals("1"))
            //    {
            //        value = 0x88;
            //    }
            //    if (one_string.Equals("0") && four_string.Equals("1"))
            //    {
            //        value = 0x08;
            //    }
            //    if (one_string.Equals("1") && four_string.Equals("0"))
            //    {
            //        value = 0x80;
            //    }
            //    if (one_string.Equals("0") && four_string.Equals("0"))
            //    {
            //        value = 0x00;
            //    }
            //}





            return value;
        }

        /// <summary>
        /// 转换为打印的矩阵
        /// </summary>
        /// <param name="a"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<byte[]> ConvertToPrintOrder(List<string> a, string type)
        {
            List<string> tmp_list = new List<string>();
            string tmp = "";
            int space_number = space_g;
            int line_point = row_g;



            List<byte[]> one_page_print_data_list = new List<byte[]>();


            byte bit4_1 = 0;//"0000";
            byte bit4_2 = 0;//"0000";
            byte bit4_total = 0;// "00000000";
            int byte160_number = 0;


            for (int i = 0; i < a.Count; i++)
            {
                string one_page_data_sz = a[i];
                byte160_number = one_page_data_sz.Length / 320;
                string[] byte160_number_byte_array = new string[byte160_number];


                List<byte> one_page_print_data = new List<byte>();

                for (int inumber = 0; inumber < byte160_number; inumber++)
                {
                    byte160_number_byte_array[inumber] = one_page_data_sz.Substring(inumber * 320, 320);
                    //byte160_number_byte_array[inumber] = one_page_data_sz.Substring(inumber * 160, 160);
                    string byte160_number_byte_string = byte160_number_byte_array[inumber];
                    byte[] byte160_array = new byte[byte160_number_byte_string.Length];
                    Console.WriteLine("inumber:"+ inumber + " " + byte160_number_byte_string);
                    int loop_ii = 0;


                    for (int j = 0; j < byte160_number_byte_string.Length; j += 2)
                    {
                        string one_sz = byte160_number_byte_string.Substring(j, 1);
                        string two_sz = "K";

                        if (j + 1 < one_page_data_sz.Length)
                        {
                            two_sz = byte160_number_byte_string.Substring(j + 1, 1);
                        }

                        if (one_sz == type)
                        {
                            //bit4_1 = "10000000";
                            bit4_1 = 0b10000000;
                        }
                        else
                        {
                            //bit4_1 = "00000000";
                            bit4_1 = 0b00000000;
                        }

                        if (two_sz == type)
                        {
                            //bit4_2 = "00001000";
                            bit4_2 = 0b00001000;
                        }
                        else
                        {
                            bit4_2 = 0b00000000;
                            //bit4_2 = "00000000";
                        }

                        int aa = bit4_1;
                        int bb = bit4_2;
                        bit4_total = (byte)(aa | bb);
                        byte160_array[loop_ii] = bit4_total;
                        loop_ii++;
                        //one_page_data_array[inumber] = bit4_total;
                        //one_page_print_data.Add(bit4_total);
                        //bit4_total = bit4_1 + bit4_2;
                        //tmp_list.Add(tmp);
                    }

                    for (int jjj = 0; jjj < byte160_array.Length; jjj++)
                    {
                        one_page_print_data.Add(byte160_array[jjj]);
                    }
                }

                byte[] byte160_array_all = new byte[one_page_print_data.Count()];

                byte160_array_all = one_page_print_data.ToArray();
                one_page_print_data_list.Add(byte160_array_all);

                //string one_page_data_sz_new = "";

                //for (int iii = 0; iii < one_page_data_sz.Length; iii += line_point)
                //{
                //    one_page_data_sz_new += one_page_data_sz.Substring(iii, line_point);
                //    for (int addx_number = 0; addx_number < 160 - line_point; addx_number++)
                //    {
                //        one_page_data_sz_new += "X";
                //    }
                //}

                //one_page_data_sz = one_page_data_sz_new;

                /*
                byte[] one_page_data_array = new byte[one_page_data_sz.Length];
                int loop_i = 0;
                for (int j = 0; j < one_page_data_sz.Length; j+=2)
                {
                    string one_sz = one_page_data_sz.Substring(j, 1);
                    string two_sz = "K";
                    if (j + 1 < one_page_data_sz.Length)
                    {
                        two_sz = one_page_data_sz.Substring(j + 1, 1);
                    }
                    if (one_sz == type)
                    {
                        //bit4_1 = "10000000";
                        bit4_1 = 0b10000000;
                    }
                    else
                    {
                        //bit4_1 = "00000000";
                        bit4_1 = 0b00000000;
                    }
                    if (two_sz == type)
                    {
                        //bit4_2 = "00001000";
                        bit4_2 = 0b00001000;
                    }
                    else
                    {
                        bit4_2 = 0b00000000;
                        //bit4_2 = "00000000";
                    }
                    int aa = bit4_1;
                    int bb = bit4_2;
                    bit4_total = (byte)(aa | bb);
                    one_page_data_array[loop_i] = bit4_total;
                    loop_i++;
                    one_page_print_data.Add(bit4_total);
                    //bit4_total = bit4_1 + bit4_2;
                    //tmp_list.Add(tmp);

                }
                byte[] one_page_data_array160 = new byte[160];
                //one_page_data_array
                int line_point_new = line_point / 2;
                int aaa = one_page_data_array.Length / (line_point/2)*160;
                byte[] one_page_data_array_new = new byte[aaa];
                int loop_number = 0;
                for (int jjj = 0; jjj < one_page_data_array.Length; jjj += line_point_new)
                {
                    Array.Copy(one_page_data_array, loop_number * line_point_new, one_page_data_array_new, loop_number * 160, line_point_new);
                    loop_number++;
                }
                one_page_print_data_list.Add(one_page_data_array_new);
                */

            }

            return one_page_print_data_list; 
            //return tmp_list;
        }

        private void TToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            //List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "T");

            //int max_length = tmp_list.Select(c => c.Length).Max();

            ////最后一行补上0
            //int last_line_length = tmp_list[tmp_list.Count - 1].Length;
            //if (last_line_length < max_length)
            //{
            //    for (int i = 0; i < (max_length - last_line_length); i++)
            //    {
            //        tmp_list[tmp_list.Count - 1] += "0";
            //    }
            //}
            //DynamicCreationDGV1(dataGridView1, 0, 0, 0, tmp_list); 
        }

        private void CToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            //List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "C");
            //int max_length = tmp_list.Select(c => c.Length).Max();

            ////最后一行补上0
            //int last_line_length = tmp_list[tmp_list.Count - 1].Length;
            //if (last_line_length < max_length)
            //{
            //    for (int i = 0; i < (max_length - last_line_length); i++)
            //    {
            //        tmp_list[tmp_list.Count - 1] += "0";
            //    }
            //}

            //DynamicCreationDGV1(dataGridView1, 0, 0, 0, tmp_list); 
        }

        private void GToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "G");
            //int max_length = tmp_list.Select(c => c.Length).Max();

            ////最后一行补上0
            //int last_line_length = tmp_list[tmp_list.Count - 1].Length;
            //if (last_line_length < max_length)
            //{
            //    for (int i = 0; i < (max_length - last_line_length); i++)
            //    {
            //        tmp_list[tmp_list.Count - 1] += "0";
            //    }
            //}
            //DynamicCreationDGV1(dataGridView1, 0, 0, 0, tmp_list); 
        }

        private void toolStripMenuItem3_Click(object sender, EventArgs e)
        {


            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            //fileDialog.Filter="所有文件(*.*)|*.*";
            fileDialog.Filter = "TXT files (*.txt)|*.txt";
            string file_name = "";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = fileDialog.FileName;
                file_name = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);



            }
            if (file_name.Equals(""))
            {
                return;
            }


            label1.Text = file_name + ".txt";
            List<string> sequencere_list = ReadTxtToList(file_name + ".txt");
            string tmp1 = "";
            //for (int i = 0; i < sequencere_list.Count; i++)
            //{
            //    tmp1 += sequencere_list[i].ToArray()[0];
            //    textBox1.AppendText(sequencere_list[i]+"\r\n");
            //}

            Console.Write(tmp1.Length);
            //Test(sequencere_list);
        }

        public string OpenFileGetPrintTotalString(ref List<string> list)
        {
            string total_string = "";
            textBox1.Text = "";

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            //fileDialog.Filter="所有文件(*.*)|*.*";
            fileDialog.Filter = "TXT files (*.txt)|*.txt";
            string file_name = "";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = fileDialog.FileName;
                file_name = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
            }
            if (file_name.Equals(""))
            {
                return "";
            }
            int row_points = int.Parse(ucTextBoxExRows.InputText);
            label1.Text = file_name + ".txt";
            List<string> sequencere_list = ReadTxtToList(file_name + ".txt");
            string tmp1 = "";
            int column_number = sequencere_list.Count;
            int remainder = column_number % row_points;
            int need_bu_column = 0;
            string ls = "";

            if (remainder != 0)
            {
                //需要补齐行
                //计算出来余数
                need_bu_column = row_points - remainder;
                //用S组成的行来补齐
                for (int i = 0; i < need_bu_column; i++)
                {
                    ls = "";
                    for (int j = 0; j < line_max_length_g; j++)
                    {
                        ls += "S";
                    }
                    sequencere_list.Add(ls);
                }
            }

            //for (int i = 0; i < sequencere_list.Count; i++)
            //{
            //    textBox1.AppendText(sequencere_list[i] + "\r\n");
            //}
            List<string> row_string_list = new List<string>();
            List<string> one_page_data = new List<string>();
            for (int i = 0; i < sequencere_list[0].Length; i++)
            {
                row_string_list.Add("");
                row_string_list[i] = GetOneTimePrintString(sequencere_list,i);
                column_number_g = row_string_list[i].Length;
                total_string += row_string_list[i];
                one_page_data.Add(row_string_list[i]);
                list.Add(row_string_list[i]);
            }
            return total_string;
        }

        public List<string> OpenFileGetPrintDataList()
        {
            string total_string = "";
            textBox1.Text = "";

            OpenFileDialog fileDialog = new OpenFileDialog();
            fileDialog.Multiselect = true;
            fileDialog.Title = "请选择文件";
            //fileDialog.Filter="所有文件(*.*)|*.*";
            fileDialog.Filter = "TXT files (*.txt)|*.txt";
            string file_name = "";
            if (fileDialog.ShowDialog() == DialogResult.OK)
            {
                string file_path = fileDialog.FileName;
                file_name = System.IO.Path.GetFileNameWithoutExtension(fileDialog.FileName);
            }

            if (file_name.Equals(""))
            {
                return new List<string>();
            }

            int row_points = int.Parse(ucTextBoxExRows.InputText);

            label1.Text = file_name + ".txt";
            List<string> sequencere_list = ReadTxtToList(file_name + ".txt");
            string tmp1 = "";

            int column_number = sequencere_list.Count;
            int remainder = column_number % row_points;
            int need_bu_column = 0;
            string ls = "";

            if (remainder != 0)
            {
                //需要补齐行
                //计算出来余数
                need_bu_column = row_points - remainder;
                //用S组成的行来补齐
                for (int i = 0; i < need_bu_column; i++)
                {
                    ls = "";
                    for (int j = 0; j < line_max_length_g; j++)
                    {
                        ls += "S";
                    }
                    sequencere_list.Add(ls);
                }
            }


            for (int i = 0; i < sequencere_list.Count; i++)
            {
                textBox1.AppendText(sequencere_list[i] + "\r\n");
            }

            List<string> row_string_list = new List<string>();

            List<string> total_page_data_list = new List<string>();

            for (int i = 0; i < sequencere_list[0].Length; i++)
            {
                row_string_list.Add("");
                row_string_list[i] = GetOneTimePrintString(sequencere_list, i);
                column_number_g = row_string_list[i].Length;
                total_string += row_string_list[i];
                total_page_data_list.Add(row_string_list[i]);
            }

            //Console.Write(tmp1.Length);
            //mList_string_g = new List<string>();

            //total_string = Test(sequencere_list);

            //total_string = row_string_list[0];
            return total_page_data_list;
        }
        /// <summary>
        /// 一列有多少个碱基
        /// </summary>
        public static int column_number_g = 0;
        public string GetOneTimePrintString(List<string> sequencere_list,int index)
        {
            //List<string> one_time_print_string_list = new List<string>();
            string tmp1 = "";
            for (int i = 0; i < sequencere_list.Count; i++)
            {
                tmp1 += sequencere_list[i].ToArray()[index];
                //one_time_print_string_list.Add(tmp1);
            }
            return tmp1;
        }


        private void ActiveToolStripMenuItem_Click(object sender, EventArgs e)
        {
            //string str = "01010100"; //扩展位元素
            //value = Convert.ToInt32(str, 2); //二进制字符串转整数

        }

        byte ConvertToByte(BitArray bits)
        {
            if (bits.Count != 8)
            {
                throw new ArgumentException("bits");
            }
            byte[] bytes = new byte[1];
            bits.CopyTo(bytes, 0);
            return bytes[0];
        }

        public List<PrintContent> print_content_list_g = new List<PrintContent>();

        private void ucBtnImg11_BtnClick(object sender, EventArgs e)
        {
            print_content_list_g = new List<PrintContent>();
            string total_string = "";
            mList_string_g = new List<string>();

            List<string> total_page_data_list = new List<string>();
            total_string = OpenFileGetPrintTotalString(ref total_page_data_list);


            //total_page_data_list = OpenFileGetPrintDataList();

            string row_number = ucTextBoxExRows.InputText.ToString();
            row_g = int.Parse(row_number);//
            string space = ucTextBoxExSpace.InputText.ToString();
            space_g = int.Parse(space);



            //如果有间隔那么处理间隔
            string total_string_add_space = "";
            string space_string = "";
            string one_page_data = "";

            for (int j = 0; j < space_g; j++)
            {
                space_string += "J";
            }

            for (int jj = 0; jj < total_page_data_list.Count; jj++)
            {
                one_page_data = total_page_data_list[jj];
                string new_one_page_data = "";
                if (space_g > 0)
                {
                    for (int i = 0; i < one_page_data.Length; i++)
                    {
                        new_one_page_data += one_page_data.Substring(i, 1) + space_string;
                    }
                    total_page_data_list[jj] = new_one_page_data;
                    //new_one_page_data = "";
                    if (row_g==160)
                    {
                    }
                    else
                    {
                        string new_one_page_data1 = "";
                        for (int i = 0; i < new_one_page_data.Length;)
                        {
                            new_one_page_data1 += new_one_page_data.Substring(i, row_g);
                            int need_add_X_number = 320 - row_g;
                            for (int xi = 0; xi < need_add_X_number; xi++)
                            {
                                new_one_page_data1 += "X";
                            }
                            i += row_g;
                        }
                        //one_page_data = new_one_page_data1;
                        if (new_one_page_data1.Length > 0)
                        {
                            total_page_data_list[jj] = new_one_page_data1;
                        }
                    }
                }
                else
                {
                    //total_string_add_space = one_page_data;
                    string new_one_page_data1 = "";
                    for (int i = 0; i < new_one_page_data.Length;)
                    {
                        new_one_page_data1 += new_one_page_data.Substring(i, row_g);
                        int need_add_X_number = 320 - row_g;
                        for (int xi = 0; xi < need_add_X_number; xi++)
                        {
                            new_one_page_data1 += "X";
                        }
                        i += row_g;
                    }
                    //one_page_data = new_one_page_data;
                    if (new_one_page_data1.Length > 0)
                    {
                        total_page_data_list[jj] = new_one_page_data1;
                    }
                }
                //one_page_data = total_string_add_space;
            }
            total_string_add_space = "";
            space_string = "";
            if (total_string.Equals(""))
            {
                return;
            }
            for (int j = 0; j < space_g; j++)
            {
                space_string += "X";
            }

            if (space_g > 0)
            {

                StringBuilder sb1 = new StringBuilder(total_string.Length * 2);
                sb1.Append(total_string);
                total_string = sb1.ToString();
                var sb = new StringBuilder(total_string.Length * 2);
                foreach (var c in total_string)
                {
                    if (space_g == 1)
                    {
                        sb.Append(new string(new[] { c, 'X' }));
                    }
                    if (space_g == 2)
                    {
                        sb.Append(new string(new[] { c, 'X', 'X' }));
                    }
                    if (space_g == 3)
                    {
                        sb.Append(new string(new[] { c, 'X', 'X', 'X' }));
                    }
                    if (space_g == 4)
                    {
                        sb.Append(new string(new[] { c, 'X', 'X', 'X', 'X' }));
                    }
                    if (space_g == 5)
                    {
                        sb.Append(new string(new[] { c, 'X', 'X', 'X', 'X' }));
                    }
                    //sb.Append(new string(new[] { c, 'X', 'X' }));
                }
                total_string_add_space = sb.ToString();
                //for (int i = 0; i < total_string.Length; i++)
                //{
                //    total_string_add_space += total_string.Substring(i, 1) + space_string;
                //}
            }
            else
            {
                total_string_add_space = total_string;
            }
            total_string = total_string_add_space;
            List<string> total_string_list = new List<string>();
            for (int i = 0; i < total_string.Length; i = i + row_g)
            {
                if ((total_string.Length - i) > row_g || (total_string.Length - i) == row_g)
                {
                    string tmp = total_string.Substring(i, row_g);
                    if (tmp.Length == row_g)
                    {
                        total_string_list.Add(tmp);
                    }
                }else
                {
                    int a = total_string.Length - i;
                    //剩下不够row_g长度的序列
                    string tmp2 = total_string.Substring(i, a);
                    int need_a = row_g - a;
                    for (int j = 0; j < need_a; j++)
                    {
                        tmp2 += "X";
                    }
                    total_string_list.Add(tmp2);
                    //if()
                }
            }
            //将total_string_list转换成一个表格
            DynamicCreationDGV_1(dataGridView1, total_string_list[0].Length, total_string_list.Count, 0, total_string_list);
            //mList_string_g
            //转换成打印指令
            //GetPrintList(total_string_list);
            GetPrintList(total_page_data_list);
            
        }

        private DataGridView DynamicCreationDGV_1(DataGridView dgv, int row, int column, int space, List<string> a)
        {
            int max_length = a.Select(c => c.Length).Max();


            micro_array_g = new int[row, column];
            if (dgv == null)
                return null;
            if (dgv.ColumnCount != 0)
                dgv.Columns.Clear();    //清除现有列
            #region 组织 DataGridViewColumn
            for (int i = 1; i < max_length + 1; i++) //指定总列数
            {
                //列名和列头名
                string dgvColName = "" + i;
                string dgvColHeaderText = string.Empty;
                DataGridViewColumn dgvcolumn = new DataGridViewTextBoxColumn();    //列默认类型
                dgvcolumn.ValueType = typeof(string);           //列数据类型默认字符串
                dgvcolumn.HeaderText = dgvColName;
                dgvcolumn.Width = 50;
                dgvcolumn.SortMode = DataGridViewColumnSortMode.NotSortable;
                dgv.Columns.Add(dgvcolumn); //增加列
            }
            #endregion
            #region 组织DataGridViewRow

            //合共有多少个格
            int total_blank = 0;
            //需要显示的索引
            int show_index = 0;

            for (int i = 0; i < a.Count; i++)    //组织指定数据行
            {
                DataGridViewRow dr = new DataGridViewRow();
                dgv.Rows.Add(dr);           //增加行
                for (int j = 0; j < max_length; j++) //部分列
                {
                    if (total_blank == 0)
                    {
                        var value = a[i].ToArray()[j];

                        dgv.Rows[i].Cells[j].Value = value;

                        //if (value == '0')
                        //{
                        //    dgv.Rows[i].Cells[j].Value = 0;//(i + 1) + "_" + (j + 1);
                        //    //micro_array_g[i, j] = 0;
                        //}
                        //else
                        //{
                        //    dgv.Rows[i].Cells[j].Value = 1;//(i + 1) + "_" + (j + 1);
                        //    //micro_array_g[i, j] = 1;
                        //}
                    }
                    if (total_blank % (space + 1) == 0)
                    {
                        var value = a[i].ToArray()[j];
                        dgv.Rows[i].Cells[j].Value = value;
                        //if (value == '0')
                        //{
                        //    dgv.Rows[i].Cells[j].Value = 0;//(i + 1) + "_" + (j + 1);
                        //    //micro_array_g[i, j] = 0;
                        //}
                        //else
                        //{
                        //    dgv.Rows[i].Cells[j].Value = 1;//(i + 1) + "_" + (j + 1);
                        //    //micro_array_g[i, j] = 1;
                        //}
                    }
                    else
                    {
                        //dgv.Rows[i].Cells[j].Value = "不可用";
                        dgv.Rows[i].Cells[j].Style.BackColor = Color.CadetBlue;
                        //micro_array_g[i, j] = 5;
                    }
                    total_blank++;
                }
            }

            Console.Write(total_blank);
            #endregion
            return dgv;
        }

    }
}
