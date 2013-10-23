// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model.Commands
{
    using System.Diagnostics;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Integrity;
    using Microsoft.Data.Entity.Design.Model.Mapping;

    internal class ChangeScalarPropertyCommand : Command
    {
        private readonly Property _entityProperty;
        internal ScalarProperty ScalarProperty { get; private set; }
        internal Property PreviousTableColumn { get; private set; }
        internal Property TableColumn { get; private set; }

        /// <summary>
        ///     Changes one or both of the ends of an existing ScalarProperty.  You can send
        ///     null to one of the ends, but both cannot be null.
        /// </summary>
        /// <param name="sp">A valid Scalar Property; this cannot be null.</param>
        /// <param name="entityProperty">The C-Side property to change to.</param>
        /// <param name="tableColumn">The S-Side property to change to.</param>
        internal ChangeScalarPropertyCommand(ScalarProperty sp, Property entityProperty, Property tableColumn)
        {
            CommandValidation.ValidateScalarProperty(sp);

            Debug.Assert(entityProperty != null || tableColumn != null, "At least one of entityProperty or tableColumn should be non-null");
            if (entityProperty != null)
            {
                CommandValidation.ValidateConceptualEntityProperty(entityProperty);
            }
            if (tableColumn != null)
            {
                CommandValidation.ValidateTableColumn(tableColumn);
            }

            ScalarProperty = sp;
            _entityProperty = entityProperty;
            TableColumn = tableColumn;
        }

        protected override void PreInvoke(CommandProcessorContext cpc)
        {
            base.PreInvoke(cpc);
            PreviousTableColumn = ScalarProperty.ColumnName.Target;
        }

        protected override void InvokeInternal(CommandProcessorContext cpc)
        {
            if (_entityProperty != null)
            {
                Debug.Assert(_entityProperty.EntityModel.IsCSDL, "_entityProperty should be from C-side model");
                ScalarProperty.Name.SetRefName(_entityProperty);
            }

            if (TableColumn != null)
            {
                Debug.Assert(TableColumn.EntityModel.IsCSDL != true, "_tableColumn should not be from C-side model");
                ScalarProperty.ColumnName.SetRefName(TableColumn);
            }

            XmlModelHelper.NormalizeAndResolve(ScalarProperty);

            // if we change a scalar in an association mapping, make sure that we still have good MSL
            if (ScalarProperty.EndProperty != null)
            {
                var asm = ScalarProperty.EndProperty.Parent as AssociationSetMapping;
                Debug.Assert(asm != null, "_sp.EndProperty parent is not an AssociationSetMapping");
                if (asm != null)
                {
                    EnforceAssociationSetMappingRules.AddRule(cpc, asm);

                    var assoc = asm.TypeName.Target;
                    Debug.Assert(assoc != null, "Could not resolve association reference");
                    if (assoc != null)
                    {
                        InferReferentialConstraints.AddRule(cpc, assoc);
                    }
                }
            }
        }
    }
}
