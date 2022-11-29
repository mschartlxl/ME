using CommunityToolkit.Mvvm.Messaging;
using HandyControl.Data.Enum;
using LogHelperLib;
using ME.BaseCore;
using ME.BaseCore.Instrument;
using ME.BaseCore.Models.Enums;
using ME.ControlLibrary.Model;
using Newtonsoft.Json;
using System;
using System.Collections.Generic;
using System.ComponentModel;
using System.Data.Common;
using System.Diagnostics;
using System.IO.Ports;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;

using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Animation;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using static ME.BaseCore.Models.Enums.PublicEnums;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
namespace ME.ControlLibrary.View
{
    /// <summary>
    /// UCCommandUnit.xaml 的交互逻辑
    /// </summary>
    public partial class UCCommandUnit : UserControl
    {
        int ItemIndex;
        CommandUnit _commandUnit;
        List<NodeInfo> _NodeInfos;

        ContextMenu menu = new ContextMenu();
        Window win;
        bool IsCheckItem = false;
        private TreeItem tree = new TreeItem();
        public UCCommandUnit()
        {
            InitializeComponent();
            this.Loaded += UCCommandUnit_Loaded;
        }
     
        private void UCCommandUnit_Loaded(object sender, RoutedEventArgs e)
        {
            win = Window.GetWindow(this);
            this.DataContext = this;
        }

        public void Init(CommandUnit commandUnit, string labTileName)
        {
            _commandUnit = commandUnit;
            if (_commandUnit != null)
            {
                _NodeInfos = new List<NodeInfo>();
                InitTree(_commandUnit.NodeInfos);
            }
        }
        /// <summary>
        /// 初始化树
        /// </summary>
        /// <param name="paraNodeInfos"></param>
        public void InitTree(List<NodeInfo> paraNodeInfos)
        {
            try
            {
                tree.Children.Clear();
                var treeNodeMoldes = InitTreeNodeStructureModels(paraNodeInfos);
                if (treeNodeMoldes.Count > 0)
                {
                    foreach (var item in treeNodeMoldes)
                    {
                        InitTreeNode(tree, item);
                    }
                }
                myTreeView.ItemsSource = tree.Children;
            }
            catch (Exception ex)
            {
                LogHelper.SystemError($"初始化指令集树异常：{ex.Message}", ex);
            }
        }
        /// <summary>
        /// 初始化树节点
        /// </summary>
        /// <param name="parentTreeItem"></param>
        /// <param name="childTreeNodeStructureModel"></param>
        private void InitTreeNode(TreeItem parentTreeItem, TreeNodeStructureModel childTreeNodeStructureModel)
        {
            var node = childTreeNodeStructureModel.NodeInfo;
            var obj = node.objParent;
            TreeItem child = new TreeItem();
            child.Tag = node;
            child.ItemType = node.csNodeMode.ToString();
            child.ItemText = string.Format("{0} {1}", node.NodeName, node.GetNodeName);
            child.NodeId = node.NodeId;
            child.ParentNodeId = node.ParentNodeId;
            child.IsCheckItem = node.IsEnabled;
            child.Parent = parentTreeItem;
            parentTreeItem.Children.Add(child);
            NodeInfo info = new NodeInfo();
            info.GetFromXElement(node.ToXElement());
            _NodeInfos.Add(info);

            foreach (var childMode in childTreeNodeStructureModel.Childs)
            {
                InitTreeNode(child, childMode);
            }

        }
        private void GetNodeInfo()
        {
            _NodeInfos?.Clear();
            foreach (var item in myTreeView.Items.SourceCollection)
            {
                GetNodeInfo(item as TreeItem);
            }
        }
        private void GetNodeInfo(TreeItem treeItems)
        {
            _NodeInfos.Add(treeItems.Tag);
            foreach (var parent in treeItems.Children)
            {
                GetNodeInfo(parent);
            }
        }
        /// <summary>
        /// 初始化树节点结构模型
        /// </summary>
        private List<TreeNodeStructureModel> InitTreeNodeStructureModels(List<NodeInfo> paraNodeInfos)
        {
            try
            {
                List<TreeNodeStructureModel> roots = new List<TreeNodeStructureModel>();

                var nodes = paraNodeInfos.Where(x => x.ParentNodeId == "0").ToList();
                if (nodes.Count > 0)
                {
                    foreach (var item in nodes)
                    {
                        roots.Add(new TreeNodeStructureModel(item));
                    }

                    foreach (var root in roots)
                    {
                        InitChildsNodes(root, paraNodeInfos);
                    }
                }
                return roots;
            }
            catch (Exception ex)
            {
                //LogHelper.SystemError($"初始化指令集树节点模型异常：{ex.Message}", ex);
                return new List<TreeNodeStructureModel>();
            }
        }
        /// <summary>
        /// 初始化树节点的子节点数据模型
        /// </summary>
        /// <param name="currentNode"></param>
        /// <param name="paraNodeInfos"></param>
        private void InitChildsNodes(TreeNodeStructureModel currentNode, List<NodeInfo> paraNodeInfos)
        {
            try
            {
                var childNodes = paraNodeInfos.Where(x => x.ParentNodeId == currentNode.NodeId).ToList();
                if (childNodes.Count > 0)
                {
                    foreach (var childNode in childNodes)
                    {
                        currentNode.Childs.Add(new TreeNodeStructureModel(childNode));
                    }
                    foreach (var child in currentNode.Childs)
                    {
                        InitChildsNodes(child, paraNodeInfos);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.SystemError($"初始化指令集树节点模型异常：{ex.Message}", ex);
            }
        }
        public CommandUnit GetCommandUnit()
        {
            if (_commandUnit != null)
            {
                GetNodeInfo();
                _commandUnit.NodeInfos = _NodeInfos;
            }
            return _commandUnit;
        }

        private void btnAddNode_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowMask(true);
                SelectModeWindow selectWindow = new SelectModeWindow
                {
                    Owner = win
                };
                selectWindow.ShowDialog();
                if (selectWindow.DialogResult == true)
                {
                    if (selectWindow.rbLoop.IsChecked == true)
                    {
                        LoopModeWindow loopWindow = new LoopModeWindow
                        {
                            Owner = win,
                            Title = "添加节点"
                        };
                        loopWindow.Init(new LoopMode());
                        loopWindow.ShowDialog();
                        if (loopWindow.DialogResult == true)
                        {
                            LoopMode loopMode = loopWindow.GetInfo();
                            AddItem(loopMode);
                        }
                        ShowMask(false);
                    }
                    else if (selectWindow.rbWait.IsChecked == true)
                    {
                        WaitModeWindow waitWindow = new WaitModeWindow
                        {
                            Owner = win,
                            Title = "添加节点"
                        };
                        waitWindow.Init(new WaitMode());
                        waitWindow.ShowDialog();
                        if (waitWindow.DialogResult == true)
                        {
                            WaitMode waitMode = waitWindow.GetInfo();
                            AddItem(waitMode);
                        }
                        ShowMask(false);
                    }
                    else if (selectWindow.rbCommon.IsChecked == true)
                    {
                        CommonModeWindow commonWindow = new CommonModeWindow
                        {
                            Owner = win,
                            Title = "添加节点"
                        };
                        commonWindow.ShowDialog();
                        if (commonWindow.DialogResult == true)
                        {
                            CommonMode commonMode = commonWindow.GetInfo(tree.Children);
                            AddItem(commonMode);
                        }
                        ShowMask(false);
                    }
                }
                else
                    ShowMask(false);

            }
            catch (Exception ex)
            {
                LogHelper.SystemError(ex.Message);
            }
        }
        /// <summary>
        /// 添加节点
        /// </summary>
        /// <param name="obj"></param>
        private void AddItem(object obj)
        {
            OperationItem(obj, false);
            MessageBox.Show("添加成功！", "提示", 1);
        }
        private void OperationItem(object obj, bool IsInsertItem)
        {
            TreeNodeInfo treeNodeInfo = obj as TreeNodeInfo;
            TreeItem selectItem = myTreeView.SelectedItem as TreeItem;
            if (_NodeInfos == null)
            {
                _NodeInfos = new List<NodeInfo>();
            }
            if (IsCheckItem == false || selectItem == null)
            {
                NodeInfo nodeInfo = new NodeInfo();
                nodeInfo.csNodeMode = treeNodeInfo.csNodeMode;
                nodeInfo.NodeName = treeNodeInfo.NodeName;
                nodeInfo.IsEnabled = treeNodeInfo.IsEnabled;
                nodeInfo.ParentNodeId = "0";
                nodeInfo.NodeId = UtilsFun.GetNewGuid();
                nodeInfo.XInfo = UtilsFun.GetNodeXml(treeNodeInfo, nodeInfo.csNodeMode, nodeInfo.ParentNodeId, nodeInfo.NodeId);
                var objpare = nodeInfo.objParent;
                _NodeInfos.Add(nodeInfo);

                TreeItem root = new TreeItem();
                root.Tag = nodeInfo;
                root.NodeId = nodeInfo.NodeId;
                root.ParentNodeId = nodeInfo.ParentNodeId;
                root.ItemType = nodeInfo.csNodeMode.ToString();
                root.ItemText = string.Format("{0} {1}", nodeInfo.NodeName, nodeInfo.GetNodeName);
                root.IsCheckItem = nodeInfo.IsEnabled;
                root.Parent = tree;
                tree.Children.Add(root);

                myTreeView.ItemsSource = tree.Children;

                var indexTree = root.Parent.Children.IndexOf(root);
                if (indexTree == 0)
                {
                    return;
                }
                root.Parent.Children[indexTree - 1].IsSelected = true;
                root.IsSelected = true;
            }
            else
            {
                NodeInfo selectNodeInfo = selectItem.Tag;
                if (selectNodeInfo.csNodeMode == NodeModel.LoopMode)
                {
                    NodeInfo nodeInfo1 = new NodeInfo();
                    nodeInfo1.csNodeMode = treeNodeInfo.csNodeMode;
                    nodeInfo1.NodeName = treeNodeInfo.NodeName;
                    nodeInfo1.IsEnabled = treeNodeInfo.IsEnabled;
                    if (IsInsertItem)
                    {
                        nodeInfo1.ParentNodeId = selectNodeInfo.ParentNodeId;
                    }
                    else
                    {
                        nodeInfo1.ParentNodeId = selectNodeInfo.NodeId;
                    }
                    nodeInfo1.NodeId = UtilsFun.GetNewGuid();
                    nodeInfo1.XInfo = UtilsFun.GetNodeXml(treeNodeInfo, nodeInfo1.csNodeMode, nodeInfo1.ParentNodeId, nodeInfo1.NodeId);
                    var objpare = nodeInfo1.objParent;
                    if (IsInsertItem)
                    {
                        int nodeinfoIndex = UtilsFun.GetIndex(_NodeInfos, selectNodeInfo);
                        if (_NodeInfos.Count == nodeinfoIndex + 1)
                        {
                            _NodeInfos.Add(nodeInfo1);
                        }
                        else
                        {
                            _NodeInfos.Insert(nodeinfoIndex + 1, nodeInfo1);
                        }
                    }
                    else
                    {
                        _NodeInfos.Add(nodeInfo1);
                    }

                    //添加子集
                    TreeItem newItem = new TreeItem
                    {
                        Tag = nodeInfo1
                    };
                    newItem.NodeId = nodeInfo1.NodeId;
                    newItem.ParentNodeId = nodeInfo1.ParentNodeId;
                    newItem.ItemType = nodeInfo1.csNodeMode.ToString();
                    newItem.ItemText = string.Format("{0} {1}", nodeInfo1.NodeName, nodeInfo1.GetNodeName);
                    newItem.IsCheckItem = nodeInfo1.IsEnabled;
                    if (IsInsertItem)
                    {
                        newItem.Parent = selectItem.Parent;
                        ItemIndex = selectItem.Parent.Children.IndexOf(selectItem);
                        if (selectItem.Parent.Children.Count == ItemIndex + 1)
                        {
                            selectItem.Parent.Children.Add(newItem);
                        }
                        else
                        {
                            selectItem.Parent.Children.Insert(ItemIndex + 1, newItem);
                        }
                    }
                    else
                    {
                        newItem.Parent = selectItem;
                        selectItem.Children.Add(newItem);
                        var indexTree = newItem.Parent.Children.IndexOf(newItem);
                        if (indexTree == 0)
                        {
                            return;
                        }
                        newItem.Parent.Children[indexTree - 1].IsSelected = true;
                        newItem.IsSelected = true;
                    }
                }
                else
                {
                    ItemIndex = selectItem.Parent.Children.IndexOf(selectItem);
                    int nodeinfoIndex = UtilsFun.GetIndex(_NodeInfos, selectNodeInfo);
                    NodeInfo nodeInfo1 = new NodeInfo();
                    nodeInfo1.csNodeMode = treeNodeInfo.csNodeMode;
                    nodeInfo1.NodeName = treeNodeInfo.NodeName;
                    nodeInfo1.IsEnabled = treeNodeInfo.IsEnabled;

                    nodeInfo1.ParentNodeId = selectNodeInfo.ParentNodeId;
                    nodeInfo1.NodeId = UtilsFun.GetNewGuid();
                    nodeInfo1.XInfo = UtilsFun.GetNodeXml(treeNodeInfo, nodeInfo1.csNodeMode, nodeInfo1.ParentNodeId, nodeInfo1.NodeId);
                    var objpare = nodeInfo1.objParent;
                    if (_NodeInfos.Count == nodeinfoIndex + 1)
                    {
                        _NodeInfos.Add(nodeInfo1);
                    }
                    else
                    {
                        _NodeInfos.Insert(nodeinfoIndex + 1, nodeInfo1);
                    }

                    //添加同级
                    TreeItem newItem = new TreeItem
                    {
                        Tag = nodeInfo1
                    };
                    newItem.NodeId = nodeInfo1.NodeId;
                    newItem.ParentNodeId = nodeInfo1.ParentNodeId;
                    newItem.ItemType = nodeInfo1.csNodeMode.ToString();
                    newItem.ItemText = string.Format("{0} {1}", nodeInfo1.NodeName, nodeInfo1.GetNodeName);
                    newItem.IsCheckItem = nodeInfo1.IsEnabled;
                    newItem.Parent = selectItem.Parent;
                    if (selectItem.Parent.Children.Count == ItemIndex + 1)
                    {
                        selectItem.Parent.Children.Add(newItem);
                        var indexTree = newItem.Parent.Children.IndexOf(newItem);
                        if (indexTree == 0)
                        {
                            return;
                        }
                        newItem.Parent.Children[indexTree - 1].IsSelected = true;
                        newItem.IsSelected = true;
                    }
                    else
                    {
                        selectItem.Parent.Children.Insert(ItemIndex + 1, newItem);
                    }
                }
            }

        }
        /// <summary>
        /// 树显示蒙板
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowMaskTree(bool isShow)
        {
            if (isShow == true)
                MaskTree.Visibility = Visibility.Visible;
            else
                MaskTree.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 是否显示蒙板
        /// </summary>
        public void ShowMask(bool isShow)
        {
            MethodInfo showMask = win.GetType().GetMethod("ShowMask");
            object[] paras = new object[1] { isShow };
            showMask.Invoke(win, paras);
        }
        static DependencyObject VisualUpwardSearch<T>(DependencyObject source)
        {
            while (source != null && source.GetType() != typeof(T))
                source = VisualTreeHelper.GetParent(source);

            return source;
        }
        #region 向上移动 向下移动

        /// <summary>
        /// 移动节点 每次移动都是 +1、-1
        /// 同时修改节点信息中的位置和树节点中的信息
        /// </summary>
        /// <param name="nodeMoveType">移动方向</param>
        /// <param name="selectTreeItem">移动的树节点</param>
        private void TreeNodeMove(NodeMoveType nodeMoveType, TreeItem selectTreeItem)
        {
            switch (nodeMoveType)
            {
                case NodeMoveType.UP:
                    TreeNodeMove(-1, 0, selectTreeItem);
                    break;
                case NodeMoveType.Down:
                    TreeNodeMove(+1, selectTreeItem.Parent.Children.Count, selectTreeItem);
                    break;
                default:
                    break;
            }
        }

        /// <summary>
        /// 移动节点
        /// </summary>
        /// <param name="moveNum">移动节点数向上-1 向下+1</param>
        /// <param name="criticalNum">移动的上限和下限，临界值</param>
        /// <param name="selectTreeItem">所选的树节点</param>
        private void TreeNodeMove(int moveNum, int criticalNum, TreeItem selectTreeItem)
        {
            try
            {
                //获取所选中节点的父节点并获取所在集合的索引
                var parent = selectTreeItem.Parent;
                var indexTree = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);

                parent.Children.Remove(selectTreeItem);
                parent.Children.Insert(indexTree == criticalNum ? indexTree : indexTree + moveNum, selectTreeItem);
            }
            catch (Exception ex)
            {
                throw ex;
            }
        }
        public TreeViewItem GetTreeViewItem(TreeItem treeItem)
        {
            foreach (var item in myTreeView.Items)
            {
                var myTreeViewItem = myTreeView.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if (myTreeViewItem.Items.Count == 0 && (item as TreeItem).NodeId == treeItem.NodeId)
                {
                    return myTreeViewItem;
                }
                if ((item as TreeItem).NodeId == treeItem.NodeId)
                {
                    return myTreeViewItem;
                }
                if (myTreeViewItem?.Items.Count > 0)
                {
                    var tempItem = GetTreeViewItem(treeItem, myTreeViewItem);
                    if ((tempItem?.Header as TreeItem)?.NodeId == treeItem.NodeId)
                    {
                        return tempItem;
                    }
                }
            }
            return null;
        }
        private TreeViewItem GetTreeViewItem(TreeItem treeItem, TreeViewItem treeViewItem)
        {
            foreach (var item in treeViewItem.Items)
            {
                var myTreeViewItem = treeViewItem.ItemContainerGenerator.ContainerFromItem(item) as TreeViewItem;
                if ((myTreeViewItem == null || myTreeViewItem.Items.Count == 0) && (item as TreeItem).NodeId == treeItem.NodeId)
                {
                    return myTreeViewItem;
                }
                if ((item as TreeItem).NodeId == treeItem.NodeId)
                {
                    return myTreeViewItem;
                }
                if (myTreeViewItem?.Items.Count > 0)
                {
                    var tempItem = GetTreeViewItem(treeItem, myTreeViewItem);
                    if ((tempItem?.Header as TreeItem)?.NodeId == treeItem.NodeId)
                    {
                        return tempItem;
                    }
                }
            }
            return null;
        }
        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="obj"></param>
        private void InsertItem(object obj)
        {
            OperationItem(obj, true);
            MessageBox.Show("插入成功！", "提示", 1);
        }
        // 获得右键上下文菜单
        ContextMenu GetItemRightContextMenu()
        {
            menu.Items.Clear();
            if (myTreeView.SelectedItem != null)
            {
                MenuItem menuEdit = new MenuItem();
                menuEdit.Header = "编辑";
                menuEdit.Click += MenuEdit_Click;
                menu.Items.Add(menuEdit);
                MenuItem menuDel = new MenuItem();
                menuDel.Header = "删除";
                menuDel.Click += MenuDel_Click;
                menu.Items.Add(menuDel);
                //if (UtilsFun._AbtInstrument.IsOpen)
                {
                    MenuItem menuZx = new MenuItem();
                    menuZx.Header = "执行";
                    menuZx.Click += MenuZx_Click;
                    menu.Items.Add(menuZx);
                }
                MenuItem menuCopy = new MenuItem();
                menuCopy.Header = "复制";
                menuCopy.Click += MenuCopy_Click;
                menu.Items.Add(menuCopy);
            }
            if (GetDataPresent())
            {
                MenuItem menuPaste = new MenuItem();
                menuPaste.Header = "粘贴";
                menuPaste.Click += MenuPaste_Click;
                menu.Items.Add(menuPaste);
            }
            return menu;
        }
        private bool GetDataPresent()
        {
            try
            {
                IDataObject iData = Clipboard.GetDataObject();
                if (iData.GetDataPresent(DataFormats.Text))
                {
                    //如果剪贴板中的数据是文本格式 
                    string json = (string)iData.GetData(DataFormats.Text);//检索与指定格式相关联的数据 
                    TreeItem treeItemModel = JsonConvert.DeserializeObject<TreeItem>(json);
                    if (treeItemModel != null)
                    {
                        GlobalVariables.Instance.TreeItemCopy = treeItemModel;
                        return true;
                    }
                }
                return false;
            }
            catch (Exception ex)
            {
                LogHelperLib.LogHelper.SystemError("获取粘贴内容出错");
                return false;
            }
        }
        /// <summary>
        /// 删除节点
        /// </summary>
        private void DeleteItem()
        {
            TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
            if (selectTreeItem != null)
            {
                //弹窗提示
                ShowMask(true);
                if (MessageBox.Show("确定要删除当前节点吗？", "删除提示", true, win) == true)
                {
                    selectTreeItem.Parent.Children.Remove(selectTreeItem);
                    ShowMask(false);
                }
                else
                {
                    ShowMask(false);
                    return;
                }
            }
        }
        private void TreeViewNodeCopy(object sender, RoutedEventArgs e)
        {
            try
            {
                GlobalVariables.Instance.TreeItemCopy = new TreeItem();
                //获取所选中的节点
                TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
                if (selectTreeItem != null)
                {
                    //实体转换成Json字符串
                    string jsonStr = JsonConvert.SerializeObject(selectTreeItem);

                    ////Json字符串转换为实体
                    //TreeItem treeItemModel = JsonConvert.DeserializeObject<TreeItem>(jsonStr);
                    //GlobalVariables.Instance.TreeItemCopy = treeItemModel;

                    Clipboard.Clear();
                    Clipboard.SetText(jsonStr);

                }

            }
            catch (Exception ex)
            {
                LogHelper.SystemError("向上移动节点错误！", ex);
            }
        }
        /// <summary>
        /// 节点粘贴
        /// </summary>
        private void TreeviewNodePaste()
        {
            try
            {
                TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
                var selectTreeItemCopy = GlobalVariables.Instance.TreeItemCopy as TreeItem;
                if (selectTreeItemCopy == null)
                {
                    return;
                }
                //实体转换成Json字符串
                string jsonStr = JsonConvert.SerializeObject(selectTreeItemCopy);
                //Json字符串转换为实体
                TreeItem copyTreeItem = JsonConvert.DeserializeObject<TreeItem>(jsonStr);
                if (selectTreeItem == null)
                {
                    var parentTreeItem = tree;
                    var parentNodId = "0";
                    TreeNodePaste(parentTreeItem, copyTreeItem, parentNodId);
                    tree.Children.Add(copyTreeItem);
                }
                else if (selectTreeItem.ItemType == NodeModel.LoopMode.ToString() && GetTreeViewItem(selectTreeItem).IsExpanded)
                {
                    var parentTreeItem = selectTreeItem;
                    var parentNodId = selectTreeItem.NodeId;
                    TreeNodePaste(parentTreeItem, copyTreeItem, parentNodId);
                    //添加子集
                    selectTreeItem.Children.Add(copyTreeItem);
                }
                else
                {
                    var parentTreeItem = selectTreeItem.Parent;
                    var parentNodId = selectTreeItem.ParentNodeId;
                    TreeNodePaste(parentTreeItem, copyTreeItem, parentNodId);
                    var index = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);
                    //选中的节点的父节点添加子集
                    selectTreeItem.Parent.Children.Insert(index + 1, copyTreeItem);
                }
                var treeViewItem = GetTreeViewItem(copyTreeItem);
                treeViewItem.Focus();
            }
            catch (Exception ex)
            {
                LogHelper.SystemError("粘贴操作错误！", ex);
            }
        }
        private void TreeNodePaste(TreeItem parentTreeItem, TreeItem copyTreeItem, string parentNodeId)
        {
            var nodeId = UtilsFun.GetNewGuid();
            copyTreeItem.ParentNodeId = parentNodeId;
            copyTreeItem.Parent = parentTreeItem;
            copyTreeItem.NodeId = nodeId;

            copyTreeItem.Tag.NodeId = nodeId;
            copyTreeItem.Tag.ParentNodeId = parentNodeId;
            copyTreeItem.Tag.XInfo = UtilsFun.GetNodeXml(copyTreeItem.Tag.objParent as TreeNodeInfo, copyTreeItem.Tag.csNodeMode, parentNodeId, nodeId);
            UpdateTreeNodeItem(copyTreeItem, nodeId);
        }
        /// <summary>
        /// 递归修改节点信息
        /// </summary>
        /// <param name="treeItem"></param>
        public void UpdateTreeNodeItem(TreeItem treeItem, string ParentNodeId)
        {
            if (treeItem != null && treeItem.Children.Count > 0)
            {
                foreach (var item in treeItem.Children)
                {
                    var nodeId = UtilsFun.GetNewGuid();
                    item.Parent = treeItem;
                    item.NodeId = nodeId;
                    item.ParentNodeId = ParentNodeId;

                    item.Tag.NodeId = nodeId;
                    item.Tag.ParentNodeId = ParentNodeId;
                    item.Tag.XInfo = UtilsFun.GetNodeXml(item.Tag.objParent as TreeNodeInfo, item.Tag.csNodeMode, ParentNodeId, nodeId);
                    if (item.Children.Count > 0)
                    {
                        UpdateTreeNodeItem(item, item.NodeId);
                    }
                }
            }
        }
        #endregion
        /// <summary>
        /// 指令执行
        /// </summary>
        /// <param name="cancelFun"></param>
        /// <param name="selectItem"></param>
        private void CmdExcute_Singleton(System.Func<bool> cancelFun, TreeItem selectItem, bool ismenuexecute)
        {
            try
            {

                while (!CommandMessage.Instance.Flag)
                {
                    if (CommandMessage.Instance.tokenSource.IsCancellationRequested)
                    {
                        return;
                    }
                    Thread.Sleep(10);
                }
                if (CommandMessage.Instance.tokenSource.IsCancellationRequested)
                {
                    return;
                }
                selectItem.IsSelected = true;
                if (selectItem != null)
                {

                    this.Dispatcher.Invoke(() => { selectItem.ExcuteStatus = OrderExcuteStatusEnum.Excuting; });
                    Thread.Sleep(100);
                    NodeInfo selectNodeInfo = selectItem.Tag;
                    if (selectNodeInfo != null)
                    {
                        switch (selectNodeInfo.csNodeMode)
                        {
                            case NodeModel.LoopMode:
                                {
                                    LoopMode loopMode = selectNodeInfo.objParent as LoopMode;
                                    int CycleNumber = loopMode.CycleNumber;
                                    WeakReferenceMessenger.Default.Send(new MessageInfo() { Flag=ismenuexecute});
                                    do
                                    {
                                        //循环数
                                        // selectItem.StrCycleNum = " " + CycleNumber.ToString() + " ";
                                        if (!CommandMessage.Instance.tokenSource.IsCancellationRequested)
                                        {
                                            while (!CommandMessage.Instance.Flag)
                                            {
                                                Thread.Sleep(10);
                                            }
                                            selectItem.StrCycleNum = ((string.IsNullOrWhiteSpace(selectItem.StrCycleNum) ? 0 : Convert.ToInt32(selectItem.StrCycleNum)) + 1).ToString();
                                        }
                                        foreach (var item in selectItem.Children)
                                        {
                                            if (CommandMessage.Instance.tokenSource.IsCancellationRequested)
                                            {
                                                return;
                                            }
                                            item.StrExcuteResult = "";
                                            CmdExcute_Singleton(cancelFun, item,ismenuexecute);
                                        }
                                        CycleNumber--;

                                    } while (CycleNumber >= 1 && (cancelFun != null ? !cancelFun() : true));
                                    break;
                                }
                            case NodeModel.CommonMode:
                                {
                                    CommonMode commonMode = selectNodeInfo.objParent as CommonMode;
                                    try
                                    {
                                        var result = FunCommonMode(cancelFun, commonMode);
                                    }
                                    catch (Exception e)
                                    {
                                        throw e;
                                    }
                                    finally
                                    {
                                        WeakReferenceMessenger.Default.Send(new MessageInfo() { Flag = ismenuexecute });
                                    }

                                    break;
                                }
                            case NodeModel.WaitMode:
                                {
                                    WaitMode waitMode = selectNodeInfo.objParent as WaitMode;
                                    FunWaitMode(cancelFun, waitMode);
                                    WeakReferenceMessenger.Default.Send(new MessageInfo() { Flag = ismenuexecute });
                                    break;
                                }

                            case NodeModel.ShowMode:
                                break;
                            default:
                                break;
                        }
                    }

                    selectItem.IsSelected = false;
                    selectItem.IsSelected = true;
                    this.Dispatcher.Invoke(() => { selectItem.ExcuteStatus = OrderExcuteStatusEnum.WaitForExcute; });

                }
            }
            catch (Exception ex)
            {
                this.Dispatcher.Invoke(() =>
                {
                    selectItem.ExcuteStatus = OrderExcuteStatusEnum.ExcuteFailed;
                    LogHelper.SystemError("执行指令失败：" + ex.Message);
                });

            }

        }
        /// <summary>
        /// 描述信息
        /// </summary>
        string _strDescInfo = "";
        private void FunWaitMode(Func<bool> cancelFun, WaitMode waitMode)
        {
            if (waitMode.IsEnabled && waitMode.WaitTime > 0)
            {
                if (waitMode.NodeName != "" && waitMode.NodeName != "WaitMode" && waitMode.NodeName != "WaitNode")
                {
                    _strDescInfo = string.Format("{0},{1}", waitMode.GetNodeName, waitMode.NodeName);
                }
                else
                {
                    _strDescInfo = string.Format("{0}", waitMode.GetNodeName);
                }
                // LOGHelper.Info(GetAllDescInfo(_strDescInfo, _strCycleNum));

                DateTime dtime = DateTime.Now.AddMilliseconds(waitMode.WaitTime);
                string syTime = "";
                while (cancelFun != null ? !cancelFun() : true)
                {
                    if (dtime < DateTime.Now)
                    {
                        break;
                    }
                    syTime = UtilsFun.GetTimeCha_End_Begin(DateTime.Now, dtime);
                    if (waitMode.WaitTime < 100)
                    {
                        Thread.Sleep(waitMode.WaitTime);
                    }
                    else
                    {
                        Thread.Sleep(100);
                    }
                }
            }
        }
        private byte[] FunCommonMode(Func<bool> cancelFun, CommonMode cmdMode)
        {

            List<Task> tasklist = new List<Task>();
            var cmdparas = cmdMode.CommandParas;
            var res = new List<int>();
            foreach (var item in cmdparas)
            {
                Thread.Sleep(50);
                if (item.ParaType == 1)
                {
                    var iteminfolist = item.ParaName.Split(',');
                    var number = Convert.ToInt32(iteminfolist[0].ToSecond());

                    byte[] senddataS = InstructionConfig.cmdPumpSpeed;
                    senddataS[1] = Convert.ToByte(number.ToString("X2"), 16);
                    var speed = Convert.ToInt32(iteminfolist[3].ToSecond());
                    var bytespeed = ABTInstrument.ConvertHex2(speed);

                    senddataS[7] = bytespeed[0];//(speedconv.Substring(0, 2));
                    senddataS[8] = bytespeed[1];
                    var senddataNewS = CRC.GetNewCrcArray(senddataS);
                    var temps = senddataNewS.Clone() as byte[];
                    Send(cancelFun, cmdMode, temps, UtilsFun._AbtInstrument.SerialPump);

                    byte[] senddata = InstructionConfig.cmdPumpDistance;

                    var distance = Convert.ToInt32(iteminfolist[2].ToSecond());
                    senddata[1] = Convert.ToByte(number.ToString("X2"), 16);
                    byte[] arrdistance = ABTInstrument.ConvertHex2(distance);
                    senddata[3] = arrdistance[0];
                    senddata[4] = arrdistance[1];
                    var senddatanew = CRC.GetNewCrcArray(senddata);
                    var temp = senddatanew.Clone() as byte[];
                    Send(cancelFun, cmdMode, temp, UtilsFun._AbtInstrument.SerialPump);


                }
                if (item.ParaType == 2)
                {
                    var iteminfolist = item.ParaName.Split(',');
                    var number = Convert.ToInt32(iteminfolist[0].ToSecond());
                    var hole = Convert.ToInt32(iteminfolist[2].ToSecond());
                    if (hole != 0)//准备开孔
                    {
                        byte[] senddataH = InstructionConfig.cmdReCircleHole;
                        senddataH[1] = Convert.ToByte(number.ToString("X2"), 16);
                        var silbhole = -1;
                        if (hole - 1 > 0)
                        {
                            silbhole = hole - 1;
                        }
                        else
                        {
                            silbhole = 8;
                        }
                        senddataH[3] = Convert.ToByte(hole);
                        senddataH[4] = Convert.ToByte(silbhole);
                        var senddatanew = CRC.GetNewCrcArray(senddataH);
                        var temp = senddatanew.Clone() as byte[];
                        Send(cancelFun, cmdMode, temp, UtilsFun._AbtInstrument.SerialPump);
                    }
                    else
                    {
                        //准备复位，关
                        byte[] senddataH = InstructionConfig.cmdReCircleReset;
                        senddataH[1] = Convert.ToByte(number.ToString("X2"), 16);
                        var senddatanew = CRC.GetNewCrcArray(senddataH);
                        var temp = senddatanew.Clone() as byte[];
                        Send(cancelFun, cmdMode, temp, UtilsFun._AbtInstrument.SerialPump);
                    }
                }
                if (item.ParaType == 3)
                {
                    var iteminfolist = item.ParaName.Split(',');
                    var number = Convert.ToInt32(iteminfolist[0].ToSecond());
                    if (iteminfolist[2].ToSecond() == "1")
                    {
                        res.Add(1 << (number - 1));
                    }
                    else
                    {
                        res.Add(0);
                    }
                }

            }

            //var switchlist=  cmdparas.Where(t => t.ParaType == 3).ToList();
            if (res.Count > 0)
            {
                var m = 0;
                for (var i = 0; i < res.Count; i++)
                {
                    m = m | res[i];
                }
                var x16 = m.ToString("X8");//转换为4个字节的十六进制
                var senddatas = InstructionConfig.cmdSwitchReset.ToList();
                senddatas.RemoveRange(7, 4);
                var x16reverse = x16.ToByteArray(16).Reverse();//字节排序下
                senddatas.AddRange(x16reverse);
                var crcarry = CRC.CRC16(senddatas.ToArray());
                senddatas.Add(crcarry[1]);
                senddatas.Add(crcarry[0]);
                Send(cancelFun, cmdMode, senddatas.ToArray(), UtilsFun._AbtInstrument.SerialSwitch);

            }



            //查询指令
            foreach (var item in cmdparas)
            {
                if (item.ParaType == 1)
                {
                    var iteminfolist = item.ParaName.Split(',');
                    byte[] senddata = InstructionConfig.cmdPumpDistanceGet;
                    var number = Convert.ToInt32(iteminfolist[0].ToSecond());
                    var targetdistance = Convert.ToInt32(iteminfolist[2].ToSecond());
                    senddata[1] = Convert.ToByte(number.ToString("X2"), 16);

                    var senddatanew = CRC.GetNewCrcArray(senddata);
                    var temp = senddatanew.Clone() as byte[];
                    var actdistance = -1;//距离不能为0，所以默认值是-1
                    var overdateTime = DateTime.Now.AddMilliseconds((double)(cmdMode.Overtime * 1000));
                    while (targetdistance != actdistance)
                    {

                        if (overdateTime < DateTime.Now)
                        {
                            MessageBox.Show($"编号：'{number}',注射泵故障，请检查!");
                            break;
                        }

                        var tasksingle = Task.Run(() =>
                {
                    return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, UtilsFun._AbtInstrument.SerialPump, cmdMode.Overtime);
                });
                        tasksingle.Wait();
                        var taskresult = tasksingle.Result;
                        var str16 = CRC.byteToHexStr(taskresult.ToArray());
                        var strlist = str16.Split(' ');
                        actdistance = Convert.ToInt32(strlist[4] + strlist[3], 16);

                    }

                }
                if (item.ParaType == 2)
                {
                    var iteminfolist = item.ParaName.Split(',');
                    byte[] senddata = InstructionConfig.cmdReCircleHoleGet;
                    var number = Convert.ToInt32(iteminfolist[0].ToSecond());
                    var targethole = Convert.ToInt32(iteminfolist[2].ToSecond());
                    senddata[1] = Convert.ToByte(number.ToString("X2"), 16);

                    var senddatanew = CRC.GetNewCrcArray(senddata);
                    var temp = senddatanew.Clone() as byte[];
                    var result = new byte[] { };
                    var overdateTime = DateTime.Now.AddMilliseconds((double)(cmdMode.Overtime * 1000));
                    if (targethole != 0)
                    {
                        while (!(result.Count() == 8 && result[4] == 0 && result[3] == targethole))
                        {

                            if (overdateTime < DateTime.Now)
                            {
                                MessageBox.Show($"编号：'{number}',旋切阀故障，请检查!");
                                break;
                            }

                            var tasksingle = Task.Run(() =>
                            {
                                return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, UtilsFun._AbtInstrument.SerialPump, cmdMode.Overtime);
                            });
                            tasksingle.Wait();
                            result = tasksingle.Result;

                        }
                    }
                    else
                    {
                        while (!(result.Count() == 8 && result[4] == 08 && result[3] == 01))//查询是否复位
                        {

                            if (overdateTime < DateTime.Now)
                            {
                                MessageBox.Show($"编号：'{number}',旋切阀故障，请检查!");
                                break;
                            }

                            var tasksingle = Task.Run(() =>
                            {
                                return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, UtilsFun._AbtInstrument.SerialPump, cmdMode.Overtime);
                            });
                            tasksingle.Wait();
                            result = tasksingle.Result;

                        }
                    }


                }

            }
            ////List<byte> Commands0 = cmdMode.CommandParas;
            //if (cmdMode.IsEnabled && Commands0 != null && !cancelFun())
            //{
            //    byte[] sendCommand = Commands0.ToArray();
            //    byte[] read = null;
            //    read = UtilsFun._AbtInstrument.Send_16(cancelFun, sendCommand, cmdMode.IsReadAnswerData, cmdMode.Overtime);
            //    return read;
            //}
            return null;
        }
        private void Send(Func<bool> cancelFun, CommonMode cmdMode, byte[] temp, SerialPort serialPort)
        {
            this.Dispatcher.Invoke(() =>
            {
                CommandMessage.Instance.Messages.Insert(0, new MessageCmd()
                {
                    TimeCmd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSendCmd = "发",
                    ContentCmd = BitConverter.ToString(temp)
                });
            });

            var tasksingle = Task.Run(() =>
            {
                return UtilsFun._AbtInstrument.Send_16(cancelFun, temp, true, serialPort, cmdMode.Overtime);
            });
            tasksingle.Wait();
            var taskresult = tasksingle.Result;
            this.Dispatcher.Invoke(() =>
            {
                CommandMessage.Instance.Messages.Insert(0, new MessageCmd()
                {
                    TimeCmd = DateTime.Now.ToString("yyyy-MM-dd HH:mm:ss"),
                    IsSendCmd = "收",
                    ContentCmd = taskresult == null ? "" : BitConverter.ToString(taskresult)
                });
            });
        }
        TreeItem _Test_selectItem = new TreeItem();
        /// <summary>
        /// 执行
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        public  void MenuZx_Click(object sender, RoutedEventArgs e)
        {
            SelectItemExec();
        }
        private async void SelectItemExec()
        {
            btnSingleRun.IsEnabled = false;
            btnSinglePause.IsEnabled = true;
            btnSingleStop.IsEnabled = true;
            CommandMessage.Instance.tokenSource = new CancellationTokenSource();
            CommandMessage.Instance.Flag = true;
            await ExecuteCommand(new object(), true);
            btnSingleRun.IsEnabled = true;
            btnSinglePause.IsEnabled = false;
            btnSingleStop.IsEnabled = false;
        }
        public async Task ExecuteCommand(object sender, bool ismenuexecute)
        {
            ShowMaskTree(true);
             TreeItem selectItem = new TreeItem();
            if (sender is TreeItem treeViewItem)
            {
                selectItem = treeViewItem;
            }
            else
            {
                selectItem = myTreeView.SelectedItem as TreeItem;
            }
            if (selectItem != null)
            {
                foreach (var item in selectItem.Children)
                {
                    item.StrExcuteResult = "";
                }
                if (selectItem.Children.Count == 0)
                {
                    selectItem.StrExcuteResult = "";
                }
                _Test_selectItem = selectItem;
                selectItem.StrCycleNum = " ";
  
                DateTime mStartTime = System.DateTime.Now;
                System.Func<bool> cancelFun = () =>
                {
                    if (CommandMessage.Instance.tokenSource.IsCancellationRequested)
                    {
                        return true;
                    }
                    else
                    {
                        return false;
                    }
                };
                try
                {
                    // this.Dispatcher.Invoke(() => { ShowMask(true); });
                    await Task.Run(() => { CmdExcute_Singleton(cancelFun, _Test_selectItem,ismenuexecute); }, CommandMessage.Instance.Token);
                    //_Test_selectItem.IsSelected = true;
                }
                catch(Exception er)
                {
                    
                }
                finally
                {
                    //this.Dispatcher.Invoke(() => { ShowMask(false); });
                }

            }
            ShowMaskTree(false);
        }

        /// <summary>
        /// 编辑
        /// </summary>
        private void MenuEdit_Click(object sender, RoutedEventArgs e)
        {
            UpdateItem();
        }
        /// <summary>
        /// 删除
        /// </summary>
        private void MenuDel_Click(object sender, RoutedEventArgs e)
        {
            DeleteItem();
        }
        /// <summary>
        /// 复制
        /// </summary>
        private void MenuCopy_Click(object sender, RoutedEventArgs e)
        {
            TreeViewNodeCopy(null, null);
        }
        /// <summary>
        /// 粘贴
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void MenuPaste_Click(object sender, RoutedEventArgs e)
        {
            TreeviewNodePaste();
        }
        private void MyTreeView_PreviewMouseLeftButtonDown(object sender, MouseButtonEventArgs e)
        {
            var item = VisualUpwardSearch<TreeViewItem>(e.OriginalSource as DependencyObject) as TreeViewItem;
            if (item != null)
            {
                if (item.IsSelected)
                {
                    item.IsSelected = false;
                }
                IsCheckItem = true;
                TabIndex = item.TabIndex;
            }
            else
            {
                IsCheckItem = false;
            }
        }
        private void btnDel_Click(object sender, RoutedEventArgs e)
        {
            TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
            if (selectTreeItem != null)
            {
                //弹窗提示
                ShowMask(true);
                if (MessageBox.Show("确定要删除当前节点吗？", "删除提示", true, win) == true)
                {
                    var indexTree = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);
                    if (indexTree == 0)
                    {
                        selectTreeItem.Parent.Children.Remove(selectTreeItem);
                        ShowMask(false);
                        return;
                    }
                    selectTreeItem.Parent.Children[indexTree - 1].IsSelected = true;

                    selectTreeItem.Parent.Children.Remove(selectTreeItem);
                    ShowMask(false);
                }
                else
                {
                    ShowMask(false);
                    return;
                }
            }
        }

        private void btnNodeMoveUp_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //获取所选中的节点
                TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
                var indexTree = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);
                if (indexTree == 0)
                {
                    MessageBox.Show("当前节点已移动到最高节点！", "提示", true, win);
                    return;
                }
                TreeNodeMove(NodeMoveType.UP, selectTreeItem);
                selectTreeItem.IsSelected = true;
                var treeViewItem = GetTreeViewItem(selectTreeItem);
                treeViewItem.Focus();
            }
            catch (Exception ex)
            {
                LogHelper.SystemError("向上移动节点错误！", ex);
            }
        }

        private void btnNodeMoveDown_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                //获取所选中的节点
                TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
                var indexTree = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);
                if (indexTree == selectTreeItem.Parent.Children.Count - 1)
                {
                    MessageBox.Show("当前节点已移动到最低节点！", "提示", true, win);
                    return;
                }
                TreeNodeMove(NodeMoveType.Down, selectTreeItem);

                var treeViewItem = GetTreeViewItem(selectTreeItem);
                treeViewItem.Focus();
            }
            catch (Exception ex)
            {
                LogHelper.SystemError("向下移动节点错误！", ex);
            }
        }

        private void btnMoveUp_Click(object sender, RoutedEventArgs e)
        {
            TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
            if (selectTreeItem == null)
            {
                return;
            }
            var indexTree = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);
            if (indexTree == 0)
            {
                return;
            }
            selectTreeItem.Parent.Children[indexTree - 1].IsSelected = true;
        }

        private void btnMoveDown_Click(object sender, RoutedEventArgs e)
        {
            TreeItem selectTreeItem = myTreeView.SelectedItem as TreeItem;
            if (selectTreeItem == null)
            {
                return;
            }
            var indexTree = selectTreeItem.Parent.Children.IndexOf(selectTreeItem);
            if (indexTree == selectTreeItem.Parent.Children.Count - 1)
            {
                return;
            }
            selectTreeItem.Parent.Children[indexTree + 1].IsSelected = true;
        }

        private void btnEdit_Click(object sender, RoutedEventArgs e)
        {
            UpdateItem();
        }
        /// <summary>
        /// 编辑节点
        /// </summary>
        private void UpdateItem()
        {
            try
            {
                TreeItem selectItem = myTreeView.SelectedItem as TreeItem;
                if (selectItem != null)
                {
                    ShowMask(true);
                    NodeInfo selectNodeInfo = selectItem.Tag;
                    if (selectNodeInfo.csNodeMode == NodeModel.LoopMode)
                    {
                        LoopMode loopMode = selectNodeInfo.objParent as LoopMode;
                        LoopModeWindow loopWindow = new LoopModeWindow
                        {
                            Owner = win,
                            Title = "编辑节点"
                        };
                        loopWindow.Init(loopMode);
                        loopWindow.ShowDialog();
                        if (loopWindow.DialogResult == true)
                        {
                            loopMode = loopWindow.GetInfo();
                            selectNodeInfo.NodeName = loopMode.NodeName;
                            selectNodeInfo.IsEnabled = loopMode.IsEnabled;
                            selectNodeInfo.XInfo = loopMode.ToXElement();

                            selectItem.ItemText = string.Format("{0} {1}", loopMode.NodeName, loopMode.GetNodeName);
                            selectItem.IsCheckItem = loopMode.IsEnabled;
                        }
                        ShowMask(false);
                    }
                    else if (selectNodeInfo.csNodeMode == NodeModel.WaitMode)
                    {
                        WaitMode waitMode = selectNodeInfo.objParent as WaitMode;
                        WaitModeWindow waitWindow = new WaitModeWindow
                        {
                            Owner = win,
                            Title = "编辑节点"
                        };
                        waitWindow.Init(waitMode);
                        waitWindow.ShowDialog();
                        if (waitWindow.DialogResult == true)
                        {
                            waitMode = waitWindow.GetInfo();
                            selectNodeInfo.NodeName = waitMode.NodeName;
                            selectNodeInfo.IsEnabled = waitMode.IsEnabled;
                            selectNodeInfo.XInfo = waitMode.ToXElement();

                            selectItem.ItemText = string.Format("{0} {1}", waitMode.NodeName, waitMode.GetNodeName);
                            selectItem.IsCheckItem = waitMode.IsEnabled;
                        }
                        ShowMask(false);
                    }
                    else if (selectNodeInfo.csNodeMode == NodeModel.CommonMode)
                    {
                        CommonMode commonMode = selectNodeInfo.objParent as CommonMode;
                        CommonModeWindow commonWindow = new CommonModeWindow
                        {
                            Owner = win,
                            Title = "编辑节点"
                        };
                        commonWindow.Init(commonMode);
                        commonWindow.ShowDialog();
                        if (commonWindow.DialogResult == true)
                        {
                            commonMode = commonWindow.GetInfo(tree.Children);
                            selectNodeInfo.NodeName = commonMode.NodeName;
                            selectNodeInfo.XInfo = commonMode.ToXElement();
                            selectItem.ItemText = string.Format("{0} {1}", commonMode.NodeName, commonMode.GetNodeName);
                        }
                        ShowMask(false);
                    }

                }
            }
            catch (Exception ex)
            {
                LogHelper.SystemError(ex.Message);
                ShowMask(false);
            }
        }
        /// <summary>
        /// 鼠标双击前
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myTreeView_PreviewMouseDoubleClick(object sender, MouseButtonEventArgs e)
        {
            UpdateItem();
        }
        /// <summary>
        /// 插入节点
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void btnInsert_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowMask(true);
                SelectModeWindow selectWindow = new SelectModeWindow
                {
                    Owner = win
                };
                selectWindow.ShowDialog();
                if (selectWindow.DialogResult == true)
                {
                    if (selectWindow.rbLoop.IsChecked == true)
                    {
                        LoopModeWindow loopWindow = new LoopModeWindow
                        {
                            Owner = win
                        };
                        loopWindow.Init(new LoopMode());
                        loopWindow.ShowDialog();
                        if (loopWindow.DialogResult == true)
                        {
                            LoopMode loopMode = loopWindow.GetInfo();
                            InsertItem(loopMode);
                        }
                        ShowMask(false);
                    }
                    else if (selectWindow.rbWait.IsChecked == true)
                    {
                        WaitModeWindow waitWindow = new WaitModeWindow
                        {
                            Owner = win
                        };
                        waitWindow.Init(new WaitMode());
                        waitWindow.ShowDialog();
                        if (waitWindow.DialogResult == true)
                        {
                            WaitMode waitMode = waitWindow.GetInfo();
                            InsertItem(waitMode);
                        }
                        ShowMask(false);
                    }
                    else if (selectWindow.rbCommon.IsChecked == true)
                    {
                        CommonModeWindow commonWindow = new CommonModeWindow
                        {
                            Owner = win
                        };
                        commonWindow.Init(new CommonMode());
                        commonWindow.ShowDialog();
                        if (commonWindow.DialogResult == true)
                        {
                            CommonMode commonMode = commonWindow.GetInfo(tree.Children);
                            InsertItem(commonMode);
                        }
                        ShowMask(false);
                    }

                }
                else
                    ShowMask(false);
            }
            catch (Exception ex)
            {
                LogHelper.SystemError(ex.Message);
            }
        }
        /// <summary>
        /// 树 右键 获取菜单
        /// 判断是否是点击到空白处
        /// </summary>
        /// <param name="sender"></param>
        /// <param name="e"></param>
        private void myTreeView_MouseRightButtonDown(object sender, MouseButtonEventArgs e)
        {
            if (e.OriginalSource is Grid && myTreeView.SelectedItem != null)
            {
                if (myTreeView.SelectedItem is TreeViewItem)
                {
                    (myTreeView.SelectedItem as TreeViewItem).IsSelected = false;
                }
                else
                {
                    TreeViewItem item = myTreeView.ItemContainerGenerator.ContainerFromIndex(0) as TreeViewItem;
                    if (item != null)
                    {
                        item.IsSelected = true;
                        item.IsSelected = false;
                    }
                }
            }
            myTreeView.ContextMenu = GetItemRightContextMenu();
        }

        private  void btnSingleRun_Click(object sender, RoutedEventArgs e)
        {
            if (MaskTree.Visibility == Visibility.Visible) 
            {
                MessageBox.Show("程序运行中,请稍后....！", "提示", 1);
                return;
            }
            SelectItemExec();
        }

        private void btnSinglePause_Click(object sender, RoutedEventArgs e)
        {
            if (btnSinglePause.Content.ToString() == "暂停")
            {
                CommandMessage.Instance.Flag = false;
                btnSinglePause.Content = "继续";
                ShowMaskTree(false);
            }
            else
            {
                CommandMessage.Instance.Flag = true;
                btnSinglePause.Content = "暂停";
                ShowMaskTree(true);
            }
        }

        private void btnSingleStop_Click(object sender, RoutedEventArgs e)
        {
            CommandMessage.Instance.tokenSource.Cancel();
            btnSingleStop.IsEnabled = false;
        }
    }
    /// <summary>
    /// 树节点数据结构模型
    /// </summary>
    class TreeNodeStructureModel
    {
        public NodeInfo NodeInfo { get; private set; }
        /// <summary>
        /// 节点Id
        /// </summary>
        public string NodeId { get => NodeInfo.NodeId; }
        public TreeNodeStructureModel(NodeInfo nodeInfo)
        {
            this.NodeInfo = nodeInfo;
        }

        /// <summary>
        /// 子节点
        /// </summary>
        public List<TreeNodeStructureModel> Childs = new List<TreeNodeStructureModel>();
    }


}
