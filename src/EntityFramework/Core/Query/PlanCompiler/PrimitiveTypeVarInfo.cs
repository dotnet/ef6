// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents information about a primitive typed Var and how it can be replaced.
    /// </summary>
    internal class PrimitiveTypeVarInfo : VarInfo
    {
        private readonly List<Var> m_newVars; // always a singleton list

        /// <summary>
        /// Initializes a new instance of <see cref="PrimitiveTypeVarInfo" /> class.
        /// </summary>
        /// <param name="newVar">
        /// New <see cref="Var" /> that replaces current <see cref="Var" /> .
        /// </param>
        internal PrimitiveTypeVarInfo(Var newVar)
        {
            DebugCheck.NotNull(newVar);
            m_newVars = new List<Var>
                {
                    newVar
                };
        }

        /// <summary>
        /// Gets the newVar.
        /// </summary>
        internal Var NewVar
        {
            get { return m_newVars[0]; }
        }

        /// <summary>
        /// Gets <see cref="VarInfoKind" /> for this <see cref="VarInfo" />. Always <see cref="VarInfoKind.CollectionVarInfo" />.
        /// </summary>
        internal override VarInfoKind Kind
        {
            get { return VarInfoKind.PrimitiveTypeVarInfo; }
        }

        /// <summary>
        /// Gets the list of all NewVars. The list contains always just one element.
        /// </summary>
        internal override List<Var> NewVars
        {
            get { return m_newVars; }
        }
    }
}
