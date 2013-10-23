// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to change aspects of a ResultBinding inside a function mapping.
    ///     Example:
    ///     &lt;InsertFunction FunctionName=&quot;PerfDB.Store.sp_insert_test_metric&quot;&gt;
    ///     &lt;ResultBinding ColumnName=&quot;id&quot; Name=&quot;id&quot; /&gt;
    ///     &lt;/InsertFunction&gt;
    ///     These can live as direct children of a ModificationFunction (not Delete).  The different
    ///     thing with these, is that ColumnName does NOT point to a Property in an S-Side entity but
    ///     to a column in the function's result set.  These "column names" cannot be validated until runtime.
    /// </summary>
    internal class ChangeResultBindingCommand : Command
    {
        private readonly ResultBinding _binding;
        private readonly Property _entityProperty;
        private readonly string _columnName;

        /// <summary>
        ///     This method lets you change aspects of a ResultBinding.
        /// </summary>
        /// <param name="sp">Must point to a valid ResultBinding</param>
        /// <param name="entityProperty">Can be null</param>
        /// <param name="columnName">Can be null</param>
        internal ChangeResultBindingCommand(ResultBinding binding, Property entityProperty, string columnName)
        {
            Debug.Assert(!(entityProperty == null && columnName == null), "All optional arguments can't be null");
            CommandValidation.ValidateResultBinding(binding);
            if (entityProperty != null)
            {
                CommandValidation.ValidateConceptualEntityProperty(entityProperty);
            }

            _binding = binding;
            _entityProperty = entityProperty;
            _columnName = columnName;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(_binding != null, "InvokeInternal is called when _binding is null.");
            if (_binding == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when _binding is null");
            }

            // set new values in if we were sent them
            if (_entityProperty != null
                && _binding.Name.Target != _entityProperty)
            {
                _binding.Name.SetRefName(_entityProperty);
            }

            // set new values in if we were sent them
            if (_columnName != null
                && !string.Equals(_binding.ColumnName.Value, _columnName, StringComparison.CurrentCulture))
            {
                _binding.ColumnName.Value = _columnName;
            }

            XmlModelHelper.NormalizeAndResolve(_binding);
        }
    }
}
