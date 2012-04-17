namespace System.Data.Entity.Core.Common.EntitySql.AST
{
    using System.Data.Entity.Resources;
    using System.Diagnostics;
    using System.Globalization;

    /// <summary>
    /// Defines literal value kind, including the eSQL untyped NULL.
    /// </summary>
    internal enum LiteralKind
    {
        Number,
        String,
        UnicodeString,
        Boolean,
        Binary,
        DateTime,
        Time,
        DateTimeOffset,
        Guid,
        Null
    }

    /// <summary>
    /// Represents a literal ast node.
    /// </summary>
    internal sealed class Literal : Node
    {
        private readonly LiteralKind _literalKind;
        private string _originalValue;
        private bool _wasValueComputed;
        private object _computedValue;
        private Type _type;
        private static readonly Byte[] _emptyByteArray = new byte[0];

        /// <summary>
        /// Initializes a literal ast node.
        /// </summary>
        /// <param name="originalValue">literal value in cql string representation</param>
        /// <param name="kind">literal value class</param>
        /// <param name="query">query</param>
        /// <param name="inputPos">input position</param>
        internal Literal(string originalValue, LiteralKind kind, string query, int inputPos)
            : base(query, inputPos)
        {
            _originalValue = originalValue;
            _literalKind = kind;
        }

        /// <summary>
        /// Static factory to create boolean literals by value only.
        /// </summary>
        /// <param name="value"></param>
        internal static Literal NewBooleanLiteral(bool value)
        {
            return new Literal(value);
        }

        private Literal(bool boolLiteral)
            : base(null, 0)
        {
            _wasValueComputed = true;
            _originalValue = String.Empty;
            _computedValue = boolLiteral;
            _type = typeof(Boolean);
        }

        /// <summary>
        /// True if literal is a number.
        /// </summary>
        internal bool IsNumber
        {
            get { return (_literalKind == LiteralKind.Number); }
        }

        /// <summary>
        /// True if literal is a signed number.
        /// </summary>
        internal bool IsSignedNumber
        {
            get { return IsNumber && (_originalValue[0] == '-' || _originalValue[0] == '+'); }
        }

        /// <summary>
        /// True if literal is a string.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException"></exception>
        /// </remarks>
        internal bool IsString
        {
            get { return _literalKind == LiteralKind.String || _literalKind == LiteralKind.UnicodeString; }
        }

        /// <summary>
        /// True if literal is a unicode string.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException"></exception>
        /// </remarks>
        internal bool IsUnicodeString
        {
            get { return _literalKind == LiteralKind.UnicodeString; }
        }

        /// <summary>
        /// True if literal is the eSQL untyped null.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException"></exception>
        /// </remarks>
        internal bool IsNullLiteral
        {
            get { return _literalKind == LiteralKind.Null; }
        }

        /// <summary>
        /// Returns the original literal value.
        /// </summary>
        internal string OriginalValue
        {
            get { return _originalValue; }
        }

        /// <summary>
        /// Prefix a numeric literal with a sign.
        /// </summary>
        internal void PrefixSign(string sign)
        {
            Debug.Assert(IsNumber && !IsSignedNumber);
            Debug.Assert(sign[0] == '-' || sign[0] == '+', "sign symbol must be + or -");
            Debug.Assert(_computedValue == null);

            _originalValue = sign + _originalValue;
        }

        #region Computed members

        /// <summary>
        /// Returns literal converted value.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException"></exception>
        /// </remarks>
        internal object Value
        {
            get
            {
                ComputeValue();

                return _computedValue;
            }
        }

        /// <summary>
        /// Returns literal value type. If value is eSQL untyped null, returns null.
        /// </summary>
        /// <remarks>
        /// <exception cref="System.Data.Entity.Core.EntityException"></exception>
        /// </remarks>
        internal Type Type
        {
            get
            {
                ComputeValue();

                return _type;
            }
        }

        #endregion

        private void ComputeValue()
        {
            if (!_wasValueComputed)
            {
                _wasValueComputed = true;

                switch (_literalKind)
                {
                    case LiteralKind.Number:
                        _computedValue = ConvertNumericLiteral(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.String:
                        _computedValue = GetStringLiteralValue(_originalValue, false /* isUnicode */);
                        break;

                    case LiteralKind.UnicodeString:
                        _computedValue = GetStringLiteralValue(_originalValue, true /* isUnicode */);
                        break;

                    case LiteralKind.Boolean:
                        _computedValue = ConvertBooleanLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.Binary:
                        _computedValue = ConvertBinaryLiteralValue(_originalValue);
                        break;

                    case LiteralKind.DateTime:
                        _computedValue = ConvertDateTimeLiteralValue(_originalValue);
                        break;

                    case LiteralKind.Time:
                        _computedValue = ConvertTimeLiteralValue(_originalValue);
                        break;

                    case LiteralKind.DateTimeOffset:
                        _computedValue = ConvertDateTimeOffsetLiteralValue(ErrCtx, _originalValue);
                        break;

                    case LiteralKind.Guid:
                        _computedValue = ConvertGuidLiteralValue(_originalValue);
                        break;

                    case LiteralKind.Null:
                        _computedValue = null;
                        break;

                    default:
                        throw new NotSupportedException(Strings.LiteralTypeNotSupported(_literalKind.ToString()));
                }

                _type = IsNullLiteral ? null : _computedValue.GetType();
            }
        }

        #region Conversion Helpers

        private static readonly char[] numberSuffixes = new[] { 'U', 'u', 'L', 'l', 'F', 'f', 'M', 'm', 'D', 'd' };
        private static readonly char[] floatTokens = new[] { '.', 'E', 'e' };

        private static object ConvertNumericLiteral(ErrorContext errCtx, string numericString)
        {
            var k = numericString.IndexOfAny(numberSuffixes);
            if (-1 != k)
            {
                var suffix = numericString.Substring(k).ToUpperInvariant();
                var numberPart = numericString.Substring(0, numericString.Length - suffix.Length);
                switch (suffix)
                {
                    case "U":
                        {
                            UInt32 value;
                            if (!UInt32.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                            {
                                string message = Strings.CannotConvertNumericLiteral(numericString, "unsigned int");
                                throw EntitySqlException.Create(errCtx, message, null);
                            }
                            return value;
                        }
                        ;

                    case "L":
                        {
                            long value;
                            if (!Int64.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                            {
                                string message = Strings.CannotConvertNumericLiteral(numericString, "long");
                                throw EntitySqlException.Create(errCtx, message, null);
                            }
                            return value;
                        }
                        ;

                    case "UL":
                    case "LU":
                        {
                            UInt64 value;
                            if (!UInt64.TryParse(numberPart, NumberStyles.Integer, CultureInfo.InvariantCulture, out value))
                            {
                                string message = Strings.CannotConvertNumericLiteral(numericString, "unsigned long");
                                throw EntitySqlException.Create(errCtx, message, null);
                            }
                            return value;
                        }
                        ;

                    case "F":
                        {
                            Single value;
                            if (!Single.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                            {
                                string message = Strings.CannotConvertNumericLiteral(numericString, "float");
                                throw EntitySqlException.Create(errCtx, message, null);
                            }
                            return value;
                        }
                        ;

                    case "M":
                        {
                            Decimal value;
                            if (
                                !Decimal.TryParse(
                                    numberPart, NumberStyles.AllowLeadingSign | NumberStyles.AllowDecimalPoint, CultureInfo.InvariantCulture,
                                    out value))
                            {
                                string message = Strings.CannotConvertNumericLiteral(numericString, "decimal");
                                throw EntitySqlException.Create(errCtx, message, null);
                            }
                            return value;
                        }
                        ;

                    case "D":
                        {
                            Double value;
                            if (!Double.TryParse(numberPart, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                            {
                                string message = Strings.CannotConvertNumericLiteral(numericString, "double");
                                throw EntitySqlException.Create(errCtx, message, null);
                            }
                            return value;
                        }
                        ;
                }
            }

            //
            // If hit this point, try default conversion
            //
            return DefaultNumericConversion(numericString, errCtx);
        }

        /// <summary>
        /// Performs conversion of numeric strings that have no type suffix hint.
        /// </summary>
        private static object DefaultNumericConversion(string numericString, ErrorContext errCtx)
        {
            if (-1
                != numericString.IndexOfAny(floatTokens))
            {
                Double value;
                if (!Double.TryParse(numericString, NumberStyles.Float, CultureInfo.InvariantCulture, out value))
                {
                    string message = Strings.CannotConvertNumericLiteral(numericString, "double");
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                return value;
            }
            else
            {
                Int32 int32Value;
                if (Int32.TryParse(numericString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int32Value))
                {
                    return int32Value;
                }

                Int64 int64Value;
                if (!Int64.TryParse(numericString, NumberStyles.Integer, CultureInfo.InvariantCulture, out int64Value))
                {
                    string message = Strings.CannotConvertNumericLiteral(numericString, "long");
                    throw EntitySqlException.Create(errCtx, message, null);
                }

                return int64Value;
            }
        }

        /// <summary>
        /// Converts boolean literal value.
        /// </summary>
        private static bool ConvertBooleanLiteralValue(ErrorContext errCtx, string booleanLiteralValue)
        {
            var result = false;
            if (!Boolean.TryParse(booleanLiteralValue, out result))
            {
                string message = Strings.InvalidLiteralFormat("Boolean", booleanLiteralValue);
                throw EntitySqlException.Create(errCtx, message, null);
            }
            return result;
        }

        /// <summary>
        /// Returns the string literal value.
        /// </summary>
        private static string GetStringLiteralValue(string stringLiteralValue, bool isUnicode)
        {
            Debug.Assert(stringLiteralValue.Length >= 2);
            Debug.Assert(isUnicode == ('N' == stringLiteralValue[0]), "invalid string literal value");

            var startIndex = (isUnicode ? 2 : 1);
            var delimiter = stringLiteralValue[startIndex - 1];

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            if (delimiter != '\''
                && delimiter != '\"')
            {
                string message = Strings.MalformedStringLiteralPayload;
                throw new EntitySqlException(message);
            }

            var result = "";

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            var before = stringLiteralValue.Split(new[] { delimiter }).Length - 1;
            Debug.Assert(before % 2 == 0, "must have an even number of delimiters in the string literal");
            if (0 != (before % 2))
            {
                string message = Strings.MalformedStringLiteralPayload;
                throw new EntitySqlException(message);
            }

            //
            // Extract the payload and replace escaped chars that match the envelope delimiter
            //
            result = stringLiteralValue.Substring(startIndex, stringLiteralValue.Length - (1 + startIndex));
            result = result.Replace(new String(delimiter, 2), new String(delimiter, 1));

            // NOTE: this is not a precondition validation. This validation is for security purposes based on the 
            // paranoid assumption that all input is evil. we should not see this exception under normal 
            // conditions.
            var after = result.Split(new[] { delimiter }).Length - 1;
            Debug.Assert(after == (before - 2) / 2);
            if ((after != ((before - 2) / 2)))
            {
                string message = Strings.MalformedStringLiteralPayload;
                throw new EntitySqlException(message);
            }

            return result;
        }

        /// <summary>
        /// Converts hex string to byte array.
        /// </summary>
        private static byte[] ConvertBinaryLiteralValue(string binaryLiteralValue)
        {
            Debug.Assert(null != binaryLiteralValue, "binaryStringLiteral must not be null");

            if (String.IsNullOrEmpty(binaryLiteralValue))
            {
                return _emptyByteArray;
            }

            var startIndex = 0;
            var endIndex = binaryLiteralValue.Length - 1;
            Debug.Assert(startIndex <= endIndex, "startIndex <= endIndex");
            var binaryStringLen = endIndex - startIndex + 1;
            var byteArrayLen = binaryStringLen / 2;
            var hasOddBytes = 0 != (binaryStringLen % 2);
            if (hasOddBytes)
            {
                byteArrayLen++;
            }

            var binaryValue = new byte[byteArrayLen];
            var arrayIndex = 0;
            if (hasOddBytes)
            {
                binaryValue[arrayIndex++] = (byte)HexDigitToBinaryValue(binaryLiteralValue[startIndex++]);
            }

            while (startIndex < endIndex)
            {
                binaryValue[arrayIndex++] =
                    (byte)
                    ((HexDigitToBinaryValue(binaryLiteralValue[startIndex++]) << 4)
                     | HexDigitToBinaryValue(binaryLiteralValue[startIndex++]));
            }

            return binaryValue;
        }

        /// <summary>
        /// Parse single hex char.
        /// PRECONDITION - hexChar must be a valid hex digit.
        /// </summary>
        private static int HexDigitToBinaryValue(char hexChar)
        {
            if (hexChar >= '0'
                && hexChar <= '9')
            {
                return (hexChar - '0');
            }
            if (hexChar >= 'A'
                && hexChar <= 'F')
            {
                return (hexChar - 'A') + 10;
            }
            if (hexChar >= 'a'
                && hexChar <= 'f')
            {
                return (hexChar - 'a') + 10;
            }
            throw new ArgumentOutOfRangeException("hexChar");
        }

        private static readonly char[] _datetimeSeparators = new[] { ' ', ':', '-', '.' };
        private static readonly char[] _datetimeOffsetSeparators = new[] { ' ', ':', '-', '.', '+', '-' };

        /// <summary>
        /// Converts datetime literal value.
        /// </summary>
        private static DateTime ConvertDateTimeLiteralValue(string datetimeLiteralValue)
        {
            var datetimeParts = datetimeLiteralValue.Split(_datetimeSeparators, StringSplitOptions.RemoveEmptyEntries);

            Debug.Assert(datetimeParts.Length >= 5, "datetime literal value must have at least 5 parts");

            int year;
            int month;
            int day;
            GetDateParts(datetimeLiteralValue, datetimeParts, out year, out month, out day);
            int hour;
            int minute;
            int second;
            int ticks;
            GetTimeParts(datetimeLiteralValue, datetimeParts, 3, out hour, out minute, out second, out ticks);

            Debug.Assert(year >= 1 && year <= 9999);
            Debug.Assert(month >= 1 && month <= 12);
            Debug.Assert(day >= 1 && day <= 31);
            Debug.Assert(hour >= 0 && hour <= 24);
            Debug.Assert(minute >= 0 && minute <= 59);
            Debug.Assert(second >= 0 && second <= 59);
            Debug.Assert(ticks >= 0 && ticks <= 9999999);
            var dateTime = new DateTime(year, month, day, hour, minute, second, 0);
            dateTime = dateTime.AddTicks(ticks);
            return dateTime;
        }

        private static DateTimeOffset ConvertDateTimeOffsetLiteralValue(ErrorContext errCtx, string datetimeLiteralValue)
        {
            var datetimeParts = datetimeLiteralValue.Split(_datetimeOffsetSeparators, StringSplitOptions.RemoveEmptyEntries);

            Debug.Assert(datetimeParts.Length >= 7, "datetime literal value must have at least 7 parts");

            int year;
            int month;
            int day;
            GetDateParts(datetimeLiteralValue, datetimeParts, out year, out month, out day);
            int hour;
            int minute;
            int second;
            int ticks;
            //Copy the time parts into a different array since the last two parts will be handled in this method.
            var timeParts = new String[datetimeParts.Length - 2];
            Array.Copy(datetimeParts, timeParts, datetimeParts.Length - 2);
            GetTimeParts(datetimeLiteralValue, timeParts, 3, out hour, out minute, out second, out ticks);

            Debug.Assert(year >= 1 && year <= 9999);
            Debug.Assert(month >= 1 && month <= 12);
            Debug.Assert(day >= 1 && day <= 31);
            Debug.Assert(hour >= 0 && hour <= 24);
            Debug.Assert(minute >= 0 && minute <= 59);
            Debug.Assert(second >= 0 && second <= 59);
            Debug.Assert(ticks >= 0 && ticks <= 9999999);
            var offsetHours = Int32.Parse(datetimeParts[datetimeParts.Length - 2], NumberStyles.Integer, CultureInfo.InvariantCulture);
            var offsetMinutes = Int32.Parse(datetimeParts[datetimeParts.Length - 1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            var offsetTimeSpan = new TimeSpan(offsetHours, offsetMinutes, 0);

            //If DateTimeOffset had a negative offset, we should negate the timespan
            if (datetimeLiteralValue.IndexOf('+')
                == -1)
            {
                offsetTimeSpan = offsetTimeSpan.Negate();
            }
            var dateTime = new DateTime(year, month, day, hour, minute, second, 0);
            dateTime = dateTime.AddTicks(ticks);

            try
            {
                return new DateTimeOffset(dateTime, offsetTimeSpan);
            }
            catch (ArgumentOutOfRangeException e)
            {
                string message = Strings.InvalidDateTimeOffsetLiteral(datetimeLiteralValue);
                throw EntitySqlException.Create(errCtx, message, e);
            }
        }

        /// <summary>
        /// Converts time literal value.
        /// </summary>
        private static TimeSpan ConvertTimeLiteralValue(string datetimeLiteralValue)
        {
            var datetimeParts = datetimeLiteralValue.Split(_datetimeSeparators, StringSplitOptions.RemoveEmptyEntries);

            Debug.Assert(datetimeParts.Length >= 2, "time literal value must have at least 2 parts");

            int hour;
            int minute;
            int second;
            int ticks;
            GetTimeParts(datetimeLiteralValue, datetimeParts, 0, out hour, out minute, out second, out ticks);

            Debug.Assert(hour >= 0 && hour <= 24);
            Debug.Assert(minute >= 0 && minute <= 59);
            Debug.Assert(second >= 0 && second <= 59);
            Debug.Assert(ticks >= 0 && ticks <= 9999999);
            var ts = new TimeSpan(hour, minute, second);
            ts = ts.Add(new TimeSpan(ticks));
            return ts;
        }

        private static void GetTimeParts(
            string datetimeLiteralValue, string[] datetimeParts, int timePartStartIndex, out int hour, out int minute, out int second,
            out int ticks)
        {
            hour = Int32.Parse(datetimeParts[timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (hour > 23)
            {
                string message = Strings.InvalidHour(datetimeParts[timePartStartIndex], datetimeLiteralValue);
                throw new EntitySqlException(message);
            }
            minute = Int32.Parse(datetimeParts[++timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (minute > 59)
            {
                string message = Strings.InvalidMinute(datetimeParts[timePartStartIndex], datetimeLiteralValue);
                throw new EntitySqlException(message);
            }
            second = 0;
            ticks = 0;
            timePartStartIndex++;
            if (datetimeParts.Length > timePartStartIndex)
            {
                second = Int32.Parse(datetimeParts[timePartStartIndex], NumberStyles.Integer, CultureInfo.InvariantCulture);
                if (second > 59)
                {
                    string message = Strings.InvalidSecond(datetimeParts[timePartStartIndex], datetimeLiteralValue);
                    throw new EntitySqlException(message);
                }
                timePartStartIndex++;
                if (datetimeParts.Length > timePartStartIndex)
                {
                    //We need fractional time part to be seven digits
                    var ticksString = datetimeParts[timePartStartIndex].PadRight(7, '0');
                    ticks = Int32.Parse(ticksString, NumberStyles.Integer, CultureInfo.InvariantCulture);
                }
            }
        }

        private static void GetDateParts(string datetimeLiteralValue, string[] datetimeParts, out int year, out int month, out int day)
        {
            year = Int32.Parse(datetimeParts[0], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (year < 1
                || year > 9999)
            {
                string message = Strings.InvalidYear(datetimeParts[0], datetimeLiteralValue);
                throw new EntitySqlException(message);
            }
            month = Int32.Parse(datetimeParts[1], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (month < 1
                || month > 12)
            {
                string message = Strings.InvalidMonth(datetimeParts[1], datetimeLiteralValue);
                throw new EntitySqlException(message);
            }
            day = Int32.Parse(datetimeParts[2], NumberStyles.Integer, CultureInfo.InvariantCulture);
            if (day < 1)
            {
                string message = Strings.InvalidDay(datetimeParts[2], datetimeLiteralValue);
                throw new EntitySqlException(message);
            }
            if (day > DateTime.DaysInMonth(year, month))
            {
                string message = Strings.InvalidDayInMonth(datetimeParts[2], datetimeParts[1], datetimeLiteralValue);
                throw new EntitySqlException(message);
            }
        }

        /// <summary>
        /// Converts guid literal value.
        /// </summary>
        private static Guid ConvertGuidLiteralValue(string guidLiteralValue)
        {
            return new Guid(guidLiteralValue);
        }

        #endregion
    }
}
