// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Another.Place
{
    public class PhoneMm
    {
        public PhoneMm()
        {
            Extension = "None";
        }

        public string PhoneNumber { get; set; }
        public string Extension { get; set; }
        public PhoneTypeMm PhoneType { get; set; }
    }
}
