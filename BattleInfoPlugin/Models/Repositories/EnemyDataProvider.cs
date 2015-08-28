using System;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography.X509Certificates;
using BattleInfoPlugin.Models.Raw;
using Grabacr07.KanColleWrapper;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models.Repositories
{
    public class EnemyDataProvider
    {
        private EnemyData EnemyData { get; } = EnemyData.Curret;
        
        private string currentEnemyID;
        
        private int previousCellNo;
        
        private map_start_next currentStartNext;

        public EnemyDataProvider()
        {
            this.previousCellNo = 0;
            this.currentStartNext = null;
        }

        public Task<bool> Merge(string path)
        {
            return this.EnemyData.Merge(path);
        }

        public void RemoveEnemy(string enemyId)
        {
            this.EnemyData.RemoveEnemy(enemyId);
            this.EnemyData.Save();
        }

        public void UpdateMapData(map_start_next startNext)
        {
            this.currentStartNext = startNext;

            this.UpdateMapRoute(startNext);
            this.UpdateMapCellData(startNext);

            this.EnemyData.Save();
        }

        public void UpdateBattleTypes<T>(T battleApi)
        {
            var battleTypeName = typeof(T).Name;
            var mapInfo = GetMapInfo(this.currentStartNext);

            if (!this.EnemyData.MapCellBattleTypes.ContainsKey(mapInfo))
                this.EnemyData.MapCellBattleTypes.Add(mapInfo, new Dictionary<int, string>());
            if (!this.EnemyData.MapCellBattleTypes[mapInfo].ContainsKey(this.currentStartNext.api_no))
                this.EnemyData.MapCellBattleTypes[mapInfo].Add(this.currentStartNext.api_no, battleTypeName);
            else
                this.EnemyData.MapCellBattleTypes[mapInfo][this.currentStartNext.api_no] = battleTypeName;
            
            this.EnemyData.Save();
        }

        public void UpdateEnemyName(battle_result result)
        {
            if (result?.api_enemy_info == null) return;

            if (this.EnemyData.EnemyNames.ContainsKey(this.currentEnemyID))
                this.EnemyData.EnemyNames[this.currentEnemyID] = result.api_enemy_info.api_deck_name;
            else
                this.EnemyData.EnemyNames.Add(this.currentEnemyID, result.api_enemy_info.api_deck_name);
            this.EnemyData.Save();
        }

        public Dictionary<MapInfo, Dictionary<MapCell, Dictionary<string, FleetData>>> GetMapEnemies()
        {
            this.EnemyData.Reload();
            return this.EnemyData.MapEnemyData
                .Where(x => Master.Current.MapInfos.ContainsKey(x.Key))
                .ToDictionary(
                info => Master.Current.MapInfos[info.Key],
                info => info.Value.ToDictionary(
                    cell => Master.Current.MapCells
                        .Select(c => c.Value)
                        .Single(c => c.MapInfoId == info.Key && c.IdInEachMapInfo == cell.Key),
                    cell => cell.Value.ToDictionary(
                        enemy => enemy,
                        enemy => new FleetData(
                            this.GetEnemiesFromId(enemy),
                            this.GetEnemyFormationFromId(enemy),
                            this.GetEnemyNameFromId(enemy),
                            FleetType.Enemy
                            ))));
        }

        public Dictionary<int, Dictionary<int, string>> GetMapCellBattleTypes()
        {
            this.EnemyData.Reload();
            return this.EnemyData.MapCellBattleTypes;
        }

        public Dictionary<int, List<MapCellData>> GetMapCellDatas()
        {
            this.EnemyData.Reload();
            return this.EnemyData.MapCellDatas;
        }

        private string GetEnemyNameFromId(string enemyId)
        {
            return this.EnemyData.EnemyNames.ContainsKey(enemyId)
                ? this.EnemyData.EnemyNames[enemyId]
                : "";
        }

        private Formation GetEnemyFormationFromId(string enemyId)
        {
            return this.EnemyData.EnemyFormation.ContainsKey(enemyId)
                ? this.EnemyData.EnemyFormation[enemyId]
                : Formation.不明;
        }

        private IEnumerable<ShipData> GetEnemiesFromId(string enemyId)
        {
            var shipInfos = KanColleClient.Current.Master.Ships;
            var slotInfos = KanColleClient.Current.Master.SlotItems;
            if (!this.EnemyData.EnemyDictionary.ContainsKey(enemyId)) return Enumerable.Repeat(new MastersShipData(), 6).ToArray();
            return this.EnemyData.EnemyDictionary[enemyId]
                .Select((x, i) =>
                {
                    var param = this.EnemyData.EnemyParams.ContainsKey(enemyId) ? this.EnemyData.EnemyParams[enemyId][i] : new[] { -1, -1, -1, -1 };
                    var upgrades = this.EnemyData.EnemyUpgraded.ContainsKey(enemyId) ? this.EnemyData.EnemyUpgraded[enemyId][i] : new[] { 0, 0, 0, 0 };
                    param = param.Zip(upgrades, (p, u) => p + u).ToArray();
                    var lv = this.EnemyData.EnemyLevels.ContainsKey(enemyId) ? this.EnemyData.EnemyLevels[enemyId][i + 1] : -1;
                    var hp = this.EnemyData.EnemyHPs.ContainsKey(enemyId) ? this.EnemyData.EnemyHPs[enemyId][i] : -1;
                    return new MastersShipData(shipInfos[x])
                    {
                        Level = lv,
                        NowHP = hp,
                        MaxHP = hp,
                        Firepower = param[0],
                        Torpedo = param[1],
                        AA = param[2],
                        Armer = param[3],
                        Slots = this.EnemyData.EnemySlotItems.ContainsKey(enemyId)
                            ? this.EnemyData.EnemySlotItems[enemyId][i]
                                .Where(s => s != -1)
                                .Select(s => slotInfos[s])
                                .Select((s, si) => new ShipSlotData(s))
                                .ToArray()
                            : new ShipSlotData[0],
                    };
                }).ToArray();
        }

        private void UpdateMapEnemyData(string enemyId)
        {
            var startNext = this.currentStartNext;
            var mapInfo = GetMapInfo(startNext);

            if (!this.EnemyData.MapEnemyData.ContainsKey(mapInfo))
                this.EnemyData.MapEnemyData.Add(mapInfo, new Dictionary<int, HashSet<string>>());
            if (!this.EnemyData.MapEnemyData[mapInfo].ContainsKey(startNext.api_no))
                this.EnemyData.MapEnemyData[mapInfo].Add(startNext.api_no, new HashSet<string>());

            this.EnemyData.MapEnemyData[mapInfo][startNext.api_no].Add(enemyId);
        }

        private void UpdateMapRoute(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.EnemyData.MapRoute.ContainsKey(mapInfo))
                this.EnemyData.MapRoute.Add(mapInfo, new HashSet<KeyValuePair<int, int>>());

            this.EnemyData.MapRoute[mapInfo].Add(new KeyValuePair<int, int>(this.previousCellNo, startNext.api_no));

            this.previousCellNo = 0 < startNext.api_next ? startNext.api_no : 0;
        }

        private void UpdateMapCellData(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.EnemyData.MapCellDatas.ContainsKey(mapInfo))
                this.EnemyData.MapCellDatas.Add(mapInfo, new List<MapCellData>());

            var mapCellData = new MapCellData
            {
                MapAreaId = startNext.api_maparea_id,
                MapInfoIdInEachMapArea = startNext.api_mapinfo_no,
                No = startNext.api_no,
                ColorNo = startNext.api_color_no,
                CommentKind = startNext.api_comment_kind,
                EventId = startNext.api_event_id,
                EventKind = startNext.api_event_kind,
                ProductionKind = startNext.api_production_kind,
                SelectCells = startNext.api_select_route != null ? startNext.api_select_route.api_select_cells : new int[0],
            };

            var exists = this.EnemyData.MapCellDatas[mapInfo].SingleOrDefault(x => x.No == mapCellData.No);
            if (exists != null) this.EnemyData.MapCellDatas[mapInfo].Remove(exists);
            this.EnemyData.MapCellDatas[mapInfo].Add(mapCellData);
        }

        private static int GetMapInfo(map_start_next startNext)
        {
            return Master.Current.MapInfos
                .Select(x => x.Value)
                .Where(m => m.MapAreaId == startNext.api_maparea_id)
                .Single(m => m.IdInEachMapArea == startNext.api_mapinfo_no)
                .Id;
        }

        public void UpdateEnemyData(
            int[] api_ship_ke,
            int[] api_formation,
            int[][] api_eSlot,
            int[][] api_eKyouka,
            int[][] api_eParam,
            int[] api_ship_lv,
            int[] api_maxhps)
        {
            var enemies = api_ship_ke.Where(x => x != -1).ToArray();
            var formation = (Formation)api_formation[1];

            var enemyId = this.GetEnemyId(enemies, formation, api_eSlot, api_eKyouka, api_eParam, api_ship_lv, api_maxhps);

            this.UpdateMapEnemyData(enemyId);

            if (this.EnemyData.EnemyDictionary.ContainsKey(enemyId))
                this.EnemyData.EnemyDictionary[enemyId] = enemies;
            else
                this.EnemyData.EnemyDictionary.Add(enemyId, enemies);

            if (this.EnemyData.EnemyFormation.ContainsKey(enemyId))
                this.EnemyData.EnemyFormation[enemyId] = formation;
            else
                this.EnemyData.EnemyFormation.Add(enemyId, formation);

            if (this.EnemyData.EnemySlotItems.ContainsKey(enemyId))
                this.EnemyData.EnemySlotItems[enemyId] = api_eSlot;
            else
                this.EnemyData.EnemySlotItems.Add(enemyId, api_eSlot);

            if (this.EnemyData.EnemyUpgraded.ContainsKey(enemyId))
                this.EnemyData.EnemyUpgraded[enemyId] = api_eKyouka;
            else
                this.EnemyData.EnemyUpgraded.Add(enemyId, api_eKyouka);

            if (this.EnemyData.EnemyParams.ContainsKey(enemyId))
                this.EnemyData.EnemyParams[enemyId] = api_eParam;
            else
                this.EnemyData.EnemyParams.Add(enemyId, api_eParam);

            if (this.EnemyData.EnemyLevels.ContainsKey(enemyId))
                this.EnemyData.EnemyLevels[enemyId] = api_ship_lv;
            else
                this.EnemyData.EnemyLevels.Add(enemyId, api_ship_lv);

            var hps = api_maxhps.GetEnemyData().ToArray();
            if (this.EnemyData.EnemyHPs.ContainsKey(enemyId))
                this.EnemyData.EnemyHPs[enemyId] = hps;
            else
                this.EnemyData.EnemyHPs.Add(enemyId, hps);

            this.currentEnemyID = enemyId;

            this.EnemyData.Save();
        }

        private string GetEnemyId(
            int[] api_ship_ke,
            Formation api_formation,
            int[][] api_eSlot,
            int[][] api_eKyouka,
            int[][] api_eParam,
            int[] api_ship_lv,
            int[] api_maxhps)
        {
            var keys = this.EnemyData.EnemyDictionary.Where(x => x.Value.EqualsValue(api_ship_ke)).Select(x => x.Key).ToArray();
            keys = this.EnemyData.EnemyFormation.Where(x => keys.Contains(x.Key)).Where(x => x.Value.Equals(api_formation)).Select(x => x.Key).ToArray();

            //以下は情報欠落がありそうなので、データがない場合はスルー。ある場合だけ絞り込む。
            var existItems = this.EnemyData.EnemySlotItems.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existItems.Any() ? existItems.Where(x => x.Value.EqualsValue(api_eSlot)).Select(x => x.Key).ToArray() : keys;

            var existsUpgrads = this.EnemyData.EnemyUpgraded.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsUpgrads.Any() ? existsUpgrads.Where(x => x.Value.EqualsValue(api_eKyouka)).Select(x => x.Key).ToArray() : keys;

            var existsParams = this.EnemyData.EnemyParams.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsParams.Any() ? existsParams.Where(x => x.Value.EqualsValue(api_eParam)).Select(x => x.Key).ToArray() : keys;

            var existsLevels = this.EnemyData.EnemyLevels.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsLevels.Any() ? existsLevels.Where(x => x.Value.EqualsValue(api_ship_lv)).Select(x => x.Key).ToArray() : keys;

            var existsHPs = this.EnemyData.EnemyHPs.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsHPs.Any() ? existsHPs.Where(x => x.Value.EqualsValue(api_maxhps)).Select(x => x.Key).ToArray() : keys;

            keys = keys.OrderBy(x => x).ToArray();
            return keys.Any() ? keys.First() : Guid.NewGuid().ToString();
        }
    }
}
