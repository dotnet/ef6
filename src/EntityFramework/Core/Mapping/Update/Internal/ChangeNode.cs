// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.Update.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Globalization;
    using System.Text;

    /// <summary>
    ///     This class encapsulates changes propagated to a node in an update mapping view.
    ///     It contains lists of deleted and inserted rows. Key intersections betweens rows
    ///     in the two sets are treated as updates in the store.
    /// </summary>
    /// <remarks>
    ///     <para> Additional tags indicating the roles of particular values (e.g., concurrency, undefined, etc.) are stored within each row: where appropriate, constants appearing within a row are associated with a <see
    ///      cref="PropagatorResult" /> through the <see cref="UpdateTranslator" /> . </para>
    ///     <para> The 'leaves' of an update mapping view (UMV) are extent expressions. A change node associated with an extent expression is simply the list of changes to the C-Space requested by a caller. As changes propagate 'up' the UMV expression tree, we recursively apply transformations such that the change node associated with the root of the UMV represents changes to apply in the S-Space. </para>
    /// </remarks>
    internal class ChangeNode
    {
        #region Constructors

        /// <summary>
        ///     Constructs a change node containing changes belonging to the specified collection
        ///     schema definition.
        /// </summary>
        /// <param name="elementType"> Sets <see cref="ElementType" /> property. </param>
        internal ChangeNode(TypeUsage elementType)
        {
            m_elementType = elementType;
        }

        #endregion

        #region Fields

        private readonly TypeUsage m_elementType;
        private readonly List<PropagatorResult> m_inserted = new List<PropagatorResult>();
        private readonly List<PropagatorResult> m_deleted = new List<PropagatorResult>();

        #endregion

        #region Properties

        /// <summary>
        ///     Gets the type of the rows contained in this node. This type corresponds (not coincidentally)
        ///     to the type of an expression in an update mapping view.
        /// </summary>
        internal TypeUsage ElementType
        {
            get { return m_elementType; }
        }

        /// <summary>
        ///     Gets a list of rows to be inserted.
        /// </summary>
        internal List<PropagatorResult> Inserted
        {
            get { return m_inserted; }
        }

        /// <summary>
        ///     Gets a list of rows to be deleted.
        /// </summary>
        internal List<PropagatorResult> Deleted
        {
            get { return m_deleted; }
        }

        /// <summary>
        ///     Gets or sets a version of a record at this node with default record. The record has the type 
        ///     of the node we are visiting.
        /// </summary>
        internal PropagatorResult Placeholder { get; set; }

        #endregion

#if DEBUG
        public override string ToString()
        {
            var builder = new StringBuilder();

            builder.AppendLine("{");
            builder.AppendFormat(CultureInfo.InvariantCulture, "    ElementType = {0}", ElementType).AppendLine();
            builder.AppendLine("    Inserted = {");
            foreach (var insert in Inserted)
            {
                builder.Append("        ").AppendLine(insert.ToString());
            }
            builder.AppendLine("    }");
            builder.AppendLine("    Deleted = {");
            foreach (var delete in Deleted)
            {
                builder.Append("        ").AppendLine(delete.ToString());
            }
            builder.AppendLine("    }");
            builder.AppendFormat(CultureInfo.InvariantCulture, "    PlaceHolder = {0}", Placeholder).AppendLine();

            builder.Append("}");
            return builder.ToString();
        }
#endif
    }
}
