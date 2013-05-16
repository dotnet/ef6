// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Core.Query.PlanCompiler;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    /// <summary>
    ///     Represents a mapping from a model function import to a store composable function.
    /// </summary>
    public class FunctionImportMappingComposable : FunctionImportMapping
    {
        [SuppressMessage("Microsoft.Design", "CA1002:DoNotExposeGenericLists")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public FunctionImportMappingComposable(
            EdmFunction functionImport,
            EdmFunction targetFunction,
            List<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>> structuralTypeMappings)
            : base(functionImport, targetFunction)
        {
            if (!functionImport.IsComposableAttribute)
            {
                throw new ArgumentException(Strings.NonComposableFunctionCannotBeMappedAsComposable("functionImport"));
            }

            if (!targetFunction.IsComposableAttribute)
            {
                throw new ArgumentException(Strings.NonComposableFunctionCannotBeMappedAsComposable("targetFunction"));
            }

            if (functionImport.EntitySet != null)
            {
                throw new NotSupportedException(Strings.ComposableFunctionImportsReturningEntitiesNotSupported);
            }

            EdmType resultType;
            if (!MetadataHelper.TryGetFunctionImportReturnType(functionImport, 0, out resultType))
            {
                throw new ArgumentException(Strings.InvalidReturnTypeForComposableFunction);
            }

            if (!TypeSemantics.IsScalarType(resultType)
                && (structuralTypeMappings == null || structuralTypeMappings.Count == 0))
            {
                throw new ArgumentException(Strings.StructuralTypeMappingsMustNotBeNullForFunctionImportsReturingNonScalarValues);
            }

            m_structuralTypeMappings = structuralTypeMappings;
        }

        internal FunctionImportMappingComposable(
            EdmFunction functionImport,
            EdmFunction targetFunction,
            List<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>> structuralTypeMappings,
            EdmProperty[] targetFunctionKeys,
            StorageMappingItemCollection mappingItemCollection)
            : base(functionImport, targetFunction)
        {
            DebugCheck.NotNull(mappingItemCollection);
            Debug.Assert(functionImport.IsComposableAttribute, "functionImport.IsComposableAttribute");
            Debug.Assert(targetFunction.IsComposableAttribute, "targetFunction.IsComposableAttribute");
            Debug.Assert(
                functionImport.EntitySet == null || structuralTypeMappings != null,
                "Function import returning entities must have structuralTypeMappings.");
            Debug.Assert(
                structuralTypeMappings == null || structuralTypeMappings.Count > 0, "Non-null structuralTypeMappings must not be empty.");
            EdmType resultType;
            Debug.Assert(
                structuralTypeMappings != null ||
                MetadataHelper.TryGetFunctionImportReturnType(functionImport, 0, out resultType) && TypeSemantics.IsScalarType(resultType),
                "Either type mappings should be specified or the function import should be Collection(Scalar).");
            Debug.Assert(
                functionImport.EntitySet == null || targetFunctionKeys != null,
                "Keys must be inferred for a function import returning entities.");
            Debug.Assert(targetFunctionKeys == null || targetFunctionKeys.Length > 0, "Keys must be null or non-empty.");

            m_mappingItemCollection = mappingItemCollection;
            // We will use these parameters to target s-space function calls in the generated command tree. 
            // Since enums don't exist in s-space we need to use the underlying type.
            m_commandParameters =
                functionImport.Parameters.Select(p => TypeHelpers.GetPrimitiveTypeUsageForScalar(p.TypeUsage).Parameter(p.Name)).ToArray();
            m_structuralTypeMappings = structuralTypeMappings;
            m_targetFunctionKeys = targetFunctionKeys;
        }

        private readonly StorageMappingItemCollection m_mappingItemCollection;

        /// <summary>
        ///     Command parameter refs created from m_edmFunction parameters.
        ///     Used as arguments to target (s-space) function calls in the generated command tree.
        /// </summary>
        private readonly DbParameterReferenceExpression[] m_commandParameters;

        /// <summary>
        ///     Result mapping as entity type hierarchy.
        /// </summary>
        private readonly List<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>>
            m_structuralTypeMappings;

        /// <summary>
        ///     Keys inside the result set of the target function. Inferred based on the mapping (using c-space entity type keys).
        /// </summary>
        private readonly EdmProperty[] m_targetFunctionKeys;

        /// <summary>
        ///     ITree template. Requires function argument substitution during function view expansion.
        /// </summary>
        private Node m_internalTreeNode;

        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public ReadOnlyCollection<Tuple<StructuralType, List<StorageConditionPropertyMapping>, List<StoragePropertyMapping>>>
            StructuralTypeMappings
        {
            get { return m_structuralTypeMappings == null ? null : m_structuralTypeMappings.AsReadOnly(); }
        }

        internal EdmProperty[] TvfKeys
        {
            get { return m_targetFunctionKeys; }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "projectOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal Node GetInternalTree(Command targetIqtCommand, IList<Node> targetIqtArguments)
        {
            if (m_internalTreeNode == null)
            {
                DiscriminatorMap discriminatorMap;
                var tree = GenerateFunctionView(out discriminatorMap);
                Debug.Assert(tree != null, "tree != null");

                // Convert this into an ITree first
                var itree = ITreeGenerator.Generate(tree, discriminatorMap);
                var rootProject = itree.Root; // PhysicalProject(RelInput)
                PlanCompiler.Assert(
                    rootProject.Op.OpType == OpType.PhysicalProject,
                    "Expected a physical projectOp at the root of the tree - found " + rootProject.Op.OpType);
                var rootProjectOp = (PhysicalProjectOp)rootProject.Op;
                Debug.Assert(rootProjectOp.Outputs.Count == 1, "rootProjectOp.Outputs.Count == 1");
                var rootInput = rootProject.Child0; // the RelInput in PhysicalProject(RelInput)

                // #554756: VarVec enumerators are not cached on the shared Command instance.
                itree.DisableVarVecEnumCaching();

                // Function import returns a collection, so convert it to a scalar by wrapping into CollectOp.
                var relNode = rootInput;
                var relVar = rootProjectOp.Outputs[0];
                // ProjectOp does not implement Type property, so get the type from the column map.
                var functionViewType = rootProjectOp.ColumnMap.Type;
                if (!Command.EqualTypes(functionViewType, FunctionImport.ReturnParameter.TypeUsage))
                {
                    Debug.Assert(
                        TypeSemantics.IsPromotableTo(functionViewType, FunctionImport.ReturnParameter.TypeUsage),
                        "Mapping expression result type must be promotable to the c-space function return type.");

                    // Build "relNode = Project(relNode, SoftCast(relVar))"
                    var expectedCollectionType = (CollectionType)FunctionImport.ReturnParameter.TypeUsage.EdmType;
                    var expectedElementType = expectedCollectionType.TypeUsage;

                    var varRefNode = itree.CreateNode(itree.CreateVarRefOp(relVar));
                    var castNode = itree.CreateNode(itree.CreateSoftCastOp(expectedElementType), varRefNode);
                    var varDefListNode = itree.CreateVarDefListNode(castNode, out relVar);

                    var projectOp = itree.CreateProjectOp(relVar);
                    relNode = itree.CreateNode(projectOp, relNode, varDefListNode);
                }

                // Build "Collect(PhysicalProject(relNode))
                m_internalTreeNode = itree.BuildCollect(relNode, relVar);
            }
            Debug.Assert(m_internalTreeNode != null, "m_internalTreeNode != null");

            // Prepare argument replacement dictionary
            Debug.Assert(m_commandParameters.Length == targetIqtArguments.Count, "m_commandParameters.Length == targetIqtArguments.Count");
            var viewArguments = new Dictionary<string, Node>(m_commandParameters.Length);
            for (var i = 0; i < m_commandParameters.Length; ++i)
            {
                var commandParam = m_commandParameters[i];
                var argumentNode = targetIqtArguments[i];

                // If function import parameter is of enum type, the argument value for it will be of enum type. We however have 
                // converted enum types to underlying types for m_commandParameters. So we now need to softcast the argument 
                // expression to the underlying type as well.
                if (TypeSemantics.IsEnumerationType(argumentNode.Op.Type))
                {
                    argumentNode = targetIqtCommand.CreateNode(
                        targetIqtCommand.CreateSoftCastOp(TypeHelpers.CreateEnumUnderlyingTypeUsage(argumentNode.Op.Type)),
                        argumentNode);
                }

                Debug.Assert(
                    TypeSemantics.IsPromotableTo(argumentNode.Op.Type, commandParam.ResultType),
                    "Argument type must be promotable to parameter type.");

                viewArguments.Add(commandParam.ParameterName, argumentNode);
            }

            return FunctionViewOpCopier.Copy(targetIqtCommand, m_internalTreeNode, viewArguments);
        }

        private sealed class FunctionViewOpCopier : OpCopier
        {
            private readonly Dictionary<string, Node> m_viewArguments;

            private FunctionViewOpCopier(Command cmd, Dictionary<string, Node> viewArguments)
                : base(cmd)
            {
                m_viewArguments = viewArguments;
            }

            internal static Node Copy(Command cmd, Node viewNode, Dictionary<string, Node> viewArguments)
            {
                return new FunctionViewOpCopier(cmd, viewArguments).CopyNode(viewNode);
            }

            public override Node Visit(VarRefOp op, Node n)
            {
                // The original function view has store function calls with arguments represented as command parameter refs.
                // We are now replacing command parameter refs with the real argument nodes from the calling tree.
                // The replacement is performed in the function view subtree and we search for parameter refs with names 
                // matching the FunctionImportMapping.FunctionImport parameter names (this is how the command parameters 
                // have been created in the first place, see m_commandParameters and GetCommandTree(...) for more info).
                // The search and replace is not performed on the argument nodes themselves. This is important because it guarantees
                // that we are not replacing unrelated (possibly user-defined) parameter refs that accidentally have the matching names.
                Node argNode;
                if (op.Var.VarType == VarType.Parameter
                    && m_viewArguments.TryGetValue(((ParameterVar)op.Var).ParameterName, out argNode))
                {
                    // Just copy the argNode, do not reapply this visitor. We do not want search and replace inside the argNode. See comment above.
                    return Copy(m_destCmd, argNode);
                }
                else
                {
                    return base.Visit(op, n);
                }
            }
        }

        internal DbQueryCommandTree GenerateFunctionView(out DiscriminatorMap discriminatorMap)
        {
            DebugCheck.NotNull(m_mappingItemCollection);

            discriminatorMap = null;

            // Prepare the direct call of the store function as StoreFunction(@EdmFunc_p1, ..., @EdmFunc_pN).
            // Note that function call arguments are command parameters created from the m_edmFunction parameters.
            Debug.Assert(TargetFunction != null, "this.TargetFunction != null");
            DbExpression storeFunctionInvoke = TargetFunction.Invoke(GetParametersForTargetFunctionCall());

            // Generate the query expression producing c-space result from s-space function call(s).
            DbExpression queryExpression;
            if (m_structuralTypeMappings != null)
            {
                queryExpression = GenerateStructuralTypeResultMappingView(storeFunctionInvoke, out discriminatorMap);
                Debug.Assert(
                    queryExpression != null
                    && TypeSemantics.IsPromotableTo(queryExpression.ResultType, FunctionImport.ReturnParameter.TypeUsage),
                    "TypeSemantics.IsPromotableTo(queryExpression.ResultType, this.FunctionImport.ReturnParameter.TypeUsage)");
            }
            else
            {
                queryExpression = GenerateScalarResultMappingView(storeFunctionInvoke);
                Debug.Assert(
                    queryExpression != null 
                    && TypeSemantics.IsEqual(queryExpression.ResultType, FunctionImport.ReturnParameter.TypeUsage),
                    "TypeSemantics.IsEqual(queryExpression.ResultType, this.FunctionImport.ReturnParameter.TypeUsage)");
            }

            // Generate parameterized command, where command parameters are semantically the c-space function parameters.
            return DbQueryCommandTree.FromValidExpression(
                m_mappingItemCollection.Workspace, TargetPerspective.TargetPerspectiveDataSpace, queryExpression);
        }

        private IEnumerable<DbExpression> GetParametersForTargetFunctionCall()
        {
            Debug.Assert(
                FunctionImport.Parameters.Count == m_commandParameters.Length,
                "this.FunctionImport.Parameters.Count == m_commandParameters.Length");
            Debug.Assert(
                TargetFunction.Parameters.Count == m_commandParameters.Length,
                "this.TargetFunction.Parameters.Count == m_commandParameters.Length");
            foreach (var targetParameter in TargetFunction.Parameters)
            {
                Debug.Assert(
                    FunctionImport.Parameters.Contains(targetParameter.Name),
                    "this.FunctionImport.Parameters.Contains(targetParameter.Name)");
                var functionImportParameter = FunctionImport.Parameters.Single(p => p.Name == targetParameter.Name);
                yield return m_commandParameters[FunctionImport.Parameters.IndexOf(functionImportParameter)];
            }
        }

        private DbExpression GenerateStructuralTypeResultMappingView(
            DbExpression storeFunctionInvoke, out DiscriminatorMap discriminatorMap)
        {
            Debug.Assert(
                m_structuralTypeMappings != null && m_structuralTypeMappings.Count > 0,
                "m_structuralTypeMappings != null && m_structuralTypeMappings.Count > 0");

            discriminatorMap = null;

            // Process explicit structural type mappings. The mapping is based on the direct call of the store function 
            // wrapped into a projection constructing the mapped structural types.

            var queryExpression = storeFunctionInvoke;

            if (m_structuralTypeMappings.Count == 1)
            {
                var mapping = m_structuralTypeMappings[0];

                var type = mapping.Item1;
                var conditions = mapping.Item2;
                var propertyMappings = mapping.Item3;

                if (conditions.Count > 0)
                {
                    queryExpression = queryExpression.Where((row) => GenerateStructuralTypeConditionsPredicate(conditions, row));
                }

                var binding = queryExpression.BindAs("row");
                var entityTypeMappingView = GenerateStructuralTypeMappingView(type, propertyMappings, binding.Variable);

                queryExpression = binding.Project(entityTypeMappingView);
            }
            else
            {
                var binding = queryExpression.BindAs("row");

                // Make sure type projection is performed over a closed set where each row is guaranteed to produce a known type.
                // To do this, filter the store function output using the type conditions.
                Debug.Assert(m_structuralTypeMappings.All(m => m.Item2.Count > 0), "In multi-type mapping each type must have conditions.");
                var structuralTypePredicates =
                    m_structuralTypeMappings.Select(m => GenerateStructuralTypeConditionsPredicate(m.Item2, binding.Variable)).ToList();
                queryExpression = binding.Filter(
                    Helpers.BuildBalancedTreeInPlace(
                        structuralTypePredicates.ToArray(), // clone, otherwise BuildBalancedTreeInPlace will change it
                        (prev, next) => prev.Or(next)));
                binding = queryExpression.BindAs("row");

                var structuralTypeMappingViews = new List<DbExpression>(m_structuralTypeMappings.Count);
                foreach (var mapping in m_structuralTypeMappings)
                {
                    var type = mapping.Item1;
                    var propertyMappings = mapping.Item3;

                    structuralTypeMappingViews.Add(GenerateStructuralTypeMappingView(type, propertyMappings, binding.Variable));
                }
                Debug.Assert(
                    structuralTypeMappingViews.Count == structuralTypePredicates.Count,
                    "structuralTypeMappingViews.Count == structuralTypePredicates.Count");

                // Because we are projecting over the closed set, we can convert the last WHEN THEN into ELSE.
                DbExpression typeConstructors = DbExpressionBuilder.Case(
                    structuralTypePredicates.Take(m_structuralTypeMappings.Count - 1),
                    structuralTypeMappingViews.Take(m_structuralTypeMappings.Count - 1),
                    structuralTypeMappingViews[m_structuralTypeMappings.Count - 1]);

                queryExpression = binding.Project(typeConstructors);

                if (DiscriminatorMap.TryCreateDiscriminatorMap(FunctionImport.EntitySet, queryExpression, out discriminatorMap))
                {
                    Debug.Assert(discriminatorMap != null, "discriminatorMap == null after it has been created");
                }
            }

            return queryExpression;
        }

        private static DbExpression GenerateStructuralTypeMappingView(
            StructuralType structuralType, List<StoragePropertyMapping> propertyMappings, DbExpression row)
        {
            // Generate property views.
            var properties = TypeHelpers.GetAllStructuralMembers(structuralType);
            Debug.Assert(properties.Count == propertyMappings.Count, "properties.Count == propertyMappings.Count");
            var constructorArgs = new List<DbExpression>(properties.Count);
            for (var i = 0; i < propertyMappings.Count; ++i)
            {
                var propertyMapping = propertyMappings[i];
                Debug.Assert(properties[i].EdmEquals(propertyMapping.EdmProperty), "properties[i].EdmEquals(propertyMapping.EdmProperty)");
                constructorArgs.Add(GeneratePropertyMappingView(propertyMapping, row));
            }
            // Return the structural type constructor.
            return TypeUsage.Create(structuralType).New(constructorArgs);
        }

        private static DbExpression GenerateStructuralTypeConditionsPredicate(
            List<StorageConditionPropertyMapping> conditions, DbExpression row)
        {
            Debug.Assert(conditions.Count > 0, "conditions.Count > 0");
            var predicate = Helpers.BuildBalancedTreeInPlace(
                conditions.Select(c => GeneratePredicate(c, row)).ToArray(), (prev, next) => prev.And(next));
            return predicate;
        }

        private static DbExpression GeneratePredicate(StorageConditionPropertyMapping condition, DbExpression row)
        {
            Debug.Assert(condition.EdmProperty == null, "C-side conditions are not supported in function mappings.");
            var columnRef = GenerateColumnRef(row, condition.ColumnProperty);

            if (condition.IsNull.HasValue)
            {
                return condition.IsNull.Value ? columnRef.IsNull() : (DbExpression)columnRef.IsNull().Not();
            }
            else
            {
                return columnRef.Equal(columnRef.ResultType.Constant(condition.Value));
            }
        }

        private static DbExpression GeneratePropertyMappingView(StoragePropertyMapping mapping, DbExpression row)
        {
            Debug.Assert(mapping is StorageScalarPropertyMapping, "Complex property mapping is not supported in function imports.");
            var scalarPropertyMapping = (StorageScalarPropertyMapping)mapping;
            return GenerateScalarPropertyMappingView(scalarPropertyMapping.EdmProperty, scalarPropertyMapping.ColumnProperty, row);
        }

        private static DbExpression GenerateScalarPropertyMappingView(EdmProperty edmProperty, EdmProperty columnProperty, DbExpression row)
        {
            var accessorExpr = GenerateColumnRef(row, columnProperty);
            if (!TypeSemantics.IsEqual(accessorExpr.ResultType, edmProperty.TypeUsage))
            {
                accessorExpr = accessorExpr.CastTo(edmProperty.TypeUsage);
            }
            return accessorExpr;
        }

        private static DbExpression GenerateColumnRef(DbExpression row, EdmProperty column)
        {
            Debug.Assert(row.ResultType.EdmType.BuiltInTypeKind == BuiltInTypeKind.RowType, "Input type is expected to be a row type.");
            var rowType = (RowType)row.ResultType.EdmType;
            Debug.Assert(rowType.Properties.Contains(column.Name), "Column name must be resolvable in the TVF result type.");
            return row.Property(column.Name);
        }

        private DbExpression GenerateScalarResultMappingView(DbExpression storeFunctionInvoke)
        {
            var queryExpression = storeFunctionInvoke;

            CollectionType functionImportReturnType;
            if (!MetadataHelper.TryGetFunctionImportReturnCollectionType(FunctionImport, 0, out functionImportReturnType))
            {
                Debug.Fail("Failed to get the result type of the function import.");
            }

            Debug.Assert(TypeSemantics.IsCollectionType(queryExpression.ResultType), "Store function must be TVF (collection expected).");
            var collectionType = (CollectionType)queryExpression.ResultType.EdmType;
            Debug.Assert(TypeSemantics.IsRowType(collectionType.TypeUsage), "Store function must be TVF (collection of rows expected).");
            var rowType = (RowType)collectionType.TypeUsage.EdmType;
            var column = rowType.Properties[0];

            Func<DbExpression, DbExpression> scalarView = row =>
                {
                    var propertyAccess = row.Property(column);
                    if (TypeSemantics.IsEqual(
                        functionImportReturnType.TypeUsage, column.TypeUsage))
                    {
                        return propertyAccess;
                    }
                    else
                    {
                        return propertyAccess.CastTo(functionImportReturnType.TypeUsage);
                    }
                };

// ReSharper disable ConvertClosureToMethodGroup
            // using Method Group breaks matching the expression in DbExpressionBuilder.ResolveToExpression
            return queryExpression.Select(row => scalarView(row));
// ReSharper restore ConvertClosureToMethodGroup
        }
    }
}
