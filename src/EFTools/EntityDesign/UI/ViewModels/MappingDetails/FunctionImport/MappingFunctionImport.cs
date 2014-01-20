// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails.FunctionImports
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Base.Shell;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Branches;
    using Microsoft.Data.Entity.Design.UI.Views.MappingDetails.Columns;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using EFExtensions = Microsoft.Data.Entity.Design.Model.EFExtensions;

    // <summary>
    //     This class represents FunctionImportMapping and it's result mappings
    // </summary>
    [TreeGridDesignerRootBranch(typeof(FunctionImportBranch))]
    [TreeGridDesignerColumn(typeof(PropertyColumn), Order = 1)]
    [TreeGridDesignerColumn(typeof(OperatorColumn), Order = 2)]
    [TreeGridDesignerColumn(typeof(ColumnNameColumn), Order = 3)]
    internal class MappingFunctionImport : MappingFunctionImportMappingRoot
    {
        public MappingFunctionImport(EditingContext context, FunctionImportMapping functionImportMapping, MappingEFElement parent)
            : base(context, functionImportMapping, parent)
        {
        }

        internal FunctionImportMapping FunctionImportMapping
        {
            get { return ModelItem as FunctionImportMapping; }
        }

        internal FunctionImport FunctionImport
        {
            get { return FunctionImportMapping.FunctionImportName.Target; }
        }

        // <summary>
        //     Name of the c-side FunctionImport
        // </summary>
        internal override string Name
        {
            get
            {
                var fi = FunctionImportMapping.FunctionImportName.Target;
                Debug.Assert(fi != null, "Couldn't find FunctionImport binding");
                if (fi != null)
                {
                    return fi.LocalName.Value;
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        // <summary>
        //     Name of the s-side Function
        // </summary>
        internal string FunctionName
        {
            get
            {
                var function = FunctionImportMapping.FunctionName.Target;
                Debug.Assert(function != null, "Couldn't find Function binding");
                if (function != null)
                {
                    return string.Format(
                        CultureInfo.CurrentCulture, Resources.MappingDetailsViewModel_StorageEntityTypeName, function.LocalName.Value);
                }
                else
                {
                    return String.Empty;
                }
            }
        }

        // <summary>
        //     Returns list of all s-side functions
        // </summary>
        internal override Dictionary<MappingLovEFElement, string> GetListOfValues(ListOfValuesCollection type)
        {
            var lov = new Dictionary<MappingLovEFElement, string>();

            Debug.Assert(type == ListOfValuesCollection.FirstColumn, "Unsupported lov type was sent");

            if (type == ListOfValuesCollection.FirstColumn)
            {
                var functionImport = FunctionImportMapping.FunctionImportName.Target;
                Debug.Assert(functionImport != null, "Couldn't find FunctionImport binding");
                if (functionImport != null)
                {
                    var storageModel = EFExtensions.StorageModel(functionImport.Artifact);

                    foreach (var function in storageModel.Functions())
                    {
                        // add the Function to the list
                        lov.Add(new MappingLovEFElement(function, function.DisplayName), function.DisplayName);
                    }

                    return lov;
                }
            }

            return base.GetListOfValues(type);
        }

        // <summary>
        //     Returns list of MappingFunctionImportScalarProperty for each Property from the ReturnType of the FunctionImport
        // </summary>
        internal IList<MappingFunctionImportScalarProperty> GetScalarProperties()
        {
            var mappingScalarProperties = new List<MappingFunctionImportScalarProperty>();
            var fi = FunctionImportMapping.FunctionImportName.Target;
            Debug.Assert(fi != null, "No binding to FunctionImport found");
            if (fi != null)
            {
                // first get the ReturnType of the FunctionImport (should be either EntityType or ComplexType)
                ComplexType complexType = null;
                EntityType entityType = null;
                if (fi.IsReturnTypeEntityType)
                {
                    entityType = fi.ReturnTypeAsEntityType.Target;
                }
                else if (fi.IsReturnTypeComplexType
                         && fi.ReturnTypeAsComplexType.Target != null)
                {
                    complexType = fi.ReturnTypeAsComplexType.Target;
                }

                Debug.Assert(entityType != null || complexType != null, "Couldn't find FunctionImport return type");
                if (entityType != null
                    || complexType != null)
                {
                    // check wether we have corresponding TypeMapping inside the FunctionImportMapping
                    FunctionImportTypeMapping typeMapping = null;
                    if (FunctionImportMapping.ResultMapping != null)
                    {
                        typeMapping = entityType != null
                                          ? FunctionImportMapping.ResultMapping.FindTypeMapping(entityType)
                                          : FunctionImportMapping.ResultMapping.FindTypeMapping(complexType);
                    }

                    var properties = entityType != null ? entityType.Properties() : complexType.Properties();
                    foreach (var property in properties)
                    {
                        if (typeMapping != null)
                        {
                            // check whether we already have ScalarProperty for this Property
                            var scalarProperty = typeMapping.FindScalarProperty(property);
                            if (scalarProperty != null)
                            {
                                // add existing ScalarProperty to the list
                                mappingScalarProperties.Add(new MappingFunctionImportScalarProperty(_context, scalarProperty, this));
                                continue;
                            }
                        }
                        // if not, add a dummy node
                        mappingScalarProperties.Add(new MappingFunctionImportScalarProperty(_context, property, this));
                    }
                }
            }
            return mappingScalarProperties;
        }

        // <summary>
        //     Changes the s-side Function in the FunctionImportMapping
        // </summary>
        internal void ChangeModelItem(EditingContext context, Function newFunction)
        {
            var fi = FunctionImportMapping.FunctionImportName.Target;
            if (fi != null
                && FunctionImportMapping.FunctionName.Target != newFunction)
            {
                var cModel = EFExtensions.RuntimeModelRoot(FunctionImportMapping.FunctionImportName.Target) as ConceptualEntityModel;
                if (cModel != null)
                {
                    var cmd = new ChangeFunctionImportCommand(
                        cModel.FirstEntityContainer as ConceptualEntityContainer,
                        fi,
                        newFunction,
                        fi.LocalName.Value,
                        fi.IsComposable.Value,
                        false,
                        null);
                    // This command can be called before the provider has been registered with the 
                    // resolver (e.g. Mapping Details window on a newly-opened project) - so ensure
                    // provider is registered here before any commands are invoked
                    var cpc = new CommandProcessorContext(
                        context, EfiTransactionOriginator.MappingDetailsOriginatorId, Resources.Tx_ChangeFuncImpMapping);
                    VsUtils.EnsureProvider(cpc.Artifact);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        protected override void LoadChildrenCollection()
        {
            foreach (var sp in GetScalarProperties())
            {
                _children.Add(sp);
            }
        }

        protected override void OnChildDeleted(MappingEFElement melem)
        {
            var child = melem as MappingFunctionImportScalarProperty;
            Debug.Assert(child != null, "Unknown child being deleted");
            if (child != null)
            {
                _children.Remove(child);
                return;
            }

            base.OnChildDeleted(melem);
        }
    }
}
