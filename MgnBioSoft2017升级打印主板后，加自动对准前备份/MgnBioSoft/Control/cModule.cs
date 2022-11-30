// VBConversions Note: VB project level imports
using System.Collections.Generic;
using System;
using System.Linq;
using System.Drawing;
using System.Diagnostics;
using System.Threading;
using System.Data;
using System.Xml.Linq;
using Microsoft.VisualBasic;
using System.Collections;
using System.Windows.Forms;
// End of VB project level imports


namespace Ewin.Client.Frame.UcGrid
{
    /// <summary>
    /// 折叠控件样式以及行数操作类
    /// </summary>
    sealed class cModule
    {
        #region CustomGrid
        static System.Windows.Forms.DataGridViewCellStyle dateCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight };
        static System.Windows.Forms.DataGridViewCellStyle amountCellStyle = new System.Windows.Forms.DataGridViewCellStyle { Alignment = DataGridViewContentAlignment.MiddleRight, Format = "N2" };
        static System.Windows.Forms.DataGridViewCellStyle gridCellStyle = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft,
            BackColor = System.Drawing.Color.FromArgb(System.Convert.ToInt32(System.Convert.ToByte(79)), System.Convert.ToInt32(System.Convert.ToByte(129)), System.Convert.ToInt32(System.Convert.ToByte(189))),
            Font = new System.Drawing.Font("微软雅黑", (float)(20.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0)),
            ForeColor = System.Drawing.SystemColors.ControlLightLight,
            SelectionBackColor = System.Drawing.SystemColors.Highlight,
            SelectionForeColor = System.Drawing.SystemColors.HighlightText,
            WrapMode = System.Windows.Forms.DataGridViewTriState.True
        };
        static System.Windows.Forms.DataGridViewCellStyle gridCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft,
            BackColor = System.Drawing.SystemColors.ControlLightLight,
            Font = new System.Drawing.Font("微软雅黑", (float)(20.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0)),
            ForeColor = System.Drawing.SystemColors.ControlText,
            SelectionBackColor = System.Drawing.Color.FromArgb(System.Convert.ToInt32(System.Convert.ToByte(155)), System.Convert.ToInt32(System.Convert.ToByte(187)), System.Convert.ToInt32(System.Convert.ToByte(89))),
            SelectionForeColor = System.Drawing.SystemColors.HighlightText,
            WrapMode = System.Windows.Forms.DataGridViewTriState.False
        };
        static System.Windows.Forms.DataGridViewCellStyle gridCellStyle3 = new System.Windows.Forms.DataGridViewCellStyle
        {
            Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleLeft,
            BackColor = System.Drawing.Color.WhiteSmoke,
            Font = new System.Drawing.Font("微软雅黑", (float)(20.0F), System.Drawing.FontStyle.Regular, System.Drawing.GraphicsUnit.Point, System.Convert.ToByte(0)),
            ForeColor = System.Drawing.SystemColors.WindowText,
            SelectionBackColor = System.Drawing.Color.FromArgb(System.Convert.ToInt32(System.Convert.ToByte(155)), System.Convert.ToInt32(System.Convert.ToByte(187)), System.Convert.ToInt32(System.Convert.ToByte(89))),
            SelectionForeColor = System.Drawing.SystemColors.HighlightText,
            WrapMode = System.Windows.Forms.DataGridViewTriState.True
        };

        //设置表格的主题样式
        static public void applyGridTheme(DataGridView grid)
        {
            grid.AllowUserToAddRows = false;
            grid.AllowUserToDeleteRows = false;
            grid.BackgroundColor = System.Drawing.SystemColors.Window;
            grid.BorderStyle = System.Windows.Forms.BorderStyle.None;
            grid.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            grid.ColumnHeadersDefaultCellStyle = gridCellStyle;
            grid.ColumnHeadersHeight = 32;
            grid.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.DisableResizing;
            grid.DefaultCellStyle = gridCellStyle2;
            grid.EnableHeadersVisualStyles = false;
            grid.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
            //grid.ReadOnly = true;
            grid.RowHeadersVisible = true;
            grid.RowHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            grid.RowHeadersDefaultCellStyle = gridCellStyle3;
            grid.RowTemplate.Height = 33;
            grid.Font = gridCellStyle.Font;
        }

        //设置表格单元格样式
        static public void setGridRowHeader(DataGridView dgv, bool hSize = false)
        {
            dgv.TopLeftHeaderCell.Value = "NO ";
            dgv.TopLeftHeaderCell.Style.Alignment = DataGridViewContentAlignment.MiddleCenter;
            dgv.AutoResizeRowHeadersWidth(DataGridViewRowHeadersWidthSizeMode.AutoSizeToDisplayedHeaders);
            foreach (DataGridViewColumn cCol in dgv.Columns)
            {
                if (cCol.ValueType.ToString() == typeof(DateTime).ToString())
                {
                    cCol.DefaultCellStyle = dateCellStyle;
                }
                else if (cCol.ValueType.ToString() == typeof(decimal).ToString() || cCol.ValueType.ToString() == typeof(double).ToString())
                {
                    cCol.DefaultCellStyle = amountCellStyle;
                }
            }
            if (hSize)
            {
                dgv.RowHeadersWidth = dgv.RowHeadersWidth + 16;
            }
            dgv.AutoResizeColumns();
        }

        //设置表格的行号
        static public void rowPostPaint_HeaderCount(object obj_sender, DataGridViewRowPostPaintEventArgs e)
        {
            try
            {
                var sender = (DataGridView)obj_sender;
                //set rowheader count
                DataGridView grid = (DataGridView)sender;
                string rowIdx = System.Convert.ToString((e.RowIndex + 1).ToString());
                var centerFormat = new StringFormat();
                centerFormat.Alignment = StringAlignment.Center;
                centerFormat.LineAlignment = StringAlignment.Center;
                Rectangle headerBounds = new Rectangle(e.RowBounds.Left, e.RowBounds.Top,
                    grid.RowHeadersWidth, e.RowBounds.Height - sender.Rows[e.RowIndex].DividerHeight);
                e.Graphics.DrawString(rowIdx, grid.Font, SystemBrushes.ControlText,
                    headerBounds, centerFormat);
            }
            catch (Exception ex)
            {

            }
        }
        #endregion

        #region

        /// <summary>
        /// 控件居中
        /// </summary>
        /// <param name="setvalue"></param>
        /// <param name="control"></param>
        static public void SetControlCenter(IWin32Window setvalue, Control control)
        {
            Form form = (Form)setvalue;
            int gLeft = form.Width / 2 - control.Width / 2;
            int gTop = form.Height / 2 - control.Height / 2;
            control.Location = new Point(gLeft, gTop);
        }
        #endregion

        public static void dataGridViewUI(DataGridView dataGridView,float header_font_size)
        {
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle1 = new System.Windows.Forms.DataGridViewCellStyle();
            System.Windows.Forms.DataGridViewCellStyle dataGridViewCellStyle2 = new System.Windows.Forms.DataGridViewCellStyle();
            dataGridView.AllowUserToAddRows = false;
            dataGridView.AllowUserToDeleteRows = false;

            dataGridViewCellStyle1.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(207)))), ((int)(((byte)(213)))), ((int)(((byte)(234)))));//System.Drawing.Color.LightCyan;
            dataGridView.AlternatingRowsDefaultCellStyle = dataGridViewCellStyle1;

            dataGridView.BackgroundColor = System.Drawing.Color.FromArgb(((int)(((byte)(233)))), ((int)(((byte)(235)))), ((int)(((byte)(245)))));//System.Drawing.Color.White;
            dataGridView.BorderStyle = System.Windows.Forms.BorderStyle.Fixed3D;

            dataGridView.ColumnHeadersBorderStyle = System.Windows.Forms.DataGridViewHeaderBorderStyle.Single;
            dataGridViewCellStyle2.Alignment = System.Windows.Forms.DataGridViewContentAlignment.MiddleCenter;//211, 223, 240
            dataGridViewCellStyle2.BackColor = System.Drawing.Color.FromArgb(((int)(((byte)(68)))), ((int)(((byte)(114)))), ((int)(((byte)(196)))));

            dataGridViewCellStyle2.Font = new System.Drawing.Font("微软雅黑", header_font_size, System.Drawing.FontStyle.Bold, System.Drawing.GraphicsUnit.Point, ((byte)(134)));
            dataGridViewCellStyle2.ForeColor = System.Drawing.Color.White;//System.Drawing.Color.FromArgb(((int)(((byte)(6)))), ((int)(((byte)(90)))), ((int)(((byte)(185)))));//System.Drawing.Color.Navy;

            dataGridViewCellStyle2.SelectionBackColor = System.Drawing.SystemColors.Highlight;
            dataGridViewCellStyle2.SelectionForeColor = System.Drawing.SystemColors.HighlightText;
            dataGridView.ColumnHeadersDefaultCellStyle = dataGridViewCellStyle2;

            dataGridView.ColumnHeadersHeightSizeMode = System.Windows.Forms.DataGridViewColumnHeadersHeightSizeMode.AutoSize;
            dataGridView.EnableHeadersVisualStyles = false;
            dataGridView.GridColor = System.Drawing.SystemColors.GradientInactiveCaption;
            dataGridView.ReadOnly = false;
            dataGridView.RowHeadersVisible = false;
            //dataGridView.RowTemplate.Height = 50;
            dataGridView.RowTemplate.ReadOnly = false;
            dataGridView.SelectionMode = DataGridViewSelectionMode.FullRowSelect;
        }


    }

    /// <summary>
    /// 控件类型，是最外层的表格还是中间层的表格
    /// </summary>
    public enum controlType
    {
        outside = 0,
        middle = 1
    }

    /// <summary>
    /// 展开图标
    /// </summary>
    public enum rowHeaderIcons
    {
        expand = 0,
        collapse = 1
    }
}
