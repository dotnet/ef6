namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;

    public class DefaultConnectionFactoryResolver : IDbDependencyResolver
    {
        private static readonly DefaultConnectionFactoryResolver _instance = new DefaultConnectionFactoryResolver();
        private IDbConnectionFactory _connectionFactory = new SqlConnectionFactory();

        public static DefaultConnectionFactoryResolver Instance
        {
            get { return _instance; }
        }

        public IDbConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory; }
            set { _connectionFactory = value; }
        }

        public object GetService(Type type, string name)
        {
            if (type == typeof(IDbConnectionFactory))
            {
                return ConnectionFactory;
            }

            return null;
        }

        public void Release(object service)
        {
        }
    }
}