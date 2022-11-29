using System.Windows;
using System.Windows.Controls;

namespace ME.Controls
{
    public class ABTFixedWidthColumn : GridViewColumn
    {
        static ABTFixedWidthColumn()
        {

            WidthProperty.OverrideMetadata(typeof(ABTFixedWidthColumn),

                new FrameworkPropertyMetadata(null, new CoerceValueCallback(OnCoerceWidth)));
        }
        public double FixedWidth
        {

            get { return (double)GetValue(FixedWidthProperty); }

            set { SetValue(FixedWidthProperty, value); }

        }

        public static readonly DependencyProperty FixedWidthProperty =

            DependencyProperty.Register(

                "FixedWidth",

          typeof(double),

                typeof(ABTFixedWidthColumn),

                new FrameworkPropertyMetadata(double.NaN, new PropertyChangedCallback(OnFixedWidthChanged)));

        private static void OnFixedWidthChanged(DependencyObject o, DependencyPropertyChangedEventArgs e)
        {

            ABTFixedWidthColumn fwc = o as ABTFixedWidthColumn;

            if (fwc != null)

                fwc.CoerceValue(WidthProperty);

        }
        private static object OnCoerceWidth(DependencyObject o, object baseValue)
        {

            ABTFixedWidthColumn fwc = o as ABTFixedWidthColumn;

            if (fwc != null)

                return fwc.FixedWidth;

            return baseValue;

        }
    }
}
