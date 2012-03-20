namespace System.Data.Entity.Migrations.Model
{
    using System.Collections.Generic;
    using System.Data.Entity.Migrations.Extensions;

    /// <summary>
    ///     Represents an operation to modify a database schema.
    /// </summary>
    public abstract class MigrationOperation
    {
        private readonly IDictionary<string, object> _anonymousArguments
            = new Dictionary<string, object>();

        /// <summary>
        ///     Initializes a new instance of the MigrationOperation class.
        /// </summary>
        /// <param name = "anonymousArguments">
        ///  
        ///     Use anonymous type syntax to specify arguments e.g. 'new { SampleArgument = "MyValue" }'.
        /// </param>
        protected MigrationOperation(object anonymousArguments)
        {
            if (anonymousArguments != null)
            {
                anonymousArguments.GetType().GetProperties()
                    .Each(p => _anonymousArguments.Add(p.Name, p.GetValue(anonymousArguments, null)));
            }
        }

        /// <summary>
        ///     Gets additional arguments that may be processed by providers.
        /// </summary>
        public IDictionary<string, object> AnonymousArguments
        {
            get { return _anonymousArguments; }
        }

        /// <summary>
        ///     Gets an operation that will revert this operation.
        /// </summary>
        public virtual MigrationOperation Inverse
        {
            get { return null; }
        }

        /// <summary>
        ///     Gets a value indicating if this operation may result in data loss.
        /// </summary>
        public abstract bool IsDestructiveChange { get; }
    }
}