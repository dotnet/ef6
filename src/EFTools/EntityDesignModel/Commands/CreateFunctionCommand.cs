// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Data;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class CreateFunctionCommand : Command
    {
        internal string Name { get; private set; }
        internal string SchemaName { get; private set; }
        internal bool IsNiladic { get; private set; }
        internal bool IsComposable { get; private set; }
        internal bool IsAggregate { get; private set; }
        internal bool IsBuiltIn { get; private set; }
        internal string CommandText { get; private set; }
        internal string ReturnType { get; private set; }
        internal string StoreFunctionName { get; private set; }
        internal IList<ParameterDefinition> ParameterInfos { get; set; }
        internal IRawDataSchemaProcedure DataSchemaProcedure { get; private set; }
        internal Function CreatedFunction { get; private set; }

        internal CreateFunctionCommand(
            string name, string storeFunctionName, string schemaName, string commandText, IEnumerable<ParameterDefinition> parameterInfos,
            IRawDataSchemaProcedure dataSchemaProcedure,
            Func<Command, CommandProcessorContext, bool> bindingAction)
            : this(bindingAction)
        {
            ValidateParameterInfo(parameterInfos);

            Name = name;
            StoreFunctionName = storeFunctionName;
            SchemaName = schemaName;
            CommandText = commandText;
            ParameterInfos = parameterInfos.ToList();
            DataSchemaProcedure = dataSchemaProcedure;
        }

        internal CreateFunctionCommand(
            string name, string schemaName, bool isNiladic, bool isComposable,
            bool isAggregate, bool isBuiltIn, string commandText, string returnType, string storeFunctionName,
            IEnumerable<ParameterDefinition> parameterInfos, IRawDataSchemaProcedure dataSchemaProcedure,
            Func<Command, CommandProcessorContext, bool> bindingAction)
            : this(bindingAction)
        {
            if (String.IsNullOrWhiteSpace(returnType))
            {
                throw new ArgumentNullException("returnType");
            }
            ValidateParameterInfo(parameterInfos);

            Name = name;
            SchemaName = schemaName;
            IsNiladic = isNiladic;
            IsComposable = isComposable;
            IsAggregate = isAggregate;
            IsBuiltIn = isBuiltIn;
            CommandText = commandText;
            ReturnType = returnType;
            StoreFunctionName = storeFunctionName;
            ParameterInfos = parameterInfos.ToList();
            DataSchemaProcedure = dataSchemaProcedure;
        }

        internal CreateFunctionCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            var service = cpc.EditingContext.GetEFArtifactService();
            var artifact = service.Artifact;

            var function = new Function(artifact.StorageModel(), null);

            function.Name.Value = Name;
            function.Schema.Value = SchemaName;
            function.IsComposable.Value = IsComposable;
            function.ReturnType.Value = ReturnType;

            if (Name != StoreFunctionName)
            {
                function.StoreFunctionName.Value = StoreFunctionName;
            }

            foreach (var parameterInfo in ParameterInfos)
            {
                var parameter = new Parameter(function, null);
                parameter.Name.Value = parameterInfo.Name;
                parameter.Mode.Value = parameterInfo.Mode;
                parameter.Type.Value = parameterInfo.Type;
                function.AddParameter(parameter);
            }

            artifact.StorageModel().AddFunction(function);
            XmlModelHelper.NormalizeAndResolve(function);

            CreatedFunction = function;
        }

        internal static CreateFunctionCommand GetCreateFunctionCommandFromDataSchemaProcedure(
            IRawDataSchemaProcedure procedure, EFArtifact artifact, Func<Command, CommandProcessorContext, bool> bindingAction = null)
        {
            CreateFunctionCommand createFunctionCommand = null;
            if (procedure.IsFunction)
            {
                createFunctionCommand = new CreateFunctionCommand
                    (
                    name: ModelHelper.CreateValidSimpleIdentifier(procedure.Name),
                    schemaName: procedure.Schema,
                    isNiladic: !procedure.RawParameters.Any(),
                    isAggregate: false, // figure out if there's some way to determine this from IDataSchemaProcedure
                    isBuiltIn: false,
                    isComposable: true, // figure out if there's some way to determine this from IDataSchemaProcedure
                    commandText: null,
                    returnType: GetReturnType(procedure, artifact),
                    storeFunctionName: procedure.Name,
                    parameterInfos: GetParameterInfos(procedure, artifact),
                    dataSchemaProcedure: procedure,
                    bindingAction: bindingAction
                    );
            }
            else
            {
                createFunctionCommand = new CreateFunctionCommand
                    (
                    name: ModelHelper.CreateValidSimpleIdentifier(procedure.Name),
                    storeFunctionName: procedure.Name,
                    schemaName: procedure.Schema,
                    commandText: null,
                    parameterInfos: GetParameterInfos(procedure, artifact),
                    dataSchemaProcedure: procedure,
                    bindingAction: bindingAction
                    );
            }

            return createFunctionCommand;
        }

        private static IEnumerable<ParameterDefinition> GetParameterInfos(IRawDataSchemaProcedure procedure, EFArtifact artifact)
        {
            var parameterInfos = new List<ParameterDefinition>();

            foreach (var dataSchemaParam in procedure.RawParameters.Where(p => p != null && p.Direction != ParameterDirection.ReturnValue))
            {
                // -1 means that the type is unknown
                if (dataSchemaParam.ProviderDataType != -1)
                {
                    var info = new ParameterDefinition();
                    parameterInfos.Add(info);
                    info.Name = ModelHelper.CreateValidSimpleIdentifier(dataSchemaParam.Name);

                    switch (dataSchemaParam.Direction)
                    {
                        case ParameterDirection.Input:
                            info.Mode = Parameter.InOutMode.In.ToString();
                            break;
                        case ParameterDirection.InputOutput:
                            info.Mode = Parameter.InOutMode.InOut.ToString();
                            break;
                        case ParameterDirection.Output:
                            info.Mode = Parameter.InOutMode.Out.ToString();
                            break;
                        case ParameterDirection.ReturnValue:
                        default:
                            Debug.Fail("Could not determine parameter mode");
                            info.Mode = Parameter.InOutMode.Unknown.ToString();
                            break;
                    }
                    info.Type = GetParameterType(dataSchemaParam, artifact);
                }
            }
            return parameterInfos;
        }

        private static void ValidateParameterInfo(IEnumerable<ParameterDefinition> parameterInfos)
        {
            // if one of the parameter type is empty, throw.
            foreach (var paramDev in parameterInfos)
            {
                if (String.IsNullOrWhiteSpace(paramDev.Type))
                {
                    throw new ArgumentException(
                        String.Format(CultureInfo.CurrentCulture, Resources.BadFunctionParameterType, paramDev.Name));
                }
            }
        }

        private static string GetReturnType(IRawDataSchemaProcedure procedure, EFArtifact artifact)
        {
            if (procedure.RawReturnValue != null)
            {
                return GetParameterType(procedure.RawReturnValue, artifact);
            }
            return null;
        }

        private static string GetParameterType(IRawDataSchemaParameter parameter, EFArtifact artifact)
        {
            var type = ModelHelper.GetPrimitiveType(artifact.StorageModel(), parameter.NativeDataType, parameter.ProviderDataType);
            if (type != null)
            {
                return type.Name;
            }

            Debug.Fail(
                "Unable to find EDM primitive type for DB type:" + parameter.NativeDataType + ", provider data type:"
                + parameter.ProviderDataType);

            return null;
        }
    }
}
