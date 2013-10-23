// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal abstract class ModelToExplorerModelXRef : ContextItem
    {
        /// <summary>
        ///     Maps ModelManager types to ModelToExlporerModelXRef types.  This way the base ModelToExplorerModelXRef class can
        ///     get the correct ModelToExplorerModelXRef instance from the EditingContext.
        /// </summary>
        private static readonly Dictionary<Type, Type> _modelManagerType2XRefType = new Dictionary<Type, Type>();

        /// <summary>
        ///     Pairs a ModelManager type with a ModelToExplorerModelXRef type.  Whenever a ModelToExplorerModelXRef is requested,
        ///     the ModelManager type will be used to find the correct ModelToExplorerModelXRef.
        /// </summary>
        /// <param name="modelManagerType">
        ///     The type of ModelManager that should be paired with the xRefType.
        /// </param>
        /// <param name="xRefType">
        ///     The type of ModelToExplorerModelXRef that will be used for the specified ModelManager type.
        /// </param>
        internal static void AddModelManager2XRefType(Type modelManagerType, Type xRefType)
        {
            Debug.Assert(
                typeof(ModelManager).IsAssignableFrom(modelManagerType) && modelManagerType != typeof(ModelManager),
                "modelManagerType needs to be a type that derives from ModelManager.");
            Debug.Assert(
                typeof(ModelToExplorerModelXRef).IsAssignableFrom(xRefType) && xRefType != typeof(ModelToExplorerModelXRef),
                "xRefType needs to be a type that derives from ModelToExplorerModelXRef.");

            Type existingXRefType;
            if (_modelManagerType2XRefType.TryGetValue(modelManagerType, out existingXRefType))
            {
                // if the modelManagerType already exists in the map, make sure the XRef types are the same
                Debug.Assert(
                    existingXRefType == xRefType,
                    "The modelManagerType already exists, but the xRefType is not the same as the existing xRefType.  Either use a different modelManagerType or use the same xRefType.");
            }
            else
            {
                _modelManagerType2XRefType.Add(modelManagerType, xRefType);
            }
        }

        internal static ModelToExplorerModelXRef GetModelToBrowserModelXRef(EditingContext context)
        {
            Type modelToExplorerModelXRefType;
            if (
                !_modelManagerType2XRefType.TryGetValue(
                    context.GetEFArtifactService().Artifact.ModelManager.GetType(), out modelToExplorerModelXRefType))
            {
                Debug.Fail(
                    "Could not find a ModelToExplorerModelXRef type for the ModelManager of type '"
                    + context.GetEFArtifactService().Artifact.ModelManager.GetType()
                    + "'.  Make sure to call AddModelManager2XRefType before calling GetModelToBrowserModelXRef.");
                return null;
            }

            // Update EFElement to BrowserEFElement cross reference so that Search Results can later access it
            var xref = (ModelToExplorerModelXRef)context.Items.GetValue(modelToExplorerModelXRefType);
            if (xref == null)
            {
                xref = (ModelToExplorerModelXRef)Activator.CreateInstance(modelToExplorerModelXRefType);
                context.Items.SetValue(xref);
            }
            return xref;
        }

        internal static ExplorerEFElement GetNew(EditingContext context, EFElement efElement, ExplorerEFElement parent, Type viewModelType)
        {
            return GetNewOrExisting(context, efElement, parent, viewModelType, true);
        }

        internal static ExplorerEFElement GetNewOrExisting(
            EditingContext context, EFElement efElement, ExplorerEFElement parent, Type viewModelType)
        {
            return GetNewOrExisting(context, efElement, parent, viewModelType, false);
        }

        private static ExplorerEFElement GetNewOrExisting(
            EditingContext context, EFElement efElement, ExplorerEFElement parent, Type viewModelType, bool mustNotExist)
        {
            var xref = GetModelToBrowserModelXRef(context);
            var result = xref.GetExisting(efElement);
            if (result != null)
            {
                if (mustNotExist)
                {
                    Debug.Fail(
                        string.Format(
                            CultureInfo.CurrentCulture,
                            Resources.BadInsertChildAlreadyExists, efElement.GetType().FullName, parent.GetType().FullName));
                    return null;
                    // TODO: we need to provide a general exception-handling mechanism and replace the above Assert()
                    // by e.g. the excepiton below
                    // throw new ArgumentException(string.Format(CultureInfo.CurrentCulture, Resources.BadInsertChildAlreadyExists, efElement.GetType().FullName, parent.GetType().FullName));
                }
                else
                {
                    result.Parent = parent;
                }
            }
            else
            {
                if (viewModelType != null)
                {
                    if (!xref.IsDisplayedInExplorerProtected(efElement))
                    {
                        Debug.Fail(
                            "Attempting to create an ExplorerEFElement of type " + viewModelType.FullName +
                            " based on an EFElement which is not displayed in the Explorer " + efElement.ToPrettyString());
                        return null;
                    }

                    result = Activator.CreateInstance(viewModelType, context, efElement, parent) as ExplorerEFElement;
                    xref.Add(efElement, result);
                }
            }

            return result;
        }

        internal static ExplorerEFElement GetParentExplorerElement(EditingContext context, EFElement efElement)
        {
            // Finds the Explorer parent of the efElement 
            // The explorer parent may not be the same as model parent

            ExplorerEFElement explorerParentItem = null;
            var parent = efElement.Parent as EFElement;
            if (parent != null)
            {
                var xref = GetModelToBrowserModelXRef(context);

                var currentItem = efElement;
                while (explorerParentItem == null
                       && currentItem != null)
                {
                    currentItem = currentItem.Parent as EFElement;
                    explorerParentItem = xref.GetExisting(currentItem);
                }
            }

            return explorerParentItem;
        }

        internal static Type GetViewModelTypeForEFlement(EditingContext context, EFElement efElement)
        {
            var xref = GetModelToBrowserModelXRef(context);
            return xref.GetViewModelTypeForEFlement(efElement);
        }

        private readonly Dictionary<EFElement, ExplorerEFElement> _dict = new Dictionary<EFElement, ExplorerEFElement>();

        protected abstract bool IsDisplayedInExplorerProtected(EFElement efElement);

        protected abstract Type GetViewModelTypeForEFlement(EFElement efElement);

        private void Add(EFElement efElement, ExplorerEFElement explorerEFElement)
        {
            _dict.Add(efElement, explorerEFElement);
        }

        internal void Remove(EFElement efElement)
        {
            ExplorerEFElement result;
            _dict.TryGetValue(efElement, out result);
            Debug.Assert(
                result != null,
                "Attempt to remove non-existent EFElement of name " + efElement.DisplayName + " from ModelToExplorerModelXRef");
            if (null != result)
            {
                _dict.Remove(efElement);
            }
        }

        internal ExplorerEFElement GetExisting(EFElement efElement)
        {
            ExplorerEFElement result;
            _dict.TryGetValue(efElement, out result);
            return result;
        }

        internal ExplorerEFElement GetExistingOrParent(EFElement efElement)
        {
            ExplorerEFElement result = null;
            while (result == null
                   && efElement != null)
            {
                if (!_dict.TryGetValue(efElement, out result))
                {
                    efElement = efElement.Parent as EFElement;
                }
            }
            return result;
        }

        internal void Clear()
        {
            _dict.Clear();
        }
    }
}
