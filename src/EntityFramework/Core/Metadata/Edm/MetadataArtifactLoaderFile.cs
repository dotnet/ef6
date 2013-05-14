// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.SchemaObjectModel;
    using System.Diagnostics;
    using System.Runtime.Versioning;
    using System.Xml;

    /// <summary>
    ///     This class represents one file-based artifact item to be loaded.
    /// </summary>
    internal class MetadataArtifactLoaderFile : MetadataArtifactLoader, IComparable
    {
        /// <summary>
        ///     This member indicates whether the file-based artifact has already been loaded.
        ///     It is used to prevent other instances of this class from (re)loading the same
        ///     artifact. See comment in the MetadataArtifactLoaderFile c'tor below.
        /// </summary>
        private readonly bool _alreadyLoaded;

        private readonly string _path;

        /// <summary>
        ///     Constructor
        /// </summary>
        /// <param name="path"> The path to the resource to load </param>
        /// <param name="uriRegistry"> The global registry of URIs </param>
        [ResourceExposure(ResourceScope.Machine)] //Exposes the file path which is a Machine resource
        public MetadataArtifactLoaderFile(string path, ICollection<string> uriRegistry)
        {
            _path = path;
            _alreadyLoaded = uriRegistry.Contains(_path);
            if (!_alreadyLoaded)
            {
                uriRegistry.Add(_path);

                // '_alreadyLoaded' is not set because while we would like to prevent
                // other instances of MetadataArtifactLoaderFile that wrap the same
                // _path from being added to the list of paths/readers, we do want to
                // include this particular instance.
            }
        }

        public override string Path
        {
            get { return _path; }
        }

        /// <summary>
        ///     Implementation of IComparable.CompareTo()
        /// </summary>
        /// <param name="obj"> The object to compare to </param>
        /// <returns> 0 if the loaders are "equal" (i.e., have the same _path value) </returns>
        public int CompareTo(object obj)
        {
            var loader = obj as MetadataArtifactLoaderFile;
            if (loader != null)
            {
                return string.Compare(_path, loader._path, StringComparison.OrdinalIgnoreCase);
            }

            Debug.Assert(false, "object is not a MetadataArtifactLoaderFile");
            return -1;
        }

        /// <summary>
        ///     Equals() returns true if the objects have the same _path value
        /// </summary>
        /// <param name="obj"> The object to compare to </param>
        /// <returns> true if the objects have the same _path value </returns>
        public override bool Equals(object obj)
        {
            return CompareTo(obj) == 0;
        }

        /// <summary>
        ///     GetHashCode override that defers the result to the _path member variable.
        /// </summary>
        public override int GetHashCode()
        {
            return _path.GetHashCode();
        }

        /// <summary>
        ///     Get paths to artifacts for a specific DataSpace.
        /// </summary>
        /// <param name="spaceToGet"> The DataSpace for the artifacts of interest </param>
        /// <returns> A List of strings identifying paths to all artifacts for a specific DataSpace </returns>
        public override List<string> GetPaths(DataSpace spaceToGet)
        {
            var list = new List<string>();
            if (!_alreadyLoaded
                && IsArtifactOfDataSpace(_path, spaceToGet))
            {
                list.Add(_path);
            }
            return list;
        }

        /// <summary>
        ///     Get paths to all artifacts
        /// </summary>
        /// <returns> A List of strings identifying paths to all resources </returns>
        public override List<string> GetPaths()
        {
            var list = new List<string>();
            if (!_alreadyLoaded)
            {
                list.Add(_path);
            }
            return list;
        }

        /// <summary>
        ///     Create and return an XmlReader around the file represented by this instance.
        /// </summary>
        /// <returns> A List of XmlReaders for all resources </returns>
        public override List<XmlReader> GetReaders(Dictionary<MetadataArtifactLoader, XmlReader> sourceDictionary)
        {
            var list = new List<XmlReader>();
            if (!_alreadyLoaded)
            {
                var reader = CreateXmlReader();
                list.Add(reader);
                if (sourceDictionary != null)
                {
                    sourceDictionary.Add(this, reader);
                }
            }
            return list;
        }

        /// <summary>
        ///     Create and return an XmlReader around the file represented by this instance
        ///     if it is of the requested DataSpace type.
        /// </summary>
        /// <param name="spaceToGet"> The DataSpace corresponding to the requested artifacts </param>
        /// <returns> A List of XmlReader objects </returns>
        public override List<XmlReader> CreateReaders(DataSpace spaceToGet)
        {
            var list = new List<XmlReader>();
            if (!_alreadyLoaded
                && IsArtifactOfDataSpace(_path, spaceToGet))
            {
                var reader = CreateXmlReader();
                list.Add(reader);
            }
            return list;
        }

        /// <summary>
        ///     Create an XmlReader around the artifact file
        /// </summary>
        /// <returns> An XmlReader that wraps a file </returns>
        [ResourceExposure(ResourceScope.None)] //The file path is not passed through to this method so nothing to expose in this method.
        [ResourceConsumption(ResourceScope.Machine, ResourceScope.Machine)] //We are not changing the scope of consumption here
        private XmlReader CreateXmlReader()
        {
            var readerSettings = Schema.CreateEdmStandardXmlReaderSettings();
            // we know that we aren't reading a fragment
            readerSettings.ConformanceLevel = ConformanceLevel.Document;
            return XmlReader.Create(_path, readerSettings);
        }
    }
}
