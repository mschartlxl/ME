using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BioSoft.Model
{
    public class ZAxisItem: ObservableObject
    {
        public ZAxisItem()
        {

        }
        private int id;
        /// <summary>
        /// 
        /// </summary>
        public int Id
        {
            get => id;
            set
            {
                SetProperty(ref id, value);
            }
        }
        private bool isCheck = true;
        /// <summary>
        /// 
        /// </summary>
        public bool IsCheck
        {
            get => isCheck;
            set
            {
                SetProperty(ref isCheck, value);
            }
        }
        private string ckContent;
        /// <summary>
        /// 
        /// </summary>
        public string CkContent
        {
            get => ckContent;
            set
            {
                SetProperty(ref ckContent,value);
            }
        }
        private string ckTxt;
        /// <summary>
        /// 
        /// </summary>
        public string CkTxt
        {
            get => ckTxt;
            set
            {
                SetProperty(ref ckTxt, value);
            }
        }

    }
}
