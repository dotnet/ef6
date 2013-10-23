using System;
using System.Diagnostics;
using Microsoft.VisualStudio.TestTools.Common;
using Microsoft.Win32;
using System.Collections.Generic;
using System.IO;
using System.Text.RegularExpressions;
using System.Diagnostics.CodeAnalysis;
using System.Globalization;

namespace Microsoft.VisualStudio.TestTools.HostAdapters.VsIde
{
    /// <summary>
    /// Compares VS Registry Hive versions, such as 9.0 and 10.0.
    /// </summary>
    internal class VsVersionComparer : IComparer<string>
    {
        private static StringComparer s_stringComparer = StringComparer.InvariantCultureIgnoreCase;

        /// <summary>
        /// Defines format of VS version in registry that we understand.
        /// Note that we ignore non-default versions, such as [VersionMajor].[VersionMinor]Exp
        /// </summary>
        internal static readonly Regex VsVersionRegex = new Regex(@"^[0-9]+\.[0-9]+$");

        #region IComparer<string> Members
        /// <summary>
        /// Compares VS Versions, such as 9.0 to 10.1 using number comparison. 
        /// The arguments must match VsVersionRegex. If they do not, the method does string comparison.
        /// Anyhow, if there's any error to parse the number, the method does string comparison.
        /// </summary>
        public int Compare(string x, string y)
        {
            if (x == null || y == null ||
                !VsVersionRegex.Match(x).Success || !VsVersionRegex.Match(y).Success)
            {
                return s_stringComparer.Compare(x, y);
            }

            string[] xParts = x.Split(new char[] { '.' }, StringSplitOptions.None);
            string[] yParts = y.Split(new char[] { '.' }, StringSplitOptions.None);

            if (xParts.Length != yParts.Length)
            {
                return s_stringComparer.Compare(x, y);
            }

            try
            {
                for (int i = 0; i < xParts.Length; ++i)
                {
                    int xPart = int.Parse(xParts[i], NumberStyles.None, CultureInfo.InvariantCulture);
                    int yPart = int.Parse(yParts[i], NumberStyles.None, CultureInfo.InvariantCulture);
                    if (xPart != yPart)
                    {
                        return xPart < yPart ? -1 : 1;
                    }
                }
            }
            catch (SystemException)
            {
                return s_stringComparer.Compare(x, y);
            }

            return 0;
        }
        #endregion
    }
}
