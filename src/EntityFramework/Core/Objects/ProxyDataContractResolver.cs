// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Entity.Utilities;
    using System.Runtime.Serialization;
    using System.Xml;

    /// <summary>
    /// A DataContractResolver that knows how to resolve proxy types created for persistent
    /// ignorant classes to their base types. This is used with the DataContractSerializer.
    /// </summary>
    public class ProxyDataContractResolver : DataContractResolver
    {
        private readonly XsdDataContractExporter _exporter = new XsdDataContractExporter();

        /// <summary>During deserialization, maps any xsi:type information to the actual type of the persistence-ignorant object. </summary>
        /// <returns>Returns the type that the xsi:type is mapped to. Returns null if no known type was found that matches the xsi:type.</returns>
        /// <param name="typeName">The xsi:type information to map.</param>
        /// <param name="typeNamespace">The namespace of the xsi:type.</param>
        /// <param name="declaredType">The declared type.</param>
        /// <param name="knownTypeResolver">
        /// An instance of <see cref="T:System.Data.Entity.Core.Objects.ProxyDataContractResolver" />.
        /// </param>
        public override Type ResolveName(string typeName, string typeNamespace, Type declaredType, DataContractResolver knownTypeResolver)
        {
            Check.NotEmpty(typeName, "typeName");
            Check.NotEmpty(typeNamespace, "typeNamespace");
            Check.NotNull(declaredType, "declaredType");
            Check.NotNull(knownTypeResolver, "knownTypeResolver");

            return knownTypeResolver.ResolveName(typeName, typeNamespace, declaredType, null);
        }

        /// <summary>During serialization, maps actual types to xsi:type information.</summary>
        /// <returns>true if the type was resolved; otherwise, false.  </returns>
        /// <param name="type">The actual type of the persistence-ignorant object.</param>
        /// <param name="declaredType">The declared type.</param>
        /// <param name="knownTypeResolver">
        /// An instance of <see cref="T:System.Data.Entity.Core.Objects.ProxyDataContractResolver" />.
        /// </param>
        /// <param name="typeName">When this method returns, contains a list of xsi:type declarations.</param>
        /// <param name="typeNamespace">When this method returns, contains a list of namespaces used.</param>
        public override bool TryResolveType(
            Type type, Type declaredType, DataContractResolver knownTypeResolver, out XmlDictionaryString typeName,
            out XmlDictionaryString typeNamespace)
        {
            Check.NotNull(type, "type");
            Check.NotNull(declaredType, "declaredType");
            Check.NotNull(knownTypeResolver, "knownTypeResolver");

            var nonProxyType = ObjectContext.GetObjectType(type);
            if (nonProxyType != type)
            {
                // Type was a proxy type, so map the name to the non-proxy name
                var qualifiedName = _exporter.GetSchemaTypeName(nonProxyType);
                var dictionary = new XmlDictionary(2);
                typeName = new XmlDictionaryString(dictionary, qualifiedName.Name, 0);
                typeNamespace = new XmlDictionaryString(dictionary, qualifiedName.Namespace, 1);
                return true;
            }
            else
            {
                // Type was not a proxy type, so do the default
                return knownTypeResolver.TryResolveType(type, declaredType, null, out typeName, out typeNamespace);
            }
        }
    }
}
