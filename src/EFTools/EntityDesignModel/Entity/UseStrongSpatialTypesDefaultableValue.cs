// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class UseStrongSpatialTypesDefaultableValue : DefaultableValue<bool>
    {
        internal const string AttributeUseStrongSpatialTypes = "UseStrongSpatialTypes";

        internal UseStrongSpatialTypesDefaultableValue(EFElement parent)
            : base(parent, AttributeUseStrongSpatialTypes, SchemaManager.GetAnnotationNamespaceName())
        {
        }

        internal override string AttributeName
        {
            get { return AttributeUseStrongSpatialTypes; }
        }

        /// <summary>
        ///     The non-existence of the attribute should be interpreted as true.
        /// </summary>
        public override bool DefaultValue
        {
            get { return true; }
        }

        internal override bool ValidateValueAgainstSchema()
        {
            if (EdmFeatureManager.GetUseStrongSpatialTypesFeatureState(Parent.Artifact.SchemaVersion).IsEnabled())
            {
                return true;
            }
            return false;
        }
    }
}
