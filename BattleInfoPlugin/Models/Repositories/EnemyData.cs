using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.Serialization;
using System.Runtime.Serialization.Json;
using System.Threading.Tasks;
using BattleInfoPlugin.Properties;

namespace BattleInfoPlugin.Models.Repositories
{
    [DataContract]
    internal class EnemyData
    {
        private static readonly DataContractJsonSerializer serializer =
            new DataContractJsonSerializer(typeof (EnemyData));

        private static readonly object margeLock = new object();
        private static readonly object saveLoadLock = new object();
        public static EnemyData Curret { get; } = new EnemyData();

        private EnemyData()
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
            if (this.EnemyEncounterRank == null) this.EnemyEncounterRank = new Dictionary<string, HashSet<int>>();
            if (this.MapEnemyData == null) this.MapEnemyData = new Dictionary<int, Dictionary<int, HashSet<string>>>();
            if (this.MapCellBattleTypes == null)
                this.MapCellBattleTypes = new Dictionary<int, Dictionary<int, string>>();
            if (this.MapRoute == null) this.MapRoute = new Dictionary<int, HashSet<KeyValuePair<int, int>>>();
            if (this.MapCellDatas == null) this.MapCellDatas = new Dictionary<int, List<MapCellData>>();
        }

        // EnemyId, EnemyMasterIDs
        [DataMember]
        public Dictionary<string, int[]> EnemyDictionary { get; set; }

        // EnemyId, Formation
        [DataMember]
        public Dictionary<string, Formation> EnemyFormation { get; set; }

        // EnemyId, api_eSlot
        [DataMember]
        public Dictionary<string, int[][]> EnemySlotItems { get; set; }

        // EnemyId, api_eKyouka
        [DataMember]
        public Dictionary<string, int[][]> EnemyUpgraded { get; set; }

        // EnemyId, api_eParam
        [DataMember]
        public Dictionary<string, int[][]> EnemyParams { get; set; }

        // EnemyId, api_ship_lv
        [DataMember]
        public Dictionary<string, int[]> EnemyLevels { get; set; }

        // EnemyId, MaxHP
        [DataMember]
        public Dictionary<string, int[]> EnemyHPs { get; set; }

        // EnemyId, Name
        [DataMember]
        public Dictionary<string, string> EnemyNames { get; set; }

        // EnemyId, Rank
        [DataMember]
        public Dictionary<string, HashSet<int>> EnemyEncounterRank { get; set; }

        // MapInfoID, CellNo, EnemyId
        [DataMember]
        public Dictionary<int, Dictionary<int, HashSet<string>>> MapEnemyData { get; set; }

        // MapInfoID, CellNo, BattleApiClassName
        [DataMember]
        public Dictionary<int, Dictionary<int, string>> MapCellBattleTypes { get; set; }

        // MapInfoID, FromCellNo, ToCellNo
        [DataMember]
        public Dictionary<int, HashSet<KeyValuePair<int, int>>> MapRoute { get; set; }

        // MapInfoID, MapCellData
        [DataMember]
        public Dictionary<int, List<MapCellData>> MapCellDatas { get; set; }

        internal Task<bool> Merge(string path)
        {
            return Task.Run(() =>
            {
                if (!File.Exists(path)) return false;

                lock (margeLock)
                {
                    using (var stream = Stream.Synchronized(new FileStream(path, FileMode.Open, FileAccess.Read)))
                    {
                        var obj = serializer.ReadObject(stream) as EnemyData;
                        if (obj == null) return false;

                        this.EnemyDictionary = this.EnemyDictionary.Merge(obj.EnemyDictionary);
                        this.EnemyFormation = this.EnemyFormation.Merge(obj.EnemyFormation);
                        this.EnemySlotItems = this.EnemySlotItems.Merge(obj.EnemySlotItems);
                        this.EnemyUpgraded = this.EnemyUpgraded.Merge(obj.EnemyUpgraded);
                        this.EnemyParams = this.EnemyParams.Merge(obj.EnemyParams);
                        this.EnemyLevels = this.EnemyLevels.Merge(obj.EnemyLevels);
                        this.EnemyHPs = this.EnemyHPs.Merge(obj.EnemyHPs);
                        this.EnemyNames = this.EnemyNames.Merge(obj.EnemyNames);
                        this.EnemyEncounterRank = this.EnemyEncounterRank.Merge(obj.EnemyEncounterRank);
                        this.MapEnemyData = this.MapEnemyData.Merge(obj.MapEnemyData, (v1, v2) => v1.Merge(v2, (h1, h2) => h1.Merge(h2)));
                        this.MapCellBattleTypes = this.MapCellBattleTypes.Merge(obj.MapCellBattleTypes, (v1, v2) => v1.Merge(v2));
                        this.MapRoute = this.MapRoute.Merge(obj.MapRoute, (v1, v2) => v1.Merge(v2));
                        this.MapCellDatas = this.MapCellDatas.Merge(obj.MapCellDatas, (v1, v2) => v1.Merge(v2, x => x.No));
                    }

                    this.RemoveDuplicate();
                    this.Save();
                }
                return true;
            });
        }

        internal void Reload()
        {
            var obj = Settings.Default.EnemyDataFileName.Deserialize<EnemyData>();
            if (obj == null) return;

            this.EnemyDictionary = obj.EnemyDictionary ?? new Dictionary<string, int[]>();
            this.EnemyFormation = obj.EnemyFormation ?? new Dictionary<string, Formation>();
            this.EnemySlotItems = obj.EnemySlotItems ?? new Dictionary<string, int[][]>();
            this.EnemyUpgraded = obj.EnemyUpgraded ?? new Dictionary<string, int[][]>();
            this.EnemyParams = obj.EnemyParams ?? new Dictionary<string, int[][]>();
            this.EnemyLevels = obj.EnemyLevels ?? new Dictionary<string, int[]>();
            this.EnemyHPs = obj.EnemyHPs ?? new Dictionary<string, int[]>();
            this.EnemyNames = obj.EnemyNames ?? new Dictionary<string, string>();
            this.EnemyEncounterRank = obj.EnemyEncounterRank ?? new Dictionary<string, HashSet<int>>();
            this.MapEnemyData = obj.MapEnemyData ?? new Dictionary<int, Dictionary<int, HashSet<string>>>();
            this.MapCellBattleTypes = obj.MapCellBattleTypes ?? new Dictionary<int, Dictionary<int, string>>();
            this.MapRoute = obj.MapRoute ?? new Dictionary<int, HashSet<KeyValuePair<int, int>>>();
            this.MapCellDatas = obj.MapCellDatas ?? new Dictionary<int, List<MapCellData>>();
            this.RemoveDuplicate();
        }

        internal void Save()
            => this.Serialize(Settings.Default.EnemyDataFileName);

        internal void RemoveDuplicate()
        {
            var keysList = this.MapEnemyData.Values.SelectMany(x => x.Values.ToArray()).ToArray();
            foreach (var keys in keysList)
            {
                keys.GroupBy(key => key, this.GetComparer())
                    .Where(x => 1 < x.Count())
                    .SelectMany(x => x.Skip(1))
                    .ToList()
                    .ForEach(this.RemoveEnemy);
            }
        }

        internal void RemoveEnemy(string enemyId)
        {
            this.EnemyDictionary.Remove(enemyId);
            this.EnemyFormation.Remove(enemyId);
            this.EnemySlotItems.Remove(enemyId);
            this.EnemyUpgraded.Remove(enemyId);
            this.EnemyParams.Remove(enemyId);
            this.EnemyLevels.Remove(enemyId);
            this.EnemyHPs.Remove(enemyId);
            this.EnemyNames.Remove(enemyId);
            this.EnemyEncounterRank.Remove(enemyId);

            var kvps = this.MapEnemyData
                .SelectMany(x => x.Value);
            foreach (var kvp in kvps)
            {
                kvp.Value.Remove(enemyId);
            }
        }

        private EnemyDataComparer comparer;
        internal EnemyDataComparer GetComparer()
        {
            return this.comparer ?? (this.comparer = new EnemyDataComparer(this));
        }
    }

    internal class EnemyDataComparer : IEqualityComparer<string>
    {
        private readonly EnemyData enemyData;

        public EnemyDataComparer(EnemyData data)
        {
            this.enemyData = data;
        }

        public bool Equals(string x, string y)
        {
            if (x == y) return true;
            if (x == null || y == null) return false;
            return this.enemyData.EnemyDictionary.GetValueOrDefault(x).EqualsValue(this.enemyData.EnemyDictionary.GetValueOrDefault(y))
                && this.enemyData.EnemyFormation.GetValueOrDefault(x).Equals(this.enemyData.EnemyFormation.GetValueOrDefault(y))
                && this.enemyData.EnemySlotItems.GetValueOrDefault(x).EqualsValue(this.enemyData.EnemySlotItems.GetValueOrDefault(y))
                && this.enemyData.EnemyUpgraded.GetValueOrDefault(x).EqualsValue(this.enemyData.EnemyUpgraded.GetValueOrDefault(y))
                && this.enemyData.EnemyParams.GetValueOrDefault(x).EqualsValue(this.enemyData.EnemyParams.GetValueOrDefault(y))
                && this.enemyData.EnemyLevels.GetValueOrDefault(x).EqualsValue(this.enemyData.EnemyLevels.GetValueOrDefault(y))
                && this.enemyData.EnemyHPs.GetValueOrDefault(x).EqualsValue(this.enemyData.EnemyHPs.GetValueOrDefault(y))
                && this.enemyData.EnemyNames.GetValueOrDefault(x) == this.enemyData.EnemyNames.GetValueOrDefault(y);
        }

        public int GetHashCode(string key)
        {
            return this.enemyData.EnemyDictionary.GetValueOrDefault(key).GetValuesHashCode()
                ^ this.enemyData.EnemyFormation.GetValueOrDefault(key).ToString().GetHashCode()
                ^ this.enemyData.EnemySlotItems.GetValueOrDefault(key).GetValuesHashCode(x => x.GetValuesHashCode())
                ^ this.enemyData.EnemyUpgraded.GetValueOrDefault(key).GetValuesHashCode(x => x.GetValuesHashCode())
                ^ this.enemyData.EnemyParams.GetValueOrDefault(key).GetValuesHashCode(x => x.GetValuesHashCode())
                ^ this.enemyData.EnemyLevels.GetValueOrDefault(key).GetValuesHashCode()
                ^ this.enemyData.EnemyHPs.GetValueOrDefault(key).GetValuesHashCode()
                ^ (this.enemyData.EnemyNames.GetValueOrDefault(key)?.GetHashCode() ?? 0)
                ;
        }
    }
}
