// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.EntityDesigner.Rules
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges;
    using Microsoft.Data.Entity.Design.EntityDesigner.Utils;
    using Microsoft.Data.Entity.Design.EntityDesigner.View;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Tools.VSXmlDesignerBase.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    /// <summary>
    ///     Rule fired when an Inheritance is created
    /// </summary>
    [RuleOn(typeof(Inheritance), FireTime = TimeToFire.TopLevelCommit)]
    internal sealed class Inheritance_DeleteRule : DeleteRule
    {
        public override void ElementDeleted(ElementDeletedEventArgs e)
        {
            base.ElementDeleted(e);

            var inheritance = e.ModelElement as Inheritance;
            if (inheritance != null)
            {
                if (inheritance.TargetEntityType != null)
                {
                    // We need to invalidate the target entitytypeshape element; so base type name will be updated correctly.
                    foreach (var pe in PresentationViewsSubject.GetPresentation(inheritance.TargetEntityType))
                    {
                        var entityShape = pe as EntityTypeShape;
                        if (entityShape != null)
                        {
                            entityShape.Invalidate();
                        }
                    }
                }

                var tx = ModelUtils.GetCurrentTx(inheritance.Store);
                Debug.Assert(tx != null);
                if (tx != null
                    && !tx.IsSerializing)
                {
                    ViewModelChangeContext.GetNewOrExistingContext(tx).ViewModelChanges.Add(new InheritanceDelete(inheritance));
                }
            }
        }
    }
}
