using System;
using System.Globalization;
using System.Windows.Data;

namespace BattleInfoPlugin.Views.Converters
{
    public class AddDoubleValueConverter : IValueConverter
    {
        public double Value { get; set; }

        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            return ((double)value) + this.Value;
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
