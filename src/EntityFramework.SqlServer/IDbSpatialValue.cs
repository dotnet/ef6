namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Spatial;

    /// <summary>
    /// Adapter interface to make working with instances of <see cref="DbGeometry"/> or <see cref="DbGeography"/> easier.  
    /// Implementing types wrap instances of DbGeography/DbGeometry and allow them to be consumed in a common way. 
    /// This interface is implemented by wrapping types for two reasons:
    /// 1. The DbGeography/DbGeometry classes cannot directly implement internal interfaces because their members are virtual (behavior is not guaranteed).
    /// 2. The wrapping types ensure that instances of IDbSpatialValue handle the <see cref="NotImplementedException"/>s thrown
    ///    by any unimplemented members of derived DbGeography/DbGeometry types that correspond to the properties and methods declared in the interface.
    /// </summary>
    internal interface IDbSpatialValue
    {
        bool IsGeography { get; }
        object ProviderValue { get; }
        int? CoordinateSystemId { get; }
        string WellKnownText { get; }
        byte[] WellKnownBinary { get; }
        string GmlString { get; }

        Exception NotSqlCompatible();
    }
}
