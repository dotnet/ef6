// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views.EntityDesigner
{
    using System;
    using System.Collections;
    using System.Collections.Generic;
    using Microsoft.Data.Entity.Design.Model;

    internal class EntityDesignerSelection : Selection
    {
        public EntityDesignerSelection()
        {
        }

        internal EntityDesignerSelection(IEnumerable<EFObject> selectedObjects)
            : base(selectedObjects)
        {
        }

        internal EntityDesignerSelection(IEnumerable<EFObject> selectedObjects, Predicate<EFObject> match)
            : base(selectedObjects, match)
        {
        }

        internal EntityDesignerSelection(IEnumerable selectedObjects)
            : base(selectedObjects)
        {
        }

        internal EntityDesignerSelection(IEnumerable selectedObjects, Predicate<EFObject> match)
            : base(selectedObjects, match)
        {
        }

        internal EntityDesignerSelection(params EFObject[] selectedObjects)
            : base(selectedObjects)
        {
        }

        internal override Type ItemType
        {
            get { return typeof(EntityDesignerSelection); }
        }
    }
}
