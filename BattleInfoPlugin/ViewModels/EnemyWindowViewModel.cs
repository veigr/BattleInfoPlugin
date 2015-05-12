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
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models;

namespace BattleInfoPlugin.ViewModels
{
    public class EnemyWindowViewModel : ViewModel
    {
        public IEnumerable<MapAreaViewModel> MapAreas { get; set; }

        public EnemyWindowViewModel()
        {
        }

        public EnemyWindowViewModel(Dictionary<MapInfo, Dictionary<int, Dictionary<int, FleetData>>> mapEnemies)
        {
            //TODO セル同一視
            this.MapAreas = KanColleClient.Current.Master.MapAreas
                .Select(area => new MapAreaViewModel
                {
                    Key = area.Value,
                    MapInfos = mapEnemies.Where(info => info.Key.MapAreaId == area.Key)
                        .Select(info => new MapInfoViewModel
                        {
                            Key = info.Key,
                            MapCells = info.Value
                                .Select(cell => new MapCellViewModel
                                {
                                    Key = cell.Key,
                                    Enemies = cell.Value
                                        .Select(enemy => new EnemyFleetViewModel
                                        {
                                            Key = enemy.Key,
                                            Fleet = enemy.Value,
                                        })
                                        .OrderBy(enemy => enemy.Key),
                                })
                                .OrderBy(cell => cell.Key),
                        })
                        .OrderBy(info => info.Key.Id),
                })
                .OrderBy(area => area.Key.Id);
        }

        public void Initialize()
        {
        }
    }
}
