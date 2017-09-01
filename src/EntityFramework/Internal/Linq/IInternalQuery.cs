// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Linq.Expressions;

    // <summary>
    // A non-generic interface implemented by <see cref="InternalQuery{TElement}" /> that allows operations on
    // any query object without knowing the type to which it applies.
    // </summary>
    internal interface IInternalQuery
    {
        void ResetQuery();
        InternalContext InternalContext { get; }
        ObjectQuery ObjectQuery { get; }

        Type ElementType { get; }
        Expression Expression { get; }
        ObjectQueryProvider ObjectQueryProvider { get; }

        string ToTraceString();
 
#if !NET40

        IDbAsyncEnumerator GetAsyncEnumerator();

#endif

        IEnumerator GetEnumerator();
    }
}
