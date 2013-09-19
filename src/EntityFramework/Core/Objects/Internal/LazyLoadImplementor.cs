// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class LazyLoadImplementor
    {
        private HashSet<EdmMember> _members;

        public LazyLoadImplementor(EntityType ospaceEntityType)
        {
            CheckType(ospaceEntityType);
        }

        public IEnumerable<EdmMember> Members
        {
            get { return _members; }
        }

        private void CheckType(EntityType ospaceEntityType)
        {
            _members = new HashSet<EdmMember>();

            foreach (var member in ospaceEntityType.Members)
            {
                var clrProperty = ospaceEntityType.ClrType.GetTopProperty(member.Name);
                if (clrProperty != null
                    &&
                    EntityProxyFactory.CanProxyGetter(clrProperty)
                    &&
                    LazyLoadBehavior.IsLazyLoadCandidate(ospaceEntityType, member))
                {
                    _members.Add(member);
                }
            }
        }

        public bool CanProxyMember(EdmMember member)
        {
            return _members.Contains(member);
        }

        public virtual void Implement(TypeBuilder typeBuilder, Action<FieldBuilder, bool> registerField)
        {
            // Add instance field to store IEntityWrapper instance
            // The field is typed as object, for two reasons:
            // 1. The practical one, IEntityWrapper is internal and not accessible from the dynamic assembly.
            // 2. We purposely want the wrapper field to be opaque on the proxy type.
            var wrapperField = typeBuilder.DefineField(EntityProxyTypeInfo.EntityWrapperFieldName, typeof(object), FieldAttributes.Public);
            registerField(wrapperField, false);
        }

        public bool EmitMember(
            TypeBuilder typeBuilder, EdmMember member, PropertyBuilder propertyBuilder, PropertyInfo baseProperty,
            BaseProxyImplementor baseImplementor)
        {
            if (_members.Contains(member))
            {
                var baseGetter = baseProperty.Getter();
                const MethodAttributes getterAttributes =
                    MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.Virtual;
                var getterAccess = baseGetter.Attributes & MethodAttributes.MemberAccessMask;

                // Define field to store interceptor Func
                // Signature of interceptor Func delegate is as follows:
                //
                //    bool intercept(ProxyType proxy, PropertyType propertyValue)
                //
                // where
                //     PropertyType is the type of the Property, such as ICollection<Customer>,
                //     ProxyType is the type of the proxy object,
                //     propertyValue is the value returned from the proxied type's property getter.

                var interceptorType = typeof(Func<,,>).MakeGenericType(typeBuilder, baseProperty.PropertyType, typeof(bool));
                var interceptorInvoke = TypeBuilder.GetMethod(interceptorType, typeof(Func<,,>).GetDeclaredMethod("Invoke"));
                var interceptorField = typeBuilder.DefineField(
                    GetInterceptorFieldName(baseProperty.Name), interceptorType, FieldAttributes.Private | FieldAttributes.Static);

                // Define a property getter override in the proxy type
                var getterBuilder = typeBuilder.DefineMethod(
                    "get_" + baseProperty.Name, getterAccess | getterAttributes, baseProperty.PropertyType, Type.EmptyTypes);
                var generator = getterBuilder.GetILGenerator();

                // Emit instructions for the following call:
                //   T value = base.SomeProperty;
                //   if(this._interceptorForSomeProperty(this, value))
                //   {  return value; }
                //   return base.SomeProperty;
                // where _interceptorForSomeProperty represents the interceptor Func field.

                var lableTrue = generator.DefineLabel();
                generator.DeclareLocal(baseProperty.PropertyType); // T value
                generator.Emit(OpCodes.Ldarg_0); // call base.SomeProperty
                generator.Emit(OpCodes.Call, baseGetter); // call to base property getter
                generator.Emit(OpCodes.Stloc_0); // value = result
                generator.Emit(OpCodes.Ldarg_0); // load this
                generator.Emit(OpCodes.Ldfld, interceptorField); // load this._interceptor
                generator.Emit(OpCodes.Ldarg_0); // load this
                generator.Emit(OpCodes.Ldloc_0); // load value
                generator.Emit(OpCodes.Callvirt, interceptorInvoke); // call to interceptor delegate with (this, value)
                generator.Emit(OpCodes.Brtrue_S, lableTrue); // if true, just return
                generator.Emit(OpCodes.Ldarg_0); // else, call the base propertty getter again
                generator.Emit(OpCodes.Call, baseGetter); // call to base property getter
                generator.Emit(OpCodes.Ret);
                generator.MarkLabel(lableTrue);
                generator.Emit(OpCodes.Ldloc_0);
                generator.Emit(OpCodes.Ret);

                propertyBuilder.SetGetMethod(getterBuilder);

                baseImplementor.AddBasePropertyGetter(baseProperty);
                return true;
            }
            return false;
        }

        internal static string GetInterceptorFieldName(string memberName)
        {
            return "ef_proxy_interceptorFor" + memberName;
        }
    }
}
