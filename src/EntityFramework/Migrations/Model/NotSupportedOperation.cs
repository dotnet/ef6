// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    public class NotSupportedOperation : MigrationOperation
    {
        internal static readonly NotSupportedOperation Instance = new NotSupportedOperation();

        private NotSupportedOperation()
            : base(null)
        {
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
