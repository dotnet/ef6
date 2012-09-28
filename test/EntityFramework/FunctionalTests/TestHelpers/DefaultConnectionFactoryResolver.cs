// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure;

    public class DefaultConnectionFactoryResolver : IDbDependencyResolver
    {
        private static readonly DefaultConnectionFactoryResolver _instance = new DefaultConnectionFactoryResolver();
        private volatile IDbConnectionFactory _connectionFactory = new SqlConnectionFactory();

        public static DefaultConnectionFactoryResolver Instance
        {
            get { return _instance; }
        }

        public IDbConnectionFactory ConnectionFactory
        {
            get { return _connectionFactory; }
            set { _connectionFactory = value; }
        }

        public object GetService(Type type, object key)
        {
            if (type == typeof(IDbConnectionFactory))
            {
                return ConnectionFactory;
            }

            return null;
        }
    }
}
