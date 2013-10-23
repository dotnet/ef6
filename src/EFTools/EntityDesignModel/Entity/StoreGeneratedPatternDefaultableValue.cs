// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class StoreGeneratedPatternForCsdlDefaultableValue : DefaultableValue<string>
    {
        // below must be const (rather than static readonly) because referenced in an attribute in PropertyClipboardFormat
        internal const string AttributeStoreGeneratedPattern = "StoreGeneratedPattern";

        internal StoreGeneratedPatternForCsdlDefaultableValue(EFElement parent)
            : base(parent, AttributeStoreGeneratedPattern, SchemaManager.GetAnnotationNamespaceName())
        {
        }

        internal override string AttributeName
        {
            get { return AttributeStoreGeneratedPattern; }
        }

        public override string DefaultValue
        {
            get { return ModelConstants.StoreGeneratedPattern_None; }
        }

        internal override bool ValidateValueAgainstSchema()
        {
            return (Parent.Artifact.SchemaVersion != EntityFrameworkVersion.Version1);
        }
    }

    internal class StoreGeneratedPatternForSsdlDefaultableValue : DefaultableValue<string>
    {
        // below must be const (rather than static readonly) because referenced in an attribute in PropertyClipboardFormat
        internal const string AttributeStoreGeneratedPattern = "StoreGeneratedPattern";

        internal StoreGeneratedPatternForSsdlDefaultableValue(EFElement parent)
            : base(parent, AttributeStoreGeneratedPattern)
        {
        }

        internal override string AttributeName
        {
            get { return AttributeStoreGeneratedPattern; }
        }

        public override string DefaultValue
        {
            get { return ModelConstants.StoreGeneratedPattern_None; }
        }
    }
}
