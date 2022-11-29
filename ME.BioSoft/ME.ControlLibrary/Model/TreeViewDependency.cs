using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;

namespace ME.ControlLibrary.Model
{
    /// <summary>
    /// 依赖属性添加，判断当前TreeItem是否选中,如果选中，则滚动到当前位置
    /// </summary>
    public class TreeViewDependency : DependencyObject
    {

        public static bool GetScrolledToView(DependencyObject obj)
        {
            return (bool)obj.GetValue(ScrolledToViewProperty);
        }

        public static void SetScrolledToView(DependencyObject obj, bool value)
        {
            obj.SetValue(ScrolledToViewProperty, value);
        }

        // Using a DependencyProperty as the backing store for ScrolledToView.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ScrolledToViewProperty =
            DependencyProperty.RegisterAttached("ScrolledToView", typeof(bool), typeof(TreeViewDependency), new PropertyMetadata(false, OnIsScrolledToViewWhenSelectedChanged));

        public static void OnIsScrolledToViewWhenSelectedChanged(
          DependencyObject depObj, DependencyPropertyChangedEventArgs e)
        {
            if (depObj is TreeViewItem treeViewItem && e.NewValue is bool isIntoViewWhenSelected)
            {
                if (treeViewItem.IsSelected)
                {
                    treeViewItem.BringIntoView();
                }
            }
        }
    }
}
