// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class ParameterRetriever : BasicCommandTreeVisitor
    {
        private readonly Dictionary<string, DbParameterReferenceExpression> paramMappings =
            new Dictionary<string, DbParameterReferenceExpression>();

        private ParameterRetriever()
        {
        }

        internal static ReadOnlyCollection<DbParameterReferenceExpression> GetParameters(DbCommandTree tree)
        {
            Debug.Assert(tree != null, "Ensure command tree is non-null before calling ParamterRetriever.GetParameters");

            var retriever = new ParameterRetriever();
            retriever.VisitCommandTree(tree);
            return retriever.paramMappings.Values.ToList().AsReadOnly();
        }

        public override void Visit(DbParameterReferenceExpression expression)
        {
            Debug.Assert(expression != null, "Command tree subexpressions should never be null");

            paramMappings[expression.ParameterName] = expression;
        }
    }
}
