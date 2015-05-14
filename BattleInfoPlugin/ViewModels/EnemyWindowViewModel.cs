using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.ComponentModel;

using Livet;
using Livet.Commands;
using Livet.Messaging;
using Livet.Messaging.IO;
using Livet.EventListeners;
using Livet.Messaging.Windows;

using BattleInfoPlugin.Models;
using BattleInfoPlugin.ViewModels.Enemies;
using BattleInfoPlugin.Models.Repositories;

namespace BattleInfoPlugin.ViewModels
{
    public class EnemyWindowViewModel : ViewModel
    {
        public IEnumerable<EnemyMapViewModel> EnemyMaps { get; set; }

        public EnemyWindowViewModel()
        {
        }

        public EnemyWindowViewModel(Dictionary<MapInfo, Dictionary<int, Dictionary<int, FleetData>>> mapEnemies)
        {
            this.EnemyMaps = Master.Current.MapInfos
                .Select(mi => new EnemyMapViewModel
                {
                    Info = mi.Value,
                    //セルポイントデータに既知の敵データを外部結合して座標でマージ
                    MapCells = MapResource.HasMapSwf(mi.Value)
                        ? MapResource.GetMapCellPoints(mi.Value) //マップSWFがあったらそれを元に作る
                            //外部結合
                            .GroupJoin(
                                CreateMapCellViewModelsFromEnemiesData(mi, mapEnemies),
                                outer => outer.Key,
                                inner => inner.Key,
                                (o, ie) => new { point = o, cells = ie })
                            .SelectMany(
                                x => x.cells.DefaultIfEmpty(),
                                (x, y) => new { x.point, cells = y })
                            //座標マージ
                            .GroupBy(x => x.point.Value)
                            .Select(x => new MapCellViewModel
                            {
                                Key = x.Min(y => y.point.Key), //若い番号を採用
                                Enemies = x.Where(y => y.cells != null) //敵データをEnemyIdでマージ
                                    .SelectMany(y => y.cells.Enemies)
                                    .GroupBy(y => y.Key)
                                    .Select(y => y.First()),
                            })
                            //敵データのないセルは除外
                            .Where(x => x.Enemies.Any())
                        : CreateMapCellViewModelsFromEnemiesData(mi, mapEnemies) //なかったら敵データだけ(TODO 重複データの解決はEnemyIDでやる？)
                            .OrderBy(cell => cell.Key),
                })
                .OrderBy(info => info.Info.Id);
        }

        private static IEnumerable<MapCellViewModel> CreateMapCellViewModelsFromEnemiesData(
            KeyValuePair<int, MapInfo> mi,
            Dictionary<MapInfo, Dictionary<int, Dictionary<int, FleetData>>> mapEnemies)
        {
            return mapEnemies.Where(info => info.Key.Id == mi.Key)
                .Select(info => info.Value)
                .SelectMany(cells => cells)
                .Select(cell => new MapCellViewModel
                {
                    Key = cell.Key,
                    Enemies = cell.Value
                        .Select(enemy => new EnemyFleetViewModel
                        {
                            Key = enemy.Key,
                            Fleet = enemy.Value,
                        })
                        .OrderBy(enemy => enemy.Key)
                });
        }

        public void Initialize()
        {
        }
    }
}
