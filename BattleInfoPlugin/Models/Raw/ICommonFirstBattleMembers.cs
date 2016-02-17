using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models.Raw
{
    interface ICommonFirstBattleMembers : ICommonBattleMembers
    {
        int[] api_formation { get; set; }
    }
}
