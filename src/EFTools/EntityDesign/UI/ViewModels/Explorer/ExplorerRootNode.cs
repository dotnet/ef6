// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.Explorer
{
    using System;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;

    /// <summary>
    ///     dummy node - has no equivalent in the Model project
    ///     used for the root node of the tree in the tree view
    /// </summary>
    internal class ExplorerRootNode : EntityDesignExplorerEFElement
    {
        private ExplorerConceptualEntityModel _conceptualModel;
        private ExplorerStorageEntityModel _storageModel;

        private readonly Uri _uri;

        // Note: this is a root node - it cannot have a parent
        public ExplorerRootNode(EditingContext context, EFElement modelItem, Uri uri)
            : base(context, modelItem, null)
        {
            _uri = uri;
        }

        #region Properties

        public override string Name
        {
            get
            {
                // if the user includes a space in the file name we don't want to show %20 instead of the space, etc.
                return Uri.UnescapeDataString(FindFileNameFromUri(_uri));
            }
        }

        public ExplorerConceptualEntityModel ConceptualModel
        {
            get { return _conceptualModel; }
            set { _conceptualModel = value; }
        }

        public ExplorerStorageEntityModel StorageModel
        {
            get { return _storageModel; }
            set { _storageModel = value; }
        }

        public ExplorerDiagrams Diagrams { get; set; }

        #endregion

        protected override void LoadChildrenFromModel()
        {
        }

        protected override void LoadWpfChildrenCollection()
        {
            _children.Clear();

            if (Diagrams != null)
            {
                _children.Add(Diagrams);
            }

            if (_conceptualModel != null)
            {
                _children.Add(_conceptualModel);
            }

            if (_storageModel != null)
            {
                _children.Add(_storageModel);
            }
        }

        private static string FindFileNameFromUri(Uri fileUri)
        {
            var filePath = fileUri.GetComponents(UriComponents.Path, UriFormat.UriEscaped);
            var lastSlash = filePath.LastIndexOf("/", StringComparison.Ordinal);
            string fileName;
            if (lastSlash == -1)
            {
                fileName = filePath;
            }
            else
            {
                fileName = filePath.Substring(lastSlash + 1);
            }

            return fileName;
        }

        internal override string ExplorerImageResourceKeyName
        {
            get { return "EdmxPngIcon"; }
        }
    }
}
