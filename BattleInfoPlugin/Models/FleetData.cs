using System;
using System.Collections.Generic;
using System.Collections.ObjectModel;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using Livet;

namespace BattleInfoPlugin.Models
{
    public class FleetData : NotificationObject
    {

        #region Name変更通知プロパティ
        private string _Name;

        public string Name
        {
            get
            { return this._Name; }
            set
            { 
                if (this._Name == value)
                    return;
                this._Name = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region Ships変更通知プロパティ
        private IEnumerable<ShipData> _Ships;

        public IEnumerable<ShipData> Ships
        {
            get
            { return this._Ships; }
            set
            { 
                if (this._Ships == value)
                    return;
                this._Ships = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region Formation変更通知プロパティ
        private Formation _Formation;

        public Formation Formation
        {
            get
            { return this._Formation; }
            set
            { 
                if (this._Formation == value)
                    return;
                this._Formation = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region IsBoss変更通知プロパティ
        private bool _IsBoss;

        public bool IsBoss
        {
            get
            { return this._IsBoss; }
            set
            {
                if (this._IsBoss == value)
                    return;
                this._IsBoss = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region AirSuperiorityPotential変更通知プロパティ
        private int _AirSuperiorityPotential;

        public int AirSuperiorityPotential
        {
            get
            { return this._AirSuperiorityPotential; }
            set
            { 
                if (this._AirSuperiorityPotential == value)
                    return;
                this._AirSuperiorityPotential = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        public int AirParityRequirements { get { return this.AirSuperiorityPotential * 2 / 3; } }
        public int AirSuperiorityRequirements { get { return this.AirSuperiorityPotential * 3 / 2; } }
        public int AirSupremacyRequirements { get { return this.AirSuperiorityPotential * 3; } }

        public FleetData() : this(new ShipData[0], Formation.なし, "")
        {
        }

        public FleetData(IEnumerable<ShipData> ships, Formation formation, string name, bool isBoss = false)
        {
            this._Ships = ships;
            this._Formation = formation;
            this._Name = name;
            this._IsBoss = isBoss;
            this._AirSuperiorityPotential = this._Ships
                .SelectMany(s => s.Slots)
                .Where(s => s.Source.IsAirSuperiorityFighter)
                .Sum(s => (int)(s.AA * Math.Sqrt(s.Current)))
                ;
        }
    }

    public static class FleetDataExtensions
    {
        /// <summary>
        /// Actionを使用して値を設定
        /// Zipするので要素数が少ない方に合わせられる
        /// </summary>
        /// <typeparam name="TSource"></typeparam>
        /// <typeparam name="TValue"></typeparam>
        /// <param name="source"></param>
        /// <param name="values"></param>
        /// <param name="setter"></param>
        public static void SetValues<TSource, TValue>(
            this IEnumerable<TSource> source,
            IEnumerable<TValue> values,
            Action<TSource, TValue> setter)
        {
            source.Zip(values, (s, v) => new { s, v })
                .ToList()
                .ForEach(x => setter(x.s, x.v));
        }

        /// <summary>
        /// ダメージ適用
        /// </summary>
        /// <param name="fleet">艦隊</param>
        /// <param name="damages">適用ダメージリスト</param>
        public static void CalcDamages(this FleetData fleet, params FleetDamages[] damages)
        {
            foreach (var damage in damages)
            {
                fleet.Ships.SetValues(damage.ToArray(), (s, d) => s.NowHP -= d);
            }
        }
    }
}
