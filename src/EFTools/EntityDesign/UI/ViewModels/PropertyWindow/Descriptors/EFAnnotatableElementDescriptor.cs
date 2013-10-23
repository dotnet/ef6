// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Extensibility;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal abstract class EFAnnotatableElementDescriptor<TEFElement> : ElementDescriptor<TEFElement>
        where TEFElement : EFElement
    {
        private readonly Dictionary<PropertyDescriptor, Object> _propertyDescriptorToOwner =
            new Dictionary<PropertyDescriptor, Object>(new PropertyDescriptorEqualityComparer());

        #region Documentation property

        private DocumentationDescriptor _documentation;

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Documentation")]
        [ReadOnly(true)]
        public DocumentationDescriptor Documentation
        {
            get { return _documentation; }
        }

        internal bool IsBrowsableDocumentation()
        {
            return (_documentation != null);
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        protected override void OnTypeDescriptorInitialize()
        {
            var documentableItem = TypedEFElement as EFDocumentableItem;
            Debug.Assert(
                !(this is IAnnotatableDocumentableDescriptor) || documentableItem != null,
                "This descriptor is marked as documentable but the TypedEFElement is not an EFDocumentableItem");
            if (this is IAnnotatableDocumentableDescriptor
                && documentableItem != null)
            {
                _documentation = new DocumentationDescriptor(documentableItem);
            }
        }

        #endregion

        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        [SuppressMessage("Microsoft.Maintainability", "CA1506:AvoidExcessiveClassCoupling")]
        public override PropertyDescriptorCollection GetProperties(Attribute[] attributes)
        {
            var baseProperties = base.GetProperties(attributes);

            // clear out dictionary, as we will rebuild it in this method 
            _propertyDescriptorToOwner.Clear();

            if (RunningInVS)
            {
                // we only dispatch to extensions when we are running in VS.  This is because there is a dep on vS things like ProjectItem
                var el = WrappedItem as EFElement;
                Debug.Assert(el != null, "wrapped item is null");
                if (el != null)
                {
                    var currentSelectionOrNull = EscherExtensionPointManager.DetermineEntityDesignerSelection(el);

                    if (currentSelectionOrNull != null)
                    {
                        var currentSelection = (EntityDesignerSelection)currentSelectionOrNull;
                        Debug.Assert((int)currentSelection != 0, "unexpected value for current selection");
                        var extensions = EscherExtensionPointManager.LoadPropertyDescriptorExtensions();
                        if (extensions.Length > 0)
                        {
                            // Note that if the user adds an EDMX file to their project and chooses not to save the project file,
                            // the project item below can be null; it won't even exist in the miscellaneous files project. 
                            var projectItem = VsUtils.GetProjectItemForDocument(el.Artifact.Uri.LocalPath, PackageManager.Package);
                            if (projectItem != null
                                && VsUtils.EntityFrameworkSupportedInProject(
                                    projectItem.ContainingProject, PackageManager.Package, allowMiscProject: false))
                            {
                                var targetSchemaVersion =
                                    EdmUtils.GetEntityFrameworkVersion(projectItem.ContainingProject, PackageManager.Package);

                                //  Use MEF extensions to create "owner" objects that define new property descriptors to add into the property sheet.
                                //  TODO: We track 'visited factories' since have to load the App-DB contracts as well as the standard contracts, but
                                //        MEF also loads the standard contracts in addition to the App-DB contracts. Need to see if MEF can support
                                //        otherwise.
                                var propDescriptorOwners = new List<Object>();
                                var visitedFactories = new HashSet<IEntityDesignerExtendedProperty>();
                                foreach (var ex in extensions)
                                {
                                    if (((int)ex.Metadata.EntityDesignerSelection & (int)currentSelection) == (int)currentSelection)
                                    {
                                        // Continue processing this extension
                                        var factory = ex.Value;
                                        if (factory != null
                                            && !visitedFactories.Contains(factory))
                                        {
                                            visitedFactories.Add(factory);
                                            try
                                            {
                                                var extensionContext = new PropertyExtensionContextImpl(
                                                    EditingContext, projectItem, targetSchemaVersion,
                                                    factory.GetType().Assembly.GetName().GetPublicKeyToken());

                                                var xelem = el.XElement;
                                                var sem = el as StorageEntityModel;
                                                if (currentSelection == EntityDesignerSelection.StorageModelEntityContainer
                                                    && sem != null)
                                                {
                                                    xelem = sem.FirstEntityContainer.XElement;
                                                }

                                                var o = factory.CreateProperty(xelem, extensionContext);
                                                if (o != null)
                                                {
                                                    propDescriptorOwners.Add(o);
                                                }
                                            }
                                            catch (Exception e)
                                            {
                                                var exceptionMessage = VsUtils.ConstructInnerExceptionErrorMessage(e);
                                                var errorMessage = String.Format(
                                                    CultureInfo.CurrentCulture, Resources.Extensibility_ErrorOccurredDuringCallToExtension,
                                                    factory.GetType().FullName, exceptionMessage);
                                                VsUtils.ShowErrorDialog(errorMessage);
                                            }
                                        }
                                    }
                                }

                                //
                                //  Create PropertyDescriptors over the new "owner"
                                //
                                foreach (var o in propDescriptorOwners)
                                {
                                    var props = TypeDescriptor.GetProperties(o, attributes, false);
                                    foreach (PropertyDescriptor pd in props)
                                    {
                                        var reflectedPropertyDescriptor = new ReflectedPropertyDescriptor(EditingContext, pd, o);
                                        if (reflectedPropertyDescriptor.IsBrowsable)
                                        {
                                            if (_propertyDescriptorToOwner.ContainsKey(reflectedPropertyDescriptor))
                                            {
                                                // handle degenerate error case where dictionary already contains key.  This shouldn't happen, but this is here just to be safe. 
                                                Debug.Fail("_propertyDescriptor2Owner table already contains entry");
                                                _propertyDescriptorToOwner.Remove(reflectedPropertyDescriptor);
                                            }
                                            _propertyDescriptorToOwner.Add(reflectedPropertyDescriptor, o);
                                            baseProperties.Add(reflectedPropertyDescriptor);
                                        }
                                    }
                                }
                            }
                        }
                    }
                }
            }

            return baseProperties;
        }

        // This is called by VS to determine the instance to which the given property should be applied. 
        public override object GetPropertyOwner(PropertyDescriptor pd)
        {
            if (pd != null)
            {
                Object val;
                if (_propertyDescriptorToOwner.TryGetValue(pd, out val))
                {
                    return val;
                }
            }
            return base.GetPropertyOwner(pd);
        }
    }
}
