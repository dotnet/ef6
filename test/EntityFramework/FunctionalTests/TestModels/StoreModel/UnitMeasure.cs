// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.
namespace FunctionalTests.Model
{
    using System.ComponentModel.DataAnnotations;

    public class UnitMeasure
    {
        public virtual string UnitMeasureCode { get; set; }

        [MaxLength(42)]
        public virtual string Name { get; set; }
    }
}