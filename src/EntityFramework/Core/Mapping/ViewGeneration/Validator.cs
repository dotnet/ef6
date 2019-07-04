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
    using System.Diagnostics;
    using System.Linq;
    using BasicSchemaConstraints =
        System.Data.Entity.Core.Mapping.ViewGeneration.Validation.SchemaConstraints<Validation.BasicKeyConstraint>;
    using ViewSchemaConstraints = System.Data.Entity.Core.Mapping.ViewGeneration.Validation.SchemaConstraints<Validation.ViewKeyConstraint>;

    // This class is responsible for validating the incoming cells for a schema
    internal class CellGroupValidator
    {
        // requires: cells are not normalized, i.e., no slot is null in the cell queries
        // effects: Constructs a validator object that is capable of
        // validating all the schema cells together
        internal CellGroupValidator(IEnumerable<Cell> cells, ConfigViewGenerator config)
        {
            m_cells = cells;
            m_config = config;
            m_errorLog = new ErrorLog();
        }

        private readonly IEnumerable<Cell> m_cells;
        private readonly ConfigViewGenerator m_config;
        private readonly ErrorLog m_errorLog; // Keeps track of errors for this set of cells
        private ViewSchemaConstraints m_cViewConstraints;
        private ViewSchemaConstraints m_sViewConstraints;

        // effects: Performs the validation of the cells in this and returns
        // an error log of all the errors/warnings that were discovered
        internal ErrorLog Validate()
        {
            // Check for errors not checked by "C-implies-S principle"
            if (m_config.IsValidationEnabled)
            {
                if (PerformSingleCellChecks() == false)
                {
                    return m_errorLog;
                }
            }
            else //Note that Metadata loading guarantees that DISTINCT flag is not present
            {
                // when update views (and validation) is disabled

                if (CheckCellsWithDistinctFlag() == false)
                {
                    return m_errorLog;
                }
            }

            var cConstraints = new BasicSchemaConstraints();
            var sConstraints = new BasicSchemaConstraints();

            // Construct intermediate "view relations" and the basic cell
            // relations along with the basic constraints
            ConstructCellRelationsWithConstraints(cConstraints, sConstraints);

            if (m_config.IsVerboseTracing)
            {
                // Trace Basic constraints
                Trace.WriteLine(String.Empty);
                Trace.WriteLine("C-Level Basic Constraints");
                Trace.WriteLine(cConstraints);
                Trace.WriteLine("S-Level Basic Constraints");
                Trace.WriteLine(sConstraints);
            }

            // Propagate the constraints
            m_cViewConstraints = PropagateConstraints(cConstraints);
            m_sViewConstraints = PropagateConstraints(sConstraints);

            // Make some basic checks on the view and basic cell constraints
            CheckConstraintSanity(cConstraints, sConstraints, m_cViewConstraints, m_sViewConstraints);

            if (m_config.IsVerboseTracing)
            {
                // Trace View constraints
                Trace.WriteLine(String.Empty);
                Trace.WriteLine("C-Level View Constraints");
                Trace.WriteLine(m_cViewConstraints);
                Trace.WriteLine("S-Level View Constraints");
                Trace.WriteLine(m_sViewConstraints);
            }

            // Check for implication
            if (m_config.IsValidationEnabled)
            {
                CheckImplication(m_cViewConstraints, m_sViewConstraints);
            }
            return m_errorLog;
        }

        // effects: Creates the base cell relation and view cell relations
        // for each cellquery/cell. Also generates the C-Side and S-side
        // basic constraints and stores them into cConstraints and
        // sConstraints. Stores them in cConstraints and sConstraints
        private void ConstructCellRelationsWithConstraints(
            BasicSchemaConstraints cConstraints,
            BasicSchemaConstraints sConstraints)
        {
            // Populate single cell constraints
            var cellNumber = 0;
            foreach (var cell in m_cells)
            {
                // We have to create the ViewCellRelation so that the
                // BasicCellRelations can be created.
                cell.CreateViewCellRelation(cellNumber);
                var cCellRelation = cell.CQuery.BasicCellRelation;
                var sCellRelation = cell.SQuery.BasicCellRelation;
                // Populate the constraints for the C relation and the S Relation
                PopulateBaseConstraints(cCellRelation, cConstraints);
                PopulateBaseConstraints(sCellRelation, sConstraints);
                cellNumber++;
            }

            // Populate two-cell constraints, i.e., inclusion
            foreach (var firstCell in m_cells)
            {
                foreach (var secondCell in m_cells)
                {
                    if (ReferenceEquals(firstCell, secondCell))
                    {
                        // We do not want to set up self-inclusion constraints unnecessarily
                        continue;
                    }
                }
            }
        }

        // effects: Generates the single-cell key+domain constraints for
        // baseRelation and adds them to constraints
        private static void PopulateBaseConstraints(
            BasicCellRelation baseRelation,
            BasicSchemaConstraints constraints)
        {
            // Populate key constraints
            baseRelation.PopulateKeyConstraints(constraints);
        }

        // effects: Propagates baseConstraints derived from the cellrelations
        // to the corresponding viewCellRelations and returns the list of
        // propagated constraints
        private static ViewSchemaConstraints PropagateConstraints(BasicSchemaConstraints baseConstraints)
        {
            var propagatedConstraints = new ViewSchemaConstraints();

            // Key constraint propagation
            foreach (var keyConstraint in baseConstraints.KeyConstraints)
            {
                var viewConstraint = keyConstraint.Propagate();
                if (viewConstraint != null)
                {
                    propagatedConstraints.Add(viewConstraint);
                }
            }
            return propagatedConstraints;
        }

        // effects: Checks if all sViewConstraints are implied by the
        // constraints in cViewConstraints. If some S-level constraints are
        // not implied, adds errors/warnings to m_errorLog
        private void CheckImplication(ViewSchemaConstraints cViewConstraints, ViewSchemaConstraints sViewConstraints)
        {
            // Check key constraints
            // i.e., if S has a key <k1, k2>, C must have a key that is a subset of this
            CheckImplicationKeyConstraints(cViewConstraints, sViewConstraints);

            // For updates, we need to ensure the following: for every
            // extent E, table T pair, some key of E is implied by T's key

            // Get all key constraints for each extent and each table
            var extentPairConstraints =
                new KeyToListMap<ExtentPair, ViewKeyConstraint>(EqualityComparer<ExtentPair>.Default);

            foreach (var cKeyConstraint in cViewConstraints.KeyConstraints)
            {
                var pair = new ExtentPair(cKeyConstraint.Cell.CQuery.Extent, cKeyConstraint.Cell.SQuery.Extent);
                extentPairConstraints.Add(pair, cKeyConstraint);
            }

            // Now check that we guarantee at least one constraint per
            // extent/table pair
            foreach (var extentPair in extentPairConstraints.Keys)
            {
                var cKeyConstraints = extentPairConstraints.ListForKey(extentPair);
                var sImpliesSomeC = false;
                // Go through all key constraints for the extent/table pair, and find one that S implies
                foreach (var cKeyConstraint in cKeyConstraints)
                {
                    foreach (var sKeyConstraint in sViewConstraints.KeyConstraints)
                    {
                        if (sKeyConstraint.Implies(cKeyConstraint))
                        {
                            sImpliesSomeC = true;
                            break; // The implication holds - so no problem
                        }
                    }
                }
                if (sImpliesSomeC == false)
                {
                    // Indicate that at least one key must be ensured on the S-side
                    m_errorLog.AddEntry(ViewKeyConstraint.GetErrorRecord(cKeyConstraints));
                }
            }
        }

        // effects: Checks for key constraint implication problems from
        // leftViewConstraints to rightViewConstraints. Adds errors/warning to m_errorLog 
        private void CheckImplicationKeyConstraints(
            ViewSchemaConstraints leftViewConstraints,
            ViewSchemaConstraints rightViewConstraints)
        {
            // if cImpliesS is true, every rightKeyConstraint must be implied
            // if it is false, at least one key constraint for each C-level
            // extent must be implied

            foreach (var rightKeyConstraint in rightViewConstraints.KeyConstraints)
            {
                // Go through all the left Side constraints and check for implication
                var found = false;
                foreach (var leftKeyConstraint in leftViewConstraints.KeyConstraints)
                {
                    if (leftKeyConstraint.Implies(rightKeyConstraint))
                    {
                        found = true;
                        break; // The implication holds - so no problem
                    }
                }
                if (false == found)
                {
                    // No C-side key constraint implies this S-level key constraint
                    // Report a problem
                    m_errorLog.AddEntry(ViewKeyConstraint.GetErrorRecord(rightKeyConstraint));
                }
            }
        }

        // <summary>
        // Checks that if a DISTINCT operator exists between some C-Extent and S-Extent, there are no additional
        // mapping fragments between that C-Extent and S-Extent.
        // We need to enforce this because DISTINCT is not understood by viewgen machinery, and two fragments may be merged
        // despite one of them having DISTINCT.
        // </summary>
        private bool CheckCellsWithDistinctFlag()
        {
            var errorLogSize = m_errorLog.Count;
            foreach (var cell in m_cells)
            {
                if (cell.SQuery.SelectDistinctFlag
                    == CellQuery.SelectDistinct.Yes)
                {
                    var cExtent = cell.CQuery.Extent;
                    var sExtent = cell.SQuery.Extent;

                    //There should be no other fragments mapping cExtent to sExtent
                    var mappedFragments = m_cells.Where(otherCell => otherCell != cell)
                                                 .Where(
                                                     otherCell => otherCell.CQuery.Extent == cExtent && otherCell.SQuery.Extent == sExtent);

                    if (mappedFragments.Any())
                    {
                        var cellsToReport = Enumerable.Repeat(cell, 1).Union(mappedFragments);
                        var record = new ErrorLog.Record(
                            ViewGenErrorCode.MultipleFragmentsBetweenCandSExtentWithDistinct,
                            Strings.Viewgen_MultipleFragmentsBetweenCandSExtentWithDistinct(cExtent.Name, sExtent.Name), cellsToReport,
                            String.Empty);
                        m_errorLog.AddEntry(record);
                    }
                }
            }

            return m_errorLog.Count == errorLogSize;
        }

        // effects: Check for problems in each cell that are not detected by the
        // "C-constraints-imply-S-constraints" principle. If the check fails,
        // adds relevant error info to m_errorLog and returns false. Else
        // retrns true
        private bool PerformSingleCellChecks()
        {
            var errorLogSize = m_errorLog.Count;
            foreach (var cell in m_cells)
            {
                // Check for duplication of element in a single cell name1, name2
                // -> name Could be done by implication but that would require
                // setting self-inclusion constraints etc That seems unnecessary

                // We need this check only for the C side. if we map cname1
                // and cmane2 to sname, that is a problem. But mapping sname1
                // and sname2 to cname is ok
                var error = cell.SQuery.CheckForDuplicateFields(cell.CQuery, cell);
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }

                // Check that the EntityKey and the Table key are mapped
                // (Key for association is all ends)
                error = cell.CQuery.VerifyKeysPresent(
                    cell, Strings.ViewGen_EntitySetKey_Missing,
                    Strings.ViewGen_AssociationSetKey_Missing, ViewGenErrorCode.KeyNotMappedForCSideExtent);

                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }

                error = cell.SQuery.VerifyKeysPresent(cell, Strings.ViewGen_TableKey_Missing, null, ViewGenErrorCode.KeyNotMappedForTable);
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }

                // Check that if any side has a not-null constraint -- if so,
                // we must project that slot
                error = cell.CQuery.CheckForProjectedNotNullSlots(cell, m_cells.Where(c => c.SQuery.Extent is AssociationSet));
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }
                error = cell.SQuery.CheckForProjectedNotNullSlots(cell, m_cells.Where(c => c.CQuery.Extent is AssociationSet));
                if (error != null)
                {
                    m_errorLog.AddEntry(error);
                }
            }
            return m_errorLog.Count == errorLogSize;
        }

        // effects: Checks for some sanity issues between the basic and view constraints. Adds to m_errorLog if needed
        [Conditional("DEBUG")]
        private static void CheckConstraintSanity(
            BasicSchemaConstraints cConstraints, BasicSchemaConstraints sConstraints,
            ViewSchemaConstraints cViewConstraints, ViewSchemaConstraints sViewConstraints)
        {
            Debug.Assert(
                cConstraints.KeyConstraints.Count() == cViewConstraints.KeyConstraints.Count(),
                "Mismatch in number of C basic and view key constraints");
            Debug.Assert(
                sConstraints.KeyConstraints.Count() == sViewConstraints.KeyConstraints.Count(),
                "Mismatch in number of S basic and view key constraints");
        }

        // Keeps track of two extent objects
        private class ExtentPair
        {
            internal ExtentPair(EntitySetBase acExtent, EntitySetBase asExtent)
            {
                cExtent = acExtent;
                sExtent = asExtent;
            }

            internal readonly EntitySetBase cExtent;
            internal readonly EntitySetBase sExtent;

            public override bool Equals(object obj)
            {
                if (ReferenceEquals(this, obj))
                {
                    return true;
                }
                var pair = obj as ExtentPair;
                if (pair == null)
                {
                    return false;
                }

                return pair.cExtent.Equals(cExtent) && pair.sExtent.Equals(sExtent);
            }

            public override int GetHashCode()
            {
                return cExtent.GetHashCode() ^ sExtent.GetHashCode();
            }
        }
    }
}
