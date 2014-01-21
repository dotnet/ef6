namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    [SuppressMessage("Microsoft.Design", "CA1064:ExceptionsShouldBePublic")]
    [Serializable]
    internal class CodeFirstModelGenerationException : Exception
    {
        public CodeFirstModelGenerationException(string message, Exception innerException)
            : base(message, innerException)
        {
            Debug.Assert(!string.IsNullOrEmpty(message), "message is null or empty.");
            Debug.Assert(innerException != null, "innerException is null.");
        }

        protected CodeFirstModelGenerationException(SerializationInfo info, StreamingContext context)
            : base(info, context)
        {
        }
    }
}
