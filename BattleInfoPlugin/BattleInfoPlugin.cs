using System.ComponentModel.Composition;
using BattleInfoPlugin.Models;
using Grabacr07.KanColleViewer.Composition;

namespace BattleInfoPlugin
{
    [Export(typeof(IToolPlugin))]
    [ExportMetadata("Title", "BattleInfo")]
    [ExportMetadata("Description", "戦闘情報を表示します。")]
    [ExportMetadata("Version", "1.0.0")]
    [ExportMetadata("Author", "@veigr")]
    public class BattleInfoPlugin : IToolPlugin
    {
        private readonly ToolViewModel vm = new ToolViewModel(new BattleData(), new BattleEndNotifier());

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
