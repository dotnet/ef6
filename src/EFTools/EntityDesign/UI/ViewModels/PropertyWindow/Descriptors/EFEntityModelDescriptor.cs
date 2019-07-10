// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EFEntityModelDescriptor : EFAnnotatableElementDescriptor<ConceptualEntityModel>,
                                             IEFConnectionDesignerDescriptorAddOn,
                                             IEFOptionsDesignerDescriptorAddOn
    {
        private EFConnectionDesignerInfoDescriptor _EFConnectionDesignerInfoDescriptor;
        private EFOptionsDesignerInfoDescriptor _EFOptionsDesignerInfoDescriptor;

        internal override void Initialize(EFObject obj, EditingContext editingContext, bool runningInVS)
        {
            base.Initialize(obj, editingContext, runningInVS);
            if (obj != null)
            {
                LoadDesignerInfoAndDescriptors(editingContext, obj.Artifact);
            }
        }

        private void LoadDesignerInfoAndDescriptors(EditingContext editingContext, EFArtifact artifact)
        {
            if (artifact != null
                && artifact.DesignerInfo() != null)
            {
                DesignerInfo connectionDesignerInfo = null;
                DesignerInfo optionsDesignerInfo = null;

                var foundConnectionDesignerInfo = artifact.DesignerInfo()
                    .TryGetDesignerInfo(ConnectionDesignerInfo.ElementName, out connectionDesignerInfo);

                if (foundConnectionDesignerInfo)
                {
                    var connectionDesigner = connectionDesignerInfo as ConnectionDesignerInfo;
                    Debug.Assert(connectionDesigner != null, "DesignerInfo with element name 'Connection' must be a ConnectionDesignerInfo");

                    if (connectionDesigner != null)
                    {
                        // if the owner of the edmx file is a website, then we can 
                        // only have one possible value (EmbedInOutputAssembly) for metadata artifact processing
                        // however the item template just adds CopyToOutputDirectory, so we need to fix it
                        var project = VSHelpers.GetProjectForDocument(artifact.Uri.LocalPath, PackageManager.Package);
                        if (project != null)
                        {
                            var appType = VsUtils.GetApplicationType(Services.ServiceProvider, project);
                            if (appType == VisualStudioProjectSystem.Website)
                            {
                                var mapDefault = ConnectionManager.GetMetadataArtifactProcessingDefault();
                                if (connectionDesigner.MetadataArtifactProcessingProperty != null
                                    && connectionDesigner.MetadataArtifactProcessingProperty.ValueAttr.Value != mapDefault)
                                {
                                    var cpc = new CommandProcessorContext(
                                        editingContext, EfiTransactionOriginator.PropertyWindowOriginatorId,
                                        Resources.Tx_ChangeMetadataArtifactProcessing);
                                    var cmd =
                                        new UpdateDefaultableValueCommand<string>(
                                            connectionDesigner.MetadataArtifactProcessingProperty.ValueAttr, mapDefault);
                                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                                }
                            }
                        }

                        _EFConnectionDesignerInfoDescriptor = new EFConnectionDesignerInfoDescriptor();
                        _EFConnectionDesignerInfoDescriptor.Initialize(connectionDesigner, editingContext);
                    }
                }

                var foundOptionsDesignerInfo = artifact.DesignerInfo()
                    .TryGetDesignerInfo(OptionsDesignerInfo.ElementName, out optionsDesignerInfo);

                if (foundOptionsDesignerInfo)
                {
                    _EFOptionsDesignerInfoDescriptor = new EFOptionsDesignerInfoDescriptor();
                    _EFOptionsDesignerInfoDescriptor.Initialize(optionsDesignerInfo as OptionsDesignerInfo, editingContext);
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_Namespace")]
        [LocDescription("PropertyWindow_Description_Namespace")]
        public string Namespace
        {
            get { return TypedEFElement.Namespace.Value; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                Command c = new RenameConceptualNamespaceCommand(TypedEFElement, value);
                var cp = new CommandProcessor(cpc, c);
                cp.Invoke();
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_EntityContainerName")]
        [LocDescription("PropertyWindow_Description_EntityContainerName")]
        public string EntityContainerName
        {
            get
            {
                var container = TypedEFElement.FirstEntityContainer;
                if (container != null)
                {
                    return container.LocalName.Value;
                }
                return null;
            }

            set
            {
                var container = TypedEFElement.FirstEntityContainer;
                var previousContainerName = container.LocalName.Value;
                if (value == previousContainerName)
                {
                    // no change to name - just return
                    return;
                }

                // if the set of existing connection string names contains the one you're
                // trying to change to then raise an error message
                if (null != TypedEFElement.Artifact
                    &&
                    null != TypedEFElement.Artifact.Uri
                    &&
                    null != TypedEFElement.Artifact.Uri.LocalPath)
                {
                    var project = VSHelpers.GetProjectForDocument(TypedEFElement.Artifact.Uri.LocalPath, PackageManager.Package);
                    if (null != project)
                    {
                        if (PackageManager.Package.ConnectionManager.HasConnectionString(project, value))
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, Resources.DuplicateEntityContainerName, value);
                            throw new CommandValidationFailedException(msg);
                        }
                    }
                }

                // otherwise implement the name change
                if (container != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new EntityDesignRenameCommand(container, value, true);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_EntityContainerAccess")]
        // Localized description is returned dynamically based on the Target Fx (see DescriptionEntityContainerAccess())
        [TypeConverter(typeof(AccessConverter))]
        public string EntityContainerAccess
        {
            get
            {
                var container = TypedEFElement.FirstEntityContainer as ConceptualEntityContainer;
                if (container != null
                    && container.TypeAccess != null)
                {
                    return container.TypeAccess.Value;
                }
                return null;
            }

            set
            {
                var container = TypedEFElement.FirstEntityContainer as ConceptualEntityContainer;
                if (container != null
                    && container.TypeAccess != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new UpdateDefaultableValueCommand<string>(container.TypeAccess, value);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        internal virtual string DescriptionEntityContainerAccess()
        {
            if (!EdmFeatureManager.GetEntityContainerTypeAccessFeatureState(TypedEFElement.Artifact.SchemaVersion)
                       .IsEnabled())
            {
                return String.Format(
                    CultureInfo.CurrentCulture, "({0}) {1}", Resources.DisabledFeatureTooltip,
                    Resources.PropertyWindow_Description_EntityContainerAccess);
            }
            return Resources.PropertyWindow_Description_EntityContainerAccess;
        }

        internal virtual bool IsReadOnlyEntityContainerAccess()
        {
            return (!EdmFeatureManager.GetEntityContainerTypeAccessFeatureState(TypedEFElement.Artifact.SchemaVersion)
                           .IsEnabled());
        }

        [LocCategory("PropertyWindow_Category_Connection")]
        [LocDisplayName("PropertyWindow_DisplayName_ConnectionString")]
        [LocDescription("PropertyWindow_Description_ConnectionString")]
        public string ConnectionString
        {
            get
            {
                var container = TypedEFElement.FirstEntityContainer;
                if (container != null)
                {
                    var documentPath = EditingContext.GetEFArtifactService().Artifact.Uri.LocalPath;
                    var project = VSHelpers.GetProjectForDocument(documentPath, PackageManager.Package);
                    var connectionString = ConnectionManager.GetConnectionStringObject(project, container.LocalName.Value);
                    if (connectionString != null)
                    {
                        return connectionString.Text;
                    }
                }
                return null;
            }
        }

        #region EFDesignerModelDescriptor proxy properties

        [LocCategory("PropertyWindow_Category_Connection")]
        [LocDisplayName("PropertyWindow_DisplayName_MetadataArtifactProcessing")]
        [LocDescription("PropertyWindow_Description_MetadataArtifactProcessing")]
        [TypeConverter(typeof(MetadataArtifactProcessingConverter))]
        public string MetadataArtifactProcessing
        {
            get
            {
                if (_EFConnectionDesignerInfoDescriptor != null)
                {
                    return _EFConnectionDesignerInfoDescriptor.MetadataArtifactProcessing;
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {
                if (_EFConnectionDesignerInfoDescriptor != null)
                {
                    _EFConnectionDesignerInfoDescriptor.MetadataArtifactProcessing = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_ValidateOnBuild")]
        [LocDescription("PropertyWindow_Description_ValidateOnBuild")]
        [TypeConverter(typeof(BoolConverter))]
        public bool ValidateOnBuild
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.ValidateOnBuild;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.ValidateOnBuild = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_PluralizeNewObjects")]
        [LocDescription("PropertyWindow_Description_PluralizeNewObjects")]
        [TypeConverter(typeof(BoolConverter))]
        public bool PluralizeNewObjects
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.PluralizeNewObjects;
                }
                else
                {
                    return OptionsDesignerInfo.EnablePluralizationDefault;
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.PluralizeNewObjects = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_DatabaseScriptGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_DatabaseGenerationWorkflow")]
        [LocDescription("PropertyWindow_Description_DatabaseGenerationWorkflow")]
        [TypeConverter(typeof(DbGenWorkflowFileListConverter))]
        public string DatabaseGenerationWorkflow
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.DatabaseGenerationWorkflow;
                }
                return DatabaseGenerationEngine.DefaultWorkflowPath;
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.DatabaseGenerationWorkflow = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_DatabaseScriptGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_DDLGenerationTemplate")]
        [LocDescription("PropertyWindow_Description_DDLGenerationTemplate")]
        [TypeConverter(typeof(DbGenTemplateFileListConverter))]
        public string DDLGenerationTemplate
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.DDLGenerationTemplate;
                }
                else
                {
                    return DatabaseGenerationEngine.DefaultTemplatePath;
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.DDLGenerationTemplate = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_DatabaseScriptGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_DatabaseSchemaName")]
        [LocDescription("PropertyWindow_Description_DatabaseSchemaName")]
        public string DatabaseSchemaName
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.DatabaseSchemaName;
                }
                else
                {
                    return DatabaseGenerationEngine.DefaultDatabaseSchemaName;
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.DatabaseSchemaName = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_ProcessDependentTemplatesOnSave")]
        [LocDescription("PropertyWindow_Description_ProcessDependentTemplatesOnSave")]
        [TypeConverter(typeof(BoolConverter))]
        public bool ProcessDependentTemplatesOnSave
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.ProcessDependentTemplatesOnSave;
                }
                else
                {
                    return true;
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.ProcessDependentTemplatesOnSave = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_CodeGenerationStrategy")]
        [LocDescription("PropertyWindow_Description_CodeGenerationStrategy")]
        [TypeConverter(typeof(CodeGenerationStrategyConverter))]
        public string CodeGenerationStrategy
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.CodeGenerationStrategy;
                }
                else
                {
                    return String.Empty;
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.CodeGenerationStrategy = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Schema")]
        [LocDisplayName("PropertyWindow_DisplayName_SynchronizePropertyFacets")]
        [LocDescription("PropertyWindow_Description_SynchronizePropertyFacets")]
        [TypeConverter(typeof(BoolConverter))]
        public bool SynchronizePropertyFacets
        {
            get
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    return _EFOptionsDesignerInfoDescriptor.SynchronizePropertyFacets;
                }
                else
                {
                    return OptionsDesignerInfo.SynchronizePropertyFacetsDefault(TypedEFElement.Artifact);
                }
            }
            set
            {
                if (_EFOptionsDesignerInfoDescriptor != null)
                {
                    _EFOptionsDesignerInfoDescriptor.SynchronizePropertyFacets = value;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Connection")]
        [LocDisplayName("PropertyWindow_DisplayName_UseLegacyProvider")]
        [LocDescription("PropertyWindow_Description_UseLegacyProvider")]
        [TypeConverter(typeof(BoolConverter))]
        public bool UseLegacyProvider
        {
            get
            {
                return _EFOptionsDesignerInfoDescriptor != null
                           ? _EFOptionsDesignerInfoDescriptor.UseLegacyProvider
                           : OptionsDesignerInfo.UseLegacyProviderDefault;
            }
        }

        #endregion

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_LazyLoadingEnabled")]
        [TypeConverter(typeof(BoolConverter))]
        public bool LazyLoadingEnabled
        {
            get
            {
                // If LazyLoading is not supported, we want to show false regardless what is in the model.
                if (EdmFeatureManager.GetLazyLoadingFeatureState(TypedEFElement.Artifact.SchemaVersion).IsEnabled())
                {
                    var container = TypedEFElement.FirstEntityContainer as ConceptualEntityContainer;
                    if (container != null)
                    {
                        return container.LazyLoadingEnabled.Value;
                    }
                }
                return false;
            }
            set
            {
                var container = TypedEFElement.FirstEntityContainer as ConceptualEntityContainer;
                if (container != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new UpdateDefaultableValueCommand<bool>(container.LazyLoadingEnabled, value);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        internal bool IsReadOnlyLazyLoadingEnabled()
        {
            return (!EdmFeatureManager.GetLazyLoadingFeatureState(TypedEFElement.Artifact.SchemaVersion).IsEnabled());
        }

        internal bool IsReadOnlyCodeGenerationStrategy()
        {
            if (TypedEFElement.Artifact.SchemaVersion < EntityFrameworkVersion.Version3 || UseLegacyProvider)
            {
                return false;
            }

            var originalPath = TypedEFElement.Artifact.Uri.LocalPath;
            var project = VSHelpers.GetProjectForDocument(originalPath, Services.ServiceProvider);
            var entityFrameworkAssemblyVersion = VsUtils.GetInstalledEntityFrameworkAssemblyVersion(project);
            return entityFrameworkAssemblyVersion != null && entityFrameworkAssemblyVersion >= RuntimeVersion.Version6;
        }

        internal virtual string DescriptionLazyLoadingEnabled()
        {
            if (!EdmFeatureManager.GetLazyLoadingFeatureState(TypedEFElement.Artifact.SchemaVersion).IsEnabled())
            {
                return String.Format(
                    CultureInfo.CurrentCulture, "({0}) {1}", Resources.DisabledFeatureTooltip,
                    Resources.PropertyWindow_Description_LazyLoadingEnabled);
            }
            return Resources.PropertyWindow_Description_LazyLoadingEnabled;
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_UseStrongSpatialTypes")]
        [TypeConverter(typeof(BoolConverter))]
        public bool UseStrongSpatialTypes
        {
            get
            {
                // If UseStrongSpatialTypes is not supported, we want to show true regardless what is in the model.
                if (EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(TypedEFElement.Artifact.SchemaVersion)
                        .IsEnabled())
                {
                    var cem = TypedEFElement;
                    if (cem != null)
                    {
                        return cem.UseStrongSpatialTypes.Value;
                    }
                }

                return true;
            }
            set
            {
                var cem = TypedEFElement;
                if (cem != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new UpdateDefaultableValueCommand<bool>(cem.UseStrongSpatialTypes, value);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal bool IsReadOnlyUseStrongSpatialTypes()
        {
            // TODO: when runtime support for the other (true) setting of this attribute is available replace the "return true" below by the commented line below it
            return true;
            // return (!EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(TypedEFElement.Artifact.SchemaVersion));
        }

        internal virtual string DescriptionUseStrongSpatialTypes()
        {
            if (!EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(TypedEFElement.Artifact.SchemaVersion)
                     .IsEnabled())
            {
                return String.Format(
                    CultureInfo.CurrentCulture, "({0}) {1}", Resources.DisabledFeatureTooltip,
                    Resources.PropertyWindow_Description_UseStrongSpatialTypes);
            }
            return Resources.PropertyWindow_Description_UseStrongSpatialTypes;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "ConceptualEntityModel";
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("Alias"))
            {
                return TypedEFElement.Alias.DefaultValue;
            }
            if (propertyDescriptorMethodName.Equals("EntityContainerAccess"))
            {
                var container = TypedEFElement.FirstEntityContainer as ConceptualEntityContainer;
                if (container != null
                    && container.TypeAccess != null)
                {
                    return container.TypeAccess.DefaultValue;
                }
            }
            if (propertyDescriptorMethodName.Equals("LazyLoadingEnabled"))
            {
                var container = TypedEFElement.FirstEntityContainer as ConceptualEntityContainer;
                if (container != null
                    && container.LazyLoadingEnabled != null)
                {
                    return container.LazyLoadingEnabled.DefaultValue;
                }
            }
            if (propertyDescriptorMethodName.Equals("UseStrongSpatialTypes"))
            {
                var cem = TypedEFElement;
                if (cem != null
                    && cem.UseStrongSpatialTypes != null)
                {
                    return cem.UseStrongSpatialTypes.DefaultValue;
                }
            }

            object defaultValue = null;

            // Ask the Connections TypeDescriptor if it can decide the default value
            if (_EFConnectionDesignerInfoDescriptor != null)
            {
                defaultValue = _EFConnectionDesignerInfoDescriptor.GetDescriptorDefaultValue(propertyDescriptorMethodName);
            }

            // Ask the Options TypeDescriptor if it can decide the default value
            if (defaultValue == null
                && _EFOptionsDesignerInfoDescriptor != null)
            {
                defaultValue = _EFOptionsDesignerInfoDescriptor.GetDescriptorDefaultValue(propertyDescriptorMethodName);
            }

            if (defaultValue != null)
            {
                return defaultValue;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
