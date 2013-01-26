// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    public abstract partial class EmployeeMf
    {
        protected EmployeeMf()
        {
        }

        protected EmployeeMf(string firstName, string lastName)
        {
            FirstName = firstName;
            LastName = lastName;
        }
    }
}
