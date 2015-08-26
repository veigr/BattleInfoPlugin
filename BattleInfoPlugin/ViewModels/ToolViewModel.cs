using System;
using System.Linq;
using BattleInfoPlugin.Models;
using BattleInfoPlugin.Models.Notifiers;
using Livet;
using Livet.EventListeners;
using Livet.Messaging;

namespace BattleInfoPlugin.ViewModels
{
    public class ToolViewModel : ViewModel
    {
        private readonly BattleEndNotifier notifier;

        private BattleData BattleData { get; } = new BattleData();

        public string BattleName
            => this.BattleData?.Name ?? "";

        public string UpdatedTime
            => this.BattleData != null && this.BattleData.UpdatedTime != default(DateTimeOffset)
                ? this.BattleData.UpdatedTime.ToString("yyyy/MM/dd HH:mm:ss")
                : "No Data";

        public string BattleSituation
            => this.BattleData != null && this.BattleData.BattleSituation != Models.BattleSituation.なし
                ? this.BattleData.BattleSituation.ToString()
                : "";

        public string FriendAirSupremacy
            => this.BattleData != null && this.BattleData.FriendAirSupremacy != AirSupremacy.航空戦なし
                ? this.BattleData.FriendAirSupremacy.ToString()
                : "";

        public string DropShipName
            => this.BattleData?.DropShipName;

        public AirCombatResult[] AirCombatResults
            => this.BattleData?.AirCombatResults ?? new AirCombatResult[0];
        

        #region FirstFleet変更通知プロパティ
        private FleetViewModel _FirstFleet;

        public FleetViewModel FirstFleet
        {
            get
            { return this._FirstFleet; }
            set
            { 
                if (this._FirstFleet == value)
                    return;
                this._FirstFleet = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region SecondFleet変更通知プロパティ
        private FleetViewModel _SecondFleet;

        public FleetViewModel SecondFleet
        {
            get
            { return this._SecondFleet; }
            set
            { 
                if (this._SecondFleet == value)
                    return;
                this._SecondFleet = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region Enemies変更通知プロパティ
        private FleetViewModel _Enemies;

        public FleetViewModel Enemies
        {
            get
            { return this._Enemies; }
            set
            { 
                if (this._Enemies == value)
                    return;
                this._Enemies = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        #region IsNotifierEnabled変更通知プロパティ
        // ここ以外で変更しないのでModel変更通知は受け取らない雑対応
        public bool IsNotifierEnabled
        {
            get
            { return this.notifier.IsEnabled; }
            set
            {
                if (this.notifier.IsEnabled == value)
                    return;
                this.notifier.IsEnabled = value;
                this.RaisePropertyChanged();
            }
        }
        #endregion


        public ToolViewModel(Plugin plugin)
        {
            this.notifier = new BattleEndNotifier(plugin);
            this._FirstFleet = new FleetViewModel("自艦隊");
            this._SecondFleet = new FleetViewModel("護衛艦隊");
            this._Enemies = new FleetViewModel("敵艦隊");

            this.CompositeDisposable.Add(new PropertyChangedEventListener(this.BattleData)
            {
                {
                    () => this.BattleData.Name,
                    (_, __) => this.RaisePropertyChanged(() => this.BattleName)
                },
                {
                    () => this.BattleData.UpdatedTime,
                    (_, __) => this.RaisePropertyChanged(() => this.UpdatedTime)
                },
                {
                    () => this.BattleData.BattleSituation,
                    (_, __) => this.RaisePropertyChanged(() => this.BattleSituation)
                },
                {
                    () => this.BattleData.FriendAirSupremacy,
                    (_, __) => this.RaisePropertyChanged(() => this.FriendAirSupremacy)
                },
                {
                    () => this.BattleData.AirCombatResults,
                    (_, __) =>
                    {
                        this.RaisePropertyChanged(() => this.AirCombatResults);
                        this.FirstFleet.AirCombatResults = this.AirCombatResults.Select(x => new AirCombatResultViewModel(x, FleetType.First)).ToArray();
                        this.SecondFleet.AirCombatResults = this.AirCombatResults.Select(x => new AirCombatResultViewModel(x, FleetType.Second)).ToArray();
                        this.Enemies.AirCombatResults = this.AirCombatResults.Select(x => new AirCombatResultViewModel(x, FleetType.Enemy)).ToArray();
                    }
                },
                {
                    () => this.BattleData.DropShipName,
                    (_, __) => this.RaisePropertyChanged(() => this.DropShipName)
                },
                {
                    () => this.BattleData.FirstFleet,
                    (_, __) => this.FirstFleet.Fleet = this.BattleData.FirstFleet
                },
                {
                    () => this.BattleData.SecondFleet,
                    (_, __) => this.SecondFleet.Fleet = this.BattleData.SecondFleet
                },
                {
                    () => this.BattleData.Enemies,
                    (_, __) => this.Enemies.Fleet = this.BattleData.Enemies
                },
            });
        }

        public void OpenEnemyWindow()
        {
            var message = new TransitionMessage("Show/EnemyWindow")
            {
                TransitionViewModel = new EnemyWindowViewModel()
            };
            this.Messenger.RaiseAsync(message);
        }
    }
}
