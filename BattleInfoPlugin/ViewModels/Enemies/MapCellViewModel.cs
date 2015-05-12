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
    public class MapCellViewModel : ViewModel
    {
        public int Key { get; set; }
        public IEnumerable<EnemyFleetViewModel> Enemies { get; set; }
        
        public void Initialize()
        {
        }
    }
}
