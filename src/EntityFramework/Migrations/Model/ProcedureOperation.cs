// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// A migration operation that affects stored procedures.
    ///
    /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
    /// (such as the end user of an application). If input is accepted from such sources it should be validated 
    /// before being passed to these APIs to protect against SQL injection attacks etc.
    /// </summary>
    public abstract class ProcedureOperation : MigrationOperation
    {
        private readonly string _name;
        private readonly string _bodySql;

        private readonly List<ParameterModel> _parameters = new List<ParameterModel>();

        /// <summary>
        /// Initializes a new instance of the <see cref="ProcedureOperation"/> class.
        ///
        /// Entity Framework Migrations APIs are not designed to accept input provided by untrusted sources 
        /// (such as the end user of an application). If input is accepted from such sources it should be validated 
        /// before being passed to these APIs to protect against SQL injection attacks etc.
        /// </summary>
        /// <param name="name">The name of the stored procedure.</param>
        /// <param name="bodySql">The body of the stored procedure expressed in SQL.</param>
        /// <param name="anonymousArguments"> Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'. </param>
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        protected ProcedureOperation(string name, string bodySql, object anonymousArguments = null)
            : base(anonymousArguments)
        {
            Check.NotEmpty(name, "name");

            _name = name;
            _bodySql = bodySql;
        }

        /// <summary>
        /// Gets the name of the stored procedure.
        /// </summary>
        /// <value>
        /// The name of the stored procedure.
        /// </value>
        public virtual string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Gets the body of the stored procedure expressed in SQL.
        /// </summary>
        /// <value>
        /// The body of the stored procedure expressed in SQL.
        /// </value>
        public string BodySql
        {
            get { return _bodySql; }
        }

        /// <summary>
        /// Gets the parameters of the stored procedure.
        /// </summary>
        /// <value>
        /// The parameters of the stored procedure.
        /// </value>
        public virtual IList<ParameterModel> Parameters
        {
            get { return _parameters; }
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
