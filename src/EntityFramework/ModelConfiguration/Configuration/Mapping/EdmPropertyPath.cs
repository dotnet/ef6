// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Edm;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Text;

    internal class EdmPropertyPath : IEnumerable<EdmProperty>
    {
        private static readonly EdmPropertyPath _empty = new EdmPropertyPath();

        private readonly List<EdmProperty> _components = new List<EdmProperty>();

        public EdmPropertyPath(IEnumerable<EdmProperty> components)
        {
            Contract.Requires(components != null);
            Contract.Assert(components.Any());

            _components.AddRange(components);
        }

        public EdmPropertyPath(EdmProperty component)
        {
            Contract.Requires(component != null);

            _components.Add(component);
        }

        private EdmPropertyPath()
        {
        }

        public static EdmPropertyPath Empty
        {
            get { return _empty; }
        }

        public override string ToString()
        {
            var propertyPathName = new StringBuilder();

            _components
                .Each(
                    pi =>
                        {
                            propertyPathName.Append(pi.Name);
                            propertyPathName.Append('.');
                        });

            return propertyPathName.ToString(0, propertyPathName.Length - 1);
        }

        #region Equality Members

        public bool Equals(EdmPropertyPath other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _components.SequenceEqual(other._components, (p1, p2) => p1 == p2);
        }

        public override bool Equals(object obj)
        {
            if (ReferenceEquals(null, obj))
            {
                return false;
            }

            if (ReferenceEquals(this, obj))
            {
                return true;
            }

            if (obj.GetType()
                != typeof(EdmPropertyPath))
            {
                return false;
            }

            return Equals((EdmPropertyPath)obj);
        }

        public override int GetHashCode()
        {
            return _components.Aggregate(0, (t, n) => t + n.GetHashCode());
        }

        public static bool operator ==(EdmPropertyPath left, EdmPropertyPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(EdmPropertyPath left, EdmPropertyPath right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator<EdmProperty> IEnumerable<EdmProperty>.GetEnumerator()
        {
            return _components.GetEnumerator();
        }

        IEnumerator IEnumerable.GetEnumerator()
        {
            return _components.GetEnumerator();
        }

        #endregion
    }
}
