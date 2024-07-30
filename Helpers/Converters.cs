using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DesktopTimer.Helpers
{
    /// <summary>
    /// Convert System.Windows.Media.Color To SolidColorBrush 
    /// </summary>
    public class SolidColorConverters : IValueConverter
    {
        object IValueConverter.Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is Color curColor)
            {
                return new SolidColorBrush(curColor);
            }
            return Binding.DoNothing;
        }

        object IValueConverter.ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    /// <summary>
    /// Convert target value as percent
    /// </summary>
    public class PercentValueConverters : IMultiValueConverter
    {
        object IMultiValueConverter.Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            if(values.Count()!=2)
            {
                return Binding.DoNothing;
            }
            var percent = (double)values[0];
            var targetValue = (double)values[1];

            return percent * targetValue;
        }

        object[] IMultiValueConverter.ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            return new object[]{Binding.DoNothing, Binding.DoNothing};
        }
    }

}
