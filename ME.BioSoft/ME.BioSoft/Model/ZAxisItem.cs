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
        private bool isCheck = true;
        /// <summary>
        /// 
        /// </summary>
        public bool IsCheck
        {
            get => isCheck;
            set
            {
                isCheck = value;
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
                ckContent = value;
                SetProperty(ref ckContent,value);
            }
        }
        
    }
}
