// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.Core.Mapping.ViewGeneration.Structures
{
    // A class that abstracts the notion of identifying table mapping
    // fragments or cells, e.g., line numbers, etc
    internal class CellLabel
    {
        #region Constructors

        /// <summary>
        /// Copy Constructor
        /// </summary>
        internal CellLabel(CellLabel source)
        {
            m_startLineNumber = source.m_startLineNumber;
            m_startLinePosition = source.m_startLinePosition;
            m_sourceLocation = source.m_sourceLocation;
        }

        internal CellLabel(StorageMappingFragment fragmentInfo)
            :
                this(fragmentInfo.StartLineNumber, fragmentInfo.StartLinePosition, fragmentInfo.SourceLocation)
        {
        }

        internal CellLabel(int startLineNumber, int startLinePosition, string sourceLocation)
        {
            m_startLineNumber = startLineNumber;
            m_startLinePosition = startLinePosition;
            m_sourceLocation = sourceLocation;
        }

        #endregion

        #region Fields

        private readonly int m_startLineNumber;
        private readonly int m_startLinePosition;
        private readonly string m_sourceLocation;

        #endregion

        #region Properties

        internal int StartLineNumber
        {
            get { return m_startLineNumber; }
        }

        internal int StartLinePosition
        {
            get { return m_startLinePosition; }
        }

        internal string SourceLocation
        {
            get { return m_sourceLocation; }
        }

        #endregion
    }
}
