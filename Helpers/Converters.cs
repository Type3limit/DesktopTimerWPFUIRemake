using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;

namespace DesktopTimerWPFUIRemake.Helpers
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
}
