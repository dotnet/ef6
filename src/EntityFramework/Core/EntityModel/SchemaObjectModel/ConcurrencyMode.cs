namespace System.Data.Entity.Core.Metadata.Edm
{
    /// <summary>
    /// The concurrency mode for properties.
    /// </summary>
    public enum ConcurrencyMode
    {
        /// <summary>
        /// Default concurrency mode: the property is never validated
        /// at write time
        /// </summary>
        None,

        /// <summary>
        /// Fixed concurrency mode: the property is always validated at 
        /// write time
        /// </summary>
        Fixed,
    }
}
