using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Data;
using System.Windows.Documents;
using System.Windows.Input;
using System.Windows.Media;
using System.Windows.Media.Imaging;
using System.Windows.Media.TextFormatting;
using System.Windows.Navigation;
using System.Windows.Shapes;

namespace WpfApp1
{
    /// <summary>
    /// MainWindow.xaml 的交互逻辑
    /// </summary>
    public partial class MainWindow : Window
    {
        public MainWindow()
        {
            InitializeComponent();
        }

        private void Button_Click(object sender, RoutedEventArgs e)
        {
            
            rich.Focus();
            var richStart = rich.Document.ContentStart;
            TextRange textRange = new TextRange(richStart, rich.Document.ContentEnd);
           var  listAll = textRange.Text.ToCharArray().ToList();
            var al= richStart.GetPositionAtOffset(10);
            textRange.Select(richStart.GetPositionAtOffset(2), richStart.GetPositionAtOffset(3));
            //rich.SelectionBrush = new SolidColorBrush(Colors.Red);
            textRange.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            //List<TextRange> textRanges = FindWordFromPosition(rich.Document.ContentStart, "A");
            //foreach (var range in textRanges)
            //{
            //    range.ApplyPropertyValue(TextElement.ForegroundProperty, new SolidColorBrush(Colors.Red));
            //    //range.ApplyPropertyValue(TextElement.FontWeightProperty, FontWeights.Bold);
            //    //range.ApplyPropertyValue(TextElement.BackgroundProperty, new SolidColorBrush(Colors.PowderBlue));
            //}

        }
        List<TextRange> FindWordFromPosition(TextPointer position, string word)
        {
            List<TextRange> matchingText = new List<TextRange>();
            while (position != null)
            {
                if (position.GetPointerContext(LogicalDirection.Forward) == TextPointerContext.Text)
                {
                    //带有内容的文本
                    string textRun = position.GetTextInRun(LogicalDirection.Forward);

                    //查找关键字在这文本中的位置
                    int indexInRun = textRun.IndexOf(word);
                    int indexHistory = 0;
                    while (indexInRun >= 0)
                    {
                        TextPointer start = position.GetPositionAtOffset(indexInRun + indexHistory);
                        TextPointer end = start.GetPositionAtOffset(word.Length);
                        matchingText.Add(new TextRange(start, end));

                        indexHistory = indexHistory + indexInRun + word.Length;
                        textRun = textRun.Substring(indexInRun + word.Length);//去掉已经采集过的内容
                        indexInRun = textRun.IndexOf(word);//重新判断新的字符串是否还有关键字
                    }
                }

                position = position.GetNextContextPosition(LogicalDirection.Forward);
            }
            return matchingText;
        }

        private void btnQ_Click(object sender, RoutedEventArgs e)
        {
            rich.ClearValue(TextElement.ForegroundProperty);

        }
    }
}
