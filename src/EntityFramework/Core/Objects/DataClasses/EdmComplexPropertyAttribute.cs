namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Attribute for complex properties
    /// Implied default AttributeUsage properties Inherited=True, AllowMultiple=False,
    /// The metadata system expects this and will only look at the first of each of these attributes, even if there are more.
    /// </summary>    
    [AttributeUsage(AttributeTargets.Property)]
    public sealed class EdmComplexPropertyAttribute : EdmPropertyAttribute
    {
    }
}
