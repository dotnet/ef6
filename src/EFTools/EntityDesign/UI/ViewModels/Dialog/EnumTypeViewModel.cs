// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels
{
    using System;
    using System.Collections.Generic;
    using System.Collections.ObjectModel;
    using System.Collections.Specialized;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EnumTypeViewModel : IDataErrorInfo, INotifyPropertyChanged
    {
        private readonly EnumType _enumType;
        private bool _isFlag;
        private bool _isReferenceExternalType;
        private string _name;
        private string _underlyingType;
        private string _externalTypeName;
        private static IEnumerable<string> _underlyingTypes;
        private readonly EntityDesignArtifact _artifact;
        private ObservableCollection<EnumTypeMemberViewModel> _members;
        private bool _isValid;

        public EnumTypeViewModel(EntityDesignArtifact artifact, string underlyingType)
        {
            Initialize();
            _underlyingType = String.IsNullOrWhiteSpace(underlyingType) ? ModelConstants.Int32PropertyType : underlyingType;
            _isFlag = false;
            _name = String.Empty;
            _artifact = artifact;
            _isValid = false;
            _externalTypeName = String.Empty;
            _isReferenceExternalType = false;
        }

        public EnumTypeViewModel(EnumType enumType)
        {
            Initialize();
            Debug.Assert(enumType != null, "Parameter enum type is null");
            if (enumType != null)
            {
                _enumType = enumType;
                _artifact = enumType.Artifact as EntityDesignArtifact;

                _isFlag = enumType.IsFlags.Value;
                _name = enumType.Name.Value;
                _underlyingType = enumType.UnderlyingType.Value;
                _externalTypeName = enumType.ExternalTypeName.Value;
                _isReferenceExternalType = (String.IsNullOrWhiteSpace(_externalTypeName) == false);

                foreach (var member in enumType.Members())
                {
                    var vm = new EnumTypeMemberViewModel(this, member);
                    vm.PropertyChanged += enumTypeMember_PropertyChanged;
                    Members.Add(vm);
                }
            }
            _isValid = true;
        }

        private void Initialize()
        {
            _members = new ObservableCollection<EnumTypeMemberViewModel>();
            _members.CollectionChanged += EnumTypeMember_CollectionChanged;
        }

        private void SetViewModelIsValidState()
        {
            IsValid = false;
            if (String.IsNullOrWhiteSpace(Error) == false)
            {
                return;
            }

            foreach (var member in _members)
            {
                if (String.IsNullOrWhiteSpace(member.Error) == false)
                {
                    return;
                }
            }
            IsValid = true;
        }

        #region Properties

        public bool IsNew
        {
            get { return (_enumType == null); }
        }

        public EnumType EnumType
        {
            get { return _enumType; }
        }

        public Version SchemaVersion
        {
            get { return _artifact.SchemaVersion; }
        }

        public string Name
        {
            get { return _name; }
            set
            {
                if (_name != value)
                {
                    _name = value;
                    OnPropertyChanged("Name");
                }
            }
        }

        public bool IsFlag
        {
            get { return _isFlag; }
            set
            {
                if (_isFlag != value)
                {
                    _isFlag = value;
                    OnPropertyChanged("IsFlag");
                }
            }
        }

        /// <summary>
        ///     The referenced external type name.
        ///     if IsReferenceExternalType is true, the value will be ignored.
        /// </summary>
        public string ExternalTypeName
        {
            get { return _externalTypeName; }
            set
            {
                if (_externalTypeName != value)
                {
                    _externalTypeName = value;
                    OnPropertyChanged("ExternalTypeName");
                }
            }
        }

        public bool IsReferenceExternalType
        {
            get { return _isReferenceExternalType; }
            set
            {
                if (_isReferenceExternalType != value)
                {
                    _isReferenceExternalType = value;
                    OnPropertyChanged("IsReferenceExternalType");

                    // If the user clears out the reference-external-type check box, we need to reset the ExternalTypeName textbox. 
                    if (_isReferenceExternalType == false)
                    {
                        ExternalTypeName = String.Empty;
                    }
                }
            }
        }

        public string SelectedUnderlyingType
        {
            get { return _underlyingType; }
            set
            {
                if (_underlyingType != value)
                {
                    _underlyingType = value;
                    OnPropertyChanged("UnderlyingType");
                }
            }
        }

        public static IEnumerable<string> UnderlyingTypes
        {
            get
            {
                if (_underlyingTypes == null)
                {
                    _underlyingTypes = ModelHelper.UnderlyingEnumTypes.Select(t => t.Name);
                }
                return _underlyingTypes;
            }
        }

        public ObservableCollection<EnumTypeMemberViewModel> Members
        {
            get { return _members; }
        }

        public bool IsValid
        {
            get { return _isValid; }
            set
            {
                _isValid = value;
                OnPropertyChanged("IsValid");
            }
        }

        #endregion

        protected void OnPropertyChanged(string propName)
        {
            if (propName != "IsValid")
            {
                SetViewModelIsValidState();
            }

            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        private void EnumTypeMember_CollectionChanged(object sender, NotifyCollectionChangedEventArgs e)
        {
            for (var i = 0; e.NewItems != null && i < e.NewItems.Count; i++)
            {
                var enumTypeMember = e.NewItems[i] as EnumTypeMemberViewModel;
                Debug.Assert(enumTypeMember != null, "Expected new item type: EnumTypeViewModel, Actual:" + e.NewItems[i].GetType().Name);

                if (enumTypeMember != null)
                {
                    enumTypeMember.Parent = this;
                    enumTypeMember.PropertyChanged += enumTypeMember_PropertyChanged;
                }
            }
            SetViewModelIsValidState();
        }

        private void enumTypeMember_PropertyChanged(object sender, PropertyChangedEventArgs e)
        {
            SetViewModelIsValidState();
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDataErrorInfo

        public string Error
        {
            get
            {
                var sb = new StringBuilder();
                sb.AppendLine(this["Name"]);
                sb.AppendLine(this["ExternalTypeName"]);
                return sb.ToString();
            }
        }

        /// <summary>
        ///     This method will return validation error for a given property (if there is one).
        /// </summary>
        public string this[string propertyName]
        {
            get
            {
                if (propertyName == "Name")
                {
                    string errorMessage;
                    if (string.IsNullOrWhiteSpace(_name)
                        || !EscherAttributeContentValidator.IsValidCsdlEnumTypeName(_name))
                    {
                        return String.Format(CultureInfo.CurrentCulture, Resources.EnumDialog_ErrorEnumTypeBadname, _name);
                    }
                    else if (IsNew
                             && ModelHelper.IsUniqueName(typeof(EnumType), _artifact.ConceptualModel, _name, true, out errorMessage)
                             == false)
                    {
                        return Resources.EnumDialog_EnsureEnumTypeUnique;
                    }
                        // if the name has changed, ensure that it will be unique across other types.
                    else if (IsNew == false
                             && string.CompareOrdinal(_enumType.Name.Value, Name) != 0
                             && ModelHelper.IsUniqueNameForExistingItem(_enumType, Name, true, out errorMessage) == false)
                    {
                        return errorMessage;
                    }
                }
                else if (propertyName == "ExternalTypeName")
                {
                    if (IsReferenceExternalType && String.IsNullOrWhiteSpace(ExternalTypeName))
                    {
                        return Resources.EnumDialog_ErrorEnterValueForExternalTypeName;
                    }
                }
                return String.Empty;
            }
        }

        #endregion
    }
}
