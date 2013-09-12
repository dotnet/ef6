// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Linq;
    using System.Reflection;
    using System.Reflection.Emit;
    using System.Runtime.Serialization;

    /// <summary>
    /// Add a DataContractAttribute to the proxy type, based on one that may have been applied to the base type.
    /// </summary>
    /// <remarks>
    /// <para> From http://msdn.microsoft.com/en-us/library/system.runtime.serialization.datacontractattribute.aspx: A data contract has two basic requirements: a stable name and a list of members. The stable name consists of the namespace uniform resource identifier (URI) and the local name of the contract. By default, when you apply the DataContractAttribute to a class, it uses the class name as the local name and the class's namespace (prefixed with "http://schemas.datacontract.org/2004/07/") as the namespace URI. You can override the defaults by setting the Name and Namespace properties. You can also change the namespace by applying the ContractNamespaceAttribute to the namespace. Use this capability when you have an existing type that processes data exactly as you require but has a different namespace and class name from the data contract. By overriding the default values, you can reuse your existing type and have the serialized data conform to the data contract. </para>
    /// <para> The first attempt at WCF serialization of proxies involved adding a DataContractAttribute to the proxy type in such a way so that the name and namespace of the proxy's data contract matched that of the base class. This worked when serializing proxy objects for the root type of the DataContractSerializer, but not for proxy objects of types derived from the root type. Attempting to add the proxy type to the list of known types failed as well, since the data contract of the proxy type did not match the base type as intended. This was due to the fact that inheritance is captured in the data contract. So while the proxy and base data contracts had the same members, the proxy data contract differed in that is declared itself as an extension of the base data contract. So the data contracts were technically not equivalent. The approach used instead is to allow proxy types to have their own DataContract. Users then have at least two options available to them. The first approach is to add the proxy types to the list of known types. The second approach is to implement an IDataContractSurrogate that can map a proxy instance to a surrogate that does have a data contract equivalent to the base type (you could use the base type itself for this purpose). While more complex to implement, it allows services to hide the use of proxies from clients. This can be quite useful in order to maximize potential interoperability. </para>
    /// </remarks>
    internal sealed class DataContractImplementor
    {
        internal static readonly ConstructorInfo DataContractAttributeConstructor =
            typeof(DataContractAttribute).GetConstructor(Type.EmptyTypes);

        internal static readonly PropertyInfo[] DataContractProperties = new[]
            {
                typeof(DataContractAttribute).GetDeclaredProperty("IsReference")
            };

        private readonly Type _baseClrType;
        private readonly DataContractAttribute _dataContract;

        internal DataContractImplementor(EntityType ospaceEntityType)
        {
            _baseClrType = ospaceEntityType.ClrType;
            _dataContract = _baseClrType.GetCustomAttributes<DataContractAttribute>(inherit: false).FirstOrDefault();
        }

        internal void Implement(TypeBuilder typeBuilder)
        {
            if (_dataContract != null)
            {
                // Use base data contract properties to help determine values of properties the proxy type's data contract.
                var propertyValues = new object[]
                    {
                        // IsReference
                        _dataContract.IsReference
                    };

                var attributeBuilder = new CustomAttributeBuilder(
                    DataContractAttributeConstructor, new object[0], DataContractProperties, propertyValues);
                typeBuilder.SetCustomAttribute(attributeBuilder);
            }
        }
    }
}
