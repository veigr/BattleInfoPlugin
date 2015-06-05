using System;
using System.ComponentModel.Composition;
using BattleInfoPlugin.ViewModels;
using BattleInfoPlugin.Views;
using Grabacr07.KanColleViewer.Composition;
using Grabacr07.KanColleWrapper;
using Grabacr07.KanColleWrapper.Models.Raw;

namespace BattleInfoPlugin
{
    [Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "BattleInfo")]
    [ExportMetadata("Description", "戦闘情報を表示します。")]
    [ExportMetadata("Version", "1.0.0")]
    [ExportMetadata("Author", "@veigr")]
    public class Plugin : IToolPlugin
    {
        private readonly ToolViewModel vm = new ToolViewModel();
        internal static readonly KcsResourceWriter ResourceWriter = new KcsResourceWriter();
        internal static readonly SortieDataListener SortieListener = new SortieDataListener();
        internal static kcsapi_start2 RawStart2 { get; private set; }

        public Plugin()
        {
            KanColleClient.Current.Proxy.api_start2.TryParse<kcsapi_start2>().Subscribe(x =>
            {
                RawStart2 = x.Data;
                Models.Repositories.Master.Current.Update(x.Data);
            });
        }

        public object GetToolView()
        {
            return new ToolView { DataContext = this.vm };
        }

        public string ToolName
        {
            get { return "BattleInfo"; }
        }

        public object GetSettingsView()
        {
            return null;
        }
    }
}
