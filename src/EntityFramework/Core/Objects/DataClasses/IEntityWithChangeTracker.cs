namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Minimum interface that a data class must implement in order to be managed by a change tracker.
    /// </summary>
    public interface IEntityWithChangeTracker
    {
        /// <summary>
        /// Used by the change tracker to provide an interface that the data class will use to report changes.
        /// </summary>
        /// <param name="changeTracker">Reference to the change tracker that is managing this entity</param>        
        void SetChangeTracker(IEntityChangeTracker changeTracker);
    }
}
