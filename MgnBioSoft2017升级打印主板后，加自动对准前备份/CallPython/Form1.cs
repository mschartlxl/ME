using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data;
using System.Drawing;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Forms;
using IronPython.Hosting;
using Microsoft.Scripting;
using Microsoft.Scripting.Hosting;

namespace CallPython
{
    public partial class Form1 : Form
    {
        public Form1()
        {
            InitializeComponent();
        }

        private void button1_Click(object sender, EventArgs e)
        {
            UsingPython();
        }

        private ScriptRuntime pyRuntime = null;
        private dynamic obj = null;

        public void UsingPython()
        {
            string serverpath = AppDomain.CurrentDomain.BaseDirectory + "test1.py";//所引用python路径
            pyRuntime = Python.CreateRuntime();
            ScriptEngine Engine = pyRuntime.GetEngine("python");

            dynamic py = Engine.ExecuteFile(@"test1.py");//读取脚本文件
            string dd = py.main("23345");//调用脚本文件中对应的函数
            label1.Text = dd;

            //ScriptScope pyScope = Engine.CreateScope(); //Python.ImportModule(Engine, "random");
            //obj = Engine.ExecuteFile(serverpath, pyScope);
        }
        public bool ExcutePython()
        {
            try
            {
                if (null != obj)
                {
                    obj.frs_init();//调用frs_main.py中的方法
                }
                else
                {
                    return false;
                }
                return true;
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
    }
}
