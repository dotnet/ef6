// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using md = System.Data.Entity.Core.Metadata.Edm;

//
// The CodeGen module is responsible for translating the ITree finally into a query
//  We assume that various tree transformations have taken place, and the tree
// is finally ready to be executed. The CodeGen module
//   * converts the Itree into one or more CTrees (in S space)
//   * produces a ColumnMap to facilitate result assembly
//   * and wraps up everything in a plan object
//
// 

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    internal class CodeGen
    {
        #region public methods

        /// <summary>
        /// This involves
        /// * Converting the ITree into a set of ProviderCommandInfo objects
        /// * Creating a column map to enable result assembly
        /// Currently, we only produce a single ITree, and correspondingly, the
        /// following steps are trivial
        /// </summary>
        /// <param name="compilerState"> current compiler state </param>
        /// <param name="childCommands"> CQTs for each store command </param>
        /// <param name="resultColumnMap"> column map to help in result assembly </param>
        internal static void Process(
            PlanCompiler compilerState, out List<ProviderCommandInfo> childCommands, out ColumnMap resultColumnMap, out int columnCount)
        {
            var codeGen = new CodeGen(compilerState);
            codeGen.Process(out childCommands, out resultColumnMap, out columnCount);
        }

        #endregion

        #region constructors

        private CodeGen(PlanCompiler compilerState)
        {
            m_compilerState = compilerState;
        }

        #endregion

        #region private methods

        /// <summary>
        /// The real driver. This routine walks the tree, converts each subcommand
        /// into a CTree, and converts the columnmap into a real column map.
        /// Finally, it produces a "real" plan that can be used by the bridge execution, and
        /// returns this plan
        /// The root of the tree must be a PhysicalProjectOp. Each child of this Op
        /// represents a command to be executed, and the ColumnMap of this Op represents
        /// the eventual columnMap to be used for result assembly
        /// </summary>
        /// <param name="childCommands"> CQTs for store commands </param>
        /// <param name="resultColumnMap"> column map for result assembly </param>
        private void Process(out List<ProviderCommandInfo> childCommands, out ColumnMap resultColumnMap, out int columnCount)
        {
            var projectOp = (PhysicalProjectOp)Command.Root.Op;

            m_subCommands = new List<Node>(new[] { Command.Root });
            childCommands = new List<ProviderCommandInfo>(
                new[]
                    {
                        ProviderCommandInfoUtils.Create(
                            Command,
                            Command.Root // input node
                            )
                    });

            // Build the final column map, and count the columns we expect for it.
            resultColumnMap = BuildResultColumnMap(projectOp);

            columnCount = projectOp.Outputs.Count;
        }

        private ColumnMap BuildResultColumnMap(PhysicalProjectOp projectOp)
        {
            // convert the column map into a real column map
            // build up a dictionary mapping Vars to their real positions in the commands
            var varMap = BuildVarMap();
            var realColumnMap = ColumnMapTranslator.Translate(projectOp.ColumnMap, varMap);

            return realColumnMap;
        }

        /// <summary>
        /// For each subcommand, build up a "location-map" for each top-level var that
        /// is projected out. This location map will ultimately be used to convert VarRefColumnMap
        /// into SimpleColumnMap
        /// </summary>
        private Dictionary<Var, KeyValuePair<int, int>> BuildVarMap()
        {
            var varMap =
                new Dictionary<Var, KeyValuePair<int, int>>();

            var commandId = 0;
            foreach (var subCommand in m_subCommands)
            {
                var projectOp = (PhysicalProjectOp)subCommand.Op;

                var columnPos = 0;
                foreach (var v in projectOp.Outputs)
                {
                    var varLocation = new KeyValuePair<int, int>(commandId, columnPos);
                    varMap[v] = varLocation;
                    columnPos++;
                }

                commandId++;
            }
            return varMap;
        }

        #endregion

        #region private state

        private readonly PlanCompiler m_compilerState;

        private Command Command
        {
            get { return m_compilerState.Command; }
        }

        private List<Node> m_subCommands;

        #endregion
    }
}
