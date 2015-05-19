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
using Grabacr07.KanColleWrapper.Models;

namespace BattleInfoPlugin.Models.Repositories
{
    [DataContract]
    public class EnemyDataProvider
    {
        private static readonly DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EnemyDataProvider));

        // EnemyId, EnemyMasterIDs
        [DataMember]
        private Dictionary<int, int[]> EnemyDictionary { get; set; }

        // EnemyId, Formation
        [DataMember]
        private Dictionary<int, Formation> EnemyFormation { get; set; }


        //以下はとりあえず保存だけする

        // EnemyId, api_eSlot
        [DataMember]
        private Dictionary<int, int[][]> EnemySlotItems { get; set; }

        // MapInfoID, CellNo, EnemyId
        [DataMember]
        private Dictionary<int, Dictionary<int, HashSet<int>>> MapEnemyData { get; set; }

        // MapInfoID, FromCellNo, ToCellNo
        [DataMember]
        private Dictionary<int, HashSet<KeyValuePair<int, int>>> MapRoute { get; set; }

        // EnemyId, Name
        [DataMember]
        private Dictionary<int, string> EnemyNames { get; set; }

        [NonSerialized] private int currentEnemyID;

        [NonSerialized] private int previousCellNo;

        public EnemyDataProvider()
        {
            this.Reload();
            if (this.EnemyDictionary == null) this.EnemyDictionary = new Dictionary<int, int[]>();
            if (this.EnemyFormation == null) this.EnemyFormation = new Dictionary<int, Formation>();
            if (this.EnemySlotItems == null) this.EnemySlotItems = new Dictionary<int, int[][]>();
            if (this.EnemyNames == null) this.EnemyNames = new Dictionary<int, string>();
            if (this.MapEnemyData == null) this.MapEnemyData = new Dictionary<int, Dictionary<int, HashSet<int>>>();
            if (this.MapRoute == null) this.MapRoute = new Dictionary<int, HashSet<KeyValuePair<int, int>>>();
            this.previousCellNo = 0;
            this.Dump("GetNextEnemyFormation");
        }
        
        public FleetData GetNextEnemyFleet(map_start_next startNext)
        {
            this.Dump("GetNextEnemyFleet");
            if (startNext.api_enemy == null) return new FleetData();
            this.currentEnemyID = startNext.api_enemy.api_enemy_id;
            return new FleetData(
                this.GetNextEnemies(startNext),
                this.GetNextEnemyFormation(startNext),
                this.GetNextEnemyName(startNext));
        }

        public void UpdateMapData(map_start_next startNext)
        {
            this.UpdateMapEnemyData(startNext);
            this.UpdateMapRoute(startNext);
            this.Save();
            this.Dump("UpdateMapData");
        }

        public void UpdateEnemyName(battle_result result)
        {
            if (result == null) return;
            if (result.api_enemy_info == null) return;

            if (this.EnemyNames.ContainsKey(this.currentEnemyID))
                this.EnemyNames[this.currentEnemyID] = result.api_enemy_info.api_deck_name;
            else
                this.EnemyNames.Add(this.currentEnemyID, result.api_enemy_info.api_deck_name);
            this.Save();
        }

        public Dictionary<MapInfo, Dictionary<int, Dictionary<int, FleetData>>> GetMapEnemies()
        {
            this.Reload();
            return this.MapEnemyData.ToDictionary(
                info => Master.Current.MapInfos[info.Key],
                info => info.Value.ToDictionary(
                    cell => cell.Key,
                    cell => cell.Value.ToDictionary(
                        enemy => enemy,
                        enemy => new FleetData(
                            this.GetEnemiesFromId(enemy),
                            this.GetEnemyFormationFromId(enemy),
                            this.GetEnemyNameFromId(enemy),
                            IsBoss(info.Key, cell.Key)
                            ))));
        }

        private static bool IsBoss(int mapInfoId, int cellNo)
        {
            var cell = Master.Current.MapCells.Values
                .Where(x => x.MapInfoId == mapInfoId)
                .SingleOrDefault(x => x.IdInEachMapInfo == cellNo);
            if (cell == null) return false;
            return cell.ColorNo == 5;
        }

        private string GetNextEnemyName(map_start_next startNext)
        {
            return startNext.api_enemy != null
                ? this.GetEnemyNameFromId(startNext.api_enemy.api_enemy_id)
                : "";
        }

        private string GetEnemyNameFromId(int enemyId)
        {
            return this.EnemyNames.ContainsKey(enemyId)
                ? this.EnemyNames[enemyId]
                : "";
        }

        private Formation GetNextEnemyFormation(map_start_next startNext)
        {
            return startNext.api_enemy != null
                ? this.GetEnemyFormationFromId(startNext.api_enemy.api_enemy_id)
                : Formation.なし;
        }

        private Formation GetEnemyFormationFromId(int enemyId)
        {
            return this.EnemyFormation.ContainsKey(enemyId)
                ? this.EnemyFormation[enemyId]
                : Formation.不明;
        }

        private IEnumerable<ShipData> GetNextEnemies(map_start_next startNext)
        {
            return startNext.api_enemy != null
                ? this.GetEnemiesFromId(startNext.api_enemy.api_enemy_id)
                : new MembersShipData[0];
        }

        private IEnumerable<ShipData> GetEnemiesFromId(int enemyId)
        {
            var shipInfos = KanColleClient.Current.Master.Ships;
            var slotInfos = KanColleClient.Current.Master.SlotItems;
            if (!this.EnemyDictionary.ContainsKey(enemyId)) return Enumerable.Repeat(new MastersShipData(), 6).ToArray();
            var enemies = this.EnemyDictionary[enemyId]
                .Select((x, i) => new MastersShipData(shipInfos[x])
                {
                    Slots = this.EnemySlotItems.ContainsKey(enemyId)
                        ? this.EnemySlotItems[enemyId][i]
                            .Where(s => s != -1)
                            .Select(s => slotInfos[s])
                            .Select((s, si) => new ShipSlotData(s, shipInfos[x].Slots[si], shipInfos[x].Slots[si]))
                        : new ShipSlotData[0],
                }).ToArray();
            return enemies;
        }

        private void UpdateMapEnemyData(map_start_next startNext)
        {
            if (startNext.api_enemy == null) return;

            var mapInfo = GetMapInfo(startNext);

            if (!this.MapEnemyData.ContainsKey(mapInfo))
                this.MapEnemyData.Add(mapInfo, new Dictionary<int, HashSet<int>>());
            if (!this.MapEnemyData[mapInfo].ContainsKey(startNext.api_no))
                this.MapEnemyData[mapInfo].Add(startNext.api_no, new HashSet<int>());

            this.MapEnemyData[mapInfo][startNext.api_no].Add(startNext.api_enemy.api_enemy_id);
        }

        private void UpdateMapRoute(map_start_next startNext)
        {
            var mapInfo = GetMapInfo(startNext);
            if (!this.MapRoute.ContainsKey(mapInfo))
                this.MapRoute.Add(mapInfo, new HashSet<KeyValuePair<int, int>>());

            this.MapRoute[mapInfo].Add(new KeyValuePair<int, int>(this.previousCellNo, startNext.api_no));

            this.previousCellNo = 0 < startNext.api_next ? startNext.api_no : 0;
        }

        private static int GetMapInfo(map_start_next startNext)
        {
            return Master.Current.MapInfos
                .Select(x => x.Value)
                .Where(m => m.MapAreaId == startNext.api_maparea_id)
                .Single(m => m.IdInEachMapArea == startNext.api_mapinfo_no)
                .Id;
        }

        public void UpdateEnemyData(int[] api_ship_ke, int[] api_formation, int[][] api_eSlot)
        {
            var enemies = api_ship_ke.Where(x => x != -1).ToArray();
            var formation = (Formation)api_formation[1];

            if (this.EnemyDictionary.ContainsKey(this.currentEnemyID))
                this.EnemyDictionary[this.currentEnemyID] = enemies;
            else
                this.EnemyDictionary.Add(this.currentEnemyID, enemies);

            if (this.EnemyFormation.ContainsKey(this.currentEnemyID))
                this.EnemyFormation[this.currentEnemyID] = formation;
            else
                this.EnemyFormation.Add(this.currentEnemyID, formation);

            if (this.EnemySlotItems.ContainsKey(this.currentEnemyID))
                this.EnemySlotItems[this.currentEnemyID] = api_eSlot;
            else
                this.EnemySlotItems.Add(this.currentEnemyID, api_eSlot);

            this.Save();
            this.Dump("UpdateEnemyData");
        }

        public void Dump(string title = "")
        {
            Debug.WriteLine(title);
            //this.EnemyDictionary.SelectMany(x => x.Value, (key, value) => new { key, value })
            //    .ToList().ForEach(x => Debug.WriteLine(x.key + " : " + x.value));
            //this.EnemyFormation
            //    .ToList().ForEach(x => Debug.WriteLine(x.Key + " : " + x.Value));
        }

        private void Reload()
        {
            //deserialize
            var path = Environment.CurrentDirectory + "\\" + Settings.Default.EnemyDataFilePath;
            if (!File.Exists(path)) return;

            using (var stream = Stream.Synchronized(new FileStream(path, FileMode.OpenOrCreate)))
            {
                var obj = serializer.ReadObject(stream) as EnemyDataProvider;
                if (obj == null) return;
                this.EnemyDictionary = obj.EnemyDictionary;
                this.EnemyFormation = obj.EnemyFormation;
                this.EnemySlotItems = obj.EnemySlotItems;
                this.EnemyNames = obj.EnemyNames;
                this.MapEnemyData = obj.MapEnemyData;
                this.MapRoute = obj.MapRoute;
            }
        }

        private void Save()
        {
            //serialize
            var path = Environment.CurrentDirectory + "\\" + Settings.Default.EnemyDataFilePath;
            using (var stream = Stream.Synchronized(new FileStream(path, FileMode.OpenOrCreate)))
            {
                serializer.WriteObject(stream, this);
            }
        }
    }
}
