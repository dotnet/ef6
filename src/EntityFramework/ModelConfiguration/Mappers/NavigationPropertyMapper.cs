// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Reflection;

    /// <summary>
    ///     Handles mapping from a CLR property to an EDM assocation and nav. prop.
    /// </summary>
    internal sealed class NavigationPropertyMapper
    {
        private readonly TypeMapper _typeMapper;

        public NavigationPropertyMapper(TypeMapper typeMapper)
        {
            DebugCheck.NotNull(typeMapper);

            _typeMapper = typeMapper;
        }

        public void Map(
            PropertyInfo propertyInfo, EntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
        {
            DebugCheck.NotNull(propertyInfo);
            DebugCheck.NotNull(entityType);
            DebugCheck.NotNull(entityTypeConfiguration);

            var targetType = propertyInfo.PropertyType;
            var targetAssociationEndKind = RelationshipMultiplicity.ZeroOrOne;

            if (targetType.IsCollection(out targetType))
            {
                targetAssociationEndKind = RelationshipMultiplicity.Many;
            }

            var targetEntityType = _typeMapper.MapEntityType(targetType);

            if (targetEntityType != null)
            {
                var sourceAssociationEndKind
                    = targetAssociationEndKind.IsMany()
                          ? RelationshipMultiplicity.ZeroOrOne
                          : RelationshipMultiplicity.Many;

                var associationType
                    = _typeMapper.MappingContext.Model.AddAssociationType(
                        entityType.Name + "_" + propertyInfo.Name,
                        entityType,
                        sourceAssociationEndKind,
                        targetEntityType,
                        targetAssociationEndKind,
                        _typeMapper.MappingContext.ModelConfiguration.ModelNamespace);

                associationType.SourceEnd.SetClrPropertyInfo(propertyInfo);

                _typeMapper.MappingContext.Model.AddAssociationSet(associationType.Name, associationType);

                var navigationProperty
                    = entityType.AddNavigationProperty(propertyInfo.Name, associationType);

                navigationProperty.SetClrPropertyInfo(propertyInfo);

                _typeMapper.MappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(
                    propertyInfo,
                    () => entityTypeConfiguration().Navigation(propertyInfo),
                    _typeMapper.MappingContext.ModelConfiguration);

                new AttributeMapper(_typeMapper.MappingContext.AttributeProvider)
                    .Map(propertyInfo, navigationProperty.Annotations);
            }
        }
    }
}
