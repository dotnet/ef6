// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects;

    /// <summary>
    /// This is the interface to a particular entry in an IEntityStateManager.  It provides
    /// information about the state of the entity in question and the ability to modify that state
    /// as appropriate for an entity adapter to function in performing updates to a backing store.
    /// </summary>
    internal interface IEntityStateEntry
    {
        IEntityStateManager StateManager { get; }
        EntityKey EntityKey { get; }
        EntitySetBase EntitySet { get; }
        bool IsRelationship { get; }
        bool IsKeyEntry { get; }
        EntityState State { get; }
        DbDataRecord OriginalValues { get; }
        CurrentValueRecord CurrentValues { get; }
        BitArray ModifiedProperties { get; }

        void AcceptChanges();
        void Delete();
        void SetModified();
        void SetModifiedProperty(string propertyName);
        IEnumerable<string> GetModifiedProperties();
    }
}
