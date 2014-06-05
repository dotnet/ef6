// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Xunit;

    // TODO - remove this comment - this is just to force line-endings (which git does not seem to accept as the only change)
    public class MultiplicityConfigurationTests
    {
        [Fact]
        public void GetMethodChain_returns_chan_when_many_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithMany(e => e.Entity1s);
            modelBuilder.Entity<Entity1>().Ignore(e => e.Entity2);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1s");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasMany(e => e.Entity2s)
                .WithMany(e => e.Entity1s)",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chan_when_many_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithRequired(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasMany(e => e.Entity2s)
                .WithRequired(e => e.Entity1)",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chan_when_many_to_optional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasMany(e => e.Entity2s).WithOptional(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2s");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasMany(e => e.Entity2s)
                .WithOptional(e => e.Entity1)",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chan_when_required_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Entity2).WithMany(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1s");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasRequired(e => e.Entity2)
                .WithMany(e => e.Entity1s)",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chan_when_required_to_optional()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasRequired(e => e.Entity2).WithOptional(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasRequired(e => e.Entity2)
                .WithOptional(e => e.Entity1)",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chan_when_optional_to_many()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasOptional(e => e.Entity2).WithMany(e => e.Entity1s);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1s");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasOptional(e => e.Entity2)
                .WithMany(e => e.Entity1s)",
                configuration.GetMethodChain(code));
        }

        [Fact]
        public void GetMethodChain_returns_chan_when_optional_to_required()
        {
            var modelBuilder = new DbModelBuilder();
            modelBuilder.Entity<Entity1>().HasOptional(e => e.Entity2).WithRequired(e => e.Entity1);
            var model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
            var entityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity1");
            var navigationProperty = entityType.NavigationProperties.First(p => p.Name == "Entity2");
            var otherEntityType = model.ConceptualModel.EntityTypes.First(t => t.Name == "Entity2");
            var otherNavigationProperty = otherEntityType.NavigationProperties.First(p => p.Name == "Entity1");

            var configuration = new MultiplicityConfiguration
                {
                    LeftEntityType = entityType,
                    LeftNavigationProperty = navigationProperty,
                    RightNavigationProperty = otherNavigationProperty
                };
            var code = new CSharpCodeHelper();

            Assert.Equal(
                @".Entity<Entity1>()
                .HasOptional(e => e.Entity2)
                .WithRequired(e => e.Entity1)",
                configuration.GetMethodChain(code));
        }

        private class Entity1
        {
            public int Id { get; set; }
            public Entity2 Entity2 { get; set; }
            public ICollection<Entity2> Entity2s { get; set; }
        }

        private class Entity2
        {
            public int Id { get; set; }
            public Entity1 Entity1 { get; set; }
            public ICollection<Entity1> Entity1s { get; set; }
        }
    }
}
