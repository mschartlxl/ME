using ME.BaseCore.Models.Enums;
using ME.ControlLibrary.Model;
using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ME.ControlLibrary.Converters
{
    /// <summary>
    /// 背景色转换器
    /// </summary>
    public class BackgoundMultiConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            //return ConstClass.ExcuteFailedBackgroundColor;
            try
            {
                //var isSelected = values[0]==null?false:(bool)values[0];
                //if (isSelected)
                //    return ConstClass.SelectedBackgroundColor;
                var excuteStatus = (OrderExcuteStatusEnum)values[1];
                switch (excuteStatus)
                {
                    case OrderExcuteStatusEnum.ExcuteFailed:
                        return ConstClass.ExcuteFailedBackgroundColor;
                    case OrderExcuteStatusEnum.ExcuteSuccess:
                        return ConstClass.ExcuteSuccessBackgroundColor;
                    case OrderExcuteStatusEnum.Excuting:
                        return ConstClass.ExcutingBackgroundColor;
                    case OrderExcuteStatusEnum.WaitForExcute:
                        return ConstClass.WaitForExcuteBackgroundColor;
                }
                return ConstClass.WaitForExcuteBackgroundColor;
            }
            catch
            {
                return ConstClass.WaitForExcuteBackgroundColor;
            }
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
