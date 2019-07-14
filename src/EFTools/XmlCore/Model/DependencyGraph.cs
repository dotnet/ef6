// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;

    /// <summary>
    ///     Class for managing and maintaining dependency info.  This class is a specialized form of a
    ///     directed graph  where each edge in one direction has a correspending "anti-edge" in the opposite
    ///     direction.  That is, an edge from U to V implies an anti-edge from V to U. Here, we call the
    ///     edges "Dependency" and the anti-edges "Anti-Dependency".
    /// </summary>
    internal class DependencyGraph<T>
    {
        #region Members

        // We weren't using an of the dependency info, only the anti-dependency data.  To reduce memory usage, we're not enabling
        // tracking of dependencies.  This can be turned back on by defining the TRACK_DEPENDENCIES macro.
#if TRACK_DEPENDENCIES
    /// <summary>
    /// Map from an Item to that Item's direct dependencies
    /// </summary>
        protected Dictionary<T, List<T>> _dependencyMap = new Dictionary<T, List<T>>();
#endif

        /// <summary>
        ///     Map from an Item to that Item's direct anti-dependencies.  An anti-dependency is a reverse dependency relation.
        ///     For example, if <code>Customer</code> is derived from <code>Person</code>, then <code>Customer</code> has a dependency on
        ///     <code>Person</code>, and <code>Person</code> has an "anti-dependency" on <code>Customer</code>.
        /// </summary>
        protected Dictionary<T, List<T>> _antiDependencyMap = new Dictionary<T, List<T>>();

        #endregion

        #region internal api methods

#if TRACK_DEPENDENCIES

    /// <summary>
    /// Retrieve the direct dependents of Item item
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
        internal ICollection<T> GetDependencies(T item)
        {
            List<T> l = null;
            if (_dependencyMap.TryGetValue(item, out l))
            {
                // TODO  return read-only wrapper around list
                return l;
            }
            else
            {
                return new List<T>(0);
            }
        }
#endif

        /// <summary>
        ///     Retrive all direct anti-dependents of Item item.  An anti-dependency is a reverse dependency relation.
        ///     For example, if <code>Customer</code> is derived from <code>Person</code>, then <code>Customer</code> has a dependency on
        ///     <code>Person</code>, and <code>Person</code> has an "anti-dependency" on <code>Customer</code>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal ICollection<T> GetAntiDependencies(T item)
        {
            List<T> l = null;
            if (_antiDependencyMap.TryGetValue(item, out l))
            {
                return l.AsReadOnly();
            }
            else
            {
                return new List<T>(0);
            }
        }

        #endregion

        #region internal methods

        /// <summary>
        ///     Add a new dependency from item->dependency.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dependency"></param>
        internal void AddDependency(T item, T dependency)
        {
            if (item != null
                && item.Equals(dependency))
            {
                Debug.Assert(true, "Attempted to add a cyclic dependency to graph");
                return;
            }

#if TRACK_DEPENDENCIES
            List<T> deps = null;
            if (!_dependencyMap.TryGetValue(item, out deps))
            {
                deps = new List<T>();
                _dependencyMap.Add(item, deps);
            }
                deps.Add(dependency);
#endif

            List<T> antiDeps = null;
            if (!_antiDependencyMap.TryGetValue(dependency, out antiDeps))
            {
                antiDeps = new List<T>();
                _antiDependencyMap.Add(dependency, antiDeps);
            }
            antiDeps.Add(item);
        }

        /// <summary>
        ///     Add a new dependency from item->dependency.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="dependency"></param>
        internal void RemoveDependency(T item, T dependency)
        {
#if TRACK_DEPENDENCIES
            List<T> deps = null;
            _dependencyMap.TryGetValue(item, out deps);

            if (deps == null || !deps.Contains(dependency))
            {
                Debug.Assert(false, "item does not have specified dependency");
            }
            else
            {
                deps.Remove(dependency);
            }

            // remove this from the map if we have no more deps
            if (deps.Count == 0)
            {
                _dependencyMap.Remove(item);
            }
#endif

            List<T> antiDeps = null;
            _antiDependencyMap.TryGetValue(dependency, out antiDeps);
            if (antiDeps == null
                || !antiDeps.Contains(item))
            {
                Debug.Assert(false, "dependency does not have specified anti-dependency");
            }
            else
            {
                antiDeps.Remove(item);
            }

            // remove this from the map if we have no more anti-deps
            if (antiDeps.Count == 0)
            {
                _antiDependencyMap.Remove(dependency);
            }
        }

        #endregion

        #region private Delegates

        private delegate ICollection<T> EdgeFunction(T i);

        #endregion

        #region private helper classes

        /// <summary>
        ///     a queue combined with a hash set.  This provides constant-time lookup of individual items,
        ///     but still provides first-in first-out behavior.
        /// </summary>
        /// <typeparam name="T"></typeparam>
        private class QueueHashSet<R>
        {
            private readonly HashSet<R> _dictionary = new HashSet<R>();
            private readonly LinkedList<R> _queue = new LinkedList<R>();

            internal void Enqueue(R t)
            {
                _dictionary.Add(t);
                _queue.AddLast(t);
                Debug.Assert(_dictionary.Count == _queue.Count);
            }

            internal bool Contains(R t)
            {
                return _dictionary.Contains(t);
            }

            internal R Dequeue()
            {
                var t = _queue.First.Value;
                _queue.RemoveFirst();
                _dictionary.Remove(t);
                Debug.Assert(_dictionary.Count == _queue.Count);
                return t;
            }

            internal int Count
            {
                get { return _queue.Count; }
            }

            internal ICollection<R> Queue
            {
                get { return new ReadOnlyCollection<R>(_queue); }
            }
        }

        #endregion

        #region Test Only Code

#if TRACK_DEPENDENCIES
    /// <summary>
    /// Retrieve all direct and indirect dependents of Item item.  
    /// </summary>
    /// <param name="item"></param>
    /// <returns></returns>
        internal ICollection<T> GetDependenciesClosure(T item)
        {
            // TODO  return read-only wrapper around list
            return GetClosure(item, new EdgeFunction(GetDependencies));
        }
#endif

        /// <summary>
        ///     Retrieve all direct and indirect anti-dependents of Item item.  An anti-dependency is a reverse dependency relation.
        ///     For example, if <code>Customer</code> is derived from <code>Person</code>, then <code>Customer</code> has a dependency on
        ///     <code>Person</code>, and <code>Person</code> has an "anti-dependency" on <code>Customer</code>.
        /// </summary>
        /// <param name="item"></param>
        /// <returns></returns>
        internal ICollection<T> GetAntiDependenciesClosure(T item)
        {
            // TODO  return read-only wrapper around list
            return GetClosure(item, GetAntiDependencies);
        }

        /// <summary>
        ///     do a breadth-first search to find all nodes reachable from item.
        ///     edgeFunction determines the nodes directly reachable from any
        ///     given item.
        /// </summary>
        /// <param name="item"></param>
        /// <param name="closureFunction"></param>
        /// <returns></returns>
        private static ICollection<T> GetClosure(T item, EdgeFunction edgeFunction)
        {
            var closure = new QueueHashSet<T>();
            var queue = new QueueHashSet<T>();

            foreach (var i in edgeFunction(item))
            {
                queue.Enqueue(i);
            }

            while (queue.Count > 0)
            {
                // get first element in queue
                var i = queue.Dequeue();
                if (!closure.Contains(i))
                {
                    closure.Enqueue(i);
                    foreach (var j in edgeFunction(i))
                    {
                        if (!queue.Contains(j)
                            && !closure.Contains(j))
                        {
                            queue.Enqueue(j);
                        }
                    }
                }
            }

            return closure.Queue;
        }

        #endregion
    }
}
