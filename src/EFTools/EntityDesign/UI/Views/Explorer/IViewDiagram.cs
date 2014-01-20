// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.Explorer
{
    using Microsoft.Data.Entity.Design.Model;

    // <summary>
    //     Interface that is implemented by DSL Diagram code.
    //     The interface is created to decouple Dsl and EntityDesigner code.
    // </summary>
    internal interface IViewDiagram
    {
        void AddOrShowEFElementInDiagram(EFElement efElement);
        string DiagramId { get; }
    }
}
