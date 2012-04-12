namespace System.Data.Entity.Core.Objects.DataClasses
{
    /// <summary>
    /// Attribute identifying the Edm base class
    /// </summary>
    [AttributeUsage(AttributeTargets.Class, AllowMultiple = false)]
    public sealed class EdmEntityTypeAttribute : EdmTypeAttribute
    {
    }
}
