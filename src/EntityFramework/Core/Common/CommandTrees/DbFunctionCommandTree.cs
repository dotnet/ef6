namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// Represents a function invocation expressed as a canonical command tree
    /// </summary>
    public sealed class DbFunctionCommandTree : DbCommandTree
    {
        private readonly EdmFunction _edmFunction;
        private readonly TypeUsage _resultType;
        private readonly ReadOnlyCollection<string> _parameterNames;
        private readonly ReadOnlyCollection<TypeUsage> _parameterTypes;

        /// <summary>
        /// Constructs a new DbFunctionCommandTree that uses the specified metadata workspace, data space and function metadata
        /// </summary>
        /// <param name="metadata">The metadata workspace that the command tree should use.</param>
        /// <param name="dataSpace">The logical 'space' that metadata in the expressions used in this command tree must belong to.</param>
        /// <param name="edmFunction"></param>
        /// <param name="resultType"></param>
        /// <param name="parameters"></param>
        /// <exception cref="ArgumentNullException"><paramref name="metadata"/>, <paramref name="dataSpace"/> or <paramref name="edmFunction"/> is null</exception>
        /// <exception cref="ArgumentException"><paramref name="dataSpace"/> does not represent a valid data space or
        /// <paramref name="edmFunction">is a composable function</paramref></exception>
        /*CQT_PUBLIC_API(*/
        internal /*)*/ DbFunctionCommandTree(
            MetadataWorkspace metadata, DataSpace dataSpace, EdmFunction edmFunction, TypeUsage resultType,
            IEnumerable<KeyValuePair<string, TypeUsage>> parameters)
            : base(metadata, dataSpace)
        {
            EntityUtil.CheckArgumentNull(edmFunction, "edmFunction");

            _edmFunction = edmFunction;
            _resultType = resultType;

            var paramNames = new List<string>();
            var paramTypes = new List<TypeUsage>();
            if (parameters != null)
            {
                foreach (var paramInfo in parameters)
                {
                    paramNames.Add(paramInfo.Key);
                    paramTypes.Add(paramInfo.Value);
                }
            }

            _parameterNames = paramNames.AsReadOnly();
            _parameterTypes = paramTypes.AsReadOnly();
        }

        /// <summary>
        /// Gets the <see cref="EdmFunction"/> that represents the function to invoke
        /// </summary>
        public EdmFunction EdmFunction
        {
            get { return _edmFunction; }
        }

        /// <summary>
        /// Gets the result type of the function; currently constrained to be a Collection of
        /// RowTypes. Unlike typical RowType instance, merely indicates name/type not parameter
        /// order.
        /// </summary>
        public TypeUsage ResultType
        {
            get { return _resultType; }
        }

        internal override DbCommandTreeKind CommandTreeKind
        {
            get { return DbCommandTreeKind.Function; }
        }

        internal override IEnumerable<KeyValuePair<string, TypeUsage>> GetParameters()
        {
            for (var idx = 0; idx < _parameterNames.Count; idx++)
            {
                yield return new KeyValuePair<string, TypeUsage>(_parameterNames[idx], _parameterTypes[idx]);
            }
        }

        internal override void DumpStructure(ExpressionDumper dumper)
        {
            if (EdmFunction != null)
            {
                dumper.Dump(EdmFunction);
            }
        }

        internal override string PrintTree(ExpressionPrinter printer)
        {
            return printer.Print(this);
        }
    }
}
