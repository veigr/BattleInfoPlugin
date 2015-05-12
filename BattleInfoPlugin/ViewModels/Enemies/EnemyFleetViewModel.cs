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

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class EnemyFleetViewModel : ViewModel
    {
        public int Key { get; set; }

        public string Name
        {
            get
            {
                return !string.IsNullOrWhiteSpace(this.Fleet.Name)
                    ? this.Fleet.Name
                    : "？？？";
            }
        }

        public FleetData Fleet { get; set; }

        public IEnumerable<ShipData> Ships { get { return this.Fleet.Ships; } }
    }
}
