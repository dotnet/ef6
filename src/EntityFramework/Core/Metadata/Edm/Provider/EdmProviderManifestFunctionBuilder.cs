// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal sealed class EdmProviderManifestFunctionBuilder
    {
        private readonly List<EdmFunction> functions = new List<EdmFunction>();
        private readonly TypeUsage[] primitiveTypes;

        internal EdmProviderManifestFunctionBuilder(ReadOnlyCollection<PrimitiveType> edmPrimitiveTypes)
        {
            Debug.Assert(edmPrimitiveTypes != null, "Primitive types should not be null");

            // Initialize all the various parameter types. We do not want to create new instance of parameter types
            // again and again for perf reasons
            var primitiveTypeUsages = new TypeUsage[edmPrimitiveTypes.Count];
            foreach (var edmType in edmPrimitiveTypes)
            {
                Debug.Assert(
                    (int)edmType.PrimitiveTypeKind < primitiveTypeUsages.Length && (int)edmType.PrimitiveTypeKind >= 0,
                    "Invalid PrimitiveTypeKind value?");
                Debug.Assert(
                    primitiveTypeUsages[(int)edmType.PrimitiveTypeKind] == null, "Duplicate PrimitiveTypeKind value in EDM primitive types?");

                primitiveTypeUsages[(int)edmType.PrimitiveTypeKind] = TypeUsage.Create(edmType);
            }

            primitiveTypes = primitiveTypeUsages;
        }

        internal ReadOnlyCollection<EdmFunction> ToFunctionCollection()
        {
            return functions.AsReadOnly();
        }

        internal static void ForAllBasePrimitiveTypes(Action<PrimitiveTypeKind> forEachType)
        {
            for (var idx = 0; idx < EdmConstants.NumPrimitiveTypes; idx++)
            {
                var typeKind = (PrimitiveTypeKind)idx;
                if (!Helper.IsStrongSpatialTypeKind(typeKind))
                {
                    forEachType(typeKind);
                }
            }
        }

        internal static void ForTypes(IEnumerable<PrimitiveTypeKind> typeKinds, Action<PrimitiveTypeKind> forEachType)
        {
            foreach (var kind in typeKinds)
            {
                forEachType(kind);
            }
        }

        internal void AddAggregate(string aggregateFunctionName, PrimitiveTypeKind collectionArgumentElementTypeKind)
        {
            AddAggregate(collectionArgumentElementTypeKind, aggregateFunctionName, collectionArgumentElementTypeKind);
        }

        internal void AddAggregate(
            PrimitiveTypeKind returnTypeKind, string aggregateFunctionName, PrimitiveTypeKind collectionArgumentElementTypeKind)
        {
            DebugCheck.NotEmpty(aggregateFunctionName);

            var returnParameter = CreateReturnParameter(returnTypeKind);
            var collectionParameter = CreateAggregateParameter(collectionArgumentElementTypeKind);

            var function = new EdmFunction(
                aggregateFunctionName,
                EdmConstants.EdmNamespace,
                DataSpace.CSpace,
                new EdmFunctionPayload
                    {
                        IsAggregate = true,
                        IsBuiltIn = true,
                        ReturnParameters = new[] { returnParameter },
                        Parameters = new FunctionParameter[1] { collectionParameter },
                        IsFromProviderManifest = true,
                    });

            function.SetReadOnly();

            functions.Add(function);
        }

        internal void AddFunction(PrimitiveTypeKind returnType, string functionName)
        {
            AddFunction(returnType, functionName, new KeyValuePair<string, PrimitiveTypeKind>[] { });
        }

        internal void AddFunction(
            PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argumentTypeKind, string argumentName)
        {
            AddFunction(returnType, functionName, new[] { new KeyValuePair<string, PrimitiveTypeKind>(argumentName, argumentTypeKind) });
        }

        internal void AddFunction(
            PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name,
            PrimitiveTypeKind argument2TypeKind, string argument2Name)
        {
            AddFunction(
                returnType, functionName,
                new[]
                    {
                        new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind)
                    });
        }

        internal void AddFunction(
            PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name,
            PrimitiveTypeKind argument2TypeKind, string argument2Name, PrimitiveTypeKind argument3TypeKind, string argument3Name)
        {
            AddFunction(
                returnType, functionName,
                new[]
                    {
                        new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind)
                    });
        }

        internal void AddFunction(
            PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name,
            PrimitiveTypeKind argument2TypeKind, string argument2Name,
            PrimitiveTypeKind argument3TypeKind, string argument3Name,
            PrimitiveTypeKind argument4TypeKind, string argument4Name,
            PrimitiveTypeKind argument5TypeKind, string argument5Name,
            PrimitiveTypeKind argument6TypeKind, string argument6Name)
        {
            AddFunction(
                returnType, functionName,
                new[]
                    {
                        new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument4Name, argument4TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument5Name, argument5TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument6Name, argument6TypeKind)
                    });
        }

        internal void AddFunction(
            PrimitiveTypeKind returnType, string functionName, PrimitiveTypeKind argument1TypeKind, string argument1Name,
            PrimitiveTypeKind argument2TypeKind, string argument2Name,
            PrimitiveTypeKind argument3TypeKind, string argument3Name,
            PrimitiveTypeKind argument4TypeKind, string argument4Name,
            PrimitiveTypeKind argument5TypeKind, string argument5Name,
            PrimitiveTypeKind argument6TypeKind, string argument6Name,
            PrimitiveTypeKind argument7TypeKind, string argument7Name)
        {
            AddFunction(
                returnType, functionName,
                new[]
                    {
                        new KeyValuePair<string, PrimitiveTypeKind>(argument1Name, argument1TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument2Name, argument2TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument3Name, argument3TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument4Name, argument4TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument5Name, argument5TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument6Name, argument6TypeKind),
                        new KeyValuePair<string, PrimitiveTypeKind>(argument7Name, argument7TypeKind)
                    });
        }

        private void AddFunction(
            PrimitiveTypeKind returnType, string functionName, KeyValuePair<string, PrimitiveTypeKind>[] parameterDefinitions)
        {
            var returnParameter = CreateReturnParameter(returnType);
            var parameters = parameterDefinitions.Select(paramDef => CreateParameter(paramDef.Value, paramDef.Key)).ToArray();

            var function = new EdmFunction(
                functionName,
                EdmConstants.EdmNamespace,
                DataSpace.CSpace,
                new EdmFunctionPayload
                    {
                        IsBuiltIn = true,
                        ReturnParameters = new[] { returnParameter },
                        Parameters = parameters,
                        IsFromProviderManifest = true,
                    });

            function.SetReadOnly();

            functions.Add(function);
        }

        private FunctionParameter CreateParameter(PrimitiveTypeKind primitiveParameterType, string parameterName)
        {
            return new FunctionParameter(parameterName, primitiveTypes[(int)primitiveParameterType], ParameterMode.In);
        }

        private FunctionParameter CreateAggregateParameter(PrimitiveTypeKind collectionParameterTypeElementTypeKind)
        {
            return new FunctionParameter(
                "collection", TypeUsage.Create(primitiveTypes[(int)collectionParameterTypeElementTypeKind].EdmType.GetCollectionType()),
                ParameterMode.In);
        }

        private FunctionParameter CreateReturnParameter(PrimitiveTypeKind primitiveReturnType)
        {
            return new FunctionParameter(EdmConstants.ReturnType, primitiveTypes[(int)primitiveReturnType], ParameterMode.ReturnValue);
        }
    }
}
