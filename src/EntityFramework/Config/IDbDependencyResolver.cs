namespace System.Data.Entity.Config
{
    public interface IDbDependencyResolver
    {
        // TODO Add other overloads of Get
        object Get(Type type, string name);
        void Release(object service);
    }
}