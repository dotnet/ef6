// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaCeModel
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.TestModels.ArubaModel;

    public class ArubaAllCeTypes
    {
        [Key]
        public int c1_int { get; set; }
        public short c2_smallint { get; set; }
        public byte c3_tinyint { get; set; }
        public bool c4_bit { get; set; }
        public DateTime c5_datetime { get; set; }
        public decimal c7_decimal_28_4 { get; set; }
        public decimal c8_numeric_28_4 { get; set; }
        public float c9_real { get; set; }
        public double c10_float { get; set; }
        public decimal c11_money { get; set; }
        public byte[] c16_binary_512_ { get; set; }
        public byte[] c17_varbinary_512_ { get; set; }
        public byte[] c18_image { get; set; }
        public string c19_nvarchar_512_ { get; set; }
        public string c20_nchar_512_ { get; set; }
        public string c21_ntext { get; set; }
        public Guid c22_uniqueidentifier { get; set; }
        public long c23_bigint { get; set; }
        public ArubaEnum c33_enum { get; set; }
        public ArubaByteEnum c34_byteenum { get; set; }
        [Timestamp]
        public byte[] c35_timestamp { get; set; }
    }
}