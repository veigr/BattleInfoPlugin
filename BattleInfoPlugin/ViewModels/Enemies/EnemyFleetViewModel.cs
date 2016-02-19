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
using System.Windows;
using MetroTrilithon.Linq;

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class EnemyFleetViewModel : ViewModel
    {
        public string Key
            => this.Fleets.Keys.JoinString("\r\n");

        public string Name
            => this.Fleet?.Name ?? "？？？";

        public string Rank
            => string.Join(", ", this.Fleet?.Rank.Where(x => 0 < x).Select(x =>
            {
                switch (x)
                {
                    case 1:
                        return "丙";
                    case 2:
                        return "乙";
                    case 3:
                        return "甲";
                    default:
                        return "？";
                }
            }));

        public Visibility RankVisibility
            => !string.IsNullOrEmpty(this.Rank) ? Visibility.Visible : Visibility.Collapsed;
        
        public Dictionary<string, FleetData> Fleets { get; set; } // 陣形ごとのデータ

        public FleetData Fleet => this.Fleets.Values.FirstOrDefault();

        public string Formation => Fleets.Values
                                    .OrderBy(x => x.Formation)
                                    .Select(x => x.Formation.ToString())
                                    .Distinct()
                                    .JoinString(", ");

        #region EnemyShips

        private EnemyShipViewModel[] _EnemyShips;

        public EnemyShipViewModel[] EnemyShips
        {
            get { return this._EnemyShips; }
            set
            {
                this._EnemyShips = value;
                if (value == null) return;
                foreach (var val in value)
                {
                    val.ParentFleet = this;
                }
            }
        }

        #endregion

        public EnemyCellViewModel ParentCell { get; set; }

        public void DeleteEnemy()
        {
            System.Diagnostics.Debug.WriteLine($"DeleteEnemy:{this.Key}");
            if (MessageBoxResult.OK != MessageBox.Show(
                $"{this.Name}(key:{this.Key})のデータを削除してよろしいですか？",
                "確認",
                MessageBoxButton.OKCancel,
                MessageBoxImage.Question))
                return;

            foreach (var key in this.Fleets.Keys)
            {
                this.ParentCell.ParentMap.WindowViewModel.RemoveEnemy(key);
            }
        }

        public void CopyIdToClipboard()
        {
            Clipboard.SetText(this.Key);
        }
    }
}
