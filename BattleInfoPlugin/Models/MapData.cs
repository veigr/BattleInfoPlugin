using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleInfoPlugin.Models.Repositories;
using System.IO;
using BattleInfoPlugin.Properties;

namespace BattleInfoPlugin.Models
{
    public class MapData
    {
        public EnemyDataProvider EnemyData { get; } = new EnemyDataProvider();

        public IReadOnlyDictionary<MapInfo, Dictionary<MapCell, Dictionary<string, FleetData>>> GetMapEnemies()
        {
            return this.EnemyData.GetMapEnemies();
        }

        public IReadOnlyDictionary<int, List<MapCellData>> GetCellDatas()
        {
            return this.EnemyData.GetMapCellDatas();
        }

        public IReadOnlyDictionary<MapCell, CellType> GetCellTypes()
        {
            var cells = Master.Current.MapCells.Select(c => c.Value);
            var cellDatas = this.EnemyData.GetMapCellDatas();
            return this.EnemyData.GetMapCellBattleTypes()
                .SelectMany(x => x.Value, (x, y) => new
                {
                    cell = cells.Single(c => c.MapInfoId == x.Key && c.IdInEachMapInfo == y.Key),
                    type = y.Value,
                })
                .Select(x => new
                {
                    x.cell,
                    type = x.type.ToCellType() | x.cell.ColorNo.ToCellType() | GetCellType(x.cell, cellDatas)
                })
                .ToDictionary(x => x.cell, x => x.type);
        }

        private static CellType GetCellType(MapCell cell, IReadOnlyDictionary<int, List<MapCellData>> cellData)
        {
            if (!cellData.ContainsKey(cell.MapInfoId)) return CellType.None;
            var datas = cellData[cell.MapInfoId];
            var data = datas.SingleOrDefault(x => cell.IdInEachMapInfo == x.No);
            if (data == default(MapCellData)) return CellType.None;
            return data.EventId.ToCellType();
        }

        public void Merge(string[] filePathList)
        {
            foreach (var filePath in filePathList)
            {
                Action<Task<bool>> continuationAction = x =>
                {
                    try
                    {
                        var result = x.Result;
                        if (result)
                            MergeResult?.Invoke(result, $"マージに成功しました。 : {filePath}");
                        else
                            MergeResult?.Invoke(result, $"マージに失敗しました。 : {filePath}");
                    }
                    catch (Exception)
                    {
                        MergeResult?.Invoke(false, $"マージに失敗しました。 : {filePath}");
                    }
                };

                var info = new FileInfo(filePath);
                if (info.Name == Settings.Default.EnemyDataFilePath)
                {
                    this.EnemyData.Merge(filePath)
                        .ContinueWith(continuationAction, TaskScheduler.FromCurrentSynchronizationContext());
                }else if (info.Name == Settings.Default.MasterDataFilePath)
                {
                    Master.Current.Merge(filePath)
                        .ContinueWith(continuationAction, TaskScheduler.FromCurrentSynchronizationContext());
                }
                else
                {
                    MergeResult?.Invoke(false, "マージ対象のファイル名ではありません。");
                }
            }
        }

        public event Action<bool, string> MergeResult;
    }
}
