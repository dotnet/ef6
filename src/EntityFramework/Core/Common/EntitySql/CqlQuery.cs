// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;

    /// <summary>
    /// Provides eSQL text Parsing and Compilation services.
    /// </summary>
    /// <remarks>
    /// This class exposes services that perform syntactic and semantic analysis of eSQL commands.
    /// The syntactic validation ensures the given command conforms to eSQL formal grammar. The semantic analysis will
    /// perform (list not exhaustive): type resolution and validation, ensure semantic and scoping rules, etc.
    /// The services exposed by this class are:
    /// <list>
    ///     <item>
    ///         Translation from eSQL text commands to valid
    ///         <see cref="DbCommandTree" />
    ///         s
    ///     </item>
    ///     <item>
    ///         Translation from eSQL text commands to valid
    ///         <see cref="DbExpression" />
    ///         s
    ///     </item>
    /// </list>
    /// Queries can be formulated in O-Space, C-Space and S-Space and the services exposed by this class are agnostic of the especific typespace or
    /// metadata instance passed as required parameter in the semantic analysis by the perspective parameter. It is assumed that the perspective and
    /// metadata was properly initialized.
    /// Provided that the command is syntacticaly correct and meaningful within the given typespace, the result will be a valid
    /// <see
    ///     cref="DbCommandTree" />
    /// or
    /// <see cref="DbExpression" /> otherwise EntityException will be thrown indicating the reason(s) why the given command cannot be accepted.
    /// It is also possible that MetadataException and MappingException be thrown if mapping or metadata related problems are encountered during compilation.
    /// </remarks>
    /// <list>
    /// <item>
    ///     <seealso cref="ParserOptions" />
    /// </item>
    /// <item>
    ///     <seealso cref="DbCommandTree" />
    /// </item>
    /// <item>
    ///     <seealso cref="DbExpression" />
    /// </item>
    /// </list>
    internal static class CqlQuery
    {
        /// <summary>
        /// Compiles an eSQL command producing a validated <see cref="DbCommandTree" />.
        /// </summary>
        /// <param name="commandText"> eSQL command text </param>
        /// <param name="perspective"> perspective </param>
        /// <param name="parserOptions">
        /// parser options <seealso cref="ParserOptions" />
        /// </param>
        /// <param name="parameters"> ordinary parameters </param>
        /// <returns> A parse result with the command tree produced by parsing the given command. </returns>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntatic or Semantic rules are violated and the query cannot be accepted</exception>
        /// <exception cref="System.Data.Entity.Core.MetadataException">Thrown when metadata related service requests fail</exception>
        /// <exception cref="System.Data.Entity.Core.MappingException">Thrown when mapping related service requests fail</exception>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <seealso cref="ParserOptions" />
        /// <seealso cref="DbCommandTree" />
        internal static ParseResult Compile(
            string commandText,
            Perspective perspective,
            ParserOptions parserOptions,
            IEnumerable<DbParameterReferenceExpression> parameters)
        {
            var result = CompileCommon(
                commandText, parserOptions,
                (astCommand, validatedParserOptions) =>
                {
                    var parseResultInternal = AnalyzeCommandSemantics(astCommand, perspective, validatedParserOptions, parameters);

                    Debug.Assert(parseResultInternal != null, "parseResultInternal != null post-condition FAILED");
                    Debug.Assert(
                        parseResultInternal.CommandTree != null, "parseResultInternal.CommandTree != null post-condition FAILED");

                    TypeHelpers.AssertEdmType(parseResultInternal.CommandTree);

                    return parseResultInternal;
                });

            return result;
        }

        /// <summary>
        /// Compiles an eSQL query command producing a validated <see cref="DbLambda" />.
        /// </summary>
        /// <param name="queryCommandText"> eSQL query command text </param>
        /// <param name="perspective"> perspective </param>
        /// <param name="parserOptions">
        /// parser options <seealso cref="ParserOptions" />
        /// </param>
        /// <param name="parameters"> ordinary command parameters </param>
        /// <param name="variables"> command free variables </param>
        /// <returns> The query expression tree produced by parsing the given query command. </returns>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntatic or Semantic rules are violated and the query expression cannot be accepted</exception>
        /// <exception cref="System.Data.Entity.Core.MetadataException">Thrown when metadata related service requests fail</exception>
        /// <exception cref="System.Data.Entity.Core.MappingException">Thrown when mapping related service requests fail</exception>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <seealso cref="ParserOptions" />
        /// <seealso cref="DbExpression" />
        internal static DbLambda CompileQueryCommandLambda(
            string queryCommandText,
            Perspective perspective,
            ParserOptions parserOptions,
            IEnumerable<DbParameterReferenceExpression> parameters,
            IEnumerable<DbVariableReferenceExpression> variables)
        {
            return CompileCommon(
                queryCommandText, parserOptions, (astCommand, validatedParserOptions) =>
                    {
                        var lambda = AnalyzeQueryExpressionSemantics(
                            astCommand,
                            perspective,
                            validatedParserOptions,
                            parameters,
                            variables);

                        TypeHelpers.AssertEdmType(lambda.Body.ResultType);

                        Debug.Assert(lambda != null, "lambda != null post-condition FAILED");

                        return lambda;
                    });
        }

        #region Private

        /// <summary>
        /// Parse eSQL command string into an AST
        /// </summary>
        /// <param name="commandText"> eSQL command </param>
        /// <param name="parserOptions">
        /// parser options <seealso cref="ParserOptions" />
        /// </param>
        /// <returns> Ast </returns>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntatic or Semantic rules are violated and the query cannot be accepted</exception>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <seealso cref="ParserOptions" />
        private static Node Parse(string commandText, ParserOptions parserOptions)
        {
            // The common practice is to make the null check at the public surface, 
            // however this method is a convergence zone from multiple public entry points and it makes sense to
            // check for null once, here.
            Check.NotEmpty(commandText, "commandText");

            //
            // Create Parser
            //
            var cqlParser = new CqlParser(parserOptions, true);

            //
            // Invoke parser
            //
            var astExpr = cqlParser.Parse(commandText);

            if (null == astExpr)
            {
                throw EntitySqlException.Create(commandText, Strings.InvalidEmptyQuery, 0, null, false, null);
            }

            return astExpr;
        }

        private static TResult CompileCommon<TResult>(
            string commandText,
            ParserOptions parserOptions,
            Func<Node, ParserOptions, TResult> compilationFunction)
            where TResult : class
        {
            DebugCheck.NotNull(commandText);

            //
            // Validate parser options - if null, give default options
            //
            parserOptions = parserOptions ?? new ParserOptions();

            //
            // Perform Semantic Analysis/Conversion
            //
            return compilationFunction(Parse(commandText, parserOptions), parserOptions);
        }

        /// <summary>
        /// Performs semantic conversion, validation on a command AST and creates a <see cref="DbCommandTree" />
        /// </summary>
        /// <param name="astExpr"> Abstract Syntax Tree of the command </param>
        /// <param name="perspective"> perspective </param>
        /// <param name="parserOptions">
        /// parser options <seealso cref="ParserOptions" />
        /// </param>
        /// <param name="parameters"> ordinary command parameters </param>
        /// <returns> a parse result with a valid command tree </returns>
        /// <remarks>
        /// Parameters name/types must be bound before invoking this method
        /// </remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntatic or Semantic rules are violated and the query cannot be accepted.</exception>
        /// <exception cref="System.Data.Entity.Core.MetadataException">Thrown as inner exception of a EntityException when metadata related service requests fail.</exception>
        /// <exception cref="System.Data.Entity.Core.MappingException">Thrown as inner exception of a EntityException when mapping related service requests fail.</exception>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <seealso cref="ParserOptions" />
        /// <seealso cref="DbCommandTree" />
        private static ParseResult AnalyzeCommandSemantics(
            Node astExpr,
            Perspective perspective,
            ParserOptions parserOptions,
            IEnumerable<DbParameterReferenceExpression> parameters)
        {
            var result = AnalyzeSemanticsCommon(
                astExpr, perspective, parserOptions, parameters, null /*variables*/,
                (analyzer, astExpression) =>
                {
                    var parseResultInternal = analyzer.AnalyzeCommand(astExpression);

                    Debug.Assert(parseResultInternal != null, "parseResultInternal != null post-condition FAILED");
                    Debug.Assert(
                        parseResultInternal.CommandTree != null, "parseResultInternal.CommandTree != null post-condition FAILED");

                    return parseResultInternal;
                });

            return result;
        }

        /// <summary>
        /// Performs semantic conversion, validation on a query command AST and creates a <see cref="DbLambda" />
        /// </summary>
        /// <param name="astQueryCommand"> Abstract Syntax Tree of the query command </param>
        /// <param name="perspective"> perspective </param>
        /// <param name="parserOptions">
        /// parser options <seealso cref="ParserOptions" />
        /// </param>
        /// <param name="parameters"> ordinary command parameters </param>
        /// <param name="variables"> command free variables </param>
        /// <remarks>
        /// Parameters name/types must be bound before invoking this method
        /// </remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntatic or Semantic rules are violated and the query cannot be accepted.</exception>
        /// <exception cref="System.Data.Entity.Core.MetadataException">Thrown as inner exception of a EntityException when metadata related service requests fail.</exception>
        /// <exception cref="System.Data.Entity.Core.MappingException">Thrown as inner exception of a EntityException when mapping related service requests fail.</exception>
        /// <remarks>
        /// This method is not thread safe.
        /// </remarks>
        /// <seealso cref="ParserOptions" />
        /// <seealso cref="DbExpression" />
        private static DbLambda AnalyzeQueryExpressionSemantics(
            Node astQueryCommand,
            Perspective perspective,
            ParserOptions parserOptions,
            IEnumerable<DbParameterReferenceExpression> parameters,
            IEnumerable<DbVariableReferenceExpression> variables)
        {
            return AnalyzeSemanticsCommon(
                astQueryCommand,
                perspective,
                parserOptions,
                parameters,
                variables,
                (analyzer, astExpr) =>
                {
                    var lambda = analyzer.AnalyzeQueryCommand(astExpr);
                    Debug.Assert(null != lambda, "null != lambda post-condition FAILED");
                    return lambda;
                });
        }

        private static TResult AnalyzeSemanticsCommon<TResult>(
            Node astExpr,
            Perspective perspective,
            ParserOptions parserOptions,
            IEnumerable<DbParameterReferenceExpression> parameters,
            IEnumerable<DbVariableReferenceExpression> variables,
            Func<SemanticAnalyzer, Node, TResult> analysisFunction)
            where TResult : class
        {
            DebugCheck.NotNull(astExpr);
            DebugCheck.NotNull(perspective);

            TResult result = null;

            try
            {
                //
                // Invoke semantic analysis
                //
                var analyzer = (new SemanticAnalyzer(SemanticResolver.Create(perspective, parserOptions, parameters, variables)));
                result = analysisFunction(analyzer, astExpr);
            }
            //
            // Wrap MetadataException as EntityException inner exception
            //
            catch (MetadataException metadataException)
            {
                var message = Strings.GeneralExceptionAsQueryInnerException("Metadata");
                throw new EntitySqlException(message, metadataException);
            }
            //
            // Wrap MappingException as EntityException inner exception
            //
            catch (MappingException mappingException)
            {
                var message = Strings.GeneralExceptionAsQueryInnerException("Mapping");
                throw new EntitySqlException(message, mappingException);
            }

            return result;
        }

        #endregion
    }
}
