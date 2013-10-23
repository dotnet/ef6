// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Validation;

    /// <summary>
    ///     This class represents a set of artifacts that are validated and resolved with respect to one another.
    ///     This class contains resolve information (symbols & bindings) and dep & anti-dep info
    /// </summary>
    internal class EntityDesignArtifactSet : EFArtifactSet
    {
        internal EntityDesignArtifactSet(EFArtifact artifact)
            : base(artifact)
        {
        }

        internal bool ShouldDoRuntimeMappingValidation()
        {
            foreach (var error in GetErrors(ErrorClass.Escher_All))
            {
                if (EscherModelValidator.IsSkipRuntimeValidationError(error))
                {
                    return false;
                }
            }
            return true;
        }

        /// <summary>
        ///     Returns conceptual entity types from every artifact in this set.
        /// </summary>
        internal IEnumerable<ConceptualEntityType> ConceptualEntityTypes
        {
            get
            {
                var artifact = this.GetEntityDesignArtifact();
                Debug.Assert(artifact != null, "ArtifactSet's Artifact collection does not contain EntityDesignArtifact");
                if (artifact != null)
                {
                    foreach (var entity in artifact.ConceptualModel.EntityTypes())
                    {
                        var cet = entity as ConceptualEntityType;
                        Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
                        yield return cet;
                    }
                }
            }
        }

        internal override Version SchemaVersion
        {
            get
            {
                var entityDesignArtifact = this.GetEntityDesignArtifact();
                Debug.Assert(entityDesignArtifact != null, "ArtifactSet's Artifact collection does not contain EntityDesignArtifact");
                return entityDesignArtifact.SchemaVersion;
            }
        }
    }
}
