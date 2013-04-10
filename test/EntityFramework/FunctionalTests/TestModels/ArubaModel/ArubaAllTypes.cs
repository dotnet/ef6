// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaModel
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Spatial;

    public enum ArubaEnum
    {
        EnumValue0 = 0,
        EnumValue1 = 1,
        EnumValue2 = 2,
        EnumValue3 = 3,
    };

    public enum ArubaByteEnum : byte
    {
        ByteEnumValue0 = 0,
        ByteEnumValue1 = 1,
        ByteEnumValue2 = 2,        
    }

    public class ArubaAllTypes
    {
        [Key]
        public int c1_int { get; set; }
        public short c2_smallint { get; set; }
        public byte c3_tinyint { get; set; }
        public bool c4_bit { get; set; }
        public DateTime c5_datetime { get; set; }
        public DateTime c6_smalldatetime { get; set; }
        public decimal c7_decimal_28_4 { get; set; }
        public decimal c8_numeric_28_4 { get; set; }
        public float c9_real { get; set; }
        public double c10_float { get; set; }
        public decimal c11_money { get; set; }
        public decimal c12_smallmoney { get; set; }
        public string c13_varchar_512_ { get; set; }
        public string c14_char_512_ { get; set; }
        public string c15_text { get; set; }
        public byte[] c16_binary_512_ { get; set; }
        public byte[] c17_varbinary_512_ { get; set; }
        public byte[] c18_image { get; set; }
        public string c19_nvarchar_512_ { get; set; }
        public string c20_nchar_512_ { get; set; }
        public string c21_ntext { get; set; }
        public Guid c22_uniqueidentifier { get; set; }
        public long c23_bigint { get; set; }
        public string c24_varchar_max_ { get; set; }
        public string c25_nvarchar_max_ { get; set; }
        public byte[] c26_varbinary_max_ { get; set; }
        public TimeSpan c27_time { get; set; }
        public DateTime c28_date { get; set; }
        public DateTime c29_datetime2 { get; set; }
        public DateTimeOffset c30_datetimeoffset { get; set; }
        public DbGeography c31_geography { get; set; }
        public DbGeometry c32_geometry { get; set; }
        public ArubaEnum c33_enum { get; set; }
        public ArubaByteEnum c34_byteenum { get; set; }
        [Timestamp]
        public byte[] c35_timestamp { get; set; }
        public DbGeometry c36_geometry_linestring { get; set; }
        public DbGeometry c37_geometry_polygon { get; set; }
    }
}