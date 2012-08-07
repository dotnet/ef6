// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Objects.DataClasses;

    /// <summary>
    ///     Implementor of IEntityKeyStrategy for entities that implement IEntityWithKey.  Getting and setting
    ///     the key is deferred to the entity itself.
    /// </summary>
    internal sealed class EntityWithKeyStrategy : IEntityKeyStrategy
    {
        private readonly IEntityWithKey _entity;

        /// <summary>
        ///     Creates a strategy object for the given entity.  Keys will be stored in the entity.
        /// </summary>
        /// <param name="entity"> The entity to use </param>
        public EntityWithKeyStrategy(IEntityWithKey entity)
        {
            _entity = entity;
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKey()
        {
            return _entity.EntityKey;
        }

        // See IEntityKeyStrategy
        public void SetEntityKey(EntityKey key)
        {
            _entity.EntityKey = key;
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKeyFromEntity()
        {
            return _entity.EntityKey;
        }
    }
}
