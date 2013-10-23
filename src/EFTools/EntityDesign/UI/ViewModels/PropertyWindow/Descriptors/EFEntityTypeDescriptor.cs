// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Descriptors
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Diagnostics.CodeAnalysis;
    using System.Windows.Forms;
    using Microsoft.Data.Entity.Design.Core.Controls;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Commands;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Mapping;
    using Microsoft.Data.Entity.Design.UI.ViewModels.PropertyWindow.Converters;
    using Microsoft.Data.Entity.Design.UI.Views;
    using Microsoft.Data.Entity.Design.VisualStudio;
    using Microsoft.Data.Entity.Design.VisualStudio.Package;
    using Microsoft.VisualStudio.Shell.Interop;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EFEntityTypeDescriptor :
        EFAnnotatableElementDescriptor<EntityType>,
        IAnnotatableDocumentableDescriptor
    {
        [LocDescription("PropertyWindow_Description_EntityName")]
        [MergableProperty(false)]
        public override string Name
        {
            get { return base.Name; }
            set { base.Name = value; }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_BaseType")]
        [LocDescription("PropertyWindow_Description_EntityBaseType")]
        [TypeConverter(typeof(BaseTypeListConverter))]
        [MergableProperty(false)]
        public ConceptualEntityType BaseType
        {
            get
            {
                var cet = TypedEFElement as ConceptualEntityType;
                Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
                var baseType = cet.BaseType.Target;
                if (baseType == null)
                {
                    // Check if it has an unresolved BaseType
                    if (!String.IsNullOrEmpty(cet.BaseType.RefName))
                    {
                        // Return current type as BaseType, so TypeConverter can show unresolved type name in PropertyWindow
                        baseType = cet;
                    }
                }
                return baseType;
            }
            set
            {
                Debug.Assert(value != TypedEFElement, "You can never set current Type as the baseType");
                // if value is the same as current value then no need to set anything
                // (note: this also covers setting from 'None' to 'None' as BaseType will return null)
                if (value == BaseType)
                {
                    return;
                }

                var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                var cet = TypedEFElement as ConceptualEntityType;
                Debug.Assert(cet != null, "EntityType is not ConceptualEntityType");
                ViewUtils.SetBaseEntityType(cpc, cet, value);
            }
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_EntitySetName")]
        [LocDescription("PropertyWindow_Description_EntitySetName")]
        [MergableProperty(false)]
        public string EntitySetName
        {
            get
            {
                var es = TypedEFElement.EntitySet;
                if (es != null)
                {
                    return es.LocalName.Value;
                }
                return null;
            }
            set
            {
                var es = TypedEFElement.EntitySet;
                Debug.Assert(es != null, "EntitySet should not be null");
                if (es != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    Command c = new EntityDesignRenameCommand(es, value, true);
                    var cp = new CommandProcessor(cpc, c);
                    cp.Invoke();
                }
            }
        }

        public override string GetComponentName()
        {
            return TypedEFElement.NormalizedNameExternal;
        }

        public override string GetClassName()
        {
            return "EntityType";
        }

        public bool IsReadOnlyEntitySetName()
        {
            var cet = TypedEFElement as ConceptualEntityType;
            if (cet != null)
            {
                return cet.HasResolvableBaseType || IsReadOnly;
            }
            else
            {
                return IsReadOnly;
            }
        }

        private bool IsReadOnly
        {
            get { return !TypedEFElement.EntityModel.IsCSDL; }
        }

        internal override bool IsReadOnlyName()
        {
            return IsReadOnly;
        }

        public bool IsReadOnlyBaseType()
        {
            return IsReadOnly;
        }

        public bool IsReadOnlyInheritanceModifier()
        {
            return IsReadOnly;
        }

        public bool IsReadOnlyAbstract()
        {
            return IsReadOnly;
        }

        public bool IsBrowsableAbstract()
        {
            return TypedEFElement.EntityModel.IsCSDL;
        }

        public bool IsBrowsableBaseType()
        {
            return TypedEFElement.EntityModel.IsCSDL;
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Abstract")]
        [LocDescription("PropertyWindow_Description_Abstract")]
        [TypeConverter(typeof(BoolConverter))]
        public bool Abstract
        {
            get
            {
                var cet = TypedEFElement as ConceptualEntityType;
                if (cet != null)
                {
                    return cet.Abstract.Value;
                }
                else
                {
                    return false;
                }
            }
            set
            {
                var cet = TypedEFElement as ConceptualEntityType;

                if (cet != null)
                {
                    if (value)
                    {
                        var etm = ModelHelper.FindEntityTypeMapping(null, TypedEFElement, EntityTypeMappingKind.Function, false);
                        if (etm != null)
                        {
                            // only show the warning dialog if the Entity has any Function mapping
                            var result = VsUtils.ShowMessageBox(
                                PackageManager.Package,
                                Resources.PropertyWindow_MessageBox_SetAbstract,
                                OLEMSGBUTTON.OLEMSGBUTTON_OKCANCEL,
                                OLEMSGDEFBUTTON.OLEMSGDEFBUTTON_FIRST,
                                OLEMSGICON.OLEMSGICON_WARNING);

                            if (result == DialogResult.Cancel)
                            {
                                return;
                            }
                        }
                    }

                    var cmd = new ChangeEntityTypeAbstractCommand(cet, value);
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        [LocCategory("PropertyWindow_Category_CodeGeneration")]
        [LocDisplayName("PropertyWindow_DisplayName_Access")]
        [LocDescription("PropertyWindow_Description_Access")]
        [TypeConverter(typeof(AccessConverter))]
        public string TypeAccess
        {
            get
            {
                var conc = TypedEFElement as ConceptualEntityType;
                if (conc != null)
                {
                    return conc.TypeAccess.Value;
                }
                else
                {
                    return string.Empty;
                }
            }
            set
            {
                var conc = TypedEFElement as ConceptualEntityType;
                if (conc != null)
                {
                    var cpc = PropertyWindowViewModelHelper.GetCommandProcessorContext();
                    var cmd = new UpdateEntityTypeTypeAccessCommand(conc.TypeAccess, value);
                    CommandProcessor.InvokeSingleCommand(cpc, cmd);
                }
            }
        }

        public bool IsBrowsableTypeAccess()
        {
            // only show this item if this is a conceptual entity type
            return TypedEFElement.EntityModel.IsCSDL;
        }

        public bool IsBrowsableDatabaseName()
        {
            // only show DB name if this item is a storage entity type
            return !TypedEFElement.EntityModel.IsCSDL;
        }

        public static bool IsReadOnlyDatabaseName()
        {
            return true;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_DatabaseName")]
        [LocDescription("PropertyWindow_Description_DatabaseName")]
        public string DatabaseName
        {
            get
            {
                var ses = TypedEFElement.EntitySet as StorageEntitySet;
                if (null != ses)
                {
                    return ses.DatabaseTableName;
                }

                return string.Empty;
            }
        }

        public bool IsBrowsableDatabaseSchema()
        {
            // only show DB name if this item is a storage entity type
            return !TypedEFElement.EntityModel.IsCSDL;
        }

        public static bool IsReadOnlyDatabaseSchema()
        {
            return true;
        }

        [LocCategory("PropertyWindow_Category_General")]
        [LocDisplayName("PropertyWindow_DisplayName_DatabaseSchema")]
        [LocDescription("PropertyWindow_Description_DatabaseSchema")]
        public string DatabaseSchema
        {
            get
            {
                var ses = TypedEFElement.EntitySet as StorageEntitySet;
                if (null != ses)
                {
                    return ses.DatabaseSchemaName;
                }

                return string.Empty;
            }
        }

        public override object GetDescriptorDefaultValue(string propertyDescriptorMethodName)
        {
            if (propertyDescriptorMethodName.Equals("TypeAccess"))
            {
                var conc = TypedEFElement as ConceptualEntityType;
                if (conc != null)
                {
                    return conc.TypeAccess.DefaultValue;
                }
                else
                {
                    return string.Empty;
                }
            }
            else if (propertyDescriptorMethodName.Equals("Abstract"))
            {
                return false;
            }
            return base.GetDescriptorDefaultValue(propertyDescriptorMethodName);
        }
    }
}
