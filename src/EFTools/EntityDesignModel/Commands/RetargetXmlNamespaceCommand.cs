// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Xml;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class RetargetXmlNamespaceCommand : Command
    {
        private readonly Version _targetSchemaVersion;
        private readonly EntityDesignArtifact _artifact;
        private readonly bool _reparseArtifact;

        // This command will replace root xml document of the edmx which requires reloading artifact.
        // But we need to commit the transaction before we can reload the artifact. The reason is that
        // XLinq representation of the parsed xml tree are not regenerated until the transaction is committed.
        // This command should not be combined with other commands unless it is executed as the last command in a
        // transaction since XmlEditor will not be aware of changes made to the Xml after we replace the root element
        internal RetargetXmlNamespaceCommand(EntityDesignArtifact artifact, Version targetSchemaVersion)
            : this(artifact, targetSchemaVersion, true)
        {
        }

        private RetargetXmlNamespaceCommand(EntityDesignArtifact artifact, Version targetSchemaVersion, bool reparseArtifact)
        {
            _targetSchemaVersion = targetSchemaVersion;
            _artifact = artifact;
            _reparseArtifact = reparseArtifact;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            RetargetWithMetadataConverter(_artifact.XDocument, _targetSchemaVersion, MetadataConverterDriver.Instance);
            if (_reparseArtifact)
            {
                cpc.Artifact.Parse(new HashSet<XName>());
                XmlModelHelper.NormalizeAndResolve(cpc.Artifact);
            }
        }

        /// <remarks>Internal for testing.</remarks>
        internal static void RetargetWithMetadataConverter(XDocument xdoc, Version targetSchemaVersion, MetadataConverterDriver converter)
        {
            Debug.Assert(xdoc != null, "xdoc != null");
            Debug.Assert(EntityFrameworkVersion.IsValidVersion(targetSchemaVersion), "invalid target schema version");

            var inputXml = new XmlDocument { PreserveWhitespace = true };
            using (var reader = xdoc.CreateReader())
            {
                inputXml.Load(reader);
            }

            var outputXml = converter.Convert(inputXml, targetSchemaVersion);
            if (outputXml != null)
            {
                // Dev10 Bug 550594: There is a bug in XmlEditor that prevents from deleting the root node
                // unless the root node has previous sibling (like a comment or Xml declaration).
                if (xdoc.Root.PreviousNode == null)
                {
                    xdoc.Root.AddBeforeSelf(new XComment(""));
                }

                // update xml document with new root element
                xdoc.Root.Remove();
                using (var reader = new XmlNodeReader(outputXml))
                {
                    var newDoc = XDocument.Load(reader);
                    xdoc.Add(newDoc.Root);
                }

                // Do not reload artifact here
                // Until the transaction is committed, the XLinq representation of the parsed xml tree hasn't been generated yet.
            }
        }

        /// <summary>
        ///     Retarget Artifact Xml Namespace and reload.
        /// </summary>
        internal static void RetargetArtifactXmlNamespaces(
            CommandProcessorContext cpc, EntityDesignArtifact artifact, Version targetSchemaVersion)
        {
            // no need to re-parse - ReloadArtifact will do this 
            CommandProcessor.InvokeSingleCommand(
                cpc, new RetargetXmlNamespaceCommand(artifact, targetSchemaVersion, reparseArtifact: false));
            // We need to ensure the command is committed before we reload the artifact.
            artifact.ReloadArtifact();
            artifact.IsDirty = true;
        }
    }
}
