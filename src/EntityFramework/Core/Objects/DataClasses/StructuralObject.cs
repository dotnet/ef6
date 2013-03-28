// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.

namespace System.Data.Entity.Core.Objects.DataClasses
{
    using System.ComponentModel;
    using System.Data.Entity.Core.Common.Utils;
    using System.Data.Entity.Resources;
    using System.Data.Entity.Spatial;
    using System.Data.Entity.Utilities;
    using System.Diagnostics.CodeAnalysis;
    using System.Runtime.Serialization;

    /// <summary>
    ///     This class contains the common methods need for an date object.
    /// </summary>
    [DataContract(IsReference = true)]
    [Serializable]
    public abstract class StructuralObject : INotifyPropertyChanging, INotifyPropertyChanged
    {
        // ------
        // Fields
        // ------

        // This class contains no fields that are serialized, but it's important to realize that
        // adding or removing a serialized field is considered a breaking change.  This includes
        // changing the field type or field name of existing serialized fields. If you need to make
        // this kind of change, it may be possible, but it will require some custom
        // serialization/deserialization code.

        /// <summary>
        ///     Public constant name used for change tracking
        ///     Providing this definition allows users to use this constant instead of
        ///     hard-coding the string. This helps to ensure the property name is correct
        ///     and allows faster comparisons in places where we are looking for this specific string.
        ///     Users can still use the case-sensitive string directly instead of the constant,
        ///     it will just be slightly slower on comparison.
        ///     Including the dash (-) character around the name ensures that this will not conflict with
        ///     a real data property, because -EntityKey- is not a valid identifier name
        /// </summary>
        public const string EntityKeyPropertyName = "-EntityKey-";

        #region INotifyPropertyChanged Members

        /// <summary>
        ///     Notification that a property has been changed.
        /// </summary>
        /// <remarks>
        ///     The PropertyChanged event can indicate all properties on the
        ///     object have changed by using either a null reference
        ///     (Nothing in Visual Basic) or String.Empty as the property name
        ///     in the PropertyChangedEventArgs.
        /// </remarks>
        [field: NonSerialized]
        public event PropertyChangedEventHandler PropertyChanged;

        #endregion

        #region INotifyPropertyChanging Members

        /// <summary>
        ///     Notification that a property is about to be changed.
        /// </summary>
        /// <remarks>
        ///     The PropertyChanging event can indicate all properties on the
        ///     object are changing by using either a null reference
        ///     (Nothing in Visual Basic) or String.Empty as the property name
        ///     in the PropertyChangingEventArgs.
        /// </remarks>
        [field: NonSerialized]
        public event PropertyChangingEventHandler PropertyChanging;

        #endregion

        #region Protected Overrideable

        /// <summary>
        ///     Raises the <see cref="E:System.Data.Entity.Core.Objects.DataClasses.StructuralObject.PropertyChanged" /> event.
        /// </summary>
        /// <param name="property">The name of the changed property.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void OnPropertyChanged(string property)
        {
            if (PropertyChanged != null)
            {
                PropertyChanged.Invoke(this, new PropertyChangedEventArgs(property));
            }
        }

        /// <summary>
        ///     Raises the <see cref="E:System.Data.Entity.Core.Objects.DataClasses.StructuralObject.PropertyChanging" /> event.
        /// </summary>
        /// <param name="property">The name of the property changing.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void OnPropertyChanging(string property)
        {
            if (PropertyChanging != null)
            {
                PropertyChanging.Invoke(this, new PropertyChangingEventArgs(property));
            }
        }

        #endregion

        #region Protected Helper

        /// <summary>Returns the minimum date time value supported by the data source.</summary>
        /// <returns>
        ///     A <see cref="T:System.DateTime" /> value that is the minimum date time that is supported by the data source.
        /// </returns>
        protected static DateTime DefaultDateTimeValue()
        {
            return DateTime.Now;
        }

        /// <summary>Raises an event that is used to report that a property change is pending.</summary>
        /// <param name="property">The name of the changing property.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void ReportPropertyChanging(
            string property)
        {
            Check.NotEmpty(property, "property");

            OnPropertyChanging(property);
        }

        /// <summary>Raises an event that is used to report that a property change has occurred.</summary>
        /// <param name="property">The name for the changed property.</param>
        [SuppressMessage("Microsoft.Naming", "CA1716:IdentifiersShouldNotMatchKeywords", MessageId = "Property")]
        protected virtual void ReportPropertyChanged(
            string property)
        {
            Check.NotEmpty(property, "property");

            OnPropertyChanged(property);
        }

        /// <summary>Returns a complex type for the specified property.</summary>
        /// <remarks>
        ///     Unlike most of the other helper methods in this class, this one is not static
        ///     because it references the SetValidValue for complex objects, which is also not static
        ///     because it needs a reference to this.
        /// </remarks>        
        /// <returns>A complex type object for the property.</returns>
        /// <param name="currentValue">A complex object that inherits from complex object.</param>
        /// <param name="property">The name of the complex property that is the complex object.</param>
        /// <param name="isNullable">Indicates whether the type supports null values.</param>
        /// <param name="isInitialized">Indicates whether the type is initialized.</param>
        /// <typeparam name="T">The type of the complex object being requested.</typeparam>
        protected internal T GetValidValue<T>(T currentValue, string property, bool isNullable, bool isInitialized)
            where T : ComplexObject, new()
        {
            // If we support complex type inheritance we will also need to check if T is abstract            
            if (!isNullable
                && !isInitialized)
            {
                currentValue = SetValidValue(currentValue, new T(), property);
            }

            return currentValue;
        }

        /// <summary>
        ///     This method is called by a ComplexObject contained in this Entity
        ///     whenever a change is about to be made to a property of the
        ///     ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName"> The name of the top-level entity property that contains the ComplexObject that is calling this method. </param>
        /// <param name="complexObject"> The instance of the ComplexObject on which the property is changing. </param>
        /// <param name="complexMemberName"> The name of the changing property on complexObject. </param>
        internal abstract void ReportComplexPropertyChanging(
            string entityMemberName, ComplexObject complexObject, string complexMemberName);

        /// <summary>
        ///     This method is called by a ComplexObject contained in this Entity
        ///     whenever a change has been made to a property of the
        ///     ComplexObject so that the change can be forwarded to the change tracker.
        /// </summary>
        /// <param name="entityMemberName"> The name of the top-level entity property that contains the ComplexObject that is calling this method. </param>
        /// <param name="complexObject"> The instance of the ComplexObject on which the property is changing. </param>
        /// <param name="complexMemberName"> The name of the changing property on complexObject. </param>
        internal abstract void ReportComplexPropertyChanged(
            string entityMemberName, ComplexObject complexObject, string complexMemberName);

        /// <summary>
        ///     Determines whether the structural object is attached to a change tracker or not
        /// </summary>
        internal abstract bool IsChangeTracked { get; }

        /// <summary>Determines whether the specified byte arrays contain identical values.</summary>
        /// <returns>true if both arrays are of the same length and contain the same byte values or if both arrays are null; otherwise, false.</returns>
        /// <param name="first">The first byte array value to compare.</param>
        /// <param name="second">The second byte array to compare.</param>
        protected internal static bool BinaryEquals(byte[] first, byte[] second)
        {
            if (ReferenceEquals(first, second))
            {
                return true;
            }

            if (first == null
                || second == null)
            {
                return false;
            }

            return ByValueEqualityComparer.CompareBinaryValues(first, second);
        }

        /// <summary>Returns a copy of the current byte value.</summary>
        /// <returns>
        ///     A copy of the current <see cref="T:System.Byte" /> value.
        /// </returns>
        /// <param name="currentValue">The current byte array value.</param>
        protected internal static byte[] GetValidValue(byte[] currentValue)
        {
            if (currentValue == null)
            {
                return null;
            }
            return (byte[])currentValue.Clone();
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Byte[]" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Byte" /> value being validated.
        /// </returns>
        /// <param name="value">The value passed into the property setter.</param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        /// <exception cref="T:System.Data.ConstraintException">If value is null for a non nullable value.</exception>
        protected internal static Byte[] SetValidValue(Byte[] value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
                return value;
            }
            return (byte[])value.Clone();
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Byte[]" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Byte" /> value being set.
        /// </returns>
        /// <param name="value">The value being set.</param>
        /// <param name="isNullable">Indicates whether the property is nullable.</param>
        protected internal static Byte[] SetValidValue(Byte[] value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Boolean" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Boolean" /> value being set.
        /// </returns>
        /// <param name="value">The Boolean value.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static bool SetValidValue(bool value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Boolean" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Boolean" /> value being set.
        /// </returns>
        /// <param name="value">The Boolean value.</param>
        protected internal static bool SetValidValue(bool value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Boolean" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Boolean" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Boolean" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static bool? SetValidValue(bool? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Boolean" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Boolean" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Boolean" /> value.
        /// </param>
        protected internal static bool? SetValidValue(bool? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Byte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.Byte" /> that is set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Byte" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static byte SetValidValue(byte value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Byte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Byte" /> value that is set.
        /// </returns>
        /// <param name="value">The value that is being validated.</param>
        protected internal static byte SetValidValue(byte value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Byte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Byte" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Byte" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static byte? SetValidValue(byte? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Byte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Byte" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Byte" /> value.
        /// </param>
        protected internal static byte? SetValidValue(byte? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.SByte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.SByte" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.SByte" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static sbyte SetValidValue(sbyte value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.SByte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.SByte" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.SByte" /> value.
        /// </param>
        [CLSCompliant(false)]
        protected internal static sbyte SetValidValue(sbyte value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.SByte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.SByte" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.SByte" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static sbyte? SetValidValue(sbyte? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.SByte" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.SByte" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.SByte" /> value.
        /// </param>
        [CLSCompliant(false)]
        protected internal static sbyte? SetValidValue(sbyte? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTime" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.DateTime" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.DateTime" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static DateTime SetValidValue(DateTime value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTime" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.DateTime" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.DateTime" /> value.
        /// </param>
        protected internal static DateTime SetValidValue(DateTime value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTime" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.DateTime" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.DateTime" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static DateTime? SetValidValue(DateTime? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTime" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.DateTime" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.DateTime" /> value.
        /// </param>
        protected internal static DateTime? SetValidValue(DateTime? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.TimeSpan" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.TimeSpan" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.TimeSpan" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static TimeSpan SetValidValue(TimeSpan value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.TimeSpan" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.TimeSpan" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.TimeSpan" /> value.
        /// </param>
        protected internal static TimeSpan SetValidValue(TimeSpan value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.TimeSpan" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.TimeSpan" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.TimeSpan" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static TimeSpan? SetValidValue(TimeSpan? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.TimeSpan" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.TimeSpan" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.TimeSpan" /> value.
        /// </param>
        protected internal static TimeSpan? SetValidValue(TimeSpan? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTimeOffset" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.DateTimeOffset" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.DateTimeOffset" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static DateTimeOffset SetValidValue(DateTimeOffset value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTimeOffset" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     A <see cref="T:System.DateTimeOffset" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.DateTimeOffset" /> value.
        /// </param>
        protected internal static DateTimeOffset SetValidValue(DateTimeOffset value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTimeOffset" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.DateTimeOffset" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.DateTimeOffset" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static DateTimeOffset? SetValidValue(DateTimeOffset? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.DateTimeOffset" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.DateTimeOffset" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.DateTimeOffset" /> value.
        /// </param>
        protected internal static DateTimeOffset? SetValidValue(DateTimeOffset? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Decimal" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Decimal" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Decimal" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static Decimal SetValidValue(Decimal value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Decimal" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Decimal" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Decimal" /> value.
        /// </param>
        protected internal static Decimal SetValidValue(Decimal value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Decimal" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Decimal" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Decimal" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static decimal? SetValidValue(decimal? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Decimal" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Decimal" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Decimal" /> value.
        /// </param>
        protected internal static decimal? SetValidValue(decimal? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Double" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Double" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Double" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static double SetValidValue(double value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Double" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Double" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Double" /> value.
        /// </param>
        protected internal static double SetValidValue(double value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Double" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Double" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Double" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static double? SetValidValue(double? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Double" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Double" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Double" /> value.
        /// </param>
        protected internal static double? SetValidValue(double? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>Makes sure the Single value being set for a property is valid.</summary>
        /// <returns>
        ///     The <see cref="T:System.Single" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Single" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static float SetValidValue(Single value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>Makes sure the Single value being set for a property is valid.</summary>
        /// <returns>
        ///     The <see cref="T:System.Single" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Single" /> value.
        /// </param>
        protected internal static float SetValidValue(Single value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Single" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Single" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Single" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static float? SetValidValue(float? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Single" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Single" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Single" /> value.
        /// </param>
        protected internal static float? SetValidValue(float? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Guid" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Guid" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Guid" /> value.
        /// </param>
        /// <param name="propertyName">Name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static Guid SetValidValue(Guid value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Guid" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Guid" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Guid" /> value.
        /// </param>
        protected internal static Guid SetValidValue(Guid value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Guid" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Guid" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Guid" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static Guid? SetValidValue(Guid? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Guid" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Guid" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Guid" /> value.
        /// </param>
        protected internal static Guid? SetValidValue(Guid? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int16" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Int16" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Int16" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static Int16 SetValidValue(Int16 value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int16" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Int16" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Int16" /> value.
        /// </param>
        protected internal static Int16 SetValidValue(Int16 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int16" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Int16" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Int16" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static short? SetValidValue(short? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int16" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Int16" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Int16" /> value.
        /// </param>
        protected internal static short? SetValidValue(short? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int32" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Int32" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Int32" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static Int32 SetValidValue(Int32 value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int32" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Int32" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Int32" /> value.
        /// </param>
        protected internal static Int32 SetValidValue(Int32 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int32" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Int32" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Int32" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static int? SetValidValue(int? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int32" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Int32" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Int32" /> value.
        /// </param>
        protected internal static int? SetValidValue(int? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Int64" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Int64" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static Int64 SetValidValue(Int64 value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.Int64" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Int64" /> value.
        /// </param>
        protected internal static Int64 SetValidValue(Int64 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Int64" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Int64" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        protected internal static long? SetValidValue(long? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.Int64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The nullable <see cref="T:System.Int64" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The nullable <see cref="T:System.Int64" /> value.
        /// </param>
        protected internal static long? SetValidValue(long? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt16" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.UInt16" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.UInt16" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static UInt16 SetValidValue(UInt16 value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt16" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.UInt16" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.UInt16" /> value.
        /// </param>
        [CLSCompliant(false)]
        protected internal static UInt16 SetValidValue(UInt16 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>Makes sure the UInt16 value being set for a property is valid.</summary>
        /// <returns>The nullable UInt16 value being set.</returns>
        /// <param name="value">The nullable UInt16 value.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static ushort? SetValidValue(ushort? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>Makes sure the UInt16 value being set for a property is valid.</summary>
        /// <returns>The nullable UInt16 value being set.</returns>
        /// <param name="value">The nullable UInt16 value.</param>
        [CLSCompliant(false)]
        protected internal static ushort? SetValidValue(ushort? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt32" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.UInt32" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.UInt32" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static UInt32 SetValidValue(UInt32 value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt32" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.UInt32" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.UInt32" /> value.
        /// </param>
        [CLSCompliant(false)]
        protected internal static UInt32 SetValidValue(UInt32 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>Makes sure the UInt32 value being set for a property is valid.</summary>
        /// <returns>The nullable UInt32 value being set.</returns>
        /// <param name="value">The nullable UInt32 value.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static uint? SetValidValue(uint? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>Makes sure the UInt32 value being set for a property is valid.</summary>
        /// <returns>The nullable UInt32 value being set.</returns>
        /// <param name="value">The nullable UInt32 value.</param>
        [CLSCompliant(false)]
        protected internal static uint? SetValidValue(uint? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.UInt64" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.UInt64" /> value.
        /// </param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static UInt64 SetValidValue(UInt64 value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>
        ///     The <see cref="T:System.UInt64" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.UInt64" /> value.
        /// </param>
        [CLSCompliant(false)]
        protected internal static UInt64 SetValidValue(UInt64 value)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>The nullable UInt64 value being set.</returns>
        /// <param name="value">The nullable UInt64 value.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        [SuppressMessage("Microsoft.Usage", "CA1801:ReviewUnusedParameters", MessageId = "propertyName")]
        [CLSCompliant(false)]
        protected internal static ulong? SetValidValue(ulong? value, string propertyName)
        {
            // no checks yet
            return value;
        }

        /// <summary>
        ///     Makes sure the <see cref="T:System.UInt64" /> value being set for a property is valid.
        /// </summary>
        /// <returns>The nullable UInt64 value being set.</returns>
        /// <param name="value">The nullable UInt64 value.</param>
        [CLSCompliant(false)]
        protected internal static ulong? SetValidValue(ulong? value)
        {
            // no checks yet
            return value;
        }

        /// <summary>Validates that the property is not null, and throws if it is.</summary>
        /// <returns>The validated property.</returns>
        /// <param name="value">The string value to be checked.</param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        /// <exception cref="T:System.Data.ConstraintException">The string value is null for a non-nullable string.</exception>
        protected internal static string SetValidValue(string value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
            }
            return value;
        }

        /// <summary>Validates that the property is not null, and throws if it is.</summary>
        /// <returns>
        ///     The validated <see cref="T:System.String" /> value.
        /// </returns>
        /// <param name="value">The string value to be checked.</param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        protected internal static string SetValidValue(string value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>Validates that the property is not null, and throws if it is.</summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value to be checked.
        /// </param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        /// <param name="propertyName">Name of the property that is being validated.</param>
        /// <exception cref="T:System.Data.ConstraintException">The value is null for a non-nullable property.</exception>
        protected internal static DbGeography SetValidValue(DbGeography value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
            }
            return value;
        }

        /// <summary>Validates that the property is not null, and throws if it is.</summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     <see cref="T:System.Data.Entity.Spatial.DbGeography" /> value to be checked.
        /// </param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        /// <exception cref="T:System.Data.ConstraintException">The value is null for a non-nullable property.</exception>
        protected internal static DbGeography SetValidValue(DbGeography value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>Validates that the property is not null, and throws if it is.</summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value to be checked.
        /// </param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        /// <param name="propertyName">The name of the property that is being validated.</param>
        /// <exception cref="T:System.Data.ConstraintException">The value is null for a non-nullable property.</exception>
        protected internal static DbGeometry SetValidValue(DbGeometry value, bool isNullable, string propertyName)
        {
            if (value == null)
            {
                if (!isNullable)
                {
                    EntityUtil.ThrowPropertyIsNotNullable(propertyName);
                }
            }
            return value;
        }

        /// <summary>Validates that the property is not null, and throws if it is.</summary>
        /// <returns>
        ///     The <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value being set.
        /// </returns>
        /// <param name="value">
        ///     The <see cref="T:System.Data.Entity.Spatial.DbGeometry" /> value to be checked.
        /// </param>
        /// <param name="isNullable">Flag indicating if this property is allowed to be null.</param>
        /// <exception cref="T:System.Data.ConstraintException">The value is null for a non-nullable property.</exception>
        protected internal static DbGeometry SetValidValue(DbGeometry value, bool isNullable)
        {
            return SetValidValue(value, isNullable, null);
        }

        /// <summary>Sets a complex object for the specified property.</summary>
        /// <returns>A complex type that derives from complex object.</returns>
        /// <param name="oldValue">The original complex object for the property, if any.</param>
        /// <param name="newValue">The complex object is being set.</param>
        /// <param name="property">The complex property that is being set to the complex object.</param>
        /// <typeparam name="T">The type of the object being replaced.</typeparam>
        protected internal T SetValidValue<T>(T oldValue, T newValue, string property) where T : ComplexObject
        {
            // Nullable complex types are not supported in v1, but we allow setting null here if the parent entity is detached
            if (newValue == null && IsChangeTracked)
            {
                throw new InvalidOperationException(Strings.ComplexObject_NullableComplexTypesNotSupported(property));
            }

            if (oldValue != null)
            {
                oldValue.DetachFromParent();
            }

            if (newValue != null)
            {
                newValue.AttachToParent(this, property);
            }

            return newValue;
        }

        /// <summary>Verifies that a complex object is not null.</summary>
        /// <returns>The complex object being validated.</returns>
        /// <param name="complexObject">The complex object that is being validated.</param>
        /// <param name="propertyName">The complex property on the parent object that is associated with  complexObject .</param>
        /// <typeparam name="TComplex">The type of the complex object being verified.</typeparam>
        protected internal static TComplex VerifyComplexObjectIsNotNull<TComplex>(TComplex complexObject, string propertyName)
            where TComplex : ComplexObject
        {
            if (complexObject == null)
            {
                EntityUtil.ThrowPropertyIsNotNullable(propertyName);
            }
            return complexObject;
        }

        #endregion
    }
}
