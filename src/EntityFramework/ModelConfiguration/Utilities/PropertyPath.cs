namespace System.Data.Entity.ModelConfiguration.Utilities
{
    using System.Collections;
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.Contracts;
    using System.Linq;
    using System.Reflection;
    using System.Text;

    internal class PropertyPath : IEnumerable<PropertyInfo>
    {
        // Note: This class is currently immutable. If you make it mutable then you
        // must ensure that instances are cloned when cloning the DbModelBuilder.
        public static readonly PropertyPath Empty = new PropertyPath();

        private readonly List<PropertyInfo> _components = new List<PropertyInfo>();

        public PropertyPath(IEnumerable<PropertyInfo> components)
        {
            Contract.Requires(components != null);
            Contract.Assert(components.Any());

            _components.AddRange(components);
        }

        public PropertyPath(PropertyInfo component)
        {
            Contract.Requires(component != null);

            _components.Add(component);
        }

        private PropertyPath()
        {
        }

        public int Count
        {
            get { return _components.Count; }
        }

        public PropertyInfo this[int index]
        {
            get { return _components[index]; }
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

        public bool Equals(PropertyPath other)
        {
            if (ReferenceEquals(null, other))
            {
                return false;
            }

            if (ReferenceEquals(this, other))
            {
                return true;
            }

            return _components.SequenceEqual(other._components, (p1, p2) => p1.IsSameAs(p2));
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
                != typeof(PropertyPath))
            {
                return false;
            }

            return Equals((PropertyPath)obj);
        }

        public override int GetHashCode()
        {
            return _components.Aggregate(0, (t, n) => t + n.GetHashCode());
        }

        public static bool operator ==(PropertyPath left, PropertyPath right)
        {
            return Equals(left, right);
        }

        public static bool operator !=(PropertyPath left, PropertyPath right)
        {
            return !Equals(left, right);
        }

        #endregion

        #region IEnumerable Members

        IEnumerator<PropertyInfo> IEnumerable<PropertyInfo>.GetEnumerator()
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
