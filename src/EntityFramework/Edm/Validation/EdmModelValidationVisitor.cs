// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Edm.Validation
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Metadata.Edm;

    internal sealed class EdmModelValidationVisitor : EdmModelVisitor
    {
        private readonly EdmModelValidationContext _context;
        private readonly EdmModelRuleSet _ruleSet;
        private readonly HashSet<MetadataItem> _visitedItems = new HashSet<MetadataItem>();

        internal EdmModelValidationVisitor(EdmModelValidationContext context, EdmModelRuleSet ruleSet)
        {
            _context = context;
            _ruleSet = ruleSet;
        }

        protected internal override void VisitMetadataItem(MetadataItem item)
        {
            if (_visitedItems.Add(item))
            {
                EvaluateItem(item);
            }
        }

        private void EvaluateItem(MetadataItem item)
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
