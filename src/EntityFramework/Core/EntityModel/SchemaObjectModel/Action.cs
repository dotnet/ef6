using System;

namespace System.Data.Entity.Core.EntityModel.SchemaObjectModel
{
    /// <summary>
    /// Valid actions in an On&lt;Operation&gt; element
    /// </summary>
    enum Action
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

