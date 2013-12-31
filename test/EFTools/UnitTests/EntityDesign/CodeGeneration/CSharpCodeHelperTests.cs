// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.CodeGeneration
{
    using System;
    using System.Collections.Generic;
    using System.Data.Entity;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Linq;
    using Moq;
    using Xunit;

    public class CSharpCodeHelperTests
    {
        private static DbModel _model;

        private static DbModel Model
        {
            get
            {
                if (_model == null)
                {
                    var modelBuilder = new DbModelBuilder();
                    modelBuilder.Entity<Entity>().HasMany(e => e.Children).WithOptional(e => e.Parent);

                    _model = modelBuilder.Build(new DbProviderInfo("System.Data.SqlClient", "2012"));
                }

                return _model;
            }
        }

        [Fact]
        public void Type_evaluates_preconditions()
        {
            ArgumentException ex;
            var code = new CSharpCodeHelper();

            ex = Assert.Throws<ArgumentNullException>(() => code.Type((EntityContainer)null));
            Assert.Equal("container", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => code.Type((EdmType)null));
            Assert.Equal("edmType", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => code.Type((EdmProperty)null));
            Assert.Equal("property", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => code.Type((NavigationProperty)null));
            Assert.Equal("navigationProperty", ex.ParamName);
        }

        [Fact]
        public void Type_returns_container_name()
        {
            var container = Model.ConceptualModel.Container;
            var code = new CSharpCodeHelper();

            Assert.Equal("CodeFirstContainer", code.Type(container));
        }

        [Fact]
        public void Type_escapes_container_name()
        {
            var container = new Mock<EntityContainer>();
            container.SetupGet(c => c.Name).Returns("null");
            var code = new CSharpCodeHelper();

            Assert.Equal("_null", code.Type(container.Object));
        }

        [Fact]
        public void Type_returns_type_name()
        {
            var container = Model.ConceptualModel.EntityTypes.First();
            var code = new CSharpCodeHelper();

            Assert.Equal("Entity", code.Type(container));
        }

        [Fact]
        public void Type_escapes_type_name()
        {
            var type = new Mock<EdmType>();
            type.SetupGet(c => c.Name).Returns("null");
            var code = new CSharpCodeHelper();

            Assert.Equal("_null", code.Type(type.Object));
        }

        [Fact]
        public void Type_returns_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().Properties.First(
                p => p.Name == "Name");
            var code = new CSharpCodeHelper();

            Assert.Equal("string", code.Type(property));
        }

        [Fact]
        public void Type_returns_value_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().Properties.First(
                p => p.Name == "Id");
            var code = new CSharpCodeHelper();

            Assert.Equal("int", code.Type(property));
        }

        [Fact]
        public void Type_returns_nullable_value_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().Properties.First(
                p => p.Name == "ParentId");
            var code = new CSharpCodeHelper();

            Assert.Equal("int?", code.Type(property));
        }

        [Fact]
        public void Type_unqualifies_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().Properties.First(
                p => p.Name == "Guid");
            var code = new CSharpCodeHelper();

            Assert.Equal("Guid", code.Type(property));
        }

        [Fact]
        public void Type_returns_reference_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().NavigationProperties.First(
                p => p.Name == "Parent");
            var code = new CSharpCodeHelper();

            Assert.Equal("Entity", code.Type(property));
        }

        [Fact]
        public void Type_returns_collection_property_type()
        {
            var property = Model.ConceptualModel.EntityTypes.First().NavigationProperties.First(
                p => p.Name == "Children");
            var code = new CSharpCodeHelper();

            Assert.Equal("ICollection<Entity>", code.Type(property));
        }

        [Fact]
        public void Property_evaluates_preconditions()
        {
            ArgumentException ex;
            var code = new CSharpCodeHelper();

            ex = Assert.Throws<ArgumentNullException>(() => code.Property((EntitySetBase)null));
            Assert.Equal("entitySet", ex.ParamName);

            ex = Assert.Throws<ArgumentNullException>(() => code.Property((EdmMember)null));
            Assert.Equal("member", ex.ParamName);
        }

        [Fact]
        public void Property_returns_entity_set_name()
        {
            var entitySet = Model.ConceptualModel.Container.EntitySets.First();
            var code = new CSharpCodeHelper();

            Assert.Equal("Entities", code.Property(entitySet));
        }

        [Fact]
        public void Property_escapes_entity_set_name()
        {
            var set = new Mock<EntitySetBase>();
            set.SetupGet(s => s.Name).Returns("null");
            var code = new CSharpCodeHelper();

            Assert.Equal("_null", code.Property(set.Object));
        }

        [Fact]
        public void Property_returns_member_name()
        {
            var member = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Id");
            var code = new CSharpCodeHelper();

            Assert.Equal("Id", code.Property(member));
        }

        [Fact]
        public void Property_escapes_member_name()
        {
            var member = new Mock<EdmMember>();
            member.SetupGet(m => m.Name).Returns("null");
            var code = new CSharpCodeHelper();

            Assert.Equal("_null", code.Property(member.Object));
        }

        [Fact]
        public void Attribute_evaluates_preconditions()
        {
            var code = new CSharpCodeHelper();

            var ex = Assert.Throws<ArgumentNullException>(() => code.Attribute(null));
            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void Attribute_surrounds_body()
        {
            var code = new CSharpCodeHelper();
            var configuration = new Mock<IAttributeConfiguration>();
            configuration.Setup(c => c.GetAttributeBody(code)).Returns("Required");

            Assert.Equal("[Required]", code.Attribute(configuration.Object));
        }

        [Fact]
        public void MethodChain_evaluates_preconditions()
        {
            var code = new CSharpCodeHelper();

            var ex = Assert.Throws<ArgumentNullException>(() => code.MethodChain(null));
            Assert.Equal("configuration", ex.ParamName);
        }

        [Fact]
        public void MethodChain_calls_GetMethodChain_on_configuration()
        {
            var code = new CSharpCodeHelper();
            var configuration = new Mock<IFluentConfiguration>();

            code.MethodChain(configuration.Object);

            configuration.Verify(c => c.GetMethodChain(code));
        }

        [Fact]
        public void Literal_returns_string_when_one()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("\"One\"", code.Literal(new[] { "One" }));
        }

        [Fact]
        public void Literal_returns_string_array_when_more_than_one()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("new[] { \"One\", \"Two\" }", code.Literal(new[] { "One", "Two" }));
        }

        [Fact]
        public void Literal_returns_string()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("\"One\"", code.Literal("One"));
        }

        [Fact]
        public void Literal_returns_int()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("42", code.Literal(42));
        }

        [Fact]
        public void Literal_returns_bool()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("true", code.Literal(true));
            Assert.Equal("false", code.Literal(false));
        }

        [Fact]
        public void BeginLambda_returns_lambda_beginning()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("x => ", code.BeginLambda("x"));
        }

        [Fact]
        public void Lambda_returns_property_accessor_when_one()
        {
            var code = new CSharpCodeHelper();
            var member = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Id");

            Assert.Equal("e => e.Id", code.Lambda(member));
        }

        [Fact]
        public void Lambda_returns_anonymous_type_when_one()
        {
            var code = new CSharpCodeHelper();
            var id = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Id");
            var name = Model.ConceptualModel.EntityTypes.First().Properties.First(p => p.Name == "Name");

            Assert.Equal("e => new { e.Id, e.Name }", code.Lambda(new[] { id, name }));
        }

        [Fact]
        public void TypeArgument_surrounds_value()
        {
            var code = new CSharpCodeHelper();

            Assert.Equal("<Entity>", code.TypeArgument("Entity"));
        }

        private class Entity
        {
            public int Id { get; set; }
            public string Name { get; set; }
            public Guid Guid { get; set; }
            public int? ParentId { get; set; }
            public Entity Parent { get; set; }
            public ICollection<Entity> Children { get; set; }
        }
    }
}
