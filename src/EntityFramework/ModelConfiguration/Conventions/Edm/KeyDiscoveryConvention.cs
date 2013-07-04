// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Data.Entity.Utilities;

    /// <summary>
    ///     Base class for conventions that discover primary key properties.
    /// </summary>
    public abstract class KeyDiscoveryConvention : IModelConvention<EntityType>
    {
        /// <inheritdoc/>
        public virtual void Apply(EntityType item, DbModel model)
        {
            Check.NotNull(item, "item");
            Check.NotNull(model, "model");

            if ((item.KeyProperties.Count > 0)
                || (item.BaseType != null))
            {
                return;
            }

            var keyProperties = MatchKeyProperty(item, item.GetDeclaredPrimitiveProperties());

            foreach (var keyProperty in keyProperties)
            {
                keyProperty.Nullable = false;
                item.AddKeyMember(keyProperty);
            }
        }

        /// <summary>
        ///     When overriden returns the subset of properties that will be part of the primary key.
        /// </summary>
        /// <param name="entityType"> The entity type. </param>
        /// <param name="primitiveProperties"> The primitive types of the entities</param>
        /// <returns> The properties that should be part of the primary key. </returns>
        protected abstract IEnumerable<EdmProperty> MatchKeyProperty(
            EntityType entityType, IEnumerable<EdmProperty> primitiveProperties);
    }
}
