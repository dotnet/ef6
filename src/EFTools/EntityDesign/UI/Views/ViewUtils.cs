// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.Views
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal static class ViewUtils
    {
        internal static bool SetBaseEntityType(
            CommandProcessorContext cpc, ConceptualEntityType derivedEntity, ConceptualEntityType baseEntity)
        {
            if (ModelHelper.CheckForCircularInheritance(derivedEntity, baseEntity))
            {
                var message = String.Format(
                    CultureInfo.CurrentCulture, Resources.Error_CircularInheritanceAborted, derivedEntity.LocalName.Value,
                    baseEntity.LocalName.Value);

                VsUtils.ShowErrorDialog(message);

                return false;
            }

            var cp = new CommandProcessor(cpc);

            if (derivedEntity.BaseType.Target != null)
            {
                // CreateInheritanceCommand works only for entities that don't have base type set
                // so we need to remove base type first in this case
                cp.EnqueueCommand(new DeleteInheritanceCommand(derivedEntity));
            }

            if (baseEntity != null)
            {
                // in case the user has chosen "(None)" then we just want to delete the existing one
                cp.EnqueueCommand(new CreateInheritanceCommand(derivedEntity, baseEntity));
            }

            // a quick check to be sure
            Debug.Assert(cp.CommandCount > 0, "Why didn't we enqueue at least one command?");
            if (cp.CommandCount > 0)
            {
                cp.Invoke();
            }

            return true;
        }

        // Fix for Dev10 Bug 592077: Display Horizontal Scroll bar if the name exceeds the container.
        internal static void DisplayHScrollOnListBoxIfNecessary(ListBox listBox)
        {
            // Display a horizontal scroll bar if necessary.
            listBox.HorizontalScrollbar = true;

            // Create a Graphics object to use when determining the size of the largest item in the ListBox.
            var g = listBox.CreateGraphics();

            // Determine the size for HorizontalExtent using the MeasureString method.
            var maxHorizontalSize = -1;
            for (var i = 0; i < listBox.Items.Count; i++)
            {
                var hzSize = (int)g.MeasureString(listBox.Items[i].ToString(), listBox.Font).Width;
                if (hzSize > maxHorizontalSize)
                {
                    maxHorizontalSize = hzSize;
                }
            }
            // Set the HorizontalExtent property.
            if (maxHorizontalSize != -1)
            {
                listBox.HorizontalExtent = maxHorizontalSize;
            }
        }
    }
}
