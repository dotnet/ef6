// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Base.Util
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;
    using System.Xml.Linq;

    internal static class Utils
    {
        [SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal static Uri FileName2Uri(string fileName)
        {
            Uri uri = null;

            if (fileName.StartsWith("file://", StringComparison.OrdinalIgnoreCase))
            {
                Uri.TryCreate(
                    fileName.Replace("#", Uri.HexEscape('#')),
                    UriKind.RelativeOrAbsolute, out uri);
            }
            else
            {
                try
                {
                    var fi = new FileInfo(fileName);
                    Uri.TryCreate(fi.FullName, UriKind.Absolute, out uri);
                }
                catch (Exception e)
                {
                    Debug.WriteLine(e.Message);
                }
            }

            return uri;
        }

        /// <summary>
        ///     Converts a string to a byte array.  The array will start with the encoding's
        ///     preamble (if any).
        /// </summary>
        internal static byte[] StringToBytes(string input, Encoding encoding)
        {
            return StreamToBytes(StringToStream(input, encoding));
        }

        /// <summary>
        ///     Converts a stream to a byte array, restoring the stream's position after reading it.
        /// </summary>
        private static byte[] StreamToBytes(Stream stream)
        {
            if (stream == null
                || stream.Length == 0)
            {
                return new byte[] { };
            }

            // remember current position
            var position = stream.Position;

            // set position to the start
            stream.Position = 0;

            // create array, copy bytes into the array
            var bytes = new byte[(int)stream.Length];
            stream.Read(bytes, 0, bytes.Length);

            // restore position
            stream.Position = position;

            return bytes;
        }

        /// <summary>
        ///     Converts a string into a stream using a certain encoding, also includes
        ///     the Encoding preamble at the start of the steam.
        /// </summary>
        /// <returns>NOTE: Returned Stream should be disposed when done!</returns>
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        private static Stream StringToStream(string inputContents, Encoding encoding)
        {
            // MDF Serialization requires the preamble
            var Preamble = encoding.GetPreamble();
            var Body = encoding.GetBytes(inputContents);

            var stream = new MemoryStream(Preamble.Length + Body.Length);

            stream.Write(Preamble, 0, Preamble.Length);
            stream.Write(Body, 0, Body.Length);
            stream.Position = 0;

            return stream;
        }

        /// <summary>
        ///     this method will format a block of XML based on the specified initial indent level.    It assumes that there is no
        ///     existing whitespace between elements.
        /// </summary>
        internal static void FormatXML(XElement xe, int currentIndentLevel)
        {
            XElement last = null;
            foreach (var c in xe.Elements())
            {
                // add whitespace before this child - this will place each child's start tag on a new line with proper indenting
                var nl = new XText(Environment.NewLine + new string(' ', (currentIndentLevel + 1) * 2));
                c.AddBeforeSelf(nl);

                // recurse on the child
                FormatXML(c, currentIndentLevel + 1);
                last = c;
            }

            // add whitespace after the last child.  This places the closing tag of xe on a new line with proper indenting. 
            if (last != null)
            {
                var nl = new XText(Environment.NewLine + new string(' ', currentIndentLevel * 2));
                last.AddAfterSelf(nl);
            }
        }
    }
}
