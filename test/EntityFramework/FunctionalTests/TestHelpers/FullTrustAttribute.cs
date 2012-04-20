namespace System.Data.Entity
{
    using System;

    /// <summary>
    /// When used in combination with <see cref="PartialTrustFixtureAttribute" />, indicates
    /// that a method should not be run under partial trust.
    /// </summary>
    [AttributeUsage(AttributeTargets.Method, AllowMultiple = false)]
    public class FullTrustAttribute : Attribute
    {
    }
}
