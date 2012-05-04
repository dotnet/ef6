namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;
    using System.Diagnostics.Contracts;

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

    internal static class IDbSpatialValueExtensionMethods
    {
        /// <summary>
        /// Returns an instance of <see cref="IDbSpatialValue"/> that wraps the specified <see cref="DbGeography"/> value.
        /// IDbSpatialValue members are guaranteed not to throw the <see cref="NotImplementedException"/>s caused by unimplemented members of their wrapped values.
        /// </summary>
        /// <param name="geographyValue">The geography instance to wrap</param>
        /// <returns>An instance of <see cref="IDbSpatialValue"/> that wraps the specified geography value</returns>
        internal static IDbSpatialValue AsSpatialValue(this DbGeography geographyValue)
        {
            Contract.Requires(geographyValue != null);

            return new DbGeographyAdapter(geographyValue);
        }

        /// <summary>
        /// Returns an instance of <see cref="IDbSpatialValue"/> that wraps the specified <see cref="DbGeometry"/> value.
        /// IDbSpatialValue members are guaranteed not to throw the <see cref="NotImplementedException"/>s caused by unimplemented members of their wrapped values.
        /// </summary>
        /// <param name="geometryValue">The geometry instance to wrap</param>
        /// <returns>An instance of <see cref="IDbSpatialValue"/> that wraps the specified geometry value</returns>
        internal static IDbSpatialValue AsSpatialValue(this DbGeometry geometryValue)
        {
            Contract.Requires(geometryValue != null);

            return new DbGeometryAdapter(geometryValue);
        }
    }

    internal class DbGeographyAdapter : IDbSpatialValue
    {
        private readonly DbGeography _value;

        internal DbGeographyAdapter(DbGeography value)
        {
            Contract.Requires(value != null);

            _value = value;
        }

        public bool IsGeography
        {
            get { return true; }
        }

        public object ProviderValue
        {
            get { return FuncExtensions.NullIfNotImplemented(() => _value.ProviderValue); }
        }

        public int? CoordinateSystemId
        {
            get { return FuncExtensions.NullIfNotImplemented<int?>(() => _value.CoordinateSystemId); }
        }

        public string WellKnownText
        {
            get
            {
                return FuncExtensions.NullIfNotImplemented(() => _value.Provider.AsTextIncludingElevationAndMeasure(_value))
                       ?? FuncExtensions.NullIfNotImplemented(() => _value.AsText());
            }
        }

        public byte[] WellKnownBinary
        {
            get { return FuncExtensions.NullIfNotImplemented(() => _value.AsBinary()); }
        }

        public string GmlString
        {
            get { return FuncExtensions.NullIfNotImplemented(() => _value.AsGml()); }
        }

        public Exception NotSqlCompatible()
        {
            return new ProviderIncompatibleException(Strings.SqlProvider_GeographyValueNotSqlCompatible);
        }
    }

    internal class DbGeometryAdapter : IDbSpatialValue
    {
        private readonly DbGeometry _value;

        internal DbGeometryAdapter(DbGeometry value)
        {
            Contract.Requires(value != null);

            _value = value;
        }

        public bool IsGeography
        {
            get { return false; }
        }

        public object ProviderValue
        {
            get { return FuncExtensions.NullIfNotImplemented(() => _value.ProviderValue); }
        }

        public int? CoordinateSystemId
        {
            get { return FuncExtensions.NullIfNotImplemented<int?>(() => _value.CoordinateSystemId); }
        }

        public string WellKnownText
        {
            get
            {
                return FuncExtensions.NullIfNotImplemented(() => _value.Provider.AsTextIncludingElevationAndMeasure(_value))
                       ?? FuncExtensions.NullIfNotImplemented(() => _value.AsText());
            }
        }

        public byte[] WellKnownBinary
        {
            get { return FuncExtensions.NullIfNotImplemented(() => _value.AsBinary()); }
        }

        public string GmlString
        {
            get { return FuncExtensions.NullIfNotImplemented(() => _value.AsGml()); }
        }

        public Exception NotSqlCompatible()
        {
            return new ProviderIncompatibleException(Strings.SqlProvider_GeometryValueNotSqlCompatible);
        }
    }
}
