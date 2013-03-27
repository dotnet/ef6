// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Utils
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    /// <summary>
    ///     This class encapsulates "external" calls from view/MDF generation to other System.Data.Entity features.
    /// </summary>
    internal static class ExternalCalls
    {
        internal static bool IsReservedKeyword(string name)
        {
            return CqlLexer.IsReservedKeyword(name);
        }

        internal static DbCommandTree CompileView(
            string viewDef,
            StorageMappingItemCollection mappingItemCollection,
            ParserOptions.CompilationMode compilationMode)
        {
            DebugCheck.NotEmpty(viewDef);
            DebugCheck.NotNull(mappingItemCollection);

            Perspective perspective = new TargetPerspective(mappingItemCollection.Workspace);
            var parserOptions = new ParserOptions();
            parserOptions.ParserCompilationMode = compilationMode;
            var expr = CqlQuery.Compile(viewDef, perspective, parserOptions, null).CommandTree;
            Debug.Assert(expr != null, "Compile returned empty tree?");

            return expr;
        }

        internal static DbExpression CompileFunctionView(
            string viewDef,
            StorageMappingItemCollection mappingItemCollection,
            ParserOptions.CompilationMode compilationMode,
            IEnumerable<DbParameterReferenceExpression> parameters)
        {
            DebugCheck.NotEmpty(viewDef);
            DebugCheck.NotNull(mappingItemCollection);

            Perspective perspective = new TargetPerspective(mappingItemCollection.Workspace);
            var parserOptions = new ParserOptions();
            parserOptions.ParserCompilationMode = compilationMode;

            // Parameters have to be accessible in the body as regular scope variables, not as command parameters.
            // Hence compile view as lambda with parameters as lambda vars, then invoke the lambda specifying
            // command parameters as values of the lambda vars.
            var functionBody = CqlQuery.CompileQueryCommandLambda(
                viewDef,
                perspective,
                parserOptions,
                null /* parameters */,
                parameters.Select(pInfo => pInfo.ResultType.Variable(pInfo.ParameterName)));
            Debug.Assert(functionBody != null, "functionBody != null");
            DbExpression expr = functionBody.Invoke(parameters);

            return expr;
        }

        /// <summary>
        ///     Compiles eSQL <paramref name="functionDefinition" /> and returns <see cref="DbLambda" />.
        ///     Guarantees type match of lambda variables and <paramref name="functionParameters" />.
        ///     Passes thru all excepions coming from <see cref="CqlQuery" />.
        /// </summary>
        internal static DbLambda CompileFunctionDefinition(
            string functionDefinition,
            IList<FunctionParameter> functionParameters,
            EdmItemCollection edmItemCollection)
        {
            DebugCheck.NotNull(functionParameters);
            DebugCheck.NotNull(edmItemCollection);

            var perspective = new ModelPerspective(
                new MetadataWorkspace(
                    () => edmItemCollection,
                    () => null,
                    () => null));

            // Since we compile lambda expression and generate variables from the function parameter definitions,
            // the returned DbLambda will contain variable types that match function parameter types.
            var functionBody = CqlQuery.CompileQueryCommandLambda(
                functionDefinition,
                perspective,
                null /* use default parser options */,
                null /* parameters */,
                functionParameters.Select(pInfo => pInfo.TypeUsage.Variable(pInfo.Name)));
            Debug.Assert(functionBody != null, "functionBody != null");

            return functionBody;
        }
    }
}
