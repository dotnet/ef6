// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    ///     The context for DataModel Validation
    /// </summary>
    internal abstract class DataModelValidationContext
    {
        internal bool ValidateSyntax { get; set; }
        internal double ValidationContextVersion { get; set; }

        internal abstract void AddError(IMetadataItem item, string propertyName, string errorMessage, int errorCode);
    }
}
