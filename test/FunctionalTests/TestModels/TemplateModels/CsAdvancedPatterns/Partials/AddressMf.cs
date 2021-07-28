// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.ProductivityApi.TemplateModels.CsAdvancedPatterns
{
    public partial class AddressMf
    {
        public AddressMf(string street, string city, string state, string zipCode, int? zone, string environment)
        {
            Street = street;
            City = city;
            State = state;
            ZipCode = zipCode;
            SiteInfo = new SiteInfoMf(zone, environment);
        }
    }
}
