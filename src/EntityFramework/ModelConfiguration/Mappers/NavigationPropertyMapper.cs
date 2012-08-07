// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Mappers
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Reflection;

    /// <summary>
    ///     Handles mapping from a CLR property to an EDM assocation and nav. prop.
    /// </summary>
    internal sealed class NavigationPropertyMapper
    {
        private readonly TypeMapper _typeMapper;

        public NavigationPropertyMapper(TypeMapper typeMapper)
        {
            Contract.Requires(typeMapper != null);

            _typeMapper = typeMapper;
        }

        public void Map(
            PropertyInfo propertyInfo, EdmEntityType entityType, Func<EntityTypeConfiguration> entityTypeConfiguration)
        {
            Contract.Requires(propertyInfo != null);
            Contract.Requires(entityType != null);
            Contract.Requires(entityTypeConfiguration != null);

            var targetType = propertyInfo.PropertyType;
            var targetAssociationEndKind = EdmAssociationEndKind.Optional;

            if (targetType.IsCollection(out targetType))
            {
                targetAssociationEndKind = EdmAssociationEndKind.Many;
            }

            var targetEntityType = _typeMapper.MapEntityType(targetType);

            if (targetEntityType != null)
            {
                var sourceAssociationEndKind
                    = targetAssociationEndKind.IsMany()
                          ? EdmAssociationEndKind.Optional
                          : EdmAssociationEndKind.Many;

                var associationType = _typeMapper.MappingContext.Model.AddAssociationType(
                    entityType.Name + "_" + propertyInfo.Name,
                    entityType,
                    sourceAssociationEndKind,
                    targetEntityType,
                    targetAssociationEndKind);

                _typeMapper.MappingContext.Model.AddAssociationSet(associationType.Name, associationType);

                var navigationProperty
                    = entityType.AddNavigationProperty(propertyInfo.Name, associationType);

                navigationProperty.SetClrPropertyInfo(propertyInfo);

                _typeMapper.MappingContext.ConventionsConfiguration.ApplyPropertyConfiguration(
                    propertyInfo,
                    () => entityTypeConfiguration().Navigation(propertyInfo));

                new AttributeMapper(_typeMapper.MappingContext.AttributeProvider)
                    .Map(propertyInfo, navigationProperty.Annotations);
            }
        }
    }
}
