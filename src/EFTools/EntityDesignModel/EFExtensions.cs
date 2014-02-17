// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;
    using System.IO;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal static class EFExtensions
    {
        internal static StorageEntityModel StorageModel(this EFArtifact thisArtifact)
        {
            var artifact = GetEntityDesignArtifact(thisArtifact);
            if (artifact != null)
            {
                return artifact.StorageModel;
            }
            return null;
        }

        internal static ConceptualEntityModel ConceptualModel(this EFArtifact thisArtifact)
        {
            var artifact = GetEntityDesignArtifact(thisArtifact);
            if (artifact != null)
            {
                return artifact.ConceptualModel;
            }
            return null;
        }

        internal static MappingModel MappingModel(this EFArtifact thisArtifact)
        {
            var artifact = GetEntityDesignArtifact(thisArtifact);
            if (artifact != null)
            {
                return artifact.MappingModel;
            }
            return null;
        }

        internal static EFDesignerInfoRoot DesignerInfo(this EFArtifact thisArtifact)
        {
            var artifact = GetEntityDesignArtifact(thisArtifact);
            if (artifact != null)
            {
                return artifact.DesignerInfo;
            }
            return null;
        }

        internal static bool IsSqlFamilyProvider(this EFArtifact thisArtifact)
        {
            Debug.Assert(thisArtifact != null, "thisArtifact != null");

            // This is needed to workaround problems with facet propagation feature.
            // For Sql Server and Sql Server CE facets on properties in S-Space are by default 
            // the same as facets on corresponding properties in C-Space and therefore it is possible 
            // to blindly (i.e. wihtout asking the provider) propagate facets from S-Space properties 
            // to C-Space properties. Providers for other databases may use facets to distinguish among 
            // different types in which case the mismatch between facets on C-Space properties and S-Space 
            // properties is intentional and facet propagation breaks this. In general we should always ask
            // the provider about the type before we propagate facet values. This is a major change though
            // so for now we will limit the default facet propagation to SqlServer and Sql Server CE only.
            var storageModel = thisArtifact.StorageModel();
            if (storageModel != null
                && storageModel.Provider != null
                && storageModel.Provider.Value != null)
            {
                return storageModel.Provider.Value.Equals("System.Data.SqlClient", StringComparison.Ordinal) ||
                       storageModel.Provider.Value.StartsWith("System.Data.SqlServerCe", StringComparison.Ordinal);
            }

            return false;
        }

        internal static EntityDesignArtifact GetEntityDesignArtifact(this EFArtifactSet artifactSet)
        {
            // For now, we only expect only 1 instance of EntityDesignArtifact exists in the ArtifactSet (with the exception of our unit-test).
            // We need to change this once we support multiple entity-design-artifact per set.
            Debug.Assert(
                artifactSet.Artifacts.OfType<EntityDesignArtifact>().Count() == 1,
                "Expect to have 1 instance of EntityDesignArtifactSet in the artifactSet's Artifacts collection. Found: "
                + artifactSet.Artifacts.OfType<EntityDesignArtifact>().Count());

            return artifactSet.Artifacts.OfType<EntityDesignArtifact>().FirstOrDefault();
        }

        [SuppressMessage("Microsoft.Performance", "CA1800:DoNotCastUnnecessarily")]
        private static EntityDesignArtifact GetEntityDesignArtifact(EFArtifact baseArtifact)
        {
            // Assumption: DiagramArtifact's ArtifactSet will only contains 2 artifacts: DiagramArtifact and EntityDesignArtifact.
            if (baseArtifact is DiagramArtifact)
            {
                return baseArtifact.ArtifactSet.GetEntityDesignArtifact();
            }
            else if (baseArtifact is EntityDesignArtifact)
            {
                return baseArtifact as EntityDesignArtifact;
            }
            else
            {
                Debug.Fail(
                    "The artifact must be an EntityDesignArtifact or DiagramArtifact to be used with the EFExtensions extension methods.");
            }
            return null;
        }

        internal static IEnumerable<ConceptualEntityType> ConceptualEntityTypes(this EFArtifactSet thisArtifactSet)
        {
            var derivedArtifactSet = thisArtifactSet as EntityDesignArtifactSet;
            Debug.Assert(
                derivedArtifactSet != null,
                "Every EFArtifactSet must be an EntityDesignArtifactSet to be used with the EFExtensions extension methods.");
            if (derivedArtifactSet != null)
            {
                return derivedArtifactSet.ConceptualEntityTypes;
            }
            return null;
        }

        internal static EFRuntimeModelRoot RuntimeModelRoot(this EFObject thisObject)
        {
            var item = thisObject.Parent;
            while (item != null
                   && item.Parent != null)
            {
                if (item is EFRuntimeModelRoot)
                {
                    break;
                }
                item = item.Parent;
            }

            return item as EFRuntimeModelRoot;
        }

        internal static string GetSsdlAsString(this EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "Artifact is null ");
            if (artifact != null)
            {
                return GetSchemaFromRuntimeModelRoot(artifact.StorageModel());
            }

            return String.Empty;
        }

        internal static string GetMslAsString(this EFArtifact artifact)
        {
            Debug.Assert(artifact != null, "Artifact is null ");
            if (artifact != null)
            {
                return GetSchemaFromRuntimeModelRoot(artifact.MappingModel());
            }

            return String.Empty;
        }

        private static string GetSchemaFromRuntimeModelRoot(EFRuntimeModelRoot modelRoot)
        {
            Debug.Assert(modelRoot != null, "EFRuntimeModelRoot is null ");
            Debug.Assert(modelRoot.XElement != null,
                "Could not find the runtime model root or its XElement in GetSchemaFromRuntimeModelRoot");

            var sb = new StringBuilder();

            if (modelRoot != null
                && modelRoot.XElement != null)
            {
                using (var writer = new StringWriter(sb, CultureInfo.CurrentCulture))
                {
                    modelRoot.XElement.Save(writer);
                }
            }

            return sb.ToString();
        }

        internal static EdmItemCollection GetEdmItemCollectionFromArtifact(this EFArtifact artifact, out IList<EdmSchemaError> schemaErrors)
        {
            Debug.Assert(artifact != null, "Artifact is null ");

            var conceptualModel = artifact.ConceptualModel();

            EdmItemCollection edmItemCollection = null;
            schemaErrors = new List<EdmSchemaError>();

            Debug.Assert(
                conceptualModel != null && conceptualModel.XElement != null,
                "Could not find the conceptual model or its XElement in GetEdmItemCollectionFromArtifact");
            if (conceptualModel != null
                && conceptualModel.XElement != null)
            {
                using (var xmlReader = conceptualModel.XElement.CreateReader())
                {
                    edmItemCollection = EdmItemCollection.Create(new[] { xmlReader }, null, out schemaErrors);
                }
            }

            return edmItemCollection;
        }

        internal static string GetProviderManifestToken(this EFArtifact artifact)
        {
            var storageModel = artifact.StorageModel();
            if (storageModel != null
                && storageModel.ProviderManifestToken != null)
            {
                return storageModel.ProviderManifestToken.Value;
            }

            Debug.Fail("Unable to determine the provider manifest token for the SSDL");

            return String.Empty;
        }
    }
}
