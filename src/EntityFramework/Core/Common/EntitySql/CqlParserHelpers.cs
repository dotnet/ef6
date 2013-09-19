// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.EntitySql.AST;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Globalization;

    /// <summary>
    /// Represents the Cql Parser engine. Also, implements helpers and util routines.
    /// </summary>
    internal sealed partial class CqlParser
    {
        private Node _parsedTree;
        private CqlLexer _lexer;
        private string _query;
        private readonly ParserOptions _parserOptions;
        private const string _internalYaccSyntaxErrorMessage = "syntax error";

        /// <summary>
        /// Contains inclusive count of method expressions.
        /// </summary>
        private uint _methodExprCounter;

        private Stack<uint> _methodExprCounterStack;

        internal CqlParser(ParserOptions parserOptions, bool debug)
        {
            // The common practice is to make the null check at the public surface, 
            // however this method is a convergence zone from multiple public entry points and it makes sense to
            // check for null once, here.
            DebugCheck.NotNull(parserOptions);

            _parserOptions = parserOptions;
            yydebug = debug;
        }

        /// <summary>
        /// Main entry point for parsing cql.
        /// </summary>
        /// <param name="query"> query text </param>
        /// <exception cref="System.Data.Entity.Core.EntityException">Thrown when Syntatic rules are violated and the query cannot be accepted</exception>
        /// <returns> Abstract Syntax Tree </returns>
        internal Node Parse(string query)
        {
            DebugCheck.NotEmpty(query);

            _query = query;
            _parsedTree = null;
            _methodExprCounter = 0;
            _methodExprCounterStack = new Stack<uint>();
            internalParseEntryPoint();
            return _parsedTree;
        }

        /// <summary>
        /// Returns query string
        /// </summary>
        internal string Query
        {
            get { return _query; }
        }

#if ENTITYSQL_PARSER_YYDEBUG
    /// <summary>
    /// Enables/Disables yacc debugging.
    /// </summary>
        internal bool EnableDebug
        {
            get { return yydebug; }
            set { yydebug = value; }
        }
#endif

        /// <summary>
        /// Returns ParserOptions used
        /// </summary>
        /// <remarks>
        /// Once parse has been invoked, ParserOptions are frozen and cannot be changed. otherwise a EntityException exception will be thrown
        /// </remarks>
        internal ParserOptions ParserOptions
        {
            get { return _parserOptions; }
        }

        /// <summary>
        /// Internal entry point
        /// </summary>
        private void internalParseEntryPoint()
        {
            _lexer = new CqlLexer(Query, ParserOptions);
#if ENTITYSQL_PARSER_YYDEBUG
            CqlLexer.Token tk = lexer.yylex();
            while (null != tk)
            {
                Console.WriteLine("{0} := {1}", tk.TokenId, lexer.yytext());
                tk = lexer.yylex();
            }
#endif
            yyparse();
        }

        //
        // Conversion/Cast/Helpers
        //
        private static Node AstNode(object o)
        {
            return ((Node)o);
        }

        private static int AstNodePos(object o)
        {
            return ((Node)o).ErrCtx.InputPosition;
        }

        private static CqlLexer.TerminalToken Terminal(object o)
        {
            return ((CqlLexer.TerminalToken)o);
        }

        private static int TerminalPos(object o)
        {
            return ((CqlLexer.TerminalToken)o).IPos;
        }

        private static NodeList<T> ToNodeList<T>(object o) where T : Node
        {
            return ((NodeList<T>)o);
        }

        private short yylex()
        {
            CqlLexer.Token token = null;
            token = _lexer.yylex();
            if (null == token)
            {
                return 0;
            }
            _lexer.AdvanceIPos();
            yylval = token.Value;
            return token.TokenId;
        }

        private void yyerror_stackoverflow()
        {
            yyerror(Strings.StackOverflowInParser);
        }

        private void yyerror(string s)
        {
            if (s.Equals(_internalYaccSyntaxErrorMessage, StringComparison.Ordinal))
            {
                var errorPosition = _lexer.IPos;
                string syntaxContextInfo = null;
                var term = _lexer.YYText;
                if (!String.IsNullOrEmpty(term))
                {
                    syntaxContextInfo = Strings.LocalizedTerm;
                    ErrorContext errCtx = null;
                    var astNode = yylval as Node;
                    if (null != astNode
                        && (null != astNode.ErrCtx)
                        && (!String.IsNullOrEmpty(astNode.ErrCtx.ErrorContextInfo)))
                    {
                        errCtx = astNode.ErrCtx;
                        errorPosition = Math.Min(errorPosition, errorPosition - term.Length);
                    }

                    if ((yylval is CqlLexer.TerminalToken)
                        && CqlLexer.IsReservedKeyword(term)
                        && !(astNode is Identifier))
                    {
                        syntaxContextInfo = Strings.LocalizedKeyword;
                        term = term.ToUpperInvariant();
                        errorPosition = Math.Min(errorPosition, errorPosition - term.Length);
                    }
                    else if (null != errCtx)
                    {
                        syntaxContextInfo = EntityRes.GetString(errCtx.ErrorContextInfo);
                    }

                    syntaxContextInfo = String.Format(CultureInfo.CurrentCulture, "{0} '{1}'", syntaxContextInfo, term);
                }

                var errorMessage = Strings.GenericSyntaxError;
                throw EntitySqlException.Create(
                    _query,
                    errorMessage,
                    errorPosition,
                    syntaxContextInfo,
                    false,
                    null);
            }
            var errorPosition1 = _lexer.IPos;
            throw EntitySqlException.Create(_query, s, errorPosition1, null, false, null);
        }

        //
        // Error tracking helpers
        //
        private void SetErrCtx(Node astExpr, CqlLexer.TerminalToken tokenValue, string info)
        {
            SetErrCtx(astExpr, tokenValue.IPos, info);
        }

        private void SetErrCtx(Node astExpr, int inputPos, string info)
        {
            astExpr.ErrCtx.InputPosition = inputPos;
            astExpr.ErrCtx.ErrorContextInfo = info;
            astExpr.ErrCtx.CommandText = _query;
        }

        private void StartMethodExprCounting()
        {
            // Save the current counter value.
            _methodExprCounterStack.Push(_methodExprCounter);

            // Reset the counter for the current level.
            _methodExprCounter = 0;
        }

        private void IncrementMethodExprCount()
        {
            ++_methodExprCounter;
        }

        private uint EndMethodExprCounting()
        {
            // Save number of method expressions on the current level.
            var count = _methodExprCounter;

            // Restore upper level counter and adjust it with the number of method expressions on the current level.
            _methodExprCounter += _methodExprCounterStack.Pop();

            return count;
        }
    }
}
