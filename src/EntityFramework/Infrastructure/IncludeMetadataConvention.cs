// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Infrastructure
{
    using System.Data.Entity.Internal;
    using System.Data.Entity.ModelConfiguration.Configuration;
    using System.Data.Entity.ModelConfiguration.Conventions;
    using System.Data.Entity.Utilities;

    /// <summary>
    /// This <see cref="DbModelBuilder" /> convention causes DbModelBuilder to include metadata about the model
    /// when it builds the model. When <see cref="DbContext" /> creates a model by convention it will
    /// add this convention to the list of those used by the DbModelBuilder.  This will then result in
    /// model metadata being written to the database if the DbContext is used to create the database.
    /// This can then be used as a quick check to see if the model has changed since the last time it was
    /// used against the database.
    /// This convention can be removed from the <see cref="DbModelBuilder" /> conventions by overriding
    /// the OnModelCreating method on a derived DbContext class.
    /// </summary>
    [Obsolete(
        "The IncludeMetadataConvention is no longer used. EdmMetadata is not included in the model. <see cref=\"EdmModelDiffer\" /> is now used to detect changes in the model."
        )]
    public class IncludeMetadataConvention : Convention
    {
        /// <summary>
        /// Adds metadata to the given model configuration.
        /// </summary>
        /// <param name="modelConfiguration"> The model configuration. </param>
        internal virtual void Apply(ModelConfiguration modelConfiguration)
        {
            Check.NotNull(modelConfiguration, "modelConfiguration");

            EdmMetadataContext.ConfigureEdmMetadata(modelConfiguration);
        }
    }
}
