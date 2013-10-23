// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

 // Temporary workaround, this interface should be remove shortly.

namespace Microsoft.Data.Entity.Design.Model.Designer
{
    internal interface DiagramEFObject
    {
        // denotes EntityTypeShapes, InheritanceConnectors, AssociationConnectors, etc.
    }
}

namespace Microsoft.Data.Tools.Model.Diagram
{
    using System.Diagnostics;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Designer;

    internal abstract class BaseDiagramObject : EFElement, DiagramEFObject
    {
        protected BaseDiagramObject(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal virtual IDiagram Diagram
        {
            get
            {
                var diagram = GetParentOfType(typeof(IDiagram)) as IDiagram;
                Debug.Assert(diagram != null, "Could not find diagram for the connector with display name:" + DisplayName);
                return diagram;
            }
        }

        internal abstract EFObject ModelItem { get; }
    }
}
