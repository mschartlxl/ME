using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows;
using System.Windows.Controls;
using System.Windows.Media;
using System.Windows.Media.Imaging;

namespace ME.Controls
{
    public class ABTAccountTextBox : TextBox
    {
        public ImageSource Image
        {
            get { return (ImageSource)GetValue(ImageProperty); }
            set { SetValue(ImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.Register("Image", typeof(ImageSource), typeof(ABTAccountTextBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/adminUncheck.png"))));

        public string Watermark
        {
            get { return (string)GetValue(WatermarkProperty); }
            set { SetValue(WatermarkProperty, value); }
        }

        // Using a DependencyProperty as the backing store for Watermark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.Register("Watermark", typeof(string), typeof(ABTAccountTextBox), new PropertyMetadata("请输入账号"));



        public ImageSource YesImage
        {
            get { return (ImageSource)GetValue(YesImageProperty); }
            set { SetValue(YesImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for YesImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YesImageProperty =
            DependencyProperty.Register("YesImage", typeof(ImageSource), typeof(ABTAccountTextBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/yes.png"))));



        public ImageSource NoImage
        {
            get { return (ImageSource)GetValue(NoImageProperty); }
            set { SetValue(NoImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for NoImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoImageProperty =
            DependencyProperty.Register("NoImage", typeof(ImageSource), typeof(ABTAccountTextBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/error.png"))));



        public ImageSource CheckedImage
        {
            get { return (ImageSource)GetValue(CheckedImageProperty); }
            set { SetValue(CheckedImageProperty, value); }
        }

        // Using a DependencyProperty as the backing store for CheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.Register("CheckedImage", typeof(ImageSource), typeof(ABTAccountTextBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/admincheck.png"))));

        public void ChangeStyle(FrameworkElement obj, bool yesno)
        {
            if (yesno)
            {
                System.Windows.Style CurrentStyle = (System.Windows.Style)obj.FindResource("YesStyle");
                this.Style = CurrentStyle;
            }
            else
            {
                System.Windows.Style CurrentStyle = (System.Windows.Style)obj.FindResource("NoStyle");
                this.Style = CurrentStyle;
            }
        }
    }
}
