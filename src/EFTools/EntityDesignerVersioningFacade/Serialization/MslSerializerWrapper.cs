// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VersioningFacade.Serialization
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Xml;

    /// <summary>
    /// Allows using MslSerializer from EntityFramework.dll from assemblies that have not been granted
    /// the permission to access internal members of EntityFramework.dll.
    /// </summary>
    internal class MslSerializerWrapper
    {
        private readonly MslSerializer _serializer = new MslSerializer();

        public bool Serialize(DbModel edmModel, XmlWriter xmlWriter)
        {
            return _serializer.Serialize(edmModel.DatabaseMapping, xmlWriter);
        }
    }
}
