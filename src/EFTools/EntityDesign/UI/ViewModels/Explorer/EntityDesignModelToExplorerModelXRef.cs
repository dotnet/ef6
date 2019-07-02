// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class EntityDesignModelToExplorerModelXRef : ModelToExplorerModelXRef
    {
        private static readonly Dictionary<Type, Type> _modelType2ExplorerViewModelType;

        [SuppressMessage("Microsoft.Performance", "CA1810:InitializeReferenceTypeStaticFieldsInline")]
        static EntityDesignModelToExplorerModelXRef()
        {
            _modelType2ExplorerViewModelType = new Dictionary<Type, Type>();
            _modelType2ExplorerViewModelType.Add(typeof(AssociationSet), typeof(ExplorerAssociationSet));
            _modelType2ExplorerViewModelType.Add(typeof(ConceptualEntityContainer), typeof(ExplorerConceptualEntityContainer));
            _modelType2ExplorerViewModelType.Add(typeof(ConceptualEntityModel), typeof(ExplorerConceptualEntityModel));
            _modelType2ExplorerViewModelType.Add(typeof(ConceptualEntitySet), typeof(ExplorerEntitySet));
            _modelType2ExplorerViewModelType.Add(typeof(ConceptualEntityType), typeof(ExplorerConceptualEntityType));
            _modelType2ExplorerViewModelType.Add(typeof(ComplexType), typeof(ExplorerComplexType));
            _modelType2ExplorerViewModelType.Add(typeof(ConceptualProperty), typeof(ExplorerConceptualProperty));
            _modelType2ExplorerViewModelType.Add(typeof(ComplexConceptualProperty), typeof(ExplorerConceptualProperty));
            _modelType2ExplorerViewModelType.Add(typeof(EntityTypeShape), typeof(ExplorerEntityTypeShape));
            _modelType2ExplorerViewModelType.Add(typeof(EnumType), typeof(ExplorerEnumType));
            _modelType2ExplorerViewModelType.Add(typeof(Diagram), typeof(ExplorerDiagram));
            _modelType2ExplorerViewModelType.Add(typeof(Diagrams), typeof(ExplorerDiagrams));
            _modelType2ExplorerViewModelType.Add(typeof(Function), typeof(ExplorerFunction));
            _modelType2ExplorerViewModelType.Add(typeof(FunctionImport), typeof(ExplorerFunctionImport));
            _modelType2ExplorerViewModelType.Add(typeof(NavigationProperty), typeof(ExplorerNavigationProperty));
            _modelType2ExplorerViewModelType.Add(typeof(Parameter), typeof(ExplorerParameter));
            _modelType2ExplorerViewModelType.Add(typeof(StorageEntityModel), typeof(ExplorerStorageEntityModel));
            _modelType2ExplorerViewModelType.Add(typeof(StorageEntityType), typeof(ExplorerStorageEntityType));
            _modelType2ExplorerViewModelType.Add(typeof(StorageProperty), typeof(ExplorerStorageProperty));
        }

        // <summary>
        //     Helper method that determine whether an EFElement is represented in model browser.
        // </summary>
        internal static bool IsDisplayedInExplorer(EFElement efElement)
        {
            // If efElement type is StorageEntityContainer or EFDesignerInfoRoot, don't display it in Model Browser.
            // Note: GetParentOfType() will also return true if self is of passed-in type.
            if (null != efElement.GetParentOfType(typeof(StorageEntityContainer)))
            {
                return false;
            }
                // We do not display Enum type members
            else if (efElement is EnumTypeMember)
            {
                return false;
            }
                // For any Designer objects, check whether the map between the EFElement and ExplorerEFElement exists.
            else if (null != efElement.GetParentOfType(typeof(EFDesignerInfoRoot)))
            {
                return _modelType2ExplorerViewModelType.ContainsKey(efElement.GetType());
            }

            return true;
        }

        // TODO - make this private, and remove the need to pass in the type to GetNew()/GetNewOrExisting().
        protected override Type GetViewModelTypeForEFElement(EFElement efElement)
        {
            if (!IsDisplayedInExplorer(efElement))
            {
                return null;
            }

            var efElementType = efElement.GetType();

            Type explorerType = null;
            _modelType2ExplorerViewModelType.TryGetValue(efElementType, out explorerType);

            // Get correct view-model type for a c-side or s-side entity type.  
            if (explorerType == null)
            {
                var assoc = efElement as Association;
                if (assoc != null)
                {
                    if (assoc.EntityModel.IsCSDL)
                    {
                        explorerType = typeof(ExplorerConceptualAssociation);
                    }
                    else
                    {
                        explorerType = typeof(ExplorerStorageAssociation);
                    }
                }
            }

            Debug.Assert(explorerType != null, "Unable to find explorer type for efobject type " + efElementType);

            return explorerType;
        }

        protected override bool IsDisplayedInExplorerProtected(EFElement efElement)
        {
            return IsDisplayedInExplorer(efElement);
        }

        internal override Type ItemType
        {
            get { return typeof(EntityDesignModelToExplorerModelXRef); }
        }
    }
}
