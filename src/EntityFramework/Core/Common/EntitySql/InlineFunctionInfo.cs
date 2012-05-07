namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;

    internal abstract class InlineFunctionInfo
    {
        internal InlineFunctionInfo(AST.FunctionDefinition functionDef, List<DbVariableReferenceExpression> parameters)
        {
            FunctionDefAst = functionDef;
            Parameters = parameters;
        }

        internal readonly AST.FunctionDefinition FunctionDefAst;
        internal readonly List<DbVariableReferenceExpression> Parameters;

        internal abstract DbLambda GetLambda(SemanticResolver sr);
    }
}