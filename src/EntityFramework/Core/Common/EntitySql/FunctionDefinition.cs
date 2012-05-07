namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Diagnostics;

    /// <summary>
    /// Entity SQL query inline function definition, returned as a part of <see cref="ParseResult"/>.
    /// </summary>
    public sealed class FunctionDefinition
    {
        private readonly string _name;
        private readonly DbLambda _lambda;
        private readonly int _startPosition;
        private readonly int _endPosition;

        internal FunctionDefinition(string name, DbLambda lambda, int startPosition, int endPosition)
        {
            Debug.Assert(name != null, "name can not be null");
            Debug.Assert(lambda != null, "lambda cannot be null");

            _name = name;
            _lambda = lambda;
            _startPosition = startPosition;
            _endPosition = endPosition;
        }

        /// <summary>
        /// Function name.
        /// </summary>
        public string Name
        {
            get { return _name; }
        }

        /// <summary>
        /// Function body and parameters.
        /// </summary>
        public DbLambda Lambda
        {
            get { return _lambda; }
        }

        /// <summary>
        /// Start position of the function definition in the eSQL query text.
        /// </summary>
        public int StartPosition
        {
            get { return _startPosition; }
        }

        /// <summary>
        /// End position of the function definition in the eSQL query text.
        /// </summary>
        public int EndPosition
        {
            get { return _endPosition; }
        }
    }
}