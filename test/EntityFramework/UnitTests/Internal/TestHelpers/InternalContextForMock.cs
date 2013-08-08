// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Internal
{
    using System.Data.Entity.Core.Objects;

    /// <summary>
    /// A derived InternalContext implementation that exposes a parameterless constructor
    /// that creates a mocked underlying DbContext such that the internal context can
    /// also be mocked.
    /// </summary>
    internal abstract class InternalContextForMock : InternalContextForMock<DbContext>
    {
        protected InternalContextForMock()
        {
        }

        protected InternalContextForMock(ObjectContext objectContext)
            : base(objectContext)
        {
        }
    }
}
