using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.BioSoft.Model
{
    public class SystemSetDTO: ObservableObject
    {
        public SystemSetDTO()
        {

        }
        private object id;
        /// <summary>
        /// 
        /// </summary>
        public object Id
        {
            get => id;
            set
            {
                SetProperty(ref id, value);
            }
        }
        private int cmdInterval;
        /// <summary>
        /// 
        /// </summary>
        public int CmdInterval
        {
            get => cmdInterval;
            set
            {
                SetProperty(ref cmdInterval, value);
            }
        }
        private int cmdReSend;
        /// <summary>
        /// 
        /// </summary>
        public int CmdReSend
        {
            get => cmdReSend;
            set
            {
                SetProperty(ref cmdReSend, value);
            }
        }
    }
}
