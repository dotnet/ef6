// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System;
    using System.Globalization;

    internal class NormalizingVisitor : MissedItemCollectingVisitor
    {
        internal override void Visit(IVisitable visitable)
        {
            var item = visitable as EFContainer;

            if (item == null)
            {
                return;
            }

            if (item.State == EFElementState.None
                ||
                item.State == EFElementState.Normalized
                ||
                item.State == EFElementState.Resolved)
            {
                return;
            }

            try
            {
                item.Normalize();
            }
            catch (Exception e)
            {
                throw new InvalidOperationException(
                    string.Format(
                        CultureInfo.CurrentCulture, "Error normalizing item {0}",
                        item.SemanticName
                        ),
                    e);
            }

            if (item.State != EFElementState.Normalized)
            {
                // only do this for elements
                var efElement = item as EFElement;
                if (efElement != null)
                {
                    _missedCount++;
                    if (!_missed.Contains(efElement))
                    {
                        _missed.Add(efElement);
                    }
                }
            }
        }
    }
}
