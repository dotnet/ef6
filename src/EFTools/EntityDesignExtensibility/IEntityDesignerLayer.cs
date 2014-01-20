// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Extensibility
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics.CodeAnalysis;
    using System.Xml.Linq;

    /// <summary>
    ///     This class allows the notion of a 'layer' in the Entity Designer.
    ///     Layers can be turned off and on; they are composed of:
    ///     (1) Simple Metadata about the feature
    ///     (2) Commands that can be executed against the feature
    ///     (3) Core property extensions
    ///     (4) Simple event sinks for operations that occur in the designer
    ///     (5) Basic selection mechanism drivers
    /// </summary>
    public interface IEntityDesignerLayer
    {
        /// <summary>
        ///     The name of the layer
        /// </summary>
        string Name { get; }

        /// <summary>
        ///     Determines where third-party property extensions can subscribe to this layer
        /// </summary>
        bool IsSealed { get; }

        /// <summary>
        ///     A layer can provide its own service provider for selection purposes. Currently
        ///     the limitation is that a layer can only proffer one sited service provider.
        /// </summary>
        IServiceProvider ServiceProvider { get; }

        /// <summary>
        ///     Core property extensions that are automatically subscribed to this feature.
        /// </summary>
        IList<IEntityDesignerExtendedProperty> Properties { get; }

        /// <summary>
        ///     Gets fired when a transaction is committed. A layer extension can take basic actions in this case
        ///     such as reloading an owning tool window.
        /// </summary>
        /// <param name="xmlChanges">A list of changes made during the transaction.</param>
        [SuppressMessage("Microsoft.Design", "CA1006:DoNotNestGenericTypesInMemberSignatures",
            Justification = "Backwards compatibility, it is already part of public API")]
        void OnAfterTransactionCommitted(IEnumerable<Tuple<XObject, XObjectChange>> xmlChanges);

        /// <summary>
        ///     Fired after the layer is loaded.
        /// </summary>
        /// <param name="xObject">the selected object in the active designer or conceptual model if nothing is selected.</param>
        [SuppressMessage("Microsoft.Naming", "CA1704:IdentifiersShouldBeSpelledCorrectly", MessageId = "x")]
        void OnAfterLayerLoaded(XObject xObject);

        /// <summary>
        ///     Fired before the layer is unloaded.
        /// </summary>
        /// <param name="conceptualModelXObject">The conceptual model.</param>
        void OnBeforeLayerUnloaded(XObject conceptualModelXObject);

        /// <summary>
        ///     Fired when selection is changed on the designer surface
        /// </summary>
        /// <param name="selection">The selected object in the active designer or conceptual model.</param>
        void OnSelectionChanged(XObject selection);

        /// <summary>
        ///     Change the selection on the entity designer. The selection identifier here corresponds to
        ///     either 'EntityName', 'AssociationName', or 'EntityName.PropertyName'.
        /// </summary>
        event EventHandler<ChangeEntityDesignerSelectionEventArgs> ChangeEntityDesignerSelection;
    }
}
