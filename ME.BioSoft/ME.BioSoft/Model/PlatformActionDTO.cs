using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BioSoft.Model
{
    public class PlatformActionDTO : ObservableObject
    {
        public PlatformActionDTO()
        {

        }
        private bool isChecked;
        /// <summary>
        /// 
        /// </summary>
        public bool IsChecked
        {
            get => isChecked;
            set
            {
                SetProperty(ref isChecked, value);
            }
        }
        private string id;
        /// <summary>
        /// 
        /// </summary>
        public string Id
        {
            get => id;
            set
            {
                SetProperty(ref id, value);
            }
        }
        private string name;
        /// <summary>
        /// 
        /// </summary>
        public string Name
        {
            get => name;
            set
            {
                SetProperty(ref name, value);
            }
        }
        private int  x;
        /// <summary>
        /// 
        /// </summary>
        public int  X
        {
            get => x;
            set
            {
                SetProperty(ref x, value);
            }
        }
        private int y;
        /// <summary>
        /// 
        /// </summary>
        public int Y
        {
            get => y;
            set
            {
                SetProperty(ref y, value);
            }
        }
        private int r;
        /// <summary>
        /// 
        /// </summary>
        public int R
        {
            get => r;
            set
            {
                SetProperty(ref r, value);
            }
        }
        private object z;
        /// <summary>
        /// 
        /// </summary>
        public object Z
        {
            get => z;
            set
            {
                SetProperty(ref z, value);
            }
        }
    }
}
