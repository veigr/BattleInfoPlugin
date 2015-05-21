using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using System.Windows.Data;
using BattleInfoPlugin.Models;

namespace BattleInfoPlugin.Views.Converters
{
    public class AttackTypeToDisplayTextConverter : IValueConverter
    {
        public object Convert(object value, Type targetType, object parameter, CultureInfo culture)
        {
            if (!(value is AttackType)) return "";
            var type = (AttackType)value;
            return type == AttackType.カットイン主主 ? "カットイン (x1.5)"
                : type == AttackType.カットイン主徹 ? "カットイン (x1.3)"
                : type == AttackType.カットイン主電 ? "カットイン (x1.2)"
                : type == AttackType.カットイン主副 ? "カットイン (x1.1)"
                : type == AttackType.カットイン雷 ? "カットイン (x1.5 x2)"
                : type == AttackType.カットイン主主主 ? "カットイン (x2.0)"
                : type == AttackType.カットイン主主副 ? "カットイン (x1.75)"
                : type == AttackType.カットイン主雷 ? "カットイン (x1.3 x2)"
                : type == AttackType.連撃 ? "連撃 (x1.2 x2)"
                : "通常";
        }

        public object ConvertBack(object value, Type targetType, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }
    }
}
