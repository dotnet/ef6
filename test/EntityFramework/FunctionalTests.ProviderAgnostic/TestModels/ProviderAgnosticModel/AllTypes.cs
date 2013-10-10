// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ProviderAgnosticModel
{
    using System;

    public enum AllTypesEnum
    {
        EnumValue0 = 0,
        EnumValue1 = 1,
        EnumValue2 = 2,
        EnumValue3 = 3,
    };

    public class AllTypes
    {
        public int Id { get; set; }
        public bool BooleanProperty { get; set; }
        public byte ByteProperty { get; set; }
        public DateTime DateTimeProperty { get; set; }
        public decimal DecimalProperty { get; set; }
        public double DoubleProperty { get; set; }
        public byte[] FixedLengthBinaryProperty { get; set; }
        public string FixedLengthStringProperty { get; set; }
        public string FixedLengthUnicodeStringProperty { get; set; }
        public float FloatProperty { get; set; }
        public Guid GuidProperty { get; set; }
        public short Int16Property { get; set; }
        public int Int32Property { get; set; }
        public long Int64Property { get; set; }
        public byte[] MaxLengthBinaryProperty { get; set; }
        public string MaxLengthStringProperty { get; set; }
        public string MaxLengthUnicodeStringProperty { get; set; }
        public TimeSpan TimeSpanProperty { get; set; }
        public string VariableLengthStringProperty { get; set; }
        public byte[] VariableLengthBinaryProperty { get; set; }
        public string VariableLengthUnicodeStringProperty { get; set; }
        public AllTypesEnum EnumProperty { get; set; }
    }
}
