// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Globalization;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EFEntityContainerDescriptor : EFAnnotatableElementDescriptor<ConceptualEntityContainer>
    {
        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_GenerateUpdateViews")]
        // Localized description is returned dynamically based on the target Fx. See DescriptionGenerateUpdateViews()
        [TypeConverter(typeof(BoolConverter))]
        public bool GenerateUpdateViews
        {
            get
            {
                var ecm = TypedEFElement.GetAntiDependenciesOfType<EntityContainerMapping>().FirstOrDefault();
                if (ecm != null)
                {
                    return ecm.GenerateUpdateViews.Value;
                }

                return true;
            }
            set
            {
                var ecm = TypedEFElement.GetAntiDependenciesOfType<EntityContainerMapping>().FirstOrDefault();
                if (ecm != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command cmd = new ChangeEntityContainerMappingCommand(ecm, value);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal virtual string DescriptionGenerateUpdateViews()
        {
            if (!EdmFeatureManager.GetGenerateUpdateViewsFeatureState(TypedEFElement.Artifact.SchemaVersion).IsEnabled())
            {
                return String.Format(
                    CultureInfo.CurrentCulture, "({0}) {1}", Resources.DisabledFeatureTooltip,
                    Resources.PropertyWindow_Description_GenerateUpdateViews);
            }
            return Resources.PropertyWindow_Description_GenerateUpdateViews;
        }

        internal virtual bool IsReadOnlyGenerateUpdateViews()
        {
            return (!EdmFeatureManager.GetGenerateUpdateViewsFeatureState(TypedEFElement.Artifact.SchemaVersion)
                           .IsEnabled());
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Access")]
        // Localized description is returned dynamically based on the Target Fx (see DescriptionTypeAccess())
        [TypeConverter(typeof(AccessConverter))]
        public string TypeAccess
        {
            get
            {
                var concEc = TypedEFElement;
                if (concEc != null)
                {
                    return concEc.TypeAccess.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                var concEc = TypedEFElement;
                if (concEc != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    var cmd = new UpdateDefaultableValueCommand<string>(concEc.TypeAccess, value);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        internal virtual string DescriptionTypeAccess()
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

        internal virtual bool IsReadOnlyTypeAccess()
        {
            return (!EdmFeatureManager.GetEntityContainerTypeAccessFeatureState(TypedEFElement.Artifact.SchemaVersion)
                           .IsEnabled());
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("GenerateUpdateViews"))
            {
                var ecm = TypedEFElement.GetAntiDependenciesOfType<EntityContainerMapping>().FirstOrDefault();
                if (ecm != null
                    && ecm.GenerateUpdateViews != null)
                {
                    return ecm.GenerateUpdateViews.DefaultValue;
                }
            }
            if (propertyDescriptorMethodName.Equals("TypeAccess"))
            {
                return TypedEFElement.TypeAccess.DefaultValue;
            }

            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }

        public override string GetClassName()
        {
            return "ConceptualEntityContainer";
        }
    }
}
