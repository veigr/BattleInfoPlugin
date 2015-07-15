using System;
using System.Reactive.Linq;
using System.Windows;
using BattleInfoPlugin.Properties;
using Grabacr07.KanColleViewer;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Livet;

namespace BattleInfoPlugin.Models.Notifiers
{
    public class BattleEndNotifier: NotificationObject
    {
        private static readonly Settings settings = Settings.Default;

        private readonly Plugin plugin;

        #region IsEnabled変更通知プロパティ
        public bool IsEnabled
        {
            get
            { return settings.IsEnabledBattleEndNotify; }
            set
            {
                if (settings.IsEnabledBattleEndNotify == value)
                    return;
                settings.IsEnabledBattleEndNotify = value;
                settings.Save();
                this.RaisePropertyChanged();
            }
        }
        #endregion


        public BattleEndNotifier(Plugin plugin)
        {
            this.plugin = plugin;

            settings.Reload();

            var proxy = KanColleClient.Current.Proxy;
            proxy.api_req_combined_battle_battleresult
                .Subscribe(_ => this.Notify());
            proxy.ApiSessionSource.Where(x => x.Request.PathAndQuery == "/kcsapi/api_req_practice/battle_result")
                .Subscribe(_ => this.Notify());
            proxy.api_req_sortie_battleresult
                .Subscribe(_ => this.Notify());
        }

        private void Notify()
        {
            var isActive = DispatcherHelper.UIDispatcher.Invoke(() => Application.Current.MainWindow.IsActive);
            if (this.IsEnabled && !isActive)
                this.plugin.InvokeNotifyRequested(new NotifyEventArgs("戦闘終了", "戦闘が終了しました。"));
        }
    }
}
