// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Describes modification function binding for change processing of entities or associations.
    /// </summary>
    internal sealed class ModificationFunctionMapping
    {
        private FunctionParameter _rowsAffectedParameter;
        private readonly EdmFunction _function;

        internal ModificationFunctionMapping(
            EntitySetBase entitySet,
            EntityTypeBase entityType,
            EdmFunction function,
            IEnumerable<ModificationFunctionParameterBinding> parameterBindings,
            FunctionParameter rowsAffectedParameter,
            IEnumerable<ModificationFunctionResultBinding> resultBindings)
        {
            DebugCheck.NotNull(entitySet);
            DebugCheck.NotNull(function);
            DebugCheck.NotNull(parameterBindings);

            _function = function;
            _rowsAffectedParameter = rowsAffectedParameter;

            ParameterBindings = new ReadOnlyCollection<ModificationFunctionParameterBinding>(parameterBindings.ToList());

            if (null != resultBindings)
            {
                var bindings = resultBindings.ToList();

                if (0 < bindings.Count)
                {
                    ResultBindings = new ReadOnlyCollection<ModificationFunctionResultBinding>(bindings);
                }
            }

            CollocatedAssociationSetEnds =
                new ReadOnlyCollection<AssociationSetEnd>(
                    GetReferencedAssociationSetEnds(entitySet as EntitySet, entityType as EntityType, parameterBindings)
                        .ToList());
        }

        /// <summary>
        /// Gets output parameter producing number of rows affected. May be null.
        /// </summary>
        public FunctionParameter RowsAffectedParameter
        {
            get { return _rowsAffectedParameter; }
            set
            {
                DebugCheck.NotNull(value);

                _rowsAffectedParameter = value;
            }
        }

        internal string RowsAffectedParameterName
        {
            get
            {
                return RowsAffectedParameter != null
                           ? RowsAffectedParameter.Name
                           : null;
            }
        }

        /// <summary>
        /// Gets Metadata of function to which we should bind.
        /// </summary>
        public EdmFunction Function
        {
            get { return _function; }
        }

        /// <summary>
        /// Gets bindings for function parameters.
        /// </summary>
        internal readonly ReadOnlyCollection<ModificationFunctionParameterBinding> ParameterBindings;

        /// <summary>
        /// Gets all association set ends collocated in this mapping.
        /// </summary>
        internal readonly ReadOnlyCollection<AssociationSetEnd> CollocatedAssociationSetEnds;

        /// <summary>
        /// Gets bindings for the results of function evaluation.
        /// </summary>
        internal readonly ReadOnlyCollection<ModificationFunctionResultBinding> ResultBindings;

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
            EntitySet entitySet, EntityType entityType, IEnumerable<ModificationFunctionParameterBinding> parameterBindings)
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
