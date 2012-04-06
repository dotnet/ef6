using System;
using System.Collections.Generic;
using System.Text;

namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// Represents the list of possible actions for delete operation
    /// </summary>
    public enum OperationAction
    {
        /// <summary>
        /// no action
        /// </summary>
        None,

        /// <summary>
        /// Cascade to other ends
        /// </summary>
        Cascade,

        /// <summary>
        /// Do not allow if other ends are not empty 
        /// </summary>
        Restrict,
    }
}
