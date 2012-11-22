// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm.Validation.Internal.EdmModel;

    internal abstract class DataModelValidationRule
    {
        internal abstract Type ValidatedType { get; }
        internal abstract void Evaluate(EdmModelValidationContext context, IMetadataItem item);
    }
}
