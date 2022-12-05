using CommunityToolkit.Mvvm.ComponentModel;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace ME.ControlLibrary.Model
{
    public class CommandMessage
    {
        private static object lockObj = new object();
        /// <summary>
        /// 单例对象
        /// </summary>
        private static CommandMessage _instance;
        /// <summary>
        /// 实例
        /// </summary>
        public static CommandMessage Instance
        {
            get
            {
                if (null == _instance)
                {
                    lock (lockObj)
                    {
                        if (null == _instance)
                        {
                            _instance = new CommandMessage();
                        }
                    }
                }
                return _instance;
            }
        }
        public ObservableCollection<MessageCmd> Messages { get; set; } = new ObservableCollection<MessageCmd>();
      
        public bool Flag { get; set; } = true;
        public CancellationTokenSource tokenSource = new CancellationTokenSource();
        public CancellationToken Token { get { return tokenSource.Token; }  }
    }
    public class MessageCmd : ObservableObject
    {
        private string timeCmd;
        /// <summary>
        /// TimeCmd
        /// </summary>
        public string TimeCmd
        {
            set
            {
                SetProperty(ref timeCmd, value);
            }
            get
            {
                return timeCmd;
            }
        }
        private string contentCmd;
        /// <summary>
        /// ContentCmd
        /// </summary>
        public string ContentCmd
        {
            set
            {
                SetProperty(ref contentCmd, value);
            }
            get
            {
                return contentCmd;
            }
        }
        private string isSendCmd;
        /// <summary>
        /// isSendCmd
        /// </summary>
        public string IsSendCmd
        {
            set
            {
                SetProperty(ref isSendCmd,value);
            }
            get
            {
                return isSendCmd;
            }
        }
        private bool isSelect;
        /// <summary>
        /// IsSelect
        /// </summary>
        public bool IsSelect
        {
            set
            {
                SetProperty(ref isSelect, value);
            }
            get
            {
                return isSelect;
            }
        }
    }
}
