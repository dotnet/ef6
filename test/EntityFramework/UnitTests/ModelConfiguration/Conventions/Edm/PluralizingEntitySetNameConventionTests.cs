// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Conventions.UnitTests
{
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Edm;
    using Xunit;

    public sealed class PluralizingEntitySetNameConventionTests
    {
        [Fact]
        public void Apply_should_set_pluralized_name()
        {
            var model = new EdmModel().Initialize();
            var entitySet = model.AddEntitySet("Cat", new EntityType());

            ((IEdmConvention<EntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet, model);

            Assert.Equal("Cats", entitySet.Name);
        }

        [Fact]
        public void Apply_should_ignore_current_entity_set()
        {
            var model = new EdmModel().Initialize();
            var entitySet = model.AddEntitySet("Cats", new EntityType());

            ((IEdmConvention<EntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet, model);

            Assert.Equal("Cats", entitySet.Name);
        }

        [Fact]
        public void Apply_should_uniquify_names()
        {
            var model = new EdmModel().Initialize();
            model.AddEntitySet("Cats", new EntityType());
            var entitySet = model.AddEntitySet("Cat", new EntityType());

            ((IEdmConvention<EntitySet>)new PluralizingEntitySetNameConvention())
                .Apply(entitySet, model);

            Assert.Equal("Cats1", entitySet.Name);
        }

        // TODO: METADATA
//        [Fact(Skip = "Need to figure out name duplication in core Metadata")]
//        public void Apply_should_uniquify_names_multiple()
//        {
//            var model = new EdmModel().Initialize();
//            model.AddEntitySet("Cats1", new EntityType());
//            var entitySet1 = model.AddEntitySet("Cat", new EntityType());
//            var entitySet2 = model.AddEntitySet("Cat", new EntityType());
//
//            ((IEdmConvention<EntitySet>)new PluralizingEntitySetNameConvention())
//                .Apply(entitySet1, model);
//
//            ((IEdmConvention<EntitySet>)new PluralizingEntitySetNameConvention())
//                .Apply(entitySet2, model);
//
//            Assert.Equal("Cats", entitySet1.Name);
//            Assert.Equal("Cats2", entitySet2.Name);
//        }
    }
}
