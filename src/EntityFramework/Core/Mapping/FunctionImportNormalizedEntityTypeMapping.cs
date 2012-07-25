// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    internal sealed class FunctionImportNormalizedEntityTypeMapping
    {
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "parent")]
        internal FunctionImportNormalizedEntityTypeMapping(
            FunctionImportStructuralTypeMappingKB parent,
            List<FunctionImportEntityTypeMappingCondition> columnConditions, BitArray impliedEntityTypes)
        {
            // validate arguments
            Contract.Requires(parent != null);
            Contract.Requires(columnConditions != null);
            Contract.Requires(impliedEntityTypes != null);

            Debug.Assert(
                columnConditions.Count == parent.DiscriminatorColumns.Count,
                "discriminator values must be ordinally aligned with discriminator columns");
            Debug.Assert(
                impliedEntityTypes.Count == parent.MappedEntityTypes.Count,
                "implied entity types must be ordinally aligned with mapped entity types");

            ColumnConditions = new ReadOnlyCollection<FunctionImportEntityTypeMappingCondition>(columnConditions.ToList());
            ImpliedEntityTypes = impliedEntityTypes;
            ComplementImpliedEntityTypes = (new BitArray(ImpliedEntityTypes)).Not();
        }

        /// <summary>
        /// Gets discriminator values aligned with DiscriminatorColumns of the parent FunctionImportMapping.
        /// A null ValueCondition indicates 'anything goes'.
        /// </summary>
        internal readonly ReadOnlyCollection<FunctionImportEntityTypeMappingCondition> ColumnConditions;

        /// <summary>
        /// Gets bit array with 'true' indicating the corresponding MappedEntityType of the parent
        /// FunctionImportMapping is implied by this fragment.
        /// </summary>
        internal readonly BitArray ImpliedEntityTypes;

        /// <summary>
        /// Gets the complement of the ImpliedEntityTypes BitArray.
        /// </summary>
        internal readonly BitArray ComplementImpliedEntityTypes;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture, "Values={0}, Types={1}",
                StringUtil.ToCommaSeparatedString(ColumnConditions), StringUtil.ToCommaSeparatedString(ImpliedEntityTypes));
        }
    }
}
