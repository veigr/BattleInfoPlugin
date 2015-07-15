using System;
using System.ComponentModel.Composition;
using BattleInfoPlugin.ViewModels;
using BattleInfoPlugin.Views;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace BattleInfoPlugin
{
    [Export(typeof(IPlugin))]
    [Export(typeof(ITool))]
    [Export(typeof(IRequestNotify))]
    [ExportMetadata("Guid", "55F1599E-5FAD-4696-A972-BF8C4B3C1B76")]
    [ExportMetadata("Title", "BattleInfo")]
    [ExportMetadata("Description", "戦闘情報を表示します。")]
    [ExportMetadata("Version", "1.0.0")]
    [ExportMetadata("Author", "@veigr")]
    public class Plugin : IPlugin, ITool, IRequestNotify
    {
        private readonly ToolViewModel vm;
        internal static readonly KcsResourceWriter ResourceWriter = new KcsResourceWriter();
        internal static readonly SortieDataListener SortieListener = new SortieDataListener();
        internal static kcsapi_start2 RawStart2 { get; private set; }

        public Plugin()
        {
            this.vm = new ToolViewModel(this);
            KanColleClient.Current.Proxy.api_start2.TryParse<kcsapi_start2>().Subscribe(x =>
            {
                RawStart2 = x.Data;
                Models.Repositories.Master.Current.Update(x.Data);
            });
        }

        public void Initialize() {}

        public string Name => "BattleInfo";

        public object View => new ToolView {DataContext = this.vm};

        public event EventHandler<NotifyEventArgs> NotifyRequested;

        public void InvokeNotifyRequested(NotifyEventArgs e) => this.NotifyRequested?.Invoke(this, e);
    }
}
