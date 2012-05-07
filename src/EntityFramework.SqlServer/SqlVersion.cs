namespace System.Data.Entity.SqlServer
{
    /// <summary>
    /// This enum describes the current server version
    /// </summary>
    internal enum SqlVersion
    {
        /// <summary>
        /// Sql Server 8
        /// </summary>
        Sql8 = 80,

        /// <summary>
        /// Sql Server 9
        /// </summary>
        Sql9 = 90,

        /// <summary>
        /// Sql Server 10
        /// </summary>
        Sql10 = 100,

        // higher versions go here
    }
}
