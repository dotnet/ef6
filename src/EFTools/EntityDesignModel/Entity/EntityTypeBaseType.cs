// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;

    /// <summary>
    ///     The purpose of this class is to define a distinct type for the Base Type
    ///     property of an EntityType so that we can map this to a particular ItemDescriptor
    ///     in the property window.  We want the user to be able to click on an inheritance line
    ///     and get a property window editing experience.
    /// </summary>
    internal class EntityTypeBaseType : SingleItemBinding<ConceptualEntityType>
    {
        internal EntityTypeBaseType(EFElement parent, string attributeName, NameNormalizer nameNormalizer)
            : base(parent, attributeName, nameNormalizer)
        {
        }

        internal ConceptualEntityType OwnerEntityType
        {
            get
            {
                var owner = Parent as ConceptualEntityType;
                Debug.Assert(owner != null, "this.Parent should be a " + typeof(ConceptualEntityType));
                return owner;
            }
        }
    }
}
