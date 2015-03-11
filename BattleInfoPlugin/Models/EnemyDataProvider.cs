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

namespace BattleInfoPlugin.Models
{
    [DataContract]
    public class EnemyDataProvider
    {
        private static DataContractJsonSerializer serializer = new DataContractJsonSerializer(typeof(EnemyDataProvider));

        [DataMember]
        private Dictionary<int, int[]> EnemyDictionary { get; set; }

        [DataMember]
        private Dictionary<int, Formation> EnemyFormation { get; set; }

        [NonSerialized]
        private int currentEnemyID;

        public EnemyDataProvider()
        {
            this.Reload();
            if (this.EnemyDictionary == null) this.EnemyDictionary = new Dictionary<int, int[]>();
            if (this.EnemyFormation == null) this.EnemyFormation = new Dictionary<int, Formation>();
            this.Dump("GetNextEnemyFormation");
        }

        public Formation GetNextEnemyFormation(map_start_next startNext)
        {
            this.Dump("GetNextEnemyFormation");

            if (startNext.api_enemy == null) return Formation.なし;
            this.currentEnemyID = startNext.api_enemy.api_enemy_id;

            return this.EnemyFormation.ContainsKey(startNext.api_enemy.api_enemy_id)
                ? this.EnemyFormation[startNext.api_enemy.api_enemy_id]
                : Formation.不明;
        }

        public ShipData[] GetNextEnemies(map_start_next startNext)
        {
            this.Dump("GetNextEnemies");

            if (startNext.api_enemy == null) return new ShipData[0];
            this.currentEnemyID = startNext.api_enemy.api_enemy_id;

            var master = KanColleClient.Current.Master.Ships;
            return this.EnemyDictionary.ContainsKey(startNext.api_enemy.api_enemy_id)
                ? this.EnemyDictionary[startNext.api_enemy.api_enemy_id].Select(x => new ShipData(master[x])).ToArray()
                : Enumerable.Repeat(new ShipData(), 6).ToArray();
        }

        public void UpdateEnemyData(int[] api_ship_ke, int[] api_formation)
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

    [DataContract]
    public enum Formation
    {
        [EnumMember]
        不明 = -1,
        [EnumMember]
        なし = 0,
        [EnumMember]
        単縦陣 = 1,
        [EnumMember]
        複縦陣 = 2,
        [EnumMember]
        輪形陣 = 3,
        [EnumMember]
        梯形陣 = 4,
        [EnumMember]
        単横陣 = 5,
        [EnumMember]
        対潜陣形 = 11,
        [EnumMember]
        前方陣形 = 12,
        [EnumMember]
        輪形陣形 = 13,
        [EnumMember]
        戦闘陣形 = 14,
    }
}
