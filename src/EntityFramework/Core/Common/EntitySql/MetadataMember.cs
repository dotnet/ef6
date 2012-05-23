namespace System.Data.Entity.Core.Common.EntitySql
{
    using System.Collections.Generic;
    using System.Data.Entity.Resources;
    using System.Diagnostics;

    /// <summary>
    /// Abstract class representing an eSQL expression classified as <see cref="ExpressionResolutionClass.MetadataMember"/>.
    /// </summary>
    internal abstract class MetadataMember : ExpressionResolution
    {
        protected MetadataMember(MetadataMemberClass @class, string name)
            : base(ExpressionResolutionClass.MetadataMember)
        {
            Debug.Assert(!String.IsNullOrEmpty(name), "name must not be empty");

            MetadataMemberClass = @class;
            Name = name;
        }

        internal override string ExpressionClassName
        {
            get { return MetadataMemberExpressionClassName; }
        }

        internal static string MetadataMemberExpressionClassName
        {
            get { return Strings.LocalizedMetadataMemberExpression; }
        }

        internal readonly MetadataMemberClass MetadataMemberClass;
        internal readonly string Name;

        /// <summary>
        /// Return the name of the <see cref="MetadataMemberClass"/> for error messages.
        /// </summary>
        internal abstract string MetadataMemberClassName { get; }

        internal static IEqualityComparer<MetadataMember> CreateMetadataMemberNameEqualityComparer(StringComparer stringComparer)
        {
            return new MetadataMemberNameEqualityComparer(stringComparer);
        }

        private sealed class MetadataMemberNameEqualityComparer : IEqualityComparer<MetadataMember>
        {
            private readonly StringComparer _stringComparer;

            internal MetadataMemberNameEqualityComparer(StringComparer stringComparer)
            {
                _stringComparer = stringComparer;
            }

            bool IEqualityComparer<MetadataMember>.Equals(MetadataMember x, MetadataMember y)
            {
                Debug.Assert(x != null && y != null, "metadata members must not be null");
                return _stringComparer.Equals(x.Name, y.Name);
            }

            int IEqualityComparer<MetadataMember>.GetHashCode(MetadataMember obj)
            {
                Debug.Assert(obj != null, "metadata member must not be null");
                return _stringComparer.GetHashCode(obj.Name);
            }
        }
    }
}
