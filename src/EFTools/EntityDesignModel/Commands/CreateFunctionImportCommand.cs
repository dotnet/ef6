// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Resources = Microsoft.Data.Entity.Design.Model.Resources;

    /// <summary>
    ///     Use this command to create a FunctionImport in the C-Side from a Function (stored procedure) in
    ///     the S-Side and a specified return type.
    ///     Example Function and corresponding FunctionImport:
    ///     &lt;Function Name=&quot;GetOrderDetails&quot; Aggregate=&quot;false&quot;
    ///     BuiltIn=&quot;false&quot; NiladicFunction=&quot;false&quot;
    ///     IsComposable=&quot;false&quot;
    ///     ParameterTypeSemantics=&quot;AllowImplicitConversion&quot;
    ///     Schema=&quot;dbo&quot;&gt;
    ///     &lt;Parameter Name=&quot;SalesOrderHeaderId&quot; Type=&quot;int&quot; Mode=&quot;in&quot; /&gt;
    ///     &lt;/Function&gt;
    ///     &lt;FunctionImport Name=&quot;GetOrderDetails&quot;
    ///     EntitySet=&quot;SalesOrderDetail&quot;
    ///     ReturnType=&quot;Collection(AdventureWorksModel.SalesOrderDetail)&quot;&gt;
    ///     &lt;Parameter Name=&quot;SalesOrderHeaderId&quot; Type=&quot;Int32&quot; Mode=&quot;in&quot;&gt;&lt;/Parameter&gt;
    ///     &lt;/FunctionImport&gt;
    /// </summary>
    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class CreateFunctionImportCommand : Command
    {
        private FunctionImport _fi;
        internal ConceptualEntityContainer Container { get; set; }
        internal Function Function { get; set; }
        internal string FunctionImportName { get; set; }
        internal IEnumerable<ParameterDefinition> ParameterDefinitions { get; set; }

        // _returnSingleType is NOT automatically a "Collection(<EntityType>)";
        // if it is not a primitive or <None> then it corresponds to a single entity type name.
        internal object ReturnSingleType { get; set; }

        /// <summary>
        ///     Creates a FunctionImport element that mimics the nodes under the Function element.
        /// </summary>
        /// <param name="function"></param>
        internal CreateFunctionImportCommand(ConceptualEntityContainer ec, Function function, string functionImportName, object returnType)
        {
            CommandValidation.ValidateConceptualEntityContainer(ec);
            CommandValidation.ValidateFunction(function);

            Container = ec;
            Function = function;
            FunctionImportName = functionImportName;
            ReturnSingleType = returnType;
        }

        internal CreateFunctionImportCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        /// <summary>
        ///     Create a Function Import whose return type being created in passed in CreateComplexTypeCommand.
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="function"></param>
        /// <param name="functionImportName"></param>
        /// <param name="prereqCommand"></param>
        internal CreateFunctionImportCommand(
            ConceptualEntityContainer ec, Function function, string functionImportName, CreateComplexTypeCommand prereqCommand)
        {
            ValidatePrereqCommand(prereqCommand);
            CommandValidation.ValidateConceptualEntityContainer(ec);
            CommandValidation.ValidateFunction(function);

            Container = ec;
            Function = function;
            FunctionImportName = functionImportName;
            AddPreReqCommand(prereqCommand);
        }

        /// <summary>
        ///     Get ComplexType value from CreateComplexTypeCommand
        /// </summary>
        protected override void ProcessPreReqCommands()
        {
            if (ReturnSingleType == null)
            {
                var prereq = GetPreReqCommand(CreateComplexTypeCommand.PrereqId) as CreateComplexTypeCommand;
                if (prereq != null)
                {
                    CommandValidation.ValidateComplexType(prereq.ComplexType);
                    ReturnSingleType = prereq.ComplexType;
                }

                Debug.Assert(ReturnSingleType != null, typeof(CreateComplexTypeCommand).Name + " command return null value of ComplexType");
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ReturnSingleType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "ParameterDefinitions")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            if (Container == null
                || (Function == null && ParameterDefinitions == null)
                || ReturnSingleType == null)
            {
                throw new InvalidOperationException(
                    "InvokeInternal is called when Container or Function or ParameterDefinitions or ReturnSingleType is null");
            }

            _fi = new FunctionImport(Container, null);
            _fi.LocalName.Value = FunctionImportName;

            // if we are using a high enough EDMX schema version then set IsComposable attribute
            if (EdmFeatureManager.GetComposableFunctionImportFeatureState(_fi.Artifact.SchemaVersion).IsEnabled())
            {
                // if Function.IsComposable is true set _fi.IsComposable to true also, but if it's false then
                // leave _fi.IsComposable unset which is equivalent to false
                if (Function.IsComposable.Value)
                {
                    _fi.IsComposable.Value = BoolOrNone.TrueValue;
                }
            }

            // if the return type is an EntityType, set the EntitySet attribute to
            // that EntityType's EntitySet. For other functions, don't set
            // the EntitySet.
            var returnSingleTypeString = ReturnSingleType as string;
            var returnSingleTypeComplexType = ReturnSingleType as ComplexType;
            var returnSingleTypeEntity = ReturnSingleType as EntityType;

            Debug.Assert(
                returnSingleTypeString != null || returnSingleTypeComplexType != null || returnSingleTypeEntity != null,
                "Return Type for function import must be of type string or ComplexType or EntityType");

            if (returnSingleTypeString != null)
            {
                // make sure that this is a primitive type or a complex type and build a "Collection()" around it.
                if (returnSingleTypeString != Tools.XmlDesignerBase.Resources.NoneDisplayValueUsedForUX)
                {
                    var edmPrimitiveTypes = ModelHelper.AllPrimitiveTypes(_fi.Artifact.SchemaVersion);
                    if (!edmPrimitiveTypes.Contains(returnSingleTypeString))
                    {
                        var msg = string.Format(CultureInfo.CurrentCulture, Resources.INVALID_FORMAT, returnSingleTypeString);
                        throw new CommandValidationFailedException(msg);
                    }

                    _fi.ReturnTypeAsPrimitiveType.Value = String.Format(
                        CultureInfo.InvariantCulture, FunctionImport.CollectionFormat, returnSingleTypeString);
                }
            }
            else if (returnSingleTypeComplexType != null)
            {
                var complexTypes = _fi.Artifact.ConceptualModel().ComplexTypes();
                if (!complexTypes.Contains(returnSingleTypeComplexType))
                {
                    var msg = string.Format(
                        CultureInfo.CurrentCulture, Resources.INVALID_FORMAT, returnSingleTypeComplexType.NormalizedNameExternal);
                    throw new CommandValidationFailedException(msg);
                }

                _fi.ReturnTypeAsComplexType.SetRefName(returnSingleTypeComplexType);
            }
            else if (returnSingleTypeEntity != null)
            {
                _fi.EntitySet.SetRefName(returnSingleTypeEntity.EntitySet);
                _fi.ReturnTypeAsEntityType.SetRefName(returnSingleTypeEntity);
            }

            Container.AddFunctionImport(_fi);
            XmlModelHelper.NormalizeAndResolve(_fi);

            if (ParameterDefinitions != null)
            {
                UpdateFunctionImportParameters(cpc, _fi, ParameterDefinitions);
            }
            if (Function != null)
            {
                UpdateFunctionImportParameters(cpc, _fi, Function);
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static void UpdateFunctionImportParameters(
            CommandProcessorContext cpc, FunctionImport fi, IEnumerable<ParameterDefinition> parameterDefinitions)
        {
            DeleteAllParameters(cpc, fi);

            if (parameterDefinitions != null)
            {
                foreach (var definition in parameterDefinitions)
                {
                    var conceptualFunctionParam = new Parameter(fi, null);
                    conceptualFunctionParam.LocalName.Value = definition.Name;
                    conceptualFunctionParam.Mode.Value = definition.Mode;
                    conceptualFunctionParam.Type.Value = definition.Type;

                    fi.AddParameter(conceptualFunctionParam);
                    XmlModelHelper.NormalizeAndResolve(conceptualFunctionParam);
                }
            }
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal static void UpdateFunctionImportParameters(CommandProcessorContext cpc, FunctionImport fi, Function function)
        {
            DeleteAllParameters(cpc, fi);

            if (function != null
                && function.Parameters().Count > 0)
            {
                var storeTypeNameToStoreTypeMap = function.EntityModel.StoreTypeNameToStoreTypeMap;
                Debug.Assert(
                    storeTypeNameToStoreTypeMap != null, "StoreTypeName to StoreType map should not be null. Not updating function import");
                if (storeTypeNameToStoreTypeMap != null)
                {
                    IDictionary<string, string> storeToEdmPrimitiveMap = storeTypeNameToStoreTypeMap.ToDictionary(
                        kvp => kvp.Key, kvp => kvp.Value.GetEdmPrimitiveType().Name);

                    // Parameter EFElement conveniently can be used in both the C-Side and S-Side.
                    foreach (var storageFunctionParam in function.Parameters())
                    {
                        var conceptualFunctionParam = new Parameter(fi, null);

                        conceptualFunctionParam.LocalName.Value = storageFunctionParam.LocalName.Value;
                        conceptualFunctionParam.Mode.Value = storageFunctionParam.Mode.Value;

                        string conceptualFunctionParamValue;
                        if (storeToEdmPrimitiveMap.TryGetValue(storageFunctionParam.Type.Value, out conceptualFunctionParamValue))
                        {
                            conceptualFunctionParam.Type.Value = conceptualFunctionParamValue;
                        }
                        else
                        {
                            conceptualFunctionParam.Type.Value = storageFunctionParam.Type.Value;
                        }

                        fi.AddParameter(conceptualFunctionParam);
                        XmlModelHelper.NormalizeAndResolve(conceptualFunctionParam);
                    }
                }
            }
        }

        private static void DeleteAllParameters(CommandProcessorContext cpc, FunctionImport fi)
        {
            IList<Parameter> parametersToDelete = new List<Parameter>();
            foreach (var parameter in fi.Parameters())
            {
                parametersToDelete.Add(parameter);
            }
            foreach (var parameter in parametersToDelete)
            {
                DeleteEFElementCommand.DeleteInTransaction(cpc, parameter);
            }
        }

        /// <summary>
        ///     Returns the FunctionScalarProperty created by this command
        /// </summary>
        internal FunctionImport FunctionImport
        {
            get { return _fi; }
        }
    }
}
