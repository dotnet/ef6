namespace System.Data.Entity.Infrastructure
{
    public interface IDbModelCacheKeyFactory
    {
        IDbModelCacheKey Create(DbContext context);
    }
}