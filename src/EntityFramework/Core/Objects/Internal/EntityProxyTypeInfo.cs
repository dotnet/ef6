// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;
    using System.Reflection.Emit;

    // <summary>
    // Contains the Type of a proxy class, along with any behaviors associated with that proxy Type.
    // </summary>
    internal sealed class EntityProxyTypeInfo
    {
        private readonly Type _proxyType;
        private readonly ClrEntityType _entityType; // The OSpace entity type that created this proxy info

        internal const string EntityWrapperFieldName = "_entityWrapper";
        private const string InitializeEntityCollectionsName = "InitializeEntityCollections";
        private readonly DynamicMethod _initializeCollections;

        private readonly Func<object, string, object> _baseGetter;
        private readonly HashSet<string> _propertiesWithBaseGetter;
        private readonly Action<object, string, object> _baseSetter;
        private readonly HashSet<string> _propertiesWithBaseSetter;
        private readonly Func<object, object> Proxy_GetEntityWrapper;
        private readonly Func<object, object, object> Proxy_SetEntityWrapper; // IEntityWrapper Func(object proxy, IEntityWrapper value)

        private readonly Func<object> _createObject;

        // An index of relationship metadata strings to an AssociationType
        // This is used when metadata is not otherwise available to the proxy
        private readonly Dictionary<string, AssociationType> _navigationPropertyAssociationTypes = new Dictionary<string, AssociationType>();

        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        internal EntityProxyTypeInfo(
            Type proxyType,
            ClrEntityType ospaceEntityType,
            DynamicMethod initializeCollections,
            List<PropertyInfo> baseGetters,
            List<PropertyInfo> baseSetters,
            MetadataWorkspace workspace)
        {
            DebugCheck.NotNull(proxyType);
            DebugCheck.NotNull(workspace);

            _proxyType = proxyType;
            _entityType = ospaceEntityType;

            _initializeCollections = initializeCollections;

            foreach (var relationshipType in GetAllRelationshipsForType(workspace, proxyType))
            {
                _navigationPropertyAssociationTypes.Add(relationshipType.FullName, relationshipType);

                if (relationshipType.Name != relationshipType.FullName)
                {
                    // Sometimes there isn't enough metadata to have a container name
                    // Default codegen doesn't qualify names
                    _navigationPropertyAssociationTypes.Add(relationshipType.Name, relationshipType);
                }
            }

            var entityWrapperField = proxyType.GetField(
                EntityWrapperFieldName, BindingFlags.DeclaredOnly | BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance);

            var Object_Parameter = Expression.Parameter(typeof(object), "proxy");
            var Value_Parameter = Expression.Parameter(typeof(object), "value");

            Debug.Assert(entityWrapperField != null, "entityWrapperField does not exist");

            // Create the Wrapper Getter
            var lambda = Expression.Lambda<Func<object, object>>(
                Expression.Field(
                    Expression.Convert(Object_Parameter, entityWrapperField.DeclaringType), entityWrapperField),
                Object_Parameter);
            var getEntityWrapperDelegate = lambda.Compile();
            Proxy_GetEntityWrapper = (object proxy) =>
                {
                    // This code validates that the wrapper points to the proxy that holds the wrapper.
                    // This guards against mischief by switching this wrapper out for another one obtained
                    // from a different object.
                    var wrapper = ((IEntityWrapper)getEntityWrapperDelegate(proxy));
                    if (wrapper != null
                        && !ReferenceEquals(wrapper.Entity, proxy))
                    {
                        throw new InvalidOperationException(Strings.EntityProxyTypeInfo_ProxyHasWrongWrapper);
                    }
                    return wrapper;
                };

            // Create the Wrapper setter
            Proxy_SetEntityWrapper = Expression.Lambda<Func<object, object, object>>(
                Expression.Assign(
                    Expression.Field(
                        Expression.Convert(Object_Parameter, entityWrapperField.DeclaringType),
                        entityWrapperField),
                    Value_Parameter),
                Object_Parameter, Value_Parameter).Compile();

            var PropertyName_Parameter = Expression.Parameter(typeof(string), "propertyName");
            var baseGetterMethod = proxyType.GetPublicInstanceMethod("GetBasePropertyValue", typeof(string));
            if (baseGetterMethod != null)
            {
                _baseGetter = Expression.Lambda<Func<object, string, object>>(
                    Expression.Call(Expression.Convert(Object_Parameter, proxyType), baseGetterMethod, PropertyName_Parameter),
                    Object_Parameter, PropertyName_Parameter).Compile();
            }

            var PropertyValue_Parameter = Expression.Parameter(typeof(object), "propertyName");
            var baseSetterMethod = proxyType.GetPublicInstanceMethod("SetBasePropertyValue", typeof(string), typeof(object));
            if (baseSetterMethod != null)
            {
                _baseSetter = Expression.Lambda<Action<object, string, object>>(
                    Expression.Call(
                        Expression.Convert(Object_Parameter, proxyType), baseSetterMethod, PropertyName_Parameter, PropertyValue_Parameter),
                    Object_Parameter, PropertyName_Parameter, PropertyValue_Parameter).Compile();
            }

            _propertiesWithBaseGetter = new HashSet<string>(baseGetters.Select(p => p.Name));
            _propertiesWithBaseSetter = new HashSet<string>(baseSetters.Select(p => p.Name));

            _createObject = DelegateFactory.CreateConstructor(proxyType);
        }

        internal static IEnumerable<AssociationType> GetAllRelationshipsForType(MetadataWorkspace workspace, Type clrType)
        {
            DebugCheck.NotNull(workspace);
            DebugCheck.NotNull(clrType);

            // Note that this gets any relationship that the CLR type participates in in any entity set. For MEST, this
            // could result in too many relationships being returned, but this doesn't matter since the extra ones will
            // not be used. Also, MEST is rare.
            return ((ObjectItemCollection)workspace.GetItemCollection(DataSpace.OSpace)).GetItems<AssociationType>().Where(
                a => IsEndMemberForType(a.AssociationEndMembers[0], clrType)
                     || IsEndMemberForType(a.AssociationEndMembers[1], clrType));
        }

        private static bool IsEndMemberForType(AssociationEndMember end, Type clrType)
        {
            var referenceType = end.TypeUsage.EdmType as RefType;
            return referenceType != null && referenceType.ElementType.ClrType.IsAssignableFrom(clrType);
        }

        internal object CreateProxyObject()
        {
            return _createObject();
        }

        internal Type ProxyType
        {
            get { return _proxyType; }
        }

        internal DynamicMethod InitializeEntityCollections
        {
            get { return _initializeCollections; }
        }

        public Func<object, string, object> BaseGetter
        {
            get { return _baseGetter; }
        }

        public bool ContainsBaseGetter(string propertyName)
        {
            return BaseGetter != null && _propertiesWithBaseGetter.Contains(propertyName);
        }

        public bool ContainsBaseSetter(string propertyName)
        {
            return BaseSetter != null && _propertiesWithBaseSetter.Contains(propertyName);
        }

        public Action<object, string, object> BaseSetter
        {
            get { return _baseSetter; }
        }

        public bool TryGetNavigationPropertyAssociationType(string relationshipName, out AssociationType associationType)
        {
            return _navigationPropertyAssociationTypes.TryGetValue(relationshipName, out associationType);
        }

        public IEnumerable<AssociationType> GetAllAssociationTypes()
        {
            return _navigationPropertyAssociationTypes.Values.Distinct();
        }

        public void ValidateType(ClrEntityType ospaceEntityType)
        {
            if (ospaceEntityType != _entityType
                && ospaceEntityType.HashedDescription != _entityType.HashedDescription)
            {
                Debug.Assert(ospaceEntityType.ClrType == _entityType.ClrType);
                throw new InvalidOperationException(Strings.EntityProxyTypeInfo_DuplicateOSpaceType(ospaceEntityType.ClrType.FullName));
            }
        }

        #region Wrapper on the Proxy

        // <summary>
        // Set the proxy object's private entity wrapper field value to the specified entity wrapper object.
        // The proxy object (representing the wrapped entity) is retrieved from the wrapper itself.
        // </summary>
        // <param name="wrapper"> Wrapper object to be referenced by the proxy. </param>
        // <returns> The supplied entity wrapper. This is done so that this method can be more easily composed within lambda expressions (such as in the materializer). </returns>
        internal IEntityWrapper SetEntityWrapper(IEntityWrapper wrapper)
        {
            DebugCheck.NotNull(wrapper);
            DebugCheck.NotNull(wrapper.Entity);
            return Proxy_SetEntityWrapper(wrapper.Entity, wrapper) as IEntityWrapper;
        }

        // <summary>
        // Gets the proxy object's entity wrapper field value
        // </summary>
        internal IEntityWrapper GetEntityWrapper(object entity)
        {
            return Proxy_GetEntityWrapper(entity) as IEntityWrapper;
        }

        internal Func<object, object> EntityWrapperDelegate
        {
            get { return Proxy_GetEntityWrapper; }
        }

        #endregion
    }
}
