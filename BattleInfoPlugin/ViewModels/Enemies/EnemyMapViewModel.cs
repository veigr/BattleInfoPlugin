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
using Grabacr07.KanColleViewer.ViewModels;
using Grabacr07.KanColleWrapper.Models;

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class EnemyMapViewModel : TabItemViewModel
    {
        public MapInfo Info { get; set; }

        public IEnumerable<MapCellViewModel> MapCells { get; set; }

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
