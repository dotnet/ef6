// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Migrations.Infrastructure
{
    using System.Data.Entity.Utilities;
    using System.Xml.Linq;

    internal class VersionedModel
    {
        private readonly XDocument _model;
        private readonly string _version;

        public VersionedModel(XDocument model, string version = null)
        {
            DebugCheck.NotNull(model);

            _model = model;
            _version = version;
        }

        public XDocument Model
        {
            get { return _model; }
        }

        public string Version
        {
            get { return _version; }
        }
    }
}
