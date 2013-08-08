// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Text;

    /// <summary>
    /// This class represents a collection of query parameters at the object layer.
    /// </summary>
    public class ObjectParameterCollection : ICollection<ObjectParameter>
    {
        // Note: There are NO public constructors for this class - it is for internal
        // ObjectQuery<T> use only, but must be public so that an instance thereof can be
        // a public property on ObjectQuery<T>.

        #region Internal Constructors

        /// <summary>
        /// This internal constructor creates a new query parameter collection and
        /// initializes the internal parameter storage.
        /// </summary>
        internal ObjectParameterCollection(ClrPerspective perspective)
        {
            DebugCheck.NotNull(perspective);

            // The perspective is required to do type-checking on parameters as they
            // are added to the collection.
            _perspective = perspective;

            // Create a new list to store the parameters.
            _parameters = new List<ObjectParameter>();
        }

        #endregion

        #region Private Fields

        /// <summary>
        /// Can parameters be added or removed from this collection?
        /// </summary>
        private bool _locked;

        /// <summary>
        /// The internal storage for the query parameters in the collection.
        /// </summary>
        private readonly List<ObjectParameter> _parameters;

        /// <summary>
        /// A CLR perspective necessary to do type-checking on parameters as they
        /// are added to the collection.
        /// </summary>
        private readonly ClrPerspective _perspective;

        /// <summary>
        /// A string that can be used to represent the current state of this parameter collection in an ObjectQuery cache key.
        /// </summary>
        private string _cacheKey;

        #endregion

        #region Public Properties

        /// <summary>Gets the number of parameters currently in the collection.</summary>
        /// <returns>
        /// The number of <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> objects that are currently in the collection.
        /// </returns>
        public int Count
        {
            get { return _parameters.Count; }
        }

        /// <summary>
        /// This collection is read-write - parameters may be added, removed
        /// and [somewhat] modified at will (value only) - provided that the
        /// implementation the collection belongs to has not locked its parameters
        /// because it's command definition has been prepared.
        /// </summary>
        bool ICollection<ObjectParameter>.IsReadOnly
        {
            get { return (_locked); }
        }

        #endregion

        #region Public Indexers

        /// <summary>Provides an indexer that allows callers to retrieve parameters by name.</summary>
        /// <returns>
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> instance.
        /// </returns>
        /// <param name="name">The name of the parameter to find. This name should not include the "@" parameter marker that is used in the Entity SQL statements, only the actual name.</param>
        /// <exception cref="T:System.ArgumentOutOfRangeException">No parameter with the specified name is found in the collection.</exception>
        public ObjectParameter this[string name]
        {
            get
            {
                var index = IndexOf(name);

                if (index == -1)
                {
                    throw new ArgumentOutOfRangeException("name", Strings.ObjectParameterCollection_ParameterNameNotFound(name));
                }

                return _parameters[index];
            }
        }

        #endregion

        #region Public Methods

        #region Add

        /// <summary>
        /// Adds the specified <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> to the collection.
        /// </summary>
        /// <param name="item">The parameter to add to the collection.</param>
        /// <exception cref="T:System.ArgumentNullException">The  parameter  argument is null.</exception>
        /// <exception cref="T:System.ArgumentException">
        /// The  parameter  argument already exists in the collection. This behavior differs from that of most collections that allow duplicate entries. -or-Another parameter with the same name as the  parameter  argument already exists in the collection. Note that the lookup is case-insensitive. This behavior differs from that of most collections, and is more like that of a
        /// <see
        ///     cref="T:System.Collections.Generic.Dictionary" />
        /// .
        /// </exception>
        /// <exception cref="T:System.ArgumentOutOfRangeException">The type of the  parameter  is not valid.</exception>
        public void Add(ObjectParameter item)
        {
            Check.NotNull(item, "item");

            CheckUnlocked();

            if (Contains(item))
            {
                throw new ArgumentException(Strings.ObjectParameterCollection_ParameterAlreadyExists(item.Name), "item");
            }

            if (Contains(item.Name))
            {
                throw new ArgumentException(Strings.ObjectParameterCollection_DuplicateParameterName(item.Name), "item");
            }

            if (!item.ValidateParameterType(_perspective))
            {
                throw new ArgumentOutOfRangeException("item", Strings.ObjectParameter_InvalidParameterType(item.ParameterType.FullName));
            }

            _parameters.Add(item);
            _cacheKey = null;
        }

        #endregion

        #region Clear

        /// <summary>
        /// Deletes all <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> instances from the collection.
        /// </summary>
        public void Clear()
        {
            CheckUnlocked();
            _parameters.Clear();
            _cacheKey = null;
        }

        #endregion

        #region Contains (ObjectParameter)

        /// <summary>
        /// Checks for the existence of a specified <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> in the collection by reference.
        /// </summary>
        /// <returns>Returns true if the parameter object was found in the collection; otherwise, false.  </returns>
        /// <param name="item">
        /// The <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> to find in the collection.
        /// </param>
        /// <exception cref="T:System.ArgumentNullException">The  parameter  argument is null.</exception>
        public bool Contains(ObjectParameter item)
        {
            Check.NotNull(item, "item");

            return _parameters.Contains(item);
        }

        #endregion

        #region Contains (string)

        /// <summary>
        /// Determines whether an <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> with the specified name is in the collection.
        /// </summary>
        /// <returns>Returns true if a parameter with the specified name was found in the collection; otherwise, false.</returns>
        /// <param name="name">The name of the parameter to look for in the collection. This name should not include the "@" parameter marker that is used in the Entity SQL statements, only the actual name.</param>
        /// <exception cref="T:System.ArgumentNullException">The  name  parameter is null.</exception>
        public bool Contains(string name)
        {
            Check.NotNull(name, "name");

            if (IndexOf(name)
                != -1)
            {
                return true;
            }

            return false;
        }

        #endregion

        #region CopyTo

        /// <summary>Allows the parameters in the collection to be copied into a supplied array, starting with the object at the specified index.</summary>
        /// <param name="array">The array into which to copy the parameters.</param>
        /// <param name="arrayIndex">The index in the array at which to start copying the parameters.</param>
        public void CopyTo(ObjectParameter[] array, int arrayIndex)
        {
            _parameters.CopyTo(array, arrayIndex);
        }

        #endregion

        #region Remove

        /// <summary>
        /// Removes an instance of an <see cref="T:System.Data.Entity.Core.Objects.ObjectParameter" /> from the collection by reference if it exists in the collection.
        /// </summary>
        /// <returns>Returns true if the parameter object was found and removed from the collection; otherwise, false. </returns>
        /// <param name="item">An object to remove from the collection.</param>
        /// <exception cref="T:System.ArgumentNullException">The  parameter  argument is null.</exception>
        public bool Remove(ObjectParameter item)
        {
            Check.NotNull(item, "item");

            CheckUnlocked();

            var removed = _parameters.Remove(item);

            // If the specified parameter was found in the collection and removed, 
            // clear out the cached string representation of this parameter collection
            // so that the next call to GetCacheKey (if any) will regenerate it based on
            // the new state of this collection.
            if (removed)
            {
                _cacheKey = null;
            }

            return removed;
        }

        #endregion

        #region GetEnumerator

        /// <summary>
        /// These methods return enumerator instances, which allow the collection to
        /// be iterated through and traversed.
        /// </summary>
        public virtual IEnumerator<ObjectParameter> GetEnumerator()
        {
            return ((ICollection<ObjectParameter>)_parameters).GetEnumerator();
        }

        /// <summary>Returns an untyped enumerator over the collection. </summary>
        /// <returns>
        /// An <see cref="T:System.Collections.IEnumerator" /> instance.
        /// </returns>
        IEnumerator IEnumerable.GetEnumerator()
        {
            return ((ICollection)_parameters).GetEnumerator();
        }

        #endregion

        #endregion

        #region Internal Methods

        /// <summary>
        /// Retrieves a string that may be used to represent this parameter collection in an ObjectQuery cache key.
        /// If this collection has not changed since the last call to this method, the same string instance is returned.
        /// Note that this string is used by various ObjectQueryImplementations to version the parameter collection.
        /// </summary>
        /// <returns> A string that may be used to represent this parameter collection in an ObjectQuery cache key. </returns>
        internal string GetCacheKey()
        {
            if (null == _cacheKey)
            {
                if (_parameters.Count > 0)
                {
                    // Future Enhancement: If the separate branch for a single parameter does not have a measurable perf advantage, remove it.
                    if (1 == _parameters.Count)
                    {
                        // if its one parameter only, there is no need to use stringbuilder
                        var theParam = _parameters[0];
                        _cacheKey = "@@1" + theParam.Name + ":" + theParam.ParameterType.FullName;
                    }
                    else
                    {
                        // Future Enhancement: Investigate whether precalculating the required size of the string builder is a better time/space tradeoff.
                        var keyBuilder = new StringBuilder(_parameters.Count * 20);
                        keyBuilder.Append("@@");
                        keyBuilder.Append(_parameters.Count);
                        for (var idx = 0; idx < _parameters.Count; idx++)
                        {
                            //
                            // CONSIDER adding other parameter properties
                            //
                            if (idx > 0)
                            {
                                keyBuilder.Append(";");
                            }

                            var thisParam = _parameters[idx];
                            keyBuilder.Append(thisParam.Name);
                            keyBuilder.Append(":");
                            keyBuilder.Append(thisParam.ParameterType.FullName);
                        }

                        _cacheKey = keyBuilder.ToString();
                    }
                }
            }

            return _cacheKey;
        }

        /// <summary>
        /// Locks or unlocks this parameter collection, allowing its contents to be added to, removed from, or cleared.
        /// Calling this method consecutively with the same value has no effect but does not throw an exception.
        /// </summary>
        /// <param name="isReadOnly">
        /// If <c>true</c> , this parameter collection is now locked; otherwise it is unlocked
        /// </param>
        internal void SetReadOnly(bool isReadOnly)
        {
            _locked = isReadOnly;
        }

        /// <summary>
        /// Creates a new copy of the specified parameter collection containing copies of its element
        /// <see
        ///     cref="ObjectParameter" />
        /// s.
        /// If the specified argument is <c>null</c>, then <c>null</c> is returned.
        /// </summary>
        /// <param name="copyParams"> The parameter collection to copy </param>
        /// <returns>
        /// The new collection containing copies of <paramref name="copyParams" /> parameters, if
        /// <paramref
        ///     name="copyParams" />
        /// is non-null; otherwise <c>null</c> .
        /// </returns>
        internal static ObjectParameterCollection DeepCopy(ObjectParameterCollection copyParams)
        {
            if (null == copyParams)
            {
                return null;
            }

            var retParams = new ObjectParameterCollection(copyParams._perspective);
            foreach (var param in copyParams)
            {
                retParams.Add(param.ShallowCopy());
            }

            return retParams;
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// This private method checks for the existence of a given parameter object
        /// by name by iterating through the list and comparing each parameter name
        /// to the specified name. This is a case-insensitive lookup.
        /// </summary>
        private int IndexOf(string name)
        {
            var index = 0;

            foreach (var parameter in _parameters)
            {
                if (0 == String.Compare(name, parameter.Name, StringComparison.OrdinalIgnoreCase))
                {
                    return index;
                }

                index++;
            }

            return -1;
        }

        /// <summary>
        /// This method successfully returns only if the parameter collection is not considered 'locked';
        /// otherwise an <see cref="InvalidOperationException" /> is thrown.
        /// </summary>
        private void CheckUnlocked()
        {
            if (_locked)
            {
                throw new InvalidOperationException(Strings.ObjectParameterCollection_ParametersLocked);
            }
        }

        #endregion
    }
}
