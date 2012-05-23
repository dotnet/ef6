namespace System.Data.Entity.Core.Common.Utils.Boolean
{
    using System.Collections.Generic;
    using System.Diagnostics;

    /// <summary>
    /// Represents a variable with finite domain, e.g., c in {1, 2, 3}
    /// </summary>
    /// <typeparam name="T_Element">Type of domain variables (int in the above example).</typeparam>
    /// <typeparam name="T_Variable">Type of the identifier (c above -- it need not be int).</typeparam>
    internal class DomainVariable<T_Variable, T_Element>
    {
        private readonly T_Variable _identifier;
        private readonly Set<T_Element> _domain;
        private readonly int _hashCode;
        private readonly IEqualityComparer<T_Variable> _identifierComparer;

        /// <summary>
        /// Constructs a new domain variable.
        /// </summary>
        /// <param name="identifier">Identifier </param>
        /// <param name="domain">Domain of variable.</param>
        /// <param name="identifierComparer">Comparer of identifier</param>
        internal DomainVariable(T_Variable identifier, Set<T_Element> domain, IEqualityComparer<T_Variable> identifierComparer)
        {
            Debug.Assert(null != identifier && null != domain);
            _identifier = identifier;
            _domain = domain.AsReadOnly();
            _identifierComparer = identifierComparer ?? EqualityComparer<T_Variable>.Default;
            var domainHashCode = _domain.GetElementsHashCode();
            var identifierHashCode = _identifierComparer.GetHashCode(_identifier);
            _hashCode = domainHashCode ^ identifierHashCode;
        }

        internal DomainVariable(T_Variable identifier, Set<T_Element> domain)
            : this(identifier, domain, null)
        {
        }

        /// <summary>
        /// Gets the variable.
        /// </summary>
        internal T_Variable Identifier
        {
            get { return _identifier; }
        }

        /// <summary>
        /// Gets the domain of this variable.
        /// </summary>
        internal Set<T_Element> Domain
        {
            get { return _domain; }
        }

        public override int GetHashCode()
        {
            return _hashCode;
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(this, obj))
            {
                return true;
            }
            var other = obj as DomainVariable<T_Variable, T_Element>;
            if (null == other)
            {
                return false;
            }
            if (_hashCode != other._hashCode)
            {
                return false;
            }
            return (_identifierComparer.Equals(_identifier, other._identifier) && _domain.SetEquals(other._domain));
        }

        public override string ToString()
        {
            return StringUtil.FormatInvariant(
                "{0}{{{1}}}",
                _identifier.ToString(), _domain);
        }
    }
}
