namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.ComponentModel.DataAnnotations;
    using System.ComponentModel.DataAnnotations.Schema;
    using System.Data.Entity.ModelConfiguration.Configuration.Types;
    using System.Diagnostics.Contracts;

    /// <summary>
    ///     Convention to process instances of <see cref = "TableAttribute" /> found on types in the model.
    /// </summary>
    public sealed class TableAttributeConvention : IConfigurationConvention<Type, EntityTypeConfiguration>
    {
        private readonly IConfigurationConvention<Type, EntityTypeConfiguration> _impl
            = new TableAttributeConventionImpl();

        internal TableAttributeConvention()
        {
        }

        void IConfigurationConvention<Type, EntityTypeConfiguration>.Apply(Type memberInfo, Func<EntityTypeConfiguration> configuration)
        {
            _impl.Apply(memberInfo, configuration);
        }

        internal sealed class TableAttributeConventionImpl : AttributeConfigurationConvention<Type, EntityTypeConfiguration, TableAttribute>
        {
            internal override void Apply(Type type, EntityTypeConfiguration entityTypeConfiguration, TableAttribute tableAttribute)
            {
                if (!entityTypeConfiguration.IsTableNameConfigured)
                {
                    if (string.IsNullOrWhiteSpace(tableAttribute.Schema))
                    {
                        entityTypeConfiguration.ToTable(tableAttribute.Name);
                    }
                    else
                    {
                        entityTypeConfiguration.ToTable(tableAttribute.Name, tableAttribute.Schema);
                    }
                }
            }
        }
    }
}