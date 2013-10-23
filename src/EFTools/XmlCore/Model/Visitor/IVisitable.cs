// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System.Collections.Generic;

    internal interface IVisitable
    {
        IEnumerable<IVisitable> Accept(Visitor visitor);
    }
}
