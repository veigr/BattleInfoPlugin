using System;
using System.Collections.Generic;
using System.Linq;
using Grabacr07.KanColleWrapper.Models;
using Livet;

namespace BattleInfoPlugin.Models
{
    public class ShipData : NotificationObject
    {

        private readonly ShipInfo EnemyInfo;

        private readonly Ship ShipSource;

        public string Name
        {
            get { return this.ShipSource != null ? this.ShipSource.Info.Name : this.EnemyInfo.Name; }
        }

        public string TypeName
        {
            get { return this.ShipSource != null ? this.ShipSource.Info.ShipType.Name : this.EnemyInfo.ShipType.Name; }
        }

        public ShipSituation Situation
        {
            get { return this.ShipSource != null ? this.ShipSource.Situation : ShipSituation.None; }
        }

        #region MaxHP変更通知プロパティ

        private int _MaxHP;

        public int MaxHP
        {
            get { return this._MaxHP; }
            set
            {
                if (this._MaxHP == value)
                    return;
                this._MaxHP = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.HP);
            }
        }

        #endregion


        #region NowHP変更通知プロパティ

        private int _NowHP;

        public int NowHP
        {
            get { return this._NowHP; }
            set
            {
                if (this._NowHP == value)
                    return;
                this._NowHP = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.HP);
            }
        }

        #endregion


        #region HP変更通知プロパティ

        public LimitedValue HP
        {
            get { return new LimitedValue(this.NowHP, this.MaxHP, 0); }
        }

        #endregion

        public ShipData()
        {
        }

        public ShipData(ShipInfo info)
        {
            this.EnemyInfo = info;
        }

        public ShipData(Ship ship)
        {
            this.ShipSource = ship;
        }
    }

    public static class ShipDataExtensions
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
            this TSource[] source,
            IEnumerable<TValue> values,
            Action<TSource, TValue> setter)
        {
            source.Zip(values, (s, v) => new {s, v})
                .ToList()
                .ForEach(x => setter(x.s, x.v));
        }

        /// <summary>
        /// ダメージ適用
        /// </summary>
        /// <param name="ships">艦隊</param>
        /// <param name="damages">適用ダメージリスト</param>
        public static void CalcDamages(this ShipData[] ships, params FleetDamages[] damages)
        {
            foreach (var damage in damages)
            {
                ships.SetValues(damage.ToArray(), (s, d) => s.NowHP -= d);
            }
        }
    }
}
