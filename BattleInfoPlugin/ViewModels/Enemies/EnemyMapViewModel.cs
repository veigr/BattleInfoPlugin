using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;
using System.Windows;
using System.Windows.Media.Imaging;
using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using BattleInfoPlugin.Models;
using BattleInfoPlugin.Models.Repositories;

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class EnemyMapViewModel : TabItemViewModel
    {
        public EnemyWindowViewModel WindowViewModel { get; set; }

        public MapInfo Info { get; set; }

        public List<MapCellData> CellDatas { get; set; }

        #region EnemyCells

        private EnemyCellViewModel[] _EnemyCells;

        public EnemyCellViewModel[] EnemyCells
        {
            get { return this._EnemyCells; }
            set
            {
                this._EnemyCells = value;
                if (value == null) return;
                foreach (var val in value)
                {
                    val.ParentMap = this;
                }
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(nameof(this.EnemyShips));
            }
        }

        #endregion

        public IEnumerable<EnemyShipViewModel> EnemyShips
            => this.EnemyCells.SelectMany(x => x.EnemyFleets).SelectMany(x => x.EnemyShips);

        public BitmapSource[] MapImages => MapResource.GetMapImages(this.Info);

        public bool HasImage => this.MapImages != null && this.MapImages.Any();

        public bool ExistsMapAssembly => MapResource.ExistsAssembly;

        public CellPointViewModel[] CellPoints
        {
            get
            {
                return MapResource.GetMapCellPoints(this.Info)
                    .Where(kvp => kvp.Value != default(Point)) //座標データがないものを除去 e.g. 6-3-13
                    .GroupBy(kvp => kvp.Value) //重複ポイントを除去
                    .Select(g => g.OrderBy(x => x.Key).First())
                    .Select(x => CreateCellPoint(x))
                    .ToArray();
            }
        }

        public IEnumerable<Point> Flags => MapResource.GetMapFlags(this.Info);

        private CellPointViewModel CreateCellPoint(KeyValuePair<int, Point> source)
        {
            var data = this.CellDatas.FirstOrDefault(x => x.No == source.Key);
            var cell = Master.Current.MapCells
                .Select(c => c.Value)
                .FirstOrDefault(c => c.IdInEachMapInfo == source.Key && c.MapInfoId == this.Info.Id);
            return new CellPointViewModel(
                source.Key.ToString(),
                source.Value,
                data?.ColorNo ?? cell?.ColorNo ?? 0,
                data?.Distance ?? 0);
        }

        public override string Name
        {
            get
            {
                return this.MapNo + ": " + this.Info.Name;
            }
            protected set { throw new NotImplementedException(); }
        }

        public string MapNo => this.Info.MapAreaId + "-" + this.Info.IdInEachMapArea;

        public string RequiredDefeatCount => 21 < this.Info.MapAreaId ? "Event" : this.Info.RequiredDefeatCount.ToString();
    }
}
