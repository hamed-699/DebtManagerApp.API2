namespace DebtManagerApp
{
    /// <summary>
    /// يحدد الحالات المختلفة للعميل لأغراض التحليل.
    /// </summary>
    public enum AnalyticsCustomerStatus
    {
        /// <summary>
        /// عميل لديه معاملات حديثة.
        /// </summary>
        Active,

        /// <summary>
        /// عميل لم يقم بأي معاملات لفترة.
        /// </summary>
        Inactive,

        /// <summary>
        /// عميل لديه ديون متأخرة.
        /// </summary>
        Overdue,

        /// <summary>
        /// عميل جديد ليس له سجل معاملات كافٍ.
        /// </summary>
        New
    }
}
