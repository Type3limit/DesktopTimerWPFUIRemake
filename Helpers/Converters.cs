using System;
using System.Collections.Generic;
using System.ComponentModel.DataAnnotations;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Markup;
using System.Windows.Media;
using Wpf.Ui.Controls;

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

    public class FontNameConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value != null && value is FontFamily font)
            {
                return font.ToString();
            }
            else if (value is LanguageSpecificStringDictionary lssd)
            {
                return lssd?.Values?.FirstOrDefault();
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }

    /// <summary>
    /// boolean to visibility
    /// </summary>
    public class BoolToVisiableConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if(value is bool visiable)
            {
                return visiable ? System.Windows.Visibility.Visible : System.Windows.Visibility.Collapsed;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }
    /// <summary>
    /// boolean to reverse visibility
    /// </summary>
    public class BoolToReverseVisiableConverters : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value is bool visiable)
            {
                return visiable ? System.Windows.Visibility.Collapsed : System.Windows.Visibility.Visible;
            }
            return Binding.DoNothing;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
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


    public class HalfConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (value == null)
                return Binding.DoNothing;
            var curValue = value as double?;
            var percent = double.Parse(parameter as string);

            return curValue == null ? Binding.DoNothing : curValue.Value / percent;

        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return Binding.DoNothing;
        }
    }


    public class PercentFormater : INumberFormatter, INumberParser
    {
        string INumberFormatter.FormatDouble(double? value)
        {
            return ((value * 100.0).Value).ToString("F2")??"0" + " %";
        }

        string INumberFormatter.FormatInt(int? value)
        {
            return (value*100).ToString() + " %";
        }

        string INumberFormatter.FormatUInt(uint? value)
        {
            return (value * 100).ToString() + " %";
        }

        double? INumberParser.ParseDouble(string? value)
        {
            if(double.TryParse(value,out var curVar))
            {
                return curVar/100.0;
            }
            return 0;
        }

        int? INumberParser.ParseInt(string? value)
        {
            if (int.TryParse(value, out var curVar))
            {
                return curVar;
            }
            return 0;
        }

        uint? INumberParser.ParseUInt(string? value)
        {
            if (uint.TryParse(value, out var curVar))
            {
                return curVar;
            }
            return 0;
        }
    }


    public class EnumDisplayNameConverter : MarkupExtension, IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((Enum)value).GetAttributeOfType<DisplayAttribute>().Name;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotSupportedException();
        }

        public override object ProvideValue(IServiceProvider serviceProvider)
        {
            return this;
        }
    }
}
