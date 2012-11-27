// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServer
{
    using System.Data.Entity.Core;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.SqlServer.Resources;
    using System.Data.Entity.SqlServer.Utilities;

    internal class DbGeographyAdapter : IDbSpatialValue
    {
        private readonly DbGeography _value;

        internal DbGeographyAdapter(DbGeography value)
        {
            DebugCheck.NotNull(value);

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
}
