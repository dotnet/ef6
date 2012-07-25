// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.CommandTrees;
    using System.Data.Entity.Core.Metadata.Edm;

    /// <summary>
    /// A Join symbol is a special kind of Symbol.
    /// It has to carry additional information
    /// <list type="bullet">
    /// <item>ColumnList for the list of columns in the select clause if this
    /// symbol represents a sql select statement.  This is set by <see cref="SqlGenerator.AddDefaultColumns"/>. </item>
    /// <item>ExtentList is the list of extents in the select clause.</item>
    /// <item>FlattenedExtentList - if the Join has multiple extents flattened at the 
    /// top level, we need this information to ensure that extent aliases are renamed
    /// correctly in <see cref="SqlSelectStatement.WriteSql"/></item>
    /// <item>NameToExtent has all the extents in ExtentList as a dictionary.
    /// This is used by <see cref="SqlGenerator.Visit(DbPropertyExpression)"/> to flatten
    /// record accesses.</item>
    /// <item>IsNestedJoin - is used to determine whether a JoinSymbol is an 
    /// ordinary join symbol, or one that has a corresponding SqlSelectStatement.</item>
    /// </list>
    /// 
    /// All the lists are set exactly once, and then used for lookups/enumerated.
    /// </summary>
    internal sealed class JoinSymbol : Symbol
    {
        private List<Symbol> columnList;

        internal List<Symbol> ColumnList
        {
            get
            {
                if (null == columnList)
                {
                    columnList = new List<Symbol>();
                }
                return columnList;
            }
            set { columnList = value; }
        }

        private readonly List<Symbol> extentList;

        internal List<Symbol> ExtentList
        {
            get { return extentList; }
        }

        private List<Symbol> flattenedExtentList;

        internal List<Symbol> FlattenedExtentList
        {
            get
            {
                if (null == flattenedExtentList)
                {
                    flattenedExtentList = new List<Symbol>();
                }
                return flattenedExtentList;
            }
            set { flattenedExtentList = value; }
        }

        private readonly Dictionary<string, Symbol> nameToExtent;

        internal Dictionary<string, Symbol> NameToExtent
        {
            get { return nameToExtent; }
        }

        internal bool IsNestedJoin { get; set; }

        public JoinSymbol(string name, TypeUsage type, List<Symbol> extents)
            : base(name, type)
        {
            extentList = new List<Symbol>(extents.Count);
            nameToExtent = new Dictionary<string, Symbol>(extents.Count, StringComparer.OrdinalIgnoreCase);
            foreach (var symbol in extents)
            {
                nameToExtent[symbol.Name] = symbol;
                ExtentList.Add(symbol);
            }
        }
    }
}
