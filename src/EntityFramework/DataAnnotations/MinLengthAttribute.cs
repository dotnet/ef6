// Copyright (c) Microsoft Open Technologies, Inc. All rights reserved. See License.txt in the project root for license information.


#if NET40

namespace System.ComponentModel.DataAnnotations
{
    using System.Data.Entity.Resources;
    using System.Diagnostics.CodeAnalysis;
    using System.Globalization;

    /// <summary>
    /// Specifies the minimum length of array/string data allowed in a property.
    /// </summary>
    [AttributeUsage(AttributeTargets.Property | AttributeTargets.Field | AttributeTargets.Parameter, AllowMultiple = false)]
    [SuppressMessage("Microsoft.Performance", "CA1813:AvoidUnsealedAttributes",
        Justification = "We want users to be able to extend this class")]
    public class MinLengthAttribute : ValidationAttribute
    {
        /// <summary>
        /// Gets the minimum allowable length of the array/string data.
        /// </summary>
        public int Length { get; private set; }

        /// <summary>
        /// Initializes a new instance of the <see cref="MinLengthAttribute" /> class.
        /// </summary>
        /// <param name="length"> The minimum allowable length of array/string data. Value must be greater than or equal to zero. </param>
        public MinLengthAttribute(int length)
            : base(() => EntityRes.GetString(EntityRes.MinLengthAttribute_ValidationError))
        {
            Length = length;
        }

        /// <summary>
        /// Determines whether a specified object is valid. (Overrides <see cref="ValidationAttribute.IsValid(object)" />)
        /// </summary>
        /// <remarks>
        /// This method returns <c>true</c> if the <paramref name="value" /> is null.  
        /// It is assumed the <see cref="RequiredAttribute" /> is used if the value may not be null.
        /// </remarks>
        /// <param name="value"> The object to validate. </param>
        /// <returns> <c>true</c> if the value is null or greater than or equal to the specified minimum length, otherwise <c>false</c> </returns>
        /// <exception cref="InvalidOperationException">Length is less than zero.</exception>
        public override bool IsValid(object value)
        {
            // Check the lengths for legality
            EnsureLegalLengths();

            if (value == null)
            {
                return true;
            }

            var str = value as string;
            var length = str != null ? str.Length : ((Array)value).Length;

            // Automatically pass if value is null. RequiredAttribute should be used to assert a value is not null.
            // We expect a cast exception if a non-{string|array} property was passed in.
            return length >= Length;
        }

        /// <summary>
        /// Applies formatting to a specified error message. (Overrides <see cref="ValidationAttribute.FormatErrorMessage" />)
        /// </summary>
        /// <param name="name"> The name to include in the formatted string. </param>
        /// <returns> A localized string to describe the minimum acceptable length. </returns>
        public override string FormatErrorMessage(string name)
        {
            // An error occurred, so we know the value is less than the minimum
            return string.Format(CultureInfo.CurrentCulture, ErrorMessageString, name, Length);
        }

        /// <summary>
        /// Checks that Length has a legal value.  Throws InvalidOperationException if not.
        /// </summary>
        private void EnsureLegalLengths()
        {
            if (Length < 0)
            {
                throw Error.MinLengthAttribute_InvalidMinLength();
            }
        }
    }
}

#endif
