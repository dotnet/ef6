// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Linq;
    using System.Data.Entity.Core.Objects.DataClasses;
    using System.Diagnostics;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Xml.Linq;
    using System.Data.Entity.Core.Metadata.Edm;

    internal class CsdlToClrAssemblyConverter
    {
        private static readonly XNamespace _csdlNs = "http://schemas.microsoft.com/ado/2009/11/edm";
        private static readonly XNamespace _csdlMappingTestExtNs = "MappingTestExtension";

        private static readonly Dictionary<string, Type> _primitiveTypes = new Dictionary<string, Type>()
        {
            { "Binary", typeof(Byte[]) },
            { "Boolean", typeof(bool) },
            { "Byte", typeof(Byte) },
            { "DateTime", typeof(DateTime) },
            { "DateTimeOffset", typeof(DateTimeOffset) },
            { "Decimal", typeof(Decimal) },
            { "Double", typeof(Double) },
            { "Guid", typeof(Guid) },
            { "Int16", typeof(Int16) },
            { "Int32", typeof(Int32) },
            { "Int64", typeof(Int64) },
            { "SByte", typeof(SByte) },
            { "Single", typeof(Single) },
            { "String", typeof(String) },
            { "Time", typeof(TimeSpan) },

            // Not valid EDM primitive types but having them is useful for negative OC Mapping tests
            { "UInt32", typeof(UInt32) },
        };

        private static readonly IList<Assembly> _createdAssemblies = new List<Assembly>(); 

        static CsdlToClrAssemblyConverter()
        {
            // This enables resolving types used in the EdmRelationshipAttribute. 
            // Dynamic, in-memory assemblies are not considered for assembly load.
            AppDomain.CurrentDomain.AssemblyResolve += (o, e) =>
            {
                return e.Name == e.RequestingAssembly.FullName ? 
                    e.RequestingAssembly : 
                    _createdAssemblies.SingleOrDefault(a => a.FullName == e.Name);
            };                
        }

        private readonly XDocument[] _csdlArtifacts;
        private readonly bool _isPOCO;
        private DynamicAssembly _assembly;

        public CsdlToClrAssemblyConverter(bool isPOCO, params XDocument[] csdlArtifacts)
        {
            _csdlArtifacts = csdlArtifacts;
            _isPOCO = isPOCO;
        }

        public Assembly BuildAssembly(string assemblyName)
        {
            _assembly = new DynamicAssembly();

            AddSchemaAttribute();

            foreach (var csdl in _csdlArtifacts)
            {
                BuildTypes(csdl);
            }

            var assemblyBuilder = _assembly.Compile(new AssemblyName(assemblyName));

            // add the assembly to the list for assembly resolution
            _createdAssemblies.Add(assemblyBuilder);

            // EdmRelationshipAttributes require real Clr types so it adds these
            // attributes using CustomAttributeBuilder on already created assembly 
            foreach (var csdl in _csdlArtifacts)
            {
                AddRelationshipAttributes(csdl, (AssemblyBuilder)assemblyBuilder);
            }

            return assemblyBuilder;
        }

        private void BuildTypes(XDocument csdl)
        {
            foreach (var enumTypeDefinition in csdl.Descendants(_csdlNs + "EnumType"))
            {
                BuildEnumType(enumTypeDefinition);
            }

            BuildStructuralTypes(
                csdl.Descendants(_csdlNs + "ComplexType")
                .Concat(csdl.Descendants(_csdlNs + "EntityType")));
        }

        private void BuildEnumType(XElement enumTypeDefinition)
        {
            var dynamicEnumType = _assembly.DynamicEnumType(GetFullTypeName(enumTypeDefinition));
            dynamicEnumType.HasUnderlyingType(
                _primitiveTypes[(string)enumTypeDefinition.Attribute("UnderlyingType") ?? "Int32"]);
            
            if (!((bool?)enumTypeDefinition.Attribute(_csdlMappingTestExtNs + "SuppressEdmTypeAttribute") ?? false))
            {
                AddEdmTypeAttribute<EdmEnumTypeAttribute>(enumTypeDefinition, dynamicEnumType);
            }
            
            long memberValue = -1;
            foreach (var enumMember in enumTypeDefinition.Elements(_csdlNs + "Member"))
            {
                var definedMemberValue = (int?)enumMember.Attribute("Value");
                memberValue = definedMemberValue.HasValue ? definedMemberValue.Value : memberValue + 1;
                dynamicEnumType.HasMember(
                    (string)enumMember.Attribute("Name"),
                    Convert.ChangeType(memberValue, dynamicEnumType.UnderlyingType));
            }
        }

        private void BuildStructuralTypes(IEnumerable<XElement> structuralTypeDefinitions)
        {       
            foreach (var structuralTypeDefinition in structuralTypeDefinitions)
            {
                BuildStructuralType(structuralTypeDefinition);
            }

            // properties are built after all types have been registered to
            // make navigation properties work
            foreach (var structuralTypeDefinition in structuralTypeDefinitions)
            {
                BuildNavigationProperties(structuralTypeDefinition);
            }
        }

        private void BuildStructuralType(XElement structuralTypeDefinition)
        {
            var fullTypeName = GetFullTypeName(structuralTypeDefinition);

            if (_assembly.DynamicTypes.All(t => t.Name != fullTypeName))
            {
                var newType = _assembly.DynamicStructuralType(fullTypeName);

                var baseTypeName = (string)structuralTypeDefinition.Attribute("BaseType");

                if (baseTypeName != null)
                {
                    BuildStructuralType(
                        structuralTypeDefinition
                            .Parent
                            .Elements(_csdlNs + "EntityType")
                            .Single(
                                e => (string)e.Attribute("Name") == baseTypeName.Split('.').Last()));
                    
                    newType.HasBaseClass(_assembly.DynamicTypes.Single(t => t.Name == baseTypeName));
                }

                if (structuralTypeDefinition.Name.LocalName == "EntityType") 
                {
                    AddEdmTypeAttribute<EdmEntityTypeAttribute>(structuralTypeDefinition, newType);
                }
                else
                {
                    AddEdmTypeAttribute<EdmComplexTypeAttribute>(structuralTypeDefinition, newType);
                }

                foreach (var propertyDefinition in structuralTypeDefinition.Elements(_csdlNs + "Property"))
                {
                    var propertyName = (string)propertyDefinition.Attribute("Name");
                    var propertyTypeName = (string)propertyDefinition.Attribute("Type");
                    var propertyIsNullable = ((bool?)propertyDefinition.Attribute("Nullable") ?? false);
                    var property = newType.Property(propertyName);
                    
                    if ((bool?)propertyDefinition.Attribute(_csdlMappingTestExtNs + "SuppressGetter") ?? false)
                    {
                        property.HasGetterAccess(MemberAccess.None);
                    }

                    if ((bool?)propertyDefinition.Attribute(_csdlMappingTestExtNs + "SuppressSetter") ?? false)
                    {
                        property.HasSetterAccess(MemberAccess.None);
                    }

                    Type primitiveType;
                    if(_primitiveTypes.TryGetValue(propertyTypeName, out primitiveType))
                    {
                        if (primitiveType.IsValueType && propertyIsNullable)
                        {
                            primitiveType = typeof(Nullable<>).MakeGenericType(primitiveType);
                        }

                        property.HasType(primitiveType);
                        AddScalarPropertyAttribute(propertyDefinition, property);
                    }
                    else
                    {
                        var type = ResolveType(propertyTypeName);                      

                        var dynamicEnumType = type as DynamicEnumType;
                        if(dynamicEnumType != null)
                        {
                            property.HasEnumType(dynamicEnumType, propertyIsNullable);
                            AddScalarPropertyAttribute(propertyDefinition, property);
                        }
                        else
                        {
                            var dynamicStructuralType = type as DynamicStructuralType;
                            if (dynamicStructuralType != null)
                            {
                                property.HasReferenceType((DynamicStructuralType)dynamicStructuralType);
                                AddComplexPropertyAttribute(property);
                            }
                            else
                            {
                                Debug.Assert(type.GetType() is Type);

                                var clrType = (Type)type;
                                property.HasType(clrType);

                                if (clrType.IsEnum)
                                {
                                    AddScalarPropertyAttribute(propertyDefinition, property);
                                }
                                else
                                {
                                    AddComplexPropertyAttribute(property);
                                }
                            }
                        }
                    }
                }
            }
        }

        private void BuildNavigationProperties(XElement structuralTypeDefinition)
        {
            foreach (var navigationPropertyDefinition in structuralTypeDefinition.Elements(_csdlNs + "NavigationProperty"))
            {
                var property = _assembly
                    .DynamicTypes.OfType<DynamicStructuralType>()
                    .Single(t => t.Name == GetFullTypeName(structuralTypeDefinition))
                    .Property((string)navigationPropertyDefinition.Attribute("Name"));

                var correspondingAssociationType =
                    navigationPropertyDefinition
                    .Document
                    .Root
                    .Elements(_csdlNs + "Association")
                    .Single(at => GetFullTypeName(at) == (string)navigationPropertyDefinition.Attribute("Relationship"));

                var targetEnd = 
                    correspondingAssociationType
                    .Elements(_csdlNs + "End")
                    .Single(e => (string)e.Attribute("Role") == (string)navigationPropertyDefinition.Attribute("ToRole"));

                var propertyType = 
                    _assembly
                    .DynamicTypes.OfType<DynamicStructuralType>()
                    .Single(t => t.Name == (string)targetEnd.Attribute("Type"));

                if ((string)targetEnd.Attribute("Multiplicity") == "*")
                {
                    property.HasCollectionType(
                        _isPOCO ? typeof(ICollection<>) : typeof(EntityCollection<>),
                        propertyType);
                }
                else
                {
                    property.HasReferenceType(propertyType);
                }

                AddNavigationPropertyAttribute(navigationPropertyDefinition, property);
            }
        }

        private void AddRelationshipAttributes(XDocument csdl, AssemblyBuilder assemblyBuilder)
        {
            if (!_isPOCO)
            {
                var relationshipNamespaceName = GetModelNamespace(csdl.Root);

                foreach (var associationDefinition in csdl.Descendants(_csdlNs + "Association"))
                {
                    var relationshipName = (string)associationDefinition.Attribute("Name");
                    string end1Name, end2Name;
                    Type end1Type, end2Type;
                    RelationshipMultiplicity end1Multiplicity, end2Multiplicity;

                    GetEndDetails(
                        assemblyBuilder, associationDefinition.Elements(_csdlNs + "End").First(), out end1Name, out end1Multiplicity,
                        out end1Type);
                    GetEndDetails(
                        assemblyBuilder, associationDefinition.Elements(_csdlNs + "End").Last(), out end2Name, out end2Multiplicity,
                        out end2Type);

                    assemblyBuilder.SetCustomAttribute(
                        new CustomAttributeBuilder(
                            typeof(EdmRelationshipAttribute).GetConstructors().Single(ci => ci.GetParameters().Length == 8),
                            new object[]
                                {
                                    relationshipNamespaceName, relationshipName, end1Name, end1Multiplicity, end1Type, end2Name,
                                    end2Multiplicity, end2Type
                                }));
                }
            }
        }

        private void GetEndDetails(AssemblyBuilder assemblyBuilder, XElement end, out string endRole, out RelationshipMultiplicity endMultiplicity, out Type endType)
        {
            Debug.Assert(assemblyBuilder != null, "assemblyBuilder != null");
            Debug.Assert(end != null, "end != null");
            Debug.Assert(end.Name == _csdlNs + "End", "End element expected");

            endRole = (string)end.Attribute("Role");

            var multiplicity = (string)end.Attribute("Multiplicity");
            Debug.Assert(multiplicity == "*" || multiplicity == "0..1" || multiplicity == "1");
            endMultiplicity = multiplicity == "*" ? RelationshipMultiplicity.Many :
                              multiplicity == "1" ? RelationshipMultiplicity.One :
                              RelationshipMultiplicity.ZeroOrOne;

            endType = assemblyBuilder.GetType((string)end.Attribute("Type"));
        }

        private void AddSchemaAttribute()
        {
            if (!_isPOCO)
            {
                _assembly.Attributes.Add(new EdmSchemaAttribute());
            }
        }

        private void AddEdmTypeAttribute<T>(XElement typeDefinition, DynamicType dynamicType)
            where T: EdmTypeAttribute, new()
        {
            if (!_isPOCO)
            {
                string typeName, typeNamespace;
                GetModelTypeNameAndNamespace(typeDefinition, out typeName, out typeNamespace);
                dynamicType.Attributes.Add(
                    new T()
                    {
                        Name = typeName,
                        NamespaceName = typeNamespace
                    });
            }
        }

        private void AddScalarPropertyAttribute(XElement propertyDefinition, DynamicProperty property)
        {
            if (!_isPOCO)
            {
                property.HasAttribute(
                    new EdmScalarPropertyAttribute()
                    {
                        IsNullable = ((bool?)propertyDefinition.Attribute("Nullable") ?? false),
                        EntityKeyProperty =
                            propertyDefinition.Parent.Descendants(_csdlNs + "PropertyRef")
                                              .Any(
                                                  e =>
                                                  e.Parent.Name == _csdlNs + "Key"
                                                  && (string)e.Attribute("Name") == property.PropertyName)
                    });
            }
        }

        private void AddComplexPropertyAttribute(DynamicProperty property)
        {
            if (!_isPOCO)
            {
                property.HasAttribute(new EdmComplexPropertyAttribute());
            }
        }

        private void AddNavigationPropertyAttribute(XElement propertyDefinition, DynamicProperty property)
        {
            if (!_isPOCO)
            {
                string[] relationship = propertyDefinition.Attribute("Relationship").Value.Split('.');

                property.HasAttribute(
                    new EdmRelationshipNavigationPropertyAttribute(
                        relationship[0],
                        relationship[1],
                        (string)propertyDefinition.Attribute("ToRole")));
            }
        }


        private static string GetFullTypeName(XElement typeDefinition)
        {
            return string.Format(
                "{0}.{1}",
                (string)GetModelNamespace(typeDefinition),
                (string)typeDefinition.Attribute("Name"));
        }

        private static string GetModelNamespace(XElement csdlElement)
        {
            return csdlElement.Document.Root.Attribute("Namespace").Value;
        }

        private static void GetModelTypeNameAndNamespace(XElement typeElement, out string typeName, out string typeNamespace)
        {
            typeName =
                (string)typeElement.Attribute(_csdlMappingTestExtNs + "OSpaceTypeName") ??
                (string)typeElement.Attribute("Name");

            typeNamespace =
                (string)typeElement.Attribute(_csdlMappingTestExtNs + "OSpaceTypeNamespace") ??
                GetModelNamespace(typeElement);
        }

        private object ResolveType(string typeName)
        {
            object type;
            if((type = _assembly.DynamicTypes.SingleOrDefault(t => t.Name == typeName)) == null)
            {
                if ((type = _createdAssemblies.SelectMany(a => a.GetTypes()).SingleOrDefault(t => t.FullName == typeName)) == null)
                {
                    throw new InvalidOperationException(string.Format("Cannot resolve type {0}", typeName));
                }
            }
                    
            return type;
        }
    }
}