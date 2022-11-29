using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.ControlLibrary.Model
{
    public class CmdParameter : ObservableObject
    {
        private string _ParaName;
        /// <summary>
        /// ParaName
        /// </summary>
        public string ParaName
        {
            set
            {
                if (_ParaName != value)
                {
                    _ParaName = value;
                    OnPropertyChanged();
                }
            }
            get
            {
                return _ParaName;
            }
        }
        private int paraType;
        /// <summary>
        /// ParaName
        /// </summary>
        public int ParaType
        {
            set
            {
                if (paraType != value)
                {
                    paraType = value;
                    OnPropertyChanged();
                }
            }
            get
            {
                return paraType;
            }
        }
    }
    
}

