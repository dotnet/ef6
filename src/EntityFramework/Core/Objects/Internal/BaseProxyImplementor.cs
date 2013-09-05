// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Reflection;
    using System.Reflection.Emit;

    internal class BaseProxyImplementor
    {
        private readonly List<PropertyInfo> _baseGetters;
        private readonly List<PropertyInfo> _baseSetters;

        public BaseProxyImplementor()
        {
            _baseGetters = new List<PropertyInfo>();
            _baseSetters = new List<PropertyInfo>();
        }

        public List<PropertyInfo> BaseGetters
        {
            get { return _baseGetters; }
        }

        public List<PropertyInfo> BaseSetters
        {
            get { return _baseSetters; }
        }

        public void AddBasePropertyGetter(PropertyInfo baseProperty)
        {
            _baseGetters.Add(baseProperty);
        }

        public void AddBasePropertySetter(PropertyInfo baseProperty)
        {
            _baseSetters.Add(baseProperty);
        }

        public void Implement(TypeBuilder typeBuilder)
        {
            if (_baseGetters.Count > 0)
            {
                ImplementBaseGetter(typeBuilder);
            }
            if (_baseSetters.Count > 0)
            {
                ImplementBaseSetter(typeBuilder);
            }
        }

        internal static readonly MethodInfo StringEquals 
            = typeof(string).GetDeclaredMethod("op_Equality", new[] { typeof(string), typeof(string) });

        private static readonly ConstructorInfo _invalidOperationConstructor =
            typeof(InvalidOperationException).GetConstructor(Type.EmptyTypes);

        private void ImplementBaseGetter(TypeBuilder typeBuilder)
        {
            // Define a property getter in the proxy type
            var getterBuilder = typeBuilder.DefineMethod(
                "GetBasePropertyValue", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(object), new[] { typeof(string) });
            var gen = getterBuilder.GetILGenerator();
            var labels = new Label[_baseGetters.Count];

            for (var i = 0; i < _baseGetters.Count; i++)
            {
                labels[i] = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldstr, _baseGetters[i].Name);
                gen.Emit(OpCodes.Call, StringEquals);
                gen.Emit(OpCodes.Brfalse_S, labels[i]);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Call, _baseGetters[i].GetGetMethod(true));
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(labels[i]);
            }
            gen.Emit(OpCodes.Newobj, _invalidOperationConstructor);
            gen.Emit(OpCodes.Throw);
        }

        private void ImplementBaseSetter(TypeBuilder typeBuilder)
        {
            var setterBuilder = typeBuilder.DefineMethod(
                "SetBasePropertyValue", MethodAttributes.Public | MethodAttributes.HideBySig, typeof(void),
                new[] { typeof(string), typeof(object) });
            var gen = setterBuilder.GetILGenerator();

            var labels = new Label[_baseSetters.Count];

            for (var i = 0; i < _baseSetters.Count; i++)
            {
                labels[i] = gen.DefineLabel();
                gen.Emit(OpCodes.Ldarg_1);
                gen.Emit(OpCodes.Ldstr, _baseSetters[i].Name);
                gen.Emit(OpCodes.Call, StringEquals);
                gen.Emit(OpCodes.Brfalse_S, labels[i]);
                gen.Emit(OpCodes.Ldarg_0);
                gen.Emit(OpCodes.Ldarg_2);
                gen.Emit(OpCodes.Castclass, _baseSetters[i].PropertyType);
                gen.Emit(OpCodes.Call, _baseSetters[i].GetSetMethod(true));
                gen.Emit(OpCodes.Ret);
                gen.MarkLabel(labels[i]);
            }
            gen.Emit(OpCodes.Newobj, _invalidOperationConstructor);
            gen.Emit(OpCodes.Throw);
        }
    }
}
