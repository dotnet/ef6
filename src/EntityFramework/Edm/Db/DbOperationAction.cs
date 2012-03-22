namespace System.Data.Entity.Edm.Db
{
    /// <summary>
    ///     Specifies the action to take on a given operation.
    /// </summary>
    internal enum DbOperationAction
    {
        /// <summary>
        ///     Default behavior
        /// </summary>
        None = 0,

        /// <summary>
        ///     Restrict the operation
        /// </summary>
        Restrict = 1,

        /// <summary>
        ///     Cascade the operation
        /// </summary>
        Cascade = 2,
    }
}
