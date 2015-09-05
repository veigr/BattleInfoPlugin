using System.Linq;
using Grabacr07.KanColleWrapper;

namespace BattleInfoPlugin.Models.Raw
{
    public interface ICommonBattleMembers
    {
        int[] api_ship_ke { get; set; }
        int[] api_ship_lv { get; set; }
        int[] api_nowhps { get; set; }
        int[] api_maxhps { get; set; }
        int[][] api_eSlot { get; set; }
        int[][] api_eKyouka { get; set; }
        int[][] api_fParam { get; set; }
        int[][] api_eParam { get; set; }
    }

    public static class CommonBattleMembersExtensions
    {
        public static MastersShipData[] ToMastersShipDataArray(this ICommonBattleMembers data)
        {
            var master = KanColleClient.Current.Master;
            return data.api_ship_ke
                .Where(x => x != -1)
                .Select((x, i) => new MastersShipData(master.Ships[x])
                {
                    Level = data.api_ship_lv[i + 1],
                    Firepower = data.api_eParam[i][0] + data.api_eKyouka[i][0],
                    Torpedo = data.api_eParam[i][1] + data.api_eKyouka[i][1],
                    AA = data.api_eParam[i][2] + data.api_eKyouka[i][2],
                    Armer = data.api_eParam[i][3] + data.api_eKyouka[i][3],
                    Slots = data.api_eSlot[i]
                        .Where(s => 0 < s)
                        .Select(s => master.SlotItems[s])
                        .Select(s => new ShipSlotData(s))
                        .ToArray(),
                })
                .ToArray();
        }
    }
}
