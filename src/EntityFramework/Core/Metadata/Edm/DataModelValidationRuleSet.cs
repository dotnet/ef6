// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;

    internal abstract class DataModelValidationRuleSet
    {
        private readonly List<DataModelValidationRule> _rules = new List<DataModelValidationRule>();

        protected void AddRule(DataModelValidationRule rule)
        {
            DebugCheck.NotNull(rule);
            Debug.Assert(!_rules.Contains(rule), "should not add the duplicate rule");

            _rules.Add(rule);
        }

        protected void RemoveRule(DataModelValidationRule rule)
        {
            DebugCheck.NotNull(rule);
            Debug.Assert(_rules.Contains(rule), "should exist");

            _rules.Remove(rule);
        }

        internal IEnumerable<DataModelValidationRule> GetRules(MetadataItem itemToValidate)
        {
            DebugCheck.NotNull(itemToValidate);

            return _rules.Where(r => r.ValidatedType.IsInstanceOfType(itemToValidate));
        }
    }
}
