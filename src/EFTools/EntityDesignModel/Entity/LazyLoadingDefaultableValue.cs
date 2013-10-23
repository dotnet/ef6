// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class LazyLoadingDefaultableValue : DefaultableValue<bool>
    {
        internal const string AttributeLazyLoadingEnabled = "LazyLoadingEnabled";

        internal LazyLoadingDefaultableValue(EFElement parent)
            : base(parent, AttributeLazyLoadingEnabled, SchemaManager.GetAnnotationNamespaceName())
        {
        }

        internal override string AttributeName
        {
            get { return AttributeLazyLoadingEnabled; }
        }

        /// <summary>
        ///     The non-existence of the attribute should be interpreted as false.
        /// </summary>
        public override bool DefaultValue
        {
            get { return false; }
        }

        internal override bool ValidateValueAgainstSchema()
        {
            if (EdmFeatureManager.GetLazyLoadingFeatureState(Parent.Artifact.SchemaVersion).IsEnabled())
            {
                return true;
            }
            return false;
        }
    }
}
