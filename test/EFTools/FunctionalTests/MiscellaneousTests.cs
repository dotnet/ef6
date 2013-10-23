// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace EFDesigner.FunctionalTests
{
    using System;
    using System.Xml.Linq;
    using Microsoft.Data.Tools.XmlDesignerBase.Base.Util;
    using Microsoft.Data.Tools.VSXmlDesignerBase.Model.VisualStudio;
    using Microsoft.VisualStudio.TextManager.Interop;
    using Microsoft.VisualStudio.XmlEditor;
    using Xunit;

    public class MiscellaneousTests
    {
        private class XmlModelMock : XmlModel
        {
            private readonly string _name;

            public XmlModelMock(string name)
            {
                _name = name;
            }

            public override event EventHandler BufferReloaded
            {
                add { }
                remove { }
            }

            public override void Dispose()
            {
            }

            public override XDocument Document
            {
                get { throw new NotImplementedException(); }
            }

            public override TextSpan GetTextSpan(XObject node)
            {
                throw new NotImplementedException();
            }

            public override string Name
            {
                get { return _name; }
            }

            public override XmlModelSaveAction SaveActionOnDispose
            {
                get { throw new NotImplementedException(); }
                set { throw new NotImplementedException(); }
            }

            public override XmlStore Store
            {
                get { throw new NotImplementedException(); }
            }
        }

        [Fact]
        public void VSXmlModel_returns_correct_local_path_for_Uri_with_hashes()
        {
            var localPath = @"C:\C# Projects\#pie#";

            Assert.Equal(
                localPath,
                new VSXmlModel(null, new XmlModelMock("file://" + localPath)).Uri.LocalPath);

            Assert.Equal(
                localPath,
                new VSXmlModel(null, new XmlModelMock(localPath)).Uri.LocalPath);
        }

        [Fact]
        public void Filename2Uri_can_handle_file_uris_with_hashes()
        {
            var localPath = @"C:\C# Projects\#pie#";

            Assert.Equal(
                localPath,
                Utils.FileName2Uri("file://" + localPath).LocalPath);

            Assert.Equal(
                localPath,
                Utils.FileName2Uri(localPath).LocalPath);
        }
    }
}
