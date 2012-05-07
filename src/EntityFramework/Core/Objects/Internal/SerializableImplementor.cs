namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;
    using System.Security;
    using System.Security.Permissions;

    /// <summary>
    /// This class determines if the proxied type implements ISerializable with the special serialization constructor.
    /// If it does, it adds the appropriate members to the proxy type.
    /// </summary>
    internal sealed class SerializableImplementor
    {
        private readonly Type _baseClrType;
        private readonly bool _baseImplementsISerializable;
        private readonly bool _canOverride;
        private readonly MethodInfo _getObjectDataMethod;
        private readonly ConstructorInfo _serializationConstructor;

        internal SerializableImplementor(EntityType ospaceEntityType)
        {
            _baseClrType = ospaceEntityType.ClrType;
            _baseImplementsISerializable = _baseClrType.IsSerializable && typeof(ISerializable).IsAssignableFrom(_baseClrType);

            if (_baseImplementsISerializable)
            {
                // Determine if interface implementation can be overridden.
                // Fortunately, there's only one method to check.
                var mapping = _baseClrType.GetInterfaceMap(typeof(ISerializable));
                _getObjectDataMethod = mapping.TargetMethods[0];

                // Members that implement interfaces must be public, unless they are explicitly implemented, in which case they are private and sealed (at least for C#).
                var canOverrideMethod = (_getObjectDataMethod.IsVirtual && !_getObjectDataMethod.IsFinal) && _getObjectDataMethod.IsPublic;

                if (canOverrideMethod)
                {
                    // Determine if proxied type provides the special serialization constructor.
                    // In order for the proxy class to properly support ISerializable, this constructor must not be private.
                    _serializationConstructor =
                        _baseClrType.GetConstructor(
                            BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic, null,
                            new[] { typeof(SerializationInfo), typeof(StreamingContext) }, null);

                    _canOverride = _serializationConstructor != null
                                   &&
                                   (_serializationConstructor.IsPublic || _serializationConstructor.IsFamily
                                    || _serializationConstructor.IsFamilyOrAssembly);
                }

                Debug.Assert(
                    !(_canOverride && (_getObjectDataMethod == null || _serializationConstructor == null)),
                    "Both GetObjectData method and Serialization Constructor must be present when proxy overrides ISerializable implementation.");
            }
        }

        internal bool TypeIsSuitable
        {
            get
            {
                // To be suitable,
                // either proxied type doesn't implement ISerializable,
                // or it does and it can be suitably overridden.
                return !_baseImplementsISerializable || _canOverride;
            }
        }

        internal bool TypeImplementsISerializable
        {
            get { return _baseImplementsISerializable; }
        }

        internal void Implement(TypeBuilder typeBuilder, IEnumerable<FieldBuilder> serializedFields)
        {
            if (_baseImplementsISerializable && _canOverride)
            {
                var serializationFormatterPermissions = new PermissionSet(null);
                serializationFormatterPermissions.AddPermission(new SecurityPermission(SecurityPermissionFlag.SerializationFormatter));

                var parameterTypes = new[] { typeof(SerializationInfo), typeof(StreamingContext) };
                var getTypeFromHandle = typeof(Type).GetMethod("GetTypeFromHandle", new[] { typeof(RuntimeTypeHandle) });
                var addValue = typeof(SerializationInfo).GetMethod("AddValue", new[] { typeof(string), typeof(object), typeof(Type) });
                var getValue = typeof(SerializationInfo).GetMethod("GetValue", new[] { typeof(string), typeof(Type) });

                //
                // Define GetObjectData method override
                //
                // [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
                // public void GetObjectData(SerializationInfo info, StreamingContext context)
                //
                var proxyGetObjectData = typeBuilder.DefineMethod(
                    _getObjectDataMethod.Name,
                    MethodAttributes.Public | MethodAttributes.HideBySig | MethodAttributes.Virtual,
                    null,
                    parameterTypes);
                proxyGetObjectData.AddDeclarativeSecurity(SecurityAction.Demand, serializationFormatterPermissions);

                {
                    var generator = proxyGetObjectData.GetILGenerator();

                    // Call SerializationInfo.AddValue to serialize each field value
                    foreach (var field in serializedFields)
                    {
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldstr, field.Name);
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldfld, field);
                        generator.Emit(OpCodes.Ldtoken, field.FieldType);
                        generator.Emit(OpCodes.Call, getTypeFromHandle);
                        generator.Emit(OpCodes.Callvirt, addValue);
                    }

                    // Emit call to base method
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, _getObjectDataMethod);
                    generator.Emit(OpCodes.Ret);
                }

                //
                // Define serialization constructor
                //
                // [SecurityPermission(SecurityAction.Demand, SerializationFormatter=true)]
                // .ctor(SerializationInfo info, StreamingContext context)
                //
                var constructorAttributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName | MethodAttributes.RTSpecialName;
                constructorAttributes |= _serializationConstructor.IsPublic ? MethodAttributes.Public : MethodAttributes.Private;

                var proxyConstructor = typeBuilder.DefineConstructor(
                    constructorAttributes, CallingConventions.Standard | CallingConventions.HasThis, parameterTypes);
                proxyConstructor.AddDeclarativeSecurity(SecurityAction.Demand, serializationFormatterPermissions);

                {
                    //Emit call to base serialization constructor
                    var generator = proxyConstructor.GetILGenerator();
                    generator.Emit(OpCodes.Ldarg_0);
                    generator.Emit(OpCodes.Ldarg_1);
                    generator.Emit(OpCodes.Ldarg_2);
                    generator.Emit(OpCodes.Call, _serializationConstructor);

                    // Call SerializationInfo.GetValue to retrieve the value of each field
                    foreach (var field in serializedFields)
                    {
                        generator.Emit(OpCodes.Ldarg_0);
                        generator.Emit(OpCodes.Ldarg_1);
                        generator.Emit(OpCodes.Ldstr, field.Name);
                        generator.Emit(OpCodes.Ldtoken, field.FieldType);
                        generator.Emit(OpCodes.Call, getTypeFromHandle);
                        generator.Emit(OpCodes.Callvirt, getValue);
                        generator.Emit(OpCodes.Castclass, field.FieldType);
                        generator.Emit(OpCodes.Stfld, field);
                    }

                    generator.Emit(OpCodes.Ret);
                }
            }
        }
    }
}