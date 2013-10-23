// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Tools.XmlDesignerBase.Base.Util
{
    using System.Globalization;
    using System.IO;
    using System.Text;

    internal class Utf8StringWriter : StringWriter
    {
        public Utf8StringWriter()
            : base(CultureInfo.CurrentCulture)
        {
        }

        public override Encoding Encoding
        {
            get { return Encoding.UTF8; }
        }
    }
}
