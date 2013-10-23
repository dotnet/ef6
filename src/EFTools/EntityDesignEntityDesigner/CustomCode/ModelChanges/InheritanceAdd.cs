// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.EntityDesigner.ViewModel;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.Views;
    using Microsoft.VisualStudio.Modeling.Diagrams;

    internal class InheritanceAdd : ViewModelChange
    {
        private readonly Inheritance _inheritance;
        private readonly ConceptualEntityType _baseEntity;
        private readonly ConceptualEntityType _derivedEntity;

        internal InheritanceAdd(Inheritance inheritance, ConceptualEntityType baseEntity, ConceptualEntityType derivedEntity)
        {
            _inheritance = inheritance;
            _baseEntity = baseEntity;
            _derivedEntity = derivedEntity;
        }

        internal override void Invoke(CommandProcessorContext cpc)
        {
            var viewModel = _inheritance.GetRootViewModel();
            Debug.Assert(viewModel != null, "Unable to find root view model from inheritance: " + _inheritance);

            if (viewModel != null)
            {
                if (ViewUtils.SetBaseEntityType(cpc, _derivedEntity, _baseEntity))
                {
                    viewModel.ModelXRef.Add(_derivedEntity.BaseType, _inheritance, viewModel.EditingContext);
                }
                else
                {
                    try
                    {
                        // setting null will clear out the selection, which may be this Inheritance thing we are deleting
                        viewModel.GetDiagram().ActiveDiagramView.Selection.Set((DiagramItem)null);

                        // in this case inheritance was not created in the model, so we need to delete it from the view model
                        // we don't want any rules to fire for this, so suspend them temporarly
                        _inheritance.Store.RuleManager.SuspendRuleNotification();
                        using (var t = _inheritance.Store.TransactionManager.BeginTransaction())
                        {
                            _inheritance.Delete();
                            t.Commit();
                        }
                    }
                    finally
                    {
                        _inheritance.Store.RuleManager.ResumeRuleNotification();
                    }
                }
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 120; }
        }
    }
}
