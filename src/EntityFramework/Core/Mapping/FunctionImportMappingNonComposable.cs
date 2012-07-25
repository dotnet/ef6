// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using OM = System.Collections.ObjectModel;

    /// <summary>
    /// Represents a mapping from a model function import to a store non-composable function.
    /// </summary>
    internal sealed class FunctionImportMappingNonComposable : FunctionImportMapping
    {
        internal FunctionImportMappingNonComposable(
            EdmFunction functionImport,
            EdmFunction targetFunction,
            List<List<FunctionImportStructuralTypeMapping>> structuralTypeMappingsList,
            ItemCollection itemCollection)
            : base(functionImport, targetFunction)
        {
            Contract.Requires(structuralTypeMappingsList != null);
            Contract.Requires(itemCollection != null);
            Debug.Assert(!functionImport.IsComposableAttribute, "!functionImport.IsComposableAttribute");
            Debug.Assert(!targetFunction.IsComposableAttribute, "!targetFunction.IsComposableAttribute");

            if (structuralTypeMappingsList.Count == 0)
            {
                ResultMappings = new OM.ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(
                    new[]
                        {
                            new FunctionImportStructuralTypeMappingKB(new List<FunctionImportStructuralTypeMapping>(), itemCollection)
                        });
                noExplicitResultMappings = true;
            }
            else
            {
                Debug.Assert(functionImport.ReturnParameters.Count == structuralTypeMappingsList.Count);
                ResultMappings = new OM.ReadOnlyCollection<FunctionImportStructuralTypeMappingKB>(
                    structuralTypeMappingsList
                        .Select(
                            (structuralTypeMappings) => new FunctionImportStructuralTypeMappingKB(
                                                            structuralTypeMappings,
                                                            itemCollection))
                        .ToArray());
                noExplicitResultMappings = false;
            }
        }

        private readonly bool noExplicitResultMappings;

        /// <summary>
        /// Gets function import return type mapping knowledge bases.
        /// </summary>
        internal readonly OM.ReadOnlyCollection<FunctionImportStructuralTypeMappingKB> ResultMappings;

        /// <summary>
        /// If no return mappings were specified in the MSL return an empty return type mapping knowledge base.
        /// Otherwise return the resultSetIndexth return type mapping knowledge base, or throw if resultSetIndex is out of range
        /// </summary>
        internal FunctionImportStructuralTypeMappingKB GetResultMapping(int resultSetIndex)
        {
            Debug.Assert(resultSetIndex >= 0, "resultSetIndex >= 0");
            if (noExplicitResultMappings)
            {
                Debug.Assert(ResultMappings.Count == 1, "this.ResultMappings.Count == 1");
                return ResultMappings[0];
            }
            else
            {
                if (ResultMappings.Count <= resultSetIndex)
                {
                    throw new ArgumentOutOfRangeException("resultSetIndex");
                }
                return ResultMappings[resultSetIndex];
            }
        }

        /// <summary>
        /// Gets the disctriminator columns resultSetIndexth result set, or an empty array if the index is not in range
        /// </summary>
        internal IList<string> GetDiscriminatorColumns(int resultSetIndex)
        {
            var resultMapping = GetResultMapping(resultSetIndex);
            return resultMapping.DiscriminatorColumns;
        }

        /// <summary>
        /// Given discriminator values (ordinally aligned with DiscriminatorColumns), determines 
        /// the entity type to return. Throws a CommandExecutionException if the type is ambiguous.
        /// </summary>
        internal EntityType Discriminate(object[] discriminatorValues, int resultSetIndex)
        {
            var resultMapping = GetResultMapping(resultSetIndex);
            Debug.Assert(resultMapping != null);

            // initialize matching types bit map
            var typeCandidates = new BitArray(resultMapping.MappedEntityTypes.Count, true);

            foreach (var typeMapping in resultMapping.NormalizedEntityTypeMappings)
            {
                // check if this type mapping is matched
                var matches = true;
                var columnConditions = typeMapping.ColumnConditions;
                for (var i = 0; i < columnConditions.Count; i++)
                {
                    if (null != columnConditions[i]
                        && // this discriminator doesn't matter for the given condition
                        !columnConditions[i].ColumnValueMatchesCondition(discriminatorValues[i]))
                    {
                        matches = false;
                        break;
                    }
                }

                if (matches)
                {
                    // if the type condition is met, narrow the set of type candidates
                    typeCandidates = typeCandidates.And(typeMapping.ImpliedEntityTypes);
                }
                else
                {
                    // if the type condition fails, all implied types are eliminated
                    // (the type mapping fragment is a co-implication, so a type is no longer
                    // a candidate if any condition referring to it is false)
                    typeCandidates = typeCandidates.And(typeMapping.ComplementImpliedEntityTypes);
                }
            }

            // find matching type condition
            EntityType entityType = null;
            for (var i = 0; i < typeCandidates.Length; i++)
            {
                if (typeCandidates[i])
                {
                    if (null != entityType)
                    {
                        throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderUnableToDetermineType);
                    }
                    entityType = resultMapping.MappedEntityTypes[i];
                }
            }

            // if there is no match, raise an exception
            if (null == entityType)
            {
                throw new EntityCommandExecutionException(Strings.ADP_InvalidDataReaderUnableToDetermineType);
            }

            return entityType;
        }

        /// <summary>
        /// Determines the expected shape of store results. We expect a column for every property
        /// of the mapped type (or types) and a column for every discriminator column. We make no
        /// assumptions about the order of columns: the provider is expected to determine appropriate
        /// types by looking at the names of the result columns, not the order of columns, which is
        /// different from the typical handling of row types in the EF.
        /// </summary>
        /// <remarks>
        /// Requires that the given function import mapping refers to a Collection(Entity) or Collection(ComplexType) CSDL
        /// function.
        /// </remarks>
        /// <returns>Row type.</returns>
        internal TypeUsage GetExpectedTargetResultType(int resultSetIndex)
        {
            var resultMapping = GetResultMapping(resultSetIndex);

            // Collect all columns as name-type pairs.
            var columns = new Dictionary<string, TypeUsage>();

            // Figure out which entity types we expect to yield from the function.
            IEnumerable<StructuralType> structuralTypes;
            if (0 == resultMapping.NormalizedEntityTypeMappings.Count)
            {
                // No explicit type mappings; just use the type specified in the ReturnType attribute on the function.
                StructuralType structuralType;
                MetadataHelper.TryGetFunctionImportReturnType(FunctionImport, resultSetIndex, out structuralType);
                Debug.Assert(null != structuralType, "this method must be called only for entity/complextype reader function imports");
                structuralTypes = new[] { structuralType };
            }
            else
            {
                // Types are explicitly mapped.
                structuralTypes = resultMapping.MappedEntityTypes.Cast<StructuralType>();
            }

            // Gather columns corresponding to all properties.
            foreach (var structuralType in structuralTypes)
            {
                foreach (EdmProperty property in TypeHelpers.GetAllStructuralMembers(structuralType))
                {
                    // NOTE: if a complex type is encountered, the column map generator will
                    // throw. For now, we just let them through.

                    // We expect to see each property multiple times, so we use indexer rather than
                    // .Add.
                    columns[property.Name] = property.TypeUsage;
                }
            }

            // Gather discriminator columns.
            foreach (var discriminatorColumn in GetDiscriminatorColumns(resultSetIndex))
            {
                if (!columns.ContainsKey(discriminatorColumn))
                {
                    // CONSIDER: we assume that discriminatorColumns are all string types. In practice,
                    // we're flexible about the runtime type during materialization, so the provider's
                    // decision is hopefully irrelevant. The alternative is to require typed stored
                    // procedure declarations in the SSDL, which is too much of a burden on the user and/or the
                    // tools (there is no reliable way of determining this metadata automatically from SQL
                    // Server).

                    var type = TypeUsage.CreateStringTypeUsage(
                        MetadataWorkspace.GetModelPrimitiveType(PrimitiveTypeKind.String), true, false);
                    columns.Add(discriminatorColumn, type);
                }
            }

            // Expected type is a collection of rows
            var rowType = new RowType(columns.Select(c => new EdmProperty(c.Key, c.Value)));
            var result = TypeUsage.Create(new CollectionType(TypeUsage.Create(rowType)));
            return result;
        }
    }
}
