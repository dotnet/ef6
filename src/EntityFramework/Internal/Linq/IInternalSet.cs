namespace System.Data.Entity.Internal.Linq
{
    using System.Collections;
    using System.Data.Objects;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     A non-generic interface implemented by <see cref = "InternalSet{TEntity}" /> that allows operations on
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
        IEnumerable ExecuteSqlQuery(string sql, bool asNoTracking, object[] parameters);
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

        IEnumerable IInternalSet.ExecuteSqlQuery(string sql, bool asNoTracking, object[] parameters)
        {
            Contract.Requires(sql != null);
            Contract.Requires(parameters != null);

            throw new NotImplementedException();
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

        IQueryProvider IInternalQuery.ObjectQueryProvider
        {
            get { throw new NotImplementedException(); }
        }

        IEnumerator IInternalQuery.GetEnumerator()
        {
            throw new NotImplementedException();
        }
    }
}
