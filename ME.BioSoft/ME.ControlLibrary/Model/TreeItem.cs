using ME.BaseCore;
using ME.BaseCore.Models.Enums;
using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.ComponentModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace ME.ControlLibrary.Model
{
    public class TreeItem : INotifyPropertyChanged
    {// 构造函数
        public TreeItem()
        {
            Children = new ObservableCollection<TreeItem>();
        }
        /// <summary>
        ///选中当前Item
        /// </summary>
        private bool isSelected = false;
        public bool IsSelected
        {
            get { return this.isSelected; }
            set
            {
                if (value != this.isSelected)
                {
                    this.isSelected = value;
                    RaisePropertyChanged("IsSelected");
                }
            }
        }
        public NodeInfo Tag { get; set; }

        // 节点类型
        public string ItemType
        {
            get;
            set;
        }

        private bool _IsCheckItem;
        /// <summary>
        /// 
        /// </summary>
        public bool IsCheckItem
        {
            set
            {
                if (_IsCheckItem != value)
                {
                    _IsCheckItem = value;
                    RaisePropertyChanged("IsCheckItem");
                }
            }
            get
            {
                return _IsCheckItem;
            }
        }

        // 节点文字信息
        //public string ItemText
        //{
        //    get;
        //    set;
        //}

        /// <summary>
        /// 当前节点
        /// </summary>
        public string NodeId
        {
            get;
            set;
        }
        // 父节点
        public string ParentNodeId
        {
            get;
            set;
        }

        private string _ItemText;
        /// <summary>
        /// 节点文字信息
        /// </summary>
        public string ItemText
        {
            set
            {
                if (_ItemText != value)
                {
                    _ItemText = value;
                    RaisePropertyChanged("ItemText");
                }
            }
            get
            {
                return _ItemText;
            }
        }

        // 节点图标路径
        public string ItemIcon
        {
            get;
            set;
        }

        // 节点其他信息
        // ...

        // 父节点
        [Newtonsoft.Json.JsonIgnore]
        public TreeItem Parent
        {
            get;
            set;
        }
        // 子节点
        //public ObservableCollection<TreeItem> Children
        //{
        //    get;
        //    set;
        //}
        //Check 相关信息
        bool? _isChecked = false;

        public bool? IsChecked
        {
            get { return _isChecked; }
            set { this.SetIsChecked(value, true, true); }
        }


        /// <summary>
        /// 循环数
        /// </summary>
        string _strCycleNum = "  ";
        /// <summary>
        /// 循环数
        /// </summary>
        public string StrCycleNum
        {
            set
            {
                if (_strCycleNum != value)
                {
                    _strCycleNum = value;
                    RaisePropertyChanged("StrCycleNum");
                }
            }
            get
            {
                return _strCycleNum;
            }
        }

        private OrderExcuteStatusEnum _ExcuteStatus = OrderExcuteStatusEnum.WaitForExcute;
        /// <summary>
        /// 指令执行状态
        /// </summary>
        public OrderExcuteStatusEnum ExcuteStatus
        {
            get => _ExcuteStatus;
            set
            {
                _ExcuteStatus = value;
                RaisePropertyChanged(nameof(ExcuteStatus));
            }
        }

        /// <summary>
        /// 循环数
        /// </summary>
        string _strExcuteResult = "  ";
        /// <summary>
        /// 循环数
        /// </summary>
        public string StrExcuteResult
        {
            set
            {
                if (_strExcuteResult != value)
                {
                    _strExcuteResult = value;
                    RaisePropertyChanged(nameof(StrExcuteResult));
                }
            }
            get
            {
                return _strExcuteResult;
            }
        }
        void SetIsChecked(bool? value, bool updateChildren, bool updateParent)
        {
            if (value == _isChecked)
                return;

            _isChecked = value;

            if (updateChildren && _isChecked.HasValue)
            {
                foreach (TreeItem child in Children)
                {
                    child.SetIsChecked(_isChecked, true, false);
                }
            }

            if (updateParent && Parent != null)
            {
                Parent.VerifyCheckState();
            }

            this.RaisePropertyChanged("IsChecked");
        }

        void VerifyCheckState()
        {
            bool? state = null;
            for (int i = 0; i < this.Children.Count; ++i)
            {
                bool? current = this.Children[i].IsChecked;
                if (i == 0)
                {
                    state = current;
                }
                else if (state != current)
                {
                    state = null;
                    break;
                }
            }
            this.SetIsChecked(state, false, true);
        }

        public event PropertyChangedEventHandler PropertyChanged;
        public void RaisePropertyChanged(string property)
        {
            PropertyChanged?.Invoke(this, new PropertyChangedEventArgs(property));
        }
        //public void Refresh(string propertyName)
        //{
        //    this.RaisePropertyChanged(propertyName);
        //}


        public string TextName { get; set; }


        /// <summary>
        /// 子节点集合(若集合保持Null，则在new后，页面不会更新，所以初始化不为Null)
        /// </summary>
        private ObservableCollection<TreeItem> _Children { get; set; } = new ObservableCollection<TreeItem>();

        public ObservableCollection<TreeItem> Children
        {
            get => _Children;
            set
            {
                _Children = value;
                RaisePropertyChanged(nameof(Children));
            }
        }
        bool u_Add;
        public bool Add
        {
            get => u_Add;
            set
            {
                u_Add = value;
                if (PropertyChanged != null)
                    PropertyChanged.Invoke(this, new PropertyChangedEventArgs("Add"));
            }
        }

        private ObservableCollection<TreeItem> _Root { get; set; } = new ObservableCollection<TreeItem>();
        public ObservableCollection<TreeItem> Root
        {
            get => _Root;
            set
            {
                _Root = value;
                RaisePropertyChanged(nameof(Root));
            }
        }
    }
}
