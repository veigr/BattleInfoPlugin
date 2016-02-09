using System;
using System.Reactive.Linq;
using BattleInfoPlugin.Models.Raw;
using BattleInfoPlugin.Models.Repositories;
using Grabacr07.KanColleWrapper;

namespace BattleInfoPlugin
{
    class SortieDataListener
    {
        private readonly EnemyDataProvider provider = new EnemyDataProvider();

        public SortieDataListener()
        {
            var proxy = KanColleClient.Current.Proxy;

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_battle_midnight/sp_midnight")
                .TryParse<battle_midnight_sp_midnight>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_airbattle
                .TryParse<combined_battle_airbattle>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_battle
                .TryParse<combined_battle_battle>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/battle_water")
                .TryParse<combined_battle_battle_water>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_combined_battle/sp_midnight")
                .TryParse<combined_battle_sp_midnight>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_sortie/airbattle")
                .TryParse<sortie_airbattle>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_sortie_battle
                .TryParse<sortie_battle>().Subscribe(x => this.Update(x.Data));


            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_map/start")
                .TryParse<map_start_next>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_map/next")
                .TryParse<map_start_next>().Subscribe(x => this.Update(x.Data));


            proxy.api_req_sortie_battleresult
                .TryParse<battle_result>().Subscribe(x => this.Update(x.Data));

            proxy.api_req_combined_battle_battleresult
                .TryParse<battle_result>().Subscribe(x => this.Update(x.Data));

            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_get_member/mapinfo")
                .TryParse<member_mapinfo[]>().Subscribe(x => this.Update(x.Data));
        }

        #region Battle

        public void Update(battle_midnight_sp_midnight data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        public void Update(combined_battle_airbattle data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        public void Update(combined_battle_battle data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        public void Update(combined_battle_battle_water data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        public void Update(combined_battle_sp_midnight data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        private void Update(sortie_airbattle data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        private void Update(sortie_battle data)
        {
            this.provider.UpdateEnemyData(
                data.api_ship_ke,
                data.api_formation,
                data.api_eSlot,
                data.api_eKyouka,
                data.api_eParam,
                data.api_ship_lv,
                data.api_maxhps);
            this.provider.UpdateBattleTypes(data);
        }

        #endregion

        #region StartNext

        private void Update(map_start_next startNext)
            => this.provider.UpdateMapData(startNext);

        private void Update(battle_result result)
            => this.provider.UpdateEnemyName(result);

        private void Update(member_mapinfo[] mapinfos)
            => this.provider.UpdateMapInfo(mapinfos);

        #endregion
    }
}
