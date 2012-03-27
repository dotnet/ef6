namespace SimpleModel
{
    using System.Collections.Generic;

    /// <summary>
    /// Compares two BaseTypeForLinq objects.
    /// </summary>
    public class BaseTypeForLinqComparer : IEqualityComparer<BaseTypeForLinq>
    {
        public bool Equals(BaseTypeForLinq left, BaseTypeForLinq right)
        {
            return (left == null && right == null) ||
                   (left.GetType() == right.GetType() && left.EntityEquals(right));
        }

        public int GetHashCode(BaseTypeForLinq entity)
        {
            return entity.EntityHashCode;
        }
    }
}
