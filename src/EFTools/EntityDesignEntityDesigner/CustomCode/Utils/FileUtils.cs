// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Utils
{
    using System.Diagnostics.CodeAnalysis;
    using System.IO;
    using System.Text;

    internal static class FileUtils
    {
        // NOTE: Returned Stream should be disposed when done!
        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        public static Stream StringToStream(string inputContents, Encoding encoding)
        {
            //MDF Serialization requires the preamble
            var preamble = encoding.GetPreamble();
            var body = encoding.GetBytes(inputContents);

            var stream = new MemoryStream(preamble.Length + body.Length);

            stream.Write(preamble, 0, preamble.Length);
            stream.Write(body, 0, body.Length);
            stream.Position = 0;

            return stream;
        }
    }
}
