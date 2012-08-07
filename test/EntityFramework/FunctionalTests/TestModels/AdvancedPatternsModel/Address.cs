// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AdvancedPatternsModel
{
    using System;
    using System.Globalization;

    public class Address
    {
        public string Street { get; set; }
        public string City { get; set; }
        public string State { get; set; }
        public string ZipCode { get; set; }
        public SiteInfo SiteInfo { get; set; }

        // None of the properties below are mapped
        public string County { get; set; }

        public string FormattedAddress
        {
            get { return String.Format(CultureInfo.InvariantCulture, "{0}, {1}, {2} {3}", Street, City, State, ZipCode); }
        }

        private string _writeOnly = "WriteOnly";

        public string WriteOnly
        {
            set { _writeOnly = value; }
        }

        public string GetWriteOnlyValue()
        {
            return _writeOnly;
        }
    }
}
