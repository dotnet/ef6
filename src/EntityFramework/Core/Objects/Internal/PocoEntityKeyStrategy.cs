// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    // <summary>
    // Implementor of IEntityKeyStrategy for getting and setting a key on an entity that does not
    // implement IEntityWithKey.  The key is stored in the strategy object.
    // </summary>
    internal sealed class PocoEntityKeyStrategy : IEntityKeyStrategy
    {
        private EntityKey _key;

        // See IEntityKeyStrategy
        public EntityKey GetEntityKey()
        {
            return _key;
        }

        // See IEntityKeyStrategy
        public void SetEntityKey(EntityKey key)
        {
            _key = key;
        }

        // See IEntityKeyStrategy
        public EntityKey GetEntityKeyFromEntity()
        {
            return null;
        }
    }
}
