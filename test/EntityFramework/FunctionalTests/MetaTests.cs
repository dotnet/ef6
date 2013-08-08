// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Meta
{
    using System;
    using System.Linq;
    using Xunit;
    using System.Reflection;

    /// <summary>
    /// Meta tests. These tests check the test suite for common issues.
    /// </summary>
    public class MetaTests : TestBase
    {
        [Fact]
        public void All_functional_tests_implement_TestBase()
        {
            var currentAssembly = Assembly.GetExecutingAssembly();
            var types = currentAssembly.GetTypes();

            foreach (var type in types)
            {
                if(IsTypeTestClass(type) && !DoesTypeImplementTestBase(type))
                {
                    throw new Exception(String.Format("Test class '{0}' does not implement TestBase.", type.Name));
                }
            }
        }

        #region Helper methods
        /// <summary>
        /// Checks the type for any test methods. 
        /// Assumes any method attribute with an Xunit namespace indicates a test method.
        /// </summary>
        /// <param name="type"> Type to be checked. </param>
        /// <returns> True if the type is a test class, false otherwise. </returns>
        private bool IsTypeTestClass(Type type)
        {
            return type.GetMethods()
                .Any(method => method.GetCustomAttributes(inherit: true)
                    .Any(attribute =>
                    {
                        var attribType = (attribute as Attribute).TypeId as Type;
                        
                        return attribType != null 
                            ? attribType.Namespace == "Xunit"
                            : false;    
                    }));
        }

        /// <summary>
        /// Checks base types of the given types for TestBase
        /// </summary>
        /// <param name="type"> Type to be checked. </param>
        /// <returns> True if the type implements TestBase, false otherwise. </returns>
        private bool DoesTypeImplementTestBase(Type type)
        {           
            var baseType = type.BaseType;

            while (baseType != null)
            {
                if (baseType.Name == "TestBase")
                {
                    return true;
                }
                baseType = baseType.BaseType;
            }            
            return false;
        }
        #endregion
    }
}
