namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class IPOCOImplementor
    {
        private readonly EntityType _ospaceEntityType;

        private FieldBuilder _changeTrackerField;
        private FieldBuilder _relationshipManagerField;
        private FieldBuilder _resetFKSetterFlagField;
        private FieldBuilder _compareByteArraysField;

        private MethodBuilder _entityMemberChanging;
        private MethodBuilder _entityMemberChanged;
        private MethodBuilder _getRelationshipManager;

        private readonly List<KeyValuePair<NavigationProperty, PropertyInfo>> _referenceProperties;
        private readonly List<KeyValuePair<NavigationProperty, PropertyInfo>> _collectionProperties;
        private bool _implementIEntityWithChangeTracker;
        private bool _implementIEntityWithRelationships;
        private HashSet<EdmMember> _scalarMembers;
        private HashSet<EdmMember> _relationshipMembers;

        private static readonly MethodInfo s_EntityMemberChanging = typeof(IEntityChangeTracker).GetMethod(
            "EntityMemberChanging", new[] { typeof(string) });

        private static readonly MethodInfo s_EntityMemberChanged = typeof(IEntityChangeTracker).GetMethod(
            "EntityMemberChanged", new[] { typeof(string) });

        private static readonly MethodInfo s_CreateRelationshipManager = typeof(RelationshipManager).GetMethod(
            "Create", new[] { typeof(IEntityWithRelationships) });

        private static readonly MethodInfo s_GetRelationshipManager =
            typeof(IEntityWithRelationships).GetProperty("RelationshipManager").GetGetMethod();

        private static readonly MethodInfo s_GetRelatedReference = typeof(RelationshipManager).GetMethod(
            "GetRelatedReference", new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo s_GetRelatedCollection = typeof(RelationshipManager).GetMethod(
            "GetRelatedCollection", new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo s_GetRelatedEnd = typeof(RelationshipManager).GetMethod(
            "GetRelatedEnd", new[] { typeof(string), typeof(string) });

        private static readonly MethodInfo s_ObjectEquals = typeof(object).GetMethod("Equals", new[] { typeof(object), typeof(object) });

        private static readonly ConstructorInfo s_InvalidOperationConstructor =
            typeof(InvalidOperationException).GetConstructor(new[] { typeof(string) });

        private static readonly MethodInfo s_IEntityWrapper_GetEntity = typeof(IEntityWrapper).GetProperty("Entity").GetGetMethod();
        private static readonly MethodInfo s_Action_Invoke = typeof(Action<object>).GetMethod("Invoke", new[] { typeof(object) });

        private static readonly MethodInfo s_Func_object_object_bool_Invoke = typeof(Func<object, object, bool>).GetMethod(
            "Invoke", new[] { typeof(object), typeof(object) });

        public IPOCOImplementor(EntityType ospaceEntityType)
        {
            var baseType = ospaceEntityType.ClrType;
            _referenceProperties = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();
            _collectionProperties = new List<KeyValuePair<NavigationProperty, PropertyInfo>>();

            _implementIEntityWithChangeTracker = (null == baseType.GetInterface(typeof(IEntityWithChangeTracker).Name));
            _implementIEntityWithRelationships = (null == baseType.GetInterface(typeof(IEntityWithRelationships).Name));

            CheckType(ospaceEntityType);

            _ospaceEntityType = ospaceEntityType;
        }

        private void CheckType(EntityType ospaceEntityType)
        {
            _scalarMembers = new HashSet<EdmMember>();
            _relationshipMembers = new HashSet<EdmMember>();

            foreach (var member in ospaceEntityType.Members)
            {
                var clrProperty = EntityUtil.GetTopProperty(ospaceEntityType.ClrType, member.Name);
                if (clrProperty != null
                    && EntityProxyFactory.CanProxySetter(clrProperty))
                {
                    if (member.BuiltInTypeKind
                        == BuiltInTypeKind.EdmProperty)
                    {
                        if (_implementIEntityWithChangeTracker)
                        {
                            _scalarMembers.Add(member);
                        }
                    }
                    else if (member.BuiltInTypeKind
                             == BuiltInTypeKind.NavigationProperty)
                    {
                        if (_implementIEntityWithRelationships)
                        {
                            var navProperty = (NavigationProperty)member;
                            var multiplicity = navProperty.ToEndMember.RelationshipMultiplicity;

                            if (multiplicity == RelationshipMultiplicity.Many)
                            {
                                if (clrProperty.PropertyType.IsGenericType
                                    &&
                                    clrProperty.PropertyType.GetGenericTypeDefinition() == typeof(ICollection<>))
                                {
                                    _relationshipMembers.Add(member);
                                }
                            }
                            else
                            {
                                _relationshipMembers.Add(member);
                            }
                        }
                    }
                }
            }

            if (ospaceEntityType.Members.Count
                != _scalarMembers.Count + _relationshipMembers.Count)
            {
                _scalarMembers.Clear();
                _relationshipMembers.Clear();
                _implementIEntityWithChangeTracker = false;
                _implementIEntityWithRelationships = false;
            }
        }

        public void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            if (_implementIEntityWithChangeTracker)
            {
                ImplementIEntityWithChangeTracker(typeBuilder, registerField);
            }
            if (_implementIEntityWithRelationships)
            {
                ImplementIEntityWithRelationships(typeBuilder, registerField);
            }

            _resetFKSetterFlagField = typeBuilder.DefineField(
                EntityProxyFactory.ResetFKSetterFlagFieldName, typeof(Action<object>), FieldAttributes.Private | FieldAttributes.Static);
            _compareByteArraysField = typeBuilder.DefineField(
                EntityProxyFactory.CompareByteArraysFieldName, typeof(Func<object, object, bool>),
                FieldAttributes.Private | FieldAttributes.Static);
        }

        public Type[] Interfaces
        {
            get
            {
                var types = new List<Type>();
                if (_implementIEntityWithChangeTracker)
                {
                    types.Add(typeof(IEntityWithChangeTracker));
                }
                if (_implementIEntityWithRelationships)
                {
                    types.Add(typeof(IEntityWithRelationships));
                }
                return types.ToArray();
            }
        }

        public DynamicMethod CreateInitalizeCollectionMethod(Type proxyType)
        {
            if (_collectionProperties.Count > 0)
            {
                var initializeEntityCollections =
                    LightweightCodeGenerator.CreateDynamicMethod(
                        proxyType.Name + "_InitializeEntityCollections", typeof(IEntityWrapper), new[] { typeof(IEntityWrapper) });
                var generator = initializeEntityCollections.GetILGenerator();
                generator.DeclareLocal(proxyType);
                generator.DeclareLocal(typeof(RelationshipManager));
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Callvirt, s_IEntityWrapper_GetEntity);
                generator.Emit(OpCodes.Castclass, proxyType);
                generator.Emit(OpCodes.Stloc_0);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Callvirt, s_GetRelationshipManager);
                generator.Emit(OpCodes.Stloc_1);

                foreach (var navProperty in _collectionProperties)
                {
                    // Update Constructor to initialize this property
                    var getRelatedCollection =
                        s_GetRelatedCollection.MakeGenericMethod(EntityUtil.GetCollectionElementType(navProperty.Value.PropertyType));

                    generator.Emit(OpCodes.Ldloc_0);
                    generator.Emit(OpCodes.Ldloc_1);
                    generator.Emit(OpCodes.Ldstr, navProperty.Key.RelationshipType.FullName);
                    generator.Emit(OpCodes.Ldstr, navProperty.Key.ToEndMember.Name);
                    generator.Emit(OpCodes.Callvirt, getRelatedCollection);
                    generator.Emit(OpCodes.Callvirt, navProperty.Value.GetSetMethod(true));
                }
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ret);

                return initializeEntityCollections;
            }
            return null;
        }

        public bool CanProxyMember(EdmMember member)
        {
            return _scalarMembers.Contains(member) || _relationshipMembers.Contains(member);
        }

        public bool EmitMember(
            TypeBuilder typeBuilder, EdmMember member, PropertyBuilder propertyBuilder, PropertyInfo baseProperty,
            BaseProxyImplementor baseImplementor)
        {
            if (_scalarMembers.Contains(member))
            {
                var isKeyMember = _ospaceEntityType.KeyMembers.Contains(member.Identity);
                EmitScalarSetter(typeBuilder, propertyBuilder, baseProperty, isKeyMember);
                return true;
            }
            else if (_relationshipMembers.Contains(member))
            {
                Debug.Assert(member != null, "member is null");
                Debug.Assert(member.BuiltInTypeKind == BuiltInTypeKind.NavigationProperty);
                var navProperty = member as NavigationProperty;
                if (navProperty.ToEndMember.RelationshipMultiplicity
                    == RelationshipMultiplicity.Many)
                {
                    EmitCollectionProperty(typeBuilder, propertyBuilder, baseProperty, navProperty);
                }
                else
                {
                    EmitReferenceProperty(typeBuilder, propertyBuilder, baseProperty, navProperty);
                }
                baseImplementor.AddBasePropertySetter(baseProperty);
                return true;
            }
            return false;
        }

        private void EmitScalarSetter(TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, bool isKeyMember)
        {
            var baseSetter = baseProperty.GetSetMethod(true);
            const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            var methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;

            var setterBuilder = typeBuilder.DefineMethod(
                "set_" + baseProperty.Name, methodAccess | methodAttributes, null, new[] { baseProperty.PropertyType });
            var generator = setterBuilder.GetILGenerator();
            var endOfMethod = generator.DefineLabel();

            // If the CLR property represents a key member of the Entity Type,
            // ignore attempts to set the key value to the same value.
            if (isKeyMember)
            {
                var baseGetter = baseProperty.GetGetMethod(true);

                if (baseGetter != null)
                {
                    // if (base.[Property] != value)
                    // { 
                    //     // perform set operation
                    // }

                    var propertyType = baseProperty.PropertyType;

                    if (propertyType == typeof(int) || // signed integer types
                        propertyType == typeof(short) ||
                        propertyType == typeof(Int64) ||
                        propertyType == typeof(bool) || // boolean
                        propertyType == typeof(byte) ||
                        propertyType == typeof(UInt32) ||
                        propertyType == typeof(UInt64) ||
                        propertyType == typeof(float) ||
                        propertyType == typeof(double)
                        ||
                        propertyType.IsEnum)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Call, baseGetter);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Beq_S, endOfMethod);
                    }
                    else if (propertyType == typeof(byte[]))
                    {
                        // Byte arrays must be compared by value
                        generator.Emit(OpCodes.Ldsfld, _compareByteArraysField);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Call, baseGetter);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Callvirt, s_Func_object_object_bool_Invoke);
                        generator.Emit(OpCodes.Brtrue_S, endOfMethod);
                    }
                    else
                    {
                        // Get the specific type's inequality method if it exists
                        var op_inequality = propertyType.GetMethod("op_Inequality", new[] { propertyType, propertyType });
                        if (op_inequality != null)
                        {
                            generator.Emit(OpCodes.Ldarg_0);
                            generator.Emit(OpCodes.Call, baseGetter);
                            generator.Emit(OpCodes.Ldarg_1);
                            generator.Emit(OpCodes.Call, op_inequality);
                            generator.Emit(OpCodes.Brfalse_S, endOfMethod);
                        }
                        else
                        {
                            // Use object inequality
                            generator.Emit(OpCodes.Ldarg_0);
                            generator.Emit(OpCodes.Call, baseGetter);
                            if (propertyType.IsValueType)
                            {
                                generator.Emit(OpCodes.Box, propertyType);
                            }
                            generator.Emit(OpCodes.Ldarg_1);
                            if (propertyType.IsValueType)
                            {
                                generator.Emit(OpCodes.Box, propertyType);
                            }
                            generator.Emit(OpCodes.Call, s_ObjectEquals);
                            generator.Emit(OpCodes.Brtrue_S, endOfMethod);
                        }
                    }
                }
            }

            // Creates code like this:
            //
            // try
            // {
            //     MemberChanging(propertyName);
            //     base.Property_set(value);
            //     MemberChanged(propertyName);
            // }
            // finally
            // {
            //     _resetFKSetterFlagField(this);
            // }
            //
            // Note that the try/finally ensures that even if an exception causes
            // the setting of the property to be aborted, we still clear the flag that
            // indicates that we are in a property setter.

            generator.BeginExceptionBlock();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldstr, baseProperty.Name);
            generator.Emit(OpCodes.Call, _entityMemberChanging);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, baseSetter);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldstr, baseProperty.Name);
            generator.Emit(OpCodes.Call, _entityMemberChanged);
            generator.BeginFinallyBlock();
            generator.Emit(OpCodes.Ldsfld, _resetFKSetterFlagField);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, s_Action_Invoke);
            generator.EndExceptionBlock();
            generator.MarkLabel(endOfMethod);
            generator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);
        }

        private void EmitReferenceProperty(
            TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, NavigationProperty navProperty)
        {
            const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            var baseSetter = baseProperty.GetSetMethod(true);
            ;
            var methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;

            var specificGetRelatedReference = s_GetRelatedReference.MakeGenericMethod(baseProperty.PropertyType);
            var specificEntityReferenceSetValue = typeof(EntityReference<>).MakeGenericType(baseProperty.PropertyType).GetMethod(
                "set_Value");
            ;

            var setterBuilder = typeBuilder.DefineMethod(
                "set_" + baseProperty.Name, methodAccess | methodAttributes, null, new[] { baseProperty.PropertyType });
            var generator = setterBuilder.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Callvirt, _getRelationshipManager);
            generator.Emit(OpCodes.Ldstr, navProperty.RelationshipType.FullName);
            generator.Emit(OpCodes.Ldstr, navProperty.ToEndMember.Name);
            generator.Emit(OpCodes.Callvirt, specificGetRelatedReference);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, specificEntityReferenceSetValue);
            generator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);

            _referenceProperties.Add(new KeyValuePair<NavigationProperty, PropertyInfo>(navProperty, baseProperty));
        }

        private void EmitCollectionProperty(
            TypeBuilder typeBuilder, PropertyBuilder propertyBuilder, PropertyInfo baseProperty, NavigationProperty navProperty)
        {
            const MethodAttributes methodAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
            var baseSetter = baseProperty.GetSetMethod(true);
            ;
            var methodAccess = baseSetter.Attributes & MethodAttributes.MemberAccessMask;

            var cannotSetException = Strings.EntityProxyTypeInfo_CannotSetEntityCollectionProperty(propertyBuilder.Name, typeBuilder.Name);
            var setterBuilder = typeBuilder.DefineMethod(
                "set_" + baseProperty.Name, methodAccess | methodAttributes, null, new[] { baseProperty.PropertyType });
            var generator = setterBuilder.GetILGenerator();
            var instanceEqual = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, _getRelationshipManager);
            generator.Emit(OpCodes.Ldstr, navProperty.RelationshipType.FullName);
            generator.Emit(OpCodes.Ldstr, navProperty.ToEndMember.Name);
            generator.Emit(OpCodes.Callvirt, s_GetRelatedEnd);
            generator.Emit(OpCodes.Beq_S, instanceEqual);
            generator.Emit(OpCodes.Ldstr, cannotSetException);
            generator.Emit(OpCodes.Newobj, s_InvalidOperationConstructor);
            generator.Emit(OpCodes.Throw);
            generator.MarkLabel(instanceEqual);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Call, baseProperty.GetSetMethod(true));
            generator.Emit(OpCodes.Ret);
            propertyBuilder.SetSetMethod(setterBuilder);

            _collectionProperties.Add(new KeyValuePair<NavigationProperty, PropertyInfo>(navProperty, baseProperty));
        }

        #region Interface Implementation

        private void ImplementIEntityWithChangeTracker(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            _changeTrackerField = typeBuilder.DefineField("_changeTracker", typeof(IEntityChangeTracker), FieldAttributes.Private);
            registerField(_changeTrackerField, false);

            // Implement EntityMemberChanging(string propertyName)
            _entityMemberChanging = typeBuilder.DefineMethod(
                "EntityMemberChanging", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(void), new[] { typeof(string) });
            var generator = _entityMemberChanging.GetILGenerator();
            var methodEnd = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Brfalse_S, methodEnd);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, s_EntityMemberChanging);
            generator.MarkLabel(methodEnd);
            generator.Emit(OpCodes.Ret);

            // Implement EntityMemberChanged(string propertyName)
            _entityMemberChanged = typeBuilder.DefineMethod(
                "EntityMemberChanged", MethodAttributes.Private | MethodAttributes.HideBySig, typeof(void), new[] { typeof(string) });
            generator = _entityMemberChanged.GetILGenerator();
            methodEnd = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Brfalse_S, methodEnd);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _changeTrackerField);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Callvirt, s_EntityMemberChanged);
            generator.MarkLabel(methodEnd);
            generator.Emit(OpCodes.Ret);

            // Implement IEntityWithChangeTracker.SetChangeTracker(IEntityChangeTracker changeTracker)
            var setChangeTracker = typeBuilder.DefineMethod(
                "SetChangeTracker",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.Virtual
                | MethodAttributes.Final,
                typeof(void), new[] { typeof(IEntityChangeTracker) });
            generator = setChangeTracker.GetILGenerator();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_1);
            generator.Emit(OpCodes.Stfld, _changeTrackerField);
            generator.Emit(OpCodes.Ret);
        }

        private void ImplementIEntityWithRelationships(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            _relationshipManagerField = typeBuilder.DefineField(
                "_relationshipManager", typeof(RelationshipManager), FieldAttributes.Private);
            registerField(_relationshipManagerField, true);

            var relationshipManagerProperty = typeBuilder.DefineProperty(
                "RelationshipManager", PropertyAttributes.None, typeof(RelationshipManager), Type.EmptyTypes);

            // Implement IEntityWithRelationships.get_RelationshipManager
            _getRelationshipManager = typeBuilder.DefineMethod(
                "get_RelationshipManager",
                MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.NewSlot | MethodAttributes.SpecialName
                | MethodAttributes.Virtual | MethodAttributes.Final,
                typeof(RelationshipManager), Type.EmptyTypes);
            var generator = _getRelationshipManager.GetILGenerator();
            var trueLabel = generator.DefineLabel();
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _relationshipManagerField);
            generator.Emit(OpCodes.Brtrue_S, trueLabel);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Call, s_CreateRelationshipManager);
            generator.Emit(OpCodes.Stfld, _relationshipManagerField);
            generator.MarkLabel(trueLabel);
            generator.Emit(OpCodes.Ldarg_0);
            generator.Emit(OpCodes.Ldfld, _relationshipManagerField);
            generator.Emit(OpCodes.Ret);
            relationshipManagerProperty.SetGetMethod(_getRelationshipManager);
        }

        #endregion
    }
}