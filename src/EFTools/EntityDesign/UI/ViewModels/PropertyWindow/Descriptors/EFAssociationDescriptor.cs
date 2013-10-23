// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Editors;

    internal class EFAssociationDescriptor :
        EFAnnotatableElementDescriptor<Association>,
        IAnnotatableDocumentableDescriptor
    {
        private AssociationEnd end1;
        private AssociationEnd end2;
        private NavigationProperty navProp1;
        private NavigationProperty navProp2;
        private ReferentialConstraintProperty _ref;

        protected override void OnTypeDescriptorInitialize()
        {
            base.OnTypeDescriptorInitialize();
            if (TypedEFElement != null)
            {
                var ends = TypedEFElement.AssociationEnds();
                if (ends.Count > 0)
                {
                    end1 = ends[0];
                    var et1 = end1.Type.Target;
                    var cet1 = et1 as ConceptualEntityType;
                    if (cet1 != null)
                    {
                        navProp1 = cet1.FindNavigationPropertyForEnd(end1);
                    }
                }
                if (ends.Count > 1)
                {
                    end2 = ends[1];
                    var et2 = end2.Type.Target;
                    var cet2 = et2 as ConceptualEntityType;

                    if (cet2 != null)
                    {
                        navProp2 = cet2.FindNavigationPropertyForEnd(end2);
                    }
                }
                _ref = new ReferentialConstraintProperty();
            }
        }

        [LocDescription("PropertyWindow_Descritpion_AssociationName")]
        [MergableProperty(false)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_AssociationSetName")]
        [LocDescription("PropertyWindow_Descritpion_AssociationSetName")]
        [MergableProperty(false)]
        public string AssociationSetName
        {
            get
            {
                var assocSet = TypedEFElement.AssociationSet;
                if (assocSet != null)
                {
                    return assocSet.LocalName.Value;
                }
                return null;
            }
            set
            {
                var assocSet = TypedEFElement.AssociationSet;
                if (assocSet != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new EntityDesignRenameCommand(assocSet, value, true);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        [LocCategory("PropertyWindow_Category_Constraint")]
        [LocDisplayName("PropertyWindow_DisplayName_RefConstraint")]
        [MergableProperty(false)]
        public ReferentialConstraintProperty ReferentialConstraint
        {
            get { return _ref; }
        }

        public bool IsBrowsableReferentialConstraint()
        {
            return TypedEFElement.EntityModel.IsCSDL;
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "Association"; // no need to localize class name
        }

        public bool IsReadOnlyAssociationSetName()
        {
            return IsReadOnly;
        }

        internal override bool IsReadOnlyName()
        {
            return IsReadOnly;
        }

        private bool IsReadOnly
        {
            get { return (TypedEFElement == null || TypedEFElement.EntityModel == null || !TypedEFElement.EntityModel.IsCSDL); }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End1Multiplicity")]
        [LocDescription("PropertyWindow_Descritpion_Multiplicity")]
        [TypeConverter(typeof(End1MultiplicityConverter))]
        public string End1Multiplicity
        {
            get { return GetEndMultiplicity(end1); }
            set { SetEndMultiplicity(end1, value); }
        }

        public bool IsReadOnlyEnd1Multiplicity()
        {
            return IsReadOnly || end1 == null;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End2Multiplicity")]
        [LocDescription("PropertyWindow_Descritpion_Multiplicity")]
        [TypeConverter(typeof(End2MultiplicityConverter))]
        public string End2Multiplicity
        {
            get { return GetEndMultiplicity(end2); }
            set { SetEndMultiplicity(end2, value); }
        }

        public bool IsReadOnlyEnd2Multiplicity()
        {
            return IsReadOnly || end2 == null;
        }

        private static string GetEndMultiplicity(AssociationEnd end)
        {
            if (end == null)
            {
                return String.Empty;
            }
            return end.Multiplicity.Value;
        }

        private static void SetEndMultiplicity(AssociationEnd end, string value)
        {
            var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
            Command c = new ChangeAssociationEndCommand(end, value, null);
            var cp = new CommandProcessor(cpc, c);
            cp.Invoke();
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End1Role")]
        [LocDescription("PropertyWindow_Descritpion_Role")]
        [MergableProperty(false)]
        public string End1Role
        {
            get { return GetEndRole(end1); }
            set { SetEndRole(end1, value); }
        }

        public bool IsReadOnlyEnd1Role()
        {
            return IsReadOnly || end1 == null;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End2Role")]
        [LocDescription("PropertyWindow_Descritpion_Role")]
        [MergableProperty(false)]
        public string End2Role
        {
            get { return GetEndRole(end2); }
            set { SetEndRole(end2, value); }
        }

        public bool IsReadOnlyEnd2Role()
        {
            return IsReadOnly || end2 == null;
        }

        private static string GetEndRole(AssociationEnd end)
        {
            if (end == null)
            {
                return String.Empty;
            }
            return end.Role.Value;
        }

        private static void SetEndRole(AssociationEnd end, string value)
        {
            var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
            Command c = new ChangeAssociationEndCommand(end, null, value);
            var cp = new CommandProcessor(cpc, c);
            cp.Invoke();
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End1NavigationProperty")]
        [LocDescription("PropertyWindow_Descritpion_EndNavigationProperty")]
        [MergableProperty(false)]
        public string End1NavigationProperty
        {
            get { return GetEndNavigationProperty(navProp1); }
            set { SetEndNavigationProperty(navProp1, value); }
        }

        public bool IsReadOnlyEnd1NavigationProperty()
        {
            return IsReadOnly || navProp1 == null;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End2NavigationProperty")]
        [LocDescription("PropertyWindow_Descritpion_EndNavigationProperty")]
        [MergableProperty(false)]
        public string End2NavigationProperty
        {
            get { return GetEndNavigationProperty(navProp2); }
            set { SetEndNavigationProperty(navProp2, value); }
        }

        public bool IsReadOnlyEnd2NavigationProperty()
        {
            return IsReadOnly || navProp2 == null;
        }

        private static string GetEndNavigationProperty(NavigationProperty navProp)
        {
            if (navProp == null)
            {
                return String.Empty;
            }
            return navProp.LocalName.Value;
        }

        private static void SetEndNavigationProperty(NavigationProperty navProp, string value)
        {
            var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
            Command c = new EntityDesignRenameCommand(navProp, value, true);
            var cp = new CommandProcessor(cpc, c);
            cp.Invoke();
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End1OnDelete")]
        [LocDescription("PropertyWindow_Descritpion_EndOnDelete")]
        [TypeConverter(typeof(OnDeleteActionConverter))]
        [MergableProperty(false)]
        public string End1OnDelete
        {
            get { return GetEndOnDelete(end1); }
            set { SetEndOnDelete(end1, value); }
        }

        public bool IsReadOnlyEnd1OnDelete()
        {
            return IsReadOnly || end1 == null;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_End2OnDelete")]
        [LocDescription("PropertyWindow_Descritpion_EndOnDelete")]
        [TypeConverter(typeof(OnDeleteActionConverter))]
        [MergableProperty(false)]
        public string End2OnDelete
        {
            get { return GetEndOnDelete(end2); }
            set { SetEndOnDelete(end2, value); }
        }

        public bool IsReadOnlyEnd2OnDelete()
        {
            return IsReadOnly || end2 == null;
        }

        private static string GetEndOnDelete(AssociationEnd end)
        {
            if (end != null
                && end.OnDeleteAction != null)
            {
                return end.OnDeleteAction.Action.Value;
            }
            return ModelConstants.OnDeleteAction_None;
        }

        private static void SetEndOnDelete(AssociationEnd end, string value)
        {
            var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
            if (end.OnDeleteAction != null
                && value == ModelConstants.OnDeleteAction_None)
            {
                DeleteEFElementCommand.DeleteInTransaction(cpc, end.OnDeleteAction);
            }
            else if (end.OnDeleteAction == null
                     && value == ModelConstants.OnDeleteAction_Cascade)
            {
                CommandProcessor.InvokeSingleCommand(cpc, new CreateOnDeleteActionCommand(end, value));
            }
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("End1Role"))
            {
                return end1.Role.DefaultValue;
            }
            else if (propertyDescriptorMethodName.Equals("End2Role"))
            {
                return end2.Role.DefaultValue;
            }
            if (propertyDescriptorMethodName.Equals("End1OnDelete")
                || propertyDescriptorMethodName.Equals("End2OnDelete"))
            {
                return ModelConstants.OnDeleteAction_None;
            }
            return null;
        }
    }

    [TypeConverter(typeof(ReferentialConstraintConverter))]
    [Editor(typeof(ReferentialConstraintEditor), typeof(UITypeEditor))]
    internal class ReferentialConstraintProperty
    {
        public ReferentialConstraintProperty()
        {
        }
    }
}
