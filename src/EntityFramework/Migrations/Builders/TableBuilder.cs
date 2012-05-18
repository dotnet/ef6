namespace System.Data.Entity.Migrations.Builders
{
    using System.ComponentModel;
    using System.Data.Entity.Migrations.Extensions;
    using System.Data.Entity.Migrations.Model;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Linq.Expressions;

    /// <summary>
    ///     Helper class that is used to further configure a table being created from a CreateTable call on <see cref = "DbMigration" />.
    /// </summary>
    public class TableBuilder<TColumns>
    {
        private readonly CreateTableOperation _createTableOperation;
        private readonly DbMigration _migration;

        /// <summary>
        ///     Initializes a new instance of the TableBuilder class.
        /// </summary>
        /// <param name = "createTableOperation">The table creation operation to be further configured.</param>
        /// <param name = "migration">The migration the table is created in.</param>
        public TableBuilder(CreateTableOperation createTableOperation, DbMigration migration)
        {
            Contract.Requires(createTableOperation != null);

            _createTableOperation = createTableOperation;
            _migration = migration;
        }

        /// <summary>
        ///     Specifies a primary key for the table.
        /// </summary>
        /// <param name = "keyExpression">
        ///     A lambda expression representing the property to be used as the primary key. 
        ///     C#: t => t.Id   
        ///     VB.Net: Function(t) t.Id
        /// 
        ///     If the primary key is made up of multiple properties then specify an anonymous type including the properties. 
        ///     C#: t => new { t.Id1, t.Id2 }
        ///     VB.Net: Function(t) New With { t.Id1, t.Id2 }
        /// </param>
        /// <param name = "name">
        ///     The name of the primary key.
        ///     If null is supplied, a default name will be generated.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <returns>Itself, so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public TableBuilder<TColumns> PrimaryKey(
            Expression<Func<TColumns, object>> keyExpression,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(keyExpression != null);

            var addPrimaryKeyOperation = new AddPrimaryKeyOperation(anonymousArguments)
                                             {
                                                 Name = name
                                             };

            keyExpression
                .GetPropertyAccessList()
                .Select(p => p.Last().Name)
                .Each(c => addPrimaryKeyOperation.Columns.Add(c));

            _createTableOperation.PrimaryKey = addPrimaryKeyOperation;

            return this;
        }

        /// <summary>
        ///     Specifies an index to be created on the table.
        /// </summary>
        /// <param name = "indexExpression">
        ///     A lambda expression representing the property to be indexed. 
        ///     C#: t => t.PropertyOne   
        ///     VB.Net: Function(t) t.PropertyOne
        /// 
        ///     If multiple properties are to be indexed then specify an anonymous type including the properties. 
        ///     C#: t => new { t.PropertyOne, t.PropertyTwo }
        ///     VB.Net: Function(t) New With { t.PropertyOne, t.PropertyTwo }
        /// </param>
        /// <param name = "unique">A value indicating whether or not this is a unique index.</param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <returns>Itself, so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public TableBuilder<TColumns> Index(
            Expression<Func<TColumns, object>> indexExpression, bool unique = false, object anonymousArguments = null)
        {
            Contract.Requires(indexExpression != null);

            var createIndexOperation
                = new CreateIndexOperation(anonymousArguments)
                      {
                          Table = _createTableOperation.Name,
                          IsUnique = unique
                      };

            indexExpression
                .GetPropertyAccessList()
                .Select(p => p.Last().Name)
                .Each(c => createIndexOperation.Columns.Add(c));

            _migration.AddOperation(createIndexOperation);

            return this;
        }

        /// <summary>
        ///     Specifies a foreign key constraint to be created on the table.
        /// </summary>
        /// <param name = "principalTable">Name of the table that the foreign key constraint targets.</param>
        /// <param name = "dependentKeyExpression">
        ///     A lambda expression representing the properties of the foreign key. 
        ///     C#: t => t.PropertyOne   
        ///     VB.Net: Function(t) t.PropertyOne
        /// 
        ///     If multiple properties make up the foreign key then specify an anonymous type including the properties. 
        ///     C#: t => new { t.PropertyOne, t.PropertyTwo }
        ///     VB.Net: Function(t) New With { t.PropertyOne, t.PropertyTwo }</param>
        /// <param name = "cascadeDelete">
        ///     A value indicating whether or not cascade delete should be configured on the foreign key constraint.
        /// </param>
        /// <param name = "name">
        ///     The name of this foreign key constraint.
        ///     If no name is supplied, a default name will be calculated.
        /// </param>
        /// <param name = "anonymousArguments">
        ///     Additional arguments that may be processed by providers. 
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        /// <returns>Itself, so that multiple calls can be chained.</returns>
        [SuppressMessage("Microsoft.Design", "CA1011:ConsiderPassingBaseTypesAsParameters")]
        [SuppressMessage("Microsoft.Design", "CA1026:DefaultParametersShouldNotBeUsed")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public TableBuilder<TColumns> ForeignKey(
            string principalTable,
            Expression<Func<TColumns, object>> dependentKeyExpression,
            bool cascadeDelete = false,
            string name = null,
            object anonymousArguments = null)
        {
            Contract.Requires(!string.IsNullOrWhiteSpace(principalTable));
            Contract.Requires(dependentKeyExpression != null);

            var addForeignKeyOperation = new AddForeignKeyOperation(anonymousArguments)
                                             {
                                                 Name = name,
                                                 PrincipalTable = principalTable,
                                                 DependentTable = _createTableOperation.Name,
                                                 CascadeDelete = cascadeDelete
                                             };

            dependentKeyExpression
                .GetPropertyAccessList()
                .Select(p => p.Last().Name)
                .Each(c => addForeignKeyOperation.DependentColumns.Add(c));

            _migration.AddOperation(addForeignKeyOperation);

            return this;
        }

        #region Hide object members

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override string ToString()
        {
            return base.ToString();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override bool Equals(object obj)
        {
            return base.Equals(obj);
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        public override int GetHashCode()
        {
            return base.GetHashCode();
        }

        [SuppressMessage("Microsoft.Design", "CA1024:UsePropertiesWhereAppropriate")]
        [EditorBrowsable(EditorBrowsableState.Never)]
        public new Type GetType()
        {
            return base.GetType();
        }

        [EditorBrowsable(EditorBrowsableState.Never)]
        protected new object MemberwiseClone()
        {
            return base.MemberwiseClone();
        }

        #endregion
    }
}
