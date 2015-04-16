namespace BattleInfoPlugin.Models
{
    public enum AirSupremacy
    {
        航空戦なし = -1,
        航空均衡 = 0,   // Air parity
        制空権確保 = 1,  // Air supremacy
        航空優勢 = 2,   // Air superiority
        航空劣勢 = 3,   // Air denial
        制空権損失 = 4,  // Air incapability
    }
}
