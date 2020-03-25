// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Metadata.Edm
{
    using System.Collections.Generic;
    using System.Linq;
    using System.Threading;
    using Xunit;

    public class EntityTypeBaseTests
    {
        [Fact]
        public void Can_remove_member()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.Members);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);

            Assert.Equal(1, entityType.KeyMembers.Count);
            Assert.Equal(1, entityType.Members.Count);

            entityType.RemoveMember(property);

            Assert.Empty(entityType.KeyMembers);
            Assert.Empty(entityType.Members);
        }

        [Fact]
        public void Can_get_list_of_key_properties()
        {
            var entityType = new EntityType("E", "N", DataSpace.CSpace);

            Assert.Empty(entityType.KeyProperties);

            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));

            entityType.AddKeyMember(property);

            Assert.Equal(1, entityType.KeyProperties.Count);
            Assert.Equal(1, entityType.KeyProperties.Where(key => entityType.DeclaredMembers.Contains(key)).Count());

            entityType.RemoveMember(property);

            var baseType = new EntityType("E", "N", DataSpace.CSpace);
            baseType.AddKeyMember(property);

            entityType.BaseType = baseType;

            Assert.Equal(1, entityType.KeyProperties.Count);
            Assert.Empty(entityType.KeyProperties.Where(key => entityType.DeclaredMembers.Contains(key)));
            Assert.Equal(1, entityType.KeyMembers.Count);
        }

        // This test is intended to prove that the KeyProperties member is thread safe. It does so by
        // executing a number of threads and have them all request KeyProperties, then reset it, then
        // requesting it again. Some assertions are tested and if they fail that means there are
        // concurrency issues in the code.
        // Note that simply running this test in your machine may not be enough to ensure the code
        // is free of issues. We realized the test failed once in the machine of one developer but ran
        // fine in other dev boxes and in the CI machine. In order to reproduce the concurrency problem it
        // was necessary to attach a debugger to this test and let it run. We did this by running the test
        // from Visual Studio with the "Debug Selected Tests" command. The result was a consistent repro
        // of the concurrency issue that would otherwise only happen sporadically in the wild.
        // It is recommended that you run this unit test under a debugger when modifying the covered code.
        [Fact]
        public void KeyProperties_is_thread_safe()
        {
            var baseType = new EntityType("E", "N", DataSpace.CSpace);
            var entityType = new EntityType("F", "N", DataSpace.CSpace);
            var property = EdmProperty.CreatePrimitive("P", PrimitiveType.GetEdmPrimitiveType(PrimitiveTypeKind.String));
            baseType.AddKeyMember(property);
            entityType.BaseType = baseType;
            const int cycles = 100;
            const int threadCount = 30;

            Action readKeyProperty = () =>
            {
                for (var i = 0; i < cycles; ++i)
                {
                    var keys = entityType.KeyProperties;

                    //touching KeyMembers.Source triggers a reset to KeyProperties
                    var sourceCount = entityType.KeyMembers.Source.Count;
                    Assert.True(sourceCount == 1);

                    var keysAfterReset = entityType.KeyProperties;

                    Assert.True(keys != null, "First reference to key properties should not be null");
                    Assert.True(keysAfterReset != null, "Second reference to key properties should not be null");
                    Assert.False(ReferenceEquals(keys, keysAfterReset), "The key properties instances should be different");
                }
            };

            var tasks = new List<Thread>();
            for (var i = 0; i < threadCount; ++i)
            {
                var thread = new Thread(new ThreadStart(readKeyProperty));
                tasks.Add(thread);
            }

            tasks.ForEach(t => t.Start());
            tasks.ForEach(t => t.Join());
        }
    }
}
