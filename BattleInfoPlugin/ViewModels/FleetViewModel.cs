using System;
using BattleInfoPlugin.Models;
using Livet;

namespace BattleInfoPlugin.ViewModels
{
    public class FleetViewModel : ViewModel
    {

        #region Name変更通知プロパティ
        private string _Name;

        public string Name
        {
            get
            { return this._Name; }
            set
            { 
                if (this._Name == value)
                    return;
                this._Name = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region Fleet変更通知プロパティ
        private ShipData[] _Fleet;

        public ShipData[] Fleet
        {
            get
            { return this._Fleet; }
            set
            {
                if (this._Fleet == value)
                    return;
                this._Fleet = value;
                this.RaisePropertyChanged();
                this.RaisePropertyChanged(() => this.IsVisible);
            }
        }
        #endregion


        #region IsVisible変更通知プロパティ

        public bool IsVisible
        {
            get
            { return this.Fleet != null && this.Fleet.Length != 0; }
        }
        #endregion


        #region FleetFormation変更通知プロパティ
        private Formation _FleetFormation;

        public string FleetFormation
        {
            get
            { return this._FleetFormation != Formation.なし ? this._FleetFormation.ToString() : string.Empty; }
            set
            {
                var newString = string.IsNullOrWhiteSpace(value) ? Formation.なし.ToString() : value;
                Formation newValue;
                if (!Enum.TryParse(newString, out newValue)) return;

                if (this._FleetFormation == newValue)
                    return;
                this._FleetFormation = newValue;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        public FleetViewModel(string name, ShipData[] data, Formation formation = Formation.なし)
        {
            this.Name = name;
            this.Fleet = data;
            this.FleetFormation = formation.ToString();
        }
    }
}
