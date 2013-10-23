// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal interface IEFConnectionDesignerDescriptorAddOn
    {
        string MetadataArtifactProcessing { get; set; }
    }

    internal interface IEFOptionsDesignerDescriptorAddOn
    {
        bool ValidateOnBuild { get; set; }
        string DatabaseGenerationWorkflow { get; set; }
        string DDLGenerationTemplate { get; set; }
        bool PluralizeNewObjects { get; set; }
        string DatabaseSchemaName { get; set; }
        bool ProcessDependentTemplatesOnSave { get; set; }
        string CodeGenerationStrategy { get; set; }
    }

    internal class EFConnectionDesignerInfoDescriptor : ElementDescriptor<ConnectionDesignerInfo>, IEFConnectionDesignerDescriptorAddOn
    {
        private string _metadataArtifactProcessingDefault;

        public string MetadataArtifactProcessing
        {
            get
            {
                if (_metadataArtifactProcessingDefault == null)
                {
                    _metadataArtifactProcessingDefault = ConnectionManager.GetMetadataArtifactProcessingDefault();
                }

                var val = _metadataArtifactProcessingDefault;
                if (TypedEFElement != null
                    && TypedEFElement.MetadataArtifactProcessingProperty != null
                    && TypedEFElement.MetadataArtifactProcessingProperty.ValueAttr != null)
                {
                    val = TypedEFElement.MetadataArtifactProcessingProperty.ValueAttr.Value;
                }
                return val;
            }
            set
            {
                var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                    TypedEFElement, ConnectionDesignerInfo.AttributeMetadataArtifactProcessing, value);
                if (cmd != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("MetadataArtifactProcessing"))
            {
                return ConnectionManager.GetMetadataArtifactProcessingDefault();
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }

    internal class EFOptionsDesignerInfoDescriptor : ElementDescriptor<OptionsDesignerInfo>, IEFOptionsDesignerDescriptorAddOn
    {
        public bool ValidateOnBuild
        {
            get
            {
                var rtrn = true;
                if (TypedEFElement != null)
                {
                    rtrn = GetBoolValue(TypedEFElement.ValidateOnBuild, true);
                }
                return rtrn;
            }
            set
            {
                if (TypedEFElement != null)
                {
                    var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        TypedEFElement, OptionsDesignerInfo.AttributeValidateOnBuild, value.ToString());
                    if (cmd != null)
                    {
                        var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        public bool PluralizeNewObjects
        {
            get
            {
                var rtrn = OptionsDesignerInfo.EnablePluralizationDefault;
                if (TypedEFElement != null)
                {
                    rtrn = GetBoolValue(TypedEFElement.CheckPluralizationInWizard, rtrn);
                }
                return rtrn;
            }
            set
            {
                if (TypedEFElement != null)
                {
                    var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        TypedEFElement, OptionsDesignerInfo.AttributeEnablePluralization, value.ToString());
                    if (cmd != null)
                    {
                        var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        public bool ProcessDependentTemplatesOnSave
        {
            get
            {
                var rtrn = true;
                if (TypedEFElement != null)
                {
                    rtrn = GetBoolValue(TypedEFElement.ProcessDependentTemplatesOnSave, true);
                }
                return rtrn;
            }
            set
            {
                if (TypedEFElement != null)
                {
                    var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        TypedEFElement, OptionsDesignerInfo.AttributeProcessDependentTemplatesOnSave, value.ToString());
                    if (cmd != null)
                    {
                        var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        public string CodeGenerationStrategy
        {
            get
            {
                var val = Resources.Default;
                if (TypedEFElement != null
                    && TypedEFElement.CodeGenerationStrategy != null
                    && TypedEFElement.CodeGenerationStrategy.ValueAttr != null)
                {
                    val = TypedEFElement.CodeGenerationStrategy.ValueAttr.Value;
                }
                return val;
            }
            set
            {
                if (TypedEFElement != null)
                {
                    var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        TypedEFElement, OptionsDesignerInfo.AttributeCodeGenerationStrategy, value);
                    if (cmd != null)
                    {
                        var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        private static bool GetBoolValue(DesignerProperty designerProperty, bool defaultValue)
        {
            try
            {
                var rtrn = defaultValue;
                if (designerProperty != null
                    && designerProperty.ValueAttr != null)
                {
                    rtrn = bool.Parse(designerProperty.ValueAttr.Value);
                }
                return rtrn;
            }
            catch (FormatException)
            {
                // user has manually modified edmx file to illegal value; just assume default
                return defaultValue;
            }
        }

        public string DatabaseGenerationWorkflow
        {
            get
            {
                if (TypedEFElement != null
                    && TypedEFElement.DatabaseGenerationWorkflow != null
                    && TypedEFElement.DatabaseGenerationWorkflow.ValueAttr != null
                    && String.IsNullOrEmpty(TypedEFElement.DatabaseGenerationWorkflow.ValueAttr.Value) == false)
                {
                    return TypedEFElement.DatabaseGenerationWorkflow.ValueAttr.Value;
                }
                return DatabaseGenerationEngine.DefaultWorkflowPath;
            }
            set
            {
                var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                    TypedEFElement, OptionsDesignerInfo.AttributeDatabaseGenerationWorkflow, value);
                if (cmd != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public string DDLGenerationTemplate
        {
            get
            {
                if (TypedEFElement != null
                    && TypedEFElement.DDLGenerationTemplate != null
                    && TypedEFElement.DDLGenerationTemplate.ValueAttr != null
                    && String.IsNullOrEmpty(TypedEFElement.DDLGenerationTemplate.ValueAttr.Value) == false)
                {
                    return TypedEFElement.DDLGenerationTemplate.ValueAttr.Value;
                }
                return DatabaseGenerationEngine.DefaultTemplatePath;
            }
            set
            {
                var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                    TypedEFElement, OptionsDesignerInfo.AttributeDDLGenerationTemplate, value);
                if (cmd != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public string DatabaseSchemaName
        {
            get
            {
                if (TypedEFElement != null
                    && TypedEFElement.DatabaseSchemaName != null
                    && TypedEFElement.DatabaseSchemaName.ValueAttr != null
                    && String.IsNullOrEmpty(TypedEFElement.DatabaseSchemaName.ValueAttr.Value) == false)
                {
                    return TypedEFElement.DatabaseSchemaName.ValueAttr.Value;
                }
                return DatabaseGenerationEngine.DefaultDatabaseSchemaName;
            }
            set
            {
                var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                    TypedEFElement, OptionsDesignerInfo.AttributeDatabaseSchemaName, value);
                if (cmd != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public bool SynchronizePropertyFacets
        {
            get
            {
                var rtrn = GetSynchronizePropertyFacetsDefault();
                if (TypedEFElement != null)
                {
                    rtrn = GetBoolValue(TypedEFElement.SynchronizePropertyFacets, rtrn);
                }
                return rtrn;
            }
            set
            {
                if (TypedEFElement != null)
                {
                    var cmd = ModelHelper.CreateSetDesignerPropertyCommandInsideDesignerInfo(
                        TypedEFElement, OptionsDesignerInfo.AttributeSynchronizePropertyFacets, value.ToString());
                    if (cmd != null)
                    {
                        var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                        CommandProcessor.InvokeSingleCommand(cpc, cmd);
                    }
                }
            }
        }

        public bool UseLegacyProvider
        {
            get
            {
                var rtrn = OptionsDesignerInfo.UseLegacyProviderDefault;
                if (TypedEFElement != null)
                {
                    rtrn = GetBoolValue(TypedEFElement.UseLegacyProvider, rtrn);
                }

                return rtrn;
            }
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("ValidateOnBuild"))
            {
                return true;
            }
            if (propertyDescriptorMethodName.Equals("PluralizeNewObjects"))
            {
                return OptionsDesignerInfo.EnablePluralizationDefault;
            }
            if (propertyDescriptorMethodName.Equals("DatabaseGenerationWorkflow"))
            {
                return DatabaseGenerationEngine.DefaultWorkflowPath;
            }
            if (propertyDescriptorMethodName.Equals("DDLGenerationTemplate"))
            {
                return DatabaseGenerationEngine.DefaultTemplatePath;
            }
            if (propertyDescriptorMethodName.Equals("ProcessDependentTemplatesOnSave"))
            {
                return true;
            }
            if (propertyDescriptorMethodName.Equals("DatabaseSchemaName"))
            {
                return DatabaseGenerationEngine.DefaultDatabaseSchemaName;
            }
            if (propertyDescriptorMethodName.Equals("CodeGenerationStrategy"))
            {
                return Resources.Default;
            }
            if (propertyDescriptorMethodName.Equals("SynchronizePropertyFacets"))
            {
                return GetSynchronizePropertyFacetsDefault();
            }
            if (propertyDescriptorMethodName.Equals("UseLegacyProvider"))
            {
                return OptionsDesignerInfo.UseLegacyProviderDefault;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }

        private bool GetSynchronizePropertyFacetsDefault()
        {
            return WrappedItem != null && OptionsDesignerInfo.SynchronizePropertyFacetsDefault(WrappedItem.Artifact);
        }
    }
}
