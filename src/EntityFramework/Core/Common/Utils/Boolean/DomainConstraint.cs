// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Data.Entity.Utilities;

    /// <summary>
    /// Represents a constraint of the form:
    /// Var1 in Range
    /// </summary>
    /// <typeparam name="T_Variable"> Type of the variable. </typeparam>
    /// <typeparam name="T_Element"> Type of range elements. </typeparam>
    internal class DomainConstraint<T_Variable, T_Element>
    {
        private readonly DomainVariable<T_Variable, T_Element> _variable;
        private readonly Set<T_Element> _range;
        private readonly int _hashCode;

        /// <summary>
        /// Constructs a new constraint for the given variable and range.
        /// </summary>
        /// <param name="variable"> Variable in constraint. </param>
        /// <param name="range"> Range of constraint. </param>
        internal DomainConstraint(DomainVariable<T_Variable, T_Element> variable, Set<T_Element> range)
        {
            DebugCheck.NotNull(variable);
            DebugCheck.NotNull(range);

            _variable = variable;
            _range = range.AsReadOnly();
            _hashCode = _variable.GetHashCode() ^ _range.GetElementsHashCode();
        }

        /// <summary>
        /// Constructor supporting a singleton range domain constraint
        /// </summary>
        internal DomainConstraint(DomainVariable<T_Variable, T_Element> variable, T_Element element)
            : this(variable, new Set<T_Element>(new[] { element }).MakeReadOnly())
        {
        }

        /// <summary>
        /// Gets the variable for this constraint.
        /// </summary>
        internal DomainVariable<T_Variable, T_Element> Variable
        {
            get { return _variable; }
        }

        /// <summary>
        /// Get the range for this constraint.
        /// </summary>
        internal Set<T_Element> Range
        {
            get { return _range; }
        }

        /// <summary>
        /// Inverts this constraint (this iff. !result)
        /// !(Var in Range) iff. Var in (Var.Domain - Range)
        /// </summary>
        internal DomainConstraint<T_Variable, T_Element> InvertDomainConstraint()
        {
            return new DomainConstraint<T_Variable, T_Element>(
                _variable,
                _variable.Domain.Difference(_range).AsReadOnly());
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as DomainConstraint<T_Variable, T_Element>;
            if (null == other)
            {
                return false;
            }
            if (_hashCode != other._hashCode)
            {
                return false;
            }
            return (_range.SetEquals(other._range) && _variable.Equals(other._variable));
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant(
                "{0} in [{1}]",
                _variable, _range);
        }
    }
}
