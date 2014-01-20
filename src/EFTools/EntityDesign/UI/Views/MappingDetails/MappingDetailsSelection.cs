// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.MappingDetails
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model;

    // <summary>
    //     The class that holds the current selection in the Mapping Details window.
    // </summary>
    internal class MappingDetailsSelection : Selection
    {
        public MappingDetailsSelection()
        {
        }

        internal MappingDetailsSelection(IEnumerable<EFObject> selectedObjects)
            : base(selectedObjects)
        {
        }

        internal MappingDetailsSelection(IEnumerable<EFObject> selectedObjects, Predicate<EFObject> match)
            : base(selectedObjects, match)
        {
        }

        internal MappingDetailsSelection(IEnumerable selectedObjects)
            : base(selectedObjects)
        {
        }

        internal MappingDetailsSelection(IEnumerable selectedObjects, Predicate<EFObject> match)
            : base(selectedObjects, match)
        {
        }

        internal MappingDetailsSelection(params EFObject[] selectedObjects)
            : base(selectedObjects)
        {
        }

        internal override Type ItemType
        {
            get { return typeof(MappingDetailsSelection); }
        }
    }
}
