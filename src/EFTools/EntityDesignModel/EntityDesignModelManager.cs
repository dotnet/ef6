// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    /// <summary>
    ///     Class that contains ado.net metadata and information about the metadata source representation, in this case xml source.
    /// </summary>
    internal class EntityDesignModelManager : ModelManager
    {
        internal EntityDesignModelManager(IEFArtifactFactory artifactFactory, IEFArtifactSetFactory artifactSetFactory)
            : base(artifactFactory, artifactSetFactory)
        {
        }

        internal override AttributeContentValidator GetAttributeContentValidator(EFArtifact artifact)
        {
            return EscherAttributeContentValidator.GetInstance(artifact.SchemaVersion);
        }

        /// <summary>
        ///     Retrieve the root "Schema" node for current node.  If there is no root mapping node, this will return null.
        /// </summary>
        internal override XNamespace GetRootNamespace(EFObject node)
        {
            var currNode = node;
            EFRuntimeModelRoot runtimeModel = null;
            EFDesignerInfoRoot designerInfo = null;
            XNamespace ns = null;

            while (currNode != null)
            {
                runtimeModel = currNode as EFRuntimeModelRoot;
                designerInfo = currNode as EFDesignerInfoRoot;

                if (runtimeModel != null)
                {
                    ns = runtimeModel.XNamespace;
                    break;
                }
                else if (designerInfo != null)
                {
                    ns = designerInfo.XNamespace;
                    break;
                }
                else
                {
                    currNode = currNode.Parent;
                }
            }

            return ns;
        }

        internal override RenameCommand CreateRenameCommand(EFNormalizableItem element, string newName, bool uniquenessIsCaseSensitive)
        {
            return new EntityDesignRenameCommand(element, newName, uniquenessIsCaseSensitive);
        }

        internal void ValidateAndCompileMappings(EntityDesignArtifactSet artifactSet, bool doEscherValidation)
        {
            var artifact = artifactSet.GetEntityDesignArtifact();

            // XSD prevents an EDMX from having both a DataServices node and a Runtime node
            // don't validate DataServices
            if (artifact != null
                && artifact.DataServicesNodePresent)
            {
                return;
            }

            if (doEscherValidation)
            {
                EscherModelValidator.ValidateEscherModel(artifactSet, false);
            }
            else
            {
                EscherModelValidator.ClearErrors(artifactSet);
                if (artifact != null)
                {
                    artifact.SetValidityDirtyForErrorClass(ErrorClass.Escher_All, true);
                }
            }

            // validate using the schema version of this artifact as the target Entity Framework version
            new RuntimeMetadataValidator(this, artifactSet.SchemaVersion, DependencyResolver.Instance)
                .ValidateAndCompileMappings(artifactSet, artifactSet.ShouldDoRuntimeMappingValidation());
        }
    }
}
