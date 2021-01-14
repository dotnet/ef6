// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class PluralizingEntitySetNameConventionTests
    {
        [Fact]
        public void Apply_should_set_pluralized_name()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entitySet = model.AddEntitySet("Cat", new EntityType("E", "N", DataSpace.CSpace));

            (new PluralizingEntitySetNameConvention())
                .Apply(entitySet, new DbModel(model, null));

            Assert.Equal("Cats", entitySet.Name);
        }

        [Fact]
        public void Apply_should_ignore_current_entity_set()
        {
            var model = new EdmModel(DataSpace.CSpace);
            var entitySet = model.AddEntitySet("Cats", new EntityType("E", "N", DataSpace.CSpace));

            (new PluralizingEntitySetNameConvention())
                .Apply(entitySet, new DbModel(model, null));

            Assert.Equal("Cats", entitySet.Name);
        }

        [Fact]
        public void Apply_should_uniquify_names()
        {
            var model = new EdmModel(DataSpace.CSpace);
            model.AddEntitySet("Cats", new EntityType("E", "N", DataSpace.CSpace));
            var entitySet = model.AddEntitySet("Cat", new EntityType("E", "N", DataSpace.CSpace));

            (new PluralizingEntitySetNameConvention())
                .Apply(entitySet, new DbModel(model, null));

            Assert.Equal("Cats1", entitySet.Name);
        }

        [Fact]
        public void Apply_should_uniquify_names_multiple()
        {
            var model = new EdmModel(DataSpace.CSpace);
            model.AddEntitySet("Cats1", new EntityType("E", "N", DataSpace.CSpace));
            var entitySet1 = model.AddEntitySet("Cats", new EntityType("E", "N", DataSpace.CSpace));
            var entitySet2 = model.AddEntitySet("Cat", new EntityType("E", "N", DataSpace.CSpace));

            (new PluralizingEntitySetNameConvention())
                .Apply(entitySet1, new DbModel(model, null));

            (new PluralizingEntitySetNameConvention())
                .Apply(entitySet2, new DbModel(model, null));

            Assert.Equal("Cats", entitySet1.Name);
            Assert.Equal("Cats2", entitySet2.Name);
        }
    }
}
