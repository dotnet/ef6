// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using md = System.Data.Entity.Core.Metadata.Edm;

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Diagnostics.CodeAnalysis;

    /// <summary>
    /// Helper class for creating a ProviderCommandInfo given an Iqt Node.
    /// </summary>
    internal static class ProviderCommandInfoUtils
    {
        #region Public Methods

        /// <summary>
        /// Creates a ProviderCommandInfo for the given node.
        /// This method should be called when the keys, foreign keys and sort keys are known ahead of time.
        /// Typically it is used when the original command is factored into multiple commands.
        /// </summary>
        /// <param name="command"> The owning command, used for creating VarVecs, etc </param>
        /// <param name="node"> The root of the sub-command for which a ProviderCommandInfo should be generated </param>
        /// <returns> The resulting ProviderCommandInfo </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "rowtype")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        internal static ProviderCommandInfo Create(
            Command command,
            Node node)
        {
            var projectOp = node.Op as PhysicalProjectOp;
            PlanCompiler.Assert(projectOp != null, "Expected root Op to be a physical Project");

            // build up the CQT
            var ctree = CTreeGenerator.Generate(command, node);
            var cqtree = ctree as DbQueryCommandTree;
            PlanCompiler.Assert(cqtree != null, "null query command tree");

            // Get the rowtype for the result cqt
            var collType = TypeHelpers.GetEdmType<md.CollectionType>(cqtree.Query.ResultType);
            PlanCompiler.Assert(md.TypeSemantics.IsRowType(collType.TypeUsage), "command rowtype is not a record");

            // Build up a mapping from Vars to the corresponding output property/column
            var outputVarMap = BuildOutputVarMap(projectOp, collType.TypeUsage);

            return new ProviderCommandInfo(ctree);
        }

        #endregion

        #region Private Methods

        /// <summary>
        /// Build up a mapping from Vars to the corresponding property of the output row type
        /// </summary>
        /// <param name="projectOp"> the physical projectOp </param>
        /// <param name="outputType"> output type </param>
        /// <returns> a map from Vars to the output type member </returns>
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "RowType")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "PhysicalProjectOp")]
        [SuppressMessage("Microsoft.Globalization", "CA1303:Do not pass literals as localized parameters",
            MessageId = "System.Data.Entity.Core.Query.PlanCompiler.PlanCompiler.Assert(System.Boolean,System.String)")]
        private static Dictionary<Var, md.EdmProperty> BuildOutputVarMap(PhysicalProjectOp projectOp, md.TypeUsage outputType)
        {
            var outputVarMap = new Dictionary<Var, md.EdmProperty>();

            PlanCompiler.Assert(md.TypeSemantics.IsRowType(outputType), "PhysicalProjectOp result type is not a RowType?");

            IEnumerator<md.EdmProperty> propertyEnumerator = TypeHelpers.GetEdmType<md.RowType>(outputType).Properties.GetEnumerator();
            IEnumerator<Var> varEnumerator = projectOp.Outputs.GetEnumerator();
            while (true)
            {
                var foundProp = propertyEnumerator.MoveNext();
                var foundVar = varEnumerator.MoveNext();
                if (foundProp != foundVar)
                {
                    throw EntityUtil.InternalError(EntityUtil.InternalErrorCode.ColumnCountMismatch, 1, null);
                }
                if (!foundProp)
                {
                    break;
                }
                outputVarMap[varEnumerator.Current] = propertyEnumerator.Current;
            }
            return outputVarMap;
        }

        #endregion
    }
}
