namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Linq;

    /// <summary>
    /// In-memory implementation of IDbSet based on a <see cref="HashSet"/>
    /// </summary>
    /// <typeparam name="T">Type of elements to be stored in the set</typeparam>
    public class HashSetBasedDbSet<T> : IDbSet<T>
        where T : class, new()
    {
        HashSet<T> _data;
        IQueryable _query;
        Func<IEnumerable<T>, T> _findFunc;

        public HashSetBasedDbSet()
            : this(null)
        {
        }

        public HashSetBasedDbSet(Func<IEnumerable<T>, T> findFunc) 
        {
            _data = new HashSet<T>();
            _query = _data.AsQueryable();
            _findFunc = findFunc;
        }

        public T Find(params object[] keyValues)
        {
            if (_findFunc == null)
            {
                throw new NotSupportedException("If you want to call find then use the constructor that specifies a find func.");
            }

            return _findFunc(_data);
        }

        public T Add(T item)
        {
            _data.Add(item);
            return item;
        }

        public T Remove(T item)
        {
            _data.Remove(item);
            return item;
        }

        public T Attach(T item)
        {
            _data.Add(item);
            return item;
        }

        Type IQueryable.ElementType
        {
            get { return _query.ElementType; }
        }

        System.Linq.Expressions.Expression IQueryable.Expression
        {
            get { return _query.Expression; }
        }

        IQueryProvider IQueryable.Provider
        {
            get { return _query.Provider; }
        }

        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        IEnumerator<T> IEnumerable<T>.GetEnumerator()
        {
            return _data.GetEnumerator();
        }

        public ObservableCollection<T> Local
        {
            get { return new ObservableCollection<T>(_data); }
        }

        public T Create()
        {
            return new T();
        }

        public TDerivedEntity Create<TDerivedEntity>() where TDerivedEntity : class, T
        {
            throw new NotImplementedException();
        }
    }
}