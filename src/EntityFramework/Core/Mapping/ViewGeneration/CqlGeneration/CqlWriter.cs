// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration.CqlGeneration
{
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Text;
    using System.Text.RegularExpressions;

    // This class contains helper methods needed for generating Cql
    internal static class CqlWriter
    {
        private static readonly Regex _wordIdentifierRegex = new Regex(@"^[_A-Za-z]\w*$", RegexOptions.ECMAScript | RegexOptions.Compiled);

        // effects: Given a block name and a field in it -- returns a string
        // of form "blockName.field". Does not perform any escaping
        internal static string GetQualifiedName(string blockName, string field)
        {
            var result = StringUtil.FormatInvariant("{0}.{1}", blockName, field);
            return result;
        }

        // effects: Modifies builder to contain an escaped version of type's name as "[namespace.typename]"
        internal static void AppendEscapedTypeName(StringBuilder builder, EdmType type)
        {
            AppendEscapedName(builder, GetQualifiedName(type.NamespaceName, type.Name));
        }

        // effects: Modifies builder to contain an escaped version of "name1.name2" as "[name1].[name2]"
        internal static void AppendEscapedQualifiedName(StringBuilder builder, string name1, string name2)
        {
            AppendEscapedName(builder, name1);
            builder.Append('.');
            AppendEscapedName(builder, name2);
        }

        // effects: Modifies builder to contain an escaped version of "name"
        internal static void AppendEscapedName(StringBuilder builder, string name)
        {
            if (_wordIdentifierRegex.IsMatch(name)
                && false == ExternalCalls.IsReservedKeyword(name))
            {
                // We do not need to escape the name if it is a simple name and it is not a keyword
                builder.Append(name);
            }
            else
            {
                var newName = name.Replace("]", "]]");
                builder.Append('[')
                       .Append(newName)
                       .Append(']');
            }
        }
    }
}
