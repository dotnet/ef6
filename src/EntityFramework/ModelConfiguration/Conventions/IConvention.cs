namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    ///     Identifies conventions that can be removed from a <see cref = "DbModelBuilder" /> instance.
    /// </summary>
    /// /// <remarks>
    /// Note that implementations of this interface must be immutable.
    /// </remarks>
    [SuppressMessage("Microsoft.Design", "CA1040:AvoidEmptyInterfaces")]
    public interface IConvention
    {
    }
}
