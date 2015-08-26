using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using BattleInfoPlugin.Models.Raw;
using BattleInfoPlugin.Properties;
using Grabacr07.KanColleWrapper;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models.Repositories
{
    [DataContract]
    public class EnemyDataProvider
    {
        private static readonly DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EnemyDataProvider));

        // EnemyId, EnemyMasterIDs
        [DataMember]
        private Dictionary<string, int[]> EnemyDictionary { get; set; }

        // EnemyId, Formation
        [DataMember]
        private Dictionary<string, Formation> EnemyFormation { get; set; }

        // EnemyId, api_eSlot
        [DataMember]
        private Dictionary<string, int[][]> EnemySlotItems { get; set; }

        // EnemyId, api_eKyouka
        [DataMember]
        private Dictionary<string, int[][]> EnemyUpgraded { get; set; }

        // EnemyId, api_eParam
        [DataMember]
        private Dictionary<string, int[][]> EnemyParams { get; set; }

        // EnemyId, api_ship_lv
        [DataMember]
        private Dictionary<string, int[]> EnemyLevels { get; set; }

        // EnemyId, MaxHP
        [DataMember]
        private Dictionary<string, int[]> EnemyHPs { get; set; }

        // MapInfoID, CellNo, EnemyId
        [DataMember]
        private Dictionary<int, Dictionary<int, HashSet<string>>> MapEnemyData { get; set; }

        // MapInfoID, CellNo, BattleApiClassName
        [DataMember]
        private Dictionary<int, Dictionary<int, string>> MapCellBattleTypes { get; set; }

        // MapInfoID, FromCellNo, ToCellNo
        [DataMember]
        private Dictionary<int, HashSet<KeyValuePair<int, int>>> MapRoute { get; set; }

        // MapInfoID, MapCellData
        [DataMember]
        private Dictionary<int, List<MapCellData>> MapCellDatas { get; set; }

        // EnemyId, Name
        [DataMember]
        private Dictionary<string, string> EnemyNames { get; set; }

        [NonSerialized]
        private string currentEnemyID;

        [NonSerialized]
        private int previousCellNo;

        [NonSerialized]
        private map_start_next currentStartNext;

        public EnemyDataProvider()
        {
            this.Reload();
            if (this.EnemyDictionary == null) this.EnemyDictionary = new Dictionary<string, int[]>();
            if (this.EnemyFormation == null) this.EnemyFormation = new Dictionary<string, Formation>();
            if (this.EnemySlotItems == null) this.EnemySlotItems = new Dictionary<string, int[][]>();
            if (this.EnemyUpgraded == null) this.EnemyUpgraded = new Dictionary<string, int[][]>();
            if (this.EnemyParams == null) this.EnemyParams = new Dictionary<string, int[][]>();
            if (this.EnemyLevels == null) this.EnemyLevels = new Dictionary<string, int[]>();
            if (this.EnemyHPs == null) this.EnemyHPs = new Dictionary<string, int[]>();
            if (this.EnemyNames == null) this.EnemyNames = new Dictionary<string, string>();
            if (this.MapEnemyData == null) this.MapEnemyData = new Dictionary<int, Dictionary<int, HashSet<string>>>();
            if (this.MapCellBattleTypes == null) this.MapCellBattleTypes = new Dictionary<int, Dictionary<int, string>>();
            if (this.MapRoute == null) this.MapRoute = new Dictionary<int, HashSet<KeyValuePair<int, int>>>();
            if (this.MapCellDatas == null) this.MapCellDatas = new Dictionary<int, List<MapCellData>>();
            this.previousCellNo = 0;
            this.currentStartNext = null;
        }

        public void UpdateMapData(map_start_next startNext)
        {
            this.currentStartNext = startNext;

            this.UpdateMapRoute(startNext);
            this.UpdateMapCellData(startNext);

            this.Save();
        }

        public void UpdateBattleTypes<T>(T battleApi)
        {
            var battleTypeName = typeof(T).Name;
            var mapInfo = GetMapInfo(this.currentStartNext);

            if (!this.MapCellBattleTypes.ContainsKey(mapInfo))
                this.MapCellBattleTypes.Add(mapInfo, new Dictionary<int, string>());
            if (!this.MapCellBattleTypes[mapInfo].ContainsKey(this.currentStartNext.api_no))
                this.MapCellBattleTypes[mapInfo].Add(this.currentStartNext.api_no, battleTypeName);
            else
                this.MapCellBattleTypes[mapInfo][this.currentStartNext.api_no] = battleTypeName;
            
            this.Save();
        }

        public void UpdateEnemyName(battle_result result)
        {
            if (result?.api_enemy_info == null) return;

            if (this.EnemyNames.ContainsKey(this.currentEnemyID))
                this.EnemyNames[this.currentEnemyID] = result.api_enemy_info.api_deck_name;
            else
                this.EnemyNames.Add(this.currentEnemyID, result.api_enemy_info.api_deck_name);
            this.Save();
        }

        public Dictionary<MapInfo, Dictionary<MapCell, Dictionary<string, FleetData>>> GetMapEnemies()
        {
            this.Reload();
            return this.MapEnemyData
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
            this.Reload();
            return this.MapCellBattleTypes;
        }

        public Dictionary<int, List<MapCellData>> GetMapCellDatas()
        {
            this.Reload();
            return this.MapCellDatas;
        }

        private string GetEnemyNameFromId(string enemyId)
        {
            return this.EnemyNames.ContainsKey(enemyId)
                ? this.EnemyNames[enemyId]
                : "";
        }

        private Formation GetEnemyFormationFromId(string enemyId)
        {
            return this.EnemyFormation.ContainsKey(enemyId)
                ? this.EnemyFormation[enemyId]
                : Formation.不明;
        }

        private IEnumerable<ShipData> GetEnemiesFromId(string enemyId)
        {
            var shipInfos = KanColleClient.Current.Master.Ships;
            var slotInfos = KanColleClient.Current.Master.SlotItems;
            if (!this.EnemyDictionary.ContainsKey(enemyId)) return Enumerable.Repeat(new MastersShipData(), 6).ToArray();
            return this.EnemyDictionary[enemyId]
                .Select((x, i) =>
                {
                    var param = this.EnemyParams.ContainsKey(enemyId) ? this.EnemyParams[enemyId][i] : new[] { -1, -1, -1, -1 };
                    var upgrades = this.EnemyUpgraded.ContainsKey(enemyId) ? this.EnemyUpgraded[enemyId][i] : new[] { 0, 0, 0, 0 };
                    param = param.Zip(upgrades, (p, u) => p + u).ToArray();
                    var lv = this.EnemyLevels.ContainsKey(enemyId) ? this.EnemyLevels[enemyId][i + 1] : -1;
                    var hp = this.EnemyHPs.ContainsKey(enemyId) ? this.EnemyHPs[enemyId][i] : -1;
                    return new MastersShipData(shipInfos[x])
                    {
                        Level = lv,
                        NowHP = hp,
                        MaxHP = hp,
                        Firepower = param[0],
                        Torpedo = param[1],
                        AA = param[2],
                        Armer = param[3],
                        Slots = this.EnemySlotItems.ContainsKey(enemyId)
                            ? this.EnemySlotItems[enemyId][i]
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

            if (!this.MapEnemyData.ContainsKey(mapInfo))
                this.MapEnemyData.Add(mapInfo, new Dictionary<int, HashSet<string>>());
            if (!this.MapEnemyData[mapInfo].ContainsKey(startNext.api_no))
                this.MapEnemyData[mapInfo].Add(startNext.api_no, new HashSet<string>());

            this.MapEnemyData[mapInfo][startNext.api_no].Add(enemyId);
        }

        private void UpdateMapRoute(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.MapRoute.ContainsKey(mapInfo))
                this.MapRoute.Add(mapInfo, new HashSet<KeyValuePair<int, int>>());

            this.MapRoute[mapInfo].Add(new KeyValuePair<int, int>(this.previousCellNo, startNext.api_no));

            this.previousCellNo = 0 < startNext.api_next ? startNext.api_no : 0;
        }

        private void UpdateMapCellData(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.MapCellDatas.ContainsKey(mapInfo))
                this.MapCellDatas.Add(mapInfo, new List<MapCellData>());

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

            var exists = this.MapCellDatas[mapInfo].SingleOrDefault(x => x.No == mapCellData.No);
            if (exists != null) this.MapCellDatas[mapInfo].Remove(exists);
            this.MapCellDatas[mapInfo].Add(mapCellData);
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

            if (this.EnemyDictionary.ContainsKey(enemyId))
                this.EnemyDictionary[enemyId] = enemies;
            else
                this.EnemyDictionary.Add(enemyId, enemies);

            if (this.EnemyFormation.ContainsKey(enemyId))
                this.EnemyFormation[enemyId] = formation;
            else
                this.EnemyFormation.Add(enemyId, formation);

            if (this.EnemySlotItems.ContainsKey(enemyId))
                this.EnemySlotItems[enemyId] = api_eSlot;
            else
                this.EnemySlotItems.Add(enemyId, api_eSlot);

            if (this.EnemyUpgraded.ContainsKey(enemyId))
                this.EnemyUpgraded[enemyId] = api_eKyouka;
            else
                this.EnemyUpgraded.Add(enemyId, api_eKyouka);

            if (this.EnemyParams.ContainsKey(enemyId))
                this.EnemyParams[enemyId] = api_eParam;
            else
                this.EnemyParams.Add(enemyId, api_eParam);

            if (this.EnemyLevels.ContainsKey(enemyId))
                this.EnemyLevels[enemyId] = api_ship_lv;
            else
                this.EnemyLevels.Add(enemyId, api_ship_lv);

            var hps = api_maxhps.GetEnemyData().ToArray();
            if (this.EnemyHPs.ContainsKey(enemyId))
                this.EnemyHPs[enemyId] = hps;
            else
                this.EnemyHPs.Add(enemyId, hps);

            this.currentEnemyID = enemyId;

            this.Save();
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
            var keys = this.EnemyDictionary.Where(x => x.Value.EqualsValue(api_ship_ke)).Select(x => x.Key).ToArray();
            keys = this.EnemyFormation.Where(x => keys.Contains(x.Key)).Where(x => x.Value.Equals(api_formation)).Select(x => x.Key).ToArray();

            //以下は情報欠落がありそうなので、データがない場合はスルー。ある場合だけ絞り込む。
            var existItems = this.EnemySlotItems.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existItems.Any() ? existItems.Where(x => x.Value.EqualsValue(api_eSlot)).Select(x => x.Key).ToArray() : keys;

            var existsUpgrads = this.EnemyUpgraded.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsUpgrads.Any() ? existsUpgrads.Where(x => x.Value.EqualsValue(api_eKyouka)).Select(x => x.Key).ToArray() : keys;

            var existsParams = this.EnemyParams.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsParams.Any() ? existsParams.Where(x => x.Value.EqualsValue(api_eParam)).Select(x => x.Key).ToArray() : keys;

            var existsLevels = this.EnemyLevels.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsLevels.Any() ? existsLevels.Where(x => x.Value.EqualsValue(api_ship_lv)).Select(x => x.Key).ToArray() : keys;

            var existsHPs = this.EnemyHPs.Where(x => keys.Contains(x.Key)).ToArray();
            keys = existsHPs.Any() ? existsHPs.Where(x => x.Value.EqualsValue(api_maxhps)).Select(x => x.Key).ToArray() : keys;

            keys = keys.OrderBy(x => x).ToArray();
            return keys.Any() ? keys.First() : Guid.NewGuid().ToString();
        }

        public Task<bool> Merge(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path)) return false;

                lock (serializer)
                {
                    using (var stream = Stream.Synchronized(new FileStream(path, FileMode.Open)))
                    {
                        var obj = serializer.ReadObject(stream) as EnemyDataProvider;
                        if (obj == null) return false;
                        this.EnemyDictionary = this.EnemyDictionary.Merge(obj.EnemyDictionary);
                        this.EnemyFormation = this.EnemyFormation.Merge(obj.EnemyFormation);
                        this.EnemySlotItems = this.EnemySlotItems.Merge(obj.EnemySlotItems);
                        this.EnemyUpgraded = this.EnemyUpgraded.Merge(obj.EnemyUpgraded);
                        this.EnemyParams = this.EnemyParams.Merge(obj.EnemyParams);
                        this.EnemyLevels = this.EnemyLevels.Merge(obj.EnemyLevels);
                        this.EnemyHPs = this.EnemyHPs.Merge(obj.EnemyHPs);
                        this.EnemyNames = this.EnemyNames.Merge(obj.EnemyNames);
                        this.MapEnemyData = this.MapEnemyData.Merge(obj.MapEnemyData, (v1, v2) => v1.Merge(v2, (h1, h2) => h1.Merge(h2)));
                        this.MapCellBattleTypes = this.MapCellBattleTypes.Merge(obj.MapCellBattleTypes, (v1, v2) => v1.Merge(v2));
                        this.MapRoute = this.MapRoute.Merge(obj.MapRoute, (v1, v2) => v1.Merge(v2));
                        this.MapCellDatas = this.MapCellDatas.Merge(obj.MapCellDatas, (v1, v2) => v1.Merge(v2).ToList());
                    }
                }

                this.Save();
                return true;
            });
        }

        private void Reload()
        {
            Debug.WriteLine("Start Reload");
            //deserialize
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.EnemyDataFilePath);
            if (!File.Exists(path)) return;

            lock (serializer)
                using (var stream = Stream.Synchronized(new FileStream(path, FileMode.OpenOrCreate)))
                {
                    var obj = serializer.ReadObject(stream) as EnemyDataProvider;
                    if (obj == null) return;
                    this.EnemyDictionary = obj.EnemyDictionary ?? new Dictionary<string, int[]>();
                    this.EnemyFormation = obj.EnemyFormation ?? new Dictionary<string, Formation>();
                    this.EnemySlotItems = obj.EnemySlotItems ?? new Dictionary<string, int[][]>();
                    this.EnemyUpgraded = obj.EnemyUpgraded ?? new Dictionary<string, int[][]>();
                    this.EnemyParams = obj.EnemyParams ?? new Dictionary<string, int[][]>();
                    this.EnemyLevels = obj.EnemyLevels ?? new Dictionary<string, int[]>();
                    this.EnemyHPs = obj.EnemyHPs ?? new Dictionary<string, int[]>();
                    this.EnemyNames = obj.EnemyNames ?? new Dictionary<string, string>();
                    this.MapEnemyData = obj.MapEnemyData ?? new Dictionary<int, Dictionary<int, HashSet<string>>>();
                    this.MapCellBattleTypes = obj.MapCellBattleTypes ?? new Dictionary<int, Dictionary<int, string>>();
                    this.MapRoute = obj.MapRoute ?? new Dictionary<int, HashSet<KeyValuePair<int, int>>>();
                    this.MapCellDatas = obj.MapCellDatas ?? new Dictionary<int, List<MapCellData>>();
                }
            Debug.WriteLine("End  Reload");
        }

        private void Save()
        {
            Debug.WriteLine("Start Save");
            //serialize
            var path = Path.Combine(AppDomain.CurrentDomain.BaseDirectory, Settings.Default.EnemyDataFilePath);
            lock (serializer)
            using (var stream = Stream.Synchronized(new FileStream(path, FileMode.OpenOrCreate)))
            {
                serializer.WriteObject(stream, this);
            }
            Debug.WriteLine("End  Save");
        }
    }
}
