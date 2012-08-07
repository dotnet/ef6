// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;

    internal sealed class DbExpressionList : ReadOnlyCollection<DbExpression>
    {
        internal DbExpressionList(IList<DbExpression> elements)
            : base(elements)
        {
        }
    }
}
