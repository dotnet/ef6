// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics.Contracts;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Describes modification function binding for change processing of entities or associations.
    /// </summary>
    internal sealed class StorageModificationFunctionMapping
    {
        internal StorageModificationFunctionMapping(
            EntitySetBase entitySet,
            EntityTypeBase entityType,
            EdmFunction function,
            IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings,
            FunctionParameter rowsAffectedParameter,
            IEnumerable<StorageModificationFunctionResultBinding> resultBindings)
        {
            Contract.Requires(entitySet != null);
            Contract.Requires(function != null);
            Contract.Requires(parameterBindings != null);

            Function = function;
            RowsAffectedParameter = rowsAffectedParameter;
            ParameterBindings = parameterBindings.ToList().AsReadOnly();
            if (null != resultBindings)
            {
                var bindings = resultBindings.ToList();
                if (0 < bindings.Count)
                {
                    ResultBindings = bindings.AsReadOnly();
                }
            }
            CollocatedAssociationSetEnds =
                GetReferencedAssociationSetEnds(entitySet as EntitySet, entityType as EntityType, parameterBindings)
                    .ToList()
                    .AsReadOnly();
        }

        /// <summary>
        /// Gets output parameter producing number of rows affected. May be null.
        /// </summary>
        internal readonly FunctionParameter RowsAffectedParameter;

        /// <summary>
        /// Gets Metadata of function to which we should bind.
        /// </summary>
        internal readonly EdmFunction Function;

        /// <summary>
        /// Gets bindings for function parameters.
        /// </summary>
        internal readonly ReadOnlyCollection<StorageModificationFunctionParameterBinding> ParameterBindings;

        /// <summary>
        /// Gets all association set ends collocated in this mapping.
        /// </summary>
        internal readonly ReadOnlyCollection<AssociationSetEnd> CollocatedAssociationSetEnds;

        /// <summary>
        /// Gets bindings for the results of function evaluation.
        /// </summary>
        internal readonly ReadOnlyCollection<StorageModificationFunctionResultBinding> ResultBindings;

        public override string ToString()
        {
            return String.Format(
                CultureInfo.InvariantCulture,
                "Func{{{0}}}: Prm={{{1}}}, Result={{{2}}}", Function,
                StringUtil.ToCommaSeparatedStringSorted(ParameterBindings),
                StringUtil.ToCommaSeparatedStringSorted(ResultBindings));
        }

        // requires: entitySet must not be null
        // Yields all referenced association set ends in this mapping.
        private static IEnumerable<AssociationSetEnd> GetReferencedAssociationSetEnds(
            EntitySet entitySet, EntityType entityType, IEnumerable<StorageModificationFunctionParameterBinding> parameterBindings)
        {
            var ends = new HashSet<AssociationSetEnd>();
            if (null != entitySet
                && null != entityType)
            {
                foreach (var parameterBinding in parameterBindings)
                {
                    var end = parameterBinding.MemberPath.AssociationSetEnd;
                    if (null != end)
                    {
                        ends.Add(end);
                    }
                }

                // If there is a referential constraint, it counts as an implicit mapping of
                // the association set
                foreach (var assocationSet in MetadataHelper.GetAssociationsForEntitySet(entitySet))
                {
                    var constraints = assocationSet.ElementType.ReferentialConstraints;
                    if (null != constraints)
                    {
                        foreach (var constraint in constraints)
                        {
                            if ((assocationSet.AssociationSetEnds[constraint.ToRole.Name].EntitySet == entitySet)
                                &&
                                (constraint.ToRole.GetEntityType().IsAssignableFrom(entityType)))
                            {
                                ends.Add(assocationSet.AssociationSetEnds[constraint.FromRole.Name]);
                            }
                        }
                    }
                }
            }
            return ends;
        }
    }
}
