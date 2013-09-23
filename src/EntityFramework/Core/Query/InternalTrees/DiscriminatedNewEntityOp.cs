// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    // <summary>
    // Polymorphic new instance creation (takes all properties of all types in the hierarchy + discriminator)
    // </summary>
    internal sealed class DiscriminatedNewEntityOp : NewEntityBaseOp
    {
        #region Private state

        private readonly ExplicitDiscriminatorMap m_discriminatorMap;

        #endregion

        #region Constructors

        internal DiscriminatedNewEntityOp(
            TypeUsage type, ExplicitDiscriminatorMap discriminatorMap,
            EntitySet entitySet, List<RelProperty> relProperties)
            : base(OpType.DiscriminatedNewEntity, type, true, entitySet, relProperties)
        {
            DebugCheck.NotNull(discriminatorMap);
            m_discriminatorMap = discriminatorMap;
        }

        private DiscriminatedNewEntityOp()
            : base(OpType.DiscriminatedNewEntity)
        {
        }

        #endregion

        #region "Public" members

        internal static readonly DiscriminatedNewEntityOp Pattern = new DiscriminatedNewEntityOp();

        // <summary>
        // Gets discriminator and type information used in construction of type.
        // </summary>
        internal ExplicitDiscriminatorMap DiscriminatorMap
        {
            get { return m_discriminatorMap; }
        }

        [DebuggerNonUserCode]
        internal override void Accept(BasicOpVisitor v, Node n)
        {
            v.Visit(this, n);
        }

        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType>(BasicOpVisitorOfT<TResultType> v, Node n)
        {
            return v.Visit(this, n);
        }

        #endregion
    }
}
