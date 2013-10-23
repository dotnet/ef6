// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.ViewModel
{
    using System.Collections.Generic;
    using Microsoft.VisualStudio.Modeling;

    internal interface IContainRelatedElementsToEmphasizeWhenSelected
    {
        IEnumerable<ModelElement> RelatedElementsToEmphasizeOnSelected { get; }
    }
}
