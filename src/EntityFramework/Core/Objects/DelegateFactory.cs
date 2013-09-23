// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Linq.Expressions;
    using System.Reflection;

    // <summary>
    // CodeGenerator class: use expression trees to dynamically generate code to get/set properties.
    // </summary>
    internal static class DelegateFactory
    {
        private static readonly MethodInfo _throwSetInvalidValue = typeof(EntityUtil).GetDeclaredMethod(
            "ThrowSetInvalidValue", new[] { typeof(object), typeof(Type), typeof(string), typeof(string) });

        // <summary>
        // For an OSpace ComplexType returns the delegate to construct the clr instance.
        // </summary>
        internal static Func<object> GetConstructorDelegateForType(ClrComplexType clrType)
        {
            return (clrType.Constructor ?? (clrType.Constructor = CreateConstructor(clrType.ClrType)));
        }

        // <summary>
        // For an OSpace EntityType returns the delegate to construct the clr instance.
        // </summary>
        internal static Func<object> GetConstructorDelegateForType(ClrEntityType clrType)
        {
            return (clrType.Constructor ?? (clrType.Constructor = CreateConstructor(clrType.ClrType)));
        }

        // <summary>
        // for an OSpace property, get the property value from a clr instance
        // </summary>
        internal static object GetValue(EdmProperty property, object target)
        {
            var getter = GetGetterDelegateForProperty(property);
            Debug.Assert(null != getter, "null getter");

            return getter(target);
        }

        internal static Func<object, object> GetGetterDelegateForProperty(EdmProperty property)
        {
            return property.ValueGetter
                   ?? (property.ValueGetter = CreatePropertyGetter(property.EntityDeclaringType, property.PropertyInfo));
        }

        // <summary>
        // for an OSpace property, set the property value on a clr instance
        // </summary>
        // <exception cref="System.Data.ConstraintException">
        // If
        // <paramref name="value" />
        // is null for a non nullable property.
        // </exception>
        // <exception cref="System.InvalidOperationException">
        // Invalid cast of
        // <paramref name="value" />
        // to property type.
        // </exception>
        // <exception cref="System.ArgumentOutOfRangeException">From generated enties via StructuralObject.SetValidValue.</exception>
        internal static void SetValue(EdmProperty property, object target, object value)
        {
            var setter = GetSetterDelegateForProperty(property);
            setter(target, value);
        }

        // <summary>
        // For an OSpace property, gets the delegate to set the property value on a clr instance.
        // </summary>
        internal static Action<object, object> GetSetterDelegateForProperty(EdmProperty property)
        {
            var setter = property.ValueSetter;
            if (null == setter)
            {
                setter = CreatePropertySetter(
                    property.EntityDeclaringType, property.PropertyInfo,
                    property.Nullable);
                property.ValueSetter = setter;
            }
            Debug.Assert(null != setter, "null setter");
            return setter;
        }

        // <summary>
        // Gets the related end instance for the source AssociationEndMember by creating a DynamicMethod to
        // call GetRelatedCollection or GetRelatedReference
        // </summary>
        internal static RelatedEnd GetRelatedEnd(
            RelationshipManager sourceRelationshipManager, AssociationEndMember sourceMember, AssociationEndMember targetMember,
            RelatedEnd existingRelatedEnd)
        {
            var getRelatedEnd = sourceMember.GetRelatedEnd;
            if (null == getRelatedEnd)
            {
                getRelatedEnd = CreateGetRelatedEndMethod(sourceMember, targetMember);
                sourceMember.GetRelatedEnd = getRelatedEnd;
            }
            Debug.Assert(null != getRelatedEnd, "null getRelatedEnd");

            return getRelatedEnd(sourceRelationshipManager, existingRelatedEnd);
        }

        internal static Action<object, object> CreateNavigationPropertySetter(Type declaringType, PropertyInfo navigationProperty)
        {
            DebugCheck.NotNull(declaringType);
            DebugCheck.NotNull(navigationProperty);

            var propertyInfoForSet = navigationProperty.GetPropertyInfoForSet();
            var setMethod = propertyInfoForSet.Setter();

            if (setMethod == null)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyNoSetter);
            }

            if (setMethod.IsStatic)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyIsStatic);
            }

            if (setMethod.DeclaringType.IsValueType)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyDeclaringTypeIsValueType);
            }

            var entityParameter = Expression.Parameter(typeof(object), "entity");
            var targetParameter = Expression.Parameter(typeof(object), "target");

            return Expression.Lambda<Action<object, object>>(
                Expression.Assign(
                    Expression.Property(Expression.Convert(entityParameter, declaringType), propertyInfoForSet),
                    Expression.Convert(targetParameter, navigationProperty.PropertyType)), entityParameter, targetParameter).Compile();
        }

        // <summary>
        // Gets a parameterless constructor for the specified type.
        // </summary>
        // <param name="type"> Type to get constructor for. </param>
        // <returns> Parameterless constructor for the specified type. </returns>
        internal static ConstructorInfo GetConstructorForType(Type type)
        {
            DebugCheck.NotNull(type);
            var ci = type.GetConstructor(
                BindingFlags.NonPublic | BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null, Type.EmptyTypes,
                null);
            if (null == ci)
            {
                throw new InvalidOperationException(Strings.CodeGen_ConstructorNoParameterless(type.FullName));
            }
            return ci;
        }

        // <summary>
        // Gets a new expression that uses the parameterless constructor for the specified collection type.
        // For HashSet{T} will use ObjectReferenceEqualityComparer.
        // </summary>
        // <param name="type"> Type to get constructor for. </param>
        // <returns> Parameterless constructor for the specified type. </returns>
        internal static NewExpression GetNewExpressionForCollectionType(Type type)
        {
            if (type.IsGenericType && type.GetGenericTypeDefinition() == typeof(HashSet<>))
            {
                var constructor = type.GetConstructor(
                    BindingFlags.Public | BindingFlags.Instance | BindingFlags.CreateInstance, null,
                    new[] { typeof(IEqualityComparer<>).MakeGenericType(type.GetGenericArguments()) }, null);
                return Expression.New(constructor, Expression.New(typeof(ObjectReferenceEqualityComparer)));
            }
            return Expression.New(GetConstructorForType(type));
        }

        // <summary>
        // generate a delegate equivalent to
        // private object Constructor() { return new XClass(); }
        // </summary>
        internal static Func<object> CreateConstructor(Type type)
        {
            DebugCheck.NotNull(type);

            GetConstructorForType(type);

            return Expression.Lambda<Func<object>>(Expression.New(type)).Compile();
        }

        // <summary>
        // generate a delegate equivalent to
        // private object MemberGetter(object target) { return target.PropertyX; }
        // or if the property is Nullable&lt;&gt; generate a delegate equivalent to
        // private object MemberGetter(object target) { Nullable&lt;X&gt; y = target.PropertyX; return ((y.HasValue) ? y.Value : null); }
        // </summary>
        internal static Func<object, object> CreatePropertyGetter(Type entityDeclaringType, PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(entityDeclaringType);
            DebugCheck.NotNull(propertyInfo);

            var getter = propertyInfo.Getter();

            if (getter == null)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyNoGetter);
            }

            if (getter.IsStatic)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyIsStatic);
            }

            if (propertyInfo.DeclaringType.IsValueType)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyDeclaringTypeIsValueType);
            }

            if (propertyInfo.GetIndexParameters().Any())
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyIsIndexed);
            }

            var propertyType = propertyInfo.PropertyType;
            if (propertyType.IsPointer)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyUnsupportedType);
            }

            var entityParameter = Expression.Parameter(typeof(object), "entity");
            Expression getterExpression = Expression.Property(Expression.Convert(entityParameter, entityDeclaringType), propertyInfo);

            if (propertyType.IsValueType)
            {
                getterExpression = Expression.Convert(getterExpression, typeof(object));
            }

            return Expression.Lambda<Func<object, object>>(getterExpression, entityParameter).Compile();
        }

        // <summary>
        // generate a delegate equivalent to
        // // if Property is Nullable value type
        // private void MemberSetter(object target, object value) {
        // if (AllowNull &amp;&amp; (null == value)) {
        // ((TargetType)target).PropertyName = default(PropertyType?);
        // return;
        // }
        // if (value is PropertyType) {
        // ((TargetType)target).PropertyName = new (PropertyType?)((PropertyType)value);
        // return;
        // }
        // ThrowInvalidValue(value, TargetType.Name, PropertyName);
        // return
        // }
        // // when PropertyType is a value type
        // private void MemberSetter(object target, object value) {
        // if (value is PropertyType) {
        // ((TargetType)target).PropertyName = (PropertyType)value;
        // return;
        // }
        // ThrowInvalidValue(value, TargetType.Name, PropertyName);
        // return
        // }
        // // when PropertyType is a reference type
        // private void MemberSetter(object target, object value) {
        // if ((AllowNull &amp;&amp; (null == value)) || (value is PropertyType)) {
        // ((TargetType)target).PropertyName = ((PropertyType)value);
        // return;
        // }
        // ThrowInvalidValue(value, TargetType.Name, PropertyName);
        // return
        // }
        // </summary>
        // <exception cref="System.InvalidOperationException">
        // If the method is missing or static or has indexed parameters.
        // Or if the declaring type is a value type.
        // Or if the parameter type is a pointer.
        // </exception>
        internal static Action<object, object> CreatePropertySetter(Type entityDeclaringType, PropertyInfo propertyInfo, bool allowNull)
        {
            var propertyInfoForSet = ValidateSetterProperty(propertyInfo);

            var entityParameter = Expression.Parameter(typeof(object), "entity");
            var targetParameter = Expression.Parameter(typeof(object), "target");
            var propertyType = propertyInfo.PropertyType;

            // allowNull comes from a model facet and if it is not possible for the property to allow nulls
            // then we switch this off even if the model has it switched on.
            if (propertyType.IsValueType
                && Nullable.GetUnderlyingType(propertyType) == null)
            {
                allowNull = false;
            }

            // The value is checked to see if it is a compatible type (or optionally null) and if it
            // fails this check then a method on EntityUtil is called to throw the appropriate exception.
            Expression checkValidValue = Expression.TypeIs(targetParameter, propertyType);
            if (allowNull)
            {
                checkValidValue = Expression.Or(Expression.ReferenceEqual(targetParameter, Expression.Constant(null)), checkValidValue);
            }

            return Expression.Lambda<Action<object, object>>(
                Expression.IfThenElse(
                    checkValidValue,
                    Expression.Assign(
                        Expression.Property(Expression.Convert(entityParameter, entityDeclaringType), propertyInfoForSet),
                        Expression.Convert(targetParameter, propertyInfo.PropertyType)),
                    Expression.Call(
                        _throwSetInvalidValue,
                        targetParameter,
                        Expression.Constant(propertyType),
                        Expression.Constant(entityDeclaringType.Name),
                        Expression.Constant(propertyInfo.Name))), entityParameter, targetParameter).Compile();
        }

        internal static PropertyInfo ValidateSetterProperty(PropertyInfo propertyInfo)
        {
            DebugCheck.NotNull(propertyInfo);

            var propertyInfoForSet = propertyInfo.GetPropertyInfoForSet();

            var setterMethodInfo = propertyInfoForSet.Setter();

            if (setterMethodInfo == null)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyNoSetter);
            }

            if (setterMethodInfo.IsStatic)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyIsStatic);
            }

            if (propertyInfoForSet.DeclaringType.IsValueType)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyDeclaringTypeIsValueType);
            }

            if (propertyInfoForSet.GetIndexParameters().Any())
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyIsIndexed);
            }

            if (propertyInfoForSet.PropertyType.IsPointer)
            {
                throw new InvalidOperationException(Strings.CodeGen_PropertyUnsupportedType);
            }

            return propertyInfoForSet;
        }

        // <summary>
        // Create delegate used to invoke either the GetRelatedReference or GetRelatedCollection generic method on the RelationshipManager.
        // </summary>
        // <param name="sourceMember"> source end of the relationship for the requested navigation </param>
        // <param name="targetMember"> target end of the relationship for the requested navigation </param>
        // <returns> Delegate that can be used to invoke the corresponding method. </returns>
        private static Func<RelationshipManager, RelatedEnd, RelatedEnd> CreateGetRelatedEndMethod(
            AssociationEndMember sourceMember, AssociationEndMember targetMember)
        {
            Debug.Assert(
                sourceMember.DeclaringType == targetMember.DeclaringType, "Source and Target members must be in the same DeclaringType");

            var sourceEntityType = MetadataHelper.GetEntityTypeForEnd(sourceMember);
            var targetEntityType = MetadataHelper.GetEntityTypeForEnd(targetMember);
            var sourceAccessor = MetadataHelper.GetNavigationPropertyAccessor(targetEntityType, targetMember, sourceMember);
            var targetAccessor = MetadataHelper.GetNavigationPropertyAccessor(sourceEntityType, sourceMember, targetMember);

            var genericCreateRelatedEndMethod = typeof(DelegateFactory).GetDeclaredMethod(
                "CreateGetRelatedEndMethod", 
                new[]
                    {
                        typeof(AssociationEndMember), typeof(AssociationEndMember), typeof(NavigationPropertyAccessor),
                        typeof(NavigationPropertyAccessor)
                    });
            Debug.Assert(genericCreateRelatedEndMethod != null, "Could not find method DelegateFactory.CreateGetRelatedEndMethod");

            var createRelatedEndMethod = genericCreateRelatedEndMethod.MakeGenericMethod(sourceEntityType.ClrType, targetEntityType.ClrType);
            var getRelatedEndDelegate = createRelatedEndMethod.Invoke(
                null, new object[] { sourceMember, targetMember, sourceAccessor, targetAccessor });

            return (Func<RelationshipManager, RelatedEnd, RelatedEnd>)getRelatedEndDelegate;
        }

        private static Func<RelationshipManager, RelatedEnd, RelatedEnd> CreateGetRelatedEndMethod<TSource, TTarget>(
            AssociationEndMember sourceMember, AssociationEndMember targetMember, NavigationPropertyAccessor sourceAccessor,
            NavigationPropertyAccessor targetAccessor)
            where TSource : class
            where TTarget : class
        {
            Func<RelationshipManager, RelatedEnd, RelatedEnd> getRelatedEnd;

            // Get the appropriate method, either collection or reference depending on the target multiplicity
            switch (targetMember.RelationshipMultiplicity)
            {
                case RelationshipMultiplicity.ZeroOrOne:
                case RelationshipMultiplicity.One:
                    {
                        getRelatedEnd = (manager, relatedEnd) =>
                                        manager.GetRelatedReference<TSource, TTarget>(
                                            sourceMember.DeclaringType.FullName,
                                            sourceMember.Name,
                                            targetMember.Name,
                                            sourceAccessor,
                                            targetAccessor,
                                            sourceMember.RelationshipMultiplicity,
                                            relatedEnd);

                        break;
                    }
                case RelationshipMultiplicity.Many:
                    {
                        getRelatedEnd = (manager, relatedEnd) =>
                                        manager.GetRelatedCollection<TSource, TTarget>(
                                            sourceMember.DeclaringType.FullName,
                                            sourceMember.Name,
                                            targetMember.Name,
                                            sourceAccessor,
                                            targetAccessor,
                                            sourceMember.RelationshipMultiplicity,
                                            relatedEnd);

                        break;
                    }
                default:
                    var type = typeof(RelationshipMultiplicity);
                    throw new ArgumentOutOfRangeException(
                        type.Name,
                        Strings.ADP_InvalidEnumerationValue(
                            type.Name, ((int)targetMember.RelationshipMultiplicity).ToString(CultureInfo.InvariantCulture)));
            }

            return getRelatedEnd;
        }
    }
}
