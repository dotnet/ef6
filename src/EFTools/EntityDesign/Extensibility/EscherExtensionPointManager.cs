// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.ComponentModel.Composition.Hosting;
    using System.Diagnostics.CodeAnalysis;
    using System.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio.Model;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.ComponentModelHost;

    [SuppressMessage("Microsoft.Design", "CA1001:TypesThatOwnDisposableFieldsShouldBeDisposable")]
    internal class EscherExtensionPointManager
    {
        private static EscherExtensionPointManager _instance;

        // This is the source that the app pulls MEF components from
        private readonly ExportProvider _exportProvider;

        private EscherExtensionPointManager()
        {
            var componentModelService = (IComponentModel)PackageManager.Package.GetService(typeof(SComponentModel));
            _exportProvider = componentModelService.DefaultExportProvider;
        }

        internal ExportProvider ExportProvider
        {
            get { return _exportProvider; }
        }

        internal static LayerManager LayerManager
        {
            get
            {
                var vsArtifact = PackageManager.Package.DocumentFrameMgr.CurrentArtifact as VSArtifact;
                return vsArtifact != null ? vsArtifact.LayerManager : null;
            }
        }

        private static EscherExtensionPointManager Instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new EscherExtensionPointManager();
                }
                return _instance;
            }
        }

        internal static Lazy<IEntityDesignerCommandFactory>[] LoadCommandExtensions(bool excludeLayers, bool excludeNonLayers)
        {
            return LoadLayerFilteredExtensions<IEntityDesignerCommandFactory>(excludeLayers, excludeNonLayers).ToArray();
        }

        internal static Lazy<IEntityDesignerExtendedProperty, IEntityDesignerPropertyData>[] LoadPropertyDescriptorExtensions()
        {
            return LoadLayerFilteredExtensions<IEntityDesignerExtendedProperty, IEntityDesignerPropertyData>().ToArray();
        }

        internal static Lazy<IModelTransformExtension>[] LoadModelTransformExtensions()
        {
            return LoadLayerFilteredExtensions<IModelTransformExtension>().ToArray();
        }

        internal static Lazy<IModelConversionExtension, IEntityDesignerConversionData>[] LoadModelConversionExtensions()
        {
            return LoadLayerFilteredExtensions<IModelConversionExtension, IEntityDesignerConversionData>().ToArray();
        }

        internal static Lazy<IModelGenerationExtension>[] LoadModelGenerationExtensions()
        {
            return LoadLayerFilteredExtensions<IModelGenerationExtension>().ToArray();
        }

        private static IEnumerable<Lazy<T>> LoadLayerFilteredExtensions<T>(bool excludeLayers = false, bool excludeNonLayers = false)
        {
            // if a layer manager exists, then use it to filter the extension based on what layers are enabled
            // or not. Otherwise, remove all layer-specific extensions from the list we're going to return.
            var extensions = new List<Lazy<T, IEntityDesignerLayerData>>();
            extensions.AddRange(Instance.ExportProvider.GetExports<T, IEntityDesignerLayerData>());
            var layerManager = LayerManager;
            if (layerManager != null)
            {
                return layerManager.Filter(extensions, excludeLayers, excludeNonLayers);
            }

            return extensions.Where(l => String.IsNullOrEmpty(l.Metadata.LayerName));
        }

        private static IEnumerable<Lazy<T, M>> LoadLayerFilteredExtensions<T, M>()
        {
            // if a layer manager exists, then use it to filter the extension based on what layers are enabled
            // or not. Otherwise, remove all layer-specific extensions from the list we're going to return.
            var extensions = Instance.ExportProvider.GetExports<T, M>();
            var layerManager = LayerManager;
            if (layerManager != null)
            {
                return layerManager.Filter(extensions);
            }

            return extensions.Where(
                l =>
                    {
                        var layerData = l.Metadata as IEntityDesignerLayerData;
                        return layerData == null || String.IsNullOrEmpty(layerData.LayerName);
                    });
        }

        internal static IEnumerable<Lazy<IEntityDesignerLayer>> LoadLayerExtensions()
        {
            return Instance.ExportProvider.GetExports<IEntityDesignerLayer>();
        }

        #region Helpers

        internal static EntityDesignerSelection? DetermineEntityDesignerSelection(EFElement el)
        {
            var isConceptual = el.RuntimeModelRoot() is ConceptualEntityModel;

            if (el.EFTypeName == BaseEntityModel.ElementName)
            {
                if (el is ConceptualEntityModel)
                {
                    return EntityDesignerSelection.DesignerSurface;
                }
                else
                {
                    return EntityDesignerSelection.StorageModelEntityContainer;
                }
            }
            else if (el.EFTypeName == EntitySet.ElementName && isConceptual)
            {
                return EntityDesignerSelection.ConceptualModelEntitySet;
            }
            else if (el.EFTypeName == AssociationSet.ElementName && isConceptual)
            {
                return EntityDesignerSelection.ConceptualModelAssociationSet;
            }
            else if (el.EFTypeName == BaseEntityContainer.ElementName)
            {
                if (isConceptual)
                {
                    return EntityDesignerSelection.ConceptualModelEntityContainer;
                }
                else
                {
                    return EntityDesignerSelection.StorageModelEntityContainer;
                }
            }
            else if (el.EFTypeName == EntityType.ElementName)
            {
                if (isConceptual)
                {
                    return EntityDesignerSelection.ConceptualModelEntityType;
                }
                else
                {
                    return EntityDesignerSelection.StorageModelEntityType;
                }
            }
            else if (el.EFTypeName == Property.ElementName)
            {
                if (isConceptual)
                {
                    if (el is ComplexConceptualProperty)
                    {
                        return EntityDesignerSelection.ConceptualModelComplexProperty;
                    }
                    else
                    {
                        return EntityDesignerSelection.ConceptualModelProperty;
                    }
                }
                else
                {
                    return EntityDesignerSelection.StorageModelProperty;
                }
            }
            else if (el.EFTypeName == NavigationProperty.ElementName)
            {
                return EntityDesignerSelection.ConceptualModelNavigationProperty;
            }
            else if (el.EFTypeName == Association.ElementName)
            {
                if (isConceptual)
                {
                    return EntityDesignerSelection.ConceptualModelAssociation;
                }
                else
                {
                    return EntityDesignerSelection.StorageModelAssociation;
                }
            }
            else if (el.EFTypeName == ComplexType.ElementName && isConceptual)
            {
                return EntityDesignerSelection.ConceptualModelComplexType;
            }
            else if (el.EFTypeName == FunctionImport.ElementName)
            {
                return EntityDesignerSelection.ConceptualModelFunctionImport;
            }
            else if (el.EFTypeName == Parameter.ElementName)
            {
                if (isConceptual)
                {
                    return EntityDesignerSelection.ConceptualModelFunctionImportParameter;
                }
                else
                {
                    return EntityDesignerSelection.StorageModelFunctionParameter;
                }
            }
            else if (el.EFTypeName == Function.ElementName)
            {
                return EntityDesignerSelection.StorageModelFunction;
            }
            else if (el.EFTypeName == EntityTypeShape.ElementName)
            {
                return EntityDesignerSelection.ConceptualModelEntityType;
            }

            return null;
        }

        #endregion
    }
}
