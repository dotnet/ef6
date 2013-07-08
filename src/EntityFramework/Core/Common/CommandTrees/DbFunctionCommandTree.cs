// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.CommandTrees
{
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Data.Entity.Core.Common.CommandTrees.Internal;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>Represents the invocation of a database function. </summary>
    public sealed class DbFunctionCommandTree : DbCommandTree
    {
        private readonly EdmFunction _edmFunction;
        private readonly TypeUsage _resultType;
        private readonly ReadOnlyCollection<string> _parameterNames;
        private readonly ReadOnlyCollection<TypeUsage> _parameterTypes;

        /// <summary>
        ///     Constructs a new DbFunctionCommandTree that uses the specified metadata workspace, data space and function metadata
        /// </summary>
        /// <param name="metadata"> The metadata workspace that the command tree should use. </param>
        /// <param name="dataSpace"> The logical 'space' that metadata in the expressions used in this command tree must belong to. </param>
        /// <param name="edmFunction"> </param>
        /// <param name="resultType"> </param>
        /// <param name="parameters"> </param>
        /// <exception cref="ArgumentNullException">
        ///     <paramref name="metadata" />, <paramref name="dataSpace" /> or <paramref name="edmFunction" /> is null
        /// </exception>
        /// <exception cref="ArgumentException">
        ///     <paramref name="dataSpace" /> does not represent a valid data space or <paramref name="edmFunction" />
        ///     is a composable function
        /// </exception>
        [SuppressMessage("StyleCop.CSharp.DocumentationRules", "SA1614:ElementParameterDocumentationMustHaveText")]
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures")]
        public DbFunctionCommandTree(
            MetadataWorkspace metadata, DataSpace dataSpace, EdmFunction edmFunction, TypeUsage resultType,
            IEnumerable<KeyValuePair<string, TypeUsage>> parameters)
            : base(metadata, dataSpace)
        {
            Check.NotNull(edmFunction, "edmFunction");

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
        ///     Gets the <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmFunction" /> that represents the function that is being invoked.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Core.Metadata.Edm.EdmFunction" /> that represents the function that is being invoked.
        /// </returns>
        public EdmFunction EdmFunction
        {
            get { return _edmFunction; }
        }

        /// <summary>Gets the expected result type for the function’s first result set.</summary>
        /// <returns>The expected result type for the function’s first result set.</returns>
        public TypeUsage ResultType
        {
            get { return _resultType; }
        }

        /// <summary>Gets or sets the command tree kind.</summary>
        /// <returns>The command tree kind.</returns>
        public override DbCommandTreeKind CommandTreeKind
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
