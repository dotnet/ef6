// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System.Diagnostics;
    using System.Xml.Linq;

    internal abstract class TextNode : EFElement
    {
        internal TextNode(EFContainer parent, XElement element)
            : base(parent, element)
        {
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            Debug.Fail("This node does not have any children");
        }

        internal string Text
        {
            get { return XElement.Value; }

            set { XElement.Value = value; }
        }
    }
}
