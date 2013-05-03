// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    public abstract class ProcedureOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _bodySql;

        private readonly List<ParameterModel> _parameters = new List<ParameterModel>();

        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected ProcedureOperation(string name, string bodySql, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _bodySql = bodySql;
        }

        public virtual string Name
        {
            get { return _name; }
        }

        public string BodySql
        {
            get { return _bodySql; }
        }

        public virtual ICollection<ParameterModel> Parameters
        {
            get { return _parameters; }
        }

        public override bool IsDestructiveChange
        {
            get { return false; }
        }
    }
}
