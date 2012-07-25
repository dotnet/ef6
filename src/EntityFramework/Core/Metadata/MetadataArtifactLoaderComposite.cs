// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Xml;

    /// <summary>
    /// This class represents a super-collection (a collection of collections) 
    /// of artifact resources. Typically, this "meta-collection" would contain
    /// artifacts represented as individual files, directories (which are in
    /// turn collections of files), and embedded resources.
    /// </summary>
    /// <remarks>This is the root class for access to all loader objects.</remarks>
    internal class MetadataArtifactLoaderComposite : MetadataArtifactLoader, IEnumerable<MetadataArtifactLoader>
    {
        /// <summary>
        /// The list of loaders aggregated by the composite.
        /// </summary>
        private readonly ReadOnlyCollection<MetadataArtifactLoader> _children;

        /// <summary>
        /// Constructor - loads all resources into the _children collection
        /// </summary>
        /// <param name="children">A list of collections to aggregate</param>
        public MetadataArtifactLoaderComposite(List<MetadataArtifactLoader> children)
        {
            Debug.Assert(children != null);
            _children = new List<MetadataArtifactLoader>(children).AsReadOnly();
        }

        public override string Path
        {
            get { return string.Empty; }
        }

        public override void CollectFilePermissionPaths(List<string> paths, DataSpace spaceToGet)
        {
            foreach (var loader in _children)
            {
                loader.CollectFilePermissionPaths(paths, spaceToGet);
            }
        }

        public override bool IsComposite
        {
            get { return true; }
        }

        /// <summary>
        /// Get the list of paths to all artifacts in the original, unexpanded form
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public override List<string> GetOriginalPaths()
        {
            var list = new List<string>();

            foreach (var loader in _children)
            {
                list.AddRange(loader.GetOriginalPaths());
            }

            return list;
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace, in the original, unexpanded 
        /// form
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetOriginalPaths(DataSpace spaceToGet)
        {
            var list = new List<string>();

            foreach (var loader in _children)
            {
                list.AddRange(loader.GetOriginalPaths(spaceToGet));
            }

            return list;
        }

        /// <summary>
        /// Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace for the artifacts of interest</param>
        /// <returns>A List of strings identifying paths to all artifacts for a specific DataSpace</returns>
        public override List<string> GetPaths(DataSpace spaceToGet)
        {
            var list = new List<string>();

            foreach (var loader in _children)
            {
                list.AddRange(loader.GetPaths(spaceToGet));
            }

            return list;
        }

        /// <summary>
        /// Get paths to all artifacts
        /// </summary>
        /// <returns>A List of strings identifying paths to all resources</returns>
        public override List<string> GetPaths()
        {
            var list = new List<string>();

            foreach (var resource in _children)
            {
                list.AddRange(resource.GetPaths());
            }

            return list;
        }

        /// <summary>
        /// Aggregates all resource streams from the _children collection
        /// </summary>
        /// <returns>A List of XmlReader objects; cannot be null</returns>
        public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
        {
            var list = new List<XmlReader>();

            foreach (var resource in _children)
            {
                list.AddRange(resource.GetReaders(sourceDictionary));
            }

            return list;
        }

        /// <summary>
        /// Get XmlReaders for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet">The DataSpace corresponding to the requested artifacts</param>
        /// <returns>A List of XmlReader objects</returns>
        public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
        {
            var list = new List<XmlReader>();

            foreach (var resource in _children)
            {
                list.AddRange(resource.CreateReaders(spaceToGet));
            }

            return list;
        }

        #region IEnumerable<MetadataArtifactLoader> Members

        public IEnumerator<MetadataArtifactLoader> GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        #endregion

        #region IEnumerable Members

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _children.GetEnumerator();
        }

        #endregion
    }
}
