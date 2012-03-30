namespace System.Data.Entity.Infrastructure
{
    /// <summary>
    ///     This attribute can be applied to either an entire derived <see cref = "DbContext" /> class or to
    ///     individual <see cref = "DbSet{T}" /> or <see cref = "IDbSet{T}" /> properties on that class.  When applied
    ///     any discovered <see cref = "DbSet{T}" /> or <see cref = "IDbSet{T}" /> properties will still be included
    ///     in the model but will not be automatically initialized.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Class, AllowMultiple = false)]
    public sealed class SuppressDbSetInitializationAttribute : Attribute
    {
    }
}
