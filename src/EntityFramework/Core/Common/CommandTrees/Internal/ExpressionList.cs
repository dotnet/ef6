using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.Data.Entity.Core.Common;
using System.Data.Common;
using System.Data.Entity.Core.Metadata.Edm;
using System.Data.Entity.Core.Common.CommandTrees;

namespace System.Data.Entity.Core.Common.CommandTrees.Internal
{
    internal sealed class DbExpressionList : System.Collections.ObjectModel.ReadOnlyCollection<DbExpression>
    {
        internal DbExpressionList(IList<DbExpression> elements) 
            : base(elements) 
        {
        }
    }
}