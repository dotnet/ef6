namespace System.Data.Entity.Utilities
{
    using System.Data.Entity.Core;
    using System.Diagnostics.Contracts;
    using System.Security;
    using System.Threading;

    internal static class ExceptionExtensions
    {
        public static bool IsCatchableExceptionType(this Exception e)
        {
            Contract.Requires(e != null);

            // a 'catchable' exception is defined by what it is not.
            var type = e.GetType();

            return ((type != typeof(StackOverflowException)) &&
                    (type != typeof(OutOfMemoryException)) &&
                    (type != typeof(ThreadAbortException)) &&
                    (type != typeof(NullReferenceException)) &&
                    (type != typeof(AccessViolationException)) &&
                    !typeof(SecurityException).IsAssignableFrom(type));
        }

        public static bool IsCatchableEntityExceptionType(this Exception e)
        {
            Contract.Requires(e != null);

            var type = e.GetType();

            return IsCatchableExceptionType(e) &&
                   type != typeof(EntityCommandExecutionException) &&
                   type != typeof(EntityCommandCompilationException) &&
                   type != typeof(EntitySqlException);
        }

        /// <summary>
        /// Determines whether the given exception requires additional context from the update pipeline (in other
        /// words, whether the exception should be wrapped in an UpdateException).
        /// </summary>
        /// <param name="e">Exception to test.</param>
        /// <returns>true if exception should be wrapped; false otherwise</returns>
        public static bool RequiresContext(this Exception e)
        {
            // if the exception isn't catchable, never wrap
            if (!e.IsCatchableExceptionType())
            {
                return false;
            }

            // update and incompatible provider exceptions already contain the necessary context
            return !(e is UpdateException) && !(e is ProviderIncompatibleException);
        }
    }
}
