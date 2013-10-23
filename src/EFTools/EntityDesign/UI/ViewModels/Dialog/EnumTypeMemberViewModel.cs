// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.UI.ViewModels
{
    using System;
    using System.ComponentModel;
    using System.Diagnostics;
    using System.Globalization;
    using System.Linq;
    using System.Text;
    using Microsoft.Data.Entity.Design.Model;
    using Microsoft.Data.Entity.Design.Model.Entity;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Resources = Microsoft.Data.Entity.Design.Resources;

    internal class EnumTypeMemberViewModel : INotifyPropertyChanged, IDataErrorInfo
    {
        private readonly EnumTypeMember _member;
        private string _name;
        private string _value;

        public EnumTypeMemberViewModel()
            : this(null, null)
        {
        }

        public EnumTypeMemberViewModel(EnumTypeViewModel parent, EnumTypeMember member)
        {
            _member = member;

            _name = String.Empty;
            _value = null;
            if (_member != null)
            {
                _name = member.Name.Value;
                _value = member.Value.Value;
            }
            Parent = parent;
        }

        public EnumTypeViewModel Parent { get; set; }

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

        public string Value
        {
            get { return _value; }
            set
            {
                if (_value != value)
                {
                    _value = value;
                    OnPropertyChanged("Value");
                }
            }
        }

        protected void OnPropertyChanged(string propName)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged(this, new PropertyChangedEventArgs(propName));
            }
        }

        #region INotifyPropertyChanged

        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region IDataErrorInfo

        public string Error
        {
            get { return this[null]; }
        }

        public string this[string propertyName]
        {
            get
            {
                if (Parent == null
                    || (String.IsNullOrEmpty(Name) && String.IsNullOrEmpty(Value)))
                {
                    return String.Empty;
                }

                var sb = new StringBuilder();

                if (propertyName == null
                    || propertyName == "Name")
                {
                    if (string.IsNullOrWhiteSpace(Name)
                        || !EscherAttributeContentValidator.IsValidCsdlEnumMemberName(Name))
                    {
                        sb.AppendLine(String.Format(CultureInfo.CurrentCulture, Resources.EnumDialog_ErrorEnumMemberBadname, Name));
                    }
                    else if (Parent.Members.Count(etm => String.Compare(etm.Name, Name, StringComparison.CurrentCulture) == 0) > 1)
                    {
                        sb.AppendLine(
                            String.Format(CultureInfo.CurrentCulture, Resources.EnumDialog_ErrorEnumMemberDuplicateName, Name));
                    }
                }

                if (propertyName == null
                    || propertyName == "Value")
                {
                    if (string.IsNullOrEmpty(Value) == false) // we will validate if the user entered white spaces.
                    {
                        var type =
                            ModelHelper.UnderlyingEnumTypes.FirstOrDefault(
                                t => String.CompareOrdinal(t.Name, Parent.SelectedUnderlyingType) == 0);

                        Debug.Assert(
                            type != null,
                            "The type :" + Parent.SelectedUnderlyingType + " is not a valid underlying type for an enum.");

                        // if value is not null or empty, we will validate the input based on enum type's underlying type.
                        if (type != null
                            && ModelHelper.IsValidValueForType(type, Value) == false)
                        {
                            sb.AppendLine(String.Format(CultureInfo.CurrentCulture, Model.Resources.BadEnumTypeMemberValue, Value));
                        }
                    }
                }
                return sb.ToString();
            }
        }

        #endregion
    }
}
