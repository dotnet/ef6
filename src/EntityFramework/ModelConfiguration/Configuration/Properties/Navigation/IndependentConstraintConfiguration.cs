namespace System.Data.Entity.ModelConfiguration.Configuration.Properties.Navigation
{
    using System.Data.Entity.Edm;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Data.Entity.ModelConfiguration.Edm;
    using System.Diagnostics.Contracts;

    internal class IndependentConstraintConfiguration : ConstraintConfiguration
    {
        public static readonly ConstraintConfiguration Instance = new IndependentConstraintConfiguration();

        private IndependentConstraintConfiguration()
        {
        }

        internal override ConstraintConfiguration Clone()
        {
            return Instance;
        }

        internal override void Configure(
            EdmAssociationType associationType, EdmAssociationEnd dependentEnd, EntityTypeConfiguration entityTypeConfiguration)
        {
            associationType.MarkIndependent();
        }
    }
}