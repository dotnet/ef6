// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.ComponentModel.Design;
    using System.Diagnostics;
    using System.Drawing.Design;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;

    [TypeConverter(typeof(ExpandablePropertyConverter))]
    internal class DocumentationDescriptor
    {
        private readonly EFDocumentableItem _efElement;
        private readonly bool _isReadOnly;

        public DocumentationDescriptor(EFDocumentableItem efElement)
        {
            Debug.Assert(efElement.HasDocumentationElement, "element does not have documentation element");
            _efElement = efElement;
            _isReadOnly = IsCsdlElement(_efElement);
        }

        public override string ToString()
        {
            return String.Empty;
        }

        private bool IsReadOnly
        {
            get { return _isReadOnly; }
        }

        public bool IsReadOnlySummary()
        {
            return IsReadOnly;
        }

        public bool IsReadOnlyLongDescription()
        {
            return IsReadOnly;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_Summary")]
        [LocDescription("PropertyWindow_Description_Summary")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string Summary
        {
            get
            {
                if (_efElement.Documentation != null
                    && _efElement.Documentation.Summary != null)
                {
                    return _efElement.Documentation.Summary.Text;
                }

                return String.Empty;
            }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new SetDocumentationSummaryCommand(_efElement, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_LongDescription")]
        [LocDescription("PropertyWindow_Description_LongDescription")]
        [Editor(typeof(MultilineStringEditor), typeof(UITypeEditor))]
        public string LongDescription
        {
            get
            {
                if (_efElement.Documentation != null
                    && _efElement.Documentation.LongDescription != null)
                {
                    return _efElement.Documentation.LongDescription.Text;
                }

                return String.Empty;
            }
            set
            {
                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cmd = new SetDocumentationLongDescriptionCommand(_efElement, value);
                CommandProcessor.InvokeSingleCommand(cpc, cmd);
            }
        }

        private static bool IsCsdlElement(EFElement element)
        {
            var entityModel = element.GetParentOfType(typeof(BaseEntityModel)) as BaseEntityModel;
            if (entityModel != null
                && entityModel.IsCSDL)
            {
                return false;
            }
            return true;
        }
    }
}
