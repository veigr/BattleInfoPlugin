namespace BattleInfoPlugin
{
    /// <summary>
    /// 通知の種類を示す静的メンバーを公開します。
    /// </summary>
    public static class NotificationType
    {
        private static readonly string baseName = typeof(NotificationType).Assembly.GetName().Name;
        /// <summary>
        /// 戦闘終了時の通知を識別するための文字列を取得します。
        /// </summary>
        public static string BattleEnd = $"{baseName}.{nameof(BattleEnd)}";
        /// <summary>
        /// 追撃確認時の通知を識別するための文字列を取得します。
        /// </summary>
        public static string ConfirmPursuit = $"{baseName}.{nameof(ConfirmPursuit)}";
    }
}
