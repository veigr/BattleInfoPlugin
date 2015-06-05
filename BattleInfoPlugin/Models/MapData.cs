using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleInfoPlugin.Models.Repositories;

namespace BattleInfoPlugin.Models
{
    public class MapData
    {
        private readonly EnemyDataProvider provider = new EnemyDataProvider();

        public IReadOnlyDictionary<MapInfo, Dictionary<MapCell, Dictionary<int, FleetData>>> GetMapEnemies()
        {
            return this.provider.GetMapEnemies();
        }

        public IReadOnlyDictionary<int, List<MapCellData>> GetCellDatas()
        {
            return this.provider.GetMapCellDatas();
        }

        public IReadOnlyDictionary<MapCell, CellType> GetCellTypes()
        {
            var cells = Master.Current.MapCells.Select(c => c.Value);
            var cellDatas = this.provider.GetMapCellDatas();
            return this.provider.GetMapCellBattleTypes()
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
    }
}
