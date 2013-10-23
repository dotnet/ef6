// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class ExternalTypeNameDefaultableValue : DefaultableValue<string>
    {
        internal const string AttributeExternalTypeName = "ExternalTypeName";

        internal ExternalTypeNameDefaultableValue(EFElement parent)
            : base(parent, AttributeExternalTypeName, SchemaManager.GetCodeGenerationNamespaceName())
        {
        }

        internal override string AttributeName
        {
            get { return AttributeExternalTypeName; }
        }

        /// <summary>
        ///     The non-existence of the attribute should be interpreted as string empty.
        /// </summary>
        public override string DefaultValue
        {
            get { return String.Empty; }
        }

        // TODO remove this once we got the XSD change from runtime.
        // This is a low priority since the XSD type will accept any value for this attribute.
        internal override bool ValidateValueAgainstSchema()
        {
            return false;
        }
    }
}
