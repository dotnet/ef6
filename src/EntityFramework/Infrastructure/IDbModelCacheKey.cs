namespace System.Data.Entity.Infrastructure
{
    public interface IDbModelCacheKey
    {
        bool Equals(object other);
        int GetHashCode();
    }
}