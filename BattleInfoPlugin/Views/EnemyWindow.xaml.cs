using System;
using System.Windows;
using MetroRadiance.UI.Controls;
using BattleInfoPlugin.ViewModels;

namespace BattleInfoPlugin.Views
{
    /* 
     * ViewModelからの変更通知などの各種イベントを受け取る場合は、PropertyChangedWeakEventListenerや
     * CollectionChangedWeakEventListenerを使うと便利です。独自イベントの場合はLivetWeakEventListenerが使用できます。
     * クローズ時などに、LivetCompositeDisposableに格納した各種イベントリスナをDisposeする事でイベントハンドラの開放が容易に行えます。
     *
     * WeakEventListenerなので明示的に開放せずともメモリリークは起こしませんが、できる限り明示的に開放するようにしましょう。
     */

    /// <summary>
    /// EnemyWindow.xaml の相互作用ロジック
    /// </summary>
    public partial class EnemyWindow : MetroWindow
    {
        public EnemyWindow()
        {
            this.InitializeComponent();
            WeakEventManager<Window, EventArgs>.AddHandler(
                Application.Current.MainWindow,
                "Closed",
                (_, __) => this.Close());
        }
        private void Window_Drop(object sender, DragEventArgs e)
        {
            if (!e.Data.GetDataPresent(DataFormats.FileDrop, true))
                return;

            if (MessageBoxResult.OK != MessageBox.Show("ドロップしたファイルをマージしますか？", "確認", MessageBoxButton.OKCancel, MessageBoxImage.Question))
                return;

            var filePathList = ((string[])e.Data.GetData(DataFormats.FileDrop, true));
            var vm = this.DataContext as EnemyWindowViewModel;
            if (vm == null) return;
            vm.Merge(filePathList);
        }
    }
}
