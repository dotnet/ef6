namespace System.Data.Entity
{
    /// <summary>
    /// Indicates that the given method is a proxy for an EDM function.
    /// </summary>
    /// <remarks>
    /// Note that this class was called EdmFunctionAttribute in some previous versions of Entity Framework.
    /// </remarks>
    [AttributeUsage(AttributeTargets.Method, Inherited = false, AllowMultiple = false)]
    public sealed class DbFunctionAttribute : Attribute
    {
        private readonly string _namespaceName;
        private readonly string _functionName;

        /// <summary>
        /// Creates a new DbFunctionAttribute instance.
        /// </summary>
        /// <param name="namespaceName">The namespace name of the EDM function represented by the attributed method</param>
        /// <param name="functionName">The function name of the EDM function represented by the attributed method</param>
        public DbFunctionAttribute(string namespaceName, string functionName)
        {
            _namespaceName = namespaceName;
            _functionName = functionName;
        }

        /// <summary>
        /// The namespace name of the EDM function represented by the attributed method
        /// </summary>
        public string NamespaceName
        {
            get { return _namespaceName; }
        }

        /// <summary>
        /// The function name of the EDM function represented by the attributed method
        /// </summary>
        public string FunctionName
        {
            get { return _functionName; }
        }
    }
}
