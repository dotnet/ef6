// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestDoubles
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Threading;
    using System.Threading.Tasks;

    public class InMemoryDbSet<T> : DbSet<T>, IQueryable<T>
#if !NET40
        , IDbAsyncEnumerable<T>
#endif
        where T : class
    {
        private readonly HashSet<T> _data;
        private readonly InMemoryAsyncQueryable<T> _query;
        private readonly Func<HashSet<T>, object[], T> _find;
        private readonly Action<string, IEnumerable<T>> _include;
        private readonly Func<string, object[], DbSqlQuery<T>> _sqlQuery;

        public InMemoryDbSet(
            IEnumerable<T> data = null,
            Func<HashSet<T>, object[], T> find = null,
            Action<string, IEnumerable<T>> include = null,
            Func<string, object[], DbSqlQuery<T>> sqlQuery = null)
        {
            _data = data == null ? new HashSet<T>() : new HashSet<T>(data);
            _query = new InMemoryAsyncQueryable<T>(_data.AsQueryable(), (p, d) =>
                {
                    var asEntityEnumerable = d as IEnumerable<T>;
                    if (include != null
                        && asEntityEnumerable != null)
                    {
                        include(p, asEntityEnumerable);
                    }
                });
            _find = find;
            _include = include;
            _sqlQuery = sqlQuery;
        }

        public override T Find(params object[] keyValues)
        {
            if (_find == null)
            {
                throw new NotSupportedException("To use 'Find' pass a 'find' function when constructing the in-memory DbSet.");
            }

            return _find(_data, keyValues);
        }

        public override T Add(T item)
        {
            _data.Add(item);
            return item;
        }

        public override IEnumerable<T> AddRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _data.Add(entity);
            }
            return entities;
        }

        public override IEnumerable<T> RemoveRange(IEnumerable<T> entities)
        {
            foreach (var entity in entities)
            {
                _data.Remove(entity);
            }
            return entities;
        }

        public override T Remove(T item)
        {
            _data.Remove(item);
            return item;
        }

        public override T Attach(T item)
        {
            _data.Add(item);
            return item;
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public override DbLocalView<T> Local
        {
            get { return new DbLocalView<T>(_data); }
        }

        public override T Create()
        {
            return Activator.CreateInstance<T>();
        }

        public override TDerivedEntity Create<TDerivedEntity>()
        {
            return Activator.CreateInstance<TDerivedEntity>();
        }

        public virtual void AddOrUpdate(params T[] entities)
        {
            AddOrUpdate(null, entities);
        }

        public virtual void AddOrUpdate(Expression<Func<T, object>> identifierExpression, params T[] entities)
        {
            // This is not a real implementation of AddOrUpdate; it only adds
            AddRange(entities);
        }

        public override DbSqlQuery<T> SqlQuery(string sql, params object[] parameters)
        {
            if (_sqlQuery == null)
            {
                throw new NotSupportedException("To use 'SqlQuery' pass a 'sqlQuery' function when constructing the in-memory DbSet.");
            }

            return _sqlQuery(sql, parameters);
        }

        public override string ToString()
        {
            return "An in-memory DbSet";
        }

        public override DbQuery<T> Include(string path)
        {
            if (_include != null)
            {
                _include(path, _data);
            }
            return this;
        }

        public static implicit operator DbSet(InMemoryDbSet<T> entry)
        {
            return new InMemoryNonGenericDbSet<T>(
                entry._data, 
                entry._find, 
                entry._include, 
                (q, p) => new InMemoryNonGenericSqlQuery<T>(entry._sqlQuery(q, p)));
        }

#if !NET40
        public override Task<T> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }

        IDbAsyncEnumerator<T> IDbAsyncEnumerable<T>.GetAsyncEnumerator()
        {
            return new InMemoryDbAsyncEnumerator<T>(_data.GetEnumerator());
        }
#endif
    }
}
