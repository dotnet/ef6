// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    internal struct EntitySetQualifiedType : IEqualityComparer<EntitySetQualifiedType>
    {
        internal static readonly IEqualityComparer<EntitySetQualifiedType> EqualityComparer = new EntitySetQualifiedType();

        internal readonly Type ClrType;
        internal readonly EntitySet EntitySet;

        internal EntitySetQualifiedType(Type type, EntitySet set)
        {
            DebugCheck.NotNull(type);
            DebugCheck.NotNull(set);
            DebugCheck.NotNull(set.EntityContainer);
            DebugCheck.NotNull(set.EntityContainer.Name);
            ClrType = EntityUtil.GetEntityIdentityType(type);
            EntitySet = set;
        }

        public bool Equals(EntitySetQualifiedType x, EntitySetQualifiedType y)
        {
            return (ReferenceEquals(x.ClrType, y.ClrType) &&
                    ReferenceEquals(x.EntitySet, y.EntitySet));
        }

        [SuppressMessage("Microsoft.Usage", "CA2303", Justification = "ClrType is not expected to be an Embedded Interop Type.")]
        public int GetHashCode(EntitySetQualifiedType obj)
        {
            return unchecked(obj.ClrType.GetHashCode() +
                             obj.EntitySet.Name.GetHashCode() +
                             obj.EntitySet.EntityContainer.Name.GetHashCode());
        }
    }
}
