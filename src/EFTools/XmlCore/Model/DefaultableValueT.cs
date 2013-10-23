// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace Microsoft.Data.Entity.Design.Model
{
    using System;
    using System.Diagnostics;
    using System.Globalization;
    using Microsoft.Data.Entity.Design.Common;
    using Microsoft.Data.Entity.Design.Model.Validation;
    using Microsoft.Data.Tools.XmlDesignerBase;

    /// <summary>
    ///     Represents a field in an EFElement for which the EF XSD defines a default value
    ///     if a value is not provided.
    /// </summary>
    internal abstract class DefaultableValue<T> : DefaultableValue, IValueProperty<T>
    {
        internal DefaultableValue(EFElement parent, string attributeName)
            : base(parent, parent.GetAttribute(attributeName))
        {
            Debug.Assert(parent != null && parent.XElement != null, "Unexpected parent == null or parent.Element == null");
        }

        internal DefaultableValue(EFElement parent, string attributeName, string attributeNamespace)
            : base(parent, parent.GetAttribute(attributeName, attributeNamespace))
        {
            _namespace = attributeNamespace;
            Debug.Assert(parent != null && parent.XElement != null, "Unexpected parent == null or parent.Element == null");
        }

        internal override string ToPrettyString()
        {
            return AttributeName + ":" + GetNonLocalizedAttributeValue(Value);
        }

        /// <summary>
        ///     For ploc builds Resources.NoneDisplayValueUsedForUX will be the localized version of the
        ///     '(None)' string. Convert it back to a non-localized string so that baseline files do not
        ///     need to be localized
        /// </summary>
        internal static object GetNonLocalizedAttributeValue(T attribute)
        {
            if (null != attribute
                && Resources.NoneDisplayValueUsedForUX.Equals(attribute.ToString(), StringComparison.CurrentCulture))
            {
                return "(None)";
            }

            return attribute;
        }

        internal abstract string AttributeName { get; }
        public abstract T DefaultValue { get; }

        [System.Diagnostics.CodeAnalysis.SuppressMessage("Microsoft.Design", "CA1031:DoNotCatchGeneralExceptionTypes")]
        internal virtual bool IsValidValue(T value)
        {
            if (value != null)
            {
                if (ValidateValueAgainstSchema())
                {
                    var stringValue = String.Empty;
                    try
                    {
                        stringValue = ConvertValueToString(value);
                    }
                    catch (Exception)
                    {
                        return false;
                    }
                    var contentValidator = Artifact.ModelManager.GetAttributeContentValidator(Artifact);
                    Debug.Assert(contentValidator != null, "Attribute content validator is null");
                    return contentValidator.IsValidAttributeValue(stringValue, this);
                }

                return true;
            }

            return false;
        }

        #region EFObject overrides

        internal override string SemanticName
        {
            get { return Parent.SemanticName + "/" + AttributeName + "(EFDefaultableValue)"; }
        }

        internal override string EFTypeName
        {
            get { return AttributeName; }
        }

        #endregion

        internal override string PropertyName
        {
            get { return AttributeName; }
        }

        internal override object ObjectValue
        {
            get { return Value; }
        }

        private T ReadValue()
        {
            var result = DefaultValue;

            if (XAttribute != null)
            {
                try
                {
                    result = ConvertStringToValue(GetXAttributeValue());
                }
                catch (FormatException)
                {
                    // we just swallow this here.  This will have been validated when being set, or when the file was validated. 
                }
            }

            return result;
        }

        /// <summary>
        ///     This method is used to take the T value in this instance and convert it to a string that can be written
        ///     out to the XAttribute.
        /// </summary>
        /// <param name="val"></param>
        /// <param name="errors"></param>
        /// <param name="errorLevel"></param>
        /// <returns></returns>
        protected internal virtual string ConvertValueToString(T val)
        {
            return GetXmlString(val);
        }

        internal virtual bool ValidateValueAgainstSchema()
        {
            return true;
        }

        /// <summary>
        ///     This method is used when reading the XAttribute value out of the XLinq tree and converting it to T for this
        ///     template.
        /// </summary>
        /// <param name="stringVal"></param>
        /// <param name="errors"></param>
        /// <param name="errorLevel"></param>
        /// <returns></returns>
        protected internal virtual T ConvertStringToValue(string stringVal)
        {
            return (T)ConvertToType(stringVal, XAttribute, typeof(T));
        }

        /// <summary>
        ///     Returns the value of the field, which is either the supplied value
        ///     or the default value if no value was supplied.
        /// </summary>
        public T Value
        {
            get
            {
                var result = ReadValue();
                return result;
            }

            set
            {
                string newValueString = null;

                if (value != null)
                {
                    newValueString = ConvertValueToString(value);
                }

                // We only have warnings or less if we are succeeding in the
                // set operation, go ahead and clear the error list and add the
                // warnings.
                Artifact.RemoveParseErrorsForObject(this);

                SetXAttributeValue(newValueString);

                //// do this validation after we set the value above, since that will guarantee we have a non-null xattribute 
                //// if the value is not null.  If value is null, then we've removed the XAttribute, in which case we don't validate the input
                //// Note that setting a value to null may make the outer XML element not valid with respect to schema, but we don't check that here.
                if (value != null)
                {
                    if (ValidateValueAgainstSchema())
                    {
                        var contentValidator = Artifact.ModelManager.GetAttributeContentValidator(Artifact);
                        Debug.Assert(contentValidator != null, "Attribute content validator is null");
                        if (!contentValidator.IsValidAttributeValue(newValueString, this))
                        {
                            var msg = string.Format(CultureInfo.CurrentCulture, Resources.INVALID_FORMAT, value);
                            Artifact.AddParseErrorForObject(
                                this, new ErrorInfo(ErrorInfo.Severity.ERROR, msg, this, ErrorCodes.INVALID_VALUE, ErrorClass.ParseError));
                        }
                    }
                }
            }
        }

        public override bool IsDefaulted
        {
            get { return (XAttribute == null); }
        }

        protected override void Dispose(bool disposing)
        {
            // Dispose(bool) is an abstract method on EFObject and so we need an implementation
            // this class doesn't have any special dispose needs, so use this no-op
            return;
        }

        protected override void AddToXlinq(string attributeValue)
        {
            if (IsConstructionCompleted)
            {
                base.AddToXlinq(attributeValue);
            }
        }

        public void ResetToDefault()
        {
            Value = DefaultValue;
        }

        public virtual bool IsReadOnly
        {
            get { return false; }
            set { throw new NotImplementedException(); }
        }

        public void SetToDefault()
        {
            RemoveFromXlinq();
        }

        public void SetToNotPresent()
        {
            RemoveFromXlinq();
        }

        public bool IsNotPresent
        {
            get { return (XAttribute == null); }
        }

        public virtual bool IsRequired
        {
            get { throw new NotImplementedException(); }
        }

        public virtual string NotPresentDisplayString
        {
            get { throw new NotImplementedException(); }
        }

        public object ActualValue
        {
            get
            {
                if (XAttribute != null)
                {
                    return XAttribute.Value;
                }
                else
                {
                    return null;
                }
            }
        }

        /// | IsNotPresent  |IsNotPresent == false|  IsNotPresent == false   |
        /// |  == true      |  && ActualValue is  |  ActualValue is not      |
        /// |               | convertible to T    |  Convertible to T        |
        /// -------------|---------------|-------------------- |--------------------------|
        /// IsRequired   |  return false |  return true        |  return false            |
        /// -------------|---------------|---------------------|--------------------------|
        /// ! IsRequired |   return true |  return true        |  return false            |
        /// -------------|---------------|---------------------|--------------------------|
        public bool TryGetValue(out T value)
        {
            value = default(T);

            if (IsNotPresent == false)
            {
                Debug.Assert(XAttribute != null, "null value for XAttribute, but IsNotPresent is false");
                var actualT = ConvertStringToValue(XAttribute.Value);
                if (actualT != null)
                {
                    value = actualT;
                    return true;
                }
                else
                {
                    return false;
                }
            }
            else
            {
                Debug.Assert(XAttribute == null, "null value for XAttribute, but IsNotPresent is false");

                if (IsRequired)
                {
                    return false;
                }
                else
                {
                    value = DefaultValue;
                    return true;
                }
            }
        }
    }
}
