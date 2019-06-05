// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ConnectionFactoryConfig
{
    using System.Collections.Generic;
    using System.Data.Entity.Utilities;
    using System.Linq;

    // <summary>
    // Represents a specification for the default connection factory to be set into a config file.
    // </summary>
    internal class ConnectionFactorySpecification
    {
        private readonly string _connectionFactoryName;
        private readonly IEnumerable<string> _constructorArguments;

        public ConnectionFactorySpecification(string connectionFactoryName, params string[] constructorArguments)
        {
            DebugCheck.NotEmpty(connectionFactoryName);

            _connectionFactoryName = connectionFactoryName;
            _constructorArguments = constructorArguments ?? Enumerable.Empty<string>();
        }

        public string ConnectionFactoryName
        {
            get { return _connectionFactoryName; }
        }

        public IEnumerable<string> ConstructorArguments
        {
            get { return _constructorArguments; }
        }
    }
}
