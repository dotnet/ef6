// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Reflection;
    using System.Threading;

    /// <summary>
    /// Class for representing a collection of items.
    /// Most of the implementation for actual maintenance of the collection is
    /// done by MetadataCollection
    /// </summary>
    public abstract class ItemCollection : ReadOnlyMetadataCollection<GlobalItem>
    {
        internal ItemCollection()
        {
        }

        /// <summary>
        /// The default constructor for ItemCollection
        /// </summary>
        internal ItemCollection(DataSpace dataspace)
            : base(new MetadataCollection<GlobalItem>())
        {
            _space = dataspace;
        }

        private readonly DataSpace _space;
        private Dictionary<string, ReadOnlyCollection<EdmFunction>> _functionLookUpTable;
        private Memoizer<Type, ICollection> _itemsCache;
        private int _itemCount;

        /// <summary>Gets the data model associated with this item collection. </summary>
        /// <returns>The data model associated with this item collection. </returns>
        public DataSpace DataSpace
        {
            get { return _space; }
        }

        /// <summary>
        /// Return the function lookUpTable
        /// </summary>
        internal Dictionary<string, ReadOnlyCollection<EdmFunction>> FunctionLookUpTable
        {
            get
            {
                if (_functionLookUpTable == null)
                {
                    var functionLookUpTable = PopulateFunctionLookUpTable(this);
                    Interlocked.CompareExchange(ref _functionLookUpTable, functionLookUpTable, null);
                }

                return _functionLookUpTable;
            }
        }

        /// <summary>
        /// Adds an item to the collection
        /// </summary>
        /// <param name="item"> The item to add to the list </param>
        /// <exception cref="System.ArgumentNullException">Thrown if item argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the item passed in or the collection itself instance is in ReadOnly state</exception>
        /// <exception cref="System.ArgumentException">Thrown if the item that is being added already belongs to another ItemCollection</exception>
        /// <exception cref="System.ArgumentException">Thrown if the ItemCollection already contains an item with the same identity</exception>
        internal void AddInternal(GlobalItem item)
        {
            Debug.Assert(item.IsReadOnly, "The item is not readonly, it should be by the time it is added to the item collection");
            Debug.Assert(item.DataSpace == DataSpace);
            base.Source.Add(item);
        }

        /// <summary>
        /// Adds a collection of items to the collection
        /// </summary>
        /// <param name="items"> The items to add to the list </param>
        /// <exception cref="System.ArgumentNullException">Thrown if item argument is null</exception>
        /// <exception cref="System.InvalidOperationException">Thrown if the item passed in or the collection itself instance is in ReadOnly state</exception>
        /// <exception cref="System.ArgumentException">Thrown if the item that is being added already belongs to another ItemCollection</exception>
        /// <exception cref="System.ArgumentException">Thrown if the ItemCollection already contains an item with the same identity</exception>
        internal bool AtomicAddRange(List<GlobalItem> items)
        {
#if DEBUG
    // We failed to add, so undo the setting of the ItemCollection reference
            foreach (var item in items)
            {
                Debug.Assert(item.IsReadOnly, "The item is not readonly, it should be by the time it is added to the item collection");
                Debug.Assert(item.DataSpace == DataSpace);
            }

#endif
            if (base.Source.AtomicAddRange(items))
            {
                return true;
            }

            return false;
        }

        /// <summary>
        /// Returns a strongly typed <see cref="T:System.Data.Entity.Core.Metadata.Edm.GlobalItem" /> object by using the specified identity.
        /// </summary>
        /// <returns>The item that is specified by the identity.</returns>
        /// <param name="identity">The identity of the item.</param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public T GetItem<T>(string identity) where T : GlobalItem
        {
            return GetItem<T>(identity, false /*ignoreCase*/);
        }

        /// <summary>
        /// Returns a strongly typed <see cref="T:System.Data.Entity.Core.Metadata.Edm.GlobalItem" /> object by using the specified identity from this item collection.
        /// </summary>
        /// <returns>true if there is an item that matches the search criteria; otherwise, false.</returns>
        /// <param name="identity">The identity of the item.</param>
        /// <param name="item">
        /// When this method returns, the output parameter contains a
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.GlobalItem" />
        /// object. If there is no global item with the specified identity in the item collection, this output parameter contains null.
        /// </param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public bool TryGetItem<T>(string identity, out T item) where T : GlobalItem
        {
            return TryGetItem(identity, false /*ignorecase*/, out item);
        }

        /// <summary>
        /// Returns a strongly typed <see cref="T:System.Data.Entity.Core.Metadata.Edm.GlobalItem" /> object by using the specified identity from this item collection.
        /// </summary>
        /// <returns>true if there is an item that matches the search criteria; otherwise, false.</returns>
        /// <param name="identity">The identity of the item.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="item">
        /// When this method returns, the output parameter contains a
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.GlobalItem" />
        /// object. If there is no global item with the specified identity in the item collection, this output parameter contains null.
        /// </param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public bool TryGetItem<T>(string identity, bool ignoreCase, out T item) where T : GlobalItem
        {
            GlobalItem outItem = null;
            TryGetValue(identity, ignoreCase, out outItem);
            item = outItem as T;
            return item != null;
        }

        /// <summary>
        /// Returns a strongly typed <see cref="T:System.Data.Entity.Core.Metadata.Edm.GlobalItem" /> object by using the specified identity with either case-sensitive or case-insensitive search.
        /// </summary>
        /// <returns>The item that is specified by the identity.</returns>
        /// <param name="identity">The identity of the item.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public T GetItem<T>(string identity, bool ignoreCase) where T : GlobalItem
        {
            T item;
            if (TryGetItem(identity, ignoreCase, out item))
            {
                return item;
            }
            throw new ArgumentException(Strings.ItemInvalidIdentity(identity), "identity");
        }

        /// <summary>Returns all the items of the specified type from this item collection.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the items of the specified type.
        /// </returns>
        /// <typeparam name="T">The type returned by the method.</typeparam>
        public virtual ReadOnlyCollection<T> GetItems<T>() where T : GlobalItem
        {
            var currentValueForItemCache = _itemsCache;
            // initialize the memoizer, update the _itemCache and _itemCount
            if (_itemsCache == null
                || _itemCount != Count)
            {
                var itemsCache =
                    new Memoizer<Type, ICollection>(InternalGetItems, null);
                Interlocked.CompareExchange(ref _itemsCache, itemsCache, currentValueForItemCache);

                _itemCount = Count;
            }

            Debug.Assert(_itemsCache != null, "check the initialization of the Memoizer");

            // use memoizer so that it won't create a new list every time this method get called
            var items = _itemsCache.Evaluate(typeof(T));
            var returnItems = items as ReadOnlyCollection<T>;

            return returnItems;
        }

        internal ICollection InternalGetItems(Type type)
        {
            var mi = typeof(ItemCollection).GetMethod("GenericGetItems", BindingFlags.NonPublic | BindingFlags.Static);
            var genericMi = mi.MakeGenericMethod(type);

            return genericMi.Invoke(null, new object[] { this }) as ICollection;
        }

        private static ReadOnlyCollection<TItem> GenericGetItems<TItem>(ItemCollection collection) where TItem : GlobalItem
        {
            var list = new List<TItem>();
            foreach (var item in collection)
            {
                var stronglyTypedItem = item as TItem;
                if (stronglyTypedItem != null)
                {
                    list.Add(stronglyTypedItem);
                }
            }
            return list.AsReadOnly();
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name and the namespace name in this item collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object that represents the type that matches the specified type name and the namespace name in this item collection. If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        public EdmType GetType(string name, string namespaceName)
        {
            return GetType(name, namespaceName, false /*ignoreCase*/);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name and the namespace name from this item collection.
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="type">
        /// When this method returns, this output parameter contains an
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// object. If there is no type with the specified name and namespace name in this item collection, this output parameter contains null.
        /// </param>
        public bool TryGetType(string name, string namespaceName, out EdmType type)
        {
            return TryGetType(name, namespaceName, false /*ignoreCase*/, out type);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name and the namespace name from this item collection.
        /// </summary>
        /// <returns>
        /// An <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object that represents the type that matches the specified type name and the namespace name in this item collection. If there is no matched type, this method returns null.
        /// </returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        public EdmType GetType(string name, string namespaceName, bool ignoreCase)
        {
            Check.NotNull(name, "name");
            Check.NotNull(namespaceName, "namespaceName");
            return GetItem<EdmType>(EdmType.CreateEdmTypeIdentity(namespaceName, name), ignoreCase);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" /> object by using the specified type name and the namespace name from this item collection.
        /// </summary>
        /// <returns>true if there is a type that matches the search criteria; otherwise, false. </returns>
        /// <param name="name">The name of the type.</param>
        /// <param name="namespaceName">The namespace of the type.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="type">
        /// When this method returns, this output parameter contains an
        /// <see
        ///     cref="T:System.Data.Entity.Core.Metadata.Edm.EdmType" />
        /// object. If there is no type with the specified name and namespace name in this item collection, this output parameter contains null.
        /// </param>
        public bool TryGetType(string name, string namespaceName, bool ignoreCase, out EdmType type)
        {
            Check.NotNull(name, "name");
            Check.NotNull(namespaceName, "namespaceName");
            GlobalItem item = null;
            TryGetValue(EdmType.CreateEdmTypeIdentity(namespaceName, name), ignoreCase, out item);
            type = item as EdmType;
            return type != null;
        }

        /// <summary>Returns all the overloads of the functions by using the specified name from this item collection.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the functions that have the specified name.
        /// </returns>
        /// <param name="functionName">The full name of the function.</param>
        public ReadOnlyCollection<EdmFunction> GetFunctions(string functionName)
        {
            return GetFunctions(functionName, false /*ignoreCase*/);
        }

        /// <summary>Returns all the overloads of the functions by using the specified name from this item collection.</summary>
        /// <returns>
        /// A collection of type <see cref="T:System.Collections.ObjectModel.ReadOnlyCollection`1" /> that contains all the functions that have the specified name.
        /// </returns>
        /// <param name="functionName">The full name of the function.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        public ReadOnlyCollection<EdmFunction> GetFunctions(string functionName, bool ignoreCase)
        {
            return GetFunctions(FunctionLookUpTable, functionName, ignoreCase);
        }

        /// <summary>Returns all the overloads of the functions by using the specified name from this item collection.</summary>
        /// <returns>A collection of type ReadOnlyCollection that contains all the functions that have the specified name.</returns>
        /// <param name="functionCollection">A dictionary of functions.</param>
        /// <param name="functionName">The full name of the function.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        protected static ReadOnlyCollection<EdmFunction> GetFunctions(
            Dictionary<string, ReadOnlyCollection<EdmFunction>> functionCollection,
            string functionName, bool ignoreCase)
        {
            ReadOnlyCollection<EdmFunction> functionOverloads;

            if (functionCollection.TryGetValue(functionName, out functionOverloads))
            {
                if (ignoreCase)
                {
                    return functionOverloads;
                }

                return GetCaseSensitiveFunctions(functionOverloads, functionName);
            }

            return Helper.EmptyEdmFunctionReadOnlyCollection;
        }

        internal static ReadOnlyCollection<EdmFunction> GetCaseSensitiveFunctions(
            ReadOnlyCollection<EdmFunction> functionOverloads,
            string functionName)
        {
            // For case-sensitive match, first check if there are anything with a different case
            // its very rare to have functions with different case. So optimizing the case where all
            // functions are of same case
            // Else create a new list with the functions with the exact name
            var caseSensitiveFunctionOverloads = new List<EdmFunction>(functionOverloads.Count);

            for (var i = 0; i < functionOverloads.Count; i++)
            {
                if (functionOverloads[i].FullName == functionName)
                {
                    caseSensitiveFunctionOverloads.Add(functionOverloads[i]);
                }
            }

            // If there are no functions with different case, just return the collection
            if (caseSensitiveFunctionOverloads.Count
                != functionOverloads.Count)
            {
                functionOverloads = caseSensitiveFunctionOverloads.AsReadOnly();
            }
            return functionOverloads;
        }

        /// <summary>
        /// Gets the function as specified by the function key.
        /// All parameters are assumed to be <see cref="ParameterMode.In" />.
        /// </summary>
        /// <param name="functionName"> Name of the function </param>
        /// <param name="parameterTypes"> types of the parameters </param>
        /// <param name="ignoreCase"> true for case-insensitive lookup </param>
        /// <param name="function"> The function that needs to be returned </param>
        /// <returns> The function as specified in the function key or null </returns>
        /// <exception cref="System.ArgumentNullException">if functionName or parameterTypes argument is null</exception>
        /// <exception cref="System.ArgumentException">if no function is found with the given name or with given input parameters</exception>
        internal bool TryGetFunction(string functionName, TypeUsage[] parameterTypes, bool ignoreCase, out EdmFunction function)
        {
            Check.NotNull(functionName, "functionName");
            Check.NotNull(parameterTypes, "parameterTypes");
            var functionIdentity = EdmFunction.BuildIdentity(functionName, parameterTypes);
            GlobalItem item = null;
            function = null;
            if (TryGetValue(functionIdentity, ignoreCase, out item)
                && Helper.IsEdmFunction(item))
            {
                function = (EdmFunction)item;
                return true;
            }
            return false;
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name.
        /// </summary>
        /// <returns>If there is no entity container, this method returns null; otherwise, it returns the first one.</returns>
        /// <param name="name">The name of the entity container.</param>
        public EntityContainer GetEntityContainer(string name)
        {
            Check.NotNull(name, "name");
            return GetEntityContainer(name, false /*ignoreCase*/);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name. If there is no entity container, the output parameter contains null; otherwise, it contains the first entity container.
        /// </summary>
        /// <returns>true if there is an entity container that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="entityContainer">
        /// When this method returns, it contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object. If there is no entity container, this output parameter contains null; otherwise, it contains the first entity container.
        /// </param>
        public bool TryGetEntityContainer(string name, out EntityContainer entityContainer)
        {
            Check.NotNull(name, "name");
            return TryGetEntityContainer(name, false /*ignoreCase*/, out entityContainer);
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name.
        /// </summary>
        /// <returns>If there is no entity container, this method returns null; otherwise, it returns the first entity container.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        public EntityContainer GetEntityContainer(string name, bool ignoreCase)
        {
            var container = GetValue(name, ignoreCase) as EntityContainer;
            if (null != container)
            {
                return container;
            }
            throw new ArgumentException(Strings.ItemInvalidIdentity(name), "name");
        }

        /// <summary>
        /// Returns an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object by using the specified entity container name. If there is no entity container, this output parameter contains null; otherwise, it contains the first entity container.
        /// </summary>
        /// <returns>true if there is an entity container that matches the search criteria; otherwise, false.</returns>
        /// <param name="name">The name of the entity container.</param>
        /// <param name="ignoreCase">true to perform the case-insensitive search; otherwise, false.</param>
        /// <param name="entityContainer">
        /// When this method returns, it contains an <see cref="T:System.Data.Entity.Core.Metadata.Edm.EntityContainer" /> object. If there is no entity container, this output parameter contains null; otherwise, it contains the first entity container.
        /// </param>
        public bool TryGetEntityContainer(string name, bool ignoreCase, out EntityContainer entityContainer)
        {
            Check.NotNull(name, "name");
            GlobalItem item = null;
            if (TryGetValue(name, ignoreCase, out item)
                && Helper.IsEntityContainer(item))
            {
                entityContainer = (EntityContainer)item;
                return true;
            }
            entityContainer = null;
            return false;
        }

        /// <summary>
        /// Given the canonical primitive type, get the mapping primitive type in the given dataspace
        /// </summary>
        /// <param name="primitiveTypeKind"> canonical primitive type </param>
        /// <returns> The mapped scalar type </returns>
        internal virtual PrimitiveType GetMappedPrimitiveType(PrimitiveTypeKind primitiveTypeKind)
        {
            //The method needs to be overloaded on methods that support this
            throw Error.NotSupported();
        }

        /// <summary>
        /// Determines whether this item collection is equivalent to another. At present, we look only
        /// at object reference equivalence. This is a somewhat reasonable approximation when caching
        /// is enabled, because collections are identical when their source resources (including
        /// provider) are known to be identical.
        /// </summary>
        /// <param name="other"> Collection to compare. </param>
        /// <returns> true if the collections are equivalent; false otherwise </returns>
        internal virtual bool MetadataEquals(ItemCollection other)
        {
            return ReferenceEquals(this, other);
        }

        private static Dictionary<string, ReadOnlyCollection<EdmFunction>> PopulateFunctionLookUpTable(ItemCollection itemCollection)
        {
            var tempFunctionLookUpTable = new Dictionary<string, List<EdmFunction>>(StringComparer.OrdinalIgnoreCase);

            foreach (var function in itemCollection.GetItems<EdmFunction>())
            {
                List<EdmFunction> functionList;
                if (!tempFunctionLookUpTable.TryGetValue(function.FullName, out functionList))
                {
                    functionList = new List<EdmFunction>();
                    tempFunctionLookUpTable[function.FullName] = functionList;
                }
                functionList.Add(function);
            }

            var functionLookUpTable = new Dictionary<string, ReadOnlyCollection<EdmFunction>>(StringComparer.OrdinalIgnoreCase);
            foreach (var functionList in tempFunctionLookUpTable.Values)
            {
                functionLookUpTable.Add(functionList[0].FullName, new ReadOnlyCollection<EdmFunction>(functionList.ToArray()));
            }

            return functionLookUpTable;
        }
    }

//---- ItemCollection
}

//---- 
