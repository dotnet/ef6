// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Concurrent;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;
    using System.Data.Entity.Core.Mapping;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using System.Threading;

    internal class MetadataCache
    {
        private const string DataDirectory = "|datadirectory|";
        private const string MetadataPathSeparator = "|";
        private const string SemicolonSeparator = ";";

        public static readonly MetadataCache Instance = new MetadataCache();

        private Memoizer<string, List<MetadataArtifactLoader>> _artifactLoaderCache
            = new Memoizer<string, List<MetadataArtifactLoader>>(SplitPaths, null);

        private readonly ConcurrentDictionary<string, MetadataWorkspace> _cachedWorkspaces
            = new ConcurrentDictionary<string, MetadataWorkspace>();

        // <summary>
        // A helper function for splitting up a string that is a concatenation of strings delimited by the metadata
        // path separator into a string list. The resulting list sorted SSDL, MSL, CSDL, if possible.
        // </summary>
        // <param name="paths"> The paths to split </param>
        // <returns> An array of strings </returns>
        private static List<MetadataArtifactLoader> SplitPaths(string paths)
        {
            DebugCheck.NotEmpty(paths);

            // This is the registry of all URIs in the global collection.
            var uriRegistry = new HashSet<string>(StringComparer.OrdinalIgnoreCase);

            // If the argument contains one or more occurrences of the macro '|DataDirectory|', we
            // pull those paths out so that we don't lose them in the string-splitting logic below.
            // Note that the macro '|DataDirectory|' cannot have any whitespace between the pipe 
            // symbols and the macro name. Also note that the macro must appear at the beginning of 
            // a path (else we will eventually fail with an invalid path exception, because in that
            // case the macro is not expanded). If a real/physical folder named 'DataDirectory' needs
            // to be included in the metadata path, whitespace should be used on either or both sides
            // of the name.
            //
            var dataDirPaths = new List<string>();

            var indexStart = paths.IndexOf(DataDirectory, StringComparison.OrdinalIgnoreCase);
            while (indexStart != -1)
            {
                var prevSeparatorIndex = indexStart == 0
                                             ? -1
                                             : paths.LastIndexOf(
                                                 MetadataPathSeparator,
                                                 indexStart - 1, // start looking here
                                                 StringComparison.Ordinal
                                                   );

                var macroPathBeginIndex = prevSeparatorIndex + 1;

                // The '|DataDirectory|' macro is composable, so identify the complete path, like
                // '|DataDirectory|\item1\item2'. If the macro appears anywhere other than at the
                // beginning, splice out the entire path, e.g. 'C:\item1\|DataDirectory|\item2'. In this
                // latter case the macro will not be expanded, and downstream code will throw an exception.
                //
                var indexEnd = paths.IndexOf(
                    MetadataPathSeparator,
                    indexStart + DataDirectory.Length,
                    StringComparison.Ordinal);
                if (indexEnd == -1)
                {
                    dataDirPaths.Add(paths.Substring(macroPathBeginIndex));
                    paths = paths.Remove(macroPathBeginIndex); // update the concatenated list of paths
                    break;
                }

                dataDirPaths.Add(paths.Substring(macroPathBeginIndex, indexEnd - macroPathBeginIndex));

                // Update the concatenated list of paths by removing the one containing the macro.
                //
                paths = paths.Remove(macroPathBeginIndex, indexEnd - macroPathBeginIndex);
                indexStart = paths.IndexOf(DataDirectory, StringComparison.OrdinalIgnoreCase);
            }

            // Split the string on the separator and remove all spaces around each parameter value
            var results = paths.Split(new[] { MetadataPathSeparator }, StringSplitOptions.RemoveEmptyEntries);

            // Now that the non-macro paths have been identified, merge the paths containing the macro
            // into the complete list.
            //
            if (dataDirPaths.Count > 0)
            {
                dataDirPaths.AddRange(results);
                results = dataDirPaths.ToArray();
            }

            var csdlLoaders = new List<MetadataArtifactLoader>();
            var mslLoaders = new List<MetadataArtifactLoader>();
            var ssdlLoaders = new List<MetadataArtifactLoader>();
            var loaders = new List<MetadataArtifactLoader>();

            for (var i = 0; i < results.Length; i++)
            {
                // Trim out all the spaces for this parameter and add it only if it's not blank
                results[i] = results[i].Trim();
                if (results[i].Length > 0)
                {
                    var loader = MetadataArtifactLoader.Create(
                        results[i],
                        MetadataArtifactLoader.ExtensionCheck.All, // validate the extension against all acceptable values
                        null,
                        uriRegistry);

                    if (results[i].EndsWith(XmlConstants.CSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        csdlLoaders.Add(loader);
                    }
                    else if (results[i].EndsWith(XmlConstants.CSSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        mslLoaders.Add(loader);
                    }
                    else if (results[i].EndsWith(XmlConstants.SSpaceSchemaExtension, StringComparison.OrdinalIgnoreCase))
                    {
                        ssdlLoaders.Add(loader);
                    }
                    else
                    {
                        loaders.Add(loader);
                    }
                }
            }

            loaders.AddRange(ssdlLoaders);
            loaders.AddRange(mslLoaders);
            loaders.AddRange(csdlLoaders);

            return loaders;
        }

        public MetadataWorkspace GetMetadataWorkspace(DbConnectionOptions effectiveConnectionOptions)
        {
            DebugCheck.NotNull(effectiveConnectionOptions);

            var artifactLoader = GetArtifactLoader(effectiveConnectionOptions);

            var cacheKey = CreateMetadataCacheKey(
                artifactLoader.GetPaths(),
                effectiveConnectionOptions[EntityConnectionStringBuilder.ProviderParameterName]);

            return GetMetadataWorkspace(cacheKey, artifactLoader);
        }

        public MetadataArtifactLoader GetArtifactLoader(DbConnectionOptions effectiveConnectionOptions)
        {
            DebugCheck.NotNull(effectiveConnectionOptions);

            var paths = effectiveConnectionOptions[EntityConnectionStringBuilder.MetadataParameterName];

            if (!string.IsNullOrEmpty(paths))
            {
                var loaders = _artifactLoaderCache.Evaluate(paths);

                return MetadataArtifactLoader.Create(
                    ShouldRecalculateMetadataArtifactLoader(loaders)
                        ? SplitPaths(paths)
                        : loaders);
            }

            return MetadataArtifactLoader.Create(new List<MetadataArtifactLoader>());
        }

        public MetadataWorkspace GetMetadataWorkspace(string cacheKey, MetadataArtifactLoader artifactLoader)
        {
            DebugCheck.NotEmpty(cacheKey);
            DebugCheck.NotNull(artifactLoader);

            return _cachedWorkspaces.GetOrAdd(
                cacheKey,
                k =>
                    {
                        var edmItemCollection = LoadEdmItemCollection(artifactLoader);

                        var mappingLoader = new Lazy<StorageMappingItemCollection>(
                            () => LoadStoreCollection(edmItemCollection, artifactLoader));

                        return new MetadataWorkspace(
                            () => edmItemCollection,
                            () => mappingLoader.Value.StoreItemCollection,
                            () => mappingLoader.Value);
                    });
        }

        public void Clear()
        {
            _cachedWorkspaces.Clear();

            Interlocked.CompareExchange(
                ref _artifactLoaderCache,
                new Memoizer<string, List<MetadataArtifactLoader>>(SplitPaths, null),
                _artifactLoaderCache);
        }

        private static StorageMappingItemCollection LoadStoreCollection(EdmItemCollection edmItemCollection, MetadataArtifactLoader loader)
        {
            StoreItemCollection storeItemCollection;
            var sSpaceXmlReaders = loader.CreateReaders(DataSpace.SSpace);
            try
            {
                storeItemCollection = new StoreItemCollection(
                    sSpaceXmlReaders,
                    loader.GetPaths(DataSpace.SSpace));
            }
            finally
            {
                Helper.DisposeXmlReaders(sSpaceXmlReaders);
            }

            var csSpaceXmlReaders = loader.CreateReaders(DataSpace.CSSpace);
            try
            {
                return new StorageMappingItemCollection(
                    edmItemCollection,
                    storeItemCollection,
                    csSpaceXmlReaders,
                    loader.GetPaths(DataSpace.CSSpace));
            }
            finally
            {
                Helper.DisposeXmlReaders(csSpaceXmlReaders);
            }
        }

        private static EdmItemCollection LoadEdmItemCollection(MetadataArtifactLoader loader)
        {
            DebugCheck.NotNull(loader);

            var readers = loader.CreateReaders(DataSpace.CSpace);
            try
            {
                return new EdmItemCollection(readers, loader.GetPaths(DataSpace.CSpace));
            }
            finally
            {
                Helper.DisposeXmlReaders(readers);
            }
        }

        private static bool ShouldRecalculateMetadataArtifactLoader(IEnumerable<MetadataArtifactLoader> loaders)
        {
            return loaders.Any(loader => loader.GetType() == typeof(MetadataArtifactLoaderCompositeFile));
        }

        private static string CreateMetadataCacheKey(IList<string> paths, string providerName)
        {
            var resultCount = 0;
            string result;

            // Do a first pass to calculate the output size of the metadata cache key,
            // then another pass to populate a StringBuilder with the exact size and
            // get the result.
            CreateMetadataCacheKeyWithCount(
                paths, providerName,
                false, ref resultCount, out result);
            CreateMetadataCacheKeyWithCount(
                paths, providerName,
                true, ref resultCount, out result);

            return result;
        }

        private static void CreateMetadataCacheKeyWithCount(
            IList<string> paths,
            string providerName,
            bool buildResult, ref int resultCount, out string result)
        {
            // Build a string as the key and look up the MetadataCache for a match
            var keyString = buildResult ? new StringBuilder(resultCount) : null;

            // At this point, we've already used resultCount. Reset it
            // to zero to make the final debug assertion that our computation
            // is correct.
            resultCount = 0;

            if (!string.IsNullOrEmpty(providerName))
            {
                resultCount += providerName.Length + 1;
                if (buildResult)
                {
                    keyString.Append(providerName);
                    keyString.Append(SemicolonSeparator);
                }
            }

            if (paths != null)
            {
                for (var i = 0; i < paths.Count; i++)
                {
                    if (paths[i].Length > 0)
                    {
                        if (i > 0)
                        {
                            resultCount++;
                            if (buildResult)
                            {
                                keyString.Append(MetadataPathSeparator);
                            }
                        }

                        resultCount += paths[i].Length;
                        if (buildResult)
                        {
                            keyString.Append(paths[i]);
                        }
                    }
                }

                resultCount++;
                if (buildResult)
                {
                    keyString.Append(SemicolonSeparator);
                }
            }

            result = buildResult ? keyString.ToString() : null;

            Debug.Assert(!buildResult || (result.Length == resultCount));
        }
    }
}
