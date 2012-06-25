namespace System.Data.Entity.Config
{
    using System.Diagnostics.Contracts;

    /// <summary>
    /// Implements <see cref="IDbDependencyResolver"/> to resolve a dependency such that it always returns
    /// the same instance and does nothing on Release.
    /// </summary>
    /// <typeparam name="T">The type that defines the contract for the dependency that will be resolved.</typeparam>
    public class SingletonDependencyResolver<T> : IDbDependencyResolver
    {
        private readonly T _singletonInstance;
        private readonly string _name;

        /// <summary>
        /// Constructs a new resolver that will return the given instance for the contract type
        /// regardless of the name passed to the Get method.
        /// </summary>
        /// <param name="singletonInstance">The instance to return.</param>
        public SingletonDependencyResolver(T singletonInstance)
            : this(singletonInstance, null)
        {
        }

        /// <summary>
        /// Constructs a new resolver that will return the given instance for the contract type
        /// if the given name matches exactly the name passed to the Get method.
        /// </summary>
        /// <param name="singletonInstance">The instance to return.</param>
        /// <para>The name of the dependency to resolve.</para>
        public SingletonDependencyResolver(T singletonInstance, string name)
        {
            Contract.Requires(singletonInstance != null);

            _singletonInstance = singletonInstance;
            _name = name;
        }

        /// <inheritdoc/>
        public object Get(Type type, string name)
        {
            return type == typeof(T) && (_name == null || name == _name)
                       ? (object)_singletonInstance
                       : null;
        }

        /// <inheritdoc/>
        public void Release(object service)
        {
        }
    }
}