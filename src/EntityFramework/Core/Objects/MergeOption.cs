namespace System.Data.Entity.Core.Objects
{
    /// <summary>
    /// 
    /// </summary>
    public enum MergeOption
    {
        /// <summary>
        /// Will only append new (top level-unique) rows.  This is the default behavior.
        /// </summary>
        AppendOnly = 0,

        /// <summary>
        /// Same behavior as LoadOption.OverwriteChanges.
        /// </summary>
        OverwriteChanges = LoadOption.OverwriteChanges,

        /// <summary>
        /// Same behavior as LoadOption.PreserveChanges.
        /// </summary>
        PreserveChanges = LoadOption.PreserveChanges,

        /// <summary>
        /// Will not modify cache.
        /// </summary>
        NoTracking = 3,
    }
}
