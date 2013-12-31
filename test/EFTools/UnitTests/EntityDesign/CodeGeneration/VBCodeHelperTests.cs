// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Moq;
    using Xunit;

    public class VBCodeHelperTests
    {
        private static DbModel _model;

        private static DbModel Model
        {
            get
            {
                if (_model == null)
                {
                    var modelBuilder = new DbModelBuilder();
                    modelBuilder.Entity<Entity>();

                    _model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
                }

                return _model;
            }
        }

        [Fact]
        public void Type_escapes_container_name()
        {
            var container = new Mock<EntityContainer>();
            container.SetupGet(c => c.Name).Returns("Nothing");
            var code = new VBCodeHelper();

            Assert.Equal("_Nothing", code.Type(container.Object));
        }

        [Fact]
        public void Type_returns_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().Properties.First(
                p => p.Name == "Name");
            var code = new VBCodeHelper();

            Assert.Equal("String", code.Type(property));
        }

        [Fact]
        public void Type_returns_collection_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().NavigationProperties.First(
                p => p.Name == "Children");
            var code = new VBCodeHelper();

            Assert.Equal("ICollection(Of Entity)", code.Type(property));
        }

        [Fact]
        public void Attribute_surrounds_body()
        {
            var code = new VBCodeHelper();
            var configuration = new Mock<IAttributeConfiguration>();
            configuration.Setup(c => c.GetAttributeBody(code)).Returns("Required");

            Assert.Equal("<Required>", code.Attribute(configuration.Object));
        }

        [Fact]
        public void Literal_returns_string_array_when_more_than_one()
        {
            var code = new VBCodeHelper();

            Assert.Equal("{\"One\", \"Two\"}", code.Literal(new[] { "One", "Two" }));
        }

        [Fact]
        public void Literal_returns_bool()
        {
            var code = new VBCodeHelper();

            Assert.Equal("True", code.Literal(true));
            Assert.Equal("False", code.Literal(false));
        }

        [Fact]
        public void BeginLambda_returns_lambda_beginning()
        {
            var code = new VBCodeHelper();

            Assert.Equal("Function(x) ", code.BeginLambda("x"));
        }

        [Fact]
        public void Lambda_returns_property_accessor_when_one()
        {
            var code = new VBCodeHelper();
            var member = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Id");

            Assert.Equal("Function(e) e.Id", code.Lambda(member));
        }

        [Fact]
        public void Lambda_returns_anonymous_type_when_one()
        {
            var code = new VBCodeHelper();
            var id = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Id");
            var name = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Name");

            Assert.Equal("Function(e) New With {e.Id, e.Name}", code.Lambda(new[] { id, name }));
        }

        [Fact]
        public void TypeArgument_surrounds_value()
        {
            var code = new VBCodeHelper();

            Assert.Equal("(Of Entity)", code.TypeArgument("Entity"));
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public ICollection<Entity> Children { get; set; }
        }
    }
}
