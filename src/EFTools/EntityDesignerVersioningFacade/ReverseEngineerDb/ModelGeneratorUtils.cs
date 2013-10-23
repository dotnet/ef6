// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb
{
    using System.Diagnostics;

    internal class ModelGeneratorUtils
    {
        internal static string CreateValidEcmaName(string name, char appendToFrontIfFirstCharIsInvalid)
        {
            Debug.Assert(name != null, "name != null");
            Debug.Assert(char.IsLetter(appendToFrontIfFirstCharIsInvalid), "invalid ECMA starting character");

            var ecmaNameArray = name.ToCharArray();
            for (var i = 0; i < ecmaNameArray.Length; i++)
            {
                // replace non -(letters or digits) with _ ( underscore )
                if (!char.IsLetterOrDigit(ecmaNameArray[i]))
                {
                    ecmaNameArray[i] = '_';
                }
            }

            var ecmaName = new string(ecmaNameArray);
            // the first letter in a part should only be a char
            // if the part is empty then implies that we have the situation like ".abc", "abc.", "ab..c", 
            // neither of them are accepted by the schema
            if (string.IsNullOrEmpty(name)
                || !char.IsLetter(ecmaName[0]))
            {
                ecmaName = appendToFrontIfFirstCharIsInvalid + ecmaName;
            }

            return ecmaName;
        }
    }
}
