using System;
using System.Collections.Generic;
using System.Linq;
using BattleInfoPlugin.Models;
using BattleInfoPlugin.Models.Repositories;
using BattleInfoPlugin.ViewModels.Enemies;
using Livet;
using Livet.Messaging;
using MetroTrilithon.Mvvm;
using System.Windows;
using System.ComponentModel;
using System.Reflection;
using MetroTrilithon.Linq;

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

        public void Initialize()
        {
        }

        public void Merge(string[] filePathList)
        {
            this.mapData.Merge(filePathList);
        }

        public void RemoveEnemy(string enemyId)
        {
            this.mapData.EnemyData.RemoveEnemy(enemyId);
            
            this.SelectedMap.EnemyCells = CreateEnemyCells(this.SelectedMap.Info, this.mapData.GetMapEnemies(), this.mapData.GetCellTypes());
        }

        private EnemyMapViewModel[] CreateEnemyMaps()
        {
            var mapEnemies = this.mapData.GetMapEnemies();
            var cellTypes = this.mapData.GetCellTypes();
            var cellDatas = this.mapData.GetCellDatas();
            return Master.Current.MapInfos
                .Select(mi => new EnemyMapViewModel
                {
                    WindowViewModel = this,
                    Info = mi.Value,
                    CellDatas = cellDatas.ContainsKey(mi.Key) ? cellDatas[mi.Key] : new List<MapCellData>(),
                    //セルポイントデータに既知の敵データを外部結合して座標でマージ
                    EnemyCells = CreateEnemyCells(mi.Value, mapEnemies, cellTypes),
                })
                .OrderBy(info => info.Info.Id)
                .ToArray();
        }

        private static EnemyCellViewModel[] CreateEnemyCells(
            MapInfo mi,
            IReadOnlyDictionary<MapInfo, Dictionary<MapCell, Dictionary<string, FleetData>>> mapEnemies,
            IReadOnlyDictionary<MapCell, CellType> cellTypes)
        {
            return MapResource.HasMapSwf(mi)
                ? MapResource.GetMapCellPoints(mi) //マップSWFがあったらそれを元に作る
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
                            .SelectMany(y => y.Fleets)
                            .MergeEnemies(),
                        ColorNo = x.Where(y => y.cells != null).Select(y => y.cells.ColorNo).FirstOrDefault(),
                        CellType = x.Where(y => y.cells != null).Select(y => y.cells.CellType).FirstOrDefault(),
                    })
                    //敵データのないセルは除外
                    .Where(x => x.EnemyFleets.Any())
                    .ToArray()
                : CreateMapCellViewModelsFromEnemiesData(mi, mapEnemies, cellTypes).ToArray();  //なかったら敵データだけ(重複るが仕方ない)
        }

        private static IEnumerable<EnemyCellViewModel> CreateMapCellViewModelsFromEnemiesData(
            MapInfo mi,
            IReadOnlyDictionary<MapInfo, Dictionary<MapCell, Dictionary<string, FleetData>>> mapEnemies,
            IReadOnlyDictionary<MapCell, CellType> cellTypes)
        {
            return mapEnemies.Where(info => info.Key.Id == mi.Id)
                .Select(info => info.Value)
                .SelectMany(cells => cells)
                .Select(cell => new EnemyCellViewModel
                {
                    Key = cell.Key.IdInEachMapInfo,
                    EnemyFleets = cell.Value.MergeEnemies(),
                    ColorNo = cell.Key.ColorNo,
                    CellType = cell.Key.GetCellType(cellTypes),
                });
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
    }

    static class Extensions
    {
        public static IEnumerable<EnemyFleetViewModel> OrderFleets(this IEnumerable<EnemyFleetViewModel> fleets)
        {
            return fleets.OrderByDescending(enemy => enemy.Fleet.Rank.FirstOrDefault(x => x == 3))
                        .ThenByDescending(enemy => enemy.Fleet.Rank.FirstOrDefault(x => x == 2))
                        .ThenByDescending(enemy => enemy.Fleet.Rank.FirstOrDefault(x => x == 1))
                        .ThenBy(enemy => enemy.EnemyShips.Length)
                        .ThenBy(enemy => enemy.EnemyShips.ElementAtOrDefault(0)?.Ship?.Id ?? 0)
                        .ThenBy(enemy => enemy.EnemyShips.ElementAtOrDefault(1)?.Ship?.Id ?? 0)
                        .ThenBy(enemy => enemy.EnemyShips.ElementAtOrDefault(2)?.Ship?.Id ?? 0)
                        .ThenBy(enemy => enemy.EnemyShips.ElementAtOrDefault(3)?.Ship?.Id ?? 0)
                        .ThenBy(enemy => enemy.EnemyShips.ElementAtOrDefault(4)?.Ship?.Id ?? 0)
                        .ThenBy(enemy => enemy.EnemyShips.ElementAtOrDefault(5)?.Ship?.Id ?? 0)
                        .ThenBy(enemy => enemy.Key);
        }

        public static EnemyFleetViewModel[] MergeEnemies(this IEnumerable<KeyValuePair<string, FleetData>> enemies)
        {
            return enemies.GroupBy(x => x.Key, EnemyData.Curret.GetComparer())
                        .Select(x => x.First())
                        .GroupBy(x => x.Value.Ships.Select(s => s.Id).JoinString(","))
                        .Select(enemy => new EnemyFleetViewModel
                        {
                            Fleets = enemy.ToDictionary(x => x.Key, x => x.Value),
                            EnemyShips = enemy.FirstOrDefault().Value.Ships.Select(s => new EnemyShipViewModel { Ship = s }).ToArray(),
                        })
                        .OrderFleets()
                        .ToArray();
        }
    }
}
