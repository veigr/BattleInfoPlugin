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
using Grabacr07.KanColleViewer.ViewModels;

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class EnemyMapViewModel : TabItemViewModel
    {
        public MapInfo Info { get; set; }

        public IEnumerable<MapCellViewModel> MapCells { get; set; }

        public BitmapSource MapImage { get { return MapResource.GetMapImage(this.Info); } }

        public bool HasImage { get { return this.MapImage != null; } }

        public IDictionary<string, Point> CellPoints
        {
            get
            {
                return MapResource.GetMapCellPoints(this.Info)
                    .GroupBy(kvp => kvp.Value)  //重複ポイントを除去
                    .Select(g => g.First())
                    .ToDictionary(x => x.Key.ToString(), x => x.Value);
            }
        }

        public override string Name
        {
            get
            {
                return this.Info.MapAreaId + "-" + this.Info.IdInEachMapArea
                    + ": " + this.Info.Name;
            }
            protected set { throw new NotImplementedException(); }
        }
    }
}
