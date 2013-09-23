// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;

    // <summary>
    // Represents a column map for a specific entity type
    // </summary>
    internal class EntityColumnMap : TypedColumnMap
    {
        private readonly EntityIdentity m_entityIdentity;

        // <summary>
        // constructor
        // </summary>
        // <param name="type"> column datatype </param>
        // <param name="name"> column name </param>
        // <param name="properties"> list of properties </param>
        // <param name="entityIdentity"> entity identity information </param>
        internal EntityColumnMap(TypeUsage type, string name, ColumnMap[] properties, EntityIdentity entityIdentity)
            : base(type, name, properties)
        {
            DebugCheck.NotNull(entityIdentity);
            m_entityIdentity = entityIdentity;
        }

        // <summary>
        // Get the entity identity information
        // </summary>
        internal EntityIdentity EntityIdentity
        {
            get { return m_entityIdentity; }
        }

        // <summary>
        // Visitor Design Pattern
        // </summary>
        [DebuggerNonUserCode]
        internal override void Accept<TArgType>(ColumnMapVisitor<TArgType> visitor, TArgType arg)
        {
            visitor.Visit(this, arg);
        }

        // <summary>
        // Visitor Design Pattern
        // </summary>
        [DebuggerNonUserCode]
        internal override TResultType Accept<TResultType, TArgType>(
            ColumnMapVisitorWithResults<TResultType, TArgType> visitor, TArgType arg)
        {
            return visitor.Visit(this, arg);
        }

        // <summary>
        // Debugging support
        // </summary>
        public override string ToString()
        {
            var str = String.Format(CultureInfo.InvariantCulture, "E{0}", base.ToString());
            return str;
        }
    }
}
