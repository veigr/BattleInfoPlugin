using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Threading.Tasks;
using System.Windows.Data;
using System.Windows.Media;
using BattleInfoPlugin.ViewModels.Enemies;

namespace BattleInfoPlugin.Views.Converters
{
    public class FleetBackgroundConverter : IMultiValueConverter
    {
        public Brush Background1 { get; set; }
        public Brush Background2 { get; set; }
        private Brush CurrentBackground { get; set; }

        public FleetBackgroundConverter()
        {
            this.Background1 = new SolidColorBrush(Colors.Transparent);
            this.Background2 = new SolidColorBrush(Colors.Transparent);
        }

        public object Convert(object[] values, Type targetType, object parameter, CultureInfo culture)
        {
            var defaultValue = new SolidColorBrush(Colors.Transparent);
            if (values.Length != 2) return defaultValue;

            var value1 = values[0] as EnemyFleetViewModel;
            var value2 = values[1] as EnemyFleetViewModel;
            if (value1 == null) return defaultValue;

            if (this.CurrentBackground == null)
                this.CurrentBackground = this.Background2;

            if (value1.ParentCell == value1.ParentCell.ParentMap.EnemyCells.First()
            && value1 == value1.ParentCell.EnemyFleets.First())
                this.CurrentBackground = this.Background2;

            if (value2 == null)
                this.SwapBackground();
            else if (value1.Key != value2.Key || value1.ParentCell.Key != value2.Key)
                this.SwapBackground();

            return this.CurrentBackground;
        }

        public object[] ConvertBack(object value, Type[] targetTypes, object parameter, CultureInfo culture)
        {
            throw new NotImplementedException();
        }

        private void SwapBackground()
        {
            this.CurrentBackground = this.CurrentBackground.Equals(this.Background1)
                ? this.Background2
                : this.Background1;
        }
    }
}
