using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using BattleInfoPlugin.Models;
using Livet;

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class EnemyShipViewModel : ViewModel
    {
        public ShipData Ship { get; set; }

        public EnemyMapViewModel MapViewModel { get; set; }

        public EnemyCellViewModel CellViewModel { get; set; }

        public EnemyFleetViewModel FleetViewModel { get; set; }
    }
}
