// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.SqlServerCompact.SqlGen
{
    using System.Data.Entity.SqlServerCompact.Resources;
    using System.IO;
    using System.Text;

    internal static class CommonUtils
    {
        private const string DataDirectoryMacro = "|DataDirectory|";
        private const string DataDirectory = "DataDirectory";

        public static string ReplaceDataDirectory(string inputString)
        {
            var trimmed = inputString.Trim();
            if (!string.IsNullOrEmpty(inputString)
                && inputString.StartsWith(DataDirectoryMacro, StringComparison.OrdinalIgnoreCase))
            {
                var dataDirectoryPath = AppDomain.CurrentDomain.GetData(DataDirectory) as string;
                if (string.IsNullOrEmpty(dataDirectoryPath))
                {
                    dataDirectoryPath = AppDomain.CurrentDomain.BaseDirectory ?? Environment.CurrentDirectory;
                }
                if (string.IsNullOrEmpty(dataDirectoryPath))
                {
                    dataDirectoryPath = string.Empty;
                }

                var length = DataDirectoryMacro.Length;
                if (inputString.Length > DataDirectoryMacro.Length
                    && '\\' == inputString[DataDirectoryMacro.Length])
                {
                    ++length;
                }
                trimmed = Path.Combine(dataDirectoryPath, inputString.Substring(length));
            }
            return trimmed;
        }

        /// <summary>
        ///     Method to add a closing square brace escape for all
        ///     embedded closing square braces in a string
        /// </summary>
        /// <param name="name"> </param>
        /// <returns> </returns>
        public static string EscapeSquareBraceNames(string name)
        {
            return EscapeNames(name, ']');
        }

        /// <summary>
        ///     Routine to replace an interesting character with itself
        ///     (to escape the interesting character)
        /// </summary>
        /// <param name="name"> </param>
        /// <param name="quote"> </param>
        /// <returns> </returns>
        public static string EscapeNames(string name, char quote)
        {
            string outputName;
            var sb = new StringBuilder();

            sb.Append(quote.ToString());
            sb.Append(quote.ToString());

            outputName = name.Replace(quote.ToString(), sb.ToString());
            return outputName;
        }

        /// <summary>
        ///     Delete the database that the fileName points to.
        /// </summary>
        /// <param name="fileName"> Database file path. </param>
        public static void DeleteDatabase(string fileName)
        {
            var expandedFileName = ReplaceDataDirectory(fileName);
            if (!DatabaseExists(expandedFileName))
            {
                throw new InvalidOperationException(EntityRes.GetString(EntityRes.DatabaseDoesNotExist));
            }

            File.Delete(expandedFileName);
        }

        /// <summary>
        ///     Check whether the database pointed to by the file name exists or not.
        /// </summary>
        /// <param name="fileName"> Database file path </param>
        /// <returns> </returns>
        public static bool DatabaseExists(string fileName)
        {
            var expandedFileName = ReplaceDataDirectory(fileName);
            return File.Exists(expandedFileName);
        }
    }
}
