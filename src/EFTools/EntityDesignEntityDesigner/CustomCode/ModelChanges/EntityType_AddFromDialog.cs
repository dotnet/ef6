// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

using Model = Microsoft.Data.Entity.Design.Model.Entity;

namespace Microsoft.Data.Entity.Design.EntityDesigner.ModelChanges
{
    using System.Diagnostics.CodeAnalysis;
    using Microsoft.Data.Entity.Design.EntityDesigner.Rules;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.Views.Dialogs;

    internal class EntityType_AddFromDialog : ViewModelChange
    {
        private readonly NewEntityDialog _dialog;

        internal EntityType_AddFromDialog(NewEntityDialog dialog)
        {
            _dialog = dialog;
        }

        [SuppressMessage("Microsoft.Reliability", "CA2000:Dispose objects before losing scope")]
        [SuppressMessage("Microsoft.Performance", "CA1804:RemoveUnusedLocals", MessageId = "modelEntity")]
        internal override void Invoke(CommandProcessorContext cpc)
        {
            EntityType modelEntity = null;
            if (_dialog.BaseEntityType != null)
            {
                modelEntity = CreateEntityTypeCommand.CreateDerivedEntityType(cpc, _dialog.EntityName, _dialog.BaseEntityType, false);
            }
            else
            {
                modelEntity = CreateEntityTypeCommand.CreateConceptualEntityTypeAndEntitySetAndProperty(
                    cpc,
                    _dialog.EntityName,
                    _dialog.EntitySetName,
                    _dialog.CreateKeyProperty,
                    _dialog.KeyPropertyName,
                    _dialog.KeyPropertyType,
                    ModelHelper.CanTypeSupportIdentity(_dialog.KeyPropertyType)
                        ? ModelConstants.StoreGeneratedPattern_Identity
                        : ModelConstants.StoreGeneratedPattern_None,
                    false);
            }
        }

        internal override int InvokeOrderPriority
        {
            get { return 100; }
        }
    }
}
