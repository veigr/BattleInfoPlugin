using System;
using System.Linq;
using System.Reactive.Linq;
using BattleInfoPlugin.Models.Raw;
using Grabacr07.KanColleWrapper;
using Livet;

namespace BattleInfoPlugin.Models
{
    public class BattleData : NotificationObject
    {
        //FIXME 敵の開幕雷撃&連合艦隊がまだ不明(とりあえず第二艦隊が受けるようにしてる)
        //FIXME 連合艦隊全般未テスト


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


        #region UpdatedTime変更通知プロパティ
        private DateTimeOffset _UpdatedTime;

        public DateTimeOffset UpdatedTime
        {
            get
            { return this._UpdatedTime; }
            set
            { 
                if (this._UpdatedTime == value)
                    return;
                this._UpdatedTime = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region FirstFleet変更通知プロパティ
        private ShipData[] _FirstFleet;

        public ShipData[] FirstFleet
        {
            get
            { return this._FirstFleet; }
            set
            { 
                if (this._FirstFleet == value)
                    return;
                this._FirstFleet = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region SecondFleet変更通知プロパティ
        private ShipData[] _SecondFleet;

        public ShipData[] SecondFleet
        {
            get
            { return this._SecondFleet; }
            set
            { 
                if (this._SecondFleet == value)
                    return;
                this._SecondFleet = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region Enemies変更通知プロパティ
        private ShipData[] _Enemies;

        public ShipData[] Enemies
        {
            get
            { return this._Enemies; }
            set
            { 
                if (this._Enemies == value)
                    return;
                this._Enemies = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        public BattleData()
        {
            var proxy = KanColleClient.Current.Proxy;

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_battle_midnight/battle")
                .TryParse<battle_midnight_battle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_battle_midnight/sp_midnight")
                .TryParse<battle_midnight_sp_midnight>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_airbattle
                .TryParse<combined_battle_airbattle>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_battle
                .TryParse<combined_battle_battle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_combined_battle/battle_water")
                .TryParse<combined_battle_battle_water>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_combined_battle/midnight_battle")
                .TryParse<combined_battle_midnight_battle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_combined_battle/sp_midnight")
                .TryParse<combined_battle_sp_midnight>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_practice/battle")
                .TryParse<practice_battle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.PathAndQuery == "/kcsapi/api_req_practice/midnight_battle")
                .TryParse<practice_midnight_battle>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_sortie_battle
                .TryParse<sortie_battle>().Subscribe(x => this.Update(x.Data));
        }

        public void Update(battle_midnight_battle data)
        {
            this.Name = "通常 - 夜戦";

            this.UpdateShipData(data.api_deck_id, data.api_ship_ke, false);
            this.UpdateMaxHP(data.api_maxhps);
            this.UpdateNowHP(data.api_nowhps);

            this.FirstFleet.CalcDamages(data.api_hougeki.GetFriendDamages());

            this.Enemies.CalcDamages(data.api_hougeki.GetEnemyDamages());
        }

        public void Update(battle_midnight_sp_midnight data)
        {
            this.Name = "通常 - 開幕夜戦";

            this.UpdateShipData(data.api_deck_id.ToString(), data.api_ship_ke, false);
            this.UpdateMaxHP(data.api_maxhps);
            this.UpdateNowHP(data.api_nowhps);

            this.FirstFleet.CalcDamages(data.api_hougeki.GetFriendDamages());

            this.Enemies.CalcDamages(data.api_hougeki.GetEnemyDamages());
        }

        public void Update(combined_battle_airbattle data)
        {
            this.Name = "連合艦隊 - 航空戦 - 昼戦";

            this.UpdateShipData(data.api_deck_id, data.api_ship_ke, true);
            this.UpdateMaxHP(data.api_maxhps, data.api_maxhps_combined);
            this.UpdateNowHP(data.api_nowhps, data.api_nowhps_combined);

            this.FirstFleet.CalcDamages(
                data.api_kouku.GetFirstFleetDamages(),
                data.api_kouku2.GetFirstFleetDamages()
                );

            this.SecondFleet.CalcDamages(
                data.api_kouku.GetSecondFleetDamages(),
                data.api_kouku2.GetSecondFleetDamages()
                );

            this.Enemies.CalcDamages(
                data.api_support_info.GetEnemyDamages(),
                data.api_kouku.GetEnemyDamages(),
                data.api_kouku2.GetEnemyDamages()
                );
        }

        public void Update(combined_battle_battle data)
        {
            this.Name = "連合艦隊 - 機動部隊 - 昼戦";

            this.UpdateShipData(data.api_deck_id, data.api_ship_ke, true);
            this.UpdateMaxHP(data.api_maxhps, data.api_maxhps_combined);
            this.UpdateNowHP(data.api_nowhps, data.api_nowhps_combined);

            this.FirstFleet.CalcDamages(
                data.api_kouku.GetFirstFleetDamages(),
                data.api_hougeki2.GetFriendDamages(),
                data.api_hougeki3.GetFriendDamages()
                );

            this.SecondFleet.CalcDamages(
                data.api_kouku.GetSecondFleetDamages(),
                data.api_opening_atack.GetFriendDamages(),
                data.api_hougeki1.GetFriendDamages(),
                data.api_raigeki.GetFriendDamages()
                );

            this.Enemies.CalcDamages(
                data.api_support_info.GetEnemyDamages(),
                data.api_kouku.GetEnemyDamages(),
                data.api_opening_atack.GetEnemyDamages(),
                data.api_hougeki1.GetEnemyDamages(),
                data.api_raigeki.GetEnemyDamages(),
                data.api_hougeki2.GetEnemyDamages(),
                data.api_hougeki3.GetEnemyDamages()
                );
        }

        public void Update(combined_battle_battle_water data)
        {
            this.Name = "連合艦隊 - 水上部隊 - 昼戦";

            this.UpdateShipData(data.api_deck_id, data.api_ship_ke, true);
            this.UpdateMaxHP(data.api_maxhps, data.api_maxhps_combined);
            this.UpdateNowHP(data.api_nowhps, data.api_nowhps_combined);

            this.FirstFleet.CalcDamages(
                data.api_kouku.GetFirstFleetDamages(),
                data.api_hougeki1.GetFriendDamages(),
                data.api_hougeki2.GetFriendDamages()
                );

            this.SecondFleet.CalcDamages(
                data.api_kouku.GetSecondFleetDamages(),
                data.api_opening_atack.GetFriendDamages(),
                data.api_hougeki3.GetFriendDamages(),
                data.api_raigeki.GetFriendDamages()
                );

            this.Enemies.CalcDamages(
                data.api_support_info.GetEnemyDamages(),
                data.api_kouku.GetEnemyDamages(),
                data.api_opening_atack.GetEnemyDamages(),
                data.api_hougeki1.GetEnemyDamages(),
                data.api_hougeki2.GetEnemyDamages(),
                data.api_hougeki3.GetEnemyDamages(),
                data.api_raigeki.GetEnemyDamages()
                );
        }

        public void Update(combined_battle_midnight_battle data)
        {
            this.Name = "連合艦隊 - 夜戦";

            this.UpdateShipData(data.api_deck_id, data.api_ship_ke, true);
            this.UpdateMaxHP(data.api_maxhps, data.api_maxhps_combined);
            this.UpdateNowHP(data.api_nowhps, data.api_nowhps_combined);

            this.SecondFleet.CalcDamages(data.api_hougeki.GetFriendDamages());

            this.Enemies.CalcDamages(data.api_hougeki.GetEnemyDamages());
        }

        public void Update(combined_battle_sp_midnight data)
        {
            this.Name = "連合艦隊 - 開幕夜戦";

            this.UpdateShipData(data.api_deck_id, data.api_ship_ke, true);
            this.UpdateMaxHP(data.api_maxhps, data.api_maxhps_combined);
            this.UpdateNowHP(data.api_nowhps, data.api_nowhps_combined);

            this.SecondFleet.CalcDamages(data.api_hougeki.GetFriendDamages());

            this.Enemies.CalcDamages(data.api_hougeki.GetEnemyDamages());
        }

        public void Update(practice_battle data)
        {
            this.Name = "演習 - 昼戦";

            this.UpdateShipData(data.api_dock_id.ToString(), data.api_ship_ke, false);
            this.UpdateMaxHP(data.api_maxhps);
            this.UpdateNowHP(data.api_nowhps);

            this.FirstFleet.CalcDamages(
                data.api_kouku.GetFirstFleetDamages(),
                data.api_opening_atack.GetFriendDamages(),
                data.api_hougeki1.GetFriendDamages(),
                data.api_hougeki2.GetFriendDamages(),
                data.api_raigeki.GetFriendDamages()
                );

            this.Enemies.CalcDamages(
                data.api_kouku.GetEnemyDamages(),
                data.api_opening_atack.GetEnemyDamages(),
                data.api_hougeki1.GetEnemyDamages(),
                data.api_hougeki2.GetEnemyDamages(),
                data.api_raigeki.GetEnemyDamages()
                );
        }

        public void Update(practice_midnight_battle data)
        {
            this.Name = "演習 - 夜戦";

            this.UpdateShipData(data.api_deck_id.ToString(), data.api_ship_ke, false);
            this.UpdateMaxHP(data.api_maxhps);
            this.UpdateNowHP(data.api_nowhps);

            this.FirstFleet.CalcDamages(data.api_hougeki.GetFriendDamages());

            this.Enemies.CalcDamages(data.api_hougeki.GetEnemyDamages());
        }

        private void Update(sortie_battle data)
        {
            this.Name = "通常 - 昼戦";

            this.UpdateShipData(data.api_dock_id.ToString(), data.api_ship_ke, false);
            this.UpdateMaxHP(data.api_maxhps);
            this.UpdateNowHP(data.api_nowhps);

            this.FirstFleet.CalcDamages(
                data.api_kouku.GetFirstFleetDamages(),
                data.api_opening_atack.GetFriendDamages(),
                data.api_hougeki1.GetFriendDamages(),
                data.api_hougeki2.GetFriendDamages(),
                data.api_raigeki.GetFriendDamages()
                );

            this.Enemies.CalcDamages(
                data.api_support_info.GetEnemyDamages(),
                data.api_kouku.GetEnemyDamages(),
                data.api_opening_atack.GetEnemyDamages(),
                data.api_hougeki1.GetEnemyDamages(),
                data.api_hougeki2.GetEnemyDamages(),
                data.api_raigeki.GetEnemyDamages()
                );
        }

        private void UpdateShipData(string api_deck_id, int[] api_ship_ke, bool isCombined)
        {
            this.UpdatedTime = DateTimeOffset.Now;

            var fleets = KanColleClient.Current.Homeport.Organization.Fleets;
            var master = KanColleClient.Current.Master.Ships;

            this.FirstFleet = fleets[int.Parse(api_deck_id)].Ships.Select(s => new ShipData(s)).ToArray();
            this.SecondFleet = isCombined ? fleets[2].Ships.Select(s => new ShipData(s)).ToArray() : new ShipData[0];
            this.Enemies = api_ship_ke.Where(x => x != -1).Select(x => new ShipData(master[x])).ToArray();
        }

        private void UpdateMaxHP(int[] api_maxhps, int[] api_maxhps_combined = null)
        {
            this.FirstFleet.SetValues(api_maxhps.GetFriendData(), (s, v) => s.MaxHP = v);
            this.Enemies.SetValues(api_maxhps.GetEnemyData(), (s, v) => s.MaxHP = v);

            if (api_maxhps_combined == null) return;
            this.SecondFleet.SetValues(api_maxhps_combined.GetFriendData(), (s, v) => s.MaxHP = v);
        }

        private void UpdateNowHP(int[] api_nowhps, int[] api_nowhps_combined = null)
        {
            this.FirstFleet.SetValues(api_nowhps.GetFriendData(), (s, v) => s.NowHP = v);
            this.Enemies.SetValues(api_nowhps.GetEnemyData(), (s, v) => s.NowHP = v);

            if (api_nowhps_combined == null) return;
            this.SecondFleet.SetValues(api_nowhps_combined.GetFriendData(), (s, v) => s.NowHP = v);
        }
    }
}
