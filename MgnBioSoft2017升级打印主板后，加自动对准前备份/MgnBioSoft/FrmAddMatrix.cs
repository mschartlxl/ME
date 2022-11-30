using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Windows.Forms;

namespace MGNBIO
{
    public partial class FrmaAddMatrix : Form
    {
        public string column
        {
            get { return textBoxColumn.Text.Trim(); }
        }

        public string row
        {
            get { return textBoxRow.Text.Trim(); }
        }

        public string space
        {
            get { return textBoxSpace.Text.Trim(); }
        }

        public FrmaAddMatrix()
        {
            InitializeComponent();
        }

        private void btnConfirm_Click(object sender, EventArgs e)
        {
            if (string.IsNullOrEmpty(textBoxColumn.Text.Trim()))
            {
                MessageBox.Show("请填写节点名称！");
                return;
            }
            DialogResult = DialogResult.OK;
        }

        private void btnCancel_Click(object sender, EventArgs e)
        {
            DialogResult = DialogResult.Cancel;
        }
    }
}
