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
    public  class ABTPasswordBox : DependencyObject
    {
        public static bool GetIsWatermark(DependencyObject obj)
        {
            return (bool)obj.GetValue(IsWatermarkProperty);
        }

        public static void SetIsWatermark(DependencyObject obj, bool value)
        {
            obj.SetValue(IsWatermarkProperty, value);
        }

        public static readonly DependencyProperty IsWatermarkProperty =
            DependencyProperty.RegisterAttached("IsWatermark", typeof(bool), typeof(ABTPasswordBox), new UIPropertyMetadata(false, OnWatermarkChanged));

        public static int GetPasswordLength(DependencyObject obj)
        {
            return (int)obj.GetValue(PasswordLengthProperty);
        }

        public static void SetPasswordLength(DependencyObject obj, int value)
        {
            obj.SetValue(PasswordLengthProperty, value);
        }

        public static readonly DependencyProperty PasswordLengthProperty =
            DependencyProperty.RegisterAttached("PasswordLength", typeof(int), typeof(ABTPasswordBox), new UIPropertyMetadata(0));

        private static void OnWatermarkChanged(DependencyObject d, DependencyPropertyChangedEventArgs e)
        {
            var pb = d as PasswordBox;
            if (pb == null)
            {
                return;
            }
            if ((bool)e.NewValue)
            {
                pb.PasswordChanged += PasswordChanged;
            }
            else
            {
                pb.PasswordChanged -= PasswordChanged;
            }
        }

        static void PasswordChanged(object sender, RoutedEventArgs e)
        {
            var pb = sender as PasswordBox;
            if (pb == null)
            {
                return;
            }
            SetPasswordLength(pb, pb.Password.Length);
        }

        public static ImageSource GetImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(ImageProperty);
        }

        public static void SetImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(ImageProperty, value);
        }

        // Using a DependencyProperty as the backing store for Image.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty ImageProperty =
            DependencyProperty.RegisterAttached("Image", typeof(ImageSource), typeof(ABTPasswordBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/passwordNocheck.png"))));


        public static string GetWatermark(DependencyObject obj)
        {
            return (string)obj.GetValue(WatermarkProperty);
        }

        public static void SetWatermark(DependencyObject obj, string value)
        {
            obj.SetValue(WatermarkProperty, value);
        }

        // Using a DependencyProperty as the backing store for Watermark.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty WatermarkProperty =
            DependencyProperty.RegisterAttached("Watermark", typeof(string), typeof(ABTPasswordBox), new UIPropertyMetadata("请输入密码"));



        public static ImageSource GetYesImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(YesImageProperty);
        }

        public static void SetYesImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(YesImageProperty, value);
        }

        // Using a DependencyProperty as the backing store for YesImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty YesImageProperty =
            DependencyProperty.RegisterAttached("YesImage", typeof(ImageSource), typeof(ABTPasswordBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/yes.png"))));


        public static ImageSource GetNoImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(NoImageProperty);
        }

        public static void SetNoImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(NoImageProperty, value);
        }

        // Using a DependencyProperty as the backing store for NoImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty NoImageProperty =
            DependencyProperty.RegisterAttached("NoImage", typeof(ImageSource), typeof(ABTPasswordBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/error.png"))));


        public static ImageSource GetCheckedImage(DependencyObject obj)
        {
            return (ImageSource)obj.GetValue(CheckedImageProperty);
        }

        public static void SetCheckedImage(DependencyObject obj, ImageSource value)
        {
            obj.SetValue(CheckedImageProperty, value);
        }

        // Using a DependencyProperty as the backing store for CheckedImage.  This enables animation, styling, binding, etc...
        public static readonly DependencyProperty CheckedImageProperty =
            DependencyProperty.RegisterAttached("CheckedImage", typeof(ImageSource), typeof(ABTPasswordBox), new PropertyMetadata(new BitmapImage(new Uri("pack://application:,,,/ME.Controls;component/Images/passwordcheck.png"))));

    }
}
