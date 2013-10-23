// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Text;

    internal class EFDependencyGraph : DependencyGraph<EFObject>
    {
        #region Test Only Code

        /// <summary>
        ///     Return a sorted string representation of the contents of this DependencyGraph
        /// </summary>
        internal string ToPrettyString()
        {
#if TRACK_DEPENDENCIES
            List<string> dependencies = new List<string>();
            foreach (EFObject item in _dependencyMap.Keys)
            {
                string line = item.ToPrettyString() + " :: ";
                line += DepsToSortedString(GetDependencies(item));
                line += "\r\n";
                dependencies.Add(line);
            }
            dependencies.Sort();
#endif

            var antideps = new List<string>();
            foreach (var item in _antiDependencyMap.Keys)
            {
                var line = item.ToPrettyString() + " :: ";
                line += DepsToSortedString(GetAntiDependencies(item));
                line += "\r\n";
                antideps.Add(line);
            }
            antideps.Sort();

            var stringBuffer = new StringBuilder();

#if TRACK_DEPENDENCIES
            stringBuffer.Append("\r\nDependencies:\r\n");
            stringBuffer.Append("=============\r\n");
            foreach (string dep in dependencies)
            {
                stringBuffer.Append(dep);
            }
#endif

            stringBuffer.Append("\r\nAnti-Dependencies:\r\n");
            stringBuffer.Append("=================\r\n");
            foreach (var antidep in antideps)
            {
                stringBuffer.Append(antidep);
            }

            foreach (var item in _antiDependencyMap.Keys)
            {
                if (item.IsDisposed)
                {
                    stringBuffer.Append(
                        item.ToPrettyString()
                        + " is Disposed, but is still present in the anti-dep map.  It is pointing to the following items:"
                        + Environment.NewLine);

                    var list = _antiDependencyMap[item];
                    foreach (var obj in list)
                    {
                        stringBuffer.Append("\t" + obj.ToPrettyString());
                    }
                    stringBuffer.Append(Environment.NewLine);
                }
            }

#if TRACK_DEPENDENCIES
            foreach (EFObject item in _dependencyMap.Keys)
            {
                if (item.IsDisposed)
                {
                    stringBuffer.Append(item.ToPrettyString() + " is Disposed, but is still present in the dependency map  It is pointing to the following items:" + Environment.NewLine);
                    List<EFObject> list = _dependencyMap[item];
                    foreach (EFObject obj in list)
                    {
                        stringBuffer.Append("\t" + obj.ToPrettyString());
                    }
                    stringBuffer.Append(Environment.NewLine);
                }
            }
#endif

            return stringBuffer.ToString();
        }

        private static string DepsToSortedString(ICollection<EFObject> deps)
        {
            var depStrings = new List<string>();
            foreach (var dep in deps)
            {
                depStrings.Add(dep.ToPrettyString());
            }
            depStrings.Sort();
            var sb = new StringBuilder();
            foreach (var s in depStrings)
            {
                sb.Append(s);
                sb.Append(", ");
            }
            return sb.ToString();
        }

        #endregion
    }
}
