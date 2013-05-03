// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class MoveProcedureOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _newSchema;

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public MoveProcedureOperation(string name, string newSchema, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _newSchema = newSchema;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string NewSchema
        {
            get { return _newSchema; }
        }

        public override MigrationOperation Inverse
        {
            get
            {
                var databaseName = _name.ToDatabaseName();

                return new MoveProcedureOperation(NewSchema + '.' + databaseName.Name, databaseName.Schema);
            }
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
