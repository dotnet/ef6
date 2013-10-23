// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Entity
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.VersioningFacade;

    internal class StorageEntityModel : BaseEntityModel
    {
        // Function elements can exists only in SSDL file
        private readonly List<Function> _functions = new List<Function>();

        private const string DefaultProvider = "System.Data.SqlClient";
        private const string DefaultProviderManifestToken = "2008";

        private DefaultableValue<string> _providerAttr;
        private DefaultableValue<string> _providerManifestTokenAttr;

        private IDictionary<string, PrimitiveType> _storeTypeNameToStoreTypeMap;

        internal StorageEntityModel(EntityDesignArtifact parent, XElement element)
            : base(parent, element)
        {
            if (parent != null)
            {
                parent.StorageModel = this;
            }
        }

        internal IDictionary<string, PrimitiveType> StoreTypeNameToStoreTypeMap
        {
            get
            {
                if (_storeTypeNameToStoreTypeMap == null)
                {
                    var providerManifest =
                        DependencyResolver.GetService<DbProviderServices>(Provider.Value)
                            .GetProviderManifest(ProviderManifestToken.Value);

                    _storeTypeNameToStoreTypeMap =
                        providerManifest
                            .GetStoreTypes()
                            .ToDictionary(t => t.Name, t => t, StringComparer.OrdinalIgnoreCase);
                }
                return _storeTypeNameToStoreTypeMap;
            }
        }

        /// <summary>
        ///     Returns true if this model is based on a CSDL file, false if an SSDL file
        /// </summary>
        public override bool IsCSDL
        {
            get { return false; }
        }

        internal bool IsEmpty
        {
            get
            {
                if (XElement != null)
                {
                    // First check that we have one XElement child and that is the EntityContainer node.
                    var xElementChildren = XElement.Elements();
                    if (xElementChildren.Count() == 1)
                    {
                        var entityContainerXElement = xElementChildren.First();
                        if (entityContainerXElement != null
                            && entityContainerXElement.Name == XNamespace + BaseEntityContainer.ElementName)
                        {
                            // Now let's check that there are no children under the EntityContainer.
                            if (entityContainerXElement.Elements().Count() == 0)
                            {
                                return true;
                            }
                        }
                    }
                }

                return false;
            }
        }

        internal IList<Function> Functions()
        {
            return _functions.AsReadOnly();
        }

        internal void AddFunction(Function function)
        {
            _functions.Add(function);
        }

        /// <summary>
        ///     Returns the string value of the Provider attribute
        /// </summary>
        internal DefaultableValue<string> Provider
        {
            get
            {
                if (_providerAttr == null)
                {
                    // we can safely create these here since we are the top node and don't need to be parsed first
                    _providerAttr = new ProviderDefaultableValue(this);
                }
                return _providerAttr;
            }
        }

        private class ProviderDefaultableValue : DefaultableValue<string>
        {
            private const string AttributeProvider = "Provider";

            internal ProviderDefaultableValue(EFElement parent)
                : base(parent, AttributeProvider)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeProvider; }
            }

            public override string DefaultValue
            {
                get { return DefaultProvider; }
            }
        }

        /// <summary>
        ///     Returns the string value of the ProviderManifestToken attribute
        /// </summary>
        internal DefaultableValue<string> ProviderManifestToken
        {
            get
            {
                if (_providerManifestTokenAttr == null)
                {
                    // we can safely create these here since we are the top node and don't need to be parsed first
                    _providerManifestTokenAttr = new ProviderManifestTokenDefaultableValue(this);
                }
                return _providerManifestTokenAttr;
            }
        }

        private class ProviderManifestTokenDefaultableValue : DefaultableValue<string>
        {
            private const string AttributeProviderManifestToken = "ProviderManifestToken";

            internal ProviderManifestTokenDefaultableValue(EFElement parent)
                : base(parent, AttributeProviderManifestToken)
            {
            }

            internal override string AttributeName
            {
                get { return AttributeProviderManifestToken; }
            }

            public override string DefaultValue
            {
                get { return DefaultProviderManifestToken; }
            }
        }

        // we unfortunately get a warning from the compiler when we use the "base" keyword in "iterator" types generated by using the
        // "yield return" keyword.  By adding this method, I was able to get around this.  Unfortunately, I wasn't able to figure out
        // a way to implement this once and have derived classes share the implementation (since the "base" keyword is resolved at 
        // compile-time and not at runtime.
        private IEnumerable<EFObject> BaseChildren
        {
            get { return base.Children; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                foreach (var efobj in BaseChildren)
                {
                    yield return efobj;
                }

                yield return Provider;
                yield return ProviderManifestToken;

                foreach (var fun in _functions)
                {
                    yield return fun;
                }
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var child = efContainer as Function;
            if (child != null)
            {
                _functions.Remove(child);
                return;
            }

            base.OnChildDeleted(efContainer);
        }

#if DEBUG
        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(Function.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_providerAttr);
            _providerAttr = null;

            ClearEFObject(_providerManifestTokenAttr);
            _providerManifestTokenAttr = null;

            // clear this here instead of in base class, since we populate it here
            ClearEFObjectCollection(_entityContainers);
            ClearEFObjectCollection(_functions);
            base.PreParse();
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            // Storage EntityModel needs to create Storage EntityContainer objects
            if (elem.Name.LocalName == BaseEntityContainer.ElementName)
            {
                var sec = new StorageEntityContainer(this, elem);
                _entityContainers.Add(sec);
                sec.Parse(unprocessedElements);
            }
                // Function element can exists only in SSDL file
            else if (elem.Name.LocalName == Function.ElementName)
            {
                var fun = new Function(this, elem);
                _functions.Add(fun);
                fun.Parse(unprocessedElements);
            }
            else
            {
                return base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        internal override XNamespace XNamespace
        {
            get
            {
                // if for some reason the XElement backing this model node doesn't exist yet, we'll use the 
                // desired namespace for the EF version of the artifact. 
                return XElement != null
                           ? XElement.Name.Namespace
                           : SchemaManager.GetSSDLNamespaceName(
                               SchemaManager.GetSchemaVersion(Artifact.XDocument.Root.Name.Namespace));
            }
        }

        internal PrimitiveType GetStoragePrimitiveType(string typeName)
        {
            PrimitiveType primType;
            StoreTypeNameToStoreTypeMap.TryGetValue(typeName, out primType);
            return primType;
        }
    }
}
