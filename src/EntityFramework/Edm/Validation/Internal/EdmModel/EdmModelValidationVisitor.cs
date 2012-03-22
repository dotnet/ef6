namespace System.Data.Entity.Edm.Validation.Internal.EdmModel
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Internal;
    using EdmModel = System.Data.Entity.Edm.EdmModel;

    /// <summary>
    ///     Visitor for EdmModel Validation
    /// </summary>
    internal sealed class EdmModelValidationVisitor : EdmModelVisitor
    {
        private readonly EdmModelValidationContext _context;
        private readonly EdmModelRuleSet _ruleSet;
        private readonly HashSet<EdmDataModelItem> _visitedItems = new HashSet<EdmDataModelItem>();

        internal EdmModelValidationVisitor(EdmModelValidationContext context, EdmModelRuleSet ruleSet)
        {
            _context = context;
            _ruleSet = ruleSet;
        }

        protected override void VisitEdmDataModelItem(EdmDataModelItem item)
        {
            if (_visitedItems.Add(item))
            {
                EvaluateItem(item);
                base.VisitEdmDataModelItem(item);
            }
        }

        private void EvaluateItem(EdmDataModelItem item)
        {
            foreach (var rule in _ruleSet.GetRules(item))
            {
                rule.Evaluate(_context, item);
            }
        }

        internal void Visit(EdmModel model)
        {
            VisitEdmModel(model);
        }
    }
}
