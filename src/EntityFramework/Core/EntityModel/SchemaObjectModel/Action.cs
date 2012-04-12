namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Valid actions in an On&lt;Operation&gt; element
    /// </summary>
    internal enum Action
    {
        /// <summary>
        /// no action
        /// </summary>
        None,

        /// <summary>
        /// Cascade to other ends
        /// </summary>
        Cascade,
    }
}
