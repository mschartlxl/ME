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

namespace MGNChipMatrix
{
    public partial class Form1 : Form
    {
        public Form1()
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

        public static int row_g = 0;
        public static int column_g = 0;
        public static int space_g = 0;

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

            GetDGVData(320, 4, 0);
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

            string tmp1 = "";
            for (int i = 0; i < sequencere_list.Count; i++)
            {
                //Console.Write(sequencere_list[i].Length + "\r\n");
                tmp1 += sequencere_list[i].ToArray()[0];
            }
            Console.Write(tmp1.Length);
            Test(sequencere_list);
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

            //List<string> tmp_list = new List<string>();
            for (int i = 0; i < total_string.Length; i++)
            {
                if (total_string.Length - i < xulie_length)
                {
                    int a = total_string.Length - i;
                    mList_string_g.Add(total_string.Substring(i, a));
                }
                else
                {
                    mList_string_g.Add(total_string.Substring(i, xulie_length));
                }
                i += xulie_length; 
            }

            mList_g = total_string.ToList();
            return total_string;
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
                    sz_list.Add(ls.Split(':')[1]);
                }
            }
            int max_length = sz_list.Select(c => c.Length).Max();
            sw.Stop();
            return max_length;
        }

        public List<string> ReadTxtToList(String _fileName)
        {
            int line_max_length = ReadLineStringMaxLength(_fileName);
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
            List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "A");

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

            DynamicCreationDGV1(dataGridView1,10,13,0,tmp_list); 
        }

        /// <summary>
        /// 转换为打印的矩阵
        /// </summary>
        /// <param name="a"></param>
        /// <param name="type"></param>
        /// <returns></returns>
        public List<string> ConvertToPrintOrder(List<string> a,string type)
        {
            List<string> tmp_list = new List<string>();
            string tmp = "";
            for (int i = 0; i < a.Count; i++)
            {
                tmp = a[i].Replace('A', ((type == "A") ? '1' : '0'));
                tmp = tmp.Replace('T', ((type == "T") ? '1' : '0'));
                tmp = tmp.Replace('C', ((type == "C") ? '1' : '0'));
                tmp = tmp.Replace('G', ((type == "G") ? '1' : '0'));
                tmp = tmp.Replace('X', '0');
                tmp_list.Add(tmp);
            }
            return tmp_list;
        }

        private void TToolStripMenuItem1_Click(object sender, EventArgs e)
        {
            List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "T");
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
            DynamicCreationDGV1(dataGridView1, 0, 0, 0, tmp_list); 
        }

        private void CToolStripMenuItem2_Click(object sender, EventArgs e)
        {
            List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "C");
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

            DynamicCreationDGV1(dataGridView1, 0, 0, 0, tmp_list); 
        }

        private void GToolStripMenuItem_Click(object sender, EventArgs e)
        {
            List<string> tmp_list = ConvertToPrintOrder(mList_string_g, "G");
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
            DynamicCreationDGV1(dataGridView1, 0, 0, 0, tmp_list); 
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
            label1.Text = file_name + ".txt";
            List<string> sequencere_list = ReadTxtToList(file_name + ".txt");
            string tmp1 = "";
            for (int i = 0; i < sequencere_list.Count; i++)
            {
                tmp1 += sequencere_list[i].ToArray()[0];
                textBox1.AppendText(sequencere_list[i]+"\r\n");
            }
            Console.Write(tmp1.Length);
            Test(sequencere_list);
        }
    }
}
