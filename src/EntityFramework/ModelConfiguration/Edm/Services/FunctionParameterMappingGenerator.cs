// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Edm.Services
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Core.Mapping.Update.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;

    internal class FunctionParameterMappingGenerator : StructuralTypeMappingGenerator
    {
        public FunctionParameterMappingGenerator(DbProviderManifest providerManifest)
            : base(providerManifest)
        {
        }

        public IEnumerable<ModificationFunctionParameterBinding> Generate(
            ModificationOperator modificationOperator,
            IEnumerable<EdmProperty> properties,
            IList<ColumnMappingBuilder> columnMappings,
            IList<EdmProperty> propertyPath,
            bool useOriginalValues = false)
        {
            DebugCheck.NotNull(properties);
            DebugCheck.NotNull(columnMappings);
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
                    foreach (var parameterBinding
                        in Generate(modificationOperator, property.ComplexType.Properties, columnMappings, propertyPath, useOriginalValues))
                    {
                        yield return parameterBinding;
                    }
                }
                else
                {
                    if ((property.GetStoreGeneratedPattern() != StoreGeneratedPattern.Identity)
                        || (modificationOperator != ModificationOperator.Insert))
                    {
                        var columnProperty
                            = columnMappings.First(cm => cm.PropertyPath.SequenceEqual(propertyPath)).ColumnProperty;

                        if ((property.GetStoreGeneratedPattern() != StoreGeneratedPattern.Computed)
                            && ((modificationOperator != ModificationOperator.Delete) || property.IsKeyMember))
                        {
                            yield return
                                new ModificationFunctionParameterBinding(
                                    new FunctionParameter(columnProperty.Name, columnProperty.TypeUsage, ParameterMode.In),
                                    new ModificationFunctionMemberPath(propertyPath, null),
                                    isCurrent: !useOriginalValues);
                        }

                        if (modificationOperator != ModificationOperator.Insert
                            && property.ConcurrencyMode == ConcurrencyMode.Fixed)
                        {
                            yield return
                                new ModificationFunctionParameterBinding(
                                    new FunctionParameter(columnProperty.Name + "_Original", columnProperty.TypeUsage, ParameterMode.In),
                                    new ModificationFunctionMemberPath(propertyPath, null),
                                    isCurrent: false);
                        }
                    }
                }

                propertyPath.Remove(property);
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        public IEnumerable<ModificationFunctionParameterBinding> Generate(
            IEnumerable<Tuple<ModificationFunctionMemberPath, EdmProperty>> iaFkProperties,
            bool useOriginalValues = false)
        {
            DebugCheck.NotNull(iaFkProperties);

            return from iaFkProperty in iaFkProperties
                   let functionParameter
                       = new FunctionParameter(
                       iaFkProperty.Item2.Name,
                       iaFkProperty.Item2.TypeUsage,
                       ParameterMode.In)
                   select new ModificationFunctionParameterBinding(
                       functionParameter,
                       iaFkProperty.Item1,
                       isCurrent: !useOriginalValues);
        }
    }
}
