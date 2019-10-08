// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.ModelConfiguration.Configuration.Mapping
{
    using System.ComponentModel.DataAnnotations;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Infrastructure;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Linq;
    using Xunit;

    public class Codeplex2465
    {
        [Fact]
        public void Model_is_built_successfully()
        {
            DbModel model = null;
            var builder = new DbModelBuilder();

            builder.Configurations.Add(new EntityTypeConfiguration());

            model = builder.Build(ProviderRegistry.Sql2008_ProviderInfo);

            var storeModel = model.StoreModel;

            Assert.Equal(
                new[] { "Customer", "FreightTerms", "PaymentTerms", "Customer1", "Customer2" }, 
                storeModel.EntityTypes.Select(t => t.Name));
            Assert.Equal(
                new[] { "GLOBAL.F_CUSTOMER", null, null, "GLOBAL.F_CUSTOMER_PREFS", "GLOBAL.F_CUSTOMER_CREDIT" }, 
                storeModel.EntityTypes.Select(t => t.GetTableName() != null ? t.GetTableName().ToString() : null));

            var customer0 = storeModel.EntityTypes.ElementAt(0);
            var customer1 = storeModel.EntityTypes.ElementAt(3);
            var customer2 = storeModel.EntityTypes.ElementAt(4);

            Assert.Equal(new[] { "COMPANY_NO", "ACTIVE_FL" }, customer0.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "COMPANY_NO", "LABEL_NOTES", "FREIGHT_TERM_NO" }, customer1.Properties.Select(p => p.Name));
            Assert.Equal(new[] { "COMPANY_NO", "PAYMENT_TERM_NO" }, customer2.Properties.Select(p => p.Name));

            Assert.Equal(
                new[] { "Customer_FreightTerms", "Customer_TypeConstraint_From_Customer_To_Customer1", "Customer_PaymentTerms", "Customer_TypeConstraint_From_Customer_To_Customer2" }, 
                storeModel.AssociationTypes.Select(t => t.Name));
            Assert.Equal(
                new[] { "Customer_FreightTerms", "Customer_TypeConstraint_From_Customer_To_Customer1", "Customer_PaymentTerms", "Customer_TypeConstraint_From_Customer_To_Customer2" },
                storeModel.AssociationTypes.Select(t => storeModel.GetAssociationSet(t).Name));

            var associationType0 = storeModel.AssociationTypes.ElementAt(0);
            var associationType1 = storeModel.AssociationTypes.ElementAt(1);
            var associationType2 = storeModel.AssociationTypes.ElementAt(2);
            var associationType3 = storeModel.AssociationTypes.ElementAt(3);

            Assert.Equal(RelationshipMultiplicity.One, associationType0.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, associationType0.TargetEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType1.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType1.TargetEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType2.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.Many, associationType2.TargetEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.One, associationType3.SourceEnd.RelationshipMultiplicity);
            Assert.Equal(RelationshipMultiplicity.ZeroOrOne, associationType3.TargetEnd.RelationshipMultiplicity);
        }

        public class Customer
        {
            public int CompanyNumber { get; set; }
            public bool Active { get; set; }
            public int PaymentTermsNumber { get; set; }
            public string LabelNotes { get; set; }
            public int FreightTermsNumber { get; set; }
            public virtual PaymentTerms PaymentTerms { get; set; }
            public virtual FreightTerms FreightTerms { get; set; }
        }

        public class PaymentTerms
        {
            [Key]
            public int PaymentTermsNumber { get; set; }
            public string Description { get; set; }
        }

        public class FreightTerms
        {
            [Key]
            public int FreightTermsNumber { get; set; }
            public string Description { get; set; }
        }

        public class EntityTypeConfiguration : EntityTypeConfiguration<Customer>
        {
            public EntityTypeConfiguration()
            {
                Property(p => p.CompanyNumber).HasColumnName("COMPANY_NO");

                HasKey(k => k.CompanyNumber);

                Map(m =>
                {
                    m.Property(p => p.Active).HasColumnName("ACTIVE_FL");
                    m.ToTable("F_CUSTOMER", "GLOBAL");
                });

                Map(m =>
                {
                    m.Property(p => p.LabelNotes).HasColumnName("LABEL_NOTES");
                    m.Property(p => p.FreightTermsNumber).HasColumnName("FREIGHT_TERM_NO");
                    m.ToTable("F_CUSTOMER_PREFS", "GLOBAL");
                });

                Map(m =>
                {
                    m.Property(p => p.PaymentTermsNumber).HasColumnName("PAYMENT_TERM_NO");
                    m.ToTable("F_CUSTOMER_CREDIT", "GLOBAL");
                });

                HasRequired(p => p.FreightTerms).WithMany().HasForeignKey(p => p.FreightTermsNumber);
                HasRequired(p => p.PaymentTerms).WithMany().HasForeignKey(p => p.PaymentTermsNumber);
            }
        }
    }
}
