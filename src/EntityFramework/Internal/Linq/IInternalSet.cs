// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;

    /// <summary>
    ///     A non-generic interface implemented by <see cref="InternalSet{TEntity}" /> that allows operations on
    ///     any set object without knowing the type to which it applies.
    /// </summary>
    [ContractClass(typeof(IInternalSetContracts))]
    internal interface IInternalSet : IInternalQuery
    {
        void Attach(object entity);
        void Add(object entity);
        void Remove(object entity);
        void Initialize();
        void TryInitialize();
        IEnumerator ExecuteSqlQuery(string sql, bool asNoTracking, object[] parameters);

#if !NET40

        IDbAsyncEnumerator ExecuteSqlQueryAsync(string sql, bool asNoTracking, object[] parameters);

#endif
    }

    [ContractClassFor(typeof(IInternalSet))]
    internal abstract class IInternalSetContracts : IInternalSet
    {
        void IInternalSet.Attach(object entity)
        {
            Contract.Requires(entity != null);

            throw new NotImplementedException();
        }

        void IInternalSet.Add(object entity)
        {
            Contract.Requires(entity != null);

            throw new NotImplementedException();
        }

        void IInternalSet.Remove(object entity)
        {
            Contract.Requires(entity != null);

            throw new NotImplementedException();
        }

        void IInternalSet.Initialize()
        {
            throw new NotImplementedException();
        }

        void IInternalSet.TryInitialize()
        {
            throw new NotImplementedException();
        }

        IEnumerator IInternalSet.ExecuteSqlQuery(string sql, bool asNoTracking, object[] parameters)
        {
            Contract.Requires(sql != null);
            Contract.Requires(parameters != null);

            throw new NotImplementedException();
        }

#if !NET40

        IDbAsyncEnumerator IInternalSet.ExecuteSqlQueryAsync(string sql, bool asNoTracking, object[] parameters)
        {
            Contract.Requires(sql != null);
            Contract.Requires(parameters != null);

            throw new NotImplementedException();
        }

#endif

        void IInternalQuery.ResetQuery()
        {
            throw new NotImplementedException();
        }

        InternalContext IInternalQuery.InternalContext
        {
            get { throw new NotImplementedException(); }
        }

        ObjectQuery IInternalQuery.ObjectQuery
        {
            get { throw new NotImplementedException(); }
        }

        Type IInternalQuery.ElementType
        {
            get { throw new NotImplementedException(); }
        }

        Expression IInternalQuery.Expression
        {
            get { throw new NotImplementedException(); }
        }

        ObjectQueryProvider IInternalQuery.ObjectQueryProvider
        {
            get { throw new NotImplementedException(); }
        }

#if !NET40

        IDbAsyncEnumerator IInternalQuery.GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }

#endif

        IEnumerator IInternalQuery.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
