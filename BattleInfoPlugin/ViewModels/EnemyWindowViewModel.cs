using System.Collections.Generic;
using System.Linq;
using BattleInfoPlugin.Models;
using BattleInfoPlugin.Models.Repositories;
using BattleInfoPlugin.ViewModels.Enemies;
using Livet;
using Livet.Messaging;
using System.Windows;

namespace BattleInfoPlugin.ViewModels
{
    public class EnemyWindowViewModel : ViewModel
    {
        private readonly MapData mapData = new MapData();

        #region EnemyMaps変更通知プロパティ
        private EnemyMapViewModel[] _EnemyMaps;

        public EnemyMapViewModel[] EnemyMaps
        {
            get
            { return this._EnemyMaps; }
            set
            {
                if (this._EnemyMaps == value)
                    return;
                this._EnemyMaps = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion

        #region SelectedMap変更通知プロパティ
        private EnemyMapViewModel _SelectedMap;

        public EnemyMapViewModel SelectedMap
        {
            get
            { return this._SelectedMap; }
            set
            { 
                if (this._SelectedMap == value)
                    return;
                this._SelectedMap = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        public EnemyWindowViewModel()
        {
            this._EnemyMaps = this.CreateEnemyMaps();
            this.mapData.MergeResult += MapData_MergeResult;
        }

        private void MapData_MergeResult(bool result, string message)
        {
            if (result)
            {
                this.Messenger.Raise(new InformationMessage(message, "マージ成功", MessageBoxImage.Information, "MergeResult"));
                this.EnemyMaps = this.CreateEnemyMaps();
            }
            else
            {
                this.Messenger.Raise(new InformationMessage(message, "マージ失敗", MessageBoxImage.Warning, "MergeResult"));
            }
        }

        public void Merge(string[] filePathList)
        {
            this.mapData.Merge(filePathList);
        }

        private EnemyMapViewModel[] CreateEnemyMaps()
        {
            var mapEnemies = this.mapData.GetMapEnemies();
            var cellTypes = this.mapData.GetCellTypes();
            var cellDatas = this.mapData.GetCellDatas();
            return Master.Current.MapInfos
                .Select(mi => new EnemyMapViewModel
                {
                    Info = mi.Value,
                    CellDatas = cellDatas.ContainsKey(mi.Key) ? cellDatas[mi.Key] : new List<MapCellData>(),
                    //セルポイントデータに既知の敵データを外部結合して座標でマージ
                    EnemyCells = MapResource.HasMapSwf(mi.Value)
                        ? MapResource.GetMapCellPoints(mi.Value) //マップSWFがあったらそれを元に作る
                                                                 //外部結合
                            .GroupJoin(
                                CreateMapCellViewModelsFromEnemiesData(mi, mapEnemies, cellTypes),
                                outer => outer.Key,
                                inner => inner.Key,
                                (o, ie) => new { point = o, cells = ie })
                            .SelectMany(
                                x => x.cells.DefaultIfEmpty(),
                                (x, y) => new { x.point, cells = y })
                            //座標マージ
                            .GroupBy(x => x.point.Value)
                            .Select(x => new EnemyCellViewModel
                            {
                                Key = x.Min(y => y.point.Key), //若い番号を採用
                                EnemyFleets = x.Where(y => y.cells != null) //敵データをEnemyIdでマージ
                                    .SelectMany(y => y.cells.EnemyFleets)
                                    .GroupBy(y => y.Key)
                                    .OrderBy(y => y.Key)
                                    .Select(y => y.First())
                                    .Distinct(new FleetComparer())  //同一編成の敵を除去
                                    .ToArray(),
                                ColorNo = x.Where(y => y.cells != null).Select(y => y.cells.ColorNo).FirstOrDefault(),
                                CellType = x.Where(y => y.cells != null).Select(y => y.cells.CellType).FirstOrDefault(),
                            })
                            //敵データのないセルは除外
                            .Where(x => x.EnemyFleets.Any())
                            .ToArray()
                        : CreateMapCellViewModelsFromEnemiesData(mi, mapEnemies, cellTypes) //なかったら敵データだけ(重複るが仕方ない)
                            .OrderBy(cell => cell.Key)
                            .ToArray(),
                })
                .OrderBy(info => info.Info.Id)
                .ToArray();
        }

        private static IEnumerable<EnemyCellViewModel> CreateMapCellViewModelsFromEnemiesData(
            KeyValuePair<int, MapInfo> mi,
            IReadOnlyDictionary<MapInfo, Dictionary<MapCell, Dictionary<string, FleetData>>> mapEnemies,
            IReadOnlyDictionary<MapCell, CellType> cellTypes)
        {
            return mapEnemies.Where(info => info.Key.Id == mi.Key)
                .Select(info => info.Value)
                .SelectMany(cells => cells)
                .Select(cell => new EnemyCellViewModel
                {
                    Key = cell.Key.IdInEachMapInfo,
                    EnemyFleets = cell.Value
                        .Select(enemy => new EnemyFleetViewModel
                        {
                            Key = enemy.Key,
                            Fleet = enemy.Value,
                            EnemyShips = enemy.Value.Ships.Select(s => new EnemyShipViewModel { Ship = s }).ToArray(),
                        })
                        .OrderBy(enemy => enemy.Key)
                        .Distinct(new FleetComparer())  //同一編成の敵を除去
                        .ToArray(),
                    ColorNo = cell.Key.ColorNo,
                    CellType = cell.Key.GetCellType(cellTypes),
                });
        }

        public void Initialize()
        {
        }
    }

    class FleetComparer : IEqualityComparer<EnemyFleetViewModel>
    {
        public bool Equals(EnemyFleetViewModel x, EnemyFleetViewModel y)
        {
            return this.GetHashCode(x) == this.GetHashCode(y);
        }

        public int GetHashCode(EnemyFleetViewModel obj)
        {
            var name = obj.Fleet.Name;
            var formation = obj.Fleet.Formation;
            var ships = obj.Fleet.Ships.OfType<MastersShipData>().Where(x => x.Source != null).ToArray();
            var slots = obj.Fleet.Ships.SelectMany(x => x.Slots).Where(x => x.Source != null).Select(x => x.Source.Id).ToArray();
            return name.GetHashCode()
                   ^ formation.GetHashCode()
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.Source.Id.GetHashCode())
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.MaxHP.GetHashCode())
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.Level.GetHashCode())
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.Firepower.GetHashCode())
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.Torpedo.GetHashCode())
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.AA.GetHashCode())
                   ^ ships.Aggregate(0, (a, b) => a.GetHashCode() ^ b.Armer.GetHashCode())
                   ^ slots.Aggregate(0, (a, b) => a.GetHashCode() ^ b.GetHashCode());
        }
    }
}
