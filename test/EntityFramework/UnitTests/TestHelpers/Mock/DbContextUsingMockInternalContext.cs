// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity
{
    using System.Data.Entity.Internal;

    /// <summary>
    ///     Allows the mocked internal context to be returned from the real DbContext that is
    ///     needed for tests that key on the context type
    /// </summary>
    public abstract class DbContextUsingMockInternalContext : DbContext
    {
        internal InternalContext MockedInternalContext { get; set; }

        internal override InternalContext InternalContext
        {
            get { return MockedInternalContext; }
        }
    }
}
