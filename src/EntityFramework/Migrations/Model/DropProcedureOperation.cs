// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class DropProcedureOperation : MigrationOperation
    {
        private readonly string _name;

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public DropProcedureOperation(string name, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public override MigrationOperation Inverse
        {
            get { return NotSupportedOperation.Instance; }
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
