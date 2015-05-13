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
                    MapCells = mapEnemies.Where(info => info.Key.Id == mi.Key)
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
                        })
                        .OrderBy(cell => cell.Key)
                })
                .OrderBy(info => info.Info.Id);
        }

        public void Initialize()
        {
        }
    }
}
