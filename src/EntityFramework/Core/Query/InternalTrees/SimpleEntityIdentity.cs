// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Globalization;
    using System.Text;

    // <summary>
    // This class is a "simple" representation of the entity identity, where the
    // entityset containing the entity is known a priori. This may be because
    // there is exactly one entityset for the entity; or because it is inferrable
    // from the query that only one entityset is relevant here
    // </summary>
    internal class SimpleEntityIdentity : EntityIdentity
    {
        private readonly EntitySet m_entitySet; // the entity set

        // <summary>
        // Basic constructor.
        // Note: the entitySet may be null - in which case, we are referring to
        // a transient entity
        // </summary>
        // <param name="entitySet"> The entityset </param>
        // <param name="keyColumns"> key columns of the entity </param>
        internal SimpleEntityIdentity(EntitySet entitySet, SimpleColumnMap[] keyColumns)
            : base(keyColumns)
        {
            // the entityset may be null
            m_entitySet = entitySet;
        }

        // <summary>
        // The entityset containing the entity
        // </summary>
        internal EntitySet EntitySet
        {
            get { return m_entitySet; }
        }

        // <summary>
        // Debugging support
        // </summary>
        public override string ToString()
        {
            var sb = new StringBuilder();
            var separator = String.Empty;
            sb.AppendFormat(CultureInfo.InvariantCulture, "[(ES={0}) (Keys={", EntitySet.Name);
            foreach (var c in Keys)
            {
                sb.AppendFormat(CultureInfo.InvariantCulture, "{0}{1}", separator, c);
                separator = ",";
            }
            sb.AppendFormat(CultureInfo.InvariantCulture, "})]");
            return sb.ToString();
        }
    }
}
