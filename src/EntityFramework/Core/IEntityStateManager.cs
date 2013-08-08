// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Interface allowing an IEntityAdapter to analyze state/change tracking information maintained
    /// by a state manager in order to perform updates on a backing store (and push back the results
    /// of those updates).
    /// </summary>
    internal interface IEntityStateManager
    {
        IEnumerable<IEntityStateEntry> GetEntityStateEntries(EntityState state);
        IEnumerable<IEntityStateEntry> FindRelationshipsByKey(EntityKey key);
        IEntityStateEntry GetEntityStateEntry(EntityKey key);
        bool TryGetEntityStateEntry(EntityKey key, out IEntityStateEntry stateEntry);
        bool TryGetReferenceKey(EntityKey dependentKey, AssociationEndMember principalRole, out EntityKey principalKey);
    }
}
