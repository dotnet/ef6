// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.UpdateFromDatabase
{
    using System.Collections.Generic;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.Data.Tools.XmlDesignerBase.Common.Diagnostics;

    /// <summary>
    ///     The identity of a given C-side EntityType when compared to
    ///     database objects which is given by the set of tables/views
    ///     to which it maps.
    /// </summary>
    internal class EntityTypeIdentity
    {
        // a sorted list of the underlying database objects (tables or views)
        // which this identity represents
        private readonly SortedList<DatabaseObject, int> _tablesAndViews =
            new SortedList<DatabaseObject, int>(new DatabaseObjectComparer());

        internal int Count
        {
            get { return _tablesAndViews.Count; }
        }

        internal IEnumerable<DatabaseObject> TablesAndViews
        {
            get
            {
                foreach (var tableOrView in _tablesAndViews.Keys)
                {
                    yield return tableOrView;
                }
            }
        }

        internal void AddTableOrView(DatabaseObject tableOrView)
        {
            _tablesAndViews.Add(tableOrView, 0);
        }

        public override bool Equals(object obj)
        {
            if (null == obj)
            {
                return false;
            }

            var objAsEntityTypeIdentity = obj as EntityTypeIdentity;
            if (null == objAsEntityTypeIdentity)
            {
                return false;
            }

            if (Count != objAsEntityTypeIdentity.Count)
            {
                return false;
            }

            if (Count == 0)
            {
                return true;
            }

            var tablesAndViews = TablesAndViews.GetEnumerator();
            var objTablesAndViews = objAsEntityTypeIdentity.TablesAndViews.GetEnumerator();
            while (tablesAndViews.MoveNext()
                   && objTablesAndViews.MoveNext())
            {
                var tableOrView = tablesAndViews.Current;
                var objTableOrView = objTablesAndViews.Current;
                if (!tableOrView.Equals(objTableOrView))
                {
                    return false;
                }
            }

            return true;
        }

        public override int GetHashCode()
        {
            var hashCode = 0;
            foreach (var tableOrView in TablesAndViews)
            {
                hashCode ^= tableOrView.GetHashCode();
            }

            return hashCode;
        }

        internal bool ContainsDatabaseObject(DatabaseObject dbObj)
        {
            return _tablesAndViews.ContainsKey(dbObj);
        }

        internal string TraceString()
        {
            var sb = new StringBuilder("[" + typeof(EntityTypeIdentity).Name);
            sb.Append(
                " " + EFToolsTraceUtils.FormatNamedEnumerable(
                    "tablesAndViews", _tablesAndViews.Keys, delegate(DatabaseObject dbObj) { return dbObj.ToString(); }));

            return sb.ToString();
        }
    }
}
