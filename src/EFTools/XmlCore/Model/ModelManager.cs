// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Eventing;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    /// <summary>
    ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
    /// </summary>
    public abstract class ModelManager : IDisposable
    {
        // TODO:  we should have a case-insensitive URI comparison here.  
        private readonly Dictionary<Uri, EFArtifact> _artifactsByUri = new Dictionary<Uri, EFArtifact>();

        // TODO:  we should have a case-insensitive URI comparison here.  
        private readonly Dictionary<EFArtifact, List<EFArtifactSet>> _artifact2ArtifactSets =
            new Dictionary<EFArtifact, List<EFArtifactSet>>();

        // views should listen to this event to be notified when changes are committed to the model
        internal EventHandler<EfiChangingEventArgs> BeforeModelChangesCommitted { get; set; }
        internal EventHandler<EfiChangedEventArgs> ModelChangesCommitted { get; set; }

        // records the list of change groups for a model transaction that might span xlinq transactions
        // note: use Queue so that if committing one changeGroup results in the creation of another there
        // is no exception when adding the second changeGroup to the list
        private readonly Queue<EfiChangeGroup> _changeGroups = new Queue<EfiChangeGroup>();

        // the type of EFArtifact to use
        private IEFArtifactFactory _artifactFactory;

        // the type of EFArtifactSet to use
        private IEFArtifactSetFactory _artifactSetFactory;

        internal ModelManager(IEFArtifactFactory artifactFactory, IEFArtifactSetFactory artifactSetFactory)
        {
            _artifactFactory = artifactFactory;
            _artifactSetFactory = artifactSetFactory;
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        ~ModelManager()
        {
            Debug.Fail("ModelManager was not disposed of properly!");
            Dispose(false);
        }

        internal ICollection<EFArtifact> Artifacts
        {
            get
            {
                lock (this)
                {
                    return _artifactsByUri.Values;
                }
            }
        }

        internal IEFArtifactFactory ArtifactFactory
        {
            get { return _artifactFactory; }
        }

        internal IEFArtifactSetFactory ArtifactSetFactory
        {
            get { return _artifactSetFactory; }
        }

        /// <summary>
        ///     Gets the AttributeContentValidator used for validating xml attribute values for this ModelManager.
        /// </summary>
        internal abstract AttributeContentValidator GetAttributeContentValidator(EFArtifact artifact);

        internal abstract XNamespace GetRootNamespace(EFObject node);

        internal abstract RenameCommand CreateRenameCommand(EFNormalizableItem element, string newName, bool uniquenessIsCaseSensitive);

        internal void RecordChangeGroup(EfiChangeGroup changeGroup)
        {
            if (changeGroup != null
                &&
                changeGroup.Count > 0)
            {
                _changeGroups.Enqueue(changeGroup);
            }
        }

        internal void BeforeCommitChangeGroups(CommandProcessorContext cpc)
        {
            var args = new EfiChangingEventArgs(cpc);
            if (BeforeModelChangesCommitted != null)
            {
                // now tell everyone that the changes are about to be committed
                BeforeModelChangesCommitted(this, args);
            }
        }

        internal void RouteChangeGroups()
        {
            try
            {
                while (_changeGroups.Count > 0)
                {
                    var changeGroup = _changeGroups.Dequeue();
                    var args = new EfiChangedEventArgs(changeGroup);
                    if (ModelChangesCommitted != null)
                    {
                        // now tell everyone that things have changed
                        ModelChangesCommitted(this, args);
                    }
                }
            }
            finally
            {
                ClearChangeGroups();
            }
        }

        internal void ClearChangeGroups()
        {
            _changeGroups.Clear();
        }

        [SuppressMessage("Microsoft.Performance", "CA1822:MarkMembersAsStatic")]
        internal IEnumerable<XName> GetUnprocessedElements()
        {
#if DEBUG
            var allXnames = new HashSet<XName>();
            foreach (var a in _artifactsByUri.Values)
            {
                foreach (var xname in a.UnprocessedElements)
                {
                    allXnames.Add(xname);
                }
            }
            return allXnames;
#else
            return new XName[0];
#endif
        }

        /// <summary>
        ///     Get the EFArtifact for a particular Uri or load it if it hasn't been loaded.
        /// </summary>
        /// <param name="uri"></param>
        /// <param name="provider">This should alway be NULL except for our unit tests</param>
        /// <remarks>virtual for testing</remarks>
        internal virtual EFArtifact GetNewOrExistingArtifact(Uri uri, XmlModelProvider xmlModelProvider)
        {
            lock (this)
            {
                var result = GetArtifact(uri);
                if (result == null)
                {
                    // Loading an artifact might cause related artifacts to be automatically loaded.
                    var artifacts = Load(uri, xmlModelProvider);
                    if (artifacts != null)
                    {
                        result = artifacts.Where(a => a.Uri == uri).FirstOrDefault();
                    }
                }
                return result;
            }
        }

        /// <summary>
        ///     Get the EFArtifactSet for a particular Uri from the cache or null if it hasn't been loaded.
        ///     Virtual to allow mocking.
        /// </summary>
        internal virtual EFArtifactSet GetArtifactSet(Uri uri)
        {
            lock (this)
            {
                EFArtifact artifact = null;
                _artifactsByUri.TryGetValue(uri, out artifact);
                if (artifact != null)
                {
                    List<EFArtifactSet> result = null;
                    if (_artifact2ArtifactSets.TryGetValue(artifact, out result))
                    {
                        Debug.Assert(result.Count == 1, "Support for an artifact spanning multiple sets is not yet implemented");
                        return result[0];
                    }
                }

                return null;
            }
        }

        /// <summary>
        ///     Get the EFArtifact for a particular Uri from the cache or null if it hasn't been loaded.
        ///     Virtual to allow mocking.
        /// </summary>
        internal virtual EFArtifact GetArtifact(Uri uri)
        {
            lock (this)
            {
                EFArtifact result = null;
                if (_artifactsByUri.TryGetValue(uri, out result))
                {
                    return result;
                }
                return null;
            }
        }

        internal void RenameArtifact(Uri oldUri, Uri newUri)
        {
            lock (this)
            {
                EFArtifact result = null;
                if (_artifactsByUri.TryGetValue(oldUri, out result))
                {
                    _artifactsByUri.Remove(oldUri);
                    _artifactsByUri.Add(newUri, result);
                    result.RenameArtifact(newUri);
                }
            }
        }

        /// <summary>
        ///     Remove the Artifact associated with the Uri from the cache.
        ///     Virtual for testing.
        /// </summary>
        internal virtual void ClearArtifact(Uri uri)
        {
            lock (this)
            {
                EFArtifact artifact = null;
                if (_artifactsByUri.TryGetValue(uri, out artifact))
                {
                    if (artifact != null)
                    {
                        var artifactSet = GetArtifactSet(uri);
                        Debug.Assert(artifactSet != null);

                        // need to dispose the artifact first, as it will try and access it's set
                        artifact.Dispose();

                        // clean up the model manager's collections
                        _artifact2ArtifactSets.Remove(artifact);
                        _artifactsByUri.Remove(uri);

                        // remove this artifact from the set
                        artifactSet.RemoveArtifact(artifact);
                    }
                }
            }
        }

        /// <summary>
        ///     Load an artifact with a give URI.
        ///     Depending on the artifactset mode, a new artifact set might be created for the newly created artifact(s).
        /// </summary>
        /// <param name="fileUri"></param>
        /// <param name="xmlModelProvider"></param>
        /// <returns></returns>
        private IList<EFArtifact> Load(Uri fileUri, XmlModelProvider xmlModelProvider)
        {
            lock (this)
            {
                List<EFArtifact> artifacts = null;
                EFArtifactSet artifactSet = null;
                try
                {
                    artifacts = _artifactFactory.Create(this, fileUri, xmlModelProvider) as List<EFArtifact>;
                    // Case where the artifact factory failed to instantiate artifact(s).
                    if (artifacts == null
                        && artifacts.Count <= 0)
                    {
                        Debug.Assert(false, "Could not create EFArtifact using current factory");
                        return null;
                    }
                        // Case where artifact factory failed to load the artifact with give URI.
                    else if (artifacts.Where(a => a.Uri == fileUri).FirstOrDefault() == null)
                    {
                        Debug.Assert(false, "Artifact Factory does not created an artifact with URI:" + fileUri.LocalPath);
                        return null;
                    }

                    EFArtifactSet efArtifactSet = null;
                    // Initialize each artifact in the list.
                    foreach (var artifact in artifacts)
                    {
                        Debug.Assert(
                            _artifact2ArtifactSets.ContainsKey(artifact) == false, "Unexpected entry for artifact in artifact2ArtifactSet");
                        if (_artifact2ArtifactSets.ContainsKey(artifact) == false)
                        {
                            if (efArtifactSet == null)
                            {
                                efArtifactSet = _artifactSetFactory.CreateArtifactSet(artifact);
                            }
                            RegisterArtifact(artifact, efArtifactSet);
                        }
                    }

                    artifactSet = GetArtifactSet(fileUri);
                    ParseArtifactSet(artifactSet);
                    NormalizeArtifactSet(artifactSet);
                    ResolveArtifactSet(artifactSet);

                    // Tell the artifacts that they are loaded and ready
                    artifacts.ForEach((a) => { a.OnLoaded(); });
                }
                catch (Exception)
                {
                    // an exception occurred during loading, dispose each artifact in the list and rethrow.
                    if (artifacts != null)
                    {
                        // call dispose & clear the artifact.  We need both since the the entry may not be 
                        // in the _artifactsByUri table. 
                        artifacts.ForEach(
                            artifact =>
                                {
                                    artifact.Dispose();
                                    ClearArtifact(artifact.Uri);
                                });
                    }
                    throw;
                }

                return artifacts;
            }
        }

        /// <summary>
        ///     Parses all of the loaded Entity and Mapping models in the given EFArtifactSet, creating EFElements for
        ///     every node in the XLinq tree.
        /// </summary>
        private void ParseArtifactSet(EFArtifactSet artifactSet)
        {
            lock (this)
            {
                foreach (var a in artifactSet.Artifacts)
                {
                    if (a.State == EFElementState.None)
                    {
                        HashSet<XName> s = null;
#if DEBUG
                        s = a.UnprocessedElements;
                        s.Clear();
#endif
                        a.Parse(s);
                    }
                }
            }
        }

        /// <summary>
        ///     Asks every EFElement in the given EFArtifactSet to create its normalized name and load that into
        ///     the global symbol table.
        /// </summary>
        private void NormalizeArtifactSet(EFArtifactSet artifactSet)
        {
            lock (this)
            {
                var visitor = new NormalizingVisitor();

                var lastMissedCount = -1;
                while (visitor.MissedCount != 0)
                {
                    visitor.ResetMissedCount();

                    foreach (var artifact in artifactSet.Artifacts)
                    {
                        visitor.Traverse(artifact);
                    }

                    // every item should be able to normalize
                    if (lastMissedCount == visitor.MissedCount)
                    {
                        throw new InvalidDataException("Subsequent passes didn't normalize any new items.");
                    }

                    lastMissedCount = visitor.MissedCount;
                }
            }
        }

        /// <summary>
        ///     Asks the passed in item all of its children to create its normalized name
        ///     and load that into the global symbol table.
        /// </summary>
        internal static void NormalizeItem(EFContainer item)
        {
            var visitor = new NormalizingVisitor();

            var lastMissedCount = -1;
            while (visitor.MissedCount != 0)
            {
                visitor.ResetMissedCount();
                visitor.Traverse(item);

                // every item should be able to normalize
                if (lastMissedCount == visitor.MissedCount)
                {
                    // subsequent passes didn't normalize any new items
                    throw new InvalidDataException();
                }

                lastMissedCount = visitor.MissedCount;
            }
        }

        /// <summary>
        ///     Asks every EFElement in the given EFArtifactSet to resolve references to other EFElements in the EFArtifactSet,
        ///     i.e., a ScalarProperty will link up to its entity and storage properties.
        /// </summary>
        private void ResolveArtifactSet(EFArtifactSet artifactSet)
        {
            lock (this)
            {
                var visitor = new ResolvingVisitor(artifactSet);

                var lastMissedCount = visitor.MissedCount;

                while (visitor.MissedCount != 0)
                {
                    visitor.ResetMissedCount();

                    foreach (var artifact in artifactSet.Artifacts)
                    {
                        visitor.Traverse(artifact);
                    }

                    // if we can't resolve any more then we are done
                    if (lastMissedCount == visitor.MissedCount)
                    {
                        break;
                    }

                    lastMissedCount = visitor.MissedCount;
                }
            }
        }

        /// <summary>
        ///     Asks the passed in item and all its children to resolve references to other
        ///     EFElements across the entire model, i.e., a ScalarProperty will link up to its
        ///     entity and storage properties.
        /// </summary>
        internal void ResolveItem(EFContainer item)
        {
            lock (this)
            {
                var visitor = new ResolvingVisitor(item.Artifact.ArtifactSet);

                var lastMissedCount = visitor.MissedCount;

                while (visitor.MissedCount != 0)
                {
                    visitor.ResetMissedCount();
                    visitor.Traverse(item);

                    // if we can't resolve any more then we are done
                    if (lastMissedCount == visitor.MissedCount)
                    {
                        break;
                    }

                    lastMissedCount = visitor.MissedCount;
                }
            }
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        public void Dispose()
        {
            Dispose(true);
            GC.SuppressFinalize(this);
        }

        /// <summary>
        ///     This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.
        /// </summary>
        /// <param name="disposing">This API supports the Entity Framework infrastructure and is not intended to be used directly from your code.</param>
        protected virtual void Dispose(bool disposing)
        {
            if (disposing)
            {
                lock (this)
                {
                    // An artifact might be depended on another artifact which means disposing an artifact might also automatically dispose its dependent artifacts.
                    foreach (var a in _artifactsByUri.Values.ToArray())
                    {
                        if (a != null
                            && a.IsDisposed == false)
                        {
                            a.Dispose();
                        }
                    }

                    _artifactsByUri.Clear();
                    _artifact2ArtifactSets.Clear();
                    ClearChangeGroups();

                    _artifactFactory = null;
                    _artifactSetFactory = null;
                }
            }
        }

        /// <summary>
        ///     This method will do the following.
        ///     - Add EFArtifact reference to the EFArtifactSet.
        ///     - Update artifactsByUri and artifact2ArtifactSet dictionaries.
        ///     - Initialize the EFArtifact.
        /// </summary>
        /// <param name="efArtifact"></param>
        /// <param name="efArtifactSet"></param>
        internal void RegisterArtifact(EFArtifact efArtifact, EFArtifactSet efArtifactSet)
        {
            Debug.Assert(
                _artifactsByUri.ContainsKey(efArtifact.Uri) == false && _artifact2ArtifactSets.ContainsKey(efArtifact) == false,
                "This artifact has been registered in model manager.");

            if (_artifactsByUri.ContainsKey(efArtifact.Uri) == false
                && _artifact2ArtifactSets.ContainsKey(efArtifact) == false)
            {
                if (efArtifactSet.Artifacts.Contains(efArtifact) == false)
                {
                    efArtifactSet.Add(efArtifact);
                }

                _artifactsByUri[efArtifact.Uri] = efArtifact;

                var artifactSetList = new List<EFArtifactSet>(1);
                artifactSetList.Add(efArtifactSet);
                _artifact2ArtifactSets[efArtifact] = artifactSetList;
                efArtifact.Init();
            }
        }
    }
}
