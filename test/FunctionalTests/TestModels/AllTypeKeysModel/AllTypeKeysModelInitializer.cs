// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace AllTypeKeysModel
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;

    public class AllTypeKeysModelInitializer : DropCreateDatabaseIfModelChanges<AllTypeKeysContext>
    {
        protected override void Seed(AllTypeKeysContext context)
        {
            var entities = new List<CompositeKeyEntity>
                {
                    new CompositeKeyEntity
                        {
                            binaryKey = new byte[] { 1, 2, 3, 4, 5, 6 },
                            intKey = 6,
                            stringKey = "TheOneWithBinaryKeyLength6",
                            Details = "Some Random Stuff"
                        },
                    new CompositeKeyEntity
                        {
                            binaryKey = new byte[] { 201, 202, 203, 204 },
                            intKey = 4,
                            stringKey = "TheOneWithBinaryKeyLength4",
                            Details = "Does it Matter?"
                        },
                    new CompositeKeyEntity
                        {
                            binaryKey = new byte[] { 101, 102, 103, 104, 105 },
                            intKey = 5,
                            stringKey = "TheOneWithBinaryKeyLength5",
                            Details = "Maybe Details are important!"
                        },
                };

            foreach (var entity in entities)
            {
                context.CompositeKeyEntities.Add(entity);
            }

            var withOrdering = new List<CompositeKeyEntityWithOrderingAnnotations>
                {
                    new CompositeKeyEntityWithOrderingAnnotations
                        {
                            intKey = 1,
                            stringKey = "TheOneWithBinaryKeyLength1",
                            binaryKey = new byte[] { 220 }
                        },
                    new CompositeKeyEntityWithOrderingAnnotations
                        {
                            intKey = 2,
                            stringKey = "TheOneWithBinaryKeyLength2",
                            binaryKey = new byte[] { 220, 221 }
                        },
                    new CompositeKeyEntityWithOrderingAnnotations
                        {
                            intKey = 3,
                            stringKey = "TheOneWithBinaryKeyLength3",
                            binaryKey = new byte[] { 230, 231, 232 }
                        },
                };

            foreach (var entity in withOrdering)
            {
                context.CompositeKeyEntitiesWithOrderingAnnotations.Add(entity);
            }

            context.Set<BoolKeyEntity>().Add(
                new BoolKeyEntity
                    {
                        key = true,
                        Description = "BoolKeyEntity"
                    });
            context.Set<ByteKeyEntity>().Add(
                new ByteKeyEntity
                    {
                        key = 255,
                        Description = "ByteKeyEntity"
                    });
            context.Set<DateTimeKeyEntity>().Add(
                new DateTimeKeyEntity
                    {
                        key = new DateTime(2008, 5, 1, 8, 30, 52),
                        Description = "DateTimeKeyEntity"
                    });
            context.Set<DateTimeOffSetKeyEntity>().Add(
                new DateTimeOffSetKeyEntity
                    {
                        key = new DateTimeOffset(new DateTime(2008, 5, 1, 8, 30, 52)),
                        Description = "DateTimeOffSetKeyEntity"
                    });
            context.Set<DecimalKeyEntity>().Add(
                new DecimalKeyEntity
                    {
                        key = 300.5m,
                        Description = "DecimalKeyEntity"
                    });
            context.Set<DoubleKeyEntity>().Add(
                new DoubleKeyEntity
                    {
                        key = 1.7E+3D,
                        Description = "DoubleKeyEntity"
                    });
            context.Set<FloatKeyEntity>().Add(
                new FloatKeyEntity
                    {
                        key = 33.2F,
                        Description = "FloatKeyEntity"
                    });
            context.Set<GuidKeyEntity>().Add(
                new GuidKeyEntity
                    {
                        key = Guid.Parse("F9168C5E-CEB2-4faa-B6BF-329BF39FA1E4"),
                        Description = "GuidKeyEntity"
                    });
            context.Set<LongKeyEntity>().Add(
                new LongKeyEntity
                    {
                        key = 4294967296L,
                        Description = "LongKeyEntity"
                    });
            context.Set<ShortKeyEntity>().Add(
                new ShortKeyEntity
                    {
                        key = 32767,
                        Description = "ShortKeyEntity"
                    });
            context.Set<TimeSpanKeyEntity>().Add(
                new TimeSpanKeyEntity
                    {
                        key = new TimeSpan(2, 14, 18),
                        Description = "TimeSpanKeyEntity"
                    });
        }
    }
}
