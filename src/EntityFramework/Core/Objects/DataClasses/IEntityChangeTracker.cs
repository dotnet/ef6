using System.Data.Entity.Core.Objects;
using System.Reflection;

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// This interface is implemented by a change tracker and is used by data classes to report changes
    /// </summary>
    public interface IEntityChangeTracker
    {
        /// <summary>
        /// Used to report that a scalar entity property is about to change        
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that is changing</param>
        void EntityMemberChanging(string entityMemberName);

        /// <summary>
        /// Used to report that a scalar entity property has been changed        
        /// </summary>
        /// <param name="entityMemberName">The name of the entity property that has changed</param>
        void EntityMemberChanged(string entityMemberName);

        /// <summary>
        /// Used to report that a complex property is about to change        
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that is changing</param>
        /// <param name="complexObject">The complex object that contains the property that is changing</param>
        /// <param name="complexObjectMemberName">The name of the property that is changing on complexObject</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        void EntityComplexMemberChanging(string entityMemberName, object complexObject, string complexObjectMemberName);
        
        /// <summary>
        /// Used to report that a complex property has been changed        
        /// </summary>
        /// <param name="entityMemberName">The name of the top-level entity property that has changed</param>
        /// <param name="complexObject">The complex object that contains the property that changed</param>
        /// <param name="complexObjectMemberName">The name of the property that changed on complexObject</param>
        [SuppressMessage("Microsoft.Naming", "CA1720:IdentifiersShouldNotContainTypeNames", MessageId = "object")]
        void EntityComplexMemberChanged(string entityMemberName, object complexObject, string complexObjectMemberName);

        /// <summary>
        /// Returns the EntityState from the change tracker, or EntityState.Detached if this
        /// entity is not being managed by a change tracker
        /// </summary>
        EntityState EntityState { get;  }
    }
}
