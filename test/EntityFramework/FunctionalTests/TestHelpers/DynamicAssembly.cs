// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

    public class DynamicAssembly
    {
        private readonly Dictionary<string, Tuple<TypeBuilder, ConstructorBuilder>> _typeBuilders =
            new Dictionary<string, Tuple<TypeBuilder, ConstructorBuilder>>();

        private readonly Dictionary<string, DynamicType> _dynamicTypes = new Dictionary<string, DynamicType>();
        private readonly Dictionary<string, Type> _types = new Dictionary<string, Type>();
        private readonly List<Attribute> _attributes = new List<Attribute>();

        private static int _assemblyCount = 1;

        public IEnumerable<DynamicType> DynamicTypes
        {
            get { return _dynamicTypes.Values; }
        }

        public IEnumerable<Type> Types
        {
            get { return _types.Values; }
        }

        public DynamicType DynamicType(string typeName)
        {
            DynamicType dynamicType;
            if (!_dynamicTypes.TryGetValue(typeName, out dynamicType))
            {
                dynamicType = new DynamicType();
                dynamicType.ClassName = typeName;
                _dynamicTypes.Add(typeName, dynamicType);
            }
            return dynamicType;
        }

        public DynamicAssembly HasAttribute(Attribute a)
        {
            _attributes.Add(a);
            return this;
        }

        public List<Attribute> Attributes
        {
            get { return _attributes; }
        }

        //private static MethodInfo s_RegisterSet = typeof(DbModelBuilder).GetMethod("RegisterSet", BindingFlags.Public | BindingFlags.Instance, null, Type.EmptyTypes, null);
        private static readonly MethodInfo s_Entity = typeof(DbModelBuilder).GetMethod(
            "Entity",
            BindingFlags.Public |
            BindingFlags.Instance, null,
            Type.EmptyTypes, null);

        public DbModelBuilder ToBuilder()
        {
            if (Types.Count() == 0)
            {
                Compile();
            }
            var builder = new DbModelBuilder();
            foreach (var t in Types)
            {
                var entityMethod = s_Entity.MakeGenericMethod(t);
                entityMethod.Invoke(builder, null);
            }
            return builder;
        }

        public Assembly Compile()
        {
            return Compile(new AssemblyName(string.Format("DynamicEntities{0}", _assemblyCount)));
        }

        public Assembly Compile(AssemblyName assemblyName)
        {
            var assemblyBuilder =
                AppDomain.CurrentDomain.DefineDynamicAssembly(assemblyName, AssemblyBuilderAccess.Run);

            foreach (var attribute in Attributes)
            {
                assemblyBuilder.SetCustomAttribute(AnnotationAttributeBuilder.Create(attribute));
            }

            var moduleName = string.Format("DynamicEntitiesModule{0}.dll", _assemblyCount);
            var module = assemblyBuilder.DefineDynamicModule(moduleName);
            _assemblyCount++;

            foreach (var typeInfo in DynamicTypes)
            {
                DefineType(module, typeInfo);
            }

            foreach (var typeInfo in DynamicTypes)
            {
                var typeBuilder = _typeBuilders[typeInfo.ClassName];

                foreach (var fieldInfo in typeInfo.Fields)
                {
                    DefineField(typeBuilder.Item1, typeBuilder.Item2, fieldInfo);
                }

                foreach (var propertyInfo in typeInfo.Properties)
                {
                    DefineProperty(typeBuilder.Item1, propertyInfo);
                }
            }

            foreach (var t in _typeBuilders)
            {
                _types.Add(t.Key, t.Value.Item1.CreateType());
            }

            return assemblyBuilder;
        }

        public Type GetType(string typeName)
        {
            return _types[typeName];
        }

        private void DefineType(ModuleBuilder module, DynamicType typeInfo)
        {
            var typeAttributes = TypeAttributes.Class;
            switch (typeInfo.ClassAccess)
            {
                case MemberAccess.Public:
                    typeAttributes |= TypeAttributes.Public;
                    break;
                case MemberAccess.Private:
                case MemberAccess.Internal:
                    typeAttributes |= TypeAttributes.NotPublic;
                    break;
            }
            if (typeInfo.IsAbstract)
            {
                typeAttributes |= TypeAttributes.Abstract;
            }
            if (typeInfo.IsSealed)
            {
                typeAttributes |= TypeAttributes.Sealed;
            }

            Type baseClass = null;
            if (typeInfo.BaseClass is Type)
            {
                baseClass = typeInfo.BaseClass as Type;
            }
            else if (typeInfo.BaseClass is DynamicType)
            {
                baseClass = _typeBuilders[((DynamicType)typeInfo.BaseClass).ClassName].Item1;
            }

            var typeBuilder = module.DefineType(typeInfo.ClassName, typeAttributes, baseClass);
            foreach (var a in typeInfo.Attributes)
            {
                typeBuilder.SetCustomAttribute(AnnotationAttributeBuilder.Create(a));
            }

            // Define the Ctor
            var constructorBuilder = typeInfo.CtorAccess == MemberAccess.None
                                         ? null
                                         : typeBuilder.DefineDefaultConstructor(GetMethodAttributes(false, typeInfo.CtorAccess));

            _typeBuilders.Add(typeInfo.ClassName, Tuple.Create(typeBuilder, constructorBuilder));
        }

        private void DefineField(TypeBuilder typeBuilder, ConstructorBuilder constructorBuilder, DynamicField fieldInfo)
        {
            var fieldAttributes = FieldAttributes.Public;
            if (fieldInfo.Static)
            {
                fieldAttributes |= FieldAttributes.Static;
            }

            var fieldType = _typeBuilders[fieldInfo.FieldType.ClassName].Item1;
            var fieldBuilder = typeBuilder.DefineField(fieldInfo.FieldName, fieldType, fieldAttributes);

            if (fieldInfo.SetInstancePattern)
            {
                var staticConstructorBuilder = typeBuilder.DefineConstructor(
                    MethodAttributes.Public | MethodAttributes.Static, CallingConventions.Standard, Type.EmptyTypes);
                var generator = staticConstructorBuilder.GetILGenerator();

                generator.Emit(OpCodes.Newobj, constructorBuilder);
                generator.Emit(OpCodes.Stsfld, fieldBuilder);
                generator.Emit(OpCodes.Ret);
            }
        }

        private void DefineProperty(TypeBuilder typeBuilder, DynamicProperty propertyInfo)
        {
            var getterAccess = propertyInfo.GetterAccess;
            var setterAccess = propertyInfo.SetterAccess;

            Type propertyType;
            if (propertyInfo.CollectionType != null)
            {
                if (propertyInfo.ReferenceType != null)
                {
                    propertyType =
                        propertyInfo.CollectionType.MakeGenericType(_typeBuilders[propertyInfo.ReferenceType.ClassName].Item1);
                }
                else
                {
                    propertyType = propertyInfo.CollectionType.MakeGenericType(propertyInfo.PropertyType);
                }
            }
            else if (propertyInfo.ReferenceType != null)
            {
                propertyType = _typeBuilders[propertyInfo.ReferenceType.ClassName].Item1;
                switch (_dynamicTypes[propertyInfo.ReferenceType.ClassName].ClassAccess)
                {
                    case MemberAccess.Private:
                        getterAccess = MemberAccess.Private;
                        setterAccess = MemberAccess.Private;
                        break;
                    case MemberAccess.Internal:
                        getterAccess = MemberAccess.Internal;
                        setterAccess = MemberAccess.Internal;
                        break;
                }
                if (propertyInfo.PropertyType != null
                    && propertyInfo.PropertyType.IsGenericTypeDefinition)
                {
                    propertyType = propertyInfo.PropertyType.MakeGenericType(propertyType);
                }
            }
            else
            {
                propertyType = propertyInfo.PropertyType;
            }

            var fieldBuilder = typeBuilder.DefineField(
                "_" + propertyInfo.PropertyName, propertyType,
                FieldAttributes.Private);

            var propertyBuilder = typeBuilder.DefineProperty(
                propertyInfo.PropertyName,
                System.Reflection.PropertyAttributes.None,
                propertyType, Type.EmptyTypes);

            foreach (var a in propertyInfo.Attributes)
            {
                propertyBuilder.SetCustomAttribute(AnnotationAttributeBuilder.Create(a));
            }

            if (propertyInfo.GetterAccess
                != MemberAccess.None)
            {
                var getter = typeBuilder.DefineMethod(
                    "get_" + propertyInfo.PropertyName,
                    GetMethodAttributes(propertyInfo.IsVirtual, getterAccess),
                    propertyType,
                    Type.EmptyTypes);
                var generator = getter.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldfld, fieldBuilder);
                generator.Emit(OpCodes.Ret);
                propertyBuilder.SetGetMethod(getter);
            }

            if (propertyInfo.SetterAccess
                != MemberAccess.None)
            {
                var setter = typeBuilder.DefineMethod(
                    "set_" + propertyInfo.PropertyName,
                    GetMethodAttributes(propertyInfo.IsVirtual, setterAccess),
                    null,
                    new[] { propertyType });
                var generator = setter.GetILGenerator();
                generator.Emit(OpCodes.Ldarg_0);
                generator.Emit(OpCodes.Ldarg_1);
                generator.Emit(OpCodes.Stfld, fieldBuilder);
                generator.Emit(OpCodes.Ret);
                propertyBuilder.SetSetMethod(setter);
            }
        }

        private MethodAttributes GetMethodAttributes(bool isVirtual, MemberAccess memberAccess)
        {
            var attributes = MethodAttributes.HideBySig | MethodAttributes.SpecialName;
            if (isVirtual)
            {
                attributes |= MethodAttributes.Virtual;
            }
            switch (memberAccess)
            {
                case MemberAccess.Public:
                    attributes |= MethodAttributes.Public | MethodAttributes.NewSlot;
                    break;
                case MemberAccess.Private:
                    attributes |= MethodAttributes.Private;
                    break;
                case MemberAccess.Internal:
                    attributes |= MethodAttributes.Assembly | MethodAttributes.NewSlot;
                    break;
                case MemberAccess.Protected:
                    attributes |= MethodAttributes.Family | MethodAttributes.NewSlot;
                    break;
                case MemberAccess.ProtectedInternal:
                    attributes |= MethodAttributes.FamORAssem | MethodAttributes.NewSlot;
                    break;
            }
            return attributes;
        }
    }

    public class AnnotationAttributeBuilder
    {
        public static CustomAttributeBuilder Create(dynamic attribute)
        {
            CustomAttributeBuilder builder = CreateCustom(attribute);
            if (builder == null)
            {
                object[] args = GetArgs(attribute);
                return
                    new CustomAttributeBuilder(
                        attribute.GetType().GetConstructor(args.Select(x => x.GetType()).ToArray()), args);
            }
            return builder;
        }

        public static CustomAttributeBuilder CreateCustom(dynamic attribute)
        {
            return null;
        }

        public static CustomAttributeBuilder CreateCustom(DataMemberAttribute attribute)
        {
            if (attribute.Order
                == -1)
            {
                return new CustomAttributeBuilder(
                    typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes),
                    new object[] { },
                    new PropertyInfo[] { },
                    new object[] { });
            }
            else
            {
                return new CustomAttributeBuilder(
                    typeof(DataMemberAttribute).GetConstructor(Type.EmptyTypes),
                    new object[] { },
                    new[]
                        { typeof(DataMemberAttribute).GetProperty("Order") },
                    new object[] { attribute.Order });
            }
        }

        private static object[] GetArgs(dynamic attribute)
        {
            return GetArgs(attribute);
        }

        private static object[] GetArgs(KeyAttribute attribute)
        {
            return new object[] { };
        }

        private static object[] GetArgs(RequiredAttribute attribute)
        {
            return new object[] { };
        }

        private static object[] GetArgs(TimestampAttribute attribute)
        {
            return new object[] { };
        }

        private static object[] GetArgs(ConcurrencyCheckAttribute attribute)
        {
            return new object[] { };
        }

        private static object[] GetArgs(StringLengthAttribute attribute)
        {
            return new object[] { attribute.MaximumLength };
        }

        private static object[] GetArgs(MaxLengthAttribute attribute)
        {
            return new object[] { attribute.Length };
        }

        private static object[] GetArgs(DatabaseGeneratedAttribute attribute)
        {
            return new object[] { attribute.DatabaseGeneratedOption };
        }

        private static object[] GetArgs(EdmEntityTypeAttribute attribute)
        {
            return new object[] { };
        }

        private static object[] GetArgs(EdmSchemaAttribute attribute)
        {
            return new object[] { };
        }
    }

    public enum MemberAccess
    {
        None,
        Public,
        Private,
        Internal,
        Protected,
        ProtectedInternal
    }

    public class DynamicProperty
    {
        private MemberAccess _getterAccess = MemberAccess.Public;
        private MemberAccess _setterAccess = MemberAccess.Public;
        private bool _isVirtual = true;
        private readonly List<Attribute> _attributes = new List<Attribute>();

        public Type PropertyType { get; set; }

        public DynamicProperty HasType(Type propertyType)
        {
            PropertyType = propertyType;
            return this;
        }

        public DynamicProperty HasType<T>()
        {
            PropertyType = typeof(T);
            return this;
        }

        public DynamicType ReferenceType { get; set; }

        public DynamicProperty HasReferenceType(DynamicType referenceType)
        {
            ReferenceType = referenceType;
            return this;
        }

        public Type CollectionType { get; set; }

        public DynamicProperty HasCollectionType(Type collectionType, DynamicType referenceType)
        {
            CollectionType = collectionType;
            ReferenceType = referenceType;
            return this;
        }

        public List<Attribute> Attributes
        {
            get { return _attributes; }
        }

        public DynamicProperty HasAttribute(Attribute a)
        {
            _attributes.Add(a);
            return this;
        }

        public string PropertyName { get; set; }

        public DynamicProperty HasName(string propertyName)
        {
            PropertyName = propertyName;
            return this;
        }

        public MemberAccess GetterAccess
        {
            get { return _getterAccess; }
            set { _getterAccess = value; }
        }

        public DynamicProperty HasGetterAccess(MemberAccess access)
        {
            GetterAccess = access;
            return this;
        }

        public MemberAccess SetterAccess
        {
            get { return _setterAccess; }
            set { _setterAccess = value; }
        }

        public DynamicProperty HasSetterAccess(MemberAccess access)
        {
            SetterAccess = access;
            return this;
        }

        public bool IsVirtual
        {
            get { return _isVirtual; }
            set { _isVirtual = value; }
        }

        public DynamicProperty HasVirtual(bool isVirtual)
        {
            IsVirtual = isVirtual;
            return this;
        }
    }

    public class DynamicField
    {
        public DynamicType FieldType { get; set; }
        public string FieldName { get; set; }
        public bool Static { get; set; }
        public bool SetInstancePattern { get; set; }

        public DynamicField HasType(DynamicType propertyType)
        {
            FieldType = propertyType;
            return this;
        }

        public DynamicField HasName(string propertyName)
        {
            FieldName = propertyName;
            return this;
        }

        public DynamicField IsStatic()
        {
            Static = true;
            return this;
        }

        /// <summary>
        ///     Sets the field up to match the Instance singleton pattern required by ADO.NET.
        /// </summary>
        public DynamicField IsInstance()
        {
            SetInstancePattern = true;
            return this;
        }
    }

    public class DynamicType
    {
        private MemberAccess _classAccess = MemberAccess.Public;
        private readonly Dictionary<string, DynamicProperty> _properties = new Dictionary<string, DynamicProperty>();
        private readonly Dictionary<string, DynamicField> _fields = new Dictionary<string, DynamicField>();
        private MemberAccess _ctorAccess = MemberAccess.None;
        private readonly List<Attribute> _attributes = new List<Attribute>();

        public string ClassName { get; set; }

        public DynamicType HasClassName(string className)
        {
            ClassName = className;
            return this;
        }

        public List<Attribute> Attributes
        {
            get { return _attributes; }
        }

        public DynamicType HasAttribute(Attribute a)
        {
            _attributes.Add(a);
            return this;
        }

        public bool IsSealed { get; set; }

        public DynamicType HasSealed(bool isSealed)
        {
            IsSealed = isSealed;
            return this;
        }

        public bool IsAbstract { get; set; }

        public DynamicType HasAbstract(bool isAbstract)
        {
            IsAbstract = isAbstract;
            return this;
        }

        public MemberAccess ClassAccess
        {
            get { return _classAccess; }
            set { _classAccess = value; }
        }

        public DynamicType HasClassAccess(MemberAccess access)
        {
            ClassAccess = access;
            return this;
        }

        public MemberAccess CtorAccess
        {
            get { return _ctorAccess; }
            set { _ctorAccess = value; }
        }

        public DynamicProperty Property(string propertyName)
        {
            DynamicProperty property;
            if (!_properties.TryGetValue(propertyName, out property))
            {
                property = new DynamicProperty();
                property.PropertyName = propertyName;
                _properties.Add(propertyName, property);
            }
            return property;
        }

        public DynamicField Field(string fieldName)
        {
            DynamicField field;
            if (!_fields.TryGetValue(fieldName, out field))
            {
                field = new DynamicField
                            {
                                FieldName = fieldName
                            };
                _fields.Add(fieldName, field);
            }
            return field;
        }

        public object BaseClass { get; set; }

        public DynamicType HasBaseClass(object baseClass)
        {
            BaseClass = baseClass;
            return this;
        }

        public IEnumerable<DynamicProperty> Properties
        {
            get { return _properties.Values; }
        }

        public IEnumerable<DynamicField> Fields
        {
            get { return _fields.Values; }
        }
    }
}
