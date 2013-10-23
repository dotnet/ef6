// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Visitor
{
    using System;
    using System.Globalization;
    using System.Xml.Linq;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Tools.XmlDesignerBase;

    internal class ResolvingVisitor : MissedItemCollectingVisitor
    {
        private readonly EFArtifactSet _artifactSet;

        internal ResolvingVisitor(EFArtifactSet artifactSet)
        {
            _artifactSet = artifactSet;
        }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal override void Visit(IVisitable visitable)
        {
            var item = visitable as EFContainer;

            if (item == null)
            {
                return;
            }

            if (item.State != EFElementState.Normalized)
            {
                return;
            }

            try
            {
                item.Resolve(_artifactSet);
            }
            catch (Exception e)
            {
                // reset element state
                item.State = EFElementState.ResolveAttempted;

                string name = null;
                var nameable = item as EFNameableItem;
                if (nameable != null)
                {
                    name = nameable.LocalName.Value;
                }
                else
                {
                    var element = item.XObject as XElement;
                    if (element != null)
                    {
                        name = element.Name.LocalName;
                    }
                    else
                    {
                        name = item.SemanticName;
                    }
                }
                var message = string.Format(CultureInfo.CurrentCulture, Resources.ErrorResolvingItem, name, e.Message);
                var errorInfo = new ErrorInfo(
                    ErrorInfo.Severity.ERROR, message, item, ErrorCodes.FATAL_RESOLVE_ERROR, ErrorClass.ResolveError);
                _artifactSet.AddError(errorInfo);
            }

            if (item.State != EFElementState.Resolved)
            {
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
