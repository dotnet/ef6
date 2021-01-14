// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace FunctionalTests.Model
{
    using System;
    using System.ComponentModel.DataAnnotations.Schema;

    [ComplexType]
    public class RowDetails
    {
        public Guid rowguid { get; set; }
        public DateTime ModifiedDate { get; set; }
    }
}
