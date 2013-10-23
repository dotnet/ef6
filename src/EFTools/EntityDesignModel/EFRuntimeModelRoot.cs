// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System.Xml.Linq;

    internal abstract class EFRuntimeModelRoot : EFNormalizableItem
    {
        protected EFRuntimeModelRoot(EFArtifact parent, XElement element)
            : base(parent, element)
        {
        }

        internal abstract XNamespace XNamespace { get; }

        /// <summary>
        ///     Retrieve the root "Schema" node for current node.  If there is no root mapping node, this will return null.
        /// </summary>
        /// <returns></returns>
        internal static XNamespace GetRootNamespace(EFObject node)
        {
            var currNode = node;
            EFRuntimeModelRoot runtimeModel = null;
            EFDesignerInfoRoot designerInfo = null;
            XNamespace ns = null;

            while (currNode != null)
            {
                runtimeModel = currNode as EFRuntimeModelRoot;
                designerInfo = currNode as EFDesignerInfoRoot;
                if (runtimeModel != null)
                {
                    ns = runtimeModel.XNamespace;
                    break;
                }
                else if (designerInfo != null)
                {
                    ns = designerInfo.XNamespace;
                    break;
                }
                else
                {
                    currNode = currNode.Parent;
                }
            }

            return ns;
        }
    }
}
