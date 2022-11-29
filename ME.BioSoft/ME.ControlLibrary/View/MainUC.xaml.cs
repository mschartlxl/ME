
using LogHelperLib;
using ME.BaseCore;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Forms;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Navigation;
using System.Windows.Shapes;
using MessageBox = ME.ControlLibrary.View.UMessageBox;
using UserControl = System.Windows.Controls.UserControl;
using Path = System.IO.Path;
using System.IO;
using System.Collections.ObjectModel;
using ME.ControlLibrary.Model;
using Newtonsoft.Json;
using ME.DB;
using CommunityToolkit.Mvvm.Messaging;
using System.ComponentModel;
using System.Runtime.CompilerServices;
using System.Threading;
using System.Collections.Concurrent;
using System.Collections;


namespace ME.ControlLibrary.View
{
    /// <summary>
    /// UserControl1.xaml 的交互逻辑
    /// </summary>
    public partial class MainUC : UserControl
    {
        public List<char> listAll = new List<char>();
        StepInfo stepInfo = new StepInfo();
        Window _Owner;

        double actcount = 0;

        public MainUC(Window owner)
        {
            InitializeComponent();
            Init();
            _Owner = owner;
            listview.ItemsSource = CommandMessage.Instance.Messages;
            _Owner.Closing += _Owner_Closing;
            //var dddd=  PlatformActionDAL.Instance.SearchMany(null);
            System.Windows.Forms.Label.CheckForIllegalCrossThreadCalls = false;
            WeakReferenceMessenger.Default.Register<MessageInfo>(this, (th, me) =>
            {
                if (!me.Flag)
                {
                    actcount++;
                    this.Dispatcher.Invoke(() =>
                    {
                        pogess.Value = actcount;

                    });
                }

            });
            WeakReferenceMessenger.Default.Register<ShowMask>(this, (th, me) =>
            {
                this.Dispatcher.Invoke(() =>
                {

                    ShowMaskTree(me.IsShow);

                });

            });

        }

        private void _Owner_Closing(object sender, System.ComponentModel.CancelEventArgs e)
        {
            if (MessageBox.Show("指令集是否已经保存?", "提示", true, _Owner))
            {
                e.Cancel = false;
            }
            else
            {
                e.Cancel = true;
            }
        }

        public void Init()
        {
            stepInfo.InitIS();
            InitComponent();
        }
        public void InitComponent()
        {
            cmdTCA.Init(stepInfo.CmdTCA, "命令单元ID：");
            cmdACN.Init(stepInfo.CmdACN, "命令单元ID：");
            cmdcapA.Init(stepInfo.CmdCapA, "命令单元ID：");
            //cmdcapB.Init(stepInfo.CmdCapB, "命令单元ID：");
            cmdO.Init(stepInfo.CmdO, "命令单元ID：");
            cmdT.Init(stepInfo.CmdT, "命令单元ID：");
            cmdA.Init(stepInfo.CmdA, "命令单元ID：");
            cmdC.Init(stepInfo.CmdC, "命令单元ID：");
            cmdG.Init(stepInfo.CmdG, "命令单元ID：");
            cmdACT.Init(stepInfo.CmdACT, "命令单元ID：");
        }
        private void btnSave_Click(object sender, RoutedEventArgs e)
        {
            if (MessageBox.Show("确定要保存指令集吗？", "删除提示", true, _Owner))
            {
                SaveCommandInfoFilePath();
                ShowMask(false);
            }
        }
        /// <summary>
        /// 显示蒙板
        /// </summary>
        /// <param name="isShow"></param>
        public void ShowMask(bool isShow)
        {
            if (isShow == true)
                Mask.Visibility = Visibility.Visible;
            else
                Mask.Visibility = Visibility.Collapsed;
        }
        /// <summary>
        /// 显示蒙板
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
        /// 获取数据，待优化
        /// </summary>
        private void GetData()
        {
            stepInfo.CmdTCA = cmdTCA.GetCommandUnit();

            stepInfo.CmdACN = cmdACN.GetCommandUnit();

            stepInfo.CmdCapA = cmdcapA.GetCommandUnit();

            //  stepInfo.CmdCapB = cmdcapB.GetCommandUnit();

            stepInfo.CmdO = cmdO.GetCommandUnit();

            stepInfo.CmdT = cmdT.GetCommandUnit();

            stepInfo.CmdA = cmdA.GetCommandUnit();

            stepInfo.CmdC = cmdC.GetCommandUnit();

            stepInfo.CmdG = cmdG.GetCommandUnit();

            stepInfo.CmdACT = cmdACT.GetCommandUnit();
        }
        private void SaveCommandInfoFilePath(string commandInfoFilePath = null)
        {
            try
            {
                GetData();
                if (stepInfo.SaveISFile(commandInfoFilePath))
                {
                    MessageBox.Show("保存成功！", "提示", 1);
                }
                else
                {
                    MessageBox.Show("保存失败！", "提示", 1);
                }
            }
            catch (Exception ex)
            {
                LogHelper.SystemError("保存指令集出错", ex);
            }
        }

        private void btnImportFile_Click(object sender, RoutedEventArgs e)
        {
            using (OpenFileDialog openFileDialog = new OpenFileDialog())
            {
                openFileDialog.Multiselect = true;
                openFileDialog.Filter = "Program File (*.xml)|*.xml|所有文件(*.*)|*.xml";
                if (openFileDialog.ShowDialog() == DialogResult.OK)
                {
                    stepInfo.InitIS(openFileDialog.FileName);
                    InitComponent();
                }
            }
        }

        private void btnExportFile_Click(object sender, RoutedEventArgs e)
        {
            try
            {
                ShowMask(true);
                ExportCommandView exportCommandView = new ExportCommandView();
                if (exportCommandView.ShowDialog() != true)
                {
                    return;
                }
                string fileName = exportCommandView.CommandInfoFileName;
                using (FolderBrowserDialog folderBrowserDialog = new FolderBrowserDialog())
                {
                    folderBrowserDialog.Description = "请选择导出路径";
                    if (folderBrowserDialog.ShowDialog() == DialogResult.OK)
                    {
                        var destinationPath = folderBrowserDialog.SelectedPath;//目的路径
                        string commandInfoFilePath = Path.Combine(destinationPath, fileName + ".xml");
                        if (File.Exists(commandInfoFilePath))
                        {
                            if (MessageBox.Show("当前文件下已存在同名文件！是否要覆盖？", "提示", true, _Owner) != true)
                            {
                                return;
                            }
                        }
                        SaveCommandInfoFilePath(commandInfoFilePath);
                    }
                }
            }
            catch (Exception ex)
            {
                LogHelper.SystemError("导出指令集出错", ex);
            }
            finally
            {
                ShowMask(false);
            }
        }

        private void MenuItem_Click(object sender, RoutedEventArgs e)
        {
            List<MessageCmd> list = new List<MessageCmd>();
            foreach (var item in listview.SelectedItems)
            {
                var currentMe = item as MessageCmd;
                if (currentMe != null)
                {
                    list.Add(currentMe);
                }
            }
            string jsonStr = JsonConvert.SerializeObject(list, Formatting.Indented);
            System.Windows.Clipboard.Clear();
            System.Windows.Clipboard.SetText(jsonStr);
        }

        private async void btnRunAll_Click(object sender, RoutedEventArgs e)
        {
            if (MaskTree.Visibility == Visibility.Visible)
            {
                MessageBox.Show("运行中.....,请稍后！", "提示", 1);
                return;
            }
            richAll.Focus();
            richAll.IsEnabled = false;
            pogess.Value = 0;
            actcount = 0;
            btnRunAll.IsEnabled = false;
            btnRunPause.IsEnabled = true;
            btnRunStop.IsEnabled = true;
            CommandMessage.Instance.tokenSource = new CancellationTokenSource();
            CommandMessage.Instance.Flag = true;
            var richStart = richAll.Document.ContentStart;
            TextRange textRange = new TextRange(richStart, richAll.Document.ContentEnd);
            textRange.ClearAllProperties();
            listAll = textRange.Text.Replace("\n", "").Replace(" ", "").Replace("\t", "").Replace("\r", "").ToCharArray().ToList();
            var itemindex = 2;
            pogess.Maximum = CalcCount(listAll);
            var tabcount = 0;
            foreach (var item in listAll)
            {
                textRange.Select(richStart.GetPositionAtOffset(itemindex), richStart.GetPositionAtOffset(itemindex + 1));
                itemindex++;
                textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
                textRange.ApplyPropertyValue(TextElement.FontWeightProperty,FontWeights.Bold);
                foreach (var t in flowTabControl.Items)
                {
                    var curItem = t as TabItem;
                    if (curItem.Header.ToString() == item.ToString())
                    {
                        tabcount++;
                        txtExecuInfo.Text = $"第{tabcount}个";
                        var acomm = curItem.Content as UCCommandUnit;
                        curItem.IsSelected = true;
                        foreach (var itemx in acomm.myTreeView.Items.SourceCollection)
                        {
                            var selectItem = itemx as TreeItem;
                            selectItem.IsSelected = true;
                            await acomm.ExecuteCommand(selectItem, false);
                            if (CommandMessage.Instance.tokenSource.IsCancellationRequested)
                            {
                                richAll.IsEnabled = true;
                                btnRunAll.IsEnabled = true;
                                btnRunPause.IsEnabled = false;
                                btnRunStop.IsEnabled = false;
                                btnRunPause.Content = "暂停";
                                return;
                            }
                        }
                        if (CommandMessage.Instance.tokenSource.IsCancellationRequested)
                        {
                            richAll.IsEnabled = true;
                            btnRunAll.IsEnabled = true;
                            btnRunPause.IsEnabled = false;
                            btnRunStop.IsEnabled = false;
                            btnRunPause.Content = "暂停";
                            return;
                        }
                    }

                }
                textRange.ClearAllProperties();
            }
            btnRunAll.IsEnabled = true;
            btnRunPause.IsEnabled = false;
            btnRunStop.IsEnabled = false;
            richAll.IsEnabled = true;
        }
        /// <summary>
        /// 计算总个数
        /// </summary>
        /// <param name="listAll"></param>
        /// <returns></returns>
        private double CalcCount(List<char> listAll) 
        {
            double allcount = 0;
            foreach (var item in listAll)
            {
                foreach (var t in flowTabControl.Items)
                {
                    var curItem = t as TabItem;
                    if (curItem.Header.ToString() == item.ToString())
                    {
                        var acomm = curItem.Content as UCCommandUnit;
                        // curItem.IsSelected = true;
                        foreach (var itemx in acomm.myTreeView.Items.SourceCollection)
                        {

                            var selectItem = itemx as TreeItem;
                            NodeInfo selectNodeInfo = selectItem.Tag;
                            if (selectNodeInfo != null)
                            {
                                switch (selectNodeInfo.csNodeMode)
                                {
                                    case NodeModel.LoopMode:
                                        {
                                            LoopMode loopMode = selectNodeInfo.objParent as LoopMode;
                                            int CycleNumber = loopMode.CycleNumber;
                                            allcount++;
                                            do
                                            {

                                                allcount += selectItem.Children.Count;
                                                CycleNumber--;
                                            } while (CycleNumber >= 1);
                                            break;

                                        }
                                    default:
                                        allcount++;
                                        break;
                                }

                            }
                        }
                    }
                }

            }
            return allcount;
        }
        private void btnRunPause_Click(object sender, RoutedEventArgs e)
        {
            if (btnRunPause.Content.ToString() == "暂停")
            {
                CommandMessage.Instance.Flag = false;
                btnRunPause.Content = "继续";
            }
            else
            {
                CommandMessage.Instance.Flag = true;
                btnRunPause.Content = "暂停";
            }
        }

        private void btnRunConitue_Click(object sender, RoutedEventArgs e)
        {
            CommandMessage.Instance.Flag = true;
        }

        private void btnRunStop_Click(object sender, RoutedEventArgs e)
        {
            CommandMessage.Instance.tokenSource.Cancel();
            btnRunStop.IsEnabled = false;

        }
        private void btnInit_Click(object sender, RoutedEventArgs e)
        {
            GlobalVariables.Instance.InitReset();
        }

        private void menuBtnClear_Click(object sender, RoutedEventArgs e)
        {
            CommandMessage.Instance.Messages.Clear();
        }

        private void btnSet_Click(object sender, RoutedEventArgs e)
        {
            ShowMask(true);
            SetWindow setWindow = new SetWindow
            {
                Owner = _Owner,
                Title = "添加指令"

            };
            setWindow.ShowDialog();
           
            ShowMask(false);
        }
    }
    public class MessageInfo
    {
        public bool Flag { get; set; }
        public bool IsShow { get; set; }
    }
    public class ShowMask
    {
        public bool IsShow { get; set; }
    }

}


