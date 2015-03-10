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
                this.IsVisible = value != null && value.Length != 0;
            }
        }
        #endregion


        #region IsVisible変更通知プロパティ
        private bool _IsVisible;

        public bool IsVisible
        {
            get
            { return this._IsVisible; }
            set
            { 
                if (this._IsVisible == value)
                    return;
                this._IsVisible = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        public FleetViewModel(string name, ShipData[] data)
        {
            this.Name = name;
            this.Fleet = data;
        }
    }
}
