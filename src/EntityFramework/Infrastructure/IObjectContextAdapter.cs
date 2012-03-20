namespace System.Data.Entity.Infrastructure
{
    using System.Data.Objects;

    /// <summary>
    ///     Interface implemented by objects that can provide an <see cref = "ObjectContext" /> instance.
    ///     The <see cref = "DbContext" /> class implements this interface to provide access to the underlying
    ///     ObjectContext.
    /// </summary>
    public interface IObjectContextAdapter
    {
        /// <summary>
        ///     Gets the object context.
        /// </summary>
        /// <value>The object context.</value>
        ObjectContext ObjectContext { get; }
    }
}