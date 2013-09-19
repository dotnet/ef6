// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.EntitySql
{
    /// <summary>
    /// Represents eSQL compilation options.
    /// </summary>
    internal sealed class ParserOptions
    {
        internal enum CompilationMode
        {
            /// <summary>
            /// Normal mode. Compiles eSQL command without restrictions.
            /// Name resolution is case-insensitive (eSQL default).
            /// </summary>
            NormalMode,

            /// <summary>
            /// View generation mode: optimizes compilation process to ignore uncessary eSQL constructs:
            /// - GROUP BY, HAVING and ORDER BY clauses are ignored.
            /// - WITH RELATIONSHIP clause is allowed in type constructors.
            /// - Name resolution is case-sensitive.
            /// </summary>
            RestrictedViewGenerationMode,

            /// <summary>
            /// Same as CompilationMode.Normal plus WITH RELATIONSHIP clause is allowed in type constructors.
            /// </summary>
            UserViewGenerationMode
        }

        /// <summary>
        /// Sets/Gets eSQL parser compilation mode.
        /// </summary>
        internal CompilationMode ParserCompilationMode;

        internal StringComparer NameComparer
        {
            get { return NameComparisonCaseInsensitive ? StringComparer.OrdinalIgnoreCase : StringComparer.Ordinal; }
        }

        internal bool NameComparisonCaseInsensitive
        {
            get { return ParserCompilationMode == CompilationMode.RestrictedViewGenerationMode ? false : true; }
        }
    }
}
