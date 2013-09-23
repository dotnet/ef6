// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.PlanCompiler
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Query.InternalTrees;

    // <summary>
    // Represents information about a collection typed Var.
    // Each such Var is replaced by a Var with a new "mapped" type - the "mapped" type
    // is simply a collection type where the element type has been "mapped"
    // </summary>
    internal class CollectionVarInfo : VarInfo
    {
        private readonly List<Var> m_newVars; // always a singleton list

        // <summary>
        // Create a CollectionVarInfo
        // </summary>
        internal CollectionVarInfo(Var newVar)
        {
            m_newVars = new List<Var>();
            m_newVars.Add(newVar);
        }

        // <summary>
        // Get the newVar
        // </summary>
        internal Var NewVar
        {
            get { return m_newVars[0]; }
        }

        // <summary>
        // Gets <see cref="VarInfoKind" /> for this <see cref="VarInfo" />. Always <see cref="VarInfoKind.CollectionVarInfo" />.
        // </summary>
        internal override VarInfoKind Kind
        {
            get { return VarInfoKind.CollectionVarInfo; }
        }

        // <summary>
        // Get the list of all NewVars - just one really
        // </summary>
        internal override List<Var> NewVars
        {
            get { return m_newVars; }
        }
    }
}
