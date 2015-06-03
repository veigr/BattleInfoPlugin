using System.Runtime.Serialization;

namespace BattleInfoPlugin.Models
{
    [DataContract]
    public class MapCellData
    {
        //セルの色
        [DataMember]
        public int ColorNo { get; set; }
        //吹き出しID
        [DataMember]
        public int CommentKind { get; set; }
        //セルイベントID
        [DataMember]
        public int EventId { get; set; }
        //セルイベント補足
        [DataMember]
        public int EventKind { get; set; }
        //セル番号
        [DataMember]
        public int No { get; set; }
        //エリアID
        [DataMember]
        public int MapAreaId { get; set; }
        //マップNo
        [DataMember]
        public int MapInfoIdInEachMapArea { get; set; }
        //要索敵
        [DataMember]
        public int ProductionKind { get; set; }
        //能動分岐
        [DataMember]
        public int[] SelectCells { get; set; }
    }
}
