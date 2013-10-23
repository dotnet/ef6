// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Mapping
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;

    internal class FunctionImportMapping : EFElement
    {
        internal static readonly string ElementName = "FunctionImportMapping";
        internal static readonly string AttributeFunctionName = "FunctionName";
        internal static readonly string AttributeFunctionImportName = "FunctionImportName";

        private SingleItemBinding<Function> _functionName;
        private SingleItemBinding<FunctionImport> _functionImportName;

        private ResultMapping _resultMapping;

        internal FunctionImportMapping(EFElement parent, XElement element)
            : base(parent, element)
        {
        }

        internal SingleItemBinding<Function> FunctionName
        {
            get
            {
                if (_functionName == null)
                {
                    _functionName = new SingleItemBinding<Function>(
                        this,
                        AttributeFunctionName,
                        FunctionNameNormalizer);
                }
                return _functionName;
            }
        }

        internal SingleItemBinding<FunctionImport> FunctionImportName
        {
            get
            {
                if (_functionImportName == null)
                {
                    _functionImportName = new SingleItemBinding<FunctionImport>(
                        this,
                        AttributeFunctionImportName,
                        FunctionImportNameNormalizer.NameNormalizer);
                }
                return _functionImportName;
            }
        }

        internal ResultMapping ResultMapping
        {
            get { return _resultMapping; }
            set { _resultMapping = value; }
        }

#if DEBUG
        internal override ICollection<string> MyAttributeNames()
        {
            var s = base.MyAttributeNames();
            s.Add(AttributeFunctionName);
            s.Add(AttributeFunctionImportName);
            return s;
        }

        internal override ICollection<string> MyChildElementNames()
        {
            var s = base.MyChildElementNames();
            s.Add(ResultMapping.ElementName);
            return s;
        }
#endif

        protected override void PreParse()
        {
            Debug.Assert(State != EFElementState.Parsed, "this object should not already be in the parsed state");

            ClearEFObject(_functionImportName);
            _functionImportName = null;

            ClearEFObject(_functionName);
            _functionName = null;

            ClearEFObject(_resultMapping);
            _resultMapping = null;

            base.PreParse();
        }

        internal override bool ParseSingleElement(ICollection<XName> unprocessedElements, XElement elem)
        {
            if (elem.Name.LocalName == ResultMapping.ElementName)
            {
                if (_resultMapping != null)
                {
                    // multiple ResultMapping elements
                    var msg = String.Format(CultureInfo.CurrentCulture, Resources.DuplicatedElementMsg, elem.Name.LocalName);
                    Artifact.AddParseErrorForObject(this, msg, ErrorCodes.DUPLICATED_ELEMENT_ENCOUNTERED);
                }
                else
                {
                    _resultMapping = new ResultMapping(this, elem);
                    _resultMapping.Parse(unprocessedElements);
                }
            }
            else
            {
                base.ParseSingleElement(unprocessedElements, elem);
            }
            return true;
        }

        protected override void DoResolve(EFArtifactSet artifactSet)
        {
            FunctionName.Rebind();
            FunctionImportName.Rebind();

            if (FunctionName.Status == BindingStatus.Known
                && FunctionImportName.Status == BindingStatus.Known)
            {
                State = EFElementState.Resolved;
            }
        }

        internal override DeleteEFElementCommand GetDeleteCommand()
        {
            var cmd = new DeleteFunctionImportMappingCommand(this);
            if (cmd == null)
            {
                // shouldn't happen, just to be safe
                throw new InvalidOperationException();
            }
            return cmd;
        }

        internal static ICollection<DeleteEFElementCommand> GetDeleteCommand(FunctionImport functionImport)
        {
            // try to locate a FunctionImportMapping for this import
            var commands = new List<DeleteEFElementCommand>();
            var functionImportMappings = functionImport.GetAntiDependenciesOfType<FunctionImportMapping>();
            foreach (var fim in functionImportMappings)
            {
                if (fim.FunctionImportName.Target == functionImport)
                {
                    commands.Add(new DeleteFunctionImportMappingCommand(fim));
                }
            }

            return commands;
        }

        internal static NormalizedName FunctionNameNormalizer(EFElement parent, string refName)
        {
            return EFNormalizableItemDefaults.DefaultNameNormalizerForMSL(parent, refName);
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
                if (_resultMapping != null)
                {
                    yield return _resultMapping;
                }
                yield return FunctionImportName;
                yield return FunctionName;
            }
        }

        protected override void OnChildDeleted(EFContainer efContainer)
        {
            var rm = efContainer as ResultMapping;
            if (rm != null)
            {
                Debug.Assert(_resultMapping == rm, "Unknown ResultMapping in OnChildDeleted");
                ClearEFObject(_resultMapping);
                _resultMapping = null;
            }
            else
            {
                base.OnChildDeleted(efContainer);
            }
        }
    }
}
