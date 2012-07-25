// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Config
{
    /// <summary>
    /// This interface is implemented by any object that can resolve a dependency, either directly
    /// or through use of an external container.
    /// </summary>
    /// <remarks>
    /// Note that multiple threads may call into the same IDbDependencyResolver instance which means
    /// that implementations of this interface must be either immutable or thread-safe.
    /// </remarks>
    public interface IDbDependencyResolver
    {
        /// <summary>
        /// Attempts to resolve a dependency for a given contract type and optionally a given name.
        /// If the resolver cannot resolve the dependency then it must return null and not throw. This
        /// allows resolvers to be used in a Chain of Responsibility pattern such that multiple resolvers
        /// can be asked to resolver a dependency until one finally does.
        /// </summary>
        /// <param name="type">
        /// The interface or abstract base class that defines the dependency to be resolved.
        /// The returned object is expected to be an instance of this type.
        /// </param>
        /// <param name="name">
        /// Optionally, the name of the dependency to be resolved. This may be null for dependencies that
        /// are not differentiated by name.
        /// </param>
        /// <returns>
        /// The resolved dependency, which must be an instance of the given contract type, or
        /// null if the dependency could not be resolved.
        /// </returns>        
        object GetService(Type type, string name);

        /// <summary>
        /// This method is called for transient services to give the resolver a chance to release the service
        /// after it has finished being used. This is roughly equivalent to the disposing the service except
        /// that the container is given the chance to control the process.
        /// This method must do nothing if the service was not one that it created or if the service does not
        /// need to be disposed. This method should not throw and should be resilient to cases where the
        /// service has already been released.
        /// </summary>
        /// <param name="service">The service to release.</param>
        void Release(object service);
    }
}
