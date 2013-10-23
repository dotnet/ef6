// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class TypeAccessDefaultableValue : DefaultableValue<string>
    {
        internal static readonly string AttributeTypeAccess = "TypeAccess";

        internal TypeAccessDefaultableValue(EFElement parent)
            : base(parent, AttributeTypeAccess, SchemaManager.GetCodeGenerationNamespaceName())
        {
        }

        internal override string AttributeName
        {
            get { return AttributeTypeAccess; }
        }

        public override string DefaultValue
        {
            get { return ModelConstants.CodeGenerationAccessPublic; }
        }
    }
}
