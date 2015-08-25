using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace BattleInfoPlugin.Models
{
    public class AirCombatResult
    {
        public string Name { get; }
        public bool IsHappen { get; }
        public int FriendCount { get; }
        public int FriendLostCount { get; }
        public int FriendRemainingCount => this.FriendCount - this.FriendLostCount;
        public int EnemyCount { get; }
        public int EnemyLostCount { get; }
        public int EnemyRemainingCount => this.EnemyCount - this.EnemyLostCount;
        public AirCombatResult(string name, int fCount, int fLost, int eCount, int eLost, bool isHappen = true)
        {
            this.Name = name;
            this.IsHappen = isHappen;
            this.FriendCount = fCount;
            this.FriendLostCount = fLost;
            this.EnemyCount = eCount;
            this.EnemyLostCount = eLost;
        }

        public AirCombatResult(string name)
        {
            this.Name = name;
            this.IsHappen = false;
            this.FriendCount = 0;
            this.FriendLostCount = 0;
            this.EnemyCount = 0;
            this.EnemyLostCount = 0;
        }
    }
}
