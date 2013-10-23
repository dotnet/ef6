// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Collections.Generic;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    /// <summary>
    ///     Use this command to change whether an entity is Abstract or not.  If we are marking an entity
    ///     as abstract, then we need to remove any function mappings.
    ///     Example:
    ///     &lt;EntityType Name=&quot;CAbstractCategory&quot; Abstract=&quot;true&quot;&gt;
    ///     &lt;Key&gt;
    ///     &lt;PropertyRef Name=&quot;CategoryId&quot; /&gt;
    ///     &lt;/Key&gt;
    ///     &lt;Property Name=&quot;CategoryId&quot; Type=&quot;String&quot; MaxLength=&quot;1024&quot; Nullable=&quot;false&quot; /&gt;
    ///     &lt;Property Name=&quot;CategoryName&quot; Type=&quot;String&quot; MaxLength=&quot;1024&quot; Nullable=&quot;false&quot; /&gt;
    ///     &lt;/EntityType&gt;
    /// </summary>
    internal class ChangeEntityTypeAbstractCommand : Command
    {
        internal ConceptualEntityType EntityType { get; set; }
        internal bool SetAbstract { get; set; }

        /// <summary>
        ///     This method lets you change whether an Entity is abstract or not.
        /// </summary>
        /// <param name="entityType">Must point to a valid C-Side entity</param>
        internal ChangeEntityTypeAbstractCommand(ConceptualEntityType entityType, bool setAbstract)
        {
            CommandValidation.ValidateConceptualEntityType(entityType);

            EntityType = entityType;
            SetAbstract = setAbstract;
        }

        internal ChangeEntityTypeAbstractCommand(Func<Command, CommandProcessorContext, bool> bindingAction)
            : base(bindingAction)
        {
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            Debug.Assert(cpc != null, "InvokeInternal is called when EntityContainerMapping is null.");

            // safety check, this should never be hit
            if (EntityType == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when entity type is null");
            }

            EntityType.Abstract.Value = SetAbstract;

            // remove any function mappings if we are setting this to abstract
            if (SetAbstract)
            {
                var etms = new List<EntityTypeMapping>();
                etms.AddRange(EntityType.GetAntiDependenciesOfType<EntityTypeMapping>());

                for (var i = etms.Count - 1; i >= 0; i--)
                {
                    var etm = etms[i];
                    if (etm != null
                        && etm.Kind == EntityTypeMappingKind.Function)
                    {
                        DeleteEFElementCommand.DeleteInTransaction(cpc, etm);
                    }
                }
            }

            XmlModelHelper.NormalizeAndResolve(EntityType);
        }

        protected override void PostInvoke(CommandProcessorContext cpc)
        {
            // changing abstractness will impact the MSL generated
            if (EntityType != null)
            {
                EnforceEntitySetMappingRules.AddRule(cpc, EntityType.EntitySet);
            }
        }
    }
}
