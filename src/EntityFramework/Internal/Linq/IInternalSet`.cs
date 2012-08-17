// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Objects;
    using System.Data.Entity.Core.Objects.ELinq;
    using System.Data.Entity.Infrastructure;
    using System.Diagnostics.Contracts;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    /// <summary>
    ///     An interface implemented by <see cref="InternalSet{TEntity}" />.
    /// </summary>
    [ContractClass(typeof(IInternalSetContracts<>))]
    internal interface IInternalSet<TEntity> : IInternalSet, IInternalQuery<TEntity>
        where TEntity : class
    {
        TEntity Find(params object[] keyValues);

#if !NET40

        Task<TEntity> FindAsync(CancellationToken cancellationToken, params object[] keyValues);

#endif

        TEntity Create();
        TEntity Create(Type derivedEntityType);
        ObservableCollection<TEntity> Local { get; }
    }

    [ContractClassFor(typeof(IInternalSet<>))]
    internal abstract class IInternalSetContracts<TEntity> : IInternalSet<TEntity>
        where TEntity : class
    {
        TEntity IInternalSet<TEntity>.Find(params object[] keyValues)
        {
            throw new NotImplementedException();
        }

#if !NET40

        Task<TEntity> IInternalSet<TEntity>.FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            throw new NotImplementedException();
        }

#endif

        TEntity IInternalSet<TEntity>.Create()
        {
            throw new NotImplementedException();
        }

        TEntity IInternalSet<TEntity>.Create(Type derivedEntityType)
        {
            Contract.Requires(derivedEntityType != null);

            throw new NotImplementedException();
        }

        ObservableCollection<TEntity> IInternalSet<TEntity>.Local
        {
            get { throw new NotImplementedException(); }
        }

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

        IInternalQuery<TEntity> IInternalQuery<TEntity>.Include(string path)
        {
            throw new NotImplementedException();
        }

        IInternalQuery<TEntity> IInternalQuery<TEntity>.AsNoTracking()
        {
            throw new NotImplementedException();
        }

#if !NET40

        IDbAsyncEnumerator<TEntity> IInternalQuery<TEntity>.GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }

        IDbAsyncEnumerator IInternalQuery.GetAsyncEnumerator()
        {
            throw new NotImplementedException();
        }

#endif

        IEnumerator<TEntity> IInternalQuery<TEntity>.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        IEnumerator IInternalQuery.GetEnumerator()
        {
            throw new NotImplementedException();
        }

        void IInternalSet.Attach(object entity)
        {
            throw new NotImplementedException();
        }

        void IInternalSet.Add(object entity)
        {
            throw new NotImplementedException();
        }

        void IInternalSet.Remove(object entity)
        {
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
            throw new NotImplementedException();
        }

#if !NET40

        IDbAsyncEnumerator IInternalSet.ExecuteSqlQueryAsync(string sql, bool asNoTracking, object[] parameters)
        {
            throw new NotImplementedException();
        }

#endif

    }
}
