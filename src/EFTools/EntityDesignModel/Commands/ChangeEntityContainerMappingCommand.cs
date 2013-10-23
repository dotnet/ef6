// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class ChangeEntityContainerMappingCommand : Command
    {
        private readonly EntityContainerMapping _ecm;
        private readonly bool _generateUpdateViews;

        /// <summary>
        ///     This method lets you change whether an EntityContainerMapping should generate update views or not (no means it is read-only).
        /// </summary>
        /// <param name="ecm">Must point to a valid EntityContainerMapping</param>
        internal ChangeEntityContainerMappingCommand(EntityContainerMapping ecm, bool generateUpdateViews)
        {
            CommandValidation.ValidateEntityContainerMapping(ecm);

            _ecm = ecm;
            _generateUpdateViews = generateUpdateViews;
        }

        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "EntityContainerMapping")]
        [SuppressMessage("Microsoft.Naming", "CA2204:Literals should be spelled correctly", MessageId = "InvokeInternal")]
        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            // safety check, this should never be hit
            Debug.Assert(cpc != null, "InvokeInternal is called when EntityContainerMapping is null.");
            if (_ecm == null)
            {
                throw new InvalidOperationException("InvokeInternal is called when EntityContainerMapping is null.");
            }

            _ecm.GenerateUpdateViews.Value = _generateUpdateViews;
            XmlModelHelper.NormalizeAndResolve(_ecm);
        }
    }
}
