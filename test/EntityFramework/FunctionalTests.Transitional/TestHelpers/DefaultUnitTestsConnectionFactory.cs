// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestHelpers
{
    /// <summary>
    /// This connection factory is set in the <see cref="FunctionalTestsConfiguration" /> but is then
    /// replaced in the Loaded event handler of that class.
    /// </summary>
    public class DefaultUnitTestsConnectionFactory : DefaultFunctionalTestsConnectionFactory
    {
    }
}
