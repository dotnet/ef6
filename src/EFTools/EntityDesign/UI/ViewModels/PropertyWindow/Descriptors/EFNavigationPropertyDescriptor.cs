// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System.ComponentModel;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EFNavigationPropertyDescriptor : EFPropertyBaseDescriptor<NavigationProperty>
    {
        [LocDescription("PropertyWindow_Description_NavigationPropertyName")]
        [MergableProperty(false)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_NavPropMultiplicity")]
        [LocDescription("PropertyWindow_Description_NavPropMultiplicity")]
        [TypeConverter(typeof(MultiplicityListConverter))]
        public string Multiplicity
        {
            get
            {
                if (TypedEFElement.ToRole.Status == BindingStatus.Known)
                {
                    return TypedEFElement.ToRole.Target.Multiplicity.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                if (TypedEFElement.ToRole.Target.Multiplicity.Value != value)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new ChangeNavigationPropertyCommand(TypedEFElement, Association, value);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_NavPropType")]
        [LocDescription("PropertyWindow_Description_NavPropType")]
        public string Type
        {
            get
            {
                if (TypedEFElement.ToRole.Status == BindingStatus.Known
                    &&
                    TypedEFElement.ToRole.Target.Type.Status == BindingStatus.Known)
                {
                    if (TypedEFElement.ToRole.Target.Multiplicity.Value == ModelConstants.Multiplicity_Many)
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture, Resources.NavPropType_CollectionText,
                            TypedEFElement.ToRole.Target.Type.Target.LocalName.Value);
                    }
                    else
                    {
                        return string.Format(
                            CultureInfo.CurrentCulture, Resources.NavPropType_InstanceText,
                            TypedEFElement.ToRole.Target.Type.Target.LocalName.Value);
                    }
                }
                else
                {
                    return string.Empty;
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Navigation")]
        [LocDisplayName("PropertyWindow_DisplayName_Association")]
        [LocDescription("PropertyWindow_Description_Association")]
        [TypeConverter(typeof(AssociationListConverter))]
        [MergableProperty(false)]
        public Association Association
        {
            get { return TypedEFElement.Relationship.Target; }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                Command c = new ChangeNavigationPropertyCommand(TypedEFElement, value);
                var cp = new CommandProcessor(cpc, c);
                cp.Invoke();
            }
        }

        [LocCategory("PropertyWindow_Category_Navigation")]
        [LocDisplayName("PropertyWindow_DisplayName_FromRole")]
        [LocDescription("PropertyWindow_Description_FromRole")]
        [TypeConverter(typeof(FromRoleListConverter))]
        [MergableProperty(false)]
        public AssociationEnd FromRole
        {
            get { return TypedEFElement.FromRole.Target; }
            set
            {
                if (value != TypedEFElement.FromRole.Target)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new ChangeNavigationPropertyCommand(TypedEFElement, Association, value, TypedEFElement.FromRole.Target);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Navigation")]
        [LocDisplayName("PropertyWindow_DisplayName_ToRole")]
        [LocDescription("PropertyWindow_Description_ToRole")]
        [TypeConverter(typeof(ToRoleListConverter))]
        [MergableProperty(false)]
        public AssociationEnd ToRole
        {
            get { return TypedEFElement.ToRole.Target; }
            set
            {
                if (value != TypedEFElement.ToRole.Target)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new ChangeNavigationPropertyCommand(TypedEFElement, Association, TypedEFElement.ToRole.Target, value);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        internal override bool IsBrowsableGetter()
        {
            return true;
        }

        internal override bool IsBrowsableSetter()
        {
            return true;
        }

        public bool IsReadOnlyMultiplicity()
        {
            return (!(TypedEFElement.Relationship != null && TypedEFElement.Relationship.Status == BindingStatus.Known));
        }

        public bool IsReadOnlyFromRole()
        {
            return IsRoleReadOnly;
        }

        public bool IsReadOnlyToRole()
        {
            return IsRoleReadOnly;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "NavigationProperty";
        }

        private bool IsRoleReadOnly
        {
            get
            {
                return (!(TypedEFElement.Relationship != null &&
                          TypedEFElement.Relationship.Status == BindingStatus.Known &&
                          TypedEFElement.FromRole.Status == BindingStatus.Known &&
                          TypedEFElement.ToRole.Status == BindingStatus.Known &&
                          TypedEFElement.FromRole.Target.Type.Target == TypedEFElement.ToRole.Target.Type.Target));
            }
        }
    }
}
