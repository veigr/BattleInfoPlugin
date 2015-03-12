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
        private string _FleetFormation;

        public string FleetFormation
        {
            get
            { return this._FleetFormation; }
            set
            {
                if (this._FleetFormation == value)
                    return;
                this._FleetFormation = value;
                this.RaisePropertyChanged();
            }
        }

        #endregion


        #region FormationSource変更通知プロパティ
        private Formation _FormationSource;

        public Formation FormationSource
        {
            get
            { return this._FormationSource; }
            set
            { 
                if (this._FormationSource == value)
                    return;
                this._FormationSource = value;
                this.RaisePropertyChanged();

                this.FleetFormation = value != Formation.なし ? value.ToString() : "";
            }
        }
        #endregion

        public FleetViewModel():this("")
        {
        }

        public FleetViewModel(string name, ShipData[] data = null, Formation formation = Formation.なし)
        {
            this.Name = name;
            this.Fleet = data;
            this.FormationSource = formation;
        }
    }
}
