namespace SimpleModel
{
    using System.Data.Entity;

    public class DisabledByConfigInitializerContext : DbContext
    {
    }

    public class DisabledByConfigWithInitializerAlsoSetInConfigContext : DbContext
    {
    }

    public class DisabledByLegacyConfigInitializerContext : DbContext
    {
    }

    public class DisabledByLegacyConfigWithEmptyInitializerContext : DbContext
    {
    }
}