namespace System.Data.Entity.ModelConfiguration.Conventions
{
    using System.Data.Entity.Edm.Db;
    using System.Data.Entity.ModelConfiguration.Edm.Db;
    using System.Data.Entity.Resources;
    using System.Linq;

    /// <summary>
    ///     Convention to apply column ordering specified via <see cref = "T:System.ComponentModel.DataAnnotations.ColumnAttribute" /> 
    ///     or the <see cref = "DbModelBuilder" /> API. This convention throws if a duplicate configured column order
    ///     is detected.
    /// </summary>
    internal sealed class ColumnOrderingConventionStrict : ColumnOrderingConvention
    {
        protected override void ValidateColumns(DbTableMetadata table)
        {
            var hasDuplicates
                = table.Columns
                    .Select(c => c.GetOrder())
                    .Where(o => o != null)
                    .GroupBy(o => o)
                    .Where(g => g.Count() > 1)
                    .Any();

            if (hasDuplicates)
            {
                throw Error.DuplicateConfiguredColumnOrder(table.DatabaseIdentifier);
            }
        }
    }
}
