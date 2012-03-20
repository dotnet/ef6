namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Data.Entity.ModelConfiguration.Utilities;
    using System.Data.Entity.Resources;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;

    internal sealed class PropertyFilter
    {
        private const BindingFlags DefaultBindingFlags
            = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Instance;

        private readonly double? _edmModelVersion;

        public PropertyFilter(double? edmModelVersion = null)
        {
            _edmModelVersion = edmModelVersion;
        }

        public IEnumerable<PropertyInfo> GetProperties(
            Type type,
            bool declaredOnly,
            IEnumerable<PropertyInfo> explicitlyMappedProperties = null,
            IEnumerable<Type> knownTypes = null)
        {
            Contract.Requires(type != null);

            explicitlyMappedProperties = explicitlyMappedProperties ?? Enumerable.Empty<PropertyInfo>();
            knownTypes = knownTypes ?? Enumerable.Empty<Type>();

            ValidatePropertiesForModelVersion(type, explicitlyMappedProperties);

            var bindingFlags
                = declaredOnly
                      ? DefaultBindingFlags | BindingFlags.DeclaredOnly
                      : DefaultBindingFlags;

            var propertyInfos
                = from p in type.GetProperties(bindingFlags)
                  where p.IsValidStructuralProperty()
                  let m = p.GetGetMethod(true)
                  where (m.IsPublic || explicitlyMappedProperties.Contains(p) || knownTypes.Contains(p.PropertyType))
                        && (!declaredOnly || !type.BaseType.GetProperties(DefaultBindingFlags).Any(bp => bp.Name == p.Name))
                        && (EdmV3FeaturesSupported || !IsEnumType(p.PropertyType)
                            && (EdmV3FeaturesSupported || !IsSpatialType(p.PropertyType)))
                  select p;

            return propertyInfos;
        }

        public void ValidatePropertiesForModelVersion(Type type, IEnumerable<PropertyInfo> explicitlyMappedProperties)
        {
            if (_edmModelVersion == null)
            {
                return;
            }

            if (!EdmV3FeaturesSupported)
            {
                var firstBadProperty =
                    explicitlyMappedProperties.FirstOrDefault(p => IsEnumType(p.PropertyType) || IsSpatialType(p.PropertyType));
                if (firstBadProperty != null)
                {
                    throw Error.UnsupportedUseOfV3Type(type.Name, firstBadProperty.Name);
                }
            }
        }

        public bool EdmV3FeaturesSupported
        {
            get { return _edmModelVersion == null || _edmModelVersion >= DataModelVersions.Version3; }
        }

        private static bool IsEnumType(Type type)
        {
            type.TryUnwrapNullableType(out type);

            return type.IsEnum;
        }

        private static bool IsSpatialType(Type type)
        {
            type.TryUnwrapNullableType(out type);

            return type == typeof(System.Data.Spatial.DbGeometry) || type == typeof(System.Data.Spatial.DbGeography);
        }
    }
}