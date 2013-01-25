// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Linq;

    internal class FunctionParameterMappingGenerator : StructuralTypeMappingGenerator
    {
        public FunctionParameterMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public IEnumerable<StorageModificationFunctionParameterBinding> Generate(
            IEnumerable<EdmProperty> properties,
            IList<EdmProperty> propertyPath,
            bool useOriginalValues = false)
        {
            DebugCheck.NotNull(properties);
            DebugCheck.NotNull(propertyPath);

            foreach (var property in properties)
            {
                if (property.IsComplexType
                    && propertyPath.Any(
                        p => p.IsComplexType
                             && (p.ComplexType == property.ComplexType)))
                {
                    throw Error.CircularComplexTypeHierarchy();
                }

                propertyPath.Add(property);

                if (property.IsComplexType)
                {
                    foreach (var parameterBinding in Generate(property.ComplexType.Properties, propertyPath, useOriginalValues))
                    {
                        yield return parameterBinding;
                    }
                }
                else
                {
                    var parameterName
                        = string.Join("_", propertyPath.Select(p => p.Name));

                    var functionParameter = MapFunctionParameter(property, parameterName);

                    yield return
                        new StorageModificationFunctionParameterBinding(
                            functionParameter,
                            new StorageModificationFunctionMemberPath(propertyPath, null),
                            isCurrent: !useOriginalValues);
                }

                propertyPath.Remove(property);
            }
        }

        public IEnumerable<StorageModificationFunctionParameterBinding> Generate(
            IEnumerable<Tuple<StorageModificationFunctionMemberPath, string>> iaFkProperties,
            bool useOriginalValues = false)
        {
            DebugCheck.NotNull(iaFkProperties);

            return from iaFkProperty in iaFkProperties
                   let property = iaFkProperty.Item1.Members.First()
                   let functionParameter = MapFunctionParameter((EdmProperty)property, iaFkProperty.Item2)
                   select new StorageModificationFunctionParameterBinding(
                       functionParameter,
                       iaFkProperty.Item1,
                       isCurrent: !useOriginalValues);
        }

        private FunctionParameter MapFunctionParameter(EdmProperty property, string parameterName)
        {
            DebugCheck.NotNull(property);
            DebugCheck.NotEmpty(parameterName);

            var underlyingTypeUsage
                = TypeUsage.Create(property.UnderlyingPrimitiveType, property.TypeUsage.Facets);

            var storeTypeUsage = _providerManifest.GetStoreType(underlyingTypeUsage);

            var functionParameter
                = new FunctionParameter(parameterName, storeTypeUsage, ParameterMode.In);

            return functionParameter;
        }
    }
}
