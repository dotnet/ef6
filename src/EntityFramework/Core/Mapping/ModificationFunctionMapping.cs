// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;

    /// <summary>
    /// Describes modification function binding for change processing of entities or associations.
    /// </summary>
    public sealed class ModificationFunctionMapping : MappingItem
    {
        private FunctionParameter _rowsAffectedParameter;
        private readonly EdmFunction _function;
        private readonly ReadOnlyCollection<ModificationFunctionParameterBinding> _parameterBindings;
        private readonly ReadOnlyCollection<AssociationSetEnd> _collocatedAssociationSetEnds;
        private readonly ReadOnlyCollection<ModificationFunctionResultBinding> _resultBindings;

        /// <summary>
        /// Initializes a new ModificationFunctionMapping instance.
        /// </summary>
        /// <param name="entitySet">The entity or association set.</param>
        /// <param name="entityType">The entity or association type.</param>
        /// <param name="function">The metadata of function to which we should bind.</param>
        /// <param name="parameterBindings">Bindings for function parameters.</param>
        /// <param name="rowsAffectedParameter">The output parameter producing number of rows affected.</param>
        /// <param name="resultBindings">Bindings for the results of function evaluation</param>
        public ModificationFunctionMapping(
            EntitySetBase entitySet,
            EntityTypeBase entityType,
            EdmFunction function,
            IEnumerable<ModificationFunctionParameterBinding> parameterBindings,
            FunctionParameter rowsAffectedParameter,
            IEnumerable<ModificationFunctionResultBinding> resultBindings)
        {
            Check.NotNull(entitySet, "entitySet");
            Check.NotNull(function, "function");
            Check.NotNull(parameterBindings, "parameterBindings");

            _function = function;
            _rowsAffectedParameter = rowsAffectedParameter;

            _parameterBindings = new ReadOnlyCollection<ModificationFunctionParameterBinding>(parameterBindings.ToList());

            if (null != resultBindings)
            {
                var bindings = resultBindings.ToList();

                if (0 < bindings.Count)
                {
                    _resultBindings = new ReadOnlyCollection<ModificationFunctionResultBinding>(bindings);
                }
            }

            _collocatedAssociationSetEnds =
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

            internal set
            {
                DebugCheck.NotNull(value);
                Debug.Assert(!IsReadOnly);

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
        public ReadOnlyCollection<ModificationFunctionParameterBinding> ParameterBindings
        {
            get { return _parameterBindings; }
        }

        /// <summary>
        /// Gets all association set ends collocated in this mapping.
        /// </summary>
        internal ReadOnlyCollection<AssociationSetEnd> CollocatedAssociationSetEnds
        {
            get { return _collocatedAssociationSetEnds; }
        }

        /// <summary>
        /// Gets bindings for the results of function evaluation.
        /// </summary>
        public ReadOnlyCollection<ModificationFunctionResultBinding> ResultBindings
        {
            get { return _resultBindings; }
        }

        /// <inheritdoc/>
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
