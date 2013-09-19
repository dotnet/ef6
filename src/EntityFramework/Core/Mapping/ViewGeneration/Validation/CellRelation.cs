// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.Validation
{
    using System.Data.Entity.Core.Common.Utils;

    // Abstract class representing a relation signature for a cell query
    internal abstract class CellRelation : InternalBase
    {
        // effects: Given a cell number (for debugging purposes), creates a
        // cell relation 
        protected CellRelation(int cellNumber)
        {
            m_cellNumber = cellNumber;
        }

        internal int m_cellNumber; // The number of the cell for which this
        // relation was made (for debugging) 

        internal int CellNumber
        {
            get { return m_cellNumber; }
        }

        protected abstract int GetHash();
    }
}
