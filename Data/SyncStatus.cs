namespace DebtManagerApp.Data
{
    /// <summary>
    /// يحدد حالة المزامنة لسجل معين في قاعدة البيانات المحلية.
    /// </summary>
    public enum SyncStatus
    {
        /// <summary>
        /// السجل متزامن مع قاعدة البيانات السحابية.
        /// </summary>
        Synced,

        /// <summary>
        /// السجل تم إنشاؤه محلياً وبحاجة للرفع إلى السحابة.
        /// </summary>
        Created,

        /// <summary>
        /// السجل تم تعديله محلياً وبحاجة للرفع إلى السحابة.
        /// </summary>
        Modified
    }
}
