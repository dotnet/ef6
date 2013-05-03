// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Diagnostics.CodeAnalysis;

    public class AlterProcedureOperation : ProcedureOperation
    {
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public AlterProcedureOperation(string name, string bodySql, object anonymousArguments = null)
            : base(name, bodySql, anonymousArguments)
        {
        }

        public override MigrationOperation Inverse
        {
            get { return NotSupportedOperation.Instance; }
        }
    }
}
