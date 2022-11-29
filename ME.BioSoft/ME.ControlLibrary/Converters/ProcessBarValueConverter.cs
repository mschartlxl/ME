using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;

namespace ME.ControlLibrary.Converters
{
    public class ProcessBarValueConverter : IMultiValueConverter
    {
        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if (values.Length != 0&&values.Length==2) 
            {
                double valueact = 0;
                double valueAll = 0;
                if (double.TryParse(values[0].ToString(),out valueact) && double.TryParse(values[1].ToString(), out valueAll)) 
                {
                   var rpercent= Math.Round(valueact/ valueAll, 4)*100;
                    return $"{rpercent}%";
                }
              
            }
            return null;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
