// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Linq;
    using System.Xml.Linq;
    using System.Xml.Schema;
    using Microsoft.Data.Entity.Design.Model.Designer;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Entity.Design.Model.Visitor;
    using Microsoft.Data.Entity.Design.Model.XLinqAnnotations;
    using Microsoft.Data.Entity.Design.VersioningFacade;
    using Microsoft.Data.Tools.XmlDesignerBase.Model;

    internal class DiagramArtifact : EFArtifact
    {
        private EFDesignerInfoRoot _designerInfoRoot;
        private HashSet<string> _namespaces;

        internal event EventHandler<DiagramElementNameCommittedArgs> ElementNameCommitted;

        /// <summary>
        ///     Constructs an DiagramArtifact for the passed in URI
        /// </summary>
        /// <param name="modelManager">A reference of ModelManager</param>
        /// <param name="uri">The Diagram File URI</param>
        /// <param name="xmlModelProvider">If you pass null, then you must derive from this class and implement CreateModelProvider().</param>
        internal DiagramArtifact(ModelManager modelManager, Uri uri, XmlModelProvider xmlModelProvider)
            : base(modelManager, uri, xmlModelProvider)
        {
        }

        internal void RaiseShapeNameCommitted(string shapeName)
        {
            var handler = ElementNameCommitted;
            if (handler != null)
            {
                handler(this, new DiagramElementNameCommittedArgs(shapeName));
            }
        }

        internal void RaisePropertyNameCommitted(string shapeName, string propertyName)
        {
            var handler = ElementNameCommitted;
            if (handler != null)
            {
                handler(this, new DiagramElementNameCommittedArgs(shapeName, propertyName));
            }
        }

        protected override void Dispose(bool disposing)
        {
            try
            {
                if (disposing)
                {
                    if (_designerInfoRoot != null)
                    {
                        _designerInfoRoot.Dispose();
                        _designerInfoRoot = null;
                    }
                }
            }
            finally
            {
                base.Dispose(disposing);
            }
        }

        internal override void Parse(ICollection<XName> unprocessedElements)
        {
            State = EFElementState.ParseAttempted;

            if (_designerInfoRoot != null)
            {
                _designerInfoRoot.Dispose();
                _designerInfoRoot = null;
            }

            // convert the xlinq tree to our model
            var elem = XObject.Document.Elements().FirstOrDefault();

            if (elem != null)
            {
                Debug.Assert(elem.Name.LocalName == "Edmx", "Incorrect element name Expected: Edmx; Actual:" + elem.Name.LocalName);
                if (elem.Name.LocalName == "Edmx")
                {
                    foreach (var elem2 in elem.Elements())
                    {
                        ParseSingleElement(unprocessedElements, elem2);
                    }
                }
            }
            State = EFElementState.Parsed;
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            Debug.Assert(elem.Name.LocalName == "Designer", "Incorrect XElement name, Expected: 'Designer'; Actual: " + elem.Name.LocalName);
            if (elem.Name.LocalName == "Designer")
            {
                ParseDesignerInfoRoot(elem);
                return true;
            }
            return false;
        }

        internal void ParseDesignerInfoRoot(XElement designerInfoElement)
        {
            if (_designerInfoRoot != null)
            {
                _designerInfoRoot.Dispose();
            }
            _designerInfoRoot = new EFDesignerInfoRoot(this, designerInfoElement);
            _designerInfoRoot.Parse(new HashSet<XName>());
        }

        internal Diagrams Diagrams
        {
            get { return _designerInfoRoot.Diagrams; }
        }

        internal override IEnumerable<EFObject> Children
        {
            get
            {
                if (_designerInfoRoot != null)
                {
                    yield return _designerInfoRoot;
                }
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            if (efContainer == _designerInfoRoot)
            {
                _designerInfoRoot = null;
            }
        }

#if DEBUG
        protected override VerifyModelIntegrityVisitor GetVerifyModelIntegrityVisitor()
        {
            return new VerifyDiagramModelIntegrityVisitor();
        }

        internal override VerifyModelIntegrityVisitor GetVerifyModelIntegrityVisitor(
            bool checkDisposed, bool checkUnresolved, bool checkXObject, bool checkAnnotations, bool checkBindingIntegrity)
        {
            return new VerifyDiagramModelIntegrityVisitor(
                checkDisposed, checkUnresolved, checkXObject, checkAnnotations, checkBindingIntegrity);
        }
#endif

        internal override void DetermineIfArtifactIsDesignerSafe()
        {
            // XmlSchemaValidator by default does not report any errors or warning if the namespace of the validated document
            // does not match the targetNamespace in the schema considering the schema not being applicable. With XmlReader 
            // it is possible to pass XmlSchemaValidationFlags.ReportValidationWarnings to be notified if this happens. However
            // it is not possible to pass this flag when using XDocument.Validate. Therefore before trying to validate the Xml
            // we just check that this is a known edmx namespace. If it is not we set the flag to false and skip validating.
            if (IsDesignerSafe = SchemaManager.GetEDMXNamespaceNames().Contains(XDocument.Root.Name.NamespaceName))
            {
                XDocument.Validate(
                    EscherAttributeContentValidator.GetInstance(SchemaVersion).EdmxSchemaSet,
                    (sender, args) => IsDesignerSafe = false);
            }
        }

        internal override Version SchemaVersion
        {
            get { return SchemaManager.GetSchemaVersion(XDocument.Root.Name.Namespace); }
        }

        /// <summary>
        ///     Return true if the XObject contains a link to an EFObject.
        /// </summary>
        /// <param name="xobject"></param>
        /// <returns></returns>
        protected internal override bool ExpectEFObjectForXObject(XObject xobject)
        {
            return ModelItemAnnotation.GetModelItem(xobject) != null;
        }

        /// <summary>
        ///     Return the namespaces that the diagram recognize.
        ///     Currently, we only recognize elements with edmx namespace in the diagram artifact.
        /// </summary>
        /// <returns></returns>
        protected internal override HashSet<string> GetNamespaces()
        {
            if (_namespaces == null)
            {
                _namespaces = new HashSet<string>();
                _namespaces.Add(SchemaManager.GetEDMXNamespaceName(SchemaVersion));
            }

            return _namespaces;
        }
    }
}
