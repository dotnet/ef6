// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.CommandTrees.ExpressionBuilder;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Metadata.Edm.Provider;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;

    // <summary>
    // Implements Semantic Analysis and Conversion
    // Provides the translation service between an abstract syntax tree to a canonical command tree
    // The class was designed to be edmType system agnostic by delegating to a given SemanticResolver instance all edmType related services as well as to TypeHelper class, however
    // we rely on the assumption that metadata was pre-loaded and is relevant to the query.
    // </summary>
    [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
    internal sealed class SemanticAnalyzer
    {
        private readonly SemanticResolver _sr;

        // <summary>
        // Initializes semantic analyzer
        // </summary>
        // <param name="sr"> initialized SemanticResolver instance for a given typespace/edmType system </param>
        internal SemanticAnalyzer(SemanticResolver sr)
        {
            DebugCheck.NotNull(sr);
            _sr = sr;
        }

        // <summary>
        // Entry point to semantic analysis. Converts AST into a <see cref="DbCommandTree" />.
        // </summary>
        // <param name="astExpr"> ast command tree </param>
        // <remarks>
        // <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntactic or Semantic rules are violated and the query cannot be accepted</exception>
        // <exception cref="System.Data.Entity.Core.MetadataException">Thrown when metadata related service requests fail</exception>
        // <exception cref="System.Data.Entity.Core.MappingException">Thrown when mapping related service requests fail</exception>
        // </remarks>
        // <returns> ParseResult with a valid DbCommandTree </returns>
        internal ParseResult AnalyzeCommand(Node astExpr)
        {
            //
            // Ensure that the AST expression is a valid Command expression
            //
            var astCommandExpr = ValidateQueryCommandAst(astExpr);

            //
            // Convert namespace imports and add them to _sr.TypeResolver.
            //
            ConvertAndRegisterNamespaceImports(astCommandExpr.NamespaceImportList, astCommandExpr.ErrCtx, _sr);

            //
            // Convert the AST command root expression to a command tree using the appropriate converter
            //
            var parseResult = ConvertStatement(astCommandExpr.Statement, _sr);

            Debug.Assert(parseResult != null, "ConvertStatement produced null parse result");
            Debug.Assert(parseResult.CommandTree != null, "ConvertStatement returned null command tree");

            return parseResult;
        }

        // <summary>
        // Converts query command AST into a <see cref="DbExpression" />.
        // </summary>
        // <param name="astExpr"> ast command tree </param>
        // <remarks>
        // <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntactic or Semantic rules are violated and the query cannot be accepted</exception>
        // <exception cref="System.Data.Entity.Core.MetadataException">Thrown when metadata related service requests fail</exception>
        // <exception cref="System.Data.Entity.Core.MappingException">Thrown when mapping related service requests fail</exception>
        // </remarks>
        // <returns> DbExpression </returns>
        internal DbLambda AnalyzeQueryCommand(Node astExpr)
        {
            //
            // Ensure that the AST expression is a valid query command expression
            // (only a query command root expression can produce a standalone DbExpression)
            //
            var astQueryCommandExpr = ValidateQueryCommandAst(astExpr);

            //
            // Convert namespace imports and add them to _sr.TypeResolver.
            //
            ConvertAndRegisterNamespaceImports(astQueryCommandExpr.NamespaceImportList, astQueryCommandExpr.ErrCtx, _sr);

            //
            // Convert the AST of the query command root expression into a DbExpression
            //
            List<FunctionDefinition> functionDefs;
            var expression = ConvertQueryStatementToDbExpression(astQueryCommandExpr.Statement, _sr, out functionDefs);

            // Construct DbLambda from free variables and the expression
            var lambda = DbExpressionBuilder.Lambda(expression, _sr.Variables.Values);

            Debug.Assert(lambda != null, "AnalyzeQueryCommand returned null");

            return lambda;
        }

        private static Command ValidateQueryCommandAst(Node astExpr)
        {
            var astCommandExpr = astExpr as Command;
            if (null == astCommandExpr)
            {
                throw new ArgumentException(Strings.UnknownAstCommandExpression);
            }

            if (!(astCommandExpr.Statement is QueryStatement))
            {
                throw new ArgumentException(Strings.UnknownAstExpressionType);
            }

            return astCommandExpr;
        }

        // <summary>
        // Converts namespace imports and adds them to the edmType resolver.
        // </summary>
        private static void ConvertAndRegisterNamespaceImports(
            NodeList<NamespaceImport> nsImportList, ErrorContext cmdErrCtx, SemanticResolver sr)
        {
            var aliasedNamespaceImports = new List<Tuple<string, MetadataNamespace, ErrorContext>>();
            var namespaceImports = new List<Tuple<MetadataNamespace, ErrorContext>>();

            //
            // Resolve all user-defined namespace imports to MetadataMember objects _before_ adding them to the edmType resolver,
            // this is needed to keep resolution within the command prolog unaffected by previously resolved imports.
            //
            if (nsImportList != null)
            {
                foreach (var namespaceImport in nsImportList)
                {
                    string[] name = null;

                    var identifier = namespaceImport.NamespaceName as Identifier;
                    if (identifier != null)
                    {
                        name = new[] { identifier.Name };
                    }

                    var dotExpr = namespaceImport.NamespaceName as DotExpr;
                    if (dotExpr != null
                        && dotExpr.IsMultipartIdentifier(out name))
                    {
                        Debug.Assert(name != null, "name != null");
                    }

                    if (name == null)
                    {
                        var errCtx = namespaceImport.NamespaceName.ErrCtx;
                        var message = Strings.InvalidMetadataMemberName;
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    var alias = namespaceImport.Alias != null ? namespaceImport.Alias.Name : null;

                    var metadataMember = sr.ResolveMetadataMemberName(name, namespaceImport.NamespaceName.ErrCtx);
                    Debug.Assert(metadataMember != null, "metadata member name resolution must not return null");

                    if (metadataMember.MetadataMemberClass
                        == MetadataMemberClass.Namespace)
                    {
                        var metadataNamespace = (MetadataNamespace)metadataMember;
                        if (alias != null)
                        {
                            aliasedNamespaceImports.Add(Tuple.Create(alias, metadataNamespace, namespaceImport.ErrCtx));
                        }
                        else
                        {
                            namespaceImports.Add(Tuple.Create(metadataNamespace, namespaceImport.ErrCtx));
                        }
                    }
                    else
                    {
                        var errCtx = namespaceImport.NamespaceName.ErrCtx;
                        var message = Strings.InvalidMetadataMemberClassResolution(
                            metadataMember.Name, metadataMember.MetadataMemberClassName, MetadataNamespace.NamespaceClassName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                }
            }

            //
            // Add resolved user-defined imports to the edmType resolver.
            // Before adding user-defined namespace imports, add EDM namespace import to make canonical functions and types available in the command text.
            //
            sr.TypeResolver.AddNamespaceImport(
                new MetadataNamespace(EdmConstants.EdmNamespace), nsImportList != null ? nsImportList.ErrCtx : cmdErrCtx);
            foreach (var resolvedAliasedNamespaceImport in aliasedNamespaceImports)
            {
                sr.TypeResolver.AddAliasedNamespaceImport(
                    resolvedAliasedNamespaceImport.Item1, resolvedAliasedNamespaceImport.Item2, resolvedAliasedNamespaceImport.Item3);
            }
            foreach (var resolvedNamespaceImport in namespaceImports)
            {
                sr.TypeResolver.AddNamespaceImport(resolvedNamespaceImport.Item1, resolvedNamespaceImport.Item2);
            }
        }

        // <summary>
        // Dispatches/Converts statement expressions.
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static ParseResult ConvertStatement(Statement astStatement, SemanticResolver sr)
        {
            DebugCheck.NotNull(astStatement);

            StatementConverter statementConverter;
            if (astStatement is QueryStatement)
            {
                statementConverter = ConvertQueryStatementToDbCommandTree;
            }
            else
            {
                throw new ArgumentException(Strings.UnknownAstExpressionType);
            }

            var converted = statementConverter(astStatement, sr);

            Debug.Assert(converted != null, "statementConverter returned null");
            Debug.Assert(converted.CommandTree != null, "statementConverter produced null command tree");

            return converted;
        }

        private delegate ParseResult StatementConverter(Statement astExpr, SemanticResolver sr);

        // <summary>
        // Converts query statement AST to a <see cref="DbQueryCommandTree" />
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static ParseResult ConvertQueryStatementToDbCommandTree(Statement astStatement, SemanticResolver sr)
        {
            DebugCheck.NotNull(astStatement);

            List<FunctionDefinition> functionDefs;
            var converted = ConvertQueryStatementToDbExpression(astStatement, sr, out functionDefs);

            Debug.Assert(converted != null, "ConvertQueryStatementToDbExpression returned null");
            Debug.Assert(functionDefs != null, "ConvertQueryStatementToDbExpression produced null functionDefs");

            return new ParseResult(
                DbQueryCommandTree.FromValidExpression(
                    sr.TypeResolver.Perspective.MetadataWorkspace, sr.TypeResolver.Perspective.TargetDataspace, converted, 
                    useDatabaseNullSemantics: true, disableFilterOverProjectionSimplificationForCustomFunctions: false),
                functionDefs);
        }

        // <summary>
        // Converts the query statement to a normalized and validated <see cref="DbExpression" />.
        // This entry point to the semantic analysis phase is used when producing a
        // query command tree or producing only a <see cref="DbExpression" />.
        // </summary>
        // <param name="astStatement"> The query statement </param>
        // <param name="sr">
        // The <see cref="SemanticResolver" /> instance to use
        // </param>
        // <returns>
        // An instance of <see cref="DbExpression" /> , adjusted to handle 'inline' projections and validated to produce a result edmType appropriate for the root of a query command tree.
        // </returns>
        private static DbExpression ConvertQueryStatementToDbExpression(
            Statement astStatement, SemanticResolver sr, out List<FunctionDefinition> functionDefs)
        {
            DebugCheck.NotNull(astStatement);

            var queryStatement = astStatement as QueryStatement;

            if (queryStatement == null)
            {
                throw new ArgumentException(Strings.UnknownAstExpressionType);
            }

            //
            // Convert query inline definitions and create parse result. 
            // Converted inline definitions are also added to the semantic resolver.
            //
            functionDefs = ConvertInlineFunctionDefinitions(queryStatement.FunctionDefList, sr);

            //
            // Convert top level expression
            //
            var converted = ConvertValueExpressionAllowUntypedNulls(queryStatement.Expr, sr);
            if (converted == null)
            {
                //
                // Ensure converted expression is not untyped null.
                // Use error context of the top-level expression.
                //
                var errCtx = queryStatement.Expr.ErrCtx;
                var message = Strings.ResultingExpressionTypeCannotBeNull;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Handle the "inline" projection case
            //
            if (converted is DbScanExpression)
            {
                var source = converted.BindAs(sr.GenerateInternalName("extent"));

                converted = source.Project(source.Variable);
            }

            //
            // Ensure return edmType is valid for query. For V1, association types are the only 
            // edmType that cannot be at 'top' level result. Note that this is only applicable in
            // general queries and association types are valid in view gen mode queries.
            // Use error context of the top-level expression.
            //
            if (sr.ParserOptions.ParserCompilationMode
                == ParserOptions.CompilationMode.NormalMode)
            {
                ValidateQueryResultType(converted.ResultType, queryStatement.Expr.ErrCtx);
            }

            Debug.Assert(null != converted, "null != converted");

            return converted;
        }

        // <summary>
        // Ensures that the result of a query expression is valid.
        // </summary>
        private static void ValidateQueryResultType(TypeUsage resultType, ErrorContext errCtx)
        {
            if (Helper.IsCollectionType(resultType.EdmType))
            {
                ValidateQueryResultType(((CollectionType)resultType.EdmType).TypeUsage, errCtx);
            }
            else if (Helper.IsRowType(resultType.EdmType))
            {
                foreach (var property in ((RowType)resultType.EdmType).Properties)
                {
                    ValidateQueryResultType(property.TypeUsage, errCtx);
                }
            }
            else if (Helper.IsAssociationType(resultType.EdmType))
            {
                var message = Strings.InvalidQueryResultType(resultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        // <summary>
        // Converts query inline function defintions. Returns empty list in case of no definitions.
        // </summary>
        private static List<FunctionDefinition> ConvertInlineFunctionDefinitions(
            NodeList<AST.FunctionDefinition> functionDefList, SemanticResolver sr)
        {
            var functionDefinitions = new List<FunctionDefinition>();

            if (functionDefList != null)
            {
                //
                // Process inline function signatures, declare functions in the edmType resolver.
                //
                var inlineFunctionInfos = new List<InlineFunctionInfo>();
                foreach (var functionDefAst in functionDefList)
                {
                    //
                    // Get and validate function name.
                    //
                    var name = functionDefAst.Name;
                    Debug.Assert(!String.IsNullOrEmpty(name), "function name must not be null or empty");

                    //
                    // Process function parameters
                    //
                    var parameters = ConvertInlineFunctionParameterDefs(functionDefAst.Parameters, sr);
                    Debug.Assert(parameters != null, "parameters must not be null"); // should be empty collection if no parameters

                    //
                    // Register new function in the edmType resolver.
                    //
                    InlineFunctionInfo functionInfo = new InlineFunctionInfoImpl(functionDefAst, parameters);
                    inlineFunctionInfos.Add(functionInfo);
                    sr.TypeResolver.DeclareInlineFunction(name, functionInfo);
                }
                Debug.Assert(functionDefList.Count == inlineFunctionInfos.Count);

                //
                // Convert function defintions.
                //
                foreach (var functionInfo in inlineFunctionInfos)
                {
                    functionDefinitions.Add(
                        new FunctionDefinition(
                            functionInfo.FunctionDefAst.Name,
                            functionInfo.GetLambda(sr),
                            functionInfo.FunctionDefAst.StartPosition,
                            functionInfo.FunctionDefAst.EndPosition));
                }
            }

            return functionDefinitions;
        }

        private static List<DbVariableReferenceExpression> ConvertInlineFunctionParameterDefs(
            NodeList<PropDefinition> parameterDefs, SemanticResolver sr)
        {
            var paramList = new List<DbVariableReferenceExpression>();
            if (parameterDefs != null)
            {
                foreach (var paramDef in parameterDefs)
                {
                    var name = paramDef.Name.Name;

                    //
                    // Validate param name
                    //
                    if (paramList.Exists(
                        (DbVariableReferenceExpression arg) =>
                        sr.NameComparer.Compare(arg.VariableName, name) == 0))
                    {
                        var errCtx = paramDef.ErrCtx;
                        var message = Strings.MultipleDefinitionsOfParameter(name);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    //
                    // Convert parameter edmType
                    //
                    var typeUsage = ConvertTypeDefinition(paramDef.Type, sr);
                    Debug.Assert(typeUsage != null, "typeUsage must not be null");

                    //
                    // Create function parameter ref expression
                    //
                    var paramRefExpr = new DbVariableReferenceExpression(typeUsage, name);
                    paramList.Add(paramRefExpr);
                }
            }
            return paramList;
        }

        private sealed class InlineFunctionInfoImpl : InlineFunctionInfo
        {
            private DbLambda _convertedDefinition;
            private bool _convertingDefinition;

            internal InlineFunctionInfoImpl(AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
                : base(functionDef, parameters)
            {
            }

            internal override DbLambda GetLambda(SemanticResolver sr)
            {
                if (_convertedDefinition == null)
                {
                    //
                    // Check for recursive definitions.
                    //
                    if (_convertingDefinition)
                    {
                        var errCtx = FunctionDefAst.ErrCtx;
                        var message = Strings.Cqt_UDF_FunctionDefinitionWithCircularReference(FunctionDefAst.Name);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    //
                    // Create a copy of semantic resolver without query scope entries to guarantee proper variable bindings inside the function body.
                    // The srSandbox shares InlineFunctionInfo objects with the original semantic resolver (sr), hence all the indirect conversions of
                    // inline functions (in addition to this direct one) will also be visible in the original semantic resolver.
                    //
                    var srSandbox = sr.CloneForInlineFunctionConversion();

                    _convertingDefinition = true;
                    _convertedDefinition = ConvertInlineFunctionDefinition(this, srSandbox);
                    _convertingDefinition = false;
                }
                return _convertedDefinition;
            }
        }

        private static DbLambda ConvertInlineFunctionDefinition(InlineFunctionInfo functionInfo, SemanticResolver sr)
        {
            //
            // Push function definition scope.
            //
            sr.EnterScope();

            //
            // Add function parameters to the scope.
            //
            functionInfo.Parameters.Each(p => sr.CurrentScope.Add(p.VariableName, new FreeVariableScopeEntry(p)));

            //
            // Convert function body expression
            //
            var body = ConvertValueExpression(functionInfo.FunctionDefAst.Body, sr);

            //
            // Pop function definition scope
            //
            sr.LeaveScope();

            //
            // Create and return lambda representing the function body.
            //
            return DbExpressionBuilder.Lambda(body, functionInfo.Parameters);
        }

        // <summary>
        // Converts general expressions (AST.Node)
        // </summary>
        private static ExpressionResolution Convert(Node astExpr, SemanticResolver sr)
        {
            var converter = _astExprConverters[astExpr.GetType()];
            if (converter == null)
            {
                var message = Strings.UnknownAstExpressionType;
                throw new EntitySqlException(message);
            }
            return converter(astExpr, sr);
        }

        // <summary>
        // Converts general expressions (AST.Node) to a <see cref="ValueExpression" />.
        // Returns <see cref="ValueExpression.Value" />.
        // Throws if conversion resulted an a non <see cref="ValueExpression" /> resolution.
        // Throws if conversion resulted in the untyped null.
        // </summary>
        private static DbExpression ConvertValueExpression(Node astExpr, SemanticResolver sr)
        {
            var expr = ConvertValueExpressionAllowUntypedNulls(astExpr, sr);
            if (expr == null)
            {
                var errCtx = astExpr.ErrCtx;
                var message = Strings.ExpressionCannotBeNull;
                throw EntitySqlException.Create(errCtx, message, null);
            }
            return expr;
        }

        // <summary>
        // Converts general expressions (AST.Node) to a <see cref="ValueExpression" />.
        // Returns <see cref="ValueExpression.Value" />.
        // Returns null if expression is the untyped null.
        // Throws if conversion resulted an a non <see cref="ValueExpression" /> resolution.
        // </summary>
        private static DbExpression ConvertValueExpressionAllowUntypedNulls(Node astExpr, SemanticResolver sr)
        {
            var resolution = Convert(astExpr, sr);
            if (resolution.ExpressionClass
                == ExpressionResolutionClass.Value)
            {
                return ((ValueExpression)resolution).Value;
            }
            else if (resolution.ExpressionClass
                     == ExpressionResolutionClass.MetadataMember)
            {
                var metadataMember = (MetadataMember)resolution;
                if (metadataMember.MetadataMemberClass
                    == MetadataMemberClass.EnumMember)
                {
                    var enumMember = (MetadataEnumMember)metadataMember;
                    return enumMember.EnumType.Constant(enumMember.EnumMember.Value);
                }
            }

            //
            // The resolution is not a value and can not be converted to a value: report an error.
            //

            var errorMessage = Strings.InvalidExpressionResolutionClass(resolution.ExpressionClassName, ValueExpression.ValueClassName);

            var identifier = astExpr as Identifier;
            if (identifier != null)
            {
                errorMessage = Strings.CouldNotResolveIdentifier(identifier.Name);
            }

            var dotExpr = astExpr as DotExpr;
            string[] names;
            if (dotExpr != null
                && dotExpr.IsMultipartIdentifier(out names))
            {
                errorMessage = Strings.CouldNotResolveIdentifier(TypeResolver.GetFullName(names));
            }

            var errCtx = astExpr.ErrCtx;
            throw EntitySqlException.Create(errCtx, errorMessage, null);
        }

        // <summary>
        // Converts left and right expressions. If any of them is the untyped null, derives the edmType and converts to a typed null.
        // Throws <see cref="EntitySqlException" /> if conversion is not possible.
        // </summary>
        private static Pair<DbExpression, DbExpression> ConvertValueExpressionsWithUntypedNulls(
            Node leftAst,
            Node rightAst,
            ErrorContext errCtx,
            Func<string> formatMessage,
            SemanticResolver sr)
        {
            var leftExpr = leftAst != null ? ConvertValueExpressionAllowUntypedNulls(leftAst, sr) : null;
            var rightExpr = rightAst != null ? ConvertValueExpressionAllowUntypedNulls(rightAst, sr) : null;

            if (leftExpr == null)
            {
                if (rightExpr == null)
                {
                    var message = formatMessage();
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                else
                {
                    leftExpr = rightExpr.ResultType.Null();
                }
            }
            else if (rightExpr == null)
            {
                rightExpr = leftExpr.ResultType.Null();
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        // <summary>
        // Converts literal expression (AST.Literal)
        // </summary>
        private static ExpressionResolution ConvertLiteral(Node expr, SemanticResolver sr)
        {
            var literal = (Literal)expr;

            if (literal.IsNullLiteral)
            {
                //
                // If it is literal null, return the untyped null: the edmType will be inferred depending on the specific expression in which it participates.
                //
                return new ValueExpression(null);
            }
            else
            {
                return new ValueExpression(GetLiteralTypeUsage(literal).Constant(literal.Value));
            }
        }

        private static TypeUsage GetLiteralTypeUsage(Literal literal)
        {
            PrimitiveType primitiveType = null;

            if (!ClrProviderManifest.Instance.TryGetPrimitiveType(literal.Type, out primitiveType))
            {
                var errCtx = literal.ErrCtx;
                var message = Strings.LiteralTypeNotFoundInMetadata(literal.OriginalValue);
                throw EntitySqlException.Create(errCtx, message, null);
            }
            var literalTypeUsage = TypeHelpers.GetLiteralTypeUsage(primitiveType.PrimitiveTypeKind, literal.IsUnicodeString);

            return literalTypeUsage;
        }

        // <summary>
        // Converts identifier expression (Identifier)
        // </summary>
        private static ExpressionResolution ConvertIdentifier(Node expr, SemanticResolver sr)
        {
            return ConvertIdentifier(((Identifier)expr), false /* leftHandSideOfMemberAccess */, sr);
        }

        private static ExpressionResolution ConvertIdentifier(Identifier identifier, bool leftHandSideOfMemberAccess, SemanticResolver sr)
        {
            return sr.ResolveSimpleName((identifier).Name, leftHandSideOfMemberAccess, identifier.ErrCtx);
        }

        // <summary>
        // Converts member access expression (AST.DotExpr)
        // </summary>
        private static ExpressionResolution ConvertDotExpr(Node expr, SemanticResolver sr)
        {
            var dotExpr = (DotExpr)expr;

            ValueExpression groupKeyResolution;
            if (sr.TryResolveDotExprAsGroupKeyAlternativeName(dotExpr, out groupKeyResolution))
            {
                return groupKeyResolution;
            }

            //
            // If dotExpr.Left is an identifier, then communicate to the resolution mechanism 
            // that the identifier might be an unqualified name in the context of a qualified name.
            // Otherwise convert the expr normally.
            //
            ExpressionResolution leftResolution;
            var leftIdentifier = dotExpr.Left as Identifier;
            if (leftIdentifier != null)
            {
                leftResolution = ConvertIdentifier(leftIdentifier, true /* leftHandSideOfMemberAccess */, sr);
            }
            else
            {
                leftResolution = Convert(dotExpr.Left, sr);
            }

            switch (leftResolution.ExpressionClass)
            {
                case ExpressionResolutionClass.Value:
                    return sr.ResolvePropertyAccess(
                        ((ValueExpression)leftResolution).Value, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);

                case ExpressionResolutionClass.EntityContainer:
                    return sr.ResolveEntityContainerMemberAccess(
                        ((EntityContainerExpression)leftResolution).EntityContainer, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);

                case ExpressionResolutionClass.MetadataMember:
                    return sr.ResolveMetadataMemberAccess(
                        (MetadataMember)leftResolution, dotExpr.Identifier.Name, dotExpr.Identifier.ErrCtx);

                default:
                    var errCtx = dotExpr.Left.ErrCtx;
                    var message = Strings.UnknownExpressionResolutionClass(leftResolution.ExpressionClass);
                    throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        // <summary>
        // Converts paren expression (AST.ParenExpr)
        // </summary>
        private static ExpressionResolution ConvertParenExpr(Node astExpr, SemanticResolver sr)
        {
            var innerExpr = ((ParenExpr)astExpr).Expr;

            //
            // Convert the inner expression.
            // Note that we allow it to be an untyped null: the consumer of this expression will handle it. 
            // The reason to allow untyped nulls is that "(null)" is a common construct for tool-generated eSQL.
            //
            var converted = ConvertValueExpressionAllowUntypedNulls(innerExpr, sr);
            return new ValueExpression(converted);
        }

        // <summary>
        // Converts GROUPPARTITION expression (AST.GroupPartitionExpr).
        // </summary>
        private static ExpressionResolution ConvertGroupPartitionExpr(Node astExpr, SemanticResolver sr)
        {
            var groupAggregateExpr = (GroupPartitionExpr)astExpr;

            DbExpression converted = null;

            //
            // If ast node was annotated in a previous pass, means it contains a ready-to-use expression.
            //
            if (!TryConvertAsResolvedGroupAggregate(groupAggregateExpr, sr, out converted))
            {
                //
                // GROUPPARTITION is allowed only in the context of a group operation provided by a query expression (SELECT ...).
                //
                if (!sr.IsInAnyGroupScope())
                {
                    var errCtx = astExpr.ErrCtx;
                    var message = Strings.GroupPartitionOutOfContext;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // Process aggregate argument.
                //
                DbExpression arg;
                GroupPartitionInfo aggregateInfo;
                using (sr.EnterGroupPartition(groupAggregateExpr, groupAggregateExpr.ErrCtx, out aggregateInfo))
                {
                    //
                    // Convert aggregate argument.
                    //
                    arg = ConvertValueExpressionAllowUntypedNulls(groupAggregateExpr.ArgExpr, sr);
                }

                //
                // Ensure converted GROUPPARTITION argument expression is not untyped null.
                //
                if (arg == null)
                {
                    var errCtx = groupAggregateExpr.ArgExpr.ErrCtx;
                    var message = Strings.ResultingExpressionTypeCannotBeNull;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // Project the argument off the DbGroupAggregate binding.
                //
                DbExpression definition = aggregateInfo.EvaluatingScopeRegion.GroupAggregateBinding.Project(arg);

                if (groupAggregateExpr.DistinctKind
                    == DistinctKind.Distinct)
                {
                    ValidateDistinctProjection(definition.ResultType, groupAggregateExpr.ArgExpr.ErrCtx, null);
                    definition = definition.Distinct();
                }

                //
                // Add aggregate to aggregate list.
                //
                aggregateInfo.AttachToAstNode(sr.GenerateInternalName("groupPartition"), definition);
                aggregateInfo.EvaluatingScopeRegion.GroupAggregateInfos.Add(aggregateInfo);

                //
                // Return stub expression with same edmType as the group aggregate.
                //
                converted = aggregateInfo.AggregateStubExpression;
            }

            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        #region ConvertMethodExpr implementation

        // <summary>
        // Converts invocation expression (AST.MethodExpr)
        // </summary>
        private static ExpressionResolution ConvertMethodExpr(Node expr, SemanticResolver sr)
        {
            return ConvertMethodExpr((MethodExpr)expr, true /* includeInlineFunctions */, sr);
        }

        private static ExpressionResolution ConvertMethodExpr(MethodExpr methodExpr, bool includeInlineFunctions, SemanticResolver sr)
        {
            //
            // Resolve methodExpr.Expr
            //
            ExpressionResolution leftResolution;
            using (sr.TypeResolver.EnterFunctionNameResolution(includeInlineFunctions))
            {
                var simpleFunctionName = methodExpr.Expr as Identifier;
                if (simpleFunctionName != null)
                {
                    leftResolution = sr.ResolveSimpleFunctionName(simpleFunctionName.Name, simpleFunctionName.ErrCtx);
                }
                else
                {
                    //
                    // Convert methodExpr.Expr optionally entering special resolution modes. See ConvertMethodExpr_TryEnter methods for more info.
                    //
                    var dotExpr = methodExpr.Expr as DotExpr;
                    using (ConvertMethodExpr_TryEnterIgnoreEntityContainerNameResolution(dotExpr, sr))
                    {
                        using (ConvertMethodExpr_TryEnterV1ViewGenBackwardCompatibilityResolution(dotExpr, sr))
                        {
                            leftResolution = Convert(methodExpr.Expr, sr);
                        }
                    }
                }
            }

            if (leftResolution.ExpressionClass
                == ExpressionResolutionClass.MetadataMember)
            {
                var metadataMember = (MetadataMember)leftResolution;

                //
                // Try converting as inline function call. If it fails, continue and try to convert as a model-defined function/function import call.
                //
                ValueExpression inlineFunctionCall;
                if (metadataMember.MetadataMemberClass
                    == MetadataMemberClass.InlineFunctionGroup)
                {
                    Debug.Assert(includeInlineFunctions, "includeInlineFunctions must be true, otherwise recursion does not stop");

                    methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxFunction(metadataMember.Name);
                    methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
                    if (TryConvertInlineFunctionCall((InlineFunctionGroup)metadataMember, methodExpr, sr, out inlineFunctionCall))
                    {
                        return inlineFunctionCall;
                    }
                    else
                    {
                        // Make another try ignoring inline functions.
                        return ConvertMethodExpr(methodExpr, false /* includeInlineFunctions */, sr);
                    }
                }

                switch (metadataMember.MetadataMemberClass)
                {
                    case MetadataMemberClass.Type:
                        methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxTypeCtor(metadataMember.Name);
                        methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
                        return ConvertTypeConstructorCall((MetadataType)metadataMember, methodExpr, sr);

                    case MetadataMemberClass.FunctionGroup:
                        methodExpr.ErrCtx.ErrorContextInfo = Strings.CtxFunction(metadataMember.Name);
                        methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier = false;
                        return ConvertModelFunctionCall((MetadataFunctionGroup)metadataMember, methodExpr, sr);

                    default:
                        var errCtx = methodExpr.Expr.ErrCtx;
                        var message = Strings.CannotResolveNameToTypeOrFunction(metadataMember.Name);
                        throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.MethodInvocationNotSupported;
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        // <summary>
        // If methodExpr.Expr is in the form of "Name1.Name2(...)" then ignore entity containers during resolution of the left expression
        // in the context of the invocation: "EntityContainer.EntitySet(...)" is not a valid expression and it should not shadow
        // a potentially valid interpretation as "Namespace.EntityType/Function(...)".
        // </summary>
        private static IDisposable ConvertMethodExpr_TryEnterIgnoreEntityContainerNameResolution(DotExpr leftExpr, SemanticResolver sr)
        {
            return leftExpr != null && leftExpr.Left is Identifier ? sr.EnterIgnoreEntityContainerNameResolution() : null;
        }

        // <summary>
        // If methodExpr.Expr is in the form of "Name1.Name2(...)"
        // and we are in the view generation mode
        // and schema version is less than V2
        // then ignore types in the resolution of Name1.
        // This is needed in order to support the following V1 case:
        // C-space edmType: AdventureWorks.Store
        // S-space edmType: [AdventureWorks.Store].Customer
        // query: select [AdventureWorks.Store].Customer(1, 2, 3) from ...
        // </summary>
        private static IDisposable ConvertMethodExpr_TryEnterV1ViewGenBackwardCompatibilityResolution(DotExpr leftExpr, SemanticResolver sr)
        {
            if (leftExpr != null
                && leftExpr.Left is Identifier
                &&
                (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode ||
                 sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.UserViewGenerationMode))
            {
                var mappingCollection =
                    sr.TypeResolver.Perspective.MetadataWorkspace.GetItemCollection(DataSpace.CSSpace) as StorageMappingItemCollection;

                Debug.Assert(mappingCollection != null, "mappingCollection != null");

                if (mappingCollection.MappingVersion
                    < XmlConstants.EdmVersionForV2)
                {
                    return sr.TypeResolver.EnterBackwardCompatibilityResolution();
                }
            }
            return null;
        }

        // <summary>
        // Attempts to create a <see cref="ValueExpression" /> representing the inline function call.
        // Returns false if <paramref name="methodExpr" />.DistinctKind != <see see="AST.Method.DistinctKind" />.None.
        // Returns false if no one of the overloads matched the given arguments.
        // Throws if given arguments cause overload resolution ambiguity.
        // </summary>
        private static bool TryConvertInlineFunctionCall(
            InlineFunctionGroup inlineFunctionGroup,
            MethodExpr methodExpr,
            SemanticResolver sr,
            out ValueExpression inlineFunctionCall)
        {
            inlineFunctionCall = null;

            //
            // An inline function can't be a group aggregate, so if DistinctKind is specified then it is not an inline function call.
            //
            if (methodExpr.DistinctKind
                != DistinctKind.None)
            {
                return false;
            }

            //
            // Convert function arguments.
            //
            List<TypeUsage> argTypes;
            var args = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);

            //
            // Find function overload match for the given argument types.
            //
            var isAmbiguous = false;
            var overload = SemanticResolver.ResolveFunctionOverloads(
                inlineFunctionGroup.FunctionMetadata,
                argTypes,
                (lambdaOverload) => lambdaOverload.Parameters,
                (varRef) => varRef.ResultType,
                (varRef) => ParameterMode.In,
                false /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more than one overload that matches the given arguments, throw.
            //
            if (isAmbiguous)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.AmbiguousFunctionArguments;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // If null, means no overload matched.
            //
            if (overload == null)
            {
                return false;
            }

            //
            // Convert untyped NULLs in arguments to typed nulls inferred from formals.
            //
            ConvertUntypedNullsInArguments(args, overload.Parameters, (formal) => formal.ResultType);

            inlineFunctionCall = new ValueExpression(overload.GetLambda(sr).Invoke(args));
            return true;
        }

        private static ValueExpression ConvertTypeConstructorCall(MetadataType metadataType, MethodExpr methodExpr, SemanticResolver sr)
        {
            //
            // Ensure edmType has a constructor.
            //
            if (!TypeSemantics.IsComplexType(metadataType.TypeUsage)
                &&
                !TypeSemantics.IsEntityType(metadataType.TypeUsage)
                &&
                !TypeSemantics.IsRelationshipType(metadataType.TypeUsage))
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.InvalidCtorUseOnType(metadataType.TypeUsage.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Abstract types cannot be instantiated.
            //
            if (metadataType.TypeUsage.EdmType.Abstract)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.CannotInstantiateAbstractType(metadataType.TypeUsage.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // DistinctKind must not be specified on a edmType constructor.
            //
            if (methodExpr.DistinctKind
                != DistinctKind.None)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.InvalidDistinctArgumentInCtor;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Convert relationships if present.
            //
            List<DbRelatedEntityRef> relshipExprList = null;
            if (methodExpr.HasRelationships)
            {
                if (!(sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode ||
                      sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.UserViewGenerationMode))
                {
                    var errCtx = methodExpr.Relationships.ErrCtx;
                    var message = Strings.InvalidModeForWithRelationshipClause;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                var driverEntityType = metadataType.TypeUsage.EdmType as EntityType;
                if (driverEntityType == null)
                {
                    var errCtx = methodExpr.Relationships.ErrCtx;
                    var message = Strings.InvalidTypeForWithRelationshipClause;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                var targetEnds = new HashSet<string>();
                relshipExprList = new List<DbRelatedEntityRef>(methodExpr.Relationships.Count);
                for (var i = 0; i < methodExpr.Relationships.Count; i++)
                {
                    var relshipExpr = methodExpr.Relationships[i];

                    var relshipTarget = ConvertRelatedEntityRef(relshipExpr, driverEntityType, sr);

                    var targetEndId = String.Join(
                        ":", new[] { relshipTarget.TargetEnd.DeclaringType.Identity, relshipTarget.TargetEnd.Identity });
                    if (targetEnds.Contains(targetEndId))
                    {
                        var errCtx = relshipExpr.ErrCtx;
                        var message = Strings.RelationshipTargetMustBeUnique(targetEndId);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    targetEnds.Add(targetEndId);

                    relshipExprList.Add(relshipTarget);
                }
            }

            List<TypeUsage> argTypes;
            return new ValueExpression(
                CreateConstructorCallExpression(
                    methodExpr,
                    metadataType.TypeUsage,
                    ConvertFunctionArguments(methodExpr.Args, sr, out argTypes),
                    relshipExprList,
                    sr));
        }

        private static ValueExpression ConvertModelFunctionCall(
            MetadataFunctionGroup metadataFunctionGroup, MethodExpr methodExpr, SemanticResolver sr)
        {
            if (metadataFunctionGroup.FunctionMetadata.Any(f => !f.IsComposableAttribute))
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.CannotCallNoncomposableFunction(metadataFunctionGroup.Name);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Decide if it is an ordinary function or group aggregate
            //
            if (TypeSemantics.IsAggregateFunction(metadataFunctionGroup.FunctionMetadata[0])
                && sr.IsInAnyGroupScope())
            {
                //
                // If it is an aggregate function inside a group scope, dispatch to the expensive ConvertAggregateFunctionInGroupScope()...
                //
                return new ValueExpression(ConvertAggregateFunctionInGroupScope(methodExpr, metadataFunctionGroup, sr));
            }
            else
            {
                //
                // Otherwise, it is just an ordinary function call (including aggregate functions outside of a group scope)
                //
                return new ValueExpression(CreateModelFunctionCallExpression(methodExpr, metadataFunctionGroup, sr));
            }
        }

        #region ConvertAggregateFunctionInGroupScope implementation

        // <summary>
        // Converts group aggregates.
        // </summary>
        // <remarks>
        // This method converts group aggregates in two phases:
        // Phase 1 - it will resolve the actual inner (argument) expression and then annotate the ast node and add the resolved aggregate
        // to the scope
        // Phase 2 - if ast node was annotated, just extract the precomputed expression from the scope.
        // </remarks>
        private static DbExpression ConvertAggregateFunctionInGroupScope(
            MethodExpr methodExpr, MetadataFunctionGroup metadataFunctionGroup, SemanticResolver sr)
        {
            DbExpression converted = null;

            //
            // First, check if methodExpr is already resolved as an aggregate...
            //
            if (TryConvertAsResolvedGroupAggregate(methodExpr, sr, out converted))
            {
                return converted;
            }

            //
            // ... then, try to convert as a collection function.
            //
            // Note that if methodExpr represents a group aggregate, 
            // then the argument conversion performed inside of TryConvertAsCollectionFunction(...) is thrown away.
            // Throwing the argument conversion however is not possible in a clean way as the argument conversion has few side-effects:
            // 1. For each group aggregate within the argument a new GroupAggregateInfo object is created and:
            //    a. Some of the aggregates are assigned to outer scope regions for evaluation, which means their aggregate info objects are
            //         - enlisted in the outer scope regions,
            //         - remain attached to the corresponding AST nodes, see GroupAggregateInfo.AttachToAstNode(...) for more info.
            //       These aggregate info objects will be reused when the aggregates are revisited, see TryConvertAsResolvedGroupAggregate(...) method for more info.
            //    b. The aggregate info objects of closest aggregates are wired to sr.CurrentGroupAggregateInfo object as contained/containing.
            // 2. sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion value is adjusted with all the scope entry references outside of nested aggregates.
            // Hence when the conversion as a collection function fails, these side-effects must be mitigated:
            // (1.a) does not cause any issues.
            // (1.b) requires rewiring which is handled by the GroupAggregateInfo.SetContainingAggregate(...) mechanism invoked by 
            //       TryConvertAsResolvedGroupAggregate(...) method.
            // (2) requires saving and restoring the InnermostReferencedScopeRegion value, which is handled in the code below.
            //
            // Note: we also do a throw-away conversions in other places, such as inline function attempt and processing of projection items in order by clause,
            // but this method is the only place where conversion attempts differ in the way how converted argument expression is processed.
            // This method is the only place that affects sr.CurrentGroupAggregateInfo with regard to the converted argument expression.
            // Hence the side-effect mitigation is needed only here.
            //
            var savedInnermostReferencedScopeRegion = sr.CurrentGroupAggregateInfo != null
                                                          ? sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion
                                                          : null;
            List<TypeUsage> argTypes;
            if (TryConvertAsCollectionFunction(methodExpr, metadataFunctionGroup, sr, out argTypes, out converted))
            {
                return converted;
            }
            else if (sr.CurrentGroupAggregateInfo != null)
            {
                sr.CurrentGroupAggregateInfo.InnermostReferencedScopeRegion = savedInnermostReferencedScopeRegion;
            }
            Debug.Assert(argTypes != null, "argTypes != null");

            //
            // Finally, try to convert as a function group aggregate.
            //
            if (TryConvertAsFunctionAggregate(methodExpr, metadataFunctionGroup, argTypes, sr, out converted))
            {
                return converted;
            }

            //
            // If we reach this point, means the resolution failed.
            //
            var errCtx = methodExpr.ErrCtx;
            var message = Strings.FailedToResolveAggregateFunction(metadataFunctionGroup.Name);
            throw EntitySqlException.Create(errCtx, message, null);
        }

        // <summary>
        // Try to convert as pre resolved group aggregate.
        // </summary>
        private static bool TryConvertAsResolvedGroupAggregate(
            GroupAggregateExpr groupAggregateExpr, SemanticResolver sr, out DbExpression converted)
        {
            converted = null;

            //
            // If ast node was annotated in a previous pass, means it contains a ready-to-use expression,
            // otherwise exit.
            //
            if (groupAggregateExpr.AggregateInfo == null)
            {
                return false;
            }

            //
            // Wire up groupAggregateExpr.AggregateInfo to the sr.CurrentGroupAggregateInfo.
            // This is needed in the following case:  ... select max(x + max(b)) ...
            // The outer max(...) is first processed as collection function, so when the nested max(b) is processed as an aggregate, it does not
            // see the outer function as a containing aggregate, so it does not wire to it. 
            // Later, when the outer max(...) is processed as an aggregate, processing of the inner max(...) gets into TryConvertAsResolvedGroupAggregate(...)
            // and at this point we finally wire up the two aggregates.
            //
            groupAggregateExpr.AggregateInfo.SetContainingAggregate(sr.CurrentGroupAggregateInfo);

            if (
                !sr.TryResolveInternalAggregateName(
                    groupAggregateExpr.AggregateInfo.AggregateName, groupAggregateExpr.AggregateInfo.ErrCtx, out converted))
            {
                Debug.Assert(
                    groupAggregateExpr.AggregateInfo.AggregateStubExpression != null, "Resolved aggregate stub expression must not be null.");
                converted = groupAggregateExpr.AggregateInfo.AggregateStubExpression;
            }

            Debug.Assert(converted != null, "converted != null");

            return true;
        }

        // <summary>
        // Try convert method expr in a group scope as a collection aggregate
        // </summary>
        // <param name="argTypes"> argTypes are returned regardless of the function result </param>
        private static bool TryConvertAsCollectionFunction(
            MethodExpr methodExpr,
            MetadataFunctionGroup metadataFunctionGroup,
            SemanticResolver sr,
            out List<TypeUsage> argTypes,
            out DbExpression converted)
        {
            //
            // Convert aggregate arguments.
            //
            var args = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);

            //
            // Try to see if there is an overload match.
            //
            var isAmbiguous = false;
            var functionType = SemanticResolver.ResolveFunctionOverloads(
                metadataFunctionGroup.FunctionMetadata,
                argTypes,
                false /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more then one overload that matches given arguments, throw.
            //
            if (isAmbiguous)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.AmbiguousFunctionArguments;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // If not null, means a match was found as a collection aggregate (ordinary function).
            //
            if (functionType != null)
            {
                //
                // Convert untyped NULLs in arguments to typed nulls inferred from function parameters.
                //
                ConvertUntypedNullsInArguments(args, functionType.Parameters, (parameter) => parameter.TypeUsage);
                converted = functionType.Invoke(args);
                return true;
            }
            else
            {
                converted = null;
                return false;
            }
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static bool TryConvertAsFunctionAggregate(
            MethodExpr methodExpr,
            MetadataFunctionGroup metadataFunctionGroup,
            List<TypeUsage> argTypes,
            SemanticResolver sr,
            out DbExpression converted)
        {
            DebugCheck.NotNull(argTypes);

            converted = null;

            //
            // Try to find an overload match as group aggregate
            //
            var isAmbiguous = false;
            var functionType = SemanticResolver.ResolveFunctionOverloads(
                metadataFunctionGroup.FunctionMetadata,
                argTypes,
                true /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more then one overload that matches given arguments, throw.
            //
            if (isAmbiguous)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.AmbiguousFunctionArguments;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // If it still null, then there is no overload as a group aggregate function.
            //
            if (null == functionType)
            {
                CqlErrorHelper.ReportFunctionOverloadError(methodExpr, metadataFunctionGroup.FunctionMetadata[0], argTypes);
            }
            //
            // Process aggregate argument.
            //
            List<DbExpression> args;
            FunctionAggregateInfo aggregateInfo;
            using (sr.EnterFunctionAggregate(methodExpr, methodExpr.ErrCtx, out aggregateInfo))
            {
                List<TypeUsage> aggArgTypes;
                args = ConvertFunctionArguments(methodExpr.Args, sr, out aggArgTypes);
                // Sanity check - argument types must agree.
                Debug.Assert(
                    argTypes.Count == aggArgTypes.Count &&
                    argTypes.Zip(aggArgTypes).All(
                        types => types.Key == null && types.Value == null || TypeSemantics.IsStructurallyEqual(types.Key, types.Value)),
                    "argument types resolved for the collection aggregate calls must match");
            }

			//
            // Aggregate functions must have at least one argument, and the first argument must be of collection edmType
            //
            Debug.Assert((1 <= functionType.Parameters.Count), "(1 <= functionType.Parameters.Count)");
            // we only support monadic aggregate functions
            Debug.Assert(
                TypeSemantics.IsCollectionType(functionType.Parameters[0].TypeUsage), "functionType.Parameters[0].Type is CollectionType");

            //
            // Convert untyped NULLs in arguments to typed nulls inferred from function parameters.
            //
            ConvertUntypedNullsInArguments(
                args, functionType.Parameters, (parameter) => TypeHelpers.GetElementTypeUsage(parameter.TypeUsage));

            //
            // Create function aggregate expression.
            //
            DbFunctionAggregate functionAggregate;
            if (methodExpr.DistinctKind
                == DistinctKind.Distinct)
            {
                functionAggregate = functionType.AggregateDistinct(args);
            }
            else
            {
                functionAggregate = functionType.Aggregate(args);
            }

            //
            // Add aggregate to aggregate list.
            //
            aggregateInfo.AttachToAstNode(sr.GenerateInternalName("groupAgg" + functionType.Name), functionAggregate);
            aggregateInfo.EvaluatingScopeRegion.GroupAggregateInfos.Add(aggregateInfo);

            //
            // Return stub expression with same edmType as the aggregate function.
            //
            converted = aggregateInfo.AggregateStubExpression;

            Debug.Assert(converted != null, "converted != null");

            return true;
        }

        #endregion ConvertAggregateFunctionInGroupScope implementation

        // <summary>
        // Creates <see cref="DbExpression" /> representing a new instance of the given edmType.
        // Validates and infers argument types.
        // </summary>
        private static DbExpression CreateConstructorCallExpression(
            MethodExpr methodExpr,
            TypeUsage type,
            List<DbExpression> args,
            List<DbRelatedEntityRef> relshipExprList,
            SemanticResolver sr)
        {
            Debug.Assert(
                TypeSemantics.IsComplexType(type) || TypeSemantics.IsEntityType(type) || TypeSemantics.IsRelationshipType(type),
                "edmType must have a constructor");

            DbExpression newInstance = null;
            var idx = 0;
            var argCount = args.Count;

            //
            // Find overloads by searching members in order of its definition.
            // Each member will be considered as a formal argument edmType in the order of its definition.
            //
            var stype = (StructuralType)type.EdmType;
            foreach (EdmMember member in TypeHelpers.GetAllStructuralMembers(stype))
            {
                var memberModelTypeUsage = Helper.GetModelTypeUsage(member);

                Debug.Assert(memberModelTypeUsage.EdmType.DataSpace == DataSpace.CSpace, "member space must be CSpace");

                //
                // Ensure given arguments are not less than 'formal' constructor arguments.
                //
                if (argCount <= idx)
                {
                    var errCtx = methodExpr.ErrCtx;
                    var message = Strings.NumberOfTypeCtorIsLessThenFormalSpec(member.Name);
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // If the given argument is the untyped null, infer edmType from the ctor formal argument edmType.
                //
                if (args[idx] == null)
                {
                    var edmProperty = member as EdmProperty;
                    if (edmProperty != null
                        && !edmProperty.Nullable)
                    {
                        var errCtx = methodExpr.Args[idx].ErrCtx;
                        var message = Strings.InvalidNullLiteralForNonNullableMember(member.Name, stype.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                    args[idx] = memberModelTypeUsage.Null();
                }

                //
                // Ensure the given argument edmType is promotable to the formal ctor argument edmType.
                //
                var isPromotable = TypeSemantics.IsPromotableTo(args[idx].ResultType, memberModelTypeUsage);
                if (ParserOptions.CompilationMode.RestrictedViewGenerationMode == sr.ParserOptions.ParserCompilationMode
                    ||
                    ParserOptions.CompilationMode.UserViewGenerationMode == sr.ParserOptions.ParserCompilationMode)
                {
                    if (!isPromotable
                        && !TypeSemantics.IsPromotableTo(memberModelTypeUsage, args[idx].ResultType))
                    {
                        var errCtx = methodExpr.Args[idx].ErrCtx;
                        var message = Strings.InvalidCtorArgumentType(
                            args[idx].ResultType.EdmType.FullName,
                            member.Name,
                            memberModelTypeUsage.EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    if (Helper.IsPrimitiveType(memberModelTypeUsage.EdmType)
                        &&
                        !TypeSemantics.IsSubTypeOf(args[idx].ResultType, memberModelTypeUsage))
                    {
                        args[idx] = args[idx].CastTo(memberModelTypeUsage);
                    }
                }
                else
                {
                    if (!isPromotable)
                    {
                        var errCtx = methodExpr.Args[idx].ErrCtx;
                        var message = Strings.InvalidCtorArgumentType(
                            args[idx].ResultType.EdmType.FullName,
                            member.Name,
                            memberModelTypeUsage.EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                }

                idx++;
            }

            //
            // Ensure all given arguments and all ctor formals were considered and properly checked.
            //
            if (idx != argCount)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.NumberOfTypeCtorIsMoreThenFormalSpec(stype.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Finally, create expression
            //
            if (relshipExprList != null
                && relshipExprList.Count > 0)
            {
                var entityType = (EntityType)type.EdmType;
                newInstance = DbExpressionBuilder.CreateNewEntityWithRelationshipsExpression(entityType, args, relshipExprList);
            }
            else
            {
                newInstance = TypeHelpers.GetReadOnlyType(type).New(args);
            }
            Debug.Assert(null != newInstance, "null != newInstance");

            return newInstance;
        }

        // <summary>
        // Creates <see cref="DbFunctionExpression" /> representing a model function call.
        // Validates overloads.
        // </summary>
        private static DbFunctionExpression CreateModelFunctionCallExpression(
            MethodExpr methodExpr,
            MetadataFunctionGroup metadataFunctionGroup,
            SemanticResolver sr)
        {
            DbFunctionExpression functionExpression = null;
            var isAmbiguous = false;

            //
            // DistinctKind must not be specified on a regular function call.
            //
            if (methodExpr.DistinctKind
                != DistinctKind.None)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.InvalidDistinctArgumentInNonAggFunction;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Convert function arguments.
            //
            List<TypeUsage> argTypes;
            var args = ConvertFunctionArguments(methodExpr.Args, sr, out argTypes);

            //
            // Find function overload match for given argument types.
            //
            var functionType = SemanticResolver.ResolveFunctionOverloads(
                metadataFunctionGroup.FunctionMetadata,
                argTypes,
                false /* isGroupAggregateFunction */,
                out isAmbiguous);

            //
            // If there is more than one overload that matches given arguments, throw.
            //
            if (isAmbiguous)
            {
                var errCtx = methodExpr.ErrCtx;
                var message = Strings.AmbiguousFunctionArguments;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // If null, means no overload matched.
            //
            if (null == functionType)
            {
                CqlErrorHelper.ReportFunctionOverloadError(methodExpr, metadataFunctionGroup.FunctionMetadata[0], argTypes);
            }

            //
            // Convert untyped NULLs in arguments to typed nulls inferred from function parameters.
            //
            ConvertUntypedNullsInArguments(args, functionType.Parameters, (parameter) => parameter.TypeUsage);

            //
            // Finally, create expression
            //
            functionExpression = functionType.Invoke(args);

            Debug.Assert(null != functionExpression, "null != functionExpression");

            return functionExpression;
        }

        // <summary>
        // Converts function call arguments into a list of <see cref="DbExpression" />s.
        // In case of no arguments returns an empty list.
        // </summary>
        private static List<DbExpression> ConvertFunctionArguments(
            NodeList<Node> astExprList, SemanticResolver sr, out List<TypeUsage> argTypes)
        {
            var convertedArgs = new List<DbExpression>();

            if (null != astExprList)
            {
                for (var i = 0; i < astExprList.Count; i++)
                {
                    convertedArgs.Add(ConvertValueExpressionAllowUntypedNulls(astExprList[i], sr));
                }
            }

            argTypes = convertedArgs.Select(a => a != null ? a.ResultType : null).ToList();
            return convertedArgs;
        }

        private static void ConvertUntypedNullsInArguments<TParameterMetadata>(
            List<DbExpression> args,
            IList<TParameterMetadata> parametersMetadata,
            Func<TParameterMetadata, TypeUsage> getParameterTypeUsage)
        {
            for (var i = 0; i < args.Count; i++)
            {
                if (args[i] == null)
                {
                    args[i] = DbExpressionBuilder.Null(getParameterTypeUsage(parametersMetadata[i]));
                }
            }
        }

        #endregion ConvertMethodExpr implementation

        // <summary>
        // Converts command parameter reference expression (AST.QueryParameter)
        // </summary>
        private static ExpressionResolution ConvertParameter(Node expr, SemanticResolver sr)
        {
            var parameter = (QueryParameter)expr;

            DbParameterReferenceExpression paramRef;
            if (null == sr.Parameters
                || !sr.Parameters.TryGetValue(parameter.Name, out paramRef))
            {
                var errCtx = parameter.ErrCtx;
                var message = Strings.ParameterWasNotDefined(parameter.Name);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return new ValueExpression(paramRef);
        }

        // <summary>
        // Converts WITH RELATIONSHIP (AST.RelshipNavigationExpr)
        // </summary>
        // <param name="relshipExpr"> the ast expression </param>
        // <param name="driverEntityType"> The entity that is being constructed for with this RELATIONSHIP clause is processed. </param>
        // <param name="sr"> the Semantic Resolver context </param>
        // <returns> a DbRelatedEntityRef instance </returns>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static DbRelatedEntityRef ConvertRelatedEntityRef(
            RelshipNavigationExpr relshipExpr, EntityType driverEntityType, SemanticResolver sr)
        {
            //
            // Resolve relationship edmType name.
            //
            var edmType = ConvertTypeName(relshipExpr.TypeName, sr).EdmType;
            var relationshipType = edmType as RelationshipType;
            if (relationshipType == null)
            {
                var errCtx = relshipExpr.TypeName.ErrCtx;
                var message = Strings.RelationshipTypeExpected(edmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Convert target instance expression.
            //
            var targetEntityRef = ConvertValueExpression(relshipExpr.RefExpr, sr);

            //
            // Make sure it is a ref edmType.
            //
            var refType = targetEntityRef.ResultType.EdmType as RefType;
            if (refType == null)
            {
                var errCtx = relshipExpr.RefExpr.ErrCtx;
                var message = Strings.RelatedEndExprTypeMustBeReference;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Convert To end if explicitly defined, derive if implicit.
            //
            RelationshipEndMember toEnd;
            if (relshipExpr.ToEndIdentifier != null)
            {
                toEnd =
                    (RelationshipEndMember)
                    relationshipType.Members.FirstOrDefault(
                        m => m.Name.Equals(relshipExpr.ToEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (toEnd == null)
                {
                    var errCtx = relshipExpr.ToEndIdentifier.ErrCtx;
                    var message = Strings.InvalidRelationshipMember(relshipExpr.ToEndIdentifier.Name, relationshipType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                //
                // ensure is *..{0|1}
                //
                if (toEnd.RelationshipMultiplicity != RelationshipMultiplicity.One
                    && toEnd.RelationshipMultiplicity != RelationshipMultiplicity.ZeroOrOne)
                {
                    var errCtx = relshipExpr.ToEndIdentifier.ErrCtx;
                    var message = Strings.InvalidWithRelationshipTargetEndMultiplicity(
                        toEnd.Name, toEnd.RelationshipMultiplicity.ToString());
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(refType, toEnd.TypeUsage.EdmType))
                {
                    var errCtx = relshipExpr.RefExpr.ErrCtx;
                    var message = Strings.RelatedEndExprTypeMustBePromotoableToToEnd(refType.FullName, toEnd.TypeUsage.EdmType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                var toEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                      .Where(
                                                          e =>
                                                          TypeSemantics.IsStructurallyEqualOrPromotableTo(refType, e.TypeUsage.EdmType) &&
                                                          (e.RelationshipMultiplicity == RelationshipMultiplicity.One ||
                                                           e.RelationshipMultiplicity == RelationshipMultiplicity.ZeroOrOne)).ToArray();
                switch (toEndCandidates.Length)
                {
                    case 1:
                        toEnd = toEndCandidates[0];
                        break;
                    case 0:
                        var errCtx = relshipExpr.ErrCtx;
                        var message = Strings.InvalidImplicitRelationshipToEnd(relationshipType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    default:
                        var errCtx1 = relshipExpr.ErrCtx;
                        var message1 = Strings.RelationshipToEndIsAmbiguos;
                        throw EntitySqlException.Create(errCtx1, message1, null);
                }
            }
            Debug.Assert(toEnd != null, "toEnd must be resolved.");

            //
            // Convert From end if explicitly defined, derive if implicit.
            //
            RelationshipEndMember fromEnd;
            if (relshipExpr.FromEndIdentifier != null)
            {
                fromEnd =
                    (RelationshipEndMember)
                    relationshipType.Members.FirstOrDefault(
                        m => m.Name.Equals(relshipExpr.FromEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (fromEnd == null)
                {
                    var errCtx = relshipExpr.FromEndIdentifier.ErrCtx;
                    var message = Strings.InvalidRelationshipMember(relshipExpr.FromEndIdentifier.Name, relationshipType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(driverEntityType.GetReferenceType(), fromEnd.TypeUsage.EdmType))
                {
                    var errCtx = relshipExpr.FromEndIdentifier.ErrCtx;
                    var message = Strings.SourceTypeMustBePromotoableToFromEndRelationType(
                        driverEntityType.FullName, fromEnd.TypeUsage.EdmType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                if (fromEnd.EdmEquals(toEnd))
                {
                    var errCtx = relshipExpr.ErrCtx;
                    var message = Strings.RelationshipFromEndIsAmbiguos;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                var fromEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                        .Where(
                                                            e =>
                                                            TypeSemantics.IsStructurallyEqualOrPromotableTo(
                                                                driverEntityType.GetReferenceType(), e.TypeUsage.EdmType) &&
                                                            !e.EdmEquals(toEnd)).ToArray();
                switch (fromEndCandidates.Length)
                {
                    case 1:
                        fromEnd = fromEndCandidates[0];
                        break;
                    case 0:
                        var errCtx = relshipExpr.ErrCtx;
                        var message = Strings.InvalidImplicitRelationshipFromEnd(relationshipType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    default:
                        Debug.Fail("N-ary relationship? N > 2");
                        var errCtx1 = relshipExpr.ErrCtx;
                        var message1 = Strings.RelationshipFromEndIsAmbiguos;
                        throw EntitySqlException.Create(errCtx1, message1, null);
                }
            }
            Debug.Assert(fromEnd != null, "fromEnd must be resolved.");

            return DbExpressionBuilder.CreateRelatedEntityRef(fromEnd, toEnd, targetEntityRef);
        }

        // <summary>
        // Converts relationship navigation expression (AST.RelshipNavigationExpr)
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static ExpressionResolution ConvertRelshipNavigationExpr(Node astExpr, SemanticResolver sr)
        {
            var relshipExpr = (RelshipNavigationExpr)astExpr;

            //
            // Resolve relationship edmType name.
            //
            var edmType = ConvertTypeName(relshipExpr.TypeName, sr).EdmType;
            var relationshipType = edmType as RelationshipType;
            if (relationshipType == null)
            {
                var errCtx = relshipExpr.TypeName.ErrCtx;
                var message = Strings.RelationshipTypeExpected(edmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Convert source instance expression.
            //
            var sourceEntityRef = ConvertValueExpression(relshipExpr.RefExpr, sr);

            //
            // Make sure it is a ref edmType. Convert to ref if possible.
            //
            var sourceRefType = sourceEntityRef.ResultType.EdmType as RefType;
            if (sourceRefType == null)
            {
                var entityType = sourceEntityRef.ResultType.EdmType as EntityType;
                if (entityType != null)
                {
                    sourceEntityRef = sourceEntityRef.GetEntityRef();
                    sourceRefType = (RefType)sourceEntityRef.ResultType.EdmType;
                }
                else
                {
                    var errCtx = relshipExpr.RefExpr.ErrCtx;
                    var message = Strings.RelatedEndExprTypeMustBeReference;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            //
            // Convert To end if explicitly defined. Derive if implicit later, after From end processing.
            //
            RelationshipEndMember toEnd;
            if (relshipExpr.ToEndIdentifier != null)
            {
                toEnd =
                    (RelationshipEndMember)
                    relationshipType.Members.FirstOrDefault(
                        m => m.Name.Equals(relshipExpr.ToEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (toEnd == null)
                {
                    var errCtx = relshipExpr.ToEndIdentifier.ErrCtx;
                    var message = Strings.InvalidRelationshipMember(relshipExpr.ToEndIdentifier.Name, relationshipType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                toEnd = null;
            }

            //
            // Convert From end if explicitly defined, derive if implicit.
            //
            RelationshipEndMember fromEnd;
            if (relshipExpr.FromEndIdentifier != null)
            {
                fromEnd =
                    (RelationshipEndMember)
                    relationshipType.Members.FirstOrDefault(
                        m => m.Name.Equals(relshipExpr.FromEndIdentifier.Name, StringComparison.OrdinalIgnoreCase));
                if (fromEnd == null)
                {
                    var errCtx = relshipExpr.FromEndIdentifier.ErrCtx;
                    var message = Strings.InvalidRelationshipMember(relshipExpr.FromEndIdentifier.Name, relationshipType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(sourceRefType, fromEnd.TypeUsage.EdmType))
                {
                    var errCtx = relshipExpr.FromEndIdentifier.ErrCtx;
                    var message = Strings.SourceTypeMustBePromotoableToFromEndRelationType(
                        sourceRefType.FullName, fromEnd.TypeUsage.EdmType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
                if (toEnd != null
                    && fromEnd.EdmEquals(toEnd))
                {
                    var errCtx = relshipExpr.ErrCtx;
                    var message = Strings.RelationshipFromEndIsAmbiguos;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                var fromEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                        .Where(
                                                            e =>
                                                            TypeSemantics.IsStructurallyEqualOrPromotableTo(
                                                                sourceRefType, e.TypeUsage.EdmType) &&
                                                            (toEnd == null || !e.EdmEquals(toEnd))).ToArray();
                switch (fromEndCandidates.Length)
                {
                    case 1:
                        fromEnd = fromEndCandidates[0];
                        break;
                    case 0:
                        var errCtx = relshipExpr.ErrCtx;
                        var message = Strings.InvalidImplicitRelationshipFromEnd(relationshipType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    default:
                        Debug.Assert(toEnd == null, "N-ary relationship? N > 2");
                        var errCtx1 = relshipExpr.ErrCtx;
                        var message1 = Strings.RelationshipFromEndIsAmbiguos;
                        throw EntitySqlException.Create(errCtx1, message1, null);
                }
            }
            Debug.Assert(fromEnd != null, "fromEnd must be resolved.");

            //
            // Derive To end if implicit.
            //
            if (toEnd == null)
            {
                var toEndCandidates = relationshipType.Members.Select(m => (RelationshipEndMember)m)
                                                      .Where(e => !e.EdmEquals(fromEnd)).ToArray();
                switch (toEndCandidates.Length)
                {
                    case 1:
                        toEnd = toEndCandidates[0];
                        break;
                    case 0:
                        var errCtx = relshipExpr.ErrCtx;
                        var message = Strings.InvalidImplicitRelationshipToEnd(relationshipType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    default:
                        Debug.Fail("N-ary relationship? N > 2");
                        var errCtx1 = relshipExpr.ErrCtx;
                        var message1 = Strings.RelationshipToEndIsAmbiguos;
                        throw EntitySqlException.Create(errCtx1, message1, null);
                }
            }
            Debug.Assert(toEnd != null, "toEnd must be resolved.");

            //
            // Create cqt expression.
            //
            DbExpression converted = sourceEntityRef.Navigate(fromEnd, toEnd);
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        // <summary>
        // Converts REF expression (AST.RefExpr)
        // </summary>
        private static ExpressionResolution ConvertRefExpr(Node astExpr, SemanticResolver sr)
        {
            var refExpr = (RefExpr)astExpr;

            var converted = ConvertValueExpression(refExpr.ArgExpr, sr);

            //
            // check if is entity edmType
            //
            if (!TypeSemantics.IsEntityType(converted.ResultType))
            {
                var errCtx = refExpr.ArgExpr.ErrCtx;
                var message = Strings.RefArgIsNotOfEntityType(converted.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // create ref expression
            //
            converted = converted.GetEntityRef();
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        // <summary>
        // Converts DEREF expression (AST.DerefExpr)
        // </summary>
        private static ExpressionResolution ConvertDeRefExpr(Node astExpr, SemanticResolver sr)
        {
            var deRefExpr = (DerefExpr)astExpr;

            DbExpression converted = null;

            converted = ConvertValueExpression(deRefExpr.ArgExpr, sr);

            //
            // check if return edmType is RefType
            //
            if (!TypeSemantics.IsReferenceType(converted.ResultType))
            {
                var errCtx = deRefExpr.ArgExpr.ErrCtx;
                var message = Strings.DeRefArgIsNotOfRefType(converted.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // create DeRef expression
            //
            converted = converted.Deref();
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        // <summary>
        // Converts CREATEREF expression (AST.CreateRefExpr)
        // </summary>
        private static ExpressionResolution ConvertCreateRefExpr(Node astExpr, SemanticResolver sr)
        {
            var createRefExpr = (CreateRefExpr)astExpr;

            DbExpression converted = null;

            //
            // Convert the entity set, also, ensure that we get back an extent expression
            //
            var entitySetExpr = ConvertValueExpression(createRefExpr.EntitySet, sr) as DbScanExpression;
            if (entitySetExpr == null)
            {
                var errCtx = createRefExpr.EntitySet.ErrCtx;
                var message = Strings.ExprIsNotValidEntitySetForCreateRef;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Ensure that the extent is an entity set
            //
            var entitySet = entitySetExpr.Target as EntitySet;
            if (entitySet == null)
            {
                var errCtx = createRefExpr.EntitySet.ErrCtx;
                var message = Strings.ExprIsNotValidEntitySetForCreateRef;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var keyRowExpression = ConvertValueExpression(createRefExpr.Keys, sr);

            var inputKeyRowType = keyRowExpression.ResultType.EdmType as RowType;
            if (null == inputKeyRowType)
            {
                var errCtx = createRefExpr.Keys.ErrCtx;
                var message = Strings.InvalidCreateRefKeyType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var entityKeyRowType = TypeHelpers.CreateKeyRowType(entitySet.ElementType);

            if (entityKeyRowType.Members.Count
                != inputKeyRowType.Members.Count)
            {
                var errCtx = createRefExpr.Keys.ErrCtx;
                var message = Strings.ImcompatibleCreateRefKeyType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            if (!TypeSemantics.IsStructurallyEqualOrPromotableTo(keyRowExpression.ResultType, TypeUsage.Create(entityKeyRowType)))
            {
                var errCtx = createRefExpr.Keys.ErrCtx;
                var message = Strings.ImcompatibleCreateRefKeyElementType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // if CREATEREF specifies a edmType, resolve and validate the edmType
            //
            if (null != createRefExpr.TypeIdentifier)
            {
                var targetTypeUsage = ConvertTypeName(createRefExpr.TypeIdentifier, sr);

                //
                // ensure edmType is entity
                //
                if (!TypeSemantics.IsEntityType(targetTypeUsage))
                {
                    var errCtx = createRefExpr.TypeIdentifier.ErrCtx;
                    var message = Strings.CreateRefTypeIdentifierMustSpecifyAnEntityType(
                        targetTypeUsage.EdmType.FullName,
                        targetTypeUsage.EdmType.BuiltInTypeKind.ToString());
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                if (!TypeSemantics.IsValidPolymorphicCast(entitySet.ElementType, targetTypeUsage.EdmType))
                {
                    var errCtx = createRefExpr.TypeIdentifier.ErrCtx;
                    var message = Strings.CreateRefTypeIdentifierMustBeASubOrSuperType(
                        entitySet.ElementType.FullName,
                        targetTypeUsage.EdmType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                converted = entitySet.RefFromKey(keyRowExpression, (EntityType)targetTypeUsage.EdmType);
            }
            else
            {
                //
                // finally creates the expression
                //
                converted = entitySet.RefFromKey(keyRowExpression);
            }

            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        // <summary>
        // Converts KEY expression (AST.KeyExpr)
        // </summary>
        private static ExpressionResolution ConvertKeyExpr(Node astExpr, SemanticResolver sr)
        {
            var keyExpr = (KeyExpr)astExpr;

            var converted = ConvertValueExpression(keyExpr.ArgExpr, sr);

            if (TypeSemantics.IsEntityType(converted.ResultType))
            {
                converted = converted.GetEntityRef();
            }
            else if (!TypeSemantics.IsReferenceType(converted.ResultType))
            {
                var errCtx = keyExpr.ArgExpr.ErrCtx;
                var message = Strings.InvalidKeyArgument(converted.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            converted = converted.GetRefKey();
            Debug.Assert(null != converted, "null != converted");

            return new ValueExpression(converted);
        }

        // <summary>
        // Converts a builtin expression (AST.BuiltInExpr).
        // </summary>
        private static ExpressionResolution ConvertBuiltIn(Node astExpr, SemanticResolver sr)
        {
            var bltInExpr = (BuiltInExpr)astExpr;

            var builtInConverter = _builtInExprConverter[bltInExpr.Kind];
            if (builtInConverter == null)
            {
                var message = Strings.UnknownBuiltInAstExpressionType;
                throw new EntitySqlException(message);
            }

            return new ValueExpression(builtInConverter(bltInExpr, sr));
        }

        // <summary>
        // Converts Arithmetic Expressions Args
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertArithmeticArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var operands = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullArithmetic,
                sr);

            if (!TypeSemantics.IsNumericType(operands.Left.ResultType))
            {
                var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                var message = Strings.ExpressionMustBeNumericType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            if (operands.Right != null)
            {
                if (!TypeSemantics.IsNumericType(operands.Right.ResultType))
                {
                    var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                    var message = Strings.ExpressionMustBeNumericType;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                if (null == TypeHelpers.GetCommonTypeUsage(operands.Left.ResultType, operands.Right.ResultType))
                {
                    var errCtx = astBuiltInExpr.ErrCtx;
                    var message = Strings.ArgumentTypesAreIncompatible(
                        operands.Left.ResultType.EdmType.FullName, operands.Right.ResultType.EdmType.FullName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            return operands;
        }

        // <summary>
        // Converts Plus Args - specific case since string edmType is an allowed edmType for '+'
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertPlusOperands(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var operands = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullArithmetic,
                sr);

            if (!TypeSemantics.IsNumericType(operands.Left.ResultType)
                && !TypeSemantics.IsPrimitiveType(operands.Left.ResultType, PrimitiveTypeKind.String))
            {
                var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                var message = Strings.PlusLeftExpressionInvalidType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            if (!TypeSemantics.IsNumericType(operands.Right.ResultType)
                && !TypeSemantics.IsPrimitiveType(operands.Right.ResultType, PrimitiveTypeKind.String))
            {
                var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                var message = Strings.PlusRightExpressionInvalidType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            if (TypeHelpers.GetCommonTypeUsage(operands.Left.ResultType, operands.Right.ResultType) == null)
            {
                var errCtx = astBuiltInExpr.ErrCtx;
                var message = Strings.ArgumentTypesAreIncompatible(
                    operands.Left.ResultType.EdmType.FullName, operands.Right.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return operands;
        }

        // <summary>
        // Converts Logical Expression Args
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertLogicalArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var leftExpr = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg1, sr);
            if (leftExpr == null)
            {
                leftExpr = TypeResolver.BooleanType.Null();
            }

            DbExpression rightExpr = null;
            if (astBuiltInExpr.Arg2 != null)
            {
                rightExpr = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg2, sr);
                if (rightExpr == null)
                {
                    rightExpr = TypeResolver.BooleanType.Null();
                }
            }

            //
            // ensure left expression edmType is boolean
            //
            if (!IsBooleanType(leftExpr.ResultType))
            {
                var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                var message = Strings.ExpressionTypeMustBeBoolean;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // ensure right expression edmType is boolean
            //
            if (null != rightExpr
                && !IsBooleanType(rightExpr.ResultType))
            {
                var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                var message = Strings.ExpressionTypeMustBeBoolean;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        // <summary>
        // Converts Equal Comparison Expression Args
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertEqualCompArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            //
            // convert left and right types and infer null types
            //
            var compArgs = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullComparison,
                sr);

            //
            // ensure both operand types are equal-comparable
            //
            if (!TypeSemantics.IsEqualComparableTo(compArgs.Left.ResultType, compArgs.Right.ResultType))
            {
                var errCtx = astBuiltInExpr.ErrCtx;
                var message = Strings.ArgumentTypesAreIncompatible(
                    compArgs.Left.ResultType.EdmType.FullName, compArgs.Right.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return compArgs;
        }

        // <summary>
        // Converts Order Comparison Expression Args
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertOrderCompArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var compArgs = ConvertValueExpressionsWithUntypedNulls(
                astBuiltInExpr.Arg1,
                astBuiltInExpr.Arg2,
                astBuiltInExpr.ErrCtx,
                () => Strings.InvalidNullComparison,
                sr);

            //
            // ensure both operand types are order-comparable
            //
            if (!TypeSemantics.IsOrderComparableTo(compArgs.Left.ResultType, compArgs.Right.ResultType))
            {
                var errCtx = astBuiltInExpr.ErrCtx;
                var message = Strings.ArgumentTypesAreIncompatible(
                    compArgs.Left.ResultType.EdmType.FullName, compArgs.Right.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return compArgs;
        }

        // <summary>
        // Converts Set Expression Args
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertSetArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            //
            // convert left expression
            //
            var leftExpr = ConvertValueExpression(astBuiltInExpr.Arg1, sr);

            //
            // convert right expression if binary set op kind
            //
            DbExpression rightExpr = null;
            if (null != astBuiltInExpr.Arg2)
            {
                //
                // binary set op
                //

                //
                // make sure left expression edmType is of sequence edmType (ICollection or Extent)
                //
                if (!TypeSemantics.IsCollectionType(leftExpr.ResultType))
                {
                    var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                    var message = Strings.LeftSetExpressionArgsMustBeCollection;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // convert right expression
                //
                rightExpr = ConvertValueExpression(astBuiltInExpr.Arg2, sr);

                //
                // make sure right expression edmType is of sequence edmType (ICollection or Extent)
                //
                if (!TypeSemantics.IsCollectionType(rightExpr.ResultType))
                {
                    var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                    var message = Strings.RightSetExpressionArgsMustBeCollection;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                TypeUsage commonType;
                var leftElemType = TypeHelpers.GetElementTypeUsage(leftExpr.ResultType);
                var rightElemType = TypeHelpers.GetElementTypeUsage(rightExpr.ResultType);
                if (!TypeSemantics.TryGetCommonType(leftElemType, rightElemType, out commonType))
                {
                    CqlErrorHelper.ReportIncompatibleCommonType(astBuiltInExpr.ErrCtx, leftElemType, rightElemType);
                }

                if (astBuiltInExpr.Kind
                    != BuiltInKind.UnionAll)
                {
                    //
                    // ensure left argument is set op comparable
                    //
                    if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(leftExpr.ResultType)))
                    {
                        var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                        var message = Strings.PlaceholderSetArgTypeIsNotEqualComparable(
                            Strings.LocalizedLeft,
                            astBuiltInExpr.Kind.ToString().ToUpperInvariant(),
                            TypeHelpers.GetElementTypeUsage(leftExpr.ResultType).EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    //
                    // ensure right argument is set op comparable
                    //
                    if (!TypeHelpers.IsSetComparableOpType(TypeHelpers.GetElementTypeUsage(rightExpr.ResultType)))
                    {
                        var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                        var message = Strings.PlaceholderSetArgTypeIsNotEqualComparable(
                            Strings.LocalizedRight,
                            astBuiltInExpr.Kind.ToString().ToUpperInvariant(),
                            TypeHelpers.GetElementTypeUsage(rightExpr.ResultType).EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                }
                else
                {
                    if (Helper.IsAssociationType(leftElemType.EdmType))
                    {
                        var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                        var message = Strings.InvalidAssociationTypeForUnion(leftElemType.EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    if (Helper.IsAssociationType(rightElemType.EdmType))
                    {
                        var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                        var message = Strings.InvalidAssociationTypeForUnion(rightElemType.EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                }
            }
            else
            {
                //
                // unary set op
                //

                //
                // make sure expression edmType is of sequence edmType (ICollection or Extent)
                //
                if (!TypeSemantics.IsCollectionType(leftExpr.ResultType))
                {
                    var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                    var message = Strings.InvalidUnarySetOpArgument(astBuiltInExpr.Name);
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // make sure that if is distinct unary operator, arg element edmType must be equal-comparable
                //
                if (astBuiltInExpr.Kind == BuiltInKind.Distinct
                    && !TypeHelpers.IsValidDistinctOpType(TypeHelpers.GetElementTypeUsage(leftExpr.ResultType)))
                {
                    var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                    var message = Strings.ExpressionTypeMustBeEqualComparable;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        // <summary>
        // Converts Set 'IN' expression args
        // </summary>
        // <param name="sr"> SemanticResolver instance relative to a specific typespace/system </param>
        private static Pair<DbExpression, DbExpression> ConvertInExprArgs(BuiltInExpr astBuiltInExpr, SemanticResolver sr)
        {
            var rightExpr = ConvertValueExpression(astBuiltInExpr.Arg2, sr);
            if (!TypeSemantics.IsCollectionType(rightExpr.ResultType))
            {
                var errCtx = astBuiltInExpr.Arg2.ErrCtx;
                var message = Strings.RightSetExpressionArgsMustBeCollection;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var leftExpr = ConvertValueExpressionAllowUntypedNulls(astBuiltInExpr.Arg1, sr);
            if (leftExpr == null)
            {
                //
                // If left expression edmType is null, infer its edmType from the collection element edmType.
                //
                var elementType = TypeHelpers.GetElementTypeUsage(rightExpr.ResultType);
                ValidateTypeForNullExpression(elementType, astBuiltInExpr.Arg1.ErrCtx);
                leftExpr = elementType.Null();
            }

            if (TypeSemantics.IsCollectionType(leftExpr.ResultType))
            {
                var errCtx = astBuiltInExpr.Arg1.ErrCtx;
                var message = Strings.ExpressionTypeMustNotBeCollection;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Ensure that if left and right are typed expressions then their types must be comparable for IN op.
            //
            var commonElemType = TypeHelpers.GetCommonTypeUsage(leftExpr.ResultType, TypeHelpers.GetElementTypeUsage(rightExpr.ResultType));
            if (null == commonElemType
                || !TypeHelpers.IsValidInOpType(commonElemType))
            {
                var errCtx = astBuiltInExpr.ErrCtx;
                var message = Strings.InvalidInExprArgs(leftExpr.ResultType.EdmType.FullName, rightExpr.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return new Pair<DbExpression, DbExpression>(leftExpr, rightExpr);
        }

        private static void ValidateTypeForNullExpression(TypeUsage type, ErrorContext errCtx)
        {
            if (TypeSemantics.IsCollectionType(type))
            {
                var message = Strings.NullLiteralCannotBePromotedToCollectionOfNulls;
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        // <summary>
        // Converts a edmType name.
        // Type name can be represented by
        // - AST.Identifier, such as "Product"
        // - AST.DotExpr, such as "Northwind.Product"
        // - AST.MethodExpr, such as "Edm.Decimal(10,4)", where "10" and "4" are edmType arguments.
        // </summary>
        private static TypeUsage ConvertTypeName(Node typeName, SemanticResolver sr)
        {
            DebugCheck.NotNull(typeName);

            string[] name = null;
            NodeList<Node> typeSpecArgs = null;

            //
            // Process AST.MethodExpr - reduce it to an identifier with edmType spec arguments
            //
            var methodExpr = typeName as MethodExpr;
            if (methodExpr != null)
            {
                typeName = methodExpr.Expr;
                typeName.ErrCtx.ErrorContextInfo = methodExpr.ErrCtx.ErrorContextInfo;
                typeName.ErrCtx.UseContextInfoAsResourceIdentifier = methodExpr.ErrCtx.UseContextInfoAsResourceIdentifier;

                typeSpecArgs = methodExpr.Args;
            }

            //
            // Try as AST.Identifier
            //
            var identifier = typeName as Identifier;
            if (identifier != null)
            {
                name = new[] { identifier.Name };
            }

            //
            // Try as AST.DotExpr
            //
            var dotExpr = typeName as DotExpr;
            if (dotExpr != null
                && dotExpr.IsMultipartIdentifier(out name))
            {
                Debug.Assert(name != null, "name != null for a multipart identifier");
            }

            if (name == null)
            {
                Debug.Fail("Unexpected AST.Node in the edmType name");
                var errCtx = typeName.ErrCtx;
                var message = Strings.InvalidMetadataMemberName;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var metadataMember = sr.ResolveMetadataMemberName(name, typeName.ErrCtx);
            Debug.Assert(metadataMember != null, "metadata member name resolution must not return null");

            switch (metadataMember.MetadataMemberClass)
            {
                case MetadataMemberClass.Type:
                    {
                        var typeUsage = ((MetadataType)metadataMember).TypeUsage;

                        if (typeSpecArgs != null)
                        {
                            typeUsage = ConvertTypeSpecArgs(typeUsage, typeSpecArgs, typeName.ErrCtx);
                        }

                        return typeUsage;
                    }

                case MetadataMemberClass.Namespace:
                    var errCtx = typeName.ErrCtx;
                    var message = Strings.TypeNameNotFound(metadataMember.Name);
                    throw EntitySqlException.Create(errCtx, message, null);

                default:
                    var errCtx1 = typeName.ErrCtx;
                    var message1 = Strings.InvalidMetadataMemberClassResolution(
                        metadataMember.Name, metadataMember.MetadataMemberClassName, MetadataType.TypeClassName);
                    throw EntitySqlException.Create(errCtx1, message1, null);
            }
        }

        private static TypeUsage ConvertTypeSpecArgs(TypeUsage parameterizedType, NodeList<Node> typeSpecArgs, ErrorContext errCtx)
        {
            DebugCheck.NotNull(typeSpecArgs);
            Debug.Assert(typeSpecArgs.Count > 0, "typeSpecArgs must be null or a non-empty list");

            //
            // Type arguments must be literals.
            //
            foreach (var arg in typeSpecArgs)
            {
                if (!(arg is Literal))
                {
                    var errCtx1 = arg.ErrCtx;
                    var message = Strings.TypeArgumentMustBeLiteral;
                    throw EntitySqlException.Create(errCtx1, message, null);
                }
            }

            //
            // The only parameterized edmType supported is Edm.Decimal
            //
            var primitiveType = parameterizedType.EdmType as PrimitiveType;
            if (primitiveType == null
                || primitiveType.PrimitiveTypeKind != PrimitiveTypeKind.Decimal)
            {
                var message = Strings.TypeDoesNotSupportSpec(primitiveType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Edm.Decimal has two optional parameters: precision and scale.
            //
            if (typeSpecArgs.Count > 2)
            {
                var message = Strings.TypeArgumentCountMismatch(primitiveType.FullName, 2);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Get precision value for Edm.Decimal
            //
            byte precision;
            ConvertTypeFacetValue(primitiveType, (Literal)typeSpecArgs[0], DbProviderManifest.PrecisionFacetName, out precision);

            //
            // Get scale value for Edm.Decimal
            //
            byte scale = 0;
            if (typeSpecArgs.Count == 2)
            {
                ConvertTypeFacetValue(primitiveType, (Literal)typeSpecArgs[1], DbProviderManifest.ScaleFacetName, out scale);
            }

            //
            // Ensure P >= S
            //
            if (precision < scale)
            {
                var errCtx1 = typeSpecArgs[0].ErrCtx;
                var message = Strings.PrecisionMustBeGreaterThanScale(precision, scale);
                throw EntitySqlException.Create(errCtx1, message, null);
            }

            return TypeUsage.CreateDecimalTypeUsage(primitiveType, precision, scale);
        }

        private static void ConvertTypeFacetValue(PrimitiveType type, Literal value, string facetName, out byte byteValue)
        {
            var facetDescription = Helper.GetFacet(type.ProviderManifest.GetFacetDescriptions(type), facetName);
            if (facetDescription == null)
            {
                var errCtx = value.ErrCtx;
                var message = Strings.TypeDoesNotSupportFacet(type.FullName, facetName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            if (value.IsNumber
                && Byte.TryParse(value.OriginalValue, out byteValue))
            {
                if (facetDescription.MaxValue.HasValue
                    && byteValue > facetDescription.MaxValue.Value)
                {
                    var errCtx = value.ErrCtx;
                    var message = Strings.TypeArgumentExceedsMax(facetName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                if (facetDescription.MinValue.HasValue
                    && byteValue < facetDescription.MinValue.Value)
                {
                    var errCtx = value.ErrCtx;
                    var message = Strings.TypeArgumentBelowMin(facetName);
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                var errCtx = value.ErrCtx;
                var message = Strings.TypeArgumentIsNotValid;
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        private static TypeUsage ConvertTypeDefinition(Node typeDefinitionExpr, SemanticResolver sr)
        {
            DebugCheck.NotNull(typeDefinitionExpr);

            TypeUsage converted = null;

            var collTypeDefExpr = typeDefinitionExpr as CollectionTypeDefinition;
            var refTypeDefExpr = typeDefinitionExpr as RefTypeDefinition;
            var rowTypeDefExpr = typeDefinitionExpr as RowTypeDefinition;

            if (collTypeDefExpr != null)
            {
                var elementType = ConvertTypeDefinition(collTypeDefExpr.ElementTypeDef, sr);
                converted = TypeHelpers.CreateCollectionTypeUsage(elementType /* readOnly */);
            }
            else if (refTypeDefExpr != null)
            {
                var targetTypeUsage = ConvertTypeName(refTypeDefExpr.RefTypeIdentifier, sr);

                //
                // Ensure edmType is entity
                //
                if (!TypeSemantics.IsEntityType(targetTypeUsage))
                {
                    var errCtx = refTypeDefExpr.RefTypeIdentifier.ErrCtx;
                    var message = Strings.RefTypeIdentifierMustSpecifyAnEntityType(
                        targetTypeUsage.EdmType.FullName,
                        targetTypeUsage.EdmType.BuiltInTypeKind.ToString());
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                converted = TypeHelpers.CreateReferenceTypeUsage((EntityType)targetTypeUsage.EdmType);
            }
            else if (rowTypeDefExpr != null)
            {
                Debug.Assert(
                    rowTypeDefExpr.Properties != null && rowTypeDefExpr.Properties.Count > 0,
                    "rowTypeDefExpr.Properties must be a non-empty collection");

                converted = TypeHelpers.CreateRowTypeUsage(
                    rowTypeDefExpr.Properties.Select(
                        p => new KeyValuePair<string, TypeUsage>(p.Name.Name, ConvertTypeDefinition(p.Type, sr))) /* readOnly */);
            }
            else
            {
                converted = ConvertTypeName(typeDefinitionExpr, sr);
            }

            Debug.Assert(converted != null, "Type definition conversion yielded null");

            return converted;
        }

        // <summary>
        // Converts row constructor expression (AST.RowConstructorExpr)
        // </summary>
        private static ExpressionResolution ConvertRowConstructor(Node expr, SemanticResolver sr)
        {
            var rowExpr = (RowConstructorExpr)expr;

            var rowColumns = new Dictionary<string, TypeUsage>(sr.NameComparer);
            var fieldExprs = new List<DbExpression>(rowExpr.AliasedExprList.Count);

            for (var i = 0; i < rowExpr.AliasedExprList.Count; i++)
            {
                var aliasExpr = rowExpr.AliasedExprList[i];

                var colExpr = ConvertValueExpressionAllowUntypedNulls(aliasExpr.Expr, sr);
                if (colExpr == null)
                {
                    var errCtx = aliasExpr.Expr.ErrCtx;
                    var message = Strings.RowCtorElementCannotBeNull;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                var aliasName = sr.InferAliasName(aliasExpr, colExpr);

                if (rowColumns.ContainsKey(aliasName))
                {
                    if (aliasExpr.Alias != null)
                    {
                        CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName, aliasExpr.Alias.ErrCtx, Strings.InRowCtor);
                    }
                    else
                    {
                        aliasName = sr.GenerateInternalName("autoRowCol");
                    }
                }

                rowColumns.Add(aliasName, colExpr.ResultType);

                fieldExprs.Add(colExpr);
            }

            return new ValueExpression(TypeHelpers.CreateRowTypeUsage(rowColumns /* readOnly */).New(fieldExprs));
        }

        // <summary>
        // Converts multiset constructor expression (AST.MultisetConstructorExpr)
        // </summary>
        private static ExpressionResolution ConvertMultisetConstructor(Node expr, SemanticResolver sr)
        {
            var msetCtor = (MultisetConstructorExpr)expr;

            if (null == msetCtor.ExprList)
            {
                var errCtx = expr.ErrCtx;
                var message = Strings.CannotCreateEmptyMultiset;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var mSetExprs = msetCtor.ExprList.Select(e => ConvertValueExpressionAllowUntypedNulls(e, sr)).ToArray();

            var multisetTypes = mSetExprs.Where(e => e != null).Select(e => e.ResultType).ToArray();

            //
            // Ensure common edmType is not an untyped null.
            //
            if (multisetTypes.Length == 0)
            {
                var errCtx = expr.ErrCtx;
                var message = Strings.CannotCreateMultisetofNulls;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var commonType = TypeHelpers.GetCommonTypeUsage(multisetTypes);

            //
            // Ensure all elems have a common edmType.
            //
            if (commonType == null)
            {
                var errCtx = expr.ErrCtx;
                var message = Strings.MultisetElemsAreNotTypeCompatible;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            commonType = TypeHelpers.GetReadOnlyType(commonType);

            //
            // Fixup untyped nulls.
            //
            for (var i = 0; i < mSetExprs.Length; i++)
            {
                if (mSetExprs[i] == null)
                {
                    ValidateTypeForNullExpression(commonType, msetCtor.ExprList[i].ErrCtx);
                    mSetExprs[i] = commonType.Null();
                }
            }

            return new ValueExpression(TypeHelpers.CreateCollectionTypeUsage(commonType /* readOnly */).New(mSetExprs));
        }

        // <summary>
        // Converts case-when-then expression (AST.CaseExpr)
        // </summary>
        private static ExpressionResolution ConvertCaseExpr(Node expr, SemanticResolver sr)
        {
            var caseExpr = (CaseExpr)expr;

            var whenExprList = new List<DbExpression>(caseExpr.WhenThenExprList.Count);
            var thenExprList = new List<DbExpression>(caseExpr.WhenThenExprList.Count);

            //
            // Convert when/then expressions.
            //
            for (var i = 0; i < caseExpr.WhenThenExprList.Count; i++)
            {
                var whenThenExpr = caseExpr.WhenThenExprList[i];

                var whenExpression = ConvertValueExpression(whenThenExpr.WhenExpr, sr);

                if (!IsBooleanType(whenExpression.ResultType))
                {
                    var errCtx = whenThenExpr.WhenExpr.ErrCtx;
                    var message = Strings.ExpressionTypeMustBeBoolean;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                whenExprList.Add(whenExpression);

                var thenExpression = ConvertValueExpressionAllowUntypedNulls(whenThenExpr.ThenExpr, sr);

                thenExprList.Add(thenExpression);
            }

            //
            // Convert else if present.
            //
            var elseExpr = caseExpr.ElseExpr != null ? ConvertValueExpressionAllowUntypedNulls(caseExpr.ElseExpr, sr) : null;

            //
            // Collect result types from THENs and the ELSE.
            //
            var resultTypes = thenExprList.Where(e => e != null).Select(e => e.ResultType).ToList();
            if (elseExpr != null)
            {
                resultTypes.Add(elseExpr.ResultType);
            }
            if (resultTypes.Count == 0)
            {
                var errCtx = caseExpr.ElseExpr.ErrCtx;
                var message = Strings.InvalidCaseWhenThenNullType;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Derive common return edmType.
            //
            var resultType = TypeHelpers.GetCommonTypeUsage(resultTypes);
            if (resultType == null)
            {
                var errCtx = caseExpr.WhenThenExprList[0].ThenExpr.ErrCtx;
                var message = Strings.InvalidCaseResultTypes;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Fixup untyped nulls
            //
            for (var i = 0; i < thenExprList.Count; i++)
            {
                if (thenExprList[i] == null)
                {
                    ValidateTypeForNullExpression(resultType, caseExpr.WhenThenExprList[i].ThenExpr.ErrCtx);
                    thenExprList[i] = resultType.Null();
                }
            }
            if (elseExpr == null)
            {
                if (caseExpr.ElseExpr == null
                    && TypeSemantics.IsCollectionType(resultType))
                {
                    //
                    // If ELSE was omitted and common return edmType is a collection,
                    // then use empty collection for elseExpr.
                    //
                    elseExpr = resultType.NewEmptyCollection();
                }
                else
                {
                    ValidateTypeForNullExpression(resultType, (caseExpr.ElseExpr ?? caseExpr).ErrCtx);
                    elseExpr = resultType.Null();
                }
            }

            return new ValueExpression(DbExpressionBuilder.Case(whenExprList, thenExprList, elseExpr));
        }

        // <summary>
        // Converts query expression (AST.QueryExpr)
        // </summary>
        private static ExpressionResolution ConvertQueryExpr(Node expr, SemanticResolver sr)
        {
            var queryExpr = (QueryExpr)expr;

            DbExpression converted = null;

            var isRestrictedViewGenerationMode = (ParserOptions.CompilationMode.RestrictedViewGenerationMode
                                                  == sr.ParserOptions.ParserCompilationMode);

            //
            // Validate & Compensate Query
            //
            if (null != queryExpr.HavingClause
                && null == queryExpr.GroupByClause)
            {
                var errCtx = queryExpr.ErrCtx;
                var message = Strings.HavingRequiresGroupClause;
                throw EntitySqlException.Create(errCtx, message, null);
            }
            if (queryExpr.SelectClause.TopExpr != null)
            {
                if (queryExpr.OrderByClause != null
                    && queryExpr.OrderByClause.LimitSubClause != null)
                {
                    var errCtx = queryExpr.SelectClause.TopExpr.ErrCtx;
                    var message = Strings.TopAndLimitCannotCoexist;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                if (queryExpr.OrderByClause != null
                    && queryExpr.OrderByClause.SkipSubClause != null)
                {
                    var errCtx = queryExpr.SelectClause.TopExpr.ErrCtx;
                    var message = Strings.TopAndSkipCannotCoexist;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            //
            // Create Source Scope Region
            //
            using (sr.EnterScopeRegion())
            {
                //
                // Process From Clause
                //
                var sourceExpr = ProcessFromClause(queryExpr.FromClause, sr);

                //
                // Process Where Clause
                //
                sourceExpr = ProcessWhereClause(sourceExpr, queryExpr.WhereClause, sr);

                Debug.Assert(
                    isRestrictedViewGenerationMode ? null == queryExpr.GroupByClause : true,
                    "GROUP BY clause must be null in RestrictedViewGenerationMode");
                Debug.Assert(
                    isRestrictedViewGenerationMode ? null == queryExpr.HavingClause : true,
                    "HAVING clause must be null in RestrictedViewGenerationMode");
                Debug.Assert(
                    isRestrictedViewGenerationMode ? null == queryExpr.OrderByClause : true,
                    "ORDER BY clause must be null in RestrictedViewGenerationMode");

                var queryProjectionProcessed = false;
                if (!isRestrictedViewGenerationMode)
                {
                    //
                    // Process GroupBy Clause
                    //
                    sourceExpr = ProcessGroupByClause(sourceExpr, queryExpr, sr);

                    //
                    // Process Having Clause
                    //
                    sourceExpr = ProcessHavingClause(sourceExpr, queryExpr.HavingClause, sr);

                    //
                    // Process OrderBy Clause
                    //
                    sourceExpr = ProcessOrderByClause(sourceExpr, queryExpr, out queryProjectionProcessed, sr);
                }

                //
                // Process Projection Clause
                //
                converted = ProcessSelectClause(sourceExpr, queryExpr, queryProjectionProcessed, sr);
            } // end query scope region

            return new ValueExpression(converted);
        }

        // <summary>
        // Process Select Clause
        // </summary>
        private static DbExpression ProcessSelectClause(
            DbExpressionBinding source, QueryExpr queryExpr, bool queryProjectionProcessed, SemanticResolver sr)
        {
            var selectClause = queryExpr.SelectClause;

            DbExpression projectExpression;
            if (queryProjectionProcessed)
            {
                projectExpression = source.Expression;
            }
            else
            {
                //
                // Convert projection items.
                //
                var projectionItems = ConvertSelectClauseItems(queryExpr, sr);

                //
                // Create project expression off the projectionItems.
                //
                projectExpression = CreateProjectExpression(source, selectClause, projectionItems);
            }

            //
            // Handle TOP/LIMIT sub-clauses.
            //
            if (selectClause.TopExpr != null
                || (queryExpr.OrderByClause != null && queryExpr.OrderByClause.LimitSubClause != null))
            {
                Node limitExpr;
                string exprName;
                if (selectClause.TopExpr != null)
                {
                    Debug.Assert(
                        queryExpr.OrderByClause == null || queryExpr.OrderByClause.LimitSubClause == null,
                        "TOP and LIMIT in the same query are not allowed");
                    limitExpr = selectClause.TopExpr;
                    exprName = "TOP";
                }
                else
                {
                    limitExpr = queryExpr.OrderByClause.LimitSubClause;
                    exprName = "LIMIT";
                }

                //
                // Convert the expression.
                //
                var convertedLimit = ConvertValueExpression(limitExpr, sr);

                //
                // Ensure the converted expression is in the range of values.
                //
                ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(convertedLimit, limitExpr.ErrCtx, exprName);

                //
                // Create the project expression with the limit.
                //
                projectExpression = projectExpression.Limit(convertedLimit);
            }

            Debug.Assert(null != projectExpression, "null != projectExpression");
            return projectExpression;
        }

        private static List<KeyValuePair<string, DbExpression>> ConvertSelectClauseItems(QueryExpr queryExpr, SemanticResolver sr)
        {
            var selectClause = queryExpr.SelectClause;

            //
            // Validate SELECT VALUE projection list.
            // 
            if (selectClause.SelectKind
                == SelectKind.Value)
            {
                if (selectClause.Items.Count != 1)
                {
                    var errCtx = selectClause.ErrCtx;
                    var message = Strings.InvalidSelectValueList;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // Aliasing is not allowed in the SELECT VALUE case, except when the ORDER BY clause is present.
                //
                if (selectClause.Items[0].Alias != null
                    && queryExpr.OrderByClause == null)
                {
                    var errCtx = selectClause.Items[0].ErrCtx;
                    var message = Strings.InvalidSelectValueAliasedExpression;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            //
            // Converts projection list
            //
            var projectionAliases = new HashSet<string>(sr.NameComparer);
            var projectionItems = new List<KeyValuePair<string, DbExpression>>(selectClause.Items.Count);
            for (var i = 0; i < selectClause.Items.Count; i++)
            {
                var projectionItem = selectClause.Items[i];

                var converted = ConvertValueExpression(projectionItem.Expr, sr);

                //
                // Infer projection item alias.
                //
                var aliasName = sr.InferAliasName(projectionItem, converted);

                //
                // Ensure the alias is not already used.
                //
                if (projectionAliases.Contains(aliasName))
                {
                    if (projectionItem.Alias != null)
                    {
                        CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName, projectionItem.Alias.ErrCtx, Strings.InSelectProjectionList);
                    }
                    else
                    {
                        aliasName = sr.GenerateInternalName("autoProject");
                    }
                }

                projectionAliases.Add(aliasName);
                projectionItems.Add(new KeyValuePair<string, DbExpression>(aliasName, converted));
            }

            Debug.Assert(projectionItems.Count > 0, "projectionItems.Count > 0");
            return projectionItems;
        }

        private static DbExpression CreateProjectExpression(
            DbExpressionBinding source, SelectClause selectClause, List<KeyValuePair<string, DbExpression>> projectionItems)
        {
            //
            // Create DbProjectExpression off the projectionItems.
            //
            DbExpression projectExpression;
            if (selectClause.SelectKind
                == SelectKind.Value)
            {
                Debug.Assert(projectionItems.Count == 1, "projectionItems.Count must be 1 for SELECT VALUE");
                projectExpression = source.Project(projectionItems[0].Value);
            }
            else
            {
                projectExpression = source.Project(DbExpressionBuilder.NewRow(projectionItems));
            }

            //
            // Handle DISTINCT modifier - create DbDistinctExpression over the current projectExpression.
            //
            if (selectClause.DistinctKind
                == DistinctKind.Distinct)
            {
                //
                // Ensure element edmType is equal-comparable.
                //
                ValidateDistinctProjection(projectExpression.ResultType, selectClause);

                //
                // Create distinct expression.
                //
                projectExpression = projectExpression.Distinct();
            }

            return projectExpression;
        }

        private static void ValidateDistinctProjection(TypeUsage projectExpressionResultType, SelectClause selectClause)
        {
            ValidateDistinctProjection(
                projectExpressionResultType,
                selectClause.Items[0].Expr.ErrCtx,
                selectClause.SelectKind == SelectKind.Row
                    ? new List<ErrorContext>(selectClause.Items.Select(item => item.Expr.ErrCtx))
                    : null);
        }

        private static void ValidateDistinctProjection(
            TypeUsage projectExpressionResultType, ErrorContext defaultErrCtx, List<ErrorContext> projectionItemErrCtxs)
        {
            var projectionType = TypeHelpers.GetElementTypeUsage(projectExpressionResultType);
            if (!TypeHelpers.IsValidDistinctOpType(projectionType))
            {
                var errCtx = defaultErrCtx;
                if (projectionItemErrCtxs != null
                    && TypeSemantics.IsRowType(projectionType))
                {
                    var rowType = projectionType.EdmType as RowType;
                    Debug.Assert(projectionItemErrCtxs.Count == rowType.Members.Count);
                    for (var i = 0; i < rowType.Members.Count; i++)
                    {
                        if (!TypeHelpers.IsValidDistinctOpType(rowType.Members[i].TypeUsage))
                        {
                            errCtx = projectionItemErrCtxs[i];
                            break;
                        }
                    }
                }
                var message = Strings.SelectDistinctMustBeEqualComparable;
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        private static void ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(
            DbExpression expr, ErrorContext errCtx, string exprName)
        {
            if (expr.ExpressionKind != DbExpressionKind.Constant
                &&
                expr.ExpressionKind != DbExpressionKind.ParameterReference)
            {
                var message = Strings.PlaceholderExpressionMustBeConstant(exprName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            if (!TypeSemantics.IsPromotableTo(expr.ResultType, TypeResolver.Int64Type))
            {
                var message = Strings.PlaceholderExpressionMustBeCompatibleWithEdm64(exprName, expr.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            var constExpr = expr as DbConstantExpression;
            if (constExpr != null
                && System.Convert.ToInt64(constExpr.Value, CultureInfo.InvariantCulture) < 0)
            {
                var message = Strings.PlaceholderExpressionMustBeGreaterThanOrEqualToZero(exprName);
                throw EntitySqlException.Create(errCtx, message, null);
            }
        }

        // <summary>
        // Process FROM clause.
        // </summary>
        private static DbExpressionBinding ProcessFromClause(FromClause fromClause, SemanticResolver sr)
        {
            DbExpressionBinding fromBinding = null;

            //
            // Process each FROM clause item.
            // If there is more than one of them, then assemble them in a string from APPLYs.
            //
            var fromClauseEntries = new List<SourceScopeEntry>();
            for (var i = 0; i < fromClause.FromClauseItems.Count; i++)
            {
                //
                // Convert FROM clause item.
                //
                List<SourceScopeEntry> fromClauseItemEntries;
                var currentItemBinding = ProcessFromClauseItem(fromClause.FromClauseItems[i], sr, out fromClauseItemEntries);
                fromClauseEntries.AddRange(fromClauseItemEntries);

                if (fromBinding == null)
                {
                    fromBinding = currentItemBinding;
                }
                else
                {
                    fromBinding = fromBinding.CrossApply(currentItemBinding).BindAs(sr.GenerateInternalName("lcapply"));

                    //
                    // Adjust scope entries with the new binding.
                    //
                    fromClauseEntries.Each(scopeEntry => scopeEntry.AddParentVar(fromBinding.Variable));
                }
            }

            Debug.Assert(fromBinding != null, "fromBinding != null");

            return fromBinding;
        }

        // <summary>
        // Process generic FROM clause item: aliasedExpr, JoinClauseItem or ApplyClauseItem.
        // Returns <see cref="DbExpressionBinding" /> and the <paramref name="scopeEntries" /> list with entries created by the clause item.
        // </summary>
        private static DbExpressionBinding ProcessFromClauseItem(
            FromClauseItem fromClauseItem, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding fromItemBinding = null;

            switch (fromClauseItem.FromClauseItemKind)
            {
                case FromClauseItemKind.AliasedFromClause:
                    fromItemBinding = ProcessAliasedFromClauseItem((AliasedExpr)fromClauseItem.FromExpr, sr, out scopeEntries);
                    break;

                case FromClauseItemKind.JoinFromClause:
                    fromItemBinding = ProcessJoinClauseItem((JoinClauseItem)fromClauseItem.FromExpr, sr, out scopeEntries);
                    break;

                default:
                    Debug.Assert(
                        fromClauseItem.FromClauseItemKind == FromClauseItemKind.ApplyFromClause,
                        "AST.FromClauseItemKind.ApplyFromClause expected");
                    fromItemBinding = ProcessApplyClauseItem((ApplyClauseItem)fromClauseItem.FromExpr, sr, out scopeEntries);
                    break;
            }

            Debug.Assert(fromItemBinding != null, "fromItemBinding != null");

            return fromItemBinding;
        }

        // <summary>
        // Process a simple FROM clause item.
        // Returns <see cref="DbExpressionBinding" /> and the <paramref name="scopeEntries" /> list with a single entry created for the clause item.
        // </summary>
        private static DbExpressionBinding ProcessAliasedFromClauseItem(
            AliasedExpr aliasedExpr, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding aliasedBinding = null;

            //
            // Convert the item expression.
            //
            var converted = ConvertValueExpression(aliasedExpr.Expr, sr);

            //
            // Validate it is of collection edmType.
            //
            if (!TypeSemantics.IsCollectionType(converted.ResultType))
            {
                var errCtx = aliasedExpr.Expr.ErrCtx;
                var message = Strings.ExpressionMustBeCollection;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Infer source var alias name.
            //
            var aliasName = sr.InferAliasName(aliasedExpr, converted);

            //
            // Validate the name was not used yet.
            //
            if (sr.CurrentScope.Contains(aliasName))
            {
                if (aliasedExpr.Alias != null)
                {
                    CqlErrorHelper.ReportAliasAlreadyUsedError(aliasName, aliasedExpr.Alias.ErrCtx, Strings.InFromClause);
                }
                else
                {
                    aliasName = sr.GenerateInternalName("autoFrom");
                }
            }

            //
            // Create CQT expression.
            //
            aliasedBinding = converted.BindAs(aliasName);

            //
            // Add source var to the _scopeEntries list and to the current scope.
            //
            var sourceScopeEntry = new SourceScopeEntry(aliasedBinding.Variable);
            sr.CurrentScope.Add(aliasedBinding.Variable.VariableName, sourceScopeEntry);
            scopeEntries = new List<SourceScopeEntry>();
            scopeEntries.Add(sourceScopeEntry);

            Debug.Assert(aliasedBinding != null, "aliasedBinding != null");

            return aliasedBinding;
        }

        // <summary>
        // Process a JOIN clause item.
        // Returns <see cref="DbExpressionBinding" /> and the <paramref name="scopeEntries" /> list with a join-left and join-right entries created for the clause item.
        // </summary>
        private static DbExpressionBinding ProcessJoinClauseItem(
            JoinClauseItem joinClause, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding joinBinding = null;

            //
            // Make sure inner join has ON predicate AND cross join has no ON predicate.
            //
            if (null == joinClause.OnExpr)
            {
                if (JoinKind.Inner
                    == joinClause.JoinKind)
                {
                    var errCtx = joinClause.ErrCtx;
                    var message = Strings.InnerJoinMustHaveOnPredicate;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }
            else
            {
                if (JoinKind.Cross
                    == joinClause.JoinKind)
                {
                    var errCtx = joinClause.OnExpr.ErrCtx;
                    var message = Strings.InvalidPredicateForCrossJoin;
                    throw EntitySqlException.Create(errCtx, message, null);
                }
            }

            //
            // Process left expression.
            //
            List<SourceScopeEntry> leftExprScopeEntries;
            var leftBindingExpr = ProcessFromClauseItem(joinClause.LeftExpr, sr, out leftExprScopeEntries);

            //
            // Mark scope entries from the left expression as such. This will disallow their usage inside of the right expression.
            // The left and right expressions of a join must be independent (they can not refer to variables in the other expression).
            // Join ON predicate may refer to variables defined in both expressions.
            // Examples:
            //     Select ... From A JOIN B JOIN A.x             -> invalid
            //     Select ... From A JOIN B JOIN C ON A.x = C.x  -> valid
            //     Select ... From A JOIN B, C JOIN A.x ...      -> valid
            //
            leftExprScopeEntries.Each(scopeEntry => scopeEntry.IsJoinClauseLeftExpr = true);

            //
            // Process right expression
            //
            List<SourceScopeEntry> rightExprScopeEntries;
            var rightBindingExpr = ProcessFromClauseItem(joinClause.RightExpr, sr, out rightExprScopeEntries);

            //
            // Unmark scope entries from the left expression to allow their usage.
            //
            leftExprScopeEntries.Each(scopeEntry => scopeEntry.IsJoinClauseLeftExpr = false);

            //
            // Switch right outer to left outer.
            //
            if (joinClause.JoinKind
                == JoinKind.RightOuter)
            {
                joinClause.JoinKind = JoinKind.LeftOuter;
                var tmpExpr = leftBindingExpr;
                leftBindingExpr = rightBindingExpr;
                rightBindingExpr = tmpExpr;
            }

            //
            // Resolve JoinType.
            //
            var joinKind = MapJoinKind(joinClause.JoinKind);

            //
            // Resolve ON.
            //
            DbExpression onExpr = null;
            if (null == joinClause.OnExpr)
            {
                if (DbExpressionKind.CrossJoin != joinKind)
                {
                    onExpr = DbExpressionBuilder.True;
                }
            }
            else
            {
                onExpr = ConvertValueExpression(joinClause.OnExpr, sr);
            }

            //
            // Create New Join
            //
            joinBinding =
                DbExpressionBuilder.CreateJoinExpressionByKind(
                    joinKind, onExpr, leftBindingExpr, rightBindingExpr).BindAs(sr.GenerateInternalName("join"));

            //
            // Combine left and right scope entries and adjust with the new binding.
            //
            scopeEntries = leftExprScopeEntries;
            scopeEntries.AddRange(rightExprScopeEntries);
            scopeEntries.Each(scopeEntry => scopeEntry.AddParentVar(joinBinding.Variable));

            Debug.Assert(joinBinding != null, "joinBinding != null");

            return joinBinding;
        }

        // <summary>
        // Maps <see cref="AST.JoinKind" /> to <see cref="DbExpressionKind" />.
        // </summary>
        private static DbExpressionKind MapJoinKind(JoinKind joinKind)
        {
            Debug.Assert(joinKind != JoinKind.RightOuter, "joinKind != JoinKind.RightOuter");
            return _joinMap[(int)joinKind];
        }

        private static readonly DbExpressionKind[] _joinMap =
            {
                DbExpressionKind.CrossJoin, DbExpressionKind.InnerJoin,
                DbExpressionKind.LeftOuterJoin, DbExpressionKind.FullOuterJoin
            };

        // <summary>
        // Process an APPLY clause item.
        // Returns <see cref="DbExpressionBinding" /> and the <paramref name="scopeEntries" /> list with an apply-left and apply-right entries created for the clause item.
        // </summary>
        private static DbExpressionBinding ProcessApplyClauseItem(
            ApplyClauseItem applyClause, SemanticResolver sr, out List<SourceScopeEntry> scopeEntries)
        {
            DbExpressionBinding applyBinding = null;

            //
            // Resolve left expression.
            //
            List<SourceScopeEntry> leftExprScopeEntries;
            var leftBindingExpr = ProcessFromClauseItem(applyClause.LeftExpr, sr, out leftExprScopeEntries);

            //
            // Resolve right expression.
            //
            List<SourceScopeEntry> rightExprScopeEntries;
            var rightBindingExpr = ProcessFromClauseItem(applyClause.RightExpr, sr, out rightExprScopeEntries);

            //
            // Create Apply.
            //
            applyBinding =
                DbExpressionBuilder.CreateApplyExpressionByKind(
                    MapApplyKind(applyClause.ApplyKind),
                    leftBindingExpr,
                    rightBindingExpr).BindAs(sr.GenerateInternalName("apply"));

            //
            // Combine left and right scope entries and adjust with the new binding.
            //
            scopeEntries = leftExprScopeEntries;
            scopeEntries.AddRange(rightExprScopeEntries);
            scopeEntries.Each(scopeEntry => scopeEntry.AddParentVar(applyBinding.Variable));

            Debug.Assert(applyBinding != null, "applyBinding != null");

            return applyBinding;
        }

        // <summary>
        // Maps <see cref="AST.ApplyKind" /> to <see cref="DbExpressionKind" />.
        // </summary>
        private static DbExpressionKind MapApplyKind(ApplyKind applyKind)
        {
            return _applyMap[(int)applyKind];
        }

        private static readonly DbExpressionKind[] _applyMap = { DbExpressionKind.CrossApply, DbExpressionKind.OuterApply };

        // <summary>
        // Process WHERE clause.
        // </summary>
        private static DbExpressionBinding ProcessWhereClause(DbExpressionBinding source, Node whereClause, SemanticResolver sr)
        {
            if (whereClause == null)
            {
                return source;
            }
            return ProcessWhereHavingClausePredicate(source, whereClause, whereClause.ErrCtx, "where", sr);
        }

        // <summary>
        // Process HAVING clause.
        // </summary>
        private static DbExpressionBinding ProcessHavingClause(DbExpressionBinding source, HavingClause havingClause, SemanticResolver sr)
        {
            if (havingClause == null)
            {
                return source;
            }
            return ProcessWhereHavingClausePredicate(source, havingClause.HavingPredicate, havingClause.ErrCtx, "having", sr);
        }

        // <summary>
        // Process WHERE or HAVING clause predicate.
        // </summary>
        private static DbExpressionBinding ProcessWhereHavingClausePredicate(
            DbExpressionBinding source, Node predicate, ErrorContext errCtx, string bindingNameTemplate, SemanticResolver sr)
        {
            DebugCheck.NotNull(predicate);

            DbExpressionBinding whereBinding = null;

            //
            // Convert the predicate.
            //
            var filterConditionExpr = ConvertValueExpression(predicate, sr);

            //
            // Ensure the predicate edmType is boolean.
            //
            if (!IsBooleanType(filterConditionExpr.ResultType))
            {
                var message = Strings.ExpressionTypeMustBeBoolean;
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // Create new filter binding.
            //
            whereBinding = source.Filter(filterConditionExpr).BindAs(sr.GenerateInternalName(bindingNameTemplate));

            //
            // Fixup Bindings.
            //
            sr.CurrentScopeRegion.ApplyToScopeEntries(
                scopeEntry =>
                {
                    Debug.Assert(
                        scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef,
                        "scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef");

                    if (scopeEntry.EntryKind
                        == ScopeEntryKind.SourceVar)
                    {
                        ((SourceScopeEntry)scopeEntry).ReplaceParentVar(whereBinding.Variable);
                    }
                });

            Debug.Assert(whereBinding != null, "whereBinding != null");

            return whereBinding;
        }

        // <summary>
        // Process Group By Clause
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static DbExpressionBinding ProcessGroupByClause(DbExpressionBinding source, QueryExpr queryExpr, SemanticResolver sr)
        {
            var groupByClause = queryExpr.GroupByClause;

            Debug.Assert(
                (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode)
                    ? null == groupByClause
                    : true, "GROUP BY clause must be null in RestrictedViewGenerationMode");

            //
            // If group expression is null, assume an implicit group and speculate that there are group aggregates in the remaining query expression.
            // If no group aggregate are found after partial evaluation of HAVING, ORDER BY and SELECT, rollback the implicit group.
            //
            var groupKeysCount = groupByClause != null ? groupByClause.GroupItems.Count : 0;
            var isImplicitGroup = groupKeysCount == 0;
            if (isImplicitGroup && !queryExpr.HasMethodCall)
            {
                return source;
            }

            //
            // Create input binding for DbGroupByExpression.
            //
            var groupInputBinding = source.Expression.GroupBindAs(sr.GenerateInternalName("geb"), sr.GenerateInternalName("group"));

            //
            // Create group partition (DbGroupAggregate) and projection template.
            //
            var groupAggregateDefinition = groupInputBinding.GroupAggregate;
            var groupAggregateVarRef = groupAggregateDefinition.ResultType.Variable(sr.GenerateInternalName("groupAggregate"));
            var groupAggregateBinding = groupAggregateVarRef.BindAs(sr.GenerateInternalName("groupPartitionItem"));

            //
            // Flag that we perform group operation.
            //
            sr.CurrentScopeRegion.EnterGroupOperation(groupAggregateBinding);

            //
            // Update group input bindings.
            //
            sr.CurrentScopeRegion.ApplyToScopeEntries(
                (scopeEntry) =>
                {
                    Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar, "scopeEntry.EntryKind == ScopeEntryKind.SourceVar");
                    ((SourceScopeEntry)scopeEntry).AdjustToGroupVar(
                        groupInputBinding.Variable, groupInputBinding.GroupVariable, groupAggregateBinding.Variable);
                });

            //
            // This set will include names of keys, aggregates and the group partition name if specified.
            // All these properties become field names of the row edmType returned by the DbGroupByExpression.
            //
            var groupPropertyNames = new HashSet<string>(sr.NameComparer);

            //
            // Convert group keys.
            //

            #region Convert group key definitions

            var groupKeys = new List<GroupKeyInfo>(groupKeysCount);
            if (!isImplicitGroup)
            {
                Debug.Assert(null != groupByClause, "groupByClause must not be null at this point");
                for (var i = 0; i < groupKeysCount; i++)
                {
                    var aliasedExpr = groupByClause.GroupItems[i];

                    sr.CurrentScopeRegion.WasResolutionCorrelated = false;

                    //
                    // Convert key expression relative to groupInputBinding.Variable.
                    // This expression will be used as key definition during construction of DbGroupByExpression.
                    //
                    DbExpression keyExpr;
                    GroupKeyAggregateInfo groupKeyAggregateInfo;
                    using (sr.EnterGroupKeyDefinition(GroupAggregateKind.GroupKey, aliasedExpr.ErrCtx, out groupKeyAggregateInfo))
                    {
                        keyExpr = ConvertValueExpression(aliasedExpr.Expr, sr);
                    }

                    //
                    // Ensure group key expression is correlated.
                    // If resolution was correlated, then the following should be true for groupKeyAggregateInfo: ESR == DSR
                    //
                    if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
                    {
                        var errCtx = aliasedExpr.Expr.ErrCtx;
                        var message = Strings.KeyMustBeCorrelated("GROUP BY");
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                    Debug.Assert(
                        groupKeyAggregateInfo.EvaluatingScopeRegion == groupKeyAggregateInfo.DefiningScopeRegion,
                        "Group key must evaluate on the scope it was defined on.");

                    //
                    // Ensure key is valid.
                    //
                    if (!TypeHelpers.IsValidGroupKeyType(keyExpr.ResultType))
                    {
                        var errCtx = aliasedExpr.Expr.ErrCtx;
                        var message = Strings.GroupingKeysMustBeEqualComparable;
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    //
                    // Convert key expression relative to groupInputBinding.GroupVariable.
                    // keyExprForFunctionAggregates will be used inside of definitions of group aggregates resolved to the current scope region.
                    //
                    DbExpression keyExprForFunctionAggregates;
                    GroupKeyAggregateInfo functionAggregateInfo;
                    using (sr.EnterGroupKeyDefinition(GroupAggregateKind.Function, aliasedExpr.ErrCtx, out functionAggregateInfo))
                    {
                        keyExprForFunctionAggregates = ConvertValueExpression(aliasedExpr.Expr, sr);
                    }
                    Debug.Assert(
                        functionAggregateInfo.EvaluatingScopeRegion == functionAggregateInfo.DefiningScopeRegion,
                        "Group key must evaluate on the scope it was defined on.");

                    //
                    // Convert key expression relative to groupAggregateBinding.Variable.
                    // keyExprForGroupPartitions will be used inside of definitions of GROUPPARTITION aggregates resolved to the current scope region.
                    //
                    DbExpression keyExprForGroupPartitions;
                    GroupKeyAggregateInfo groupPartitionInfo;
                    using (sr.EnterGroupKeyDefinition(GroupAggregateKind.Partition, aliasedExpr.ErrCtx, out groupPartitionInfo))
                    {
                        keyExprForGroupPartitions = ConvertValueExpression(aliasedExpr.Expr, sr);
                    }
                    Debug.Assert(
                        groupPartitionInfo.EvaluatingScopeRegion == groupPartitionInfo.DefiningScopeRegion,
                        "Group key must evaluate on the scope it was defined on.");

                    //
                    // Infer group key alias name.
                    //
                    var groupKeyAlias = sr.InferAliasName(aliasedExpr, keyExpr);

                    //
                    // Check if alias was already used.
                    //
                    if (groupPropertyNames.Contains(groupKeyAlias))
                    {
                        if (aliasedExpr.Alias != null)
                        {
                            CqlErrorHelper.ReportAliasAlreadyUsedError(groupKeyAlias, aliasedExpr.Alias.ErrCtx, Strings.InGroupClause);
                        }
                        else
                        {
                            groupKeyAlias = sr.GenerateInternalName("autoGroup");
                        }
                    }

                    //
                    // Add alias to dictionary.
                    //
                    groupPropertyNames.Add(groupKeyAlias);

                    //
                    // Add key to keys collection.
                    //
                    var groupKeyInfo = new GroupKeyInfo(groupKeyAlias, keyExpr, keyExprForFunctionAggregates, keyExprForGroupPartitions);
                    groupKeys.Add(groupKeyInfo);

                    //
                    // Group keys should be visible by their 'original' key expression name. The following three forms should be allowed:
                    //   SELECT k       FROM ... as p GROUP BY p.Price as k (explicit key alias) - handled above by InferAliasName()
                    //   SELECT Price   FROM ... as p GROUP BY p.Price      (implicit alias - leading name) - handled above by InferAliasName()
                    //   SELECT p.Price FROM ... as p GROUP BY p.Price      (original key expression) - case handled in the code bellow
                    //
                    if (aliasedExpr.Alias == null)
                    {
                        var dotExpr = aliasedExpr.Expr as DotExpr;
                        string[] alternativeName;
                        if (null != dotExpr
                            && dotExpr.IsMultipartIdentifier(out alternativeName))
                        {
                            groupKeyInfo.AlternativeName = alternativeName;

                            var alternativeFullName = TypeResolver.GetFullName(alternativeName);
                            if (groupPropertyNames.Contains(alternativeFullName))
                            {
                                CqlErrorHelper.ReportAliasAlreadyUsedError(alternativeFullName, dotExpr.ErrCtx, Strings.InGroupClause);
                            }

                            groupPropertyNames.Add(alternativeFullName);
                        }
                    }
                }
            }

            #endregion

            //
            // Save scope. It will be used to rollback the temporary group scope created below.
            //
            var groupInputScope = sr.CurrentScopeIndex;

            //
            // Push temporary group scope.
            //
            sr.EnterScope();

            //
            // Add scope entries for group keys and the group partition to the current scope,
            // this is needed for the aggregate search phase during which keys may be referenced.
            //
            foreach (var groupKeyInfo in groupKeys)
            {
                sr.CurrentScope.Add(
                    groupKeyInfo.Name,
                    new GroupKeyDefinitionScopeEntry(
                        groupKeyInfo.VarBasedKeyExpr,
                        groupKeyInfo.GroupVarBasedKeyExpr,
                        groupKeyInfo.GroupAggBasedKeyExpr,
                        null));

                if (groupKeyInfo.AlternativeName != null)
                {
                    var strAlternativeName = TypeResolver.GetFullName(groupKeyInfo.AlternativeName);
                    sr.CurrentScope.Add(
                        strAlternativeName,
                        new GroupKeyDefinitionScopeEntry(
                            groupKeyInfo.VarBasedKeyExpr,
                            groupKeyInfo.GroupVarBasedKeyExpr,
                            groupKeyInfo.GroupAggBasedKeyExpr,
                            groupKeyInfo.AlternativeName));
                }
            }

            //
            // Convert/Search Aggregates
            // since aggregates can be defined in Having, OrderBy and/or Select clauses must be resolved as part of the group expression.
            // The resolution of these clauses result in potential collection of resolved group aggregates and the actual resulting
            // expression is ignored. These clauses will be then resolved as usual on a second pass.
            //

            #region Search for group aggregates (functions and GROUPPARTITIONs)

            //
            // Search for aggregates in HAVING clause.
            //
            if (null != queryExpr.HavingClause
                && queryExpr.HavingClause.HasMethodCall)
            {
                ConvertValueExpression(queryExpr.HavingClause.HavingPredicate, sr);
            }

            //
            // Search for aggregates in SELECT clause.
            //
            Dictionary<string, DbExpression> projectionExpressions = null;
            if (null != queryExpr.OrderByClause
                || queryExpr.SelectClause.HasMethodCall)
            {
                projectionExpressions = new Dictionary<string, DbExpression>(queryExpr.SelectClause.Items.Count, sr.NameComparer);
                for (var i = 0; i < queryExpr.SelectClause.Items.Count; i++)
                {
                    var aliasedExpr = queryExpr.SelectClause.Items[i];

                    //
                    // Convert projection item expression.
                    //
                    var converted = ConvertValueExpression(aliasedExpr.Expr, sr);

                    //
                    // Create Null Expression with actual edmType.
                    //
                    converted = converted.ExpressionKind == DbExpressionKind.Null ? converted : converted.ResultType.Null();

                    //
                    // Infer alias.
                    //
                    var aliasName = sr.InferAliasName(aliasedExpr, converted);

                    if (projectionExpressions.ContainsKey(aliasName))
                    {
                        if (aliasedExpr.Alias != null)
                        {
                            CqlErrorHelper.ReportAliasAlreadyUsedError(
                                aliasName,
                                aliasedExpr.Alias.ErrCtx,
                                Strings.InSelectProjectionList);
                        }
                        else
                        {
                            aliasName = sr.GenerateInternalName("autoProject");
                        }
                    }

                    projectionExpressions.Add(aliasName, converted);
                }
            }

            //
            // Search for aggregates in ORDER BY clause.
            //
            if (null != queryExpr.OrderByClause
                && queryExpr.OrderByClause.HasMethodCall)
            {
                //
                // Push temporary projection scope.
                //
                sr.EnterScope();

                //
                // Add projection items to the temporary scope (items may be used in ORDER BY).
                //
                foreach (var kvp in projectionExpressions)
                {
                    sr.CurrentScope.Add(kvp.Key, new ProjectionItemDefinitionScopeEntry(kvp.Value));
                }

                //
                // Search for aggregates in ORDER BY clause.
                //
                for (var i = 0; i < queryExpr.OrderByClause.OrderByClauseItem.Count; i++)
                {
                    var orderItem = queryExpr.OrderByClause.OrderByClauseItem[i];

                    sr.CurrentScopeRegion.WasResolutionCorrelated = false;

                    ConvertValueExpression(orderItem.OrderExpr, sr);

                    //
                    // Ensure key expression is correlated.
                    //
                    if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
                    {
                        var errCtx = orderItem.ErrCtx;
                        var message = Strings.KeyMustBeCorrelated("ORDER BY");
                        throw EntitySqlException.Create(errCtx, message, null);
                    }
                }

                //
                // Pop temporary projection scope.
                //
                sr.LeaveScope();
            }

            #endregion

            //
            // If we introduced a fake group but did not find any group aggregates
            // on the first pass, then there is no need for creating an implicit group.
            // Rollback to the status before entering ProcessGroupByClause().
            // If we did find group aggregates, make sure all non-group aggregate function
            // expressions refer to group scope variables only.
            //
            if (isImplicitGroup)
            {
                if (0 == sr.CurrentScopeRegion.GroupAggregateInfos.Count)
                {
                    #region Implicit Group Rollback

                    //
                    // Rollback the temporary group scope.
                    //
                    sr.RollbackToScope(groupInputScope);

                    //
                    // Undo any group source fixups: re-applying the source var and remove the group var.
                    //
                    sr.CurrentScopeRegion.ApplyToScopeEntries(
                        (scopeEntry) =>
                        {
                            Debug.Assert(
                                scopeEntry.EntryKind == ScopeEntryKind.SourceVar, "scopeEntry.EntryKind == ScopeEntryKind.SourceVar");
                            ((SourceScopeEntry)scopeEntry).RollbackAdjustmentToGroupVar(source.Variable);
                        });

                    //
                    // Remove the group operation flag.
                    //
                    sr.CurrentScopeRegion.RollbackGroupOperation();

                    #endregion

                    //
                    // Return the original source var binding.
                    //
                    return source;
                }
            }

            //
            // Prepare list of aggregate definitions and their internal names.
            //
            var aggregates = new List<KeyValuePair<string, DbAggregate>>(sr.CurrentScopeRegion.GroupAggregateInfos.Count);
            var groupPartitionRefFound = false;
            foreach (var groupAggregateInfo in sr.CurrentScopeRegion.GroupAggregateInfos)
            {
                switch (groupAggregateInfo.AggregateKind)
                {
                    case GroupAggregateKind.Function:
                        aggregates.Add(
                            new KeyValuePair<string, DbAggregate>(
                                groupAggregateInfo.AggregateName,
                                ((FunctionAggregateInfo)groupAggregateInfo).AggregateDefinition));
                        break;

                    case GroupAggregateKind.Partition:
                        groupPartitionRefFound = true;
                        break;

                    default:
                        Debug.Fail("Unexpected group aggregate kind:" + groupAggregateInfo.AggregateKind.ToString());
                        break;
                }
            }
            if (groupPartitionRefFound)
            {
                //
                // Add DbAggregate to support GROUPPARTITION definitions.
                //
                aggregates.Add(new KeyValuePair<string, DbAggregate>(groupAggregateVarRef.VariableName, groupAggregateDefinition));
            }

            //
            // Create GroupByExpression and a binding to it.
            //
            var groupBy = groupInputBinding.GroupBy(
                groupKeys.Select(keyInfo => new KeyValuePair<string, DbExpression>(keyInfo.Name, keyInfo.VarBasedKeyExpr)),
                aggregates);
            var groupBinding = groupBy.BindAs(sr.GenerateInternalName("group"));

            //
            // If there are GROUPPARTITION expressions, then add an extra projection off the groupBinding to
            //  - project all the keys and aggregates, except the DbGroupAggregate,
            //  - project definitions of GROUPPARTITION expressions.
            //
            if (groupPartitionRefFound)
            {
                //
                // All GROUPPARTITION definitions reference groupAggregateVarRef, make sure the variable is properly defined in the groupBy expression.
                //
                Debug.Assert(
                    aggregates.Any((aggregate) => String.CompareOrdinal(aggregate.Key, groupAggregateVarRef.VariableName) == 0),
                    "DbAggregate is not defined");

                //
                // Get projection of GROUPPARTITION definitions.
                // This method may return null if all GROUPPARTITION definitions are reduced to the value of groupAggregateVarRef.
                //
                var projectionItems = ProcessGroupPartitionDefinitions(
                    sr.CurrentScopeRegion.GroupAggregateInfos,
                    groupAggregateVarRef,
                    groupBinding);

                if (projectionItems != null)
                {
                    //
                    // Project group keys along with GROUPPARTITION definitions.
                    //
                    projectionItems.AddRange(
                        groupKeys.Select(
                            keyInfo =>
                            new KeyValuePair<string, DbExpression>(keyInfo.Name, groupBinding.Variable.Property(keyInfo.Name))));

                    // 
                    // Project function group aggregates along with GROUPPARTITION definitions and group keys.
                    //
                    projectionItems.AddRange(
                        sr.CurrentScopeRegion.GroupAggregateInfos
                          .Where(groupAggregateInfo => groupAggregateInfo.AggregateKind == GroupAggregateKind.Function)
                          .Select(
                              groupAggregateInfo => new KeyValuePair<string, DbExpression>(
                                                        groupAggregateInfo.AggregateName,
                                                        groupBinding.Variable.Property(groupAggregateInfo.AggregateName))));

                    DbExpression projectExpression = DbExpressionBuilder.NewRow(projectionItems);
                    groupBinding = groupBinding.Project(projectExpression).BindAs(sr.GenerateInternalName("groupPartitionDefs"));
                }
            }

            //
            // Remove the temporary group scope with group key definitions,
            // Replace all existing pre-group scope entries with InvalidGroupInputRefScopeEntry stubs - 
            // they are no longer available for proper referencing and only to be used for user error messages.
            //
            sr.RollbackToScope(groupInputScope);
            sr.CurrentScopeRegion.ApplyToScopeEntries(
                (scopeEntry) =>
                {
                    Debug.Assert(scopeEntry.EntryKind == ScopeEntryKind.SourceVar, "scopeEntry.EntryKind == ScopeEntryKind.SourceVar");
                    return new InvalidGroupInputRefScopeEntry();
                });

            //
            // Add final group scope.
            //
            sr.EnterScope();

            //
            // Add group keys to the group scope.
            //
            foreach (var groupKeyInfo in groupKeys)
            {
                //
                // Add new scope entry 
                //
                sr.CurrentScope.Add(
                    groupKeyInfo.VarRef.VariableName,
                    new SourceScopeEntry(groupKeyInfo.VarRef).AddParentVar(groupBinding.Variable));

                //
                // Handle the alternative name entry.
                //
                if (groupKeyInfo.AlternativeName != null)
                {
                    //
                    // We want two scope entries with keys as groupKeyInfo.VarRef.VariableName and groupKeyInfo.AlternativeName, 
                    // both pointing to the same variable (groupKeyInfo.VarRef).
                    //
                    var strAlternativeName = TypeResolver.GetFullName(groupKeyInfo.AlternativeName);
                    sr.CurrentScope.Add(
                        strAlternativeName,
                        new SourceScopeEntry(groupKeyInfo.VarRef, groupKeyInfo.AlternativeName).AddParentVar(groupBinding.Variable));
                }
            }

            //
            // Add group aggregates to the scope.
            //
            foreach (var groupAggregateInfo in sr.CurrentScopeRegion.GroupAggregateInfos)
            {
                var aggVarRef = groupAggregateInfo.AggregateStubExpression.ResultType.Variable(groupAggregateInfo.AggregateName);

                Debug.Assert(
                    !sr.CurrentScope.Contains(aggVarRef.VariableName) ||
                    groupAggregateInfo.AggregateKind == GroupAggregateKind.Partition,
                    "DbFunctionAggregate's with duplicate names are not allowed.");

                if (!sr.CurrentScope.Contains(aggVarRef.VariableName))
                {
                    sr.CurrentScope.Add(
                        aggVarRef.VariableName,
                        new SourceScopeEntry(aggVarRef).AddParentVar(groupBinding.Variable));
                    sr.CurrentScopeRegion.RegisterGroupAggregateName(aggVarRef.VariableName);
                }

                //
                // Cleanup the stub expression as it must not be used after this point.
                //
                groupAggregateInfo.AggregateStubExpression = null;
            }

            return groupBinding;
        }

        // <summary>
        // Generates the list of projections for GROUPPARTITION definitions.
        // All GROUPPARTITION definitions over the trivial projection of input are reduced to the value of groupAggregateVarRef,
        // only one projection item is created for such definitions.
        // Returns null if all GROUPPARTITION definitions are reduced to the value of groupAggregateVarRef.
        // </summary>
        private static List<KeyValuePair<string, DbExpression>> ProcessGroupPartitionDefinitions(
            List<GroupAggregateInfo> groupAggregateInfos,
            DbVariableReferenceExpression groupAggregateVarRef,
            DbExpressionBinding groupBinding)
        {
            var gpExpressionLambdaVariables = new ReadOnlyCollection<DbVariableReferenceExpression>(
                new[] { groupAggregateVarRef });

            var groupPartitionDefinitions = new List<KeyValuePair<string, DbExpression>>();
            var foundTrivialGroupAggregateProjection = false;
            foreach (var groupAggregateInfo in groupAggregateInfos)
            {
                if (groupAggregateInfo.AggregateKind
                    == GroupAggregateKind.Partition)
                {
                    var groupPartitionInfo = (GroupPartitionInfo)groupAggregateInfo;
                    var aggregateDefinition = groupPartitionInfo.AggregateDefinition;
                    if (IsTrivialInputProjection(groupAggregateVarRef, aggregateDefinition))
                    {
                        //
                        // Reduce the case of the trivial projection of input to the value of groupAggregateVarRef.
                        //
                        groupAggregateInfo.AggregateName = groupAggregateVarRef.VariableName;
                        foundTrivialGroupAggregateProjection = true;
                    }
                    else
                    {
                        //
                        // Build a projection item for the non-trivial definition.
                        //
                        var gpExpressionLambda = new DbLambda(gpExpressionLambdaVariables, groupPartitionInfo.AggregateDefinition);
                        groupPartitionDefinitions.Add(
                            new KeyValuePair<string, DbExpression>(
                                groupAggregateInfo.AggregateName,
                                gpExpressionLambda.Invoke(groupBinding.Variable.Property(groupAggregateVarRef.VariableName))));
                    }
                }
            }

            if (foundTrivialGroupAggregateProjection)
            {
                if (groupPartitionDefinitions.Count > 0)
                {
                    //
                    // Add projection item for groupAggregateVarRef if there are reduced definitions.
                    //
                    groupPartitionDefinitions.Add(
                        new KeyValuePair<string, DbExpression>(
                            groupAggregateVarRef.VariableName,
                            groupBinding.Variable.Property(groupAggregateVarRef.VariableName)));
                }
                else
                {
                    //
                    // If all GROUPPARTITION definitions have been reduced, return null.
                    // In this case the wrapping projection will not be created and 
                    // groupAggregateVarRef will be projected directly from the DbGroupByExpression.
                    //
                    groupPartitionDefinitions = null;
                }
            }

            return groupPartitionDefinitions;
        }

        // <summary>
        // Returns true if lambda accepts a collection variable and trivially projects out its elements.
        // </summary>
        private static bool IsTrivialInputProjection(DbVariableReferenceExpression lambdaVariable, DbExpression lambdaBody)
        {
            if (lambdaBody.ExpressionKind
                != DbExpressionKind.Project)
            {
                return false;
            }
            var projectExpression = (DbProjectExpression)lambdaBody;

            if (projectExpression.Input.Expression != lambdaVariable)
            {
                return false;
            }

            Debug.Assert(TypeSemantics.IsCollectionType(lambdaVariable.ResultType));

            if (projectExpression.Projection.ExpressionKind
                == DbExpressionKind.VariableReference)
            {
                var projectionExpression = (DbVariableReferenceExpression)projectExpression.Projection;
                return projectionExpression == projectExpression.Input.Variable;
            }
            else if (projectExpression.Projection.ExpressionKind == DbExpressionKind.NewInstance
                     &&
                     TypeSemantics.IsRowType(projectExpression.Projection.ResultType))
            {
                if (!TypeSemantics.IsEqual(projectExpression.Projection.ResultType, projectExpression.Input.Variable.ResultType))
                {
                    return false;
                }

                var inputVariableTypeProperties = TypeHelpers.GetAllStructuralMembers(projectExpression.Input.Variable.ResultType);

                var projectionExpression = (DbNewInstanceExpression)projectExpression.Projection;

                Debug.Assert(
                    projectionExpression.Arguments.Count == inputVariableTypeProperties.Count,
                    "projectionExpression.Arguments.Count == inputVariableTypeProperties.Count");
                for (var i = 0; i < projectionExpression.Arguments.Count; ++i)
                {
                    if (projectionExpression.Arguments[i].ExpressionKind
                        != DbExpressionKind.Property)
                    {
                        return false;
                    }
                    var propertyRef = (DbPropertyExpression)projectionExpression.Arguments[i];

                    if (propertyRef.Instance != projectExpression.Input.Variable
                        ||
                        propertyRef.Property != inputVariableTypeProperties[i])
                    {
                        return false;
                    }
                }

                return true;
            }

            return false;
        }

        private sealed class GroupKeyInfo
        {
            internal GroupKeyInfo(
                string name, DbExpression varBasedKeyExpr, DbExpression groupVarBasedKeyExpr, DbExpression groupAggBasedKeyExpr)
            {
                Name = name;
                VarRef = varBasedKeyExpr.ResultType.Variable(name);
                VarBasedKeyExpr = varBasedKeyExpr;
                GroupVarBasedKeyExpr = groupVarBasedKeyExpr;
                GroupAggBasedKeyExpr = groupAggBasedKeyExpr;
            }

            // <summary>
            // The primary name of the group key. It is used to refer to the key from other expressions.
            // </summary>
            internal readonly string Name;

            // <summary>
            // Optional alternative name of the group key.
            // Used to support the following scenario:
            // SELECT Price, p.Price   FROM ... as p GROUP BY p.Price
            // In this case the group key Name is "Price" and the AlternativeName is "p.Price" as if it is coming as an escaped identifier.
            // </summary>
            internal string[] AlternativeName
            {
                get { return _alternativeName; }
                set
                {
                    Debug.Assert(_alternativeName == null, "GroupKeyInfo.AlternativeName can not be reset");
                    _alternativeName = value;
                }
            }

            private string[] _alternativeName;

            internal readonly DbVariableReferenceExpression VarRef;

            internal readonly DbExpression VarBasedKeyExpr;

            internal readonly DbExpression GroupVarBasedKeyExpr;

            internal readonly DbExpression GroupAggBasedKeyExpr;
        }

        // <summary>
        // Process ORDER BY clause.
        // </summary>
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        private static DbExpressionBinding ProcessOrderByClause(
            DbExpressionBinding source, QueryExpr queryExpr, out bool queryProjectionProcessed, SemanticResolver sr)
        {
            Debug.Assert(
                (sr.ParserOptions.ParserCompilationMode == ParserOptions.CompilationMode.RestrictedViewGenerationMode)
                    ? null == queryExpr.OrderByClause
                    : true, "ORDER BY clause must be null in RestrictedViewGenerationMode");

            queryProjectionProcessed = false;

            if (queryExpr.OrderByClause == null)
            {
                return source;
            }

            DbExpressionBinding sortBinding = null;
            var orderByClause = queryExpr.OrderByClause;
            var selectClause = queryExpr.SelectClause;

            //
            // Convert SKIP sub-clause if exists before adding projection expressions to the scope.
            //
            DbExpression convertedSkip = null;

            #region

            if (orderByClause.SkipSubClause != null)
            {
                //
                // Convert the skip expression.
                //
                convertedSkip = ConvertValueExpression(orderByClause.SkipSubClause, sr);

                //
                // Ensure the converted expression is in the range of values.
                //
                ValidateExpressionIsCommandParamOrNonNegativeIntegerConstant(convertedSkip, orderByClause.SkipSubClause.ErrCtx, "SKIP");
            }

            #endregion

            //
            // Convert SELECT clause items before processing the rest of the ORDER BY clause:
            //      - If it is the SELECT DISTINCT case:
            //          SELECT clause item definitions will be used to create DbDistinctExpression, which becomes the new source expression.
            //          Sort keys can only reference:
            //              a. SELECT clause items by their aliases (only these aliases are projected by the new source expression),
            //              b. entries from outer scopes.
            //      - Otherwise:
            //          Sort keys may references any available scope entries, including SELECT clause items.
            //          If a sort key references a SELECT clause item, the item _definition_ will be used as the sort key definition (not a variable ref).
            //
            var projectionItems = ConvertSelectClauseItems(queryExpr, sr);

            if (selectClause.DistinctKind
                == DistinctKind.Distinct)
            {
                //
                // SELECT DISTINCT ... ORDER BY case:
                //      - All scope entries created below SELECT DISTINCT are not valid above it in this query, even for error messages, so remove them.
                //      - The scope entries created by SELECT DISTINCT (the SELECT clause items) will be added to a temporary scope in the code below,
                //        this will make them available for sort keys.
                //
                sr.CurrentScopeRegion.RollbackAllScopes();
            }

            //
            // Create temporary scope for SELECT clause items and add the items to the scope.
            //
            var savedScope = sr.CurrentScopeIndex;
            sr.EnterScope();
            projectionItems.Each(
                projectionItem => sr.CurrentScope.Add(projectionItem.Key, new ProjectionItemDefinitionScopeEntry(projectionItem.Value)));

            //
            // Process SELECT DISTINCT ... ORDER BY case:
            //      - create projection expression: new Row(SELECT clause item defintions) or just the single SELECT clause item defintion;
            //      - create DbDistinctExpression over the projection expression;
            //      - set source expression to the binding to the distinct.
            //
            if (selectClause.DistinctKind
                == DistinctKind.Distinct)
            {
                //
                // Create distinct projection expression and bind to it.
                //
                var projectExpression = CreateProjectExpression(source, selectClause, projectionItems);
                Debug.Assert(projectExpression is DbDistinctExpression, "projectExpression is DbDistinctExpression");
                source = projectExpression.BindAs(sr.GenerateInternalName("distinct"));

                //
                // Replace SELECT clause item definitions with regular source scope entries pointing into the new source binding.
                //
                if (selectClause.SelectKind
                    == SelectKind.Value)
                {
                    Debug.Assert(projectionItems.Count == 1, "projectionItems.Count == 1");
                    sr.CurrentScope.Replace(projectionItems[0].Key, new SourceScopeEntry(source.Variable));
                }
                else
                {
                    Debug.Assert(selectClause.SelectKind == SelectKind.Row, "selectClause.SelectKind == AST.SelectKind.Row");
                    foreach (var projectionExpression in projectionItems)
                    {
                        var projectionExpressionRef = projectionExpression.Value.ResultType.Variable(projectionExpression.Key);

                        sr.CurrentScope.Replace(
                            projectionExpressionRef.VariableName,
                            new SourceScopeEntry(projectionExpressionRef).AddParentVar(source.Variable));
                    }
                }

                //
                // At this point source contains all projected items, so query processing is mostly complete,
                // the only task remaining is processing of TOP/LIMIT subclauses, which happens in ProcessSelectClause(...) method.
                //
                queryProjectionProcessed = true;
            }

            //
            // Convert sort keys.
            //
            var sortKeys = new List<DbSortClause>(orderByClause.OrderByClauseItem.Count);

            #region

            for (var i = 0; i < orderByClause.OrderByClauseItem.Count; i++)
            {
                var orderClauseItem = orderByClause.OrderByClauseItem[i];

                sr.CurrentScopeRegion.WasResolutionCorrelated = false;

                //
                // Convert order key expression.
                //
                var keyExpr = ConvertValueExpression(orderClauseItem.OrderExpr, sr);

                //
                // Ensure key expression is correlated.
                //
                if (!sr.CurrentScopeRegion.WasResolutionCorrelated)
                {
                    var errCtx = orderClauseItem.ErrCtx;
                    var message = Strings.KeyMustBeCorrelated("ORDER BY");
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // Ensure key is order comparable.
                //
                if (!TypeHelpers.IsValidSortOpKeyType(keyExpr.ResultType))
                {
                    var errCtx = orderClauseItem.OrderExpr.ErrCtx;
                    var message = Strings.OrderByKeyIsNotOrderComparable;
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                //
                // Convert order direction.
                //
                var ascSort = (orderClauseItem.OrderKind == OrderKind.None) || (orderClauseItem.OrderKind == OrderKind.Asc);

                //
                // Convert collation.
                //
                string collation = null;
                if (orderClauseItem.Collation != null)
                {
                    if (!IsStringType(keyExpr.ResultType))
                    {
                        var errCtx = orderClauseItem.OrderExpr.ErrCtx;
                        var message = Strings.InvalidKeyTypeForCollation(keyExpr.ResultType.EdmType.FullName);
                        throw EntitySqlException.Create(errCtx, message, null);
                    }

                    collation = orderClauseItem.Collation.Name;
                }

                //
                // Finish key conversion and add converted keys to key collection.
                //
                if (string.IsNullOrEmpty(collation))
                {
                    sortKeys.Add(ascSort ? keyExpr.ToSortClause() : keyExpr.ToSortClauseDescending());
                }
                else
                {
                    sortKeys.Add(ascSort ? keyExpr.ToSortClause(collation) : keyExpr.ToSortClauseDescending(collation));
                }
            }

            #endregion

            //
            // Remove the temporary projection scope with all the SELECT clause items on it.
            //
            sr.RollbackToScope(savedScope);

            //
            // Create sort expression.
            //
            DbExpression sortSourceExpr = null;
            if (convertedSkip != null)
            {
                sortSourceExpr = source.Skip(sortKeys, convertedSkip);
            }
            else
            {
                sortSourceExpr = source.Sort(sortKeys);
            }

            //
            // Create Sort Binding.
            //
            sortBinding = sortSourceExpr.BindAs(sr.GenerateInternalName("sort"));

            //
            // Fixup Bindings.
            //
            if (queryProjectionProcessed)
            {
                Debug.Assert(
                    sr.CurrentScopeIndex < sr.CurrentScopeRegion.FirstScopeIndex, "Current scope region is expected to have no scopes.");

                /*
                 * The following code illustrates definition of the projected output in the case of DISTINCT ORDER BY.
                 * There is nothing above this point that should reference any scope entries produced by this query, 
                 * so we do not really add them to the scope region (hence the code is commented out).
                 * 

                //
                // All the scopes of this current scope region have been rolled back.
                // Add new scope with all the projected items on it.
                //
                sr.EnterScope();
                if (selectClause.SelectKind == AST.SelectKind.SelectRow)
                {
                    foreach (var projectionExpression in projectionItems)
                    {
                        DbVariableReferenceExpression projectionExpressionRef = projectionExpression.Value.ResultType.Variable(projectionExpression.Key);
                        sr.CurrentScope.Add(projectionExpressionRef.VariableName, 
                            new SourceScopeEntry(projectionExpressionRef).AddParentVar(sortBinding.Variable));
                    }
                }
                else
                {
                    Debug.Assert(selectClause.SelectKind == AST.SelectKind.SelectValue, "selectClause.SelectKind == AST.SelectKind.SelectValue");
                    Debug.Assert(projectionItems.Count == 1, "projectionItems.Count == 1");

                    sr.CurrentScope.Add(projectionItems[0].Key, new SourceScopeEntry(sortBinding.Variable));
                }*/
            }
            else
            {
                sr.CurrentScopeRegion.ApplyToScopeEntries(
                    scopeEntry =>
                    {
                        Debug.Assert(
                            scopeEntry.EntryKind == ScopeEntryKind.SourceVar
                            || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef,
                            "scopeEntry.EntryKind == ScopeEntryKind.SourceVar || scopeEntry.EntryKind == ScopeEntryKind.InvalidGroupInputRef");

                        if (scopeEntry.EntryKind
                            == ScopeEntryKind.SourceVar)
                        {
                            ((SourceScopeEntry)scopeEntry).ReplaceParentVar(sortBinding.Variable);
                        }
                    });
            }

            Debug.Assert(null != sortBinding, "null != sortBinding");

            return sortBinding;
        }

        // <summary>
        // Convert "x in multiset(y1, y2, ..., yn)" into
        // x = y1 or x = y2 or x = y3 ...
        // </summary>
        // <param name="left"> left-expression (the probe) </param>
        // <param name="right"> right expression (the collection) </param>
        // <returns> Or tree of equality comparisons </returns>
        private static DbExpression ConvertSimpleInExpression(DbExpression left, DbExpression right)
        {
            // Only handle cases when the right-side is a new instance expression
            Debug.Assert(right.ExpressionKind == DbExpressionKind.NewInstance, "right.ExpressionKind == DbExpressionKind.NewInstance");
            var rightColl = (DbNewInstanceExpression)right;

            if (rightColl.Arguments.Count == 0)
            {
                return DbExpressionBuilder.False;
            }

            var predicates = rightColl.Arguments.Select(arg => left.Equal(arg));
            var args = new List<DbExpression>(predicates);
            var orExpr = Helpers.BuildBalancedTreeInPlace(args, (prev, next) => prev.Or(next));

            return orExpr;
        }

        private static bool IsStringType(TypeUsage type)
        {
            return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.String);
        }

        private static bool IsBooleanType(TypeUsage type)
        {
            return TypeSemantics.IsPrimitiveType(type, PrimitiveTypeKind.Boolean);
        }

        private static bool IsSubOrSuperType(TypeUsage type1, TypeUsage type2)
        {
            return TypeSemantics.IsStructurallyEqual(type1, type2) || type1.IsSubtypeOf(type2) || type2.IsSubtypeOf(type1);
        }

        #region Expression converters

        private delegate ExpressionResolution AstExprConverter(Node astExpr, SemanticResolver sr);

        private static readonly Dictionary<Type, AstExprConverter> _astExprConverters = CreateAstExprConverters();

        private delegate DbExpression BuiltInExprConverter(BuiltInExpr astBltInExpr, SemanticResolver sr);

        private static readonly Dictionary<BuiltInKind, BuiltInExprConverter> _builtInExprConverter = CreateBuiltInExprConverter();

        private static Dictionary<Type, AstExprConverter> CreateAstExprConverters()
        {
            const int NumberOfElements = 17; // number of elements initialized by the dictionary
            var astExprConverters = new Dictionary<Type, AstExprConverter>(NumberOfElements);
            astExprConverters.Add(typeof(Literal), ConvertLiteral);
            astExprConverters.Add(typeof(QueryParameter), ConvertParameter);
            astExprConverters.Add(typeof(Identifier), ConvertIdentifier);
            astExprConverters.Add(typeof(DotExpr), ConvertDotExpr);
            astExprConverters.Add(typeof(BuiltInExpr), ConvertBuiltIn);
            astExprConverters.Add(typeof(QueryExpr), ConvertQueryExpr);
            astExprConverters.Add(typeof(ParenExpr), ConvertParenExpr);
            astExprConverters.Add(typeof(RowConstructorExpr), ConvertRowConstructor);
            astExprConverters.Add(typeof(MultisetConstructorExpr), ConvertMultisetConstructor);
            astExprConverters.Add(typeof(CaseExpr), ConvertCaseExpr);
            astExprConverters.Add(typeof(RelshipNavigationExpr), ConvertRelshipNavigationExpr);
            astExprConverters.Add(typeof(RefExpr), ConvertRefExpr);
            astExprConverters.Add(typeof(DerefExpr), ConvertDeRefExpr);
            astExprConverters.Add(typeof(MethodExpr), ConvertMethodExpr);
            astExprConverters.Add(typeof(CreateRefExpr), ConvertCreateRefExpr);
            astExprConverters.Add(typeof(KeyExpr), ConvertKeyExpr);
            astExprConverters.Add(typeof(GroupPartitionExpr), ConvertGroupPartitionExpr);
            Debug.Assert(NumberOfElements == astExprConverters.Count, "The number of elements and initial capacity don't match");
            return astExprConverters;
        }

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        [SuppressMessage("Microsoft.Maintainability", "CA1505:AvoidUnmaintainableCode")]
        [SuppressMessage("Microsoft.Maintainability", "CA1502:AvoidExcessiveComplexity")]
        private static Dictionary<BuiltInKind, BuiltInExprConverter> CreateBuiltInExprConverter()
        {
            var builtInExprConverter = new Dictionary<BuiltInKind, BuiltInExprConverter>(sizeof(BuiltInKind));

            ////////////////////////////
            // Arithmetic Expressions
            ////////////////////////////

            //
            // e1 + e2
            //

            #region e1 + e2

            builtInExprConverter.Add(
                BuiltInKind.Plus, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertPlusOperands(bltInExpr, sr);

                        if (TypeSemantics.IsNumericType(args.Left.ResultType))
                        {
                            return args.Left.Plus(args.Right);
                        }
                        else
                        {
                            //
                            // fold '+' operator into concat canonical function
                            //
                            MetadataFunctionGroup function;
                            if (!sr.TypeResolver.TryGetFunctionFromMetadata("Edm", "Concat", out function))
                            {
                                var errCtx = bltInExpr.ErrCtx;
                                var message = Strings.ConcatBuiltinNotSupported;
                                throw EntitySqlException.Create(errCtx, message, null);
                            }

                            var argTypes = new List<TypeUsage>(2);
                            argTypes.Add(args.Left.ResultType);
                            argTypes.Add(args.Right.ResultType);

                            var isAmbiguous = false;
                            var concatFunction = SemanticResolver.ResolveFunctionOverloads(
                                function.FunctionMetadata,
                                argTypes,
                                false /* isGroupAggregate */,
                                out isAmbiguous);

                            if (null == concatFunction || isAmbiguous)
                            {
                                var errCtx = bltInExpr.ErrCtx;
                                var message = Strings.ConcatBuiltinNotSupported;
                                throw EntitySqlException.Create(errCtx, message, null);
                            }

                            return concatFunction.Invoke(new[] { args.Left, args.Right });
                        }
                    });

            #endregion

            //
            // e1 - e2
            //

            #region e1 - e2

            builtInExprConverter.Add(
                BuiltInKind.Minus, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertArithmeticArgs(bltInExpr, sr);

                        return args.Left.Minus(args.Right);
                    });

            #endregion

            //
            // e1 * e2
            //

            #region e1 * e2

            builtInExprConverter.Add(
                BuiltInKind.Multiply, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertArithmeticArgs(bltInExpr, sr);

                        return args.Left.Multiply(args.Right);
                    });

            #endregion

            //
            // e1 / e2
            //

            #region e1 / e2

            builtInExprConverter.Add(
                BuiltInKind.Divide, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertArithmeticArgs(bltInExpr, sr);

                        return args.Left.Divide(args.Right);
                    });

            #endregion

            //
            // e1 % e2
            //

            #region e1 % e2

            builtInExprConverter.Add(
                BuiltInKind.Modulus, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertArithmeticArgs(bltInExpr, sr);

                        return args.Left.Modulo(args.Right);
                    });

            #endregion

            //
            // - e
            //

            #region - e

            builtInExprConverter.Add(
                BuiltInKind.UnaryMinus, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var argument = ConvertArithmeticArgs(bltInExpr, sr).Left;
                        if (TypeSemantics.IsUnsignedNumericType(argument.ResultType))
                        {
                            TypeUsage closestPromotableType = null;
                            if (
                                !TypeHelpers.TryGetClosestPromotableType(
                                    argument.ResultType, out closestPromotableType))
                            {
                                var message = Strings.InvalidUnsignedTypeForUnaryMinusOperation(
                                    argument.ResultType.EdmType.FullName);
                                throw new EntitySqlException(message);
                            }
                        }

                        DbExpression unaryExpr = argument.UnaryMinus();
                        return unaryExpr;
                    });

            #endregion

            //
            // + e
            //

            #region + e

            builtInExprConverter.Add(
                BuiltInKind.UnaryPlus,
                delegate(BuiltInExpr bltInExpr, SemanticResolver sr) { return ConvertArithmeticArgs(bltInExpr, sr).Left; });

            #endregion

            ////////////////////////////
            // Logical Expressions
            ////////////////////////////

            //
            // e1 AND e2
            // e1 && e2
            //

            #region e1 AND e2

            builtInExprConverter.Add(
                BuiltInKind.And, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertLogicalArgs(bltInExpr, sr);

                        return args.Left.And(args.Right);
                    });

            #endregion

            //
            // e1 OR e2
            // e1 || e2
            //

            #region e1 OR e2

            builtInExprConverter.Add(
                BuiltInKind.Or, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertLogicalArgs(bltInExpr, sr);

                        return args.Left.Or(args.Right);
                    });

            #endregion

            //
            // NOT e
            // ! e
            //

            #region NOT e

            builtInExprConverter.Add(
                BuiltInKind.Not,
                delegate(BuiltInExpr bltInExpr, SemanticResolver sr) { return ConvertLogicalArgs(bltInExpr, sr).Left.Not(); });

            #endregion

            ////////////////////////////
            // Comparison Expressions
            ////////////////////////////

            //
            // e1 == e2 | e1 = e2
            //

            #region e1 == e2

            builtInExprConverter.Add(
                BuiltInKind.Equal, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertEqualCompArgs(bltInExpr, sr);

                        return args.Left.Equal(args.Right);
                    });

            #endregion

            //
            // e1 != e2 | e1 <> e2
            //

            #region e1 != e2

            builtInExprConverter.Add(
                BuiltInKind.NotEqual, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertEqualCompArgs(bltInExpr, sr);

                        // This was originally CreateNotExpression(CreateEqualsExpression(left, right))
                        // and this semantic is maintained with left.Equal(right).Not(), even though left.NotEqual
                        // seems like the more obvious (correct?) implementation.
                        return args.Left.Equal(args.Right).Not();
                    });

            #endregion

            //
            // e1 >= e2
            //

            #region e1 >= e2

            builtInExprConverter.Add(
                BuiltInKind.GreaterEqual, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertOrderCompArgs(bltInExpr, sr);

                        return args.Left.GreaterThanOrEqual(args.Right);
                    });

            #endregion

            //
            // e1 > e2
            //

            #region e1 > e2

            builtInExprConverter.Add(
                BuiltInKind.GreaterThan, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertOrderCompArgs(bltInExpr, sr);

                        return args.Left.GreaterThan(args.Right);
                    });

            #endregion

            //
            // e1 <= e2
            //

            #region e1 <= e2

            builtInExprConverter.Add(
                BuiltInKind.LessEqual, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertOrderCompArgs(bltInExpr, sr);

                        return args.Left.LessThanOrEqual(args.Right);
                    });

            #endregion

            //
            // e1 < e2
            //

            #region e1 < e2

            builtInExprConverter.Add(
                BuiltInKind.LessThan, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertOrderCompArgs(bltInExpr, sr);

                        return args.Left.LessThan(args.Right);
                    });

            #endregion

            ////////////////////////////
            //    SET EXPRESSIONS
            ////////////////////////////

            //
            // e1 UNION e2
            //

            #region e1 UNION e2

            builtInExprConverter.Add(
                BuiltInKind.Union, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertSetArgs(bltInExpr, sr);

                        return args.Left.UnionAll(args.Right).Distinct();
                    });

            #endregion

            //
            // e1 UNION ALL e2
            //

            #region e1 UNION ALL e2

            builtInExprConverter.Add(
                BuiltInKind.UnionAll, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertSetArgs(bltInExpr, sr);

                        return args.Left.UnionAll(args.Right);
                    });

            #endregion

            //
            // e1 INTERSECT e2
            //

            #region e1 INTERSECT e2

            builtInExprConverter.Add(
                BuiltInKind.Intersect, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertSetArgs(bltInExpr, sr);

                        return args.Left.Intersect(args.Right);
                    });

            #endregion

            //
            // e1 OVERLAPS e2
            //

            #region e1 OVERLAPS e1

            builtInExprConverter.Add(
                BuiltInKind.Overlaps, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertSetArgs(bltInExpr, sr);

                        return args.Left.Intersect(args.Right).IsEmpty().Not();
                    });

            #endregion

            //
            // ANYELEMENT( e )
            //

            #region ANYELEMENT( e )

            builtInExprConverter.Add(
                BuiltInKind.AnyElement,
                delegate(BuiltInExpr bltInExpr, SemanticResolver sr) { return ConvertSetArgs(bltInExpr, sr).Left.Element(); });

            #endregion

            //
            // ELEMENT( e )
            //

            #region ELEMENT( e ) - NOT SUPPORTED IN ORCAS TIMEFRAME

            builtInExprConverter.Add(
                BuiltInKind.Element, delegate { throw new NotSupportedException(Strings.ElementOperatorIsNotSupported); });

            #endregion

            //
            // e1 EXCEPT e2
            //

            #region e1 EXCEPT e2

            builtInExprConverter.Add(
                BuiltInKind.Except, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertSetArgs(bltInExpr, sr);

                        return args.Left.Except(args.Right);
                    });

            #endregion

            //
            // EXISTS( e )
            //

            #region EXISTS( e )

            builtInExprConverter.Add(
                BuiltInKind.Exists,
                delegate(BuiltInExpr bltInExpr, SemanticResolver sr) { return ConvertSetArgs(bltInExpr, sr).Left.IsEmpty().Not(); });

            #endregion

            //
            // FLATTEN( e )
            //

            #region FLATTEN( e )

            builtInExprConverter.Add(
                BuiltInKind.Flatten, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var elemExpr = ConvertValueExpression(bltInExpr.Arg1, sr);

                        if (!TypeSemantics.IsCollectionType(elemExpr.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.InvalidFlattenArgument;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!TypeSemantics.IsCollectionType(TypeHelpers.GetElementTypeUsage(elemExpr.ResultType)))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.InvalidFlattenArgument;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        var leftExpr = elemExpr.BindAs(sr.GenerateInternalName("l_flatten"));

                        var rightExpr = leftExpr.Variable.BindAs(sr.GenerateInternalName("r_flatten"));

                        var applyBinding = leftExpr.CrossApply(rightExpr).BindAs(sr.GenerateInternalName("flatten"));

                        return applyBinding.Project(applyBinding.Variable.Property(rightExpr.VariableName));
                    });

            #endregion

            //
            // e1 IN e2
            //

            #region e1 IN e2

            builtInExprConverter.Add(
                BuiltInKind.In, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertInExprArgs(bltInExpr, sr);

                        //
                        // Convert "x in multiset(y1, y2, ..., yn)" into x = y1 or x = y2 or x = y3 ...
                        //
                        if (args.Right.ExpressionKind
                            == DbExpressionKind.NewInstance)
                        {
                            return ConvertSimpleInExpression(args.Left, args.Right);
                        }
                        else
                        {
                            var rSet = args.Right.BindAs(sr.GenerateInternalName("in-filter"));

                            var leftIn = args.Left;
                            DbExpression rightSet = rSet.Variable;

                            DbExpression exists = rSet.Filter(leftIn.Equal(rightSet)).IsEmpty().Not();

                            var whenExpr = new List<DbExpression>(1);
                            whenExpr.Add(leftIn.IsNull());
                            var thenExpr = new List<DbExpression>(1);
                            thenExpr.Add(TypeResolver.BooleanType.Null());

                            DbExpression left = DbExpressionBuilder.Case(whenExpr, thenExpr, DbExpressionBuilder.False);

                            DbExpression converted = left.Or(exists);

                            return converted;
                        }
                    });

            #endregion

            //
            // e1 NOT IN e1
            //

            #region e1 NOT IN e1

            builtInExprConverter.Add(
                BuiltInKind.NotIn, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertInExprArgs(bltInExpr, sr);

                        if (args.Right.ExpressionKind
                            == DbExpressionKind.NewInstance)
                        {
                            return ConvertSimpleInExpression(args.Left, args.Right).Not();
                        }
                        else
                        {
                            var rSet = args.Right.BindAs(sr.GenerateInternalName("in-filter"));

                            var leftIn = args.Left;
                            DbExpression rightSet = rSet.Variable;

                            DbExpression exists = rSet.Filter(leftIn.Equal(rightSet)).IsEmpty();

                            var whenExpr = new List<DbExpression>(1);
                            whenExpr.Add(leftIn.IsNull());
                            var thenExpr = new List<DbExpression>(1);
                            thenExpr.Add(TypeResolver.BooleanType.Null());

                            DbExpression left = DbExpressionBuilder.Case(whenExpr, thenExpr, DbExpressionBuilder.True);

                            DbExpression converted = left.And(exists);

                            return converted;
                        }
                    });

            #endregion

            //
            // SET( e ) - DISTINCT( e ) before
            //

            #region SET( e )

            builtInExprConverter.Add(
                BuiltInKind.Distinct, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var args = ConvertSetArgs(bltInExpr, sr);

                        return args.Left.Distinct();
                    });

            #endregion

            ////////////////////////////
            // Nullability Expressions
            ////////////////////////////

            //
            // e IS NULL
            //

            #region e IS NULL

            builtInExprConverter.Add(
                BuiltInKind.IsNull, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var isNullExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);

                        //
                        // Ensure expression edmType is valid for this operation.
                        //
                        if (isNullExpr != null
                            && !TypeHelpers.IsValidIsNullOpType(isNullExpr.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.IsNullInvalidType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        return isNullExpr != null ? (DbExpression)isNullExpr.IsNull() : DbExpressionBuilder.True;
                    });

            #endregion

            //
            // e IS NOT NULL
            //

            #region e IS NOT NULL

            builtInExprConverter.Add(
                BuiltInKind.IsNotNull, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var isNullExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);

                        //
                        // Ensure expression edmType is valid for this operation.
                        //
                        if (isNullExpr != null
                            && !TypeHelpers.IsValidIsNullOpType(isNullExpr.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.IsNullInvalidType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        return isNullExpr != null
                                   ? (DbExpression)isNullExpr.IsNull().Not()
                                   : DbExpressionBuilder.False;
                    });

            #endregion

            ////////////////////////////
            //    Type Expressions
            ////////////////////////////

            //
            // e IS OF ( [ONLY] T )
            //

            #region e IS OF ( [ONLY] T )

            builtInExprConverter.Add(
                BuiltInKind.IsOf, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var exprToFilter = ConvertValueExpression(bltInExpr.Arg1, sr);
                        var typeToFilterTo = ConvertTypeName(bltInExpr.Arg2, sr);

                        var isOnly = (bool)((Literal)bltInExpr.Arg3).Value;
                        var isNot = (bool)((Literal)bltInExpr.Arg4).Value;
                        var isNominalTypeAllowed = sr.ParserOptions.ParserCompilationMode
                                                   == ParserOptions.CompilationMode.RestrictedViewGenerationMode;

                        if (!isNominalTypeAllowed
                            && !TypeSemantics.IsEntityType(exprToFilter.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.ExpressionTypeMustBeEntityType(
                                Strings.CtxIsOf,
                                exprToFilter.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                exprToFilter.ResultType.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }
                        else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(exprToFilter.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.ExpressionTypeMustBeNominalType(
                                Strings.CtxIsOf,
                                exprToFilter.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                exprToFilter.ResultType.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!isNominalTypeAllowed
                            && !TypeSemantics.IsEntityType(typeToFilterTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeEntityType(
                                Strings.CtxIsOf,
                                typeToFilterTo.EdmType.BuiltInTypeKind.ToString(),
                                typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }
                        else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(typeToFilterTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeNominalType(
                                Strings.CtxIsOf,
                                typeToFilterTo.EdmType.BuiltInTypeKind.ToString(),
                                typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!TypeSemantics.IsPolymorphicType(exprToFilter.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.TypeMustBeInheritableType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!TypeSemantics.IsPolymorphicType(typeToFilterTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeInheritableType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!IsSubOrSuperType(exprToFilter.ResultType, typeToFilterTo))
                        {
                            var errCtx = bltInExpr.ErrCtx;
                            var message = Strings.NotASuperOrSubType(
                                exprToFilter.ResultType.EdmType.FullName,
                                typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        typeToFilterTo = TypeHelpers.GetReadOnlyType(typeToFilterTo);

                        DbExpression retExpr = null;
                        if (isOnly)
                        {
                            retExpr = exprToFilter.IsOfOnly(typeToFilterTo);
                        }
                        else
                        {
                            retExpr = exprToFilter.IsOf(typeToFilterTo);
                        }

                        if (isNot)
                        {
                            retExpr = retExpr.Not();
                        }

                        return retExpr;
                    });

            #endregion

            //
            // TREAT( e as T )
            //

            #region TREAT( e as T )

            builtInExprConverter.Add(
                BuiltInKind.Treat, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var exprToTreat = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
                        var typeToTreatTo = ConvertTypeName(bltInExpr.Arg2, sr);

                        var isNominalTypeAllowed = sr.ParserOptions.ParserCompilationMode
                                                   == ParserOptions.CompilationMode.RestrictedViewGenerationMode;

                        if (!isNominalTypeAllowed
                            && !TypeSemantics.IsEntityType(typeToTreatTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeEntityType(
                                Strings.CtxTreat,
                                typeToTreatTo.EdmType.BuiltInTypeKind.ToString(),
                                typeToTreatTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }
                        else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(typeToTreatTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeNominalType(
                                Strings.CtxTreat,
                                typeToTreatTo.EdmType.BuiltInTypeKind.ToString(),
                                typeToTreatTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (exprToTreat == null)
                        {
                            exprToTreat = typeToTreatTo.Null();
                        }
                        else if (!isNominalTypeAllowed
                                 && !TypeSemantics.IsEntityType(exprToTreat.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.ExpressionTypeMustBeEntityType(
                                Strings.CtxTreat,
                                exprToTreat.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                exprToTreat.ResultType.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }
                        else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(exprToTreat.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.ExpressionTypeMustBeNominalType(
                                Strings.CtxTreat,
                                exprToTreat.ResultType.EdmType.BuiltInTypeKind.ToString(),
                                exprToTreat.ResultType.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!TypeSemantics.IsPolymorphicType(exprToTreat.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.TypeMustBeInheritableType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!TypeSemantics.IsPolymorphicType(typeToTreatTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeInheritableType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!IsSubOrSuperType(exprToTreat.ResultType, typeToTreatTo))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.NotASuperOrSubType(
                                exprToTreat.ResultType.EdmType.FullName,
                                typeToTreatTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        return exprToTreat.TreatAs(TypeHelpers.GetReadOnlyType(typeToTreatTo));
                    });

            #endregion

            //
            // CAST( e AS T )
            //

            #region CAST( e AS T )

            builtInExprConverter.Add(
                BuiltInKind.Cast, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var exprToCast = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
                        var typeToCastTo = ConvertTypeName(bltInExpr.Arg2, sr);

                        //
                        // Ensure CAST target edmType is scalar.
                        //
                        if (!TypeSemantics.IsScalarType(typeToCastTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.InvalidCastType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (exprToCast == null)
                        {
                            return typeToCastTo.Null();
                        }

                        //
                        // Ensure CAST source edmType is scalar.
                        //
                        if (!TypeSemantics.IsScalarType(exprToCast.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.InvalidCastExpressionType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!TypeSemantics.IsCastAllowed(exprToCast.ResultType, typeToCastTo))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.InvalidCast(
                                exprToCast.ResultType.EdmType.FullName, typeToCastTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        return exprToCast.CastTo(TypeHelpers.GetReadOnlyType(typeToCastTo));
                    });

            #endregion

            //
            // OFTYPE( [ONLY] e, T )
            //

            #region OFTYPE( [ONLY] e, T )

            builtInExprConverter.Add(
                BuiltInKind.OfType, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        var exprToFilter = ConvertValueExpression(bltInExpr.Arg1, sr);
                        var typeToFilterTo = ConvertTypeName(bltInExpr.Arg2, sr);

                        var isOnly = (bool)((Literal)bltInExpr.Arg3).Value;

                        var isNominalTypeAllowed = sr.ParserOptions.ParserCompilationMode
                                                   == ParserOptions.CompilationMode.RestrictedViewGenerationMode;

                        if (!TypeSemantics.IsCollectionType(exprToFilter.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.ExpressionMustBeCollection;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        var elementType = TypeHelpers.GetElementTypeUsage(exprToFilter.ResultType);
                        if (!isNominalTypeAllowed
                            && !TypeSemantics.IsEntityType(elementType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.OfTypeExpressionElementTypeMustBeEntityType(
                                elementType.EdmType.BuiltInTypeKind.ToString(), elementType);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }
                        else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(elementType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.OfTypeExpressionElementTypeMustBeNominalType(
                                elementType.EdmType.BuiltInTypeKind.ToString(), elementType);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!isNominalTypeAllowed
                            && !TypeSemantics.IsEntityType(typeToFilterTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeEntityType(
                                Strings.CtxOfType, typeToFilterTo.EdmType.BuiltInTypeKind.ToString(),
                                typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }
                        else if (isNominalTypeAllowed && !TypeSemantics.IsNominalType(typeToFilterTo))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.TypeMustBeNominalType(
                                Strings.CtxOfType, typeToFilterTo.EdmType.BuiltInTypeKind.ToString(),
                                typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (isOnly && typeToFilterTo.EdmType.Abstract)
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.OfTypeOnlyTypeArgumentCannotBeAbstract(
                                typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (!IsSubOrSuperType(elementType, typeToFilterTo))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.NotASuperOrSubType(
                                elementType.EdmType.FullName, typeToFilterTo.EdmType.FullName);
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        DbExpression ofTypeExpression = null;
                        if (isOnly)
                        {
                            ofTypeExpression = exprToFilter.OfTypeOnly(TypeHelpers.GetReadOnlyType(typeToFilterTo));
                        }
                        else
                        {
                            ofTypeExpression = exprToFilter.OfType(TypeHelpers.GetReadOnlyType(typeToFilterTo));
                        }

                        return ofTypeExpression;
                    });

            #endregion

            //
            // e LIKE pattern [ESCAPE escape]
            //

            #region e LIKE pattern [ESCAPE escape]

            builtInExprConverter.Add(
                BuiltInKind.Like, delegate(BuiltInExpr bltInExpr, SemanticResolver sr)
                    {
                        DbExpression likeExpr = null;

                        var matchExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
                        if (matchExpr == null)
                        {
                            matchExpr = TypeResolver.StringType.Null();
                        }
                        else if (!IsStringType(matchExpr.ResultType))
                        {
                            var errCtx = bltInExpr.Arg1.ErrCtx;
                            var message = Strings.LikeArgMustBeStringType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        var patternExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg2, sr);
                        if (patternExpr == null)
                        {
                            patternExpr = TypeResolver.StringType.Null();
                        }
                        else if (!IsStringType(patternExpr.ResultType))
                        {
                            var errCtx = bltInExpr.Arg2.ErrCtx;
                            var message = Strings.LikeArgMustBeStringType;
                            throw EntitySqlException.Create(errCtx, message, null);
                        }

                        if (3 == bltInExpr.ArgCount)
                        {
                            var escapeExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg3, sr);
                            if (escapeExpr == null)
                            {
                                escapeExpr = TypeResolver.StringType.Null();
                            }
                            else if (!IsStringType(escapeExpr.ResultType))
                            {
                                var errCtx = bltInExpr.Arg3.ErrCtx;
                                var message = Strings.LikeArgMustBeStringType;
                                throw EntitySqlException.Create(errCtx, message, null);
                            }

                            likeExpr = matchExpr.Like(patternExpr, escapeExpr);
                        }
                        else
                        {
                            likeExpr = matchExpr.Like(patternExpr);
                        }

                        return likeExpr;
                    });

            #endregion

            //
            // e BETWEEN e1 AND e2
            //

            #region e BETWEEN e1 AND e2

            builtInExprConverter.Add(BuiltInKind.Between, ConvertBetweenExpr);

            #endregion

            //
            // e NOT BETWEEN e1 AND e2
            //

            #region e NOT BETWEEN e1 AND e2

            builtInExprConverter.Add(
                BuiltInKind.NotBetween,
                delegate(BuiltInExpr bltInExpr, SemanticResolver sr) { return ConvertBetweenExpr(bltInExpr, sr).Not(); });

            #endregion

            return builtInExprConverter;
        }

        private static DbExpression ConvertBetweenExpr(BuiltInExpr bltInExpr, SemanticResolver sr)
        {
            Debug.Assert(
                bltInExpr.Kind == BuiltInKind.Between || bltInExpr.Kind == BuiltInKind.NotBetween,
                "bltInExpr.Kind must be Between or NotBetween");
            Debug.Assert(bltInExpr.ArgCount == 3, "bltInExpr.ArgCount == 3");

            //
            // convert lower and upper limits
            //
            var limitsExpr = ConvertValueExpressionsWithUntypedNulls(
                bltInExpr.Arg2,
                bltInExpr.Arg3,
                bltInExpr.Arg1.ErrCtx,
                () => Strings.BetweenLimitsCannotBeUntypedNulls,
                sr);

            //
            // Get and check common edmType for limits
            //
            var rangeCommonType = TypeHelpers.GetCommonTypeUsage(limitsExpr.Left.ResultType, limitsExpr.Right.ResultType);
            if (null == rangeCommonType)
            {
                var errCtx = bltInExpr.Arg1.ErrCtx;
                var message = Strings.BetweenLimitsTypesAreNotCompatible(
                    limitsExpr.Left.ResultType.EdmType.FullName, limitsExpr.Right.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // check if limit types are order-comp
            //
            if (!TypeSemantics.IsOrderComparableTo(limitsExpr.Left.ResultType, limitsExpr.Right.ResultType))
            {
                var errCtx = bltInExpr.Arg1.ErrCtx;
                var message = Strings.BetweenLimitsTypesAreNotOrderComparable(
                    limitsExpr.Left.ResultType.EdmType.FullName, limitsExpr.Right.ResultType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            //
            // convert value expression
            //
            var valueExpr = ConvertValueExpressionAllowUntypedNulls(bltInExpr.Arg1, sr);
            if (valueExpr == null)
            {
                valueExpr = rangeCommonType.Null();
            }

            //
            // check if valueExpr is order-comparable to limits
            //
            if (!TypeSemantics.IsOrderComparableTo(valueExpr.ResultType, rangeCommonType))
            {
                var errCtx = bltInExpr.Arg1.ErrCtx;
                var message = Strings.BetweenValueIsNotOrderComparable(
                    valueExpr.ResultType.EdmType.FullName, rangeCommonType.EdmType.FullName);
                throw EntitySqlException.Create(errCtx, message, null);
            }

            return valueExpr.GreaterThanOrEqual(limitsExpr.Left).And(valueExpr.LessThanOrEqual(limitsExpr.Right));
        }

        #endregion
    }
}
