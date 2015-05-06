using System.Runtime.Serialization;

namespace BattleInfoPlugin.Models
{
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