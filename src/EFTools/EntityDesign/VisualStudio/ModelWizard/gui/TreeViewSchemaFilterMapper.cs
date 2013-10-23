// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Gui
{
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.VersioningFacade.ReverseEngineerDb;
    using Microsoft.Data.Entity.Design.VisualStudio.ModelWizard.Engine;

    /// <summary>
    ///     Given a set of TreeViews, this will map EntityStoreSchemaFilterEntries to tree nodes and vice versa.
    /// </summary>
    internal class TreeViewSchemaFilterMapper
    {
        private readonly IList<TreeView> _treeViews;
        private readonly IDictionary<TreeView, TreeViewSchemaFilterMapperSettings> _treeView2Settings;

        internal TreeViewSchemaFilterMapper()
        {
            _treeViews = new List<TreeView>();
            _treeView2Settings = new Dictionary<TreeView, TreeViewSchemaFilterMapperSettings>();
        }

        /// <summary>
        ///     Instantiate the mapper with a TreeView: pass in null for settings to use the default settings
        /// </summary>
        internal TreeViewSchemaFilterMapper(TreeView treeView, TreeViewSchemaFilterMapperSettings settings = null)
            : this()
        {
            AddTreeView(treeView, settings);
        }

        /// <summary>
        ///     Add a TreeView with settings; pass in null for settings to use the default settings
        /// </summary>
        internal void AddTreeView(TreeView treeView, TreeViewSchemaFilterMapperSettings settings)
        {
            _treeViews.Add(treeView);
            if (settings == null)
            {
                settings = TreeViewSchemaFilterMapperSettings.GetDefaultSettings();
            }
            _treeView2Settings.Add(treeView, settings);
        }

        internal SchemaFilterEntryBag CreateSchemaFilterEntryBag()
        {
            var schemaFilterEntryBag = new SchemaFilterEntryBag();
            foreach (var treeView in _treeViews)
            {
                TreeViewSchemaFilterMapperSettings settings;
                _treeView2Settings.TryGetValue(treeView, out settings);

                foreach (TreeNode parentNode in treeView.Nodes)
                {
                    foreach (TreeNode schemaNode in parentNode.Nodes)
                    {
                        foreach (TreeNode child in schemaNode.Nodes)
                        {
                            UpdateSchemaFilterEntryBagFromLeafNode(child, schemaFilterEntryBag, settings);
                        }
                    }
                }
            }
            return schemaFilterEntryBag;
        }

        internal static void ApplyFilterEntriesToTreeView(TreeView treeView, IEnumerable<EntityStoreSchemaFilterEntry> filterEntriesToApply)
        {
            foreach (TreeNode parentNode in treeView.Nodes)
            {
                foreach (TreeNode schemaNode in parentNode.Nodes)
                {
                    foreach (TreeNode child in schemaNode.Nodes)
                    {
                        // Check to see if the filter entries allow this.
                        // TODO this is not very performant, but we assume a small number of filters based on our
                        // existing optimization logic for selecting the filters in the first place
                        var entryToTest = child.Tag as EntityStoreSchemaFilterEntry;
                        Debug.Assert(entryToTest != null, "entryToTest should not be null");
                        if (entryToTest != null)
                        {
                            var effect = entryToTest.GetEffectViaFilter(filterEntriesToApply);

                            // Check the resulting effect
                            if (effect == EntityStoreSchemaFilterEffect.Allow)
                            {
                                child.Checked = true;
                            }
                        }
                    }
                }
            }
        }

        private static void UpdateSchemaFilterEntryBagFromLeafNode(
            TreeNode leafNode, SchemaFilterEntryBag schemaFilterEntryBag, TreeViewSchemaFilterMapperSettings settings)
        {
            Debug.Assert(
                settings != null,
                "We should not be passing null settings into this method; they should at least be the default settings");
            if (settings == null)
            {
                return;
            }

            var e = leafNode.Tag as EntityStoreSchemaFilterEntry;
            Debug.Assert(
                e != null,
                "Either the Tag property of the leaf node is null or the leaf node is not an EntityStoreSchemaFilterEntry");
            if (e != null)
            {
                switch (e.Types)
                {
                    case EntityStoreSchemaFilterObjectTypes.Table:
                        if (false == settings.UseOnlyCheckedNodes
                            || leafNode.Checked)
                        {
                            schemaFilterEntryBag.IncludedTableEntries.Add(e);
                        }
                        else
                        {
                            schemaFilterEntryBag.ExcludedTableEntries.Add(
                                new EntityStoreSchemaFilterEntry(
                                    e.Catalog, e.Schema, e.Name, e.Types, EntityStoreSchemaFilterEffect.Exclude));
                        }
                        break;
                    case EntityStoreSchemaFilterObjectTypes.View:
                        if (false == settings.UseOnlyCheckedNodes
                            || leafNode.Checked)
                        {
                            schemaFilterEntryBag.IncludedViewEntries.Add(e);
                        }
                        else
                        {
                            schemaFilterEntryBag.ExcludedViewEntries.Add(
                                new EntityStoreSchemaFilterEntry(
                                    e.Catalog, e.Schema, e.Name, e.Types, EntityStoreSchemaFilterEffect.Exclude));
                        }
                        break;
                    case EntityStoreSchemaFilterObjectTypes.Function:
                        if (false == settings.UseOnlyCheckedNodes
                            || leafNode.Checked)
                        {
                            schemaFilterEntryBag.IncludedSprocEntries.Add(e);
                        }
                        else
                        {
                            schemaFilterEntryBag.ExcludedSprocEntries.Add(
                                new EntityStoreSchemaFilterEntry(
                                    e.Catalog, e.Schema, e.Name, e.Types, EntityStoreSchemaFilterEffect.Exclude));
                        }
                        break;
                    default:
                        Debug.Fail("Unexpected filter object type");
                        break;
                }
            }
        }
    }
}
