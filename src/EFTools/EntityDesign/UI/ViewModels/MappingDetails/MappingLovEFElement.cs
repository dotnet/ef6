// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.MappingDetails
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;

    // <summary>
    //     This class is used to hold on object to display in a list-of-values (LOV).
    //     It contains the name by which we want a given underlying object displayed
    //     and, optionally, the underlying object itself.
    //     The underlying object is usually an EFElement, but there are cases
    //     (in particular see the comments in MappingScalarProperty) where
    //     we need to store the MappingEFElement because the EFElement does
    //     not yet exist.
    //     Another exception is when we need to store a list of Properties for MappingScalarProperty.
    //     Lastly it is possible to store "dummy" elements which do
    //     not map to any underlying object (e.g. the "&lt;Delete&gt;" element in a
    //     drop-down, or the "=" sign in a Condition).
    // </summary>
    internal class MappingLovEFElement
    {
        private readonly object _object;
        private readonly string _displayName;

        internal MappingLovEFElement(EFElement modelElement, string displayName)
        {
            Debug.Assert(modelElement != null, "null modelElement");
            Debug.Assert(displayName != null, "null displayName");

            _object = modelElement;
            _displayName = displayName;
        }

        internal MappingLovEFElement(MappingEFElement mappingElement, string displayName)
        {
            Debug.Assert(mappingElement != null, "null mappingElement");
            Debug.Assert(displayName != null, "null displayName");
            _object = mappingElement;
            _displayName = displayName;
        }

        internal MappingLovEFElement(List<Property> properties, string displayName)
        {
            Debug.Assert(properties != null, "null properties");
            Debug.Assert(displayName != null, "null displayName");
            _object = properties;
            _displayName = displayName;
        }

        internal MappingLovEFElement(string displayName)
        {
            Debug.Assert(displayName != null, "null displayName");
            _displayName = displayName;
        }

        internal EFElement ModelElement
        {
            get { return _object as EFElement; }
        }

        internal MappingEFElement MappingElement
        {
            get { return _object as MappingEFElement; }
        }

        internal object Object
        {
            get { return _object; }
        }

        internal string DisplayName
        {
            get { return _displayName; }
        }

        public override string ToString()
        {
            return DisplayName;
        }
    }
}
