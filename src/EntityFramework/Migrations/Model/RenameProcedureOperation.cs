// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public class RenameProcedureOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _newName;

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        public RenameProcedureOperation(string name, string newName, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");
            Check.NotEmpty(newName, "newName");

            _name = name;
            _newName = newName;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public virtual string NewName
        {
            get { return _newName; }
        }

        public override MigrationOperation Inverse
        {
            get
            {
                var originalName = DatabaseName.Parse(_name);
                var newTable = DatabaseName.Parse(_newName).Name;

                return new RenameProcedureOperation(
                    new DatabaseName(newTable, originalName.Schema).ToString(),
                    originalName.Name);
            }
        }

        /// <inheritdoc />
        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
