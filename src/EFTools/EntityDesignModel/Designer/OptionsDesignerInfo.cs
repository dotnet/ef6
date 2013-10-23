// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Designer
{
    using System.Collections.Generic;
    using System.Xml.Linq;

    internal class OptionsDesignerInfo : DesignerInfo
    {
        // DesignerProperty objects
        private DesignerProperty _propValidateOnBuild;
        private DesignerProperty _propDatabaseGenerationWorkflow;
        private DesignerProperty _propDDLGenerationTemplate;
        private DesignerProperty _propUsePluralizationInWizard;
        private DesignerProperty _propIncludeForeignKeysInModel;
        private DesignerProperty _propProcessDependentTemplatesOnSave;
        private DesignerProperty _propDatabaseSchemaName;
        private DesignerProperty _propCodeGenerationStrategy;
        private DesignerProperty _propSynchronizePropertyFacets;
        private DesignerProperty _propUseLegacyProvider;
        internal static readonly string ElementName = "Options";

        // XElement name attribute values
        internal static readonly string AttributeValidateOnBuild = "ValidateOnBuild";
        internal static readonly string AttributeDatabaseGenerationWorkflow = "DatabaseGenerationWorkflow";
        internal static readonly string AttributeDDLGenerationTemplate = "DDLGenerationTemplate";
        internal static readonly string AttributeEnablePluralization = "EnablePluralization";

        // the default value of the EnablePluralization flag, if not explicitly set
        internal static readonly bool EnablePluralizationDefault = true;

        internal static readonly string AttributeProcessDependentTemplatesOnSave = "ProcessDependentTemplatesOnSave";
        internal static readonly string AttributeIncludeForeignKeysInModel = "IncludeForeignKeysInModel";
        internal static readonly string AttributeDatabaseSchemaName = "DefaultDatabaseSchema";
        internal static readonly string AttributeCodeGenerationStrategy = "CodeGenerationStrategy";
        internal static readonly string AttributeSynchronizePropertyFacets = "SynchronizePropertyFacets";
        internal static readonly string AttributeUseLegacyProvider = "UseLegacyProvider";
        internal static readonly bool UseLegacyProviderDefault = true;

        internal OptionsDesignerInfo(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal override string EFTypeName
        {
            get { return ElementName; }
        }

        // virtual to allow mocking
        internal virtual DesignerProperty ValidateOnBuild
        {
            get
            {
                if (_propValidateOnBuild == null
                    && PropertySet != null)
                {
                    PropertySet.TryGetDesignerProperty(AttributeValidateOnBuild, out _propValidateOnBuild);
                }
                return _propValidateOnBuild;
            }
        }

        internal DesignerProperty CheckPluralizationInWizard
        {
            get
            {
                _propUsePluralizationInWizard = SafeGetDesignerProperty(_propUsePluralizationInWizard, AttributeEnablePluralization);
                return _propUsePluralizationInWizard;
            }
        }

        internal DesignerProperty CheckIncludeForeignKeysInModel
        {
            get
            {
                _propIncludeForeignKeysInModel = SafeGetDesignerProperty(_propIncludeForeignKeysInModel, AttributeIncludeForeignKeysInModel);
                return _propIncludeForeignKeysInModel;
            }
        }

        internal DesignerProperty CodeGenerationStrategy
        {
            get
            {
                _propCodeGenerationStrategy = SafeGetDesignerProperty(_propCodeGenerationStrategy, AttributeCodeGenerationStrategy);
                return _propCodeGenerationStrategy;
            }
        }

        internal DesignerProperty DatabaseGenerationWorkflow
        {
            get
            {
                _propDatabaseGenerationWorkflow = SafeGetDesignerProperty(
                    _propDatabaseGenerationWorkflow, AttributeDatabaseGenerationWorkflow);
                return _propDatabaseGenerationWorkflow;
            }
        }

        internal DesignerProperty DDLGenerationTemplate
        {
            get
            {
                _propDDLGenerationTemplate = SafeGetDesignerProperty(_propDDLGenerationTemplate, AttributeDDLGenerationTemplate);
                return _propDDLGenerationTemplate;
            }
        }

        internal DesignerProperty DatabaseSchemaName
        {
            get
            {
                _propDatabaseSchemaName = SafeGetDesignerProperty(_propDatabaseSchemaName, AttributeDatabaseSchemaName);
                return _propDatabaseSchemaName;
            }
        }

        internal DesignerProperty ProcessDependentTemplatesOnSave
        {
            get
            {
                _propProcessDependentTemplatesOnSave = SafeGetDesignerProperty(
                    _propProcessDependentTemplatesOnSave, AttributeProcessDependentTemplatesOnSave);
                return _propProcessDependentTemplatesOnSave;
            }
        }

        internal DesignerProperty SynchronizePropertyFacets
        {
            get
            {
                _propSynchronizePropertyFacets = SafeGetDesignerProperty(_propSynchronizePropertyFacets, AttributeSynchronizePropertyFacets);
                return _propSynchronizePropertyFacets;
            }
        }

        internal DesignerProperty UseLegacyProvider
        {
            get
            {
                _propUseLegacyProvider = SafeGetDesignerProperty(_propUseLegacyProvider, AttributeUseLegacyProvider);

                return _propUseLegacyProvider;
            }
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            return base.ParseSingleElement(unprocessedElements, elem);
        }

        internal static bool SynchronizePropertyFacetsDefault(EFArtifact artifact)
        {
            return artifact != null && artifact.IsSqlFamilyProvider();
        }
    }
}
