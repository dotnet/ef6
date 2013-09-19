// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Mapping.ViewGeneration
{
    using System.Collections.Generic;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Structures;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Utils;
    using System.Data.Entity.Core.Mapping.ViewGeneration.Validation;
    using System.Data.Entity.Core.Metadata.Edm;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Utilities;
    using System.Diagnostics;
    using System.Linq;
    using System.Text;
    using CellGroup = System.Data.Entity.Core.Common.Utils.Set<Structures.Cell>;

    internal abstract class ViewgenGatekeeper : InternalBase
    {
        /// <summary>
        /// Entry point for View Generation
        /// </summary>
        /// <returns> Generated Views for EntitySets </returns>
        internal static ViewGenResults GenerateViewsFromMapping(StorageEntityContainerMapping containerMapping, ConfigViewGenerator config)
        {
            DebugCheck.NotNull(containerMapping);
            DebugCheck.NotNull(config);
            Debug.Assert(containerMapping.HasViews, "Precondition Violated: No mapping exists to generate views for!");

            //Create Cells from StorageEntityContainerMapping
            var cellCreator = new CellCreator(containerMapping);
            var cells = cellCreator.GenerateCells();
            var identifiers = cellCreator.Identifiers;

            return GenerateViewsFromCells(cells, config, identifiers, containerMapping);
        }

        /// <summary>
        /// Entry point for Type specific generation of Query Views
        /// </summary>
        internal static ViewGenResults GenerateTypeSpecificQueryView(
            StorageEntityContainerMapping containerMapping,
            ConfigViewGenerator config,
            EntitySetBase entity,
            EntityTypeBase type,
            bool includeSubtypes,
            out bool success)
        {
            DebugCheck.NotNull(containerMapping);
            DebugCheck.NotNull(config);
            DebugCheck.NotNull(entity);
            DebugCheck.NotNull(type);
            Debug.Assert(!type.Abstract, "Can not generate OfType/OfTypeOnly query view for and abstract type");

            if (config.IsNormalTracing)
            {
                Helpers.StringTraceLine("");
                Helpers.StringTraceLine(
                    "<<<<<<<< Generating Query View for Entity [" + entity.Name + "] OfType" + (includeSubtypes ? "" : "Only") + "("
                    + type.Name + ") >>>>>>>");
            }

            if (containerMapping.GetEntitySetMapping(entity.Name).QueryView != null)
            {
                //Type-specific QV does not exist in the cache, but 
                // there is a EntitySet QV. So we can't generate the view (no mapping exists for this EntitySet)
                // and we rely on Query to call us again to get the EntitySet View.
                success = false;
                return null;
            }

            //Compute Cell Groups or get it from Memoizer
            var args = new InputForComputingCellGroups(containerMapping, config);
            var result = containerMapping.GetCellgroups(args);
            success = result.Success;

            if (!success)
            {
                return null;
            }

            var foreignKeyConstraints = result.ForeignKeyConstraints;
            // Get a Clone of cell groups from cache since cells are modified during viewgen, and we dont want the cached copy to change
            var cellGroups = result.CellGroups.Select(setOfcells => new CellGroup(setOfcells.Select(cell => new Cell(cell)))).ToList();
            var cells = result.Cells;
            var identifiers = result.Identifiers;

            var viewGenResults = new ViewGenResults();
            var tmpLog = EnsureAllCSpaceContainerSetsAreMapped(cells, containerMapping);
            if (tmpLog.Count > 0)
            {
                viewGenResults.AddErrors(tmpLog);
                Helpers.StringTraceLine(viewGenResults.ErrorsToString());
                success = true; //atleast we tried successfully
                return viewGenResults;
            }

            foreach (var cellGroup in cellGroups)
            {
                if (!DoesCellGroupContainEntitySet(cellGroup, entity))
                {
                    continue;
                }

                ViewGenerator viewGenerator = null;
                var groupErrorLog = new ErrorLog();
                try
                {
                    viewGenerator = new ViewGenerator(cellGroup, config, foreignKeyConstraints, containerMapping);
                }
                catch (InternalMappingException exception)
                {
                    // All exceptions have mapping errors in them
                    Debug.Assert(exception.ErrorLog.Count > 0, "Incorrectly created mapping exception");
                    groupErrorLog = exception.ErrorLog;
                }

                if (groupErrorLog.Count > 0)
                {
                    break;
                }
                Debug.Assert(viewGenerator != null); //make sure there is no exception thrown that does not add error to log

                var mode = includeSubtypes ? ViewGenMode.OfTypeViews : ViewGenMode.OfTypeOnlyViews;

                groupErrorLog = viewGenerator.GenerateQueryViewForSingleExtent(viewGenResults.Views, identifiers, entity, type, mode);

                if (groupErrorLog.Count != 0)
                {
                    viewGenResults.AddErrors(groupErrorLog);
                }
            }

            success = true;
            return viewGenResults;
        }

        // effects: Given a list of cells in the schema, generates the query and
        // update mapping views for OFTYPE(Extent, Type) combinations in this schema
        // container. Returns a list of generated query and update views.
        // If it is false and some columns in a table are unmapped, an
        // exception is raised
        private static ViewGenResults GenerateViewsFromCells(
            List<Cell> cells, ConfigViewGenerator config,
            CqlIdentifiers identifiers,
            StorageEntityContainerMapping containerMapping)
        {
            DebugCheck.NotNull(cells);
            DebugCheck.NotNull(config);
            Debug.Assert(cells.Count > 0, "There must be at least one cell in the container mapping");

            // Go through each table and determine their foreign key constraints
            var container = containerMapping.StorageEntityContainer;
            Debug.Assert(container != null);

            var viewGenResults = new ViewGenResults();
            var tmpLog = EnsureAllCSpaceContainerSetsAreMapped(cells, containerMapping);
            if (tmpLog.Count > 0)
            {
                viewGenResults.AddErrors(tmpLog);
                Helpers.StringTraceLine(viewGenResults.ErrorsToString());
                return viewGenResults;
            }

            var foreignKeyConstraints = ForeignConstraint.GetForeignConstraints(container);

            var partitioner = new CellPartitioner(cells, foreignKeyConstraints);
            var cellGroups = partitioner.GroupRelatedCells();
            foreach (var cellGroup in cellGroups)
            {
                ViewGenerator viewGenerator = null;
                var groupErrorLog = new ErrorLog();
                try
                {
                    viewGenerator = new ViewGenerator(cellGroup, config, foreignKeyConstraints, containerMapping);
                }
                catch (InternalMappingException exception)
                {
                    // All exceptions have mapping errors in them
                    Debug.Assert(exception.ErrorLog.Count > 0, "Incorrectly created mapping exception");
                    groupErrorLog = exception.ErrorLog;
                }

                if (groupErrorLog.Count == 0)
                {
                    Debug.Assert(viewGenerator != null);
                    groupErrorLog = viewGenerator.GenerateAllBidirectionalViews(viewGenResults.Views, identifiers);
                }

                if (groupErrorLog.Count != 0)
                {
                    viewGenResults.AddErrors(groupErrorLog);
                }
            }
            // We used to print the errors here. Now we trace them as they are being thrown
            //if (viewGenResults.HasErrors && config.IsViewTracing) {
            //    Helpers.StringTraceLine(viewGenResults.ErrorsToString());
            //}
            return viewGenResults;
        }

        // effects: Given a container, ensures that all entity/association
        // sets in container on the C-side have been mapped
        private static ErrorLog EnsureAllCSpaceContainerSetsAreMapped(
            IEnumerable<Cell> cells,
            StorageEntityContainerMapping containerMapping)
        {
            var mappedExtents = new Set<EntitySetBase>();
            string mslFileLocation = null;
            EntityContainer container = null;
            // Determine the container and name of the file while determining
            // the set of mapped extents in the cells
            foreach (var cell in cells)
            {
                mappedExtents.Add(cell.CQuery.Extent);
                mslFileLocation = cell.CellLabel.SourceLocation;
                // All cells are from the same container
                container = cell.CQuery.Extent.EntityContainer;
            }
            Debug.Assert(container != null);

            var missingExtents = new List<EntitySetBase>();
            // Go through all the extents in the container and determine
            // extents that are missing
            foreach (var extent in container.BaseEntitySets)
            {
                if (mappedExtents.Contains(extent) == false
                    && !(containerMapping.HasQueryViewForSetMap(extent.Name)))
                {
                    var associationSet = extent as AssociationSet;
                    if (associationSet == null
                        || !associationSet.ElementType.IsForeignKey)
                    {
                        missingExtents.Add(extent);
                    }
                }
            }
            var errorLog = new ErrorLog();
            // If any extent is not mapped, add an error
            if (missingExtents.Count > 0)
            {
                var extentBuilder = new StringBuilder();
                var isFirst = true;
                foreach (var extent in missingExtents)
                {
                    if (isFirst == false)
                    {
                        extentBuilder.Append(", ");
                    }
                    isFirst = false;
                    extentBuilder.Append(extent.Name);
                }
                var message = Strings.ViewGen_Missing_Set_Mapping(extentBuilder);
                // Find the cell with smallest line number - so that we can
                // point to the beginning of the file
                var lowestLineNum = -1;
                Cell smallestCell = null;
                foreach (var cell in cells)
                {
                    if (lowestLineNum == -1
                        || cell.CellLabel.StartLineNumber < lowestLineNum)
                    {
                        smallestCell = cell;
                        lowestLineNum = cell.CellLabel.StartLineNumber;
                    }
                }
                Debug.Assert(smallestCell != null && lowestLineNum >= 0);
                var edmSchemaError = new EdmSchemaError(
                    message, (int)ViewGenErrorCode.MissingExtentMapping,
                    EdmSchemaErrorSeverity.Error, containerMapping.SourceLocation, containerMapping.StartLineNumber,
                    containerMapping.StartLinePosition, null);
                var record = new ErrorLog.Record(edmSchemaError);
                errorLog.AddEntry(record);
            }
            return errorLog;
        }

        private static bool DoesCellGroupContainEntitySet(CellGroup group, EntitySetBase entity)
        {
            foreach (var cell in group)
            {
                if (cell.GetLeftQuery(ViewTarget.QueryView).Extent.Equals(entity))
                {
                    return true;
                }
            }

            return false;
        }

        internal override void ToCompactString(StringBuilder builder)
        {
        }
    }
}
