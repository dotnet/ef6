namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     Specifies the action to take on a given operation. <seealso cref = "EdmAssociationEnd.DeleteAction" />
    /// </summary>
    internal enum EdmOperationAction
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