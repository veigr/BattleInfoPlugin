using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models
{
    [Flags]
    public enum CellType
    {
        None = 0,

        開始 = 1 << 0,
        イベント無し = 1 << 1,
        補給 = 1 << 2,
        渦潮 = 1 << 3,
        戦闘 = 1 << 4,
        ボス = 1 << 5,
        気のせい = 1 << 6,  //Frameでは気のせい変更前(赤)
        航空戦 = 1 << 7,
        母港 = 1 << 8,

        夜戦 = 1 << 31,
    }

    public static class CellTypeExtensions
    {
        public static CellType ToCellType(this int colorNo)
        {
            return (CellType)(1 << colorNo);
        }

        public static CellType ToCellType(this string battleType)
        {
            return battleType.Contains("sp_midnight") ? CellType.夜戦
                : battleType.Contains("airbattle") ? CellType.航空戦
                : CellType.None;
        }

        public static CellType GetCellType(this MapCell cell, Dictionary<MapCell, CellType> knownTypes)
        {
            var result = CellType.None;
            if (knownTypes.ContainsKey(cell)) result = result | knownTypes[cell];
            var cellMaster = Repositories.Master.Current.MapCells[cell.Id];
            result = result | cellMaster.ColorNo.ToCellType();
            return result;
        }
    }
}
