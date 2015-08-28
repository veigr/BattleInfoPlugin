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
                            .GroupBy(y => y.Key, EnemyData.Curret.GetComparer())
                            .OrderBy(y => y.Key)
                            .Select(y => y.First())
                            .ToArray(),
                        ColorNo = x.Where(y => y.cells != null).Select(y => y.cells.ColorNo).FirstOrDefault(),
                        CellType = x.Where(y => y.cells != null).Select(y => y.cells.CellType).FirstOrDefault(),
                    })
                    //敵データのないセルは除外
                    .Where(x => x.EnemyFleets.Any())
                    .ToArray()
                : CreateMapCellViewModelsFromEnemiesData(mi, mapEnemies, cellTypes) //なかったら敵データだけ(重複るが仕方ない)
                    .OrderBy(cell => cell.Key)
                    .ToArray();
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
                    EnemyFleets = cell.Value
                        .Select(enemy => new EnemyFleetViewModel
                        {
                            Key = enemy.Key,
                            Fleet = enemy.Value,
                            EnemyShips = enemy.Value.Ships.Select(s => new EnemyShipViewModel { Ship = s }).ToArray(),
                        })
                        .GroupBy(x => x.Key, EnemyData.Curret.GetComparer())
                        .Select(x => x.First())
                        .OrderBy(enemy => enemy.Key)
                        .ToArray(),
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
}
