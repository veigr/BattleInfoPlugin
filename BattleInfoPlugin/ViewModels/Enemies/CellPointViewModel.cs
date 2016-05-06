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

namespace BattleInfoPlugin.ViewModels.Enemies
{
    public class CellPointViewModel : ViewModel
    {
        public string Label { get; }
        public Point Point { get; }
        public int ColorNo { get; }
        public string Distance { get; }
        public bool HasDistance { get; }
        public CellPointViewModel(string label, Point point, int colorNo, int distance)
        {
            this.Label = label;
            this.Point = point;
            this.ColorNo = colorNo;
            this.Distance = 0 < distance ? distance.ToString() : "";
            this.HasDistance = 0 < distance;
        }
    }
}
