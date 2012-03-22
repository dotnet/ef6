namespace System.Data.Entity.Edm.Validation.Internal
{
    using System.Collections.Generic;
    using System.Data.Entity.Edm.Common;
    using System.Diagnostics.Contracts;
    using System.Linq;

    /// <summary>
    ///     RuleSet for DataModel Validation
    /// </summary>
    internal abstract class DataModelValidationRuleSet
    {
        private readonly List<DataModelValidationRule> _rules = new List<DataModelValidationRule>();

        protected void AddRule(DataModelValidationRule rule)
        {
            Contract.Assert(!_rules.Contains(rule), "should not add the duplicate rule");

            _rules.Add(rule);
        }

        protected void RemoveRule(DataModelValidationRule rule)
        {
            Contract.Assert(_rules.Contains(rule), "should exist");

            _rules.Remove(rule);
        }

        /// <summary>
        ///     Get the related rules given certain DataModelItem
        /// </summary>
        /// <param name = "itemToValidate"> The <see cref = "DataModelItem" /> to validate </param>
        /// <returns> A collection of <see cref = "DataModelValidationRule" /> </returns>
        internal IEnumerable<DataModelValidationRule> GetRules(DataModelItem itemToValidate)
        {
            return _rules.Where(r => r.ValidatedType.IsAssignableFrom(itemToValidate.GetType()));
        }
    }
}
