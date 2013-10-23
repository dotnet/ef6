// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.Data.Sql
{
    using System;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Database;
    using Microsoft.VisualStudio.Data.Services;
    using Microsoft.VisualStudio.Data.Services.SupportEntities;

    internal class DataSchemaObject : IDataSchemaObject
    {
        protected DataSchemaServer _server;
        private readonly IVsDataMappedObject _mappedObject;
        private string _displayName;
        private string _quotedShortName;
        private string _shortName;

        public DataSchemaObject(DataSchemaServer server, IVsDataMappedObject mappedObject)
        {
            if (server == null)
            {
                throw new ArgumentNullException("server");
            }
            if (mappedObject == null)
            {
                throw new ArgumentNullException("mappedObject");
            }

            _server = server;
            _mappedObject = mappedObject;
        }

        #region IDataSchemaObject Members

        public IVsDataMappedObject MappedObject
        {
            get { return _mappedObject; }
        }

        public IVsDataObjectIdentifier Identifier
        {
            get { return _mappedObject.UnderlyingObject.Identifier; }
        }

        public DataSchemaServer Server
        {
            get { return _server; }
        }

        public string ShortName
        {
            get
            {
                if (_shortName == null)
                {
                    var parts = Identifier.ToArray();
                    var length = parts.Length;

                    Debug.Assert(length > 0, "Identifier must have at least one part");

                    if (length >= 2)
                    {
                        _shortName = parts[length - 2] + "." + parts[length - 1];
                    }
                    else if (length == 1)
                    {
                        _shortName = parts[0].ToString();
                    }
                }

                return _shortName;
            }
        }

        public string QuotedShortName
        {
            get
            {
                if (_quotedShortName == null)
                {
                    var parts = Identifier.ToArray();
                    object[] importantParts = null;
                    var length = parts.Length;

                    Debug.Assert(length > 0, "Identifier must have at least one part");

                    if (length >= 2)
                    {
                        importantParts = new[] { parts[length - 2], parts[length - 1] };
                    }
                    else if (length == 1)
                    {
                        importantParts = new[] { parts[0] };
                    }

                    if (importantParts != null)
                    {
                        var iConv =
                            (_server.Connection).GetService(typeof(IVsDataObjectIdentifierConverter)) as IVsDataObjectIdentifierConverter;
                        Debug.Assert(iConv != null, "Cannot get identifier converter");
                        if (iConv != null)
                        {
                            _quotedShortName = iConv.ConvertToString(
                                _mappedObject.UnderlyingObject.Type.Name, importantParts, DataObjectIdentifierFormat.WithQuotes);
                        }
                    }
                }

                return _quotedShortName;
            }
        }

        public string DisplayName
        {
            get
            {
                if (_displayName == null)
                {
                    _displayName = Identifier.ToString(DataObjectIdentifierFormat.ForDisplay);
                }

                return _displayName;
            }
        }

        public string Name
        {
            get { return _mappedObject.Name; }
        }

        #endregion
    }
}
