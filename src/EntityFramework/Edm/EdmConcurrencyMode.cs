namespace System.Data.Entity.Edm
{
    /// <summary>
    ///     Concurrency mode for properties.
    /// </summary>
    internal enum EdmConcurrencyMode
    {
        /// <summary>
        ///     Default concurrency mode: the property is never validated at write time
        /// </summary>
        None = 0,

        /// <summary>
        ///     Fixed concurrency mode: the property is always validated at write time
        /// </summary>
        Fixed = 1,
    }
}
