// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;

    internal class SchemaFilterEntryBag
    {
        internal ICollection<EntityStoreSchemaFilterEntry> IncludedTableEntries { get; set; }
        internal ICollection<EntityStoreSchemaFilterEntry> IncludedViewEntries { get; set; }
        internal ICollection<EntityStoreSchemaFilterEntry> IncludedSprocEntries { get; set; }
        internal ICollection<EntityStoreSchemaFilterEntry> ExcludedTableEntries { get; set; }
        internal ICollection<EntityStoreSchemaFilterEntry> ExcludedViewEntries { get; set; }
        internal ICollection<EntityStoreSchemaFilterEntry> ExcludedSprocEntries { get; set; }

        internal SchemaFilterEntryBag()
        {
            IncludedTableEntries = new List<EntityStoreSchemaFilterEntry>();
            IncludedViewEntries = new List<EntityStoreSchemaFilterEntry>();
            IncludedSprocEntries = new List<EntityStoreSchemaFilterEntry>();
            ExcludedTableEntries = new List<EntityStoreSchemaFilterEntry>();
            ExcludedViewEntries = new List<EntityStoreSchemaFilterEntry>();
            ExcludedSprocEntries = new List<EntityStoreSchemaFilterEntry>();
        }

        /// <summary>
        ///     Identifies if we need to explicitly state each schema filter entry or leverage 'wildcard' filter entries
        ///     so that we don't overload the provider.
        ///     TODO: There are several interesting possibilities here to improve performance if the language of the
        ///     EntityStoreSchemaFilterEntry allows it (i.e. identifying patterns in what the user has selected and
        ///     returning back a minimal set of schema filter entries
        /// </summary>
        /// <param name="schemaFilterPolicy">The policy used for optimizing changes. We will store a </param>
        internal IList<EntityStoreSchemaFilterEntry> CollapseAndOptimize(SchemaFilterPolicy schemaFilterPolicy)
        {
            var optimizedFilterEntryList = new List<EntityStoreSchemaFilterEntry>();
            // 
            //  Add filter entries. Since catalog & schema can be null values, we pass null values for them into the allow-all/exclude-all filters.   
            //  Passing in null will mean the filter will "accept" any value, including string values.  Passing in "%" will mean it will accept any 
            //  non-null string value.
            //

            // Add filter entries for tables
            switch (schemaFilterPolicy.Tables)
            {
                case ObjectFilterPolicy.Allow:
                    {
                        // Policy is to allow new tables, so excluded tables must be written as exceptions
                        optimizedFilterEntryList.AddRange(ExcludedTableEntries);
                        optimizedFilterEntryList.Add(
                            new EntityStoreSchemaFilterEntry(
                                null, null, "%", EntityStoreSchemaFilterObjectTypes.Table, EntityStoreSchemaFilterEffect.Allow));
                        break;
                    }
                case ObjectFilterPolicy.Exclude:
                    {
                        // Policy is to exclude new tables, so included tables must be written as exceptions
                        optimizedFilterEntryList.AddRange(IncludedTableEntries);
                        optimizedFilterEntryList.Add(
                            new EntityStoreSchemaFilterEntry(
                                null, null, "%", EntityStoreSchemaFilterObjectTypes.Table, EntityStoreSchemaFilterEffect.Exclude));
                        break;
                    }
                default:
                    {
                        // If we have some unknown policy, assert and use the optimal policy...
                        Debug.Assert(
                            schemaFilterPolicy.Tables == ObjectFilterPolicy.Optimal,
                            "Unknown ObjectFilterPolicy type for tables: " + schemaFilterPolicy.Tables.ToString());

                        // Pick whatever gives us the smallest changeset
                        if (IncludedTableEntries.Count == 0)
                        {
                            optimizedFilterEntryList.Add(
                                new EntityStoreSchemaFilterEntry(
                                    null, null, "%", EntityStoreSchemaFilterObjectTypes.Table, EntityStoreSchemaFilterEffect.Exclude));
                        }
                        else if (ExcludedTableEntries.Count == 0)
                        {
                            optimizedFilterEntryList.Add(
                                new EntityStoreSchemaFilterEntry(
                                    null, null, "%", EntityStoreSchemaFilterObjectTypes.Table, EntityStoreSchemaFilterEffect.Allow));
                        }
                        else
                        {
                            optimizedFilterEntryList.AddRange(
                                ExcludedTableEntries.Count < IncludedTableEntries.Count ? ExcludedTableEntries : IncludedTableEntries);
                        }
                        break;
                    }
            }

            // add filter entries for views
            switch (schemaFilterPolicy.Views)
            {
                case ObjectFilterPolicy.Allow:
                    {
                        // Policy is to allow new views, so excluded views must be written as exceptions
                        optimizedFilterEntryList.AddRange(ExcludedViewEntries);
                        optimizedFilterEntryList.Add(
                            new EntityStoreSchemaFilterEntry(
                                null, null, "%", EntityStoreSchemaFilterObjectTypes.View, EntityStoreSchemaFilterEffect.Allow));
                        break;
                    }
                case ObjectFilterPolicy.Exclude:
                    {
                        // Policy is to exclude new views, so included views must be written as exceptions
                        optimizedFilterEntryList.AddRange(IncludedViewEntries);
                        optimizedFilterEntryList.Add(
                            new EntityStoreSchemaFilterEntry(
                                null, null, "%", EntityStoreSchemaFilterObjectTypes.View, EntityStoreSchemaFilterEffect.Exclude));
                        break;
                    }
                default:
                    {
                        // If we have some unknown policy, assert and use the optimal policy...
                        Debug.Assert(
                            schemaFilterPolicy.Views == ObjectFilterPolicy.Optimal,
                            "Unknown ObjectFilterPolicy type for views: " + schemaFilterPolicy.Views.ToString());

                        // Pick whatever gives us the smallest changeset
                        if (IncludedViewEntries.Count == 0)
                        {
                            optimizedFilterEntryList.Add(
                                new EntityStoreSchemaFilterEntry(
                                    null, null, "%", EntityStoreSchemaFilterObjectTypes.View, EntityStoreSchemaFilterEffect.Exclude));
                        }
                        else if (ExcludedViewEntries.Count == 0)
                        {
                            optimizedFilterEntryList.Add(
                                new EntityStoreSchemaFilterEntry(
                                    null, null, "%", EntityStoreSchemaFilterObjectTypes.View, EntityStoreSchemaFilterEffect.Allow));
                        }
                        else
                        {
                            optimizedFilterEntryList.AddRange(
                                ExcludedViewEntries.Count < IncludedViewEntries.Count ? ExcludedViewEntries : IncludedViewEntries);
                        }
                        break;
                    }
            }

            // add filter entries for sprocs
            switch (schemaFilterPolicy.Sprocs)
            {
                case ObjectFilterPolicy.Allow:
                    {
                        // Policy is to allow new sprocs, so excluded sprocs must be written as exceptions
                        optimizedFilterEntryList.AddRange(ExcludedSprocEntries);
                        optimizedFilterEntryList.Add(
                            new EntityStoreSchemaFilterEntry(
                                null, null, "%", EntityStoreSchemaFilterObjectTypes.Function, EntityStoreSchemaFilterEffect.Allow));
                        break;
                    }
                case ObjectFilterPolicy.Exclude:
                    {
                        // Policy is to exclude new sprocs, so included sprocs must be written as exceptions
                        optimizedFilterEntryList.AddRange(IncludedSprocEntries);
                        optimizedFilterEntryList.Add(
                            new EntityStoreSchemaFilterEntry(
                                null, null, "%", EntityStoreSchemaFilterObjectTypes.Function, EntityStoreSchemaFilterEffect.Exclude));
                        break;
                    }
                default:
                    {
                        // If we have some unknown policy, assert and use the optimal policy...
                        Debug.Assert(
                            schemaFilterPolicy.Sprocs == ObjectFilterPolicy.Optimal,
                            "Unknown ObjectFilterPolicy type for sprocs: " + schemaFilterPolicy.Sprocs.ToString());

                        // Pick whatever gives us the smallest changeset
                        if (IncludedSprocEntries.Count == 0)
                        {
                            optimizedFilterEntryList.Add(
                                new EntityStoreSchemaFilterEntry(
                                    null, null, "%", EntityStoreSchemaFilterObjectTypes.Function, EntityStoreSchemaFilterEffect.Exclude));
                        }
                        else if (ExcludedSprocEntries.Count == 0)
                        {
                            optimizedFilterEntryList.Add(
                                new EntityStoreSchemaFilterEntry(
                                    null, null, "%", EntityStoreSchemaFilterObjectTypes.Function, EntityStoreSchemaFilterEffect.Allow));
                        }
                        else
                        {
                            optimizedFilterEntryList.AddRange(
                                ExcludedSprocEntries.Count < IncludedSprocEntries.Count ? ExcludedSprocEntries : IncludedSprocEntries);
                        }
                        break;
                    }
            }

            return optimizedFilterEntryList;
        }
    }
}
