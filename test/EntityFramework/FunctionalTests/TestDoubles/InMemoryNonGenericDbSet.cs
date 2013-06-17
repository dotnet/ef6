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

    public class InMemoryNonGenericDbSet<T> : DbSet, IQueryable
#if !NET40
, IDbAsyncEnumerable
#endif
        where T : class
    {
        private readonly HashSet<T> _data;
        private readonly InMemoryAsyncQueryable<T> _query;
        private readonly Func<HashSet<T>, object[], T> _find;
        private readonly Action<string, IEnumerable<T>> _include;
        private readonly Func<string, object[], DbSqlQuery> _sqlQuery;

        public InMemoryNonGenericDbSet(
            IEnumerable<T> data = null,
            Func<HashSet<T>, object[], T> find = null,
            Action<string, IEnumerable<T>> include = null,
            Func<string, object[], DbSqlQuery> sqlQuery = null)
        {
            _data = data == null ? new HashSet<T>() : new HashSet<T>(data);
            _query = new InMemoryAsyncQueryable<T>(
                _data.AsQueryable(), (p, d) =>
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

        public override object Find(params object[] keyValues)
        {
            if (_find == null)
            {
                throw new NotSupportedException("To use 'Find' pass a 'find' function when constructing the in-memory DbSet.");
            }

            return _find(_data, keyValues);
        }

        public override object Add(object item)
        {
            _data.Add((T)item);
            return item;
        }

        public override IEnumerable AddRange(IEnumerable entities)
        {
            foreach (T entity in entities)
            {
                _data.Add(entity);
            }
            return entities;
        }

        public override IEnumerable RemoveRange(IEnumerable entities)
        {
            foreach (T entity in entities)
            {
                _data.Remove(entity);
            }
            return entities;
        }

        public override object Remove(object item)
        {
            _data.Remove((T)item);
            return item;
        }

        public override object Attach(object item)
        {
            _data.Add((T)item);
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

        public override IList Local
        {
            get { return new ObservableListSource<T>(_data); }
        }

        public override object Create()
        {
            return Activator.CreateInstance<T>();
        }

        public override object Create(Type derivedEntityType)
        {
            return Activator.CreateInstance(derivedEntityType);
        }

        public override DbSqlQuery SqlQuery(string sql, params object[] parameters)
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

        public override DbQuery Include(string path)
        {
            if (_include != null)
            {
                _include(path, _data);
            }
            return this;
        }

        public DbSet<T> Cast()
        {
            return new InMemoryDbSet<T>(
                _data,
                _find,
                _include,
                (q, p) => new InMemorySqlQuery<T>(_sqlQuery(q, p).OfType<T>()));
        }

#if !NET40
        public override Task<object> FindAsync(CancellationToken cancellationToken, params object[] keyValues)
        {
            return Task.FromResult(Find(keyValues));
        }

        IDbAsyncEnumerator IDbAsyncEnumerable.GetAsyncEnumerator()
        {
            return new InMemoryDbAsyncEnumerator<T>(_data.GetEnumerator());
        }
#endif
    }
}
