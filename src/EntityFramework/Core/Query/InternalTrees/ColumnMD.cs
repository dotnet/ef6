// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Query.InternalTrees
{
    using System.Data.Entity.Core.Metadata.Edm;

    // <summary>
    // Describes information about each column
    // </summary>
    internal class ColumnMD
    {
        private readonly string m_name;
        private readonly TypeUsage m_type;
        private readonly EdmMember m_property;

        // <summary>
        // Default constructor
        // </summary>
        // <param name="name"> Column name </param>
        // <param name="type"> Datatype of the column </param>
        internal ColumnMD(string name, TypeUsage type)
        {
            m_name = name;
            m_type = type;
        }

        // <summary>
        // More useful default constructor
        // </summary>
        // <param name="property"> property describing this column </param>
        internal ColumnMD(EdmMember property)
            : this(property.Name, property.TypeUsage)
        {
            m_property = property;
        }

        // <summary>
        // Column Name
        // </summary>
        internal string Name
        {
            get { return m_name; }
        }

        // <summary>
        // Datatype of the column
        // </summary>
        internal TypeUsage Type
        {
            get { return m_type; }
        }

        // <summary>
        // Is this column nullable ?
        // </summary>
        internal bool IsNullable
        {
            get { return (m_property == null) || TypeSemantics.IsNullable(m_property); }
        }

        // <summary>
        // debugging help
        // </summary>
        public override string ToString()
        {
            return m_name;
        }
    }
}
