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
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to change aspects of a FunctionImport in the C-Side
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
    internal class ChangeFunctionImportCommand : Command
    {
        internal FunctionImport FunctionImport { get; set; }
        internal Function Function { get; set; }
        internal Function FunctionOld { get; set; }
        internal ConceptualEntityContainer EntityContainer { get; set; }
        internal string FunctionImportName { get; set; }
        internal string FunctionImportNameOld { get; private set; }
        internal BoolOrNone FunctionImportIsComposable { get; set; }
        internal bool ChangeReturnType { get; set; }
        internal object ReturnSingleType { get; set; }
        internal object ReturnSingleTypeOld { get; set; }
        private readonly ICollection<EFContainer> _efContainerToBeNormalized;

        internal ChangeFunctionImportCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
            _efContainerToBeNormalized = new List<EFContainer>();
        }

        /// <summary>
        ///     Change various aspects of the passed in Function Import. The passed in function import will be modified based on the
        ///     other passed-in parameters (null is not ignored).
        /// </summary>
        /// <param name="fi"></param>
        /// <param name="function"></param>
        /// <param name="functionImportName"></param>
        /// <param name="returnType">Object of type EntityType or string representing the primitive type/'None'</param>
        internal ChangeFunctionImportCommand(
            ConceptualEntityContainer ec, FunctionImport fi, Function function, string functionImportName,
            BoolOrNone functionImportIsComposable, bool changeReturnType, object returnType)
        {
            CommandValidation.ValidateFunctionImport(fi);
            ValidateString(functionImportName);

            FunctionImport = fi;
            Function = function;
            EntityContainer = ec;
            FunctionImportName = functionImportName;
            FunctionImportIsComposable = functionImportIsComposable;
            ChangeReturnType = changeReturnType;
            ReturnSingleType = returnType;
            _efContainerToBeNormalized = new List<EFContainer>();
        }

        /// <summary>
        ///     Change various aspects of the passed in Function Import. The passed in function import will be modified based on the
        ///     other passed-in parameters (null is not ignored).
        /// </summary>
        /// <param name="ec"></param>
        /// <param name="fi"></param>
        /// <param name="function"></param>
        /// <param name="functionImportName"></param>
        /// <param name="prereqCommand"></param>
        internal ChangeFunctionImportCommand(
            ConceptualEntityContainer ec, FunctionImport fi, Function function, string functionImportName,
            BoolOrNone functionImportIsComposable, CreateComplexTypeCommand prereqCommand)
        {
            ValidatePrereqCommand(prereqCommand);
            CommandValidation.ValidateFunctionImport(fi);
            ValidateString(functionImportName);

            FunctionImport = fi;
            Function = function;
            EntityContainer = ec;
            FunctionImportName = functionImportName;
            FunctionImportIsComposable = functionImportIsComposable;
            ChangeReturnType = true;
            _efContainerToBeNormalized = new List<EFContainer>();
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
                    Debug.Assert(ReturnSingleType != null, "CreateComplexTypeCommand command return null value of ComplexType");
                }
            }
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "FunctionImport")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(FunctionImport != null, "InvokeInternal is called when FunctionImport is null");
            // safety check, this should never be hit
            if (FunctionImport == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when FunctionImport is null");
            }

            FunctionImportNameOld = FunctionImport.Name.Value;
            FunctionOld = FunctionImport.Function;
            if (FunctionImport.IsReturnTypeComplexType)
            {
                ReturnSingleTypeOld = FunctionImport.ReturnTypeAsComplexType;
            }
            else if (FunctionImport.IsReturnTypeEntityType)
            {
                ReturnSingleTypeOld = FunctionImport.ReturnTypeAsEntityType;
            }
            else
            {
                ReturnSingleTypeOld = FunctionImport.ReturnTypeAsPrimitiveType.Value;
            }

            UpdateFunctionImportName();

            // only change IsComposable if artifact has high enough schema version to support it
            if (EdmFeatureManager.GetComposableFunctionImportFeatureState(FunctionImport.Artifact.SchemaVersion)
                    .IsEnabled())
            {
                UpdateFunctionImportIsComposable(cpc);
            }

            UpdateFunctionImportFunction(cpc);
            UpdateFunctionImportReturnType(cpc);
        }

        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            // Normalize EFContainer if necessary
            foreach (var item in _efContainerToBeNormalized)
            {
                XmlModelHelper.NormalizeAndResolve(item);
            }
            base.PostInvoke(cpc);
        }

        #region helper methods

        /// <summary>
        ///     Add item to a bucket. On Post invoke, the item in the collection will be re-normalized.
        /// </summary>
        /// <param name="item"></param>
        private void AddToBeNormalizedEFContainerItem(EFContainer item)
        {
            if (!_efContainerToBeNormalized.Contains(item))
            {
                _efContainerToBeNormalized.Add(item);
            }
        }

        /// <summary>
        ///     Update function import name if changed.
        /// </summary>
        private void UpdateFunctionImportName()
        {
            var functionImportMapping = FunctionImport.FunctionImportMapping;
            // Update Function-Import name if updated.
            if (String.Compare(FunctionImportName, FunctionImport.LocalName.Value, StringComparison.OrdinalIgnoreCase) != 0)
            {
                FunctionImport.LocalName.Value = FunctionImportName;
                AddToBeNormalizedEFContainerItem(FunctionImport);
                if (functionImportMapping != null)
                {
                    functionImportMapping.FunctionImportName.SetRefName(FunctionImport);
                    AddToBeNormalizedEFContainerItem(functionImportMapping);
                }
            }
        }

        /// <summary>
        ///     Update function import IsComposable attribute if changed.
        /// </summary>
        private void UpdateFunctionImportIsComposable(CommandProcessorContext cpc)
        {
            var previousValue = FunctionImport.IsComposable.Value;
            if (FunctionImportIsComposable != previousValue)
            {
                Command updateComposableCommand = new UpdateDefaultableValueCommand<BoolOrNone>(
                    FunctionImport.IsComposable, FunctionImportIsComposable);
                CommandProcessor.InvokeSingleCommand(cpc, updateComposableCommand);
            }
        }

        /// <summary>
        ///     Update the underlying function/store-procedure if they are changed.
        /// </summary>
        private void UpdateFunctionImportFunction(CommandProcessorContext cpc)
        {
            if (Function != FunctionImport.Function)
            {
                // if the user selected "(None)" then delete the FunctionImportMapping
                if (Function == null)
                {
                    DeleteEFElementCommand.DeleteInTransaction(cpc, FunctionImport.FunctionImportMapping);
                }

                    // if the user selected another stored procedure, update the mapping
                    // and the parameters of the function import
                else
                {
                    var functionImportMapping = FunctionImport.FunctionImportMapping;
                    if (functionImportMapping == null)
                    {
                        // if there isn't a FunctionImportMapping already, we need to create it with the FunctionName
                        if (FunctionImport.Artifact != null
                            && FunctionImport.Artifact.MappingModel() != null
                            && FunctionImport.Artifact.MappingModel().FirstEntityContainerMapping != null)
                        {
                            var cmdFuncImpMapping = new CreateFunctionImportMappingCommand(
                                FunctionImport.Artifact.MappingModel().FirstEntityContainerMapping,
                                Function,
                                FunctionImport);
                            CommandProcessor.InvokeSingleCommand(cpc, cmdFuncImpMapping);
                        }
                    }
                    else
                    {
                        // update the FunctionName in the FunctionImportMapping
                        functionImportMapping.FunctionName.SetRefName(Function);
                        AddToBeNormalizedEFContainerItem(functionImportMapping);
                    }
                }

                // finally, update the parameters of the function import to match the function
                CreateFunctionImportCommand.UpdateFunctionImportParameters(cpc, FunctionImport, Function);
            }
        }

        /// <summary>
        ///     Update function import return type if requested.
        /// </summary>
        private void UpdateFunctionImportReturnType(CommandProcessorContext cpc)
        {
            if (ChangeReturnType)
            {
                // figure out if we are using a complex type, an entity type, primitive type or none as the return type
                var complexType = ReturnSingleType as ComplexType;
                var entityType = ReturnSingleType as EntityType;
                // if returnTypeStringValue is not null, the value could be "None" or the string representation of primitive types (for example: "string", "Int16").
                var returnTypeStringValue = ReturnSingleType as string;

                // Only delete type-mapping if the function-import's return-type does not match type-mapping's type.
                var functionImportMapping = FunctionImport.FunctionImportMapping;
                if (functionImportMapping != null
                    && functionImportMapping.ResultMapping != null)
                {
                    foreach (var typeMapping in functionImportMapping.ResultMapping.TypeMappings().ToList())
                    {
                        // If the old type mapping is FunctionImportComplexTypeMapping and function import does not return a complex type
                        // or return a different complex type.
                        if (typeMapping is FunctionImportComplexTypeMapping
                            && (complexType == null
                                || !String.Equals(typeMapping.TypeName.Target.DisplayName, 
                                                  complexType.DisplayName, StringComparison.CurrentCulture)))
                        {
                            DeleteEFElementCommand.DeleteInTransaction(cpc, typeMapping);
                        }
                            // If the old type mapping is FunctionImportEntityTypeMapping and function import does not return an entity type
                            // or return a different entity type.
                        else if (typeMapping is FunctionImportEntityTypeMapping
                                 && (entityType == null
                                     || !String.Equals(typeMapping.TypeName.Target.DisplayName,
                                                       entityType.DisplayName, StringComparison.CurrentCulture)))
                        {
                            DeleteEFElementCommand.DeleteInTransaction(cpc, typeMapping);
                        }
                    }
                    // If ResultMapping does not contain any type mappings, delete it.
                    if (functionImportMapping.ResultMapping.TypeMappings().Count == 0)
                    {
                        DeleteEFElementCommand.DeleteInTransaction(cpc, functionImportMapping.ResultMapping);
                    }
                }

                // we won't do any equality checking here against the original function import's return type
                // because we would expend cycles determining the 'collection' string, etc.
                string updatedReturnTypeAsString = null;
                if (entityType != null)
                {
                    // Return type could be a collection of EntityType, ComplexType or primitive type.
                    // If we change from the return type from a complex type to an entity type, we need to remove the complex type binding in the function import.
                    // Check if complex type binding is not null and reset it.
                    if (FunctionImport.ReturnTypeAsComplexType != null)
                    {
                        FunctionImport.ReturnTypeAsComplexType.SetRefName(null);
                        FunctionImport.ReturnTypeAsComplexType.Rebind();
                    }

                    // if we are using an entity type, the return type is "Collection(entityType)"
                    Debug.Assert(entityType.EntitySet != null, "Entity Type doesn't have an Entity Set we can use for the Function Import");
                    if (entityType.EntitySet != null)
                    {
                        FunctionImport.EntitySet.SetRefName(entityType.EntitySet);
                        FunctionImport.EntitySet.Rebind();
                        FunctionImport.ReturnTypeAsEntityType.SetRefName(entityType);
                        FunctionImport.ReturnTypeAsEntityType.Rebind();
                    }
                }
                else if (complexType != null)
                {
                    // if we change from an entity type to any other type, we need to remove the EntitySet binding on the FunctionImport
                    if (FunctionImport.EntitySet.RefName != null)
                    {
                        FunctionImport.EntitySet.SetRefName(null);
                        FunctionImport.EntitySet.Rebind();
                    }

                    FunctionImport.ReturnTypeAsComplexType.SetRefName(complexType);
                    FunctionImport.ReturnTypeAsComplexType.Rebind();
                }
                else
                {
                    // Return type could be a collection of EntityType, ComplexType or primitive type.
                    // If we change from the return type from a complex type to a primitive type, we need to remove the complex type binding in the function import.
                    // Check if complex type binding is not null and reset it.
                    if (FunctionImport.ReturnTypeAsComplexType != null)
                    {
                        FunctionImport.ReturnTypeAsComplexType.SetRefName(null);
                        FunctionImport.ReturnTypeAsComplexType.Rebind();
                    }

                    // if we change from an entity type to any other type, we need to remove the EntitySet binding on the FunctionImport
                    if (FunctionImport.EntitySet.RefName != null)
                    {
                        FunctionImport.EntitySet.SetRefName(null);
                        FunctionImport.EntitySet.Rebind();
                    }

                    // if the new value is 'None' then set the return type to null
                    if (returnTypeStringValue != Tools.XmlDesignerBase.Resources.NoneDisplayValueUsedForUX)
                    {
                        updatedReturnTypeAsString = String.Format(
                            CultureInfo.InvariantCulture, FunctionImport.CollectionFormat, returnTypeStringValue);
                    }

                    // update the actual return type of the function import
                    FunctionImport.ReturnTypeAsPrimitiveType.Value = updatedReturnTypeAsString;
                }
            }
        }

        #endregion
    }
}
