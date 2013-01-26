// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.TestHelpers
{
    using System;
    using System.Data.Entity.Config;
    using System.Data.Entity.Infrastructure.Pluralization;

    public class DefaultPluralizationServiceResolver : IDbDependencyResolver
    {
        private static readonly DefaultPluralizationServiceResolver _instance = new DefaultPluralizationServiceResolver();
        private volatile IPluralizationService _pluralizationService = new EnglishPluralizationService();

        public static DefaultPluralizationServiceResolver Instance
        {
            get { return _instance; }
        }

        public IPluralizationService PluralizationService
        {
            get { return _pluralizationService; }
            set { _pluralizationService = value; }
        }

        public object GetService(Type type, object key)
        {
            if (type == typeof(IPluralizationService))
            {
                return _pluralizationService;
            }

            return null;
        }
    }
}
