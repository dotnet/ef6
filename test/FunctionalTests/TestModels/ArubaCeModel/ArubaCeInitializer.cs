// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.TestModels.ArubaCeModel
{
    using System.Data.Entity.TestModels.ArubaModel;
    using System.Linq;

    public class ArubaCeInitializer : DropCreateDatabaseIfModelChanges<ArubaCeContext>
    {
        private const int EntitiesCount = 10;

        protected override void Seed(ArubaCeContext context)
        {
            var allTypes = InitializeAllTypes();

            for (int i = 0; i < EntitiesCount; i++)
            {
                context.AllTypes.Add(allTypes[i]);
            }

            context.SaveChanges();
            base.Seed(context);
        }

        private ArubaAllCeTypes[] InitializeAllTypes()
        {
            var allTypesList = new ArubaAllCeTypes[EntitiesCount];
            for (var i = 1; i < EntitiesCount + 1; i++)
            {
                var allTypes = new ArubaAllCeTypes
                    {
                        c2_smallint = (short)i,
                        c3_tinyint = (byte)i,
                        c4_bit = i % 2 == 0,
                        c5_datetime = new DateTime(1990, i % 12 + 1, i % 28 + 1, i % 12, i % 60, i % 60),
                        c7_decimal_28_4 = 10 + (decimal)((double)i / 4),
                        c8_numeric_28_4 = -5 + (decimal)((double)i / 8),
                        c9_real = (float)i / 3,
                        c10_float = i + (double)i / 3,
                        c11_money = i + (decimal)((double)i / 5),
                        c16_binary_512_ = Enumerable.Repeat<byte>((byte)i, 512).ToArray(),
                        c17_varbinary_512_ = Enumerable.Repeat<byte>((byte)i, 1 + i % 7).ToArray(),
                        c18_image = Enumerable.Repeat<byte>((byte)i, i + 10).ToArray(),
                        c19_nvarchar_512_ = new string((char)(i + 'a'), i),
                        c20_nchar_512_ = new string((char)(i + 'a'), 512),
                        c21_ntext = new string((char)(i + 'a'), 20 + i) + "unicorn",
                        c22_uniqueidentifier = new Guid(new string((char)((i % 5) + '0'), 32)),
                        c23_bigint = (long)i * 10,
                        c33_enum = (ArubaEnum)(i % 4),
                        c34_byteenum = (ArubaByteEnum)(i % 3)
                    };

                allTypesList[i-1] = allTypes;
            }

            return allTypesList;
        }

    }
}
