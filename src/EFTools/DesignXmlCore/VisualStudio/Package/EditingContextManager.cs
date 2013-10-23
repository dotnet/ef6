// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Package
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Base.Context;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio.Shell;
    using Microsoft.VisualStudio.Shell.Design.Serialization;

    /// <summary>
    ///     The ArtifactManager keeps track of all EDM artifact files, a list
    ///     of frames that are loaded with artifacts file documents,
    ///     and the association of frames to a particular artifact
    /// </summary>
    internal class EditingContextManager : IDisposable
    {
        // we need this hash table; we could do reverse-lookups on the EFArtifact for the EditingContext that holds it, but we don't want to have
        // any references to the designer inside the artifact. 
        private Dictionary<EFArtifact, EditingContext> _mapArtifactToEditingContext = new Dictionary<EFArtifact, EditingContext>();
        private Dictionary<FrameWrapper, EditingContext> _mapFrameToUri = new Dictionary<FrameWrapper, EditingContext>();
        private readonly IXmlDesignerPackage _package;

        internal EditingContextManager(IXmlDesignerPackage package)
        {
            _package = package;
        }

        public void Dispose()
        {
            lock (this)
            {
                if (_mapArtifactToEditingContext != null)
                {
                    _mapArtifactToEditingContext.Clear();
                    _mapArtifactToEditingContext = null;
                }

                if (_mapFrameToUri != null)
                {
                    _mapFrameToUri.Clear();
                    _mapFrameToUri = null;
                }
            }
        }

        internal static EFArtifact GetArtifact(EditingContext context)
        {
            if (context != null)
            {
                var service = context.GetEFArtifactService();
                if (service != null)
                {
                    return service.Artifact;
                }
            }
            return null;
        }

        internal static Uri GetArtifactUri(EditingContext context)
        {
            var item = GetArtifact(context);
            if (item != null)
            {
                return item.Uri;
            }
            return null;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal bool DoesContextExist(Uri itemUri)
        {
            var artifact = _package.ModelManager.GetNewOrExistingArtifact(itemUri, new VSXmlModelProvider(_package, _package));
            if (artifact != null)
            {
                return _mapArtifactToEditingContext.ContainsKey(artifact);
            }

            return false;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        internal EditingContext GetNewOrExistingContext(Uri itemUri)
        {
            EditingContext itemContext = null;

            // creating a new context is an expensive operation, so optimize for the case where it exists
            var item = _package.ModelManager.GetArtifact(itemUri);
            if (item != null)
            {
                _mapArtifactToEditingContext.TryGetValue(item, out itemContext);
            }

            // there isn't one, so call the path that will create it
            if (itemContext == null)
            {
                item = _package.ModelManager.GetNewOrExistingArtifact(itemUri, new VSXmlModelProvider(_package, _package));
                if (itemUri != null
                    && item != null
                    && !_mapArtifactToEditingContext.TryGetValue(item, out itemContext))
                {
                    var service = new EFArtifactService(item);

                    var editingContext = new EditingContext();
                    editingContext.SetEFArtifactService(service);
                    itemContext = editingContext;
                    _mapArtifactToEditingContext[item] = itemContext;
                }
            }

            return itemContext;
        }

        internal void OnCloseFrame(FrameWrapper closingFrame)
        {
            if (_mapFrameToUri.ContainsKey(closingFrame))
            {
                _mapFrameToUri.Remove(closingFrame);

                if (null != closingFrame.Uri)
                {
                    var rdt = new RunningDocumentTable(_package);
                    var doc = rdt.FindDocument(closingFrame.Uri.LocalPath);
                    if (doc != null)
                    {
                        var isModified = false;
                        using (var docData = new DocData(doc))
                        {
                            isModified = docData.Modified;
                        }
                        if (isModified)
                        {
                            // document was modified but was closed without saving changes;
                            // we need to refresh all sets that refer to the document
                            // so that they revert to the document that is persisted in the file system

                            // TODO: add this functinality
                            //ModelManager.RefreshModelForLocation(closingFrame.Uri);
                        }
                    }
                }
            }
        }

        internal void CloseArtifact(Uri artifactUri)
        {
            if (artifactUri != null)
            {
                CloseArtifacts(new[] { artifactUri });
            }
        }

        internal void CloseArtifacts(IEnumerable<Uri> artifactsToClose)
        {
            var artifactsToDispose = new List<EFArtifact>();
            var editingContextsToDispose = new List<EditingContext>();

            foreach (var artifactUri in artifactsToClose)
            {
                EditingContext artifactContext = null;
                var artifact = _package.ModelManager.GetArtifact(artifactUri);
                if (artifact != null
                    && _mapArtifactToEditingContext.TryGetValue(artifact, out artifactContext))
                {
                    artifactsToDispose.Add(artifact);

                    _mapArtifactToEditingContext.Remove(artifact);
                    editingContextsToDispose.Add(artifactContext);
                }
            }

            foreach (var artifact in artifactsToDispose)
            {
                _package.ModelManager.ClearArtifact(artifact.Uri);
            }

            foreach (var editingContext in editingContextsToDispose)
            {
                editingContext.Dispose();
            }
        }

        private IEnumerable<EditingContext> GetOpenContexts()
        {
            return new List<EditingContext>(_mapArtifactToEditingContext.Values);
        }

        internal Collection<Uri> GetAssociatedUris(FrameWrapper frame)
        {
            if (frame.IsDesignerDocInDesigner)
            {
                return new Collection<Uri>(new[] { frame.Uri });
            }
            if (frame.IsDesignerDocInXmlEditor)
            {
                return GetAssociatedUris(frame.Uri);
            }
            return null;
        }

        private Collection<Uri> GetAssociatedUris(Uri itemDocUri)
        {
            var associated = new Collection<Uri>();
            associated.Add(itemDocUri);
            foreach (var editingContext in GetOpenContexts())
            {
                var item = GetArtifact(editingContext);
                if (item != null)
                {
                    var itemUri = item.Uri;
                    if (!UriComparer.OrdinalIgnoreCase.Equals(itemUri, itemDocUri))
                    {
                        associated.Add(itemUri);
                    }
                }
            }

            return associated;
        }

        internal Uri GetCurrentUri(FrameWrapper frame)
        {
            EditingContext context = null;
            if (!_mapFrameToUri.TryGetValue(frame, out context))
            {
                if (context == null)
                {
                    SetCurrentUri(frame, frame.Uri);
                    return frame.Uri;
                }
            }

            var artifactService = context.GetEFArtifactService();
            Debug.Assert(
                artifactService != null && artifactService.Artifact != null,
                "There is no artifact service/artifact tied to this editing context!");
            if (artifactService != null
                && artifactService.Artifact != null)
            {
                return artifactService.Artifact.Uri;
            }
            return null;
        }

        internal void SetCurrentUri(FrameWrapper frame, Uri itemUri)
        {
            var context = GetNewOrExistingContext(itemUri);
            _mapFrameToUri[frame] = context;
        }
    }
}
