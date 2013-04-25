// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects
{
    using System.Data.Common;
    using System.Data.Entity.Core.EntityClient;
    using System.Data.Entity.Core.EntityClient.Internal;

    public class ObjectContextForMock : ObjectContext
    {
        private readonly EntityConnection _connection;

        internal ObjectContextForMock(EntityConnection connection, IEntityAdapter entityAdapter = null)
            : base(null, null, null, entityAdapter)
        {
            _connection = connection;
        }

        public override DbConnection Connection
        {
            get { return _connection; }
        }
    }
}
