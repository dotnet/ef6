// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    /// <summary>
    /// Represents a migration operation that can not be performed, possibly because it is not supported by the targeted database provider.
    /// </summary>
    public class NotSupportedOperation : MigrationOperation
    {
        internal static readonly NotSupportedOperation Instance = new NotSupportedOperation();

        private NotSupportedOperation()
            : base(null)
        {
        }

        /// <summary>
        /// Gets a value indicating if this operation may result in data loss. Always returns false.
        /// </summary>
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
